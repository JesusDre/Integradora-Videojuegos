using System;
using System.Collections.Generic;
using Animas.Core;
using UnityEngine;

namespace Animas.Items
{
    // One inventory space: an item and how many of it (up to the item's maxStack).
    [Serializable]
    public class InventorySlot
    {
        public ItemDefinition item;
        public int count;
        public bool IsEmpty => item == null || count <= 0;

        public void Clear()
        {
            item = null;
            count = 0;
        }
    }

    // The Padre's inventory (GDD §18–19): 8 spaces for objects, and materials apart, up to 99 per type,
    // without taking space. The first 4 spaces are the quick slots used with keys 1–4.
    // It only holds data and rules; the HUD and the inventory screen (#22) listen to its events.
    public class Inventory : MonoBehaviour
    {
        public const int SlotCount = 8;
        public const int QuickSlotCount = 4;
        public const int MaxPerMaterial = 99;

        [SerializeField] Health owner;
        [Tooltip("Who uses the items (aim, weapons). On the Padre.")]
        [SerializeField] ItemUser user;
        [Tooltip("Used when a dropped item has no world prefab of its own.")]
        [SerializeField] GameObject fallbackPickupPrefab;

        readonly InventorySlot[] slots = new InventorySlot[SlotCount];
        readonly Dictionary<ItemDefinition, int> materials = new Dictionary<ItemDefinition, int>();

        public IReadOnlyList<InventorySlot> Slots => slots;
        // Read-only so only TryAdd/Remove change materials (they keep the 0–99 limit).
        public IReadOnlyDictionary<ItemDefinition, int> Materials => materials;

        // Something in the inventory changed (the HUD redraws).
        public event Action Changed;
        // A short message for the player: "Inventario lleno", "Recogiste: venda"...
        public event Action<string> Notice;

        void Awake()
        {
            for (int i = 0; i < SlotCount; i++) slots[i] = new InventorySlot();
            if (owner == null) owner = GetComponent<Health>();
            if (user == null) user = GetComponent<ItemUser>();
        }

        public int MaterialCount(ItemDefinition material) =>
            materials.TryGetValue(material, out int n) ? n : 0;

        public int CountOf(ItemDefinition item)
        {
            if (item.IsMaterial) return MaterialCount(item);
            int total = 0;
            foreach (var s in slots) if (s.item == item) total += s.count;
            return total;
        }

        // Tries to add `amount` of an item and returns how many actually fit. What doesn't fit stays
        // where it was (on the floor), and the player gets a notice. notify = false for the altar,
        // which shows its own message ("Fabricaste…") instead of "Recogiste…".
        public int TryAdd(ItemDefinition item, int amount = 1, bool notify = true)
        {
            if (item == null || amount <= 0) return 0;

            int added = item.IsMaterial ? AddMaterial(item, amount) : AddToSlots(item, amount);
            if (added > 0)
            {
                Changed?.Invoke();
                if (notify) Notice?.Invoke(added > 1 ? $"Recogiste: {item.displayName} ×{added}" : $"Recogiste: {item.displayName}");
            }
            if (added < amount && notify)
                Notice?.Invoke(item.IsMaterial ? $"No puedes cargar más {item.displayName} (máximo {MaxPerMaterial})" : "Inventario lleno");
            return added;
        }

        // Whether `amount` of an item would fit right now, without adding anything.
        public bool CanFit(ItemDefinition item, int amount)
        {
            if (item == null || amount <= 0) return false;
            if (item.IsMaterial) return MaterialCount(item) + amount <= MaxPerMaterial;
            int room = 0;
            foreach (var s in slots)
            {
                if (s.IsEmpty) room += item.maxStack;
                else if (s.item == item) room += item.maxStack - s.count;
            }
            return room >= amount;
        }

        // Replaces everything (loading a save). counts[i] goes with items[i]; materials the same way.
        public void SetContents(ItemDefinition[] slotItems, int[] slotCounts, ItemDefinition[] materialItems, int[] materialCounts)
        {
            for (int i = 0; i < SlotCount; i++)
            {
                slots[i].Clear();
                if (i < slotItems.Length && slotItems[i] != null && slotCounts[i] > 0)
                {
                    slots[i].item = slotItems[i];
                    slots[i].count = Mathf.Clamp(slotCounts[i], 1, slotItems[i].maxStack);
                }
            }
            materials.Clear();
            for (int i = 0; i < materialItems.Length; i++)
                if (materialItems[i] != null && materialCounts[i] > 0)
                    materials[materialItems[i]] = Mathf.Min(materialCounts[i], MaxPerMaterial);
            Changed?.Invoke();
        }

