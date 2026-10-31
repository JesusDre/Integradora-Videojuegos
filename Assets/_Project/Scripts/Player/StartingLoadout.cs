using System.Collections;
using Animas.Items;
using UnityEngine;

namespace Animas.Player
{
    // A new run starts with ONE weapon, chosen at random (#134): the machete, or the escopeta with a few
    // salt rounds. The other one waits on the supply table of the first safe room (StarterWeaponPickup).
    // A Padre who already carries weapons (luggage from the previous instance, a save) gets nothing.
    public class StartingLoadout : MonoBehaviour
    {
        [SerializeField] WeaponBag bag;
        [SerializeField] Inventory inventory;
        [SerializeField] ItemDefinition machete;
        [SerializeField] ItemDefinition escopeta;
        [Tooltip("Salt rounds that come with the escopeta when it is the starting weapon.")]
        [SerializeField, Min(0)] int escopetaRounds = 6;

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
            bool gun = Random.value < 0.5f;
            bag.Add(gun ? escopeta : machete);
            if (gun) inventory.TryAdd(escopeta.rangedWeapon.ammoItem, escopetaRounds, false);
            MissingStarter = gun ? machete : escopeta;
        }
    }
}
