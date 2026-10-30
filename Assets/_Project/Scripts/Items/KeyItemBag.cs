using System;
using System.Collections.Generic;
using UnityEngine;

namespace Animas.Items
{
    // The mission objects (GDD §10): 3 muñecas, 3 monedas or 3 fragmentos per instance. They don't take
    // inventory spaces, like materials: the player must never have to choose between them and a bandage.
    // Also remembers which notes were read. Both go into the save (#26).
    public class KeyItemBag : MonoBehaviour
    {
        readonly Dictionary<ItemDefinition, int> keyItems = new Dictionary<ItemDefinition, int>();
        // Which pickups in the world were taken (by their id), so each one knows whether to show up.
        // Placing an offering in the anchor lowers the count but the pickup stays taken.
        readonly HashSet<string> collected = new HashSet<string>();
        readonly HashSet<NoteDefinition> notesRead = new HashSet<NoteDefinition>();

        public IReadOnlyDictionary<ItemDefinition, int> KeyItems => keyItems;
        public IReadOnlyCollection<string> Collected => collected;
        public IReadOnlyCollection<NoteDefinition> NotesRead => notesRead;

        public event Action Changed;

        public int Count(ItemDefinition item) => keyItems.TryGetValue(item, out int n) ? n : 0;

        public bool HasCollected(string pickupId) => collected.Contains(pickupId);

        public void Add(ItemDefinition item, string pickupId)
        {
            if (!collected.Add(pickupId)) return;
            keyItems[item] = Count(item) + 1;
            Changed?.Invoke();
        }

        // All or nothing, like Inventory.Remove: the anchor takes the offerings it gets.
        public bool Remove(ItemDefinition item, int amount)
        {
            if (Count(item) < amount) return false;
            keyItems[item] -= amount;
            if (keyItems[item] == 0) keyItems.Remove(item);
            Changed?.Invoke();
            return true;
        }

        // Returns true the first time a note is read.
        public bool MarkRead(NoteDefinition note) => notesRead.Add(note);

        public bool HasRead(NoteDefinition note) => notesRead.Contains(note);

        // Loading a save.
        public void SetContents(IEnumerable<(ItemDefinition item, int count)> items, IEnumerable<string> collectedIds,
            IEnumerable<NoteDefinition> notes)
        {
            keyItems.Clear();
            foreach (var (item, count) in items)
                if (item != null && count > 0) keyItems[item] = count;
            collected.Clear();
            foreach (string id in collectedIds) collected.Add(id);
            notesRead.Clear();
            foreach (NoteDefinition note in notes)
                if (note != null) notesRead.Add(note);
            Changed?.Invoke();
        }
    }
}
