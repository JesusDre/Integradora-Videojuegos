using Animas.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Animas.UI
{
    // One inventory space on screen: the item's icon (or its name while there are no icons yet),
    // how many there are and, for quick slots, the key that uses it.
    public class ItemSlotView : MonoBehaviour
    {
        [SerializeField] Image background;
        [SerializeField] Image icon;
        [SerializeField] TMP_Text nameLabel;
        [SerializeField] TMP_Text countLabel;
        [SerializeField] TMP_Text keyLabel;
        [SerializeField] Color normalColor = new Color(0.05f, 0.06f, 0.07f, 0.7f);
        [SerializeField] Color selectedColor = new Color(0.91f, 0.88f, 0.83f, 0.3f);

        public void Show(InventorySlot slot)
        {
            bool empty = slot == null || slot.IsEmpty;
            bool hasIcon = !empty && slot.item.icon != null;
            icon.enabled = hasIcon;
            if (hasIcon) icon.sprite = slot.item.icon;
            nameLabel.text = empty || hasIcon ? "" : slot.item.displayName;
            countLabel.text = !empty && slot.count > 1 ? $"×{slot.count}" : "";
        }

        public void SetKey(string key)
        {
            if (keyLabel != null) keyLabel.text = key;
        }

        public void SetSelected(bool selected) => background.color = selected ? selectedColor : normalColor;
    }
}
