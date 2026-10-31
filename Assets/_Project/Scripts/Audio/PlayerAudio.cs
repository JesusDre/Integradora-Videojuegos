using Animas.Core;
using Animas.Items;
using Animas.Player;
using UnityEngine;

namespace Animas.Audio
{
    // The Padre's body sounds: a grunt when hit, the heartbeat when health is low, panting when out of
    // stamina, and the sound of each item he uses (ItemDefinition.useClip).
    public class PlayerAudio : MonoBehaviour
    {
        [SerializeField] Health health;
        [SerializeField] PlayerStamina stamina;
        [SerializeField] Inventory inventory;
        [Tooltip("One-shot sounds: hits and items (its AudioVolume follows the Effects slider).")]
        [SerializeField] AudioSource oneShots;
        [SerializeField] AudioClip hurtClip;

        [Header("Loops")]
        [SerializeField] AudioSource heartbeat;
        [Tooltip("Health at or under this starts the heartbeat (the same warning as the red pulse).")]
        [SerializeField, Min(1f)] float lowHealth = 30f;
        [SerializeField] AudioSource breathing;
        [SerializeField, Min(0.1f)] float fadeSpeed = 2f;

        float heartbeatVolume, breathingVolume;

        void Awake()
        {
            // The loops' Inspector volume is their loudest; they start silent and fade in.
            heartbeatVolume = heartbeat.volume;
            breathingVolume = breathing.volume;
            heartbeat.volume = 0f;
            breathing.volume = 0f;
        }

        void OnEnable()
        {
            health.Damaged += OnDamaged;
            inventory.Used += OnItemUsed;
        }

        void OnDisable()
        {
            health.Damaged -= OnDamaged;
            inventory.Used -= OnItemUsed;
        }

        void Update()
        {
            float effects = GameSettings.GetVolume(VolumeCategory.Effects);
            bool low = !health.IsDead && health.Current <= lowHealth;
            Fade(heartbeat, low ? heartbeatVolume * effects : 0f);
            Fade(breathing, stamina.IsExhausted && !health.IsDead ? breathingVolume * effects : 0f);
        }

        // Fading instead of cutting: a loop that stops dead is very noticeable.
        void Fade(AudioSource loop, float target)
        {
            loop.volume = Mathf.MoveTowards(loop.volume, target, fadeSpeed * Time.deltaTime);
            if (loop.volume > 0f && !loop.isPlaying) loop.Play();
            else if (loop.volume <= 0f && loop.isPlaying) loop.Stop();
        }

        void OnDamaged(float amount) => Play(hurtClip);

        void OnItemUsed(ItemDefinition item) => Play(item.useClip);

        void Play(AudioClip clip)
        {
            if (clip != null) oneShots.PlayOneShot(clip);
        }
    }
}
