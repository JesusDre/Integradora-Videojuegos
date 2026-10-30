using Animas.Core;
using Animas.Items;
using Animas.Levels;
using Animas.Save;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Animas.UI
{
    // End of an instance (GDD §17): when the Padre reaches the exit, the game stops and a summary shows
    // (time and deaths). It unlocks the next instance and packs the luggage for it (GDD P4: inventory and
    // materials pass complete). After instance 3 it's the final victory: "Ver créditos" opens them in the menu.
    public class VictoryScreen : MonoBehaviour
    {
        [SerializeField] InstanceCatalog catalog;
        [SerializeField] Health playerHealth;
        [SerializeField] PlayerPersistence persistence;
        [SerializeField] KeyItemBag keyItems;
        [Tooltip("Turned off while the screen shows (the camera script frees the mouse when it turns off).")]
        [SerializeField] Behaviour[] disableWhileShown;
        [SerializeField] GameObject panel;
        [SerializeField] TMP_Text title;
        [SerializeField] TMP_Text summary;
        [SerializeField] Button nextButton;
        [SerializeField] TMP_Text nextLabel;
        [SerializeField] Button quitButton;

        public bool IsShown => panel.activeSelf;

        int deaths;
        int nextIndex = -1;
        bool finalVictory;

        void Awake()
        {
            nextButton.onClick.AddListener(Next);
            quitButton.onClick.AddListener(() => SceneFlow.LoadMenu(catalog));
            panel.SetActive(false);
        }

        void OnEnable()
        {
            LevelExit.Reached += Show;
            playerHealth.Died += CountDeath;
        }

        void OnDisable()
        {
            LevelExit.Reached -= Show;
            playerHealth.Died -= CountDeath;
        }

        void CountDeath() => deaths++;

        void Show(LevelExit exit)
        {
            int index = SceneFlow.CurrentInstanceIndex(catalog);
            finalVictory = index == catalog.instances.Length - 1;
            nextIndex = index >= 0 && !finalVictory ? index + 1 : -1;

            if (nextIndex >= 0)
            {
                // Instance numbers start at 1; the catalog index at 0.
                GameProgress.Unlock(nextIndex + 1);
                string nextScene = catalog.instances[nextIndex].sceneName;
                SaveData luggage = persistence.CaptureForNextInstance(nextScene);
                SaveSystem.SaveCarry(nextScene, luggage);
                // "Continuar" in the menu now starts the next instance with that luggage.
                SaveSystem.Save(luggage);
            }
            else if (finalVictory)
            {
                // The campaign is over: there's nothing to continue (the selection stays unlocked).
                SaveSystem.Delete();
            }

            title.text = finalVictory ? "¡La Tzitzimime ha sido sellada!" : $"Escapaste {exit.InstanceName}";
            float seconds = Time.timeSinceLevelLoad;
            int notes = keyItems != null ? keyItems.NotesRead.Count : 0;
            summary.text = $"Tiempo: {(int)seconds / 60}:{(int)seconds % 60:00}    ·    Muertes: {deaths}    ·    Notas leídas: {notes}"
                + (finalVictory ? "\n\nEl Padre Anselmo escapa del convento. Los Hijos de la Estrella Negra han perdido." : "");
            nextLabel.text = finalVictory ? "Ver créditos" : nextIndex >= 0 ? "Siguiente instancia" : "Volver al menú";

            // Stop the game under the screen, like the pause does.
            Time.timeScale = 0f;
            InputSystem.actions.FindActionMap("Player", true).Disable();
            foreach (Behaviour b in disableWhileShown)
                if (b != null) b.enabled = false;
            panel.SetActive(true);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(nextButton.gameObject);
        }

        void Next()
        {
            if (nextIndex >= 0)
            {
                SceneFlow.StartInstance(catalog.instances[nextIndex]);
                return;
            }
            SceneFlow.ShowCreditsOnMenu = finalVictory;
            SceneFlow.LoadMenu(catalog);
        }
    }
}
