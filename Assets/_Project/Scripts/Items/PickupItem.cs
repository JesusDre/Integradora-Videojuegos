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

        [Tooltip("Pickups show their model a bit bigger than real size, so small things (a bandage) are seen.")]
        [SerializeField, Min(0.1f)] float modelScale = 1.4f;

        GameObject shownModel;

        void Start() => ShowModel();

        // Used when the player drops something: the new pickup carries the dropped item.
        public void Set(ItemDefinition newItem, int newAmount)
        {
            item = newItem;
            amount = Mathf.Max(1, newAmount);
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