        // Shows a message through the same channel as the inventory's own notices (the altar uses it).
        public void Notify(string message) => Notice?.Invoke(message);

        int AddMaterial(ItemDefinition material, int amount)
        {
            int have = MaterialCount(material);
            int fits = Mathf.Min(amount, MaxPerMaterial - have);
            if (fits > 0) materials[material] = have + fits;
            return Mathf.Max(fits, 0);
        }

        int AddToSlots(ItemDefinition item, int amount)
        {
            int left = amount;
            // First top up stacks of the same item, then use empty spaces.
            foreach (var s in slots)
            {
                if (left == 0) break;
                if (s.item != item || s.count >= item.maxStack) continue;
                int put = Mathf.Min(left, item.maxStack - s.count);
                s.count += put;
                left -= put;
            }
            foreach (var s in slots)
            {
                if (left == 0) break;
                if (!s.IsEmpty) continue;
                int put = Mathf.Min(left, item.maxStack);
                s.item = item;
                s.count = put;
                left -= put;
            }
            return amount - left;
        }

        // Takes items out (crafting, ammo). All or nothing: returns false and changes nothing if there aren't enough.
        public bool Remove(ItemDefinition item, int amount = 1)
        {
            if (item == null || amount <= 0 || CountOf(item) < amount) return false;

            if (item.IsMaterial)
            {
                materials[item] -= amount;
                if (materials[item] == 0) materials.Remove(item);
            }
            else
            {
                int left = amount;
                // Take from the last spaces first, so the quick slots stay filled as long as possible.
                for (int i = SlotCount - 1; i >= 0 && left > 0; i--)
                {
                    var s = slots[i];
                    if (s.item != item) continue;
                    int take = Mathf.Min(left, s.count);
                    s.count -= take;
                    left -= take;
                    if (s.count == 0) s.Clear();
                }
            }
            Changed?.Invoke();
            return true;
        }

        // Uses one item from a space (keys 1–4 for the quick slots). For now only healing items do
        // something; faith objects and throwables get their effects in #31.
        public bool UseSlot(int index)
        {
            if (index < 0 || index >= SlotCount) return false;
            var s = slots[index];
            if (s.IsEmpty) return false;

            if (s.item.healAmount > 0f && owner != null)
            {
                if (owner.Heal(s.item.healAmount) <= 0f)
                {
                    Notice?.Invoke("Ya tienes la vida completa");
                    return false;
                }
                s.count--;
                if (s.count == 0) s.Clear();
                Changed?.Invoke();
                return true;
            }

            // Faith objects and weapons (GDD §18): thrown, placed or equipped by their effect asset.
            if (s.item.useEffect != null && user != null)
            {
                if (!s.item.useEffect.Use(user, s.item)) return false;
                if (s.item.useEffect.ConsumesItem)
                {
                    s.count--;
                    if (s.count == 0) s.Clear();
                }
                Changed?.Invoke();
                return true;
            }

            Notice?.Invoke($"{s.item.displayName}: todavía no se puede usar");
            return false;
        }

        // Drops a whole space on the floor in front of the Padre, as a pickup he can take again.
        public bool Drop(int index, Transform dropPoint)
        {
            if (index < 0 || index >= SlotCount) return false;
            var s = slots[index];
            if (s.IsEmpty) return false;

            GameObject prefab = s.item.worldPrefab != null ? s.item.worldPrefab : fallbackPickupPrefab;
            if (prefab != null)
            {
                Vector3 pos = dropPoint.position + dropPoint.forward * 0.8f;
                if (Physics.Raycast(pos + Vector3.up, Vector3.down, out RaycastHit ground, 3f)) pos = ground.point;
                var dropped = Instantiate(prefab, pos, Quaternion.identity);
                if (dropped.TryGetComponent(out PickupItem pickup)) pickup.Set(s.item, s.count);
            }
            Notice?.Invoke($"Tiraste: {s.item.displayName}");
            s.Clear();
            Changed?.Invoke();
            return true;
        }
    }
}
