using Animas.Core;
using UnityEngine;

namespace Animas.Audio
{
    // Now and then, a sound from this spot (issue #123): the dolls laughing in the chinampa, bells far away,
    // the chapel bell. Random clip, random wait, and only while the Padre is within hearing range, so the
    // place feels alive without a fixed loop the player learns to ignore.
    [RequireComponent(typeof(AudioSource))]
    public class AmbientOneShots : MonoBehaviour
    {
        [SerializeField] AudioClip[] clips;
        [Tooltip("Seconds between sounds (random between both).")]
        [SerializeField] Vector2 interval = new Vector2(20f, 40f);
        [Tooltip("Only plays while the Padre is closer than this (0 = anywhere in the level).")]
        [SerializeField, Min(0f)] float hearingRadius = 30f;

        AudioSource source;
        Transform player;
        float nextAt;
        int last = -1;

        void Awake()
        {
            source = GetComponent<AudioSource>();
            nextAt = Time.time + Random.Range(interval.x * 0.3f, interval.y * 0.6f);
        }

        void Start()
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
        }

        void Update()
        {
            if (Time.time < nextAt || clips == null || clips.Length == 0) return;
            nextAt = Time.time + Random.Range(interval.x, interval.y);
            if (player != null && hearingRadius > 0f && (player.position - transform.position).sqrMagnitude > hearingRadius * hearingRadius) return;
            // Not the same one twice in a row.
            int index = Random.Range(0, clips.Length);
            if (index == last && clips.Length > 1) index = (index + 1) % clips.Length;
            last = index;
            source.PlayOneShot(clips[index], GameSettings.GetVolume(VolumeCategory.Effects));
        }
    }
}
