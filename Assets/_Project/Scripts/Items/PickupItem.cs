using System;
using Animas.Player;
using UnityEngine;

namespace Animas.Items
{
    // An object on the ground that disappears when picked up with F.
    // Issue #14 connects it to the inventory; for now it only announces the pickup.
    public class PickupItem : MonoBehaviour, IInteractable
    {
        [SerializeField] string displayName = "objeto";
        [SerializeField] AudioClip pickupClip;

        public string DisplayName => displayName;
        public string Prompt => $"Recoger {displayName}";

        // Static so systems (inventory, tutorial hints) can listen to every pickup without a reference to each one.
        public static event Action<PickupItem> PickedUp;

        public void Interact(Interactor interactor)
        {
            // PlayClipAtPoint because this object is about to be destroyed and its own AudioSource would cut off.
            if (pickupClip != null) AudioSource.PlayClipAtPoint(pickupClip, transform.position);
            PickedUp?.Invoke(this);
            Debug.Log($"[PickupItem] Recogido: {displayName}");
            Destroy(gameObject);
        }
    }
}
