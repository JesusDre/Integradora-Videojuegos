using Animas.Items;
using Animas.Player;
using Animas.Save;
using UnityEngine;

namespace Animas.Levels
{
    // The chapel's holy water font (GDD §18): F gives 1 holy water, as many times as needed (#156). It
    // used to run dry after 3, and a missed canaleta could leave the Charro fight impossible to finish.
    // The limit is the inventory itself: when the spaces are full, TryAdd says so and nothing happens.
    public class HolyWaterFont : MonoBehaviour, IInteractable
    {
        [SerializeField] ItemDefinition holyWater;
        [SerializeField] AudioClip fillClip;

        public string Prompt => "Tomar agua bendita";

        public void Interact(Interactor interactor)
        {
            var inventory = interactor.GetComponentInParent<Inventory>();
            if (inventory == null || inventory.TryAdd(holyWater, 1) == 0) return;
            if (fillClip != null) AudioSource.PlayClipAtPoint(fillClip, transform.position);
        }
    }
}
