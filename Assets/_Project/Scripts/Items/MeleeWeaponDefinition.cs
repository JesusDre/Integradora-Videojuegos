using UnityEngine;

namespace Animas.Items
{
    // Data for a melee weapon (machete, chancla bendita). Values from GDD §12 "Valores iniciales de balance".
    [CreateAssetMenu(fileName = "WPN_", menuName = "Ánimas/Melee Weapon")]
    public class MeleeWeaponDefinition : ScriptableObject
    {
        public string displayName = "machete";

        [Header("Damage")]
        [Min(0f)] public float damageToHumans = 20f;
        [Tooltip("Spirits only take a quarter of an unblessed weapon's damage; blessed weapons hit them harder.")]
        [Min(0f)] public float damageToSpirits = 5f;

        [Header("Use")]
        [Min(0.05f)] public float cooldown = 0.6f;
        [Min(0.5f)] public float range = 2f;
        [Tooltip("Radius of the swing: a little forgiving so the player doesn't need pixel-perfect aim.")]
        [Min(0f)] public float hitRadius = 0.35f;
        [Min(0f)] public float staminaCost = 15f;

        [Header("Feedback")]
        public AudioClip swingClip;
        public AudioClip hitClip;
    }
}
