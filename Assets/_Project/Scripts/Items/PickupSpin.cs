using UnityEngine;

namespace Animas.Items
{
    // Turns and gently floats a key item so it catches the eye from far away (issue #29).
    // Only the model moves; the pickup's collider stays still so F always finds it.
    public class PickupSpin : MonoBehaviour
    {
        [SerializeField] float degreesPerSecond = 45f;
        [SerializeField, Min(0f)] float bobHeight = 0.05f;
        [SerializeField, Min(0.1f)] float bobSeconds = 2.5f;

        Vector3 start;

        void Start() => start = transform.localPosition;

        void Update()
        {
            transform.Rotate(0f, degreesPerSecond * Time.deltaTime, 0f, Space.World);
            float wave = Mathf.Sin(Time.time * 2f * Mathf.PI / bobSeconds);
            transform.localPosition = start + Vector3.up * (wave * bobHeight);
        }
    }
}
