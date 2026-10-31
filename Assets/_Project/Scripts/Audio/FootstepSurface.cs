using UnityEngine;

namespace Animas.Audio
{
    public enum SurfaceKind { Dirt, Wood, Stone }

    // Put on a floor's collider to say what it is made of; PlayerFootsteps reads it. Floors without
    // one sound like the scene's default (dirt in Xochimilco and the hacienda, stone in the convent).
    public class FootstepSurface : MonoBehaviour
    {
        public SurfaceKind kind = SurfaceKind.Wood;
    }
}
