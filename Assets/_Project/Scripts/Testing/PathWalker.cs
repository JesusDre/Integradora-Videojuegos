using UnityEngine;
using UnityEngine.AI;

namespace Animas.Testing
{
    // Test helper for automated playtests: walks the Padre to a point along the NavMesh, moving his
    // CharacterController like the player would (so walls, doors and safe-room checks all apply).
    // Added only while testing; it is never part of a level.
    [RequireComponent(typeof(CharacterController))]
    public class PathWalker : MonoBehaviour
    {
        public float speed = 6f;

        public bool Walking => corners != null && index < corners.Length;
        public Vector3 Target { get; private set; }

        CharacterController body;
        Vector3[] corners;
        int index;

        void Awake() => body = GetComponent<CharacterController>();

        public bool WalkTo(Vector3 target)
        {
            Target = target;
            var path = new NavMeshPath();
            if (!NavMesh.CalculatePath(transform.position, target, NavMesh.AllAreas, path) || path.status == NavMeshPathStatus.PathInvalid)
                return false;
            corners = path.corners;
            index = 1;
            return path.status == NavMeshPathStatus.PathComplete;
        }

        public void Stop() => corners = null;

        void Update()
        {
            if (!Walking || !body.enabled) return;
            Vector3 to = corners[index] - transform.position;
            to.y = 0f;
            if (to.magnitude < 0.3f)
            {
                index++;
                return;
            }
            Vector3 step = to.normalized * speed * Time.deltaTime;
            body.Move(step + Vector3.down * 2f * Time.deltaTime);
            transform.rotation = Quaternion.LookRotation(to.normalized);
        }
    }
}
