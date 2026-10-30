using UnityEngine;

namespace Animas.Items
{
    // A note left by the cult (GDD §10 and §18). Reading it can teach a recipe: the Padre reads the
    // cult's symbols (GDD §11), which is how new altar recipes are unlocked.
    [CreateAssetMenu(fileName = "NOTE_", menuName = "Ánimas/Note")]
    public class NoteDefinition : ScriptableObject
    {
        public string title = "Nota";
        [TextArea(4, 12)] public string text;
        [Tooltip("Recipe learned by reading it. Empty = a story note only.")]
        public RecipeDefinition unlocksRecipe;
    }
}
