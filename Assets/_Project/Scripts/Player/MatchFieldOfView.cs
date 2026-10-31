using UnityEngine;

namespace Animas.Player
{
    // The arms and the weapons are drawn by their own overlay camera, on top of the world, so they never
    // sink into a wall the Padre is standing against (issue #51; the rubric penalizes going through
    // objects). That camera must see with the same field of view as the main one, also while the shotgun
    // zooms in, or the arms would float apart from the world.
    [RequireComponent(typeof(Camera))]
    public class MatchFieldOfView : MonoBehaviour
    {
        [SerializeField] Camera source;

        Camera own;

        void Awake() => own = GetComponent<Camera>();

        void LateUpdate() => own.fieldOfView = source.fieldOfView;
    }
}
