using Animas.Entities;
using Animas.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Animas.UI
{
    // The scare screen (#137): when the Llorona or the Charro catches the Padre before the final fight, its
    // face fills the screen for a moment, rushing toward the camera, with a sting and a hard shake.
    // Unscaled time, so it also plays if something pauses the game in that instant.
    public class JumpscareUI : MonoBehaviour
    {
        [SerializeField] Image face;
        [SerializeField] AudioSource sting;
        [SerializeField, Min(0.1f)] float holdSeconds = 0.55f;
        [SerializeField, Min(0.05f)] float fadeSeconds = 0.45f;
        [Tooltip("Seconds before another scare can play (a grab that lasts doesn't repeat it).")]
        [SerializeField, Min(0f)] float cooldown = 8f;

        PlayerLook look;
        float startedAt = -100f;
        float readyAt;

        void Start()
        {
            face.enabled = false;
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) look = p.GetComponent<PlayerLook>();
        }

        void OnEnable() => EntityScare.Caught += Show;
        void OnDisable() => EntityScare.Caught -= Show;

        void Show(Sprite sprite, AudioClip clip)
        {
            if (Time.unscaledTime < readyAt) return;
            readyAt = Time.unscaledTime + cooldown;
            startedAt = Time.unscaledTime;
            face.sprite = sprite;
            face.enabled = true;
            if (sting != null && clip != null) sting.PlayOneShot(clip);
            if (look != null) look.AddShake(1f);
        }

        void Update()
        {
            if (!face.enabled) return;
            float t = Time.unscaledTime - startedAt;
            // It lunges at the camera (from 115 % to 100 % in a blink), holds, then fades out.
            float s = Mathf.Lerp(1.15f, 1f, Mathf.Clamp01(t / 0.08f));
            face.rectTransform.localScale = new Vector3(s, s, 1f);
            float alpha = t < holdSeconds ? 1f : 1f - (t - holdSeconds) / fadeSeconds;
            Color c = face.color;
            c.a = Mathf.Clamp01(alpha);
            face.color = c;
            if (alpha <= 0f) face.enabled = false;
        }
    }
}
