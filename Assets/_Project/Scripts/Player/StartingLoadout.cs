using System.Collections;
using Animas.Items;
using UnityEngine;

namespace Animas.Player
{
    // A new run starts with the machete (#164; until then it was the machete or the escopeta at random, but
    // with so little ammo before the first safe room the escopeta was useless). The escopeta waits on the
    // supply table of the first safe room (StarterWeaponPickup).
    // A Padre who already carries weapons (luggage from the previous instance, a save) gets nothing.
    public class StartingLoadout : MonoBehaviour
    {
        [SerializeField] WeaponBag bag;
        [SerializeField] ItemDefinition machete;
        [SerializeField] ItemDefinition escopeta;

        public static ItemDefinition MissingStarter { get; private set; }

        IEnumerator Start()
        {
            // One frame: the save or the luggage of the previous instance is restored in Start.
            yield return null;
            if (bag.Count > 0)
            {
                MissingStarter = bag.Has(machete) ? (bag.Has(escopeta) ? null : escopeta) : machete;
                yield break;
            }
            bag.Add(machete);
            MissingStarter = escopeta;
        }
    }
}
