using Animas.UI;
using UnityEngine;

namespace Animas.Narrative
{
    // A place where the Padre says something the first time he arrives (issue #33). Like the other zones,
    // it checks the Padre's position each frame instead of physics triggers, so teleports never miss it.
    [RequireComponent(typeof(BoxCollider))]
    public class SubtitleTrigger : MonoBehaviour
    {
        [SerializeField, TextArea] string line = "«…»";

        BoxCollider zone;
        Transform player;
        SubtitleUI subtitles;
        bool said;

        void Awake()
        {
            zone = GetComponent<BoxCollider>();
            zone.isTrigger = true;
        }

        void Start()
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
            subtitles = FindAnyObjectByType<SubtitleUI>(FindObjectsInactive.Include);
        }

        void Update()
        {
            if (said || player == null || !zone.bounds.Contains(player.position + Vector3.up * 0.5f)) return;
            said = true;
            if (subtitles != null) subtitles.Show(line);
        }
    }
}
