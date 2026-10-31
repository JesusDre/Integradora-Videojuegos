using System;
using System.Collections.Generic;
using Animas.Items;
using UnityEngine;

namespace Animas.Player
{
    // The weapons the Padre carries (#134): the machete and the escopeta stopped being always there and
    // became items you find. They don't take inventory spaces (like the key items) and travel with the
    // Padre from one instance to the next. Q cycles through them (WeaponSwitcher).
    public class WeaponBag : MonoBehaviour
    {
        readonly List<ItemDefinition> weapons = new List<ItemDefinition>();

        public IReadOnlyList<ItemDefinition> Weapons => weapons;
        public int Count => weapons.Count;
        public event Action<ItemDefinition> Added;

        public bool Has(ItemDefinition weapon) => weapons.Contains(weapon);

        // Returns false if it wasn't a weapon or was already carried.
        public bool Add(ItemDefinition weapon)
        {
            if (weapon == null || !weapon.IsWeapon || weapons.Contains(weapon)) return false;
            weapons.Add(weapon);
            Added?.Invoke(weapon);
            return true;
        }

        // Loading a save or the luggage of the previous instance.
        public void SetContents(IEnumerable<ItemDefinition> carried)
        {
            weapons.Clear();
            foreach (ItemDefinition w in carried)
                if (w != null && w.IsWeapon && !weapons.Contains(w)) weapons.Add(w);
            foreach (ItemDefinition w in weapons) Added?.Invoke(w);
        }
    }
}
