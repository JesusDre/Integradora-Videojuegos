using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Animas.UI
{
    // A horizontal bar (life, stamina, ritual): the fill image's width follows current / max.
    public class StatBar : MonoBehaviour
    {
        [SerializeField] Image fill;
        [SerializeField] TMP_Text valueLabel;

        public void Set(float current, float max)
        {
            // max can be 0 before the owner is set up; avoid dividing by zero.
            float ratio = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            fill.fillAmount = ratio;
            if (valueLabel != null) valueLabel.text = Mathf.CeilToInt(current).ToString();
        }

        public void SetColor(Color color) => fill.color = color;
    }
}
