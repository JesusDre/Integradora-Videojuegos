using System.Collections;
using TMPro;
using UnityEngine;

namespace Animas.UI
{
    // The horror mood of the main menu (behind the buttons there is a small 3D scene):
    //  - the camera sways very slowly, as if someone were watching;
    //  - the candles flicker;
    //  - the title glows red and trembles now and then, like a failing candle;
    //  - every so often the candles die for an instant and, when they come back, the Llorona is much
    //    closer (only her silhouette in the fog); the next blackout puts her back far away.
    public class MenuAtmosphere : MonoBehaviour
    {
        [Header("Camera")]
        [SerializeField] Transform cameraRig;
        [Tooltip("Degrees of the slow sway (x = up/down, y = left/right).")]
        [SerializeField] Vector2 swayDegrees = new Vector2(0.8f, 1.6f);
        [SerializeField] float swaySpeed = 0.12f;

        [Header("Candles")]
        [SerializeField] Light[] candles;
        [Min(0f)] [SerializeField] float flickerAmount = 0.35f;

        [Header("Title")]
        [SerializeField] TMP_Text title;

        [Header("The Llorona")]
        [SerializeField] Transform llorona;
        [Tooltip("Where she appears after a blackout (close to the camera).")]
        [SerializeField] Transform nearSpot;
        [Tooltip("Seconds between blackouts (random in this range).")]
        [SerializeField] Vector2 blackoutEvery = new Vector2(14f, 24f);
        [SerializeField] AudioSource stinger;

        Quaternion cameraRest;
        float[] baseIntensity;
        Vector3 farPosition;
        Quaternion farRotation;
        Color titleColor;
        bool near;
        public bool Paused { get; set; }   // the intro takes over the camera

        void Start()
        {
            if (cameraRig != null) cameraRest = cameraRig.localRotation;
            baseIntensity = new float[candles.Length];
            for (int i = 0; i < candles.Length; i++) baseIntensity[i] = candles[i].intensity;
            if (llorona != null) { farPosition = llorona.position; farRotation = llorona.rotation; }
            if (title != null) titleColor = title.color;
            StartCoroutine(Blackouts());
        }

        void Update()
        {
            float t = Time.time;
            if (cameraRig != null && !Paused)
            {
                float x = (Mathf.PerlinNoise(t * swaySpeed, 0.3f) - 0.5f) * 2f * swayDegrees.x;
                float y = (Mathf.PerlinNoise(0.7f, t * swaySpeed) - 0.5f) * 2f * swayDegrees.y;
                cameraRig.localRotation = cameraRest * Quaternion.Euler(x, y, 0f);
            }

            // Each candle flickers on its own (noise with a different seed per candle).
            for (int i = 0; i < candles.Length; i++)
            {
                if (candles[i] == null || !candles[i].enabled) continue;
                float n = Mathf.PerlinNoise(t * 9f, i * 3.7f);
                candles[i].intensity = baseIntensity[i] * (1f - flickerAmount + n * flickerAmount * 2f);
            }

            // The title: a dim red pulse, and rare short tremors.
            if (title != null)
            {
                float pulse = 0.75f + 0.25f * Mathf.PerlinNoise(t * 1.3f, 5f);
                bool tremor = Mathf.PerlinNoise(t * 4f, 9f) > 0.78f;
                title.color = new Color(titleColor.r * pulse, titleColor.g * pulse, titleColor.b * pulse, titleColor.a);
                title.rectTransform.localRotation = Quaternion.Euler(0f, 0f, tremor ? Random.Range(-0.6f, 0.6f) : 0f);
            }
        }

        IEnumerator Blackouts()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(blackoutEvery.x, blackoutEvery.y));
                if (Paused) continue;
                SetCandles(false);
                yield return new WaitForSeconds(0.35f);
                if (llorona != null && nearSpot != null)
                {
                    near = !near;
                    if (near) llorona.SetPositionAndRotation(nearSpot.position, nearSpot.rotation);
                    else llorona.SetPositionAndRotation(farPosition, farRotation);
                }
                SetCandles(true);
                if (near && stinger != null) stinger.Play();
            }
        }

        void SetCandles(bool on)
        {
            foreach (Light l in candles) if (l != null) l.enabled = on;
        }
    }
}
