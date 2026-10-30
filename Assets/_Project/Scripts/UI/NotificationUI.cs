using Animas.Items;
using Animas.Player;
using TMPro;
using UnityEngine;

namespace Animas.UI
{
    // Short messages at the bottom of the screen ("Inventario lleno", "Recogiste: venda") that fade out.
    // Listens to the inventory; other systems can call Show too.
    public class NotificationUI : MonoBehaviour
    {
        [SerializeField] Inventory inventory;
        [Tooltip("Optional: \"Aún no puedes rezar\".")]
        [SerializeField] PlayerPrayer prayer;
        [SerializeField] TMP_Text label;
        [SerializeField, Min(0.1f)] float visibleSeconds = 2f;
        [SerializeField, Min(0.1f)] float fadeSeconds = 0.5f;

        float shownAt = float.NegativeInfinity;

        void OnEnable()
        {
            if (inventory == null)
            {
                Debug.LogError($"[NotificationUI] '{name}' has no Inventory assigned.", this);
                enabled = false;
                return;
            }
            inventory.Notice += Show;
            if (prayer != null) prayer.Notice += Show;
            label.alpha = 0f;
        }

        void OnDisable()
        {
            if (inventory != null) inventory.Notice -= Show;
            if (prayer != null) prayer.Notice -= Show;
        }

        public void Show(string message)
        {
            label.text = message;
            shownAt = Time.time;
        }

        void Update()
        {
            float age = Time.time - shownAt;
            // Fully visible for a moment, then fades out.
            label.alpha = age < visibleSeconds ? 1f : Mathf.Clamp01(1f - (age - visibleSeconds) / fadeSeconds);
        }
    }
}
