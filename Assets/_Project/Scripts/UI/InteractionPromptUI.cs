using Animas.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Animas.UI
{
    // Center-screen dot plus "[F] Abrir puerta" when the player looks at something usable.
    public class InteractionPromptUI : MonoBehaviour
    {
        [SerializeField] Interactor interactor;
        [SerializeField] GameObject promptRoot;
        [SerializeField] TMP_Text promptText;
        [SerializeField] Image crosshair;
        [SerializeField] Color idleColor = new Color(1f, 1f, 1f, 0.35f);
        [SerializeField] Color focusColor = Color.white;

        void OnEnable()
        {
            // The prefab can't point to the player's Interactor; each scene must wire it.
            if (interactor == null)
            {
                Debug.LogError($"[InteractionPromptUI] '{name}' has no Interactor assigned.", this);
                enabled = false;
                return;
            }
            interactor.FocusChanged += Show;
            Show(interactor.Current);
        }

        void OnDisable()
        {
            if (interactor != null) interactor.FocusChanged -= Show;
        }

        void Show(IInteractable target)
        {
            bool hasTarget = target != null;
            promptRoot.SetActive(hasTarget);
            if (hasTarget) promptText.text = $"[F] {target.Prompt}";
            // The dot brightens when something is usable: a hint even before reading the text.
            crosshair.color = hasTarget ? focusColor : idleColor;
        }
    }
}
