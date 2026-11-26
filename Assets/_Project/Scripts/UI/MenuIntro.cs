using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Animas.UI
{
    // When the player picks an instance, the menu does not cut straight to the level: the buttons fade,
    // the camera glides to the Padre standing in the fog (the full model, not just his arms), his name
    // shows for a moment, and the screen goes black. Then the level loads.
    public class MenuIntro : MonoBehaviour
    {
        [SerializeField] Transform cameraRig;
        [Tooltip("Where the camera ends: just behind the Padre's shoulder.")]
        [SerializeField] Transform closeUp;
        [SerializeField] CanvasGroup menuGroup;
        [Tooltip("Full-screen black image for the fade.")]
        [SerializeField] Image fade;
        [SerializeField] CanvasGroup nameCard;
        [SerializeField] MenuAtmosphere atmosphere;
        [Min(0.5f)] [SerializeField] float seconds = 3.2f;

        public bool Playing { get; private set; }

        public void Play(Action then)
        {
            if (Playing) return;
            Playing = true;
            StartCoroutine(Run(then));
        }

        IEnumerator Run(Action then)
        {
            if (atmosphere != null) atmosphere.Paused = true;
            if (menuGroup != null) menuGroup.interactable = false;
            Vector3 fromPos = cameraRig.position;
            Quaternion fromRot = cameraRig.rotation;
            for (float t = 0f; t < seconds; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / seconds);
                cameraRig.SetPositionAndRotation(Vector3.Lerp(fromPos, closeUp.position, k),
                                                 Quaternion.Slerp(fromRot, closeUp.rotation, k));
                if (menuGroup != null) menuGroup.alpha = 1f - Mathf.Clamp01(t / 0.6f);
                // His name in the middle of the move, then black in the last second.
                if (nameCard != null) nameCard.alpha = Mathf.Clamp01((t - seconds * 0.3f) / 0.5f) * Mathf.Clamp01((seconds - 0.4f - t) / 0.5f);
                if (fade != null) fade.color = new Color(0f, 0f, 0f, Mathf.Clamp01((t - (seconds - 1f)) / 1f));
                yield return null;
            }
            if (fade != null) fade.color = Color.black;
            then?.Invoke();
        }
    }
}
