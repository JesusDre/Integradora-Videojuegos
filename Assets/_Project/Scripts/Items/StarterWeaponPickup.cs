using System.Collections;
using Animas.Player;
using UnityEngine;

namespace Animas.Items
{
    // On the supply table of the first safe room: it becomes whichever starting weapon the Padre did NOT
    // get (machete or escopeta, #134). If he already has both, the table has no weapon.
    [RequireComponent(typeof(PickupItem))]
    public class StarterWeaponPickup : MonoBehaviour
    {
        IEnumerator Start()
        {
            // StartingLoadout decides on its second frame; wait until it has.
            yield return null;
            yield return null;
            ItemDefinition missing = StartingLoadout.MissingStarter;
            if (missing == null) gameObject.SetActive(false);
            else GetComponent<PickupItem>().Set(missing, 1);
        }
    }
}
