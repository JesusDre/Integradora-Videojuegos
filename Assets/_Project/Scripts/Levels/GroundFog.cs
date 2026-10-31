using UnityEngine;

namespace Animas.Levels
{
    // Low fog that crawls along the ground (issue #109). The particles are born in a wide box around the
    // Padre and stay where they were born (world space), so walking through them looks like real mist;
    // the emitter only follows him on the ground plane so there is always fog nearby.
    [RequireComponent(typeof(ParticleSystem))]
    public class GroundFog : MonoBehaviour
    {
        [Tooltip("Height of the fog layer above the floor the Padre is standing on.")]
        [SerializeField] float height = 0.4f;

        Transform player;

        void Start()
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
        }

        void LateUpdate()
        {
            if (player == null) return;
            transform.position = new Vector3(player.position.x, player.position.y + height, player.position.z);
        }
    }
}
