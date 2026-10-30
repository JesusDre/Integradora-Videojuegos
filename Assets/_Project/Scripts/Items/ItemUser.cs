using Animas.Player;
using UnityEngine;

namespace Animas.Items
{
    // The Padre's side of using items: where he aims, how to equip a melee weapon and which item is
    // equipped. Item effects use it instead of searching for the player's components.
    public class ItemUser : MonoBehaviour
    {
        [SerializeField] Transform aim;
        [SerializeField] PlayerCombat combat;
        [SerializeField] WeaponSwitcher weapons;
        [SerializeField] Inventory inventory;

        public Transform Aim => aim;
        public Inventory Inventory => inventory;
        public ItemDefinition EquippedItem { get; private set; }

        MeleeWeaponDefinition defaultWeapon;

        void Awake() => defaultWeapon = combat.Weapon;

        void OnEnable() => inventory.Changed += CheckEquipped;
        void OnDisable() => inventory.Changed -= CheckEquipped;

        // Using the chancla or the cruz equips it; using it again goes back to the machete.
        public void ToggleMelee(ItemDefinition item, MeleeWeaponDefinition weapon)
        {
            if (EquippedItem == item)
            {
                Unequip();
                inventory.Notify($"Guardaste: {item.displayName}");
                return;
            }
            EquippedItem = item;
            combat.SetWeapon(weapon);
            weapons.Equip(WeaponSwitcher.Slot.Melee);
            inventory.Notify($"En la mano: {item.displayName}");
        }

        void Unequip()
        {
            EquippedItem = null;
            combat.SetWeapon(defaultWeapon);
        }

        // If the equipped item was dropped (or spent), the machete comes back.
        void CheckEquipped()
        {
            if (EquippedItem != null && inventory.CountOf(EquippedItem) == 0) Unequip();
        }
    }
}
