using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Animas.Player
{
    // Lives on the camera: finds the IInteractable the player is looking at and uses it when F is pressed.
    public class Interactor : MonoBehaviour
    {
        [SerializeField] PlayerTuning tuning;
        [SerializeField] LayerMask interactableLayers = ~0;
        [Tooltip("How far off-center an object can be and still count as looked at. Helps with small items.")]
        [SerializeField, Range(0f, 0.5f)] float aimAssistRadius = 0.2f;

        public IInteractable Current { get; private set; }

        // Fires only when the focused object changes (null = nothing in front), so the prompt UI updates on events, not polling.
        public event Action<IInteractable> FocusChanged;

        InputAction interactAction;
        readonly RaycastHit[] hits = new RaycastHit[16];  // reused every frame, so no garbage

        void Awake()
        {
            interactAction = InputSystem.actions.FindAction("Player/Interact", true);
        }

        void Update()
        {
            IInteractable found = FindTarget();

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

        IInteractable FindTarget()
        {
            // 1) Exactly where the player looks: the nearest thing the ray touches, skipping the Padre's own
            // body (looking down at something close, the ray starts inside his collider and hits it first).
            int count = Physics.RaycastNonAlloc(transform.position, transform.forward, hits,
                tuning.interactDistance, interactableLayers, QueryTriggerInteraction.Ignore);
            RaycastHit? nearest = null;
            for (int i = 0; i < count; i++)
            {
                if (IsSelf(hits[i].collider)) continue;
                if (nearest == null || hits[i].distance < nearest.Value.distance) nearest = hits[i];
            }
            if (nearest != null)
            {
                // InParent: the collider can be on a child mesh (a door panel) while the script is on the root.
                var direct = nearest.Value.collider.GetComponentInParent<IInteractable>();
                if (direct != null) return direct;
            }

            // 2) A thicker ray for small things: looking down at a bottle on a table, the thin ray often
            // hits the table first. Take the nearest usable object the thick ray touches.
            count = Physics.SphereCastNonAlloc(transform.position, aimAssistRadius, transform.forward, hits,
                tuning.interactDistance, interactableLayers, QueryTriggerInteraction.Ignore);
            IInteractable best = null;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                var candidate = hits[i].collider.GetComponentInParent<IInteractable>();
                if (candidate != null && hits[i].distance < bestDistance)
                {
                    best = candidate;
                    bestDistance = hits[i].distance;
                }
            }
            return best;
        }

        bool IsSelf(Collider c) => c.transform.IsChildOf(transform.root);
    }
}
