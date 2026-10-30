using System;
using Animas.Player;
using UnityEngine;

namespace Animas.Items
{
    // An item lying in the world: F puts it in the inventory. If only part fits (inventory full or 99
    // materials), the rest stays on the floor.
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

        // Used when the player drops something: the new pickup carries the dropped item.
        public void Set(ItemDefinition newItem, int newAmount)
        {
            item = newItem;
            amount = Mathf.Max(1, newAmount);
        }

        public void Interact(Interactor interactor)
        {
            var inventory = interactor.GetComponentInParent<Inventory>();
            if (inventory == null || item == null) return;

            int taken = inventory.TryAdd(item, amount);
            if (taken == 0) return;  // the inventory already showed "Inventario lleno"

            amount -= taken;
            // PlayClipAtPoint because this object may be destroyed right away and its own AudioSource would cut off.
            if (pickupClip != null) AudioSource.PlayClipAtPoint(pickupClip, transform.position);
            PickedUp?.Invoke(this);
            if (amount == 0) Destroy(gameObject);
        }
    }
}
