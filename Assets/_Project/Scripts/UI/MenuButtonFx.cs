using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Animas.UI
{
    // A menu button that reacts like the rest of the menu: when the mouse is over it (or the keyboard
    // selects it) its label turns blood red, flickers for a moment, and a soft sound plays.
    public class MenuButtonFx : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] TMP_Text label;
        [SerializeField] AudioSource sound;
        [SerializeField] Color hoverColor = new Color(0.85f, 0.12f, 0.08f);

        Color normalColor;
        bool hovered;
        float flickerUntil;

        void Awake()
        {
            if (label == null) label = GetComponentInChildren<TMP_Text>();
            normalColor = label.color;
        }

        void OnDisable()
        {
            hovered = false;
            if (label != null) label.color = normalColor;
        }

        public void OnPointerEnter(PointerEventData e) => Hover(true);
        public void OnPointerExit(PointerEventData e) => Hover(false);
        public void OnSelect(BaseEventData e) => Hover(true);
        public void OnDeselect(BaseEventData e) => Hover(false);

        void Hover(bool on)
        {
            if (on == hovered) return;
            hovered = on;
            if (on)
            {
                flickerUntil = Time.unscaledTime + 0.25f;
                if (sound != null) sound.PlayOneShot(sound.clip);
            }
            label.color = on ? hoverColor : normalColor;
        }

        void Update()
        {
            if (!hovered || Time.unscaledTime > flickerUntil) return;
            // A quick on/off flicker right after the mouse arrives, like a dying bulb.
            bool lit = Mathf.Repeat(Time.unscaledTime * 30f, 1f) > 0.35f;
            label.color = lit ? hoverColor : normalColor * 0.5f;
        }
    }
}
