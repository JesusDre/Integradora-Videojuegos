using System;
using Animas.Player;
using UnityEngine;

namespace Animas.Items
{
    // An item lying in the world. Items and materials go into the inventory as soon as the Padre walks over
    // them (#138); weapons wait for F, so a new weapon is never picked up by accident. If only part fits
    // (inventory full or 99 materials), the rest stays on the floor.
    public class PickupItem : MonoBehaviour, IInteractable
    {
        [SerializeField] ItemDefinition item;
        [SerializeField, Min(1)] int amount = 1;
        [SerializeField] AudioClip pickupClip;

        public ItemDefinition Item => item;
        public int Amount => amount;
        public string Prompt => item == null ? "Recoger" : amount > 1 ? $"Recoger {item.displayName} ×{amount}" : $"Recoger {item.displayName}";

        // Static so systems (tutorial hints, quests) can listen to every pickup without a reference to each one.
        public static event Action<PickupItem> PickedUp;

        [Tooltip("Pickups show their model a bit bigger than real size, so small things (a bandage) are seen.")]
        [SerializeField, Min(0.1f)] float modelScale = 1.4f;

        [Tooltip("Distance on the ground at which an item is collected by walking over it.")]
        [SerializeField, Min(0.2f)] float collectRadius = 1.3f;

        GameObject shownModel;
        Transform player;
        Inventory playerInventory;
        float nextTry;
        bool waitForPlayerToLeave;

        // Weapons are a choice (they replace what is in the hands): those keep needing F.
        public bool AutoCollect => item != null && item.category != ItemCategory.MeleeWeapon && item.category != ItemCategory.RangedWeapon;

        void Start()
        {
            ShowModel();
            GameObject p = GameObject.FindWithTag("Player");
            if (p == null) return;
            player = p.transform;
            playerInventory = p.GetComponent<Inventory>();
        }

        void Update()
        {
            if (!AutoCollect || player == null || Time.time < nextTry) return;
            Vector3 d = player.position - transform.position;
            bool near = Mathf.Abs(d.y) < 1.5f && new Vector2(d.x, d.z).sqrMagnitude < collectRadius * collectRadius;
            // Something the Padre just dropped lies at his feet: wait until he walks away before it can come back.
            if (waitForPlayerToLeave)
            {
                if (!near) waitForPlayerToLeave = false;
                return;
            }
            if (!near) return;
            // Full inventory: try again in a while instead of repeating "Inventario lleno" every frame.
            if (!Collect(playerInventory)) nextTry = Time.time + 3f;
        }

        // Used when the player drops something: the new pickup carries the dropped item.
        public void Set(ItemDefinition newItem, int newAmount)
        {
            item = newItem;
            amount = Mathf.Max(1, newAmount);
            waitForPlayerToLeave = true;
            ShowModel();
        }

        // One generic pickup prefab serves every item: it hides its box and shows the item's own model.
        void ShowModel()
        {
            if (item == null || item.model == null) return;
            if (shownModel != null) Destroy(shownModel);
            var box = GetComponent<MeshRenderer>();
            if (box != null) box.enabled = false;
            shownModel = Instantiate(item.model, transform);
            // The pickup itself is scaled (it was a small cube); undo that so the model keeps its real size.
            Vector3 s = transform.lossyScale;
            shownModel.transform.localScale = new Vector3(modelScale / s.x, modelScale / s.y, modelScale / s.z);
            shownModel.transform.localPosition = Vector3.down * 0.5f;  // the cube's bottom: the model sits on the floor
        }

        public void Interact(Interactor interactor) => Collect(interactor.GetComponentInParent<Inventory>());

        // F and walking over it end here. Returns false if nothing fit.
        bool Collect(Inventory inventory)
        {
            if (inventory == null || item == null) return false;

            // A weapon goes to the WeaponBag and into the hands, not into an inventory space (#134).
            int taken = item.IsWeapon ? TakeWeapon(inventory) : inventory.TryAdd(item, amount);
            if (taken == 0) return false;  // the inventory already showed why

            amount -= taken;
            // PlayClipAtPoint because this object may be destroyed right away and its own AudioSource would cut off.
            if (pickupClip != null) AudioSource.PlayClipAtPoint(pickupClip, transform.position);
            PickedUp?.Invoke(this);
            if (amount == 0) Destroy(gameObject);
            return true;
        }

        int TakeWeapon(Inventory inventory)
        {
            var bag = inventory.GetComponent<WeaponBag>();
            if (bag == null) return 0;
            if (bag.Has(item))
            {
                inventory.Notify($"Ya tienes {item.displayName}");
                return 0;
            }
            bag.Add(item);
            inventory.Notify($"Recogiste: {item.displayName} (Q para cambiar de arma)");
            return amount;
        }
    }
}
