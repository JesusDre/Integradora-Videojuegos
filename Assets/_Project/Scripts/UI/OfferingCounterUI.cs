using Animas.Items;
using Animas.Levels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Animas.UI
{
    // Top left of the HUD (#160): the icon of this instance's offering (muñecas, monedas or fragmentos)
    // and how many the Padre has found, "1/3". Found = carried in the bag + already placed on the anchor,
    // so placing them doesn't make the count go down. It turns gold when all are found. It reads the
    // anchor of the scene, so it works in every instance without wiring; without an anchor it hides.
    public class OfferingCounterUI : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] Image icon;
        [SerializeField] TMP_Text countLabel;
        [SerializeField] Color incompleteColor = new Color(0.91f, 0.88f, 0.83f);
        [SerializeField] Color completeColor = new Color(0.96f, 0.78f, 0.35f);

        Anchor anchor;
        KeyItemBag bag;

        void Start()
        {
            anchor = FindAnyObjectByType<Anchor>();
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) bag = player.GetComponent<KeyItemBag>();
            if (anchor == null || bag == null || anchor.Offering == null)
            {
                root.SetActive(false);
                return;
            }
            icon.sprite = anchor.Offering.icon;
            bag.Changed += Refresh;
            anchor.OfferingPlaced += OnPlaced;
            Refresh();
        }

        void OnDestroy()
        {
            if (bag != null) bag.Changed -= Refresh;
            if (anchor != null) anchor.OfferingPlaced -= OnPlaced;
        }

        void OnPlaced(int placed) => Refresh();

        void Refresh()
        {
            int found = Mathf.Min(bag.Count(anchor.Offering) + anchor.Placed, anchor.Required);
            bool complete = found >= anchor.Required;
            countLabel.text = $"{found}/{anchor.Required}";
            countLabel.color = complete ? completeColor : incompleteColor;
            // The icon is dim until the first one is found.
            icon.color = found > 0 ? Color.white : new Color(1f, 1f, 1f, 0.45f);
        }
    }
}
