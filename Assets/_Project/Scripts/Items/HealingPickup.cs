using Animas.Core;
using Animas.Player;
using UnityEngine;

namespace Animas.Items
{
    // A bandage on the ground: F heals the Padre 30 points (GDD §5) and it disappears.
    // At full health it isn't wasted. When the inventory exists (#14), bandages go to the quick slots instead.
    public class HealingPickup : MonoBehaviour, IInteractable
    {
        [SerializeField, Min(1f)] float healAmount = 30f;
        [SerializeField] AudioClip useClip;

        public string Prompt => $"Usar venda (+{healAmount:0})";

        public void Interact(Interactor interactor)
        {
            Health health = interactor.GetComponentInParent<Health>();
            if (health == null) return;

            if (health.Heal(healAmount) <= 0f)
            {
                Debug.Log("[HealingPickup] Vida completa: la venda no se gasta.");
                return;
            }
            if (useClip != null) AudioSource.PlayClipAtPoint(useClip, transform.position);
            Destroy(gameObject);
        }
    }
}
