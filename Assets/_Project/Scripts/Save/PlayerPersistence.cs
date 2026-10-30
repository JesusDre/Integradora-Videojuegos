using System.Collections.Generic;
using Animas.Core;
using Animas.Items;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Animas.Save
{
    // Puts the Padre's state into a SaveData and back: life, the 8 spaces, materials and recipes.
    // The safe room calls Capture when saving; respawning and "Continuar" call Restore.
    public class PlayerPersistence : MonoBehaviour
    {
        [SerializeField] ItemDatabase database;
        [SerializeField] Health health;
        [SerializeField] Inventory inventory;
        [SerializeField] RecipeBook recipeBook;
        [SerializeField] KeyItemBag keyItems;

        public SaveData Capture(string checkpoint, int section)
        {
            var data = new SaveData
            {
                scene = SceneManager.GetActiveScene().name,
                section = section,
                checkpoint = checkpoint,
                health = health.Current,
            };
            foreach (InventorySlot slot in inventory.Slots)
            {
                data.slotItems.Add(slot.IsEmpty ? "" : slot.item.name);
                data.slotCounts.Add(slot.IsEmpty ? 0 : slot.count);
            }
            foreach (var pair in inventory.Materials)
            {
                data.materialItems.Add(pair.Key.name);
                data.materialCounts.Add(pair.Value);
            }
            foreach (RecipeDefinition recipe in recipeBook.Known) data.recipes.Add(recipe.name);
            if (keyItems != null)
            {
                foreach (var pair in keyItems.KeyItems)
                    for (int i = 0; i < pair.Value; i++) data.keyItems.Add(pair.Key.name);
                data.collectedKeyItems.AddRange(keyItems.Collected);
                foreach (NoteDefinition note in keyItems.NotesRead) data.notes.Add(note.name);
            }
            return data;
        }

        // minimumHealth: after dying the Padre comes back with at least 60 (GDD §6).
        public void Restore(SaveData data, float minimumHealth)
        {
            var slotItems = new ItemDefinition[data.slotItems.Count];
            for (int i = 0; i < slotItems.Length; i++) slotItems[i] = Find(data.slotItems[i]);
            var materials = new ItemDefinition[data.materialItems.Count];
            for (int i = 0; i < materials.Length; i++) materials[i] = Find(data.materialItems[i]);
            inventory.SetContents(slotItems, data.slotCounts.ToArray(), materials, data.materialCounts.ToArray());

            var recipes = new List<RecipeDefinition>();
            foreach (string name in data.recipes)
            {
                RecipeDefinition recipe = database.FindRecipe(name);
                if (recipe != null) recipes.Add(recipe);
                else Debug.LogWarning($"[PlayerPersistence] Unknown recipe in save: {name}");
            }
            recipeBook.SetKnown(recipes);

            if (keyItems != null)
            {
                var carried = new Dictionary<ItemDefinition, int>();
                foreach (string name in data.keyItems)
                {
                    ItemDefinition item = Find(name);
                    if (item != null) carried[item] = carried.TryGetValue(item, out int n) ? n + 1 : 1;
                }
                var notes = new List<NoteDefinition>();
                foreach (string name in data.notes)
                {
                    NoteDefinition note = database.FindNote(name);
                    if (note != null) notes.Add(note);
                }
                var pairs = new List<(ItemDefinition, int)>();
                foreach (var pair in carried) pairs.Add((pair.Key, pair.Value));
                keyItems.SetContents(pairs, data.collectedKeyItems, notes);
            }

            health.Revive(Mathf.Max(minimumHealth, data.health));
        }

        ItemDefinition Find(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            ItemDefinition item = database.FindItem(name);
            // An item removed from the game shouldn't break old saves: skip it and say so.
            if (item == null) Debug.LogWarning($"[PlayerPersistence] Unknown item in save: {name}");
            return item;
        }
    }
}
