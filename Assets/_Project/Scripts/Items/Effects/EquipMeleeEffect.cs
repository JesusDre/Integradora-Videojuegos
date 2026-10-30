using UnityEngine;

namespace Animas.Items
{
    // Chancla bendita and cruz de plata: using them puts them in the Padre's hand instead of the
    // machete; using them again puts the machete back. They are not spent.
    [CreateAssetMenu(fileName = "EFF_Equipar", menuName = "Ánimas/Effects/Equip Melee")]
    public class EquipMeleeEffect : ItemEffect
    {
        [SerializeField] MeleeWeaponDefinition weapon;

        public override bool ConsumesItem => false;

        public override bool Use(ItemUser user, ItemDefinition item)
        {
            user.ToggleMelee(item, weapon);
            return true;
        }
    }
}
