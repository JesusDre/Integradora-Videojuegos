using UnityEngine;

namespace Animas.Items
{
    // Data for a firearm (escopeta, and since #135 pistola, metralleta and ballesta). Two kinds of ammo
    // (#134): salt rounds hit harder and are the only ones that hurt spirits (the Llorona, the Charro, the
    // Tzitzimime); normal rounds only hurt people. A reload uses salt rounds whenever there are any.
    [CreateAssetMenu(fileName = "WPN_", menuName = "Ánimas/Ranged Weapon")]
    public class RangedWeaponDefinition : ScriptableObject
    {
        public string displayName = "escopeta";
        [Tooltip("Salt rounds (blessed): more damage and they hurt spirits. Empty if the weapon has none.")]
        public ItemDefinition ammoItem;
        [Tooltip("Normal rounds: only hurt people. Empty if the weapon has none.")]
        public ItemDefinition normalAmmoItem;
        [Tooltip("Salt rounds multiply the damage by this.")]
        [Min(1f)] public float saltDamageMultiplier = 1.5f;
        [Tooltip("The model held in first person (instantiated under the gun's view).")]
        public GameObject viewModel;

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
        [Tooltip("Keeps firing while the click is held (metralleta).")]
        public bool automatic;
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
