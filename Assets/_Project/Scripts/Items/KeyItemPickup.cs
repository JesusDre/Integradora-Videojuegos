using Animas.Player;
using UnityEngine;

namespace Animas.Items
{
    // A key item in the world (a muñeca ofrenda). F takes it into the KeyItemBag.
    // It isn't destroyed: it hides while it counts as taken, and shows up again if the Padre dies before
    // the next save and the bag goes back to an older state (GDD §6: the section 2 key item is repeated).
    // Each pickup has its own id, so only the ones really lost come back and the level can always be finished.
    public class KeyItemPickup : MonoBehaviour, IInteractable
    {
        [Tooltip("Unique in the game, stored in the save (e.g. \"I1_Muneca3\").")]
        [SerializeField] string id = "KeyItem";
        [SerializeField] ItemDefinition item;
        [Tooltip("Total of this item in the instance, for the \"(2/3)\" message.")]
        [SerializeField, Min(1)] int totalInLevel = 3;
        [SerializeField] AudioClip pickupClip;

        public string Prompt => $"Tomar {item.displayName}";

        KeyItemBag bag;

        void Start()
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) bag = player.GetComponent<KeyItemBag>();
            if (bag == null) return;
            bag.Changed += Refresh;
            Refresh();
        }

        void OnDestroy()
        {
            if (bag != null) bag.Changed -= Refresh;
        }

        public void Interact(Interactor interactor)
        {
            if (bag == null || bag.HasCollected(id)) return;
            bag.Add(item, id);
            if (pickupClip != null) AudioSource.PlayClipAtPoint(pickupClip, transform.position);
            var inventory = bag.GetComponent<Inventory>();
            if (inventory != null) inventory.Notify($"{Capitalize(item.displayName)} ({bag.Count(item)}/{totalInLevel})");
        }

        void Refresh()
        {
            bool visible = !bag.HasCollected(id);
            foreach (Renderer r in GetComponentsInChildren<Renderer>()) r.enabled = visible;
            foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = visible;
        }

        static string Capitalize(string s) => string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s.Substring(1);
    }
}
