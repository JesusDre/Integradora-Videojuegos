using UnityEngine;

namespace Animas.Items
{
    // Item categories from GDD §18. Materials don't take inventory space (they stack up to 99 by type).
    public enum ItemCategory { MeleeWeapon, RangedWeapon, Ammo, FaithObject, Consumable, Material, KeyItem, Note }

    // One kind of item: its name, icon and how it stacks. Every item in the game is one of these assets
    // (Data/Items), so adding a new item is creating an asset, not writing code.
    [CreateAssetMenu(fileName = "ITM_", menuName = "Ánimas/Item")]
    public class ItemDefinition : ScriptableObject
    {
        [Tooltip("Name shown to the player, in Spanish.")]
        public string displayName = "objeto";
        [TextArea] public string description;
        public Sprite icon;
        public ItemCategory category = ItemCategory.Consumable;

        [Tooltip("How many fit in one inventory slot. 1 = each one takes its own slot.")]
        [Min(1)] public int maxStack = 1;

        [Header("Use")]
        [Tooltip("Health restored when used from a quick slot (bandage: 30). 0 = not a healing item.")]
        [Min(0f)] public float healAmount;
        [Tooltip("What using it does (throw, place, equip). Empty: only healing items do something.")]
        public ItemEffect useEffect;
        [Tooltip("Sound when it is used (the bandage, the salt, the match of the candle). Optional.")]
        public AudioClip useClip;

        [Header("Weapon (#134)")]
        [Tooltip("For weapons carried in the WeaponBag: the melee data (machete, martillo...). Empty otherwise.")]
        public MeleeWeaponDefinition meleeWeapon;
        [Tooltip("For firearms carried in the WeaponBag: the gun data (escopeta, pistola...). Empty otherwise.")]
        public RangedWeaponDefinition rangedWeapon;

        [Header("World")]
        [Tooltip("What appears on the floor when the player drops it. Empty: a generic box.")]
        public GameObject worldPrefab;
        [Tooltip("The 3D model shown when it lies on the floor (issue #19). Empty = the generic box.")]
        public GameObject model;

        public bool IsMaterial => category == ItemCategory.Material;
        public bool IsWeapon => meleeWeapon != null || rangedWeapon != null;
    }
}
