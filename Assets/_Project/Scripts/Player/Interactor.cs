using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Animas.Player
{
    // Lives on the camera: casts a short ray where the player looks and uses the IInteractable it hits when F is pressed.
    public class Interactor : MonoBehaviour
    {
        [SerializeField] PlayerTuning tuning;
        [SerializeField] LayerMask interactableLayers = ~0;

        public IInteractable Current { get; private set; }

        // Fires only when the focused object changes (null = nothing in front), so the prompt UI updates on events, not polling.
        public event Action<IInteractable> FocusChanged;

        InputAction interactAction;

        void Awake()
        {
            interactAction = InputSystem.actions.FindAction("Player/Interact", true);
        }

        void Update()
        {
            IInteractable found = null;
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit,
                    tuning.interactDistance, interactableLayers, QueryTriggerInteraction.Ignore))
            {
                // InParent: the collider can be on a child mesh (a door panel) while the script is on the root.
                found = hit.collider.GetComponentInParent<IInteractable>();
            }

            if (found != Current)
            {
                Current = found;
                FocusChanged?.Invoke(Current);
            }

            if (Current != null && interactAction.WasPressedThisFrame())
            {
                Current.Interact(this);
                // The prompt may change after use ("Abrir" → "Cerrar"), so notify again.
                FocusChanged?.Invoke(Current);
            }
        }
    }
}
