using UnityEngine;

namespace Animas.Core
{
    // Makes a flame light (veladora, antorcha, fire) flicker instead of shining flat (issue #44).
    // Perlin noise moves smoothly, so it looks like a flame and not like a broken bulb.
    [RequireComponent(typeof(Light))]
    public class LightFlicker : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] float amount = 0.25f;
        [SerializeField, Min(0.1f)] float speed = 6f;

        Light flame;
        float baseIntensity;
        float seed;

        void Awake()
        {
            flame = GetComponent<Light>();
            baseIntensity = flame.intensity;
            seed = Random.value * 100f; // two candles side by side don't flicker in sync
        }

        void Update()
        {
            float noise = Mathf.PerlinNoise(seed, Time.time * speed);
            flame.intensity = baseIntensity * (1f - amount + amount * 2f * noise);
        }
    }
}
