using UnityEngine;

namespace Animas.Items
{
    // The list of every item and recipe in the game. The save file stores names ("ITM_Venda");
    // this turns them back into the assets. Add each new item or recipe here.
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "Ánimas/Item Database")]
    public class ItemDatabase : ScriptableObject
    {
        public ItemDefinition[] items;
        public RecipeDefinition[] recipes;
        public NoteDefinition[] notes;

        public ItemDefinition FindItem(string assetName)
        {
            foreach (ItemDefinition item in items)
                if (item != null && item.name == assetName) return item;
            return null;
        }

        public NoteDefinition FindNote(string assetName)
        {
            foreach (NoteDefinition note in notes)
                if (note != null && note.name == assetName) return note;
            return null;
        }

        public RecipeDefinition FindRecipe(string assetName)
        {
            foreach (RecipeDefinition recipe in recipes)
                if (recipe != null && recipe.name == assetName) return recipe;
            return null;
        }
    }
}
