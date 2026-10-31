using UnityEngine;

namespace Animas.Items
{
    // Data for the shotgun. Values from GDD §12: 30 damage under 8 m (15 farther), 2 shells,
    // 2 s reload, heard by cultists 20 m away. Ammo is the "balas de sal" item from the inventory.
    [CreateAssetMenu(fileName = "WPN_", menuName = "Ánimas/Ranged Weapon")]
    public class RangedWeaponDefinition : ScriptableObject
    {
        public string displayName = "escopeta";
        public ItemDefinition ammoItem;

        [Header("Damage")]
        [Min(0f)] public float damageNear = 30f;
        [Min(0f)] public float damageFar = 15f;
        [Tooltip("Under this distance the shot does damageNear; beyond it, damageFar.")]
        [Min(0f)] public float nearDistance = 8f;
        [Min(1f)] public float maxRange = 40f;
        [Tooltip("Spirits take this part of the damage (GDD: 25 %).")]
        [Range(0f, 1f)] public float spiritMultiplier = 0.25f;
        [Tooltip("Radius of the shot: a shotgun spreads, so it forgives a bit of aim.")]
        [Min(0f)] public float hitRadius = 0.3f;
        [Tooltip("Pellets per shot. The damage above is the total if every pellet hits; each carries its share.")]
        [Min(1)] public int pellets = 8;
        [Tooltip("Half-angle of the cone the pellets spread in: they open up with distance.")]
        [Range(0f, 20f)] public float spreadDegrees = 5f;

        [Header("Use")]
        [Min(1)] public int shellCapacity = 2;
        [Min(0.1f)] public float reloadSeconds = 2f;
        [Min(0.05f)] public float fireCooldown = 0.4f;
        [Tooltip("Cultists inside this radius hear the shot and come to look.")]
        [Min(0f)] public float noiseRadius = 20f;

        [Header("Aim (right click)")]
        [Range(20f, 90f)] public float aimFieldOfView = 55f;

        [Header("Feedback")]
        [Min(0f)] public float kickDegrees = 4f;
        public AudioClip fireClip;
        public AudioClip reloadClip;
        public AudioClip emptyClip;
    }
}
