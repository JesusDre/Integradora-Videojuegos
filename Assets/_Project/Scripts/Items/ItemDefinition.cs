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

        [Header("World")]
        [Tooltip("What appears on the floor when the player drops it. Empty: a generic box.")]
        public GameObject worldPrefab;

        public bool IsMaterial => category == ItemCategory.Material;
    }
}
