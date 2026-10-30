using Animas.Core;
using Animas.Save;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Animas.UI
{
    // Esc pauses (GDD §17): Continuar, Reiniciar (back to the last safe room, like Reintentar),
    // Opciones and Salir al menú. The game stops with Time.timeScale = 0 and the Padre's controls are
    // switched off, except the pause key itself.
    // Runs before the other UI scripts, so the Esc that closes an inventory or a note doesn't also pause.
    [DefaultExecutionOrder(-50)]
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] InstanceCatalog catalog;
        [SerializeField] GameObject panel;
        [SerializeField] OptionsUI options;
        [SerializeField] Button continueButton;
        [SerializeField] Button restartButton;
        [SerializeField] Button optionsButton;
        [SerializeField] Button quitButton;

        [Header("Player")]
        [SerializeField] Health playerHealth;
        [SerializeField] PlayerRespawn respawn;
        [Tooltip("Off while paused (the camera script also frees the mouse cursor when it turns off).")]
        [SerializeField] Behaviour[] disableWhilePaused;
        [Tooltip("If any of these panels is open, Esc closes it instead of pausing.")]
        [SerializeField] InventoryUI inventory;
        [SerializeField] AltarUI altar;
        [SerializeField] NoteUI note;

        public bool IsPaused { get; private set; }

        InputAction pauseAction;
        InputActionMap playerMap;

        void Awake()
        {
            pauseAction = InputSystem.actions.FindAction("Player/Pause", true);
            playerMap = pauseAction.actionMap;
            continueButton.onClick.AddListener(Resume);
            restartButton.onClick.AddListener(Restart);
            optionsButton.onClick.AddListener(OpenOptions);
            quitButton.onClick.AddListener(QuitToMenu);
            options.Closed += ShowPanel;
            panel.SetActive(false);
            options.gameObject.SetActive(false);
        }

        void Update()
        {
            if (!pauseAction.WasPressedThisFrame()) return;
            if (IsPaused)
            {
                // Esc inside Options goes back to the pause panel, not straight to the game.
                if (options.gameObject.activeSelf) options.Close();
                else Resume();
                return;
            }
            if (playerHealth.IsDead || AnyPanelOpen()) return;
            Pause();
        }

        bool AnyPanelOpen() =>
            (inventory != null && inventory.IsOpen) || (altar != null && altar.IsOpen) || (note != null && note.IsOpen);

        public void Pause()
        {
            IsPaused = true;
            Time.timeScale = 0f;
            AudioListener.pause = true;
            // Attack, interact, pray... must not fire while the menu is open; only Esc stays active.
            foreach (InputAction action in playerMap)
                if (action != pauseAction) action.Disable();
            foreach (Behaviour b in disableWhilePaused)
                if (b != null) b.enabled = false;
            ShowPanel();
        }

        public void Resume()
        {
            IsPaused = false;
            panel.SetActive(false);
            options.gameObject.SetActive(false);
            Time.timeScale = 1f;
            AudioListener.pause = false;
            foreach (InputAction action in playerMap) action.Enable();
            foreach (Behaviour b in disableWhilePaused)
                if (b != null) b.enabled = true;
        }

        void Restart()
        {
            Resume();
            // No safe room reached yet: start the instance over.
            if (respawn == null || !respawn.Respawn())
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        void OpenOptions()
        {
            panel.SetActive(false);
            options.gameObject.SetActive(true);
            Select(options.FirstSelected);
        }

        void ShowPanel()
        {
            panel.SetActive(true);
            Select(continueButton);
        }

        void QuitToMenu()
        {
            Resume();
            SceneFlow.LoadMenu(catalog);
        }

        static void Select(Selectable selectable)
        {
            if (EventSystem.current != null && selectable != null)
                EventSystem.current.SetSelectedGameObject(selectable.gameObject);
        }

        // Leaving Play Mode or the scene while paused must not leave the actions switched off.
        void OnDestroy()
        {
            if (IsPaused && playerMap != null)
                foreach (InputAction action in playerMap) action.Enable();
        }
    }
}
