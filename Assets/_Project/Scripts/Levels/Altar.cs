using System;
using System.Collections;
using Animas.Core;
using Animas.Player;
using UnityEngine;

namespace Animas.Levels
{
    // The altar of the safe room (GDD §5): F opens the crafting menu and, if the Padre is hurt,
    // brings his life back up to 60 %. When something is crafted, the altar's candle light flares:
    // the Padre blesses the object.
    public class Altar : MonoBehaviour, IInteractable
    {
        [SerializeField, Range(0f, 1f)] float restoreUpTo = 0.6f;
        [SerializeField] Light candleLight;
        [SerializeField] float blessIntensity = 6f;
        [SerializeField] float blessSeconds = 0.6f;
        [SerializeField] AudioSource audioSource;
        [SerializeField] AudioClip blessClip;

        public string Prompt => "Usar altar";

        // Static: the altar UI (in the HUD prefab) opens for any altar without each one pointing to it.
        public static event Action<Altar, Interactor> Opened;

        float baseIntensity;

        void Awake()
        {
            if (candleLight != null) baseIntensity = candleLight.intensity;
        }

        public void Interact(Interactor interactor)
        {
            var health = interactor.GetComponentInParent<Health>();
            if (health != null && health.Current < health.Max * restoreUpTo)
                health.Heal(health.Max * restoreUpTo - health.Current);
            Opened?.Invoke(this, interactor);
        }

        public void Bless()
        {
            if (audioSource != null && blessClip != null) audioSource.PlayOneShot(blessClip);
            if (candleLight != null)
            {
                StopAllCoroutines();
                StartCoroutine(Flare());
            }
        }

        IEnumerator Flare()
        {
            for (float t = 0f; t < blessSeconds; t += Time.deltaTime)
            {
                // Bright at once, then fading out: a short glow over the altar.
                float k = 1f - t / blessSeconds;
                candleLight.intensity = Mathf.Lerp(baseIntensity, blessIntensity, k * k);
                yield return null;
            }
            candleLight.intensity = baseIntensity;
        }
    }
}
