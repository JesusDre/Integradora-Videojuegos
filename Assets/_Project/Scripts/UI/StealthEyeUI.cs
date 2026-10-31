using Animas.Enemies;
using Animas.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Animas.UI
{
    // The stealth eye (#136). While the Padre is crouched, or while someone is after him, an eye on the HUD
    // says how hidden he is: closed = nobody knows where he is; half-open (yellow) = a cultist is searching
    // for him; wide open (red, pulsing) = a cultist sees him. It only reads the cultists' states.
    public class StealthEyeUI : MonoBehaviour
    {
        [SerializeField] Image eye;
        [SerializeField] Sprite hidden;
        [SerializeField] Sprite searching;
        [SerializeField] Sprite seen;

        enum Alert { Hidden, Searching, Seen }

        PlayerController player;
        Alert alert;
        float nextCheck;

        void Start()
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) player = p.GetComponent<PlayerController>();
        }

        void Update()
        {
            // Five times a second is enough for a warning, and cheaper than every frame.
            if (Time.time >= nextCheck)
            {
                nextCheck = Time.time + 0.2f;
                alert = Check();
            }
            bool show = player != null && (player.IsCrouching || alert != Alert.Hidden);
            eye.enabled = show;
            if (!show) return;
            eye.sprite = alert == Alert.Seen ? seen : alert == Alert.Searching ? searching : hidden;
            // Seen: the eye throbs so the warning can't be missed.
            float s = alert == Alert.Seen ? 1f + 0.12f * Mathf.Sin(Time.time * 12f) : 1f;
            eye.rectTransform.localScale = new Vector3(s, s, 1f);
        }

        static Alert Check()
        {
            Alert worst = Alert.Hidden;
            foreach (CultistAI c in FindObjectsByType<CultistAI>(FindObjectsSortMode.None))
            {
                if (!c.isActiveAndEnabled) continue;
                if (c.Current == CultistAI.State.Chase || c.Current == CultistAI.State.Attack) return Alert.Seen;
                if (c.Current == CultistAI.State.Search) worst = Alert.Searching;
            }
            return worst;
        }
    }
}
