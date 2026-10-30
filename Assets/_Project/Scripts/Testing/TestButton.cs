using Animas.Player;
using UnityEngine;
using UnityEngine.Events;

namespace Animas.Testing
{
    // A button for test scenes: press F on it and it runs whatever is wired in the Inspector
    // (spawn a cultist, heal the Padre...). Only used in Scenes/Test, never in the real levels.
    public class TestButton : MonoBehaviour, IInteractable
    {
        [SerializeField] string prompt = "Probar";
        [SerializeField] UnityEvent onPressed;

        public string Prompt => prompt;

        public void Interact(Interactor interactor) => onPressed.Invoke();
    }
}
