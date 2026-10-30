namespace Animas.Player
{
    // Anything the player can use with F: doors, pickups, notes, the altar...
    // An interface lets the Interactor work with all of them without knowing each type.
    public interface IInteractable
    {
        // Text shown next to the F key, in Spanish (e.g. "Abrir puerta").
        string Prompt { get; }

        void Interact(Interactor interactor);
    }
}
