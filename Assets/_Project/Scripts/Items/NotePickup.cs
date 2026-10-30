using System;
using Animas.Player;
using UnityEngine;

namespace Animas.Items
{
    // A note on a table or a wall: F opens it on screen (NoteUI). The first time it's read, its recipe
    // is learned. It stays in the world so it can be read again.
    public class NotePickup : MonoBehaviour, IInteractable
    {
        [SerializeField] NoteDefinition note;

        public string Prompt => $"Leer: {note.title}";

        // The note screen (in the HUD) listens; static so notes don't need a reference to it.
        public static event Action<NoteDefinition> Opened;

        public void Interact(Interactor interactor)
        {
            var bag = interactor.GetComponentInParent<KeyItemBag>();
            bool firstTime = bag == null || bag.MarkRead(note);
            Opened?.Invoke(note);
            if (!firstTime || note.unlocksRecipe == null) return;

            var book = interactor.GetComponentInParent<RecipeBook>();
            var inventory = interactor.GetComponentInParent<Inventory>();
            if (book != null && !book.Knows(note.unlocksRecipe))
            {
                book.Unlock(note.unlocksRecipe);
                if (inventory != null) inventory.Notify($"Receta nueva en el altar: {note.unlocksRecipe.result.displayName}");
            }
        }
    }
}
