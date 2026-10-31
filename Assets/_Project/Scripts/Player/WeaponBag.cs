using System;
using System.Collections.Generic;
using Animas.Items;
using UnityEngine;

namespace Animas.Player
{
    // The weapons the Padre carries (#134): the machete and the escopeta stopped being always there and
    // became items you find. They don't take inventory spaces (like the key items) and travel with the
    // Padre from one instance to the next. Q cycles through them (WeaponSwitcher). Only 2 fit (#152), any
    // mix of melee and guns: a third one is taken by swapping it for one carried (PickupItem).
    public class WeaponBag : MonoBehaviour
    {
        public const int MaxWeapons = 2;

        readonly List<ItemDefinition> weapons = new List<ItemDefinition>();

        public IReadOnlyList<ItemDefinition> Weapons => weapons;
        public int Count => weapons.Count;
        public bool IsFull => weapons.Count >= MaxWeapons;
        public event Action<ItemDefinition> Added;
        // The list changed in any way (added, swapped, loaded): the HUD redraws its weapon spaces.
        public event Action Changed;

        public bool Has(ItemDefinition weapon) => weapons.Contains(weapon);

        // Returns false if it wasn't a weapon, was already carried or there is no room.
        public bool Add(ItemDefinition weapon)
        {
            if (weapon == null || !weapon.IsWeapon || weapons.Contains(weapon) || IsFull) return false;
            weapons.Add(weapon);
            Added?.Invoke(weapon);
            Changed?.Invoke();
            return true;
        }

        // The new weapon takes the place (and the space on screen) of the one given away.
        public bool Swap(ItemDefinition givenAway, ItemDefinition taken)
        {
            int i = weapons.IndexOf(givenAway);
            if (i < 0 || taken == null || !taken.IsWeapon || weapons.Contains(taken)) return false;
            weapons[i] = taken;
            Added?.Invoke(taken);
            Changed?.Invoke();
            return true;
        }

        // Loading a save or the luggage of the previous instance. Older saves could carry more than 2:
        // the first 2 are kept.
        public void SetContents(IEnumerable<ItemDefinition> carried)
        {
            weapons.Clear();
            foreach (ItemDefinition w in carried)
                if (w != null && w.IsWeapon && !weapons.Contains(w) && !IsFull) weapons.Add(w);
            foreach (ItemDefinition w in weapons) Added?.Invoke(w);
            Changed?.Invoke();
        }
    }
}
