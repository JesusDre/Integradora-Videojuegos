using UnityEngine;

namespace Animas.Core
{
    // "Can the player see this point right now?" (#172): it must be on screen and nothing may stand
    // between the camera and it. Hitting the thing itself counts as seeing it (its own collider is
    // usually the first thing the line touches).
    public static class CameraSight
    {
        public static bool CanSee(Camera cam, Vector3 point, Transform self = null)
        {
            if (cam == null) return false;
            Vector3 v = cam.WorldToViewportPoint(point);
            if (v.z <= 0f || v.x < 0f || v.x > 1f || v.y < 0f || v.y > 1f) return false;
            if (!Physics.Linecast(cam.transform.position, point, out RaycastHit hit, ~0, QueryTriggerInteraction.Ignore))
                return true;
            return self != null && hit.transform.IsChildOf(self);
        }
    }
}
