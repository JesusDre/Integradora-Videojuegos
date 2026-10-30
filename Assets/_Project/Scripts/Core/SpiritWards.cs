using System.Collections.Generic;
using UnityEngine;

namespace Animas.Core
{
    // Something spirits can't go into: the prayer circle now, the salt line later (#31).
    public interface ISpiritWard
    {
        bool Contains(Vector3 point);
        // The nearest point just outside the ward, where a spirit that touched it is pushed back to.
        Vector3 PushOut(Vector3 point);
    }

    // Every active ward registers here, so a spirit asks one place instead of searching the scene.
    public static class SpiritWards
    {
        static readonly List<ISpiritWard> active = new List<ISpiritWard>();

        public static void Add(ISpiritWard ward)
        {
            if (!active.Contains(ward)) active.Add(ward);
        }

        public static void Remove(ISpiritWard ward) => active.Remove(ward);

        // If the point is inside a ward, returns true and where to put the spirit instead.
        public static bool TryRepel(Vector3 point, out Vector3 safePoint)
        {
            foreach (ISpiritWard ward in active)
            {
                if (!ward.Contains(point)) continue;
                safePoint = ward.PushOut(point);
                return true;
            }
            safePoint = point;
            return false;
        }

        // Domain reload can be off in the Editor: start each Play Mode with an empty list.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetOnPlay() => active.Clear();
    }
}
