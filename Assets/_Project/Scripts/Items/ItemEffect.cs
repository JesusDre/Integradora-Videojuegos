using UnityEngine;

namespace Animas.Items
{
    // What happens when an item is used from a quick slot (GDD §14): thrown where the Padre looks,
    // placed in front of him, or equipped as a weapon. Each kind is a subclass and each item points to
    // one asset (Data/Items/Effects), so a new faith object is data, not new inventory code.
    public abstract class ItemEffect : ScriptableObject
    {
        // False = the item stays in the inventory after use (weapons are equipped, not spent).
        public virtual bool ConsumesItem => true;

        // Returns true if it was used; false leaves the item where it was.
        public abstract bool Use(ItemUser user, ItemDefinition item);
    }
}
