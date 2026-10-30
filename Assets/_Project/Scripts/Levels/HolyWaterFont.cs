using Animas.Items;
using Animas.Player;
using Animas.Save;
using UnityEngine;

namespace Animas.Levels
{
    // The chapel's holy water font (GDD §18): F gives 1 holy water, up to 3 times. It fills up again when
    // the Padre respawns, so dying in the Charro fight never leaves him without water for the canaletas.
    public class HolyWaterFont : MonoBehaviour, IInteractable
    {
        [SerializeField] ItemDefinition holyWater;
        [SerializeField, Min(1)] int uses = 3;
        [Tooltip("The water surface: hidden when the font is empty.")]
        [SerializeField] GameObject water;
        [SerializeField] AudioClip fillClip;

        public int Left { get; private set; }
        public string Prompt => Left > 0 ? $"Tomar agua bendita ({Left})" : "La pila está vacía";

        PlayerRespawn respawn;

        void Start()
        {
            Refill();
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) respawn = player.GetComponent<PlayerRespawn>();
            if (respawn != null) respawn.Respawned += Refill;
        }

        void OnDestroy()
        {
            if (respawn != null) respawn.Respawned -= Refill;
        }

        public void Interact(Interactor interactor)
        {
            var inventory = interactor.GetComponentInParent<Inventory>();
            if (Left <= 0 || inventory == null) return;
            // TryAdd already says "no space" when the 8 spaces are full; then nothing is used up.
            if (inventory.TryAdd(holyWater, 1) == 0) return;
            Left--;
            if (fillClip != null) AudioSource.PlayClipAtPoint(fillClip, transform.position);
            if (water != null) water.SetActive(Left > 0);
        }

        void Refill()
        {
            Left = uses;
            if (water != null) water.SetActive(true);
        }
    }
}
