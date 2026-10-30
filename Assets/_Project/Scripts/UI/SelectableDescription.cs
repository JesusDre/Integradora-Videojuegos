using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Animas.UI
{
    // Shows a text in a shared label while this button is hovered (mouse) or selected (keyboard).
    // Used by the instance selection to describe each place.
    public class SelectableDescription : MonoBehaviour, IPointerEnterHandler, ISelectHandler
    {
        TMP_Text target;
        string text;

        public void Setup(TMP_Text label, string description)
        {
            target = label;
            text = description;
        }

        public void OnPointerEnter(PointerEventData eventData) => ShowText();
        public void OnSelect(BaseEventData eventData) => ShowText();

        void ShowText()
        {
            if (target != null) target.text = text;
        }
    }
}
