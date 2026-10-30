using Animas.Core;
using UnityEngine;

namespace Animas.Levels
{
    // A place a spirit can't enter, but people can: the hacienda's chapel keeps the Charro Negro out
    // while the cultists walk in (GDD §13, P2). It's always on, unlike the prayer circle or the salt line.
    public class SpiritBarrier : MonoBehaviour, ISpiritWard
    {
        [Tooltip("Trigger box covering the protected area.")]
        [SerializeField] BoxCollider area;
        [Tooltip("Where a spirit that touched the area is put back (outside the entrance).")]
        [SerializeField] Transform spiritExit;

        void OnEnable() => SpiritWards.Add(this);
        void OnDisable() => SpiritWards.Remove(this);

        public bool Contains(Vector3 point) => area.bounds.Contains(point + Vector3.up * 0.5f);

        public Vector3 PushOut(Vector3 point) => spiritExit != null ? spiritExit.position : point;
    }
}
