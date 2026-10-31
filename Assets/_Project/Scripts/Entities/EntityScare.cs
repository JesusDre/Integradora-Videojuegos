using System;
using UnityEngine;

namespace Animas.Entities
{
    // An entity caught the Padre (#137): the HUD shows its face over the camera with a loud sting.
    // A static event so the entities don't need to know the HUD (the same idea as SafeRoom.Saved).
    public static class EntityScare
    {
        public static event Action<Sprite, AudioClip> Caught;

        public static void Trigger(Sprite face, AudioClip sting)
        {
            if (face != null) Caught?.Invoke(face, sting);
        }
    }
}
