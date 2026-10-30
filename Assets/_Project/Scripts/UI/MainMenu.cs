using Animas.Core;
using Animas.Save;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Animas.UI
{
    // The main menu scene (GDD §17): Nueva partida → instance selection, Continuar, Opciones, Créditos
    // and Salir. Only one panel is visible at a time; Esc goes back to the main panel.
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] InstanceCatalog catalog;

        [Header("Panels")]
        [SerializeField] GameObject mainPanel;
        [SerializeField] GameObject selectPanel;
        [SerializeField] OptionsUI optionsPanel;
        [SerializeField] GameObject creditsPanel;

        [Header("Main panel")]
        [SerializeField] Button newGameButton;
        [SerializeField] Button continueButton;
        [SerializeField] Button optionsButton;
        [SerializeField] Button creditsButton;
        [SerializeField] Button quitButton;

        [Header("Instance selection")]
        [Tooltip("One button per instance, in order (its label shows the name or 'Bloqueada').")]
        [SerializeField] Button[] instanceButtons;
        [SerializeField] TMP_Text instanceDescription;
        [SerializeField] Button selectBackButton;

        [Header("Credits")]
        [SerializeField] Button creditsBackButton;

        void Awake()
        {
            newGameButton.onClick.AddListener(() => Show(selectPanel, instanceButtons[0]));
            continueButton.onClick.AddListener(ContinueGame);
            optionsButton.onClick.AddListener(() => Show(optionsPanel.gameObject, optionsPanel.FirstSelected));
            creditsButton.onClick.AddListener(() => Show(creditsPanel, creditsBackButton));
            quitButton.onClick.AddListener(Quit);
            selectBackButton.onClick.AddListener(ShowMain);
            creditsBackButton.onClick.AddListener(ShowMain);
            optionsPanel.Closed += ShowMain;

            for (int i = 0; i < instanceButtons.Length; i++)
            {
                int index = i; // captured by the lambda: each button needs its own copy
                instanceButtons[i].onClick.AddListener(() => StartInstance(index));
            }
        }

        void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 1f;
            RefreshInstances();
            ShowMain();
        }

        void Update()
        {
            // Esc (or the gamepad's B) backs out of any sub-panel.
            if (!mainPanel.activeSelf && UnityEngine.InputSystem.Keyboard.current != null
                && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
                ShowMain();
        }

        void RefreshInstances()
        {
            // Continuar only works if there's a save of a scene that is in the build.
            SaveData save = SaveSystem.Load();
            continueButton.interactable = save != null && SceneFlow.CanLoad(save.scene);

            for (int i = 0; i < instanceButtons.Length; i++)
            {
                bool exists = i < catalog.instances.Length;
                bool unlocked = exists && GameProgress.IsUnlocked(i + 1);
                instanceButtons[i].interactable = unlocked;
                TMP_Text label = instanceButtons[i].GetComponentInChildren<TMP_Text>();
                if (label != null && exists)
                    label.text = unlocked ? catalog.instances[i].displayName : $"{catalog.instances[i].displayName}  (bloqueada)";

                // Hovering or selecting with the keyboard shows the description below.
                var trigger = instanceButtons[i].GetComponent<SelectableDescription>();
                if (trigger != null && exists) trigger.Setup(instanceDescription, catalog.instances[i].description);
            }
        }

        void StartInstance(int index)
        {
            if (index >= catalog.instances.Length || !GameProgress.IsUnlocked(index + 1)) return;
            SceneFlow.StartInstance(catalog.instances[index]);
        }

        void ContinueGame()
        {
            if (!SceneFlow.Continue()) continueButton.interactable = false;
        }

        void ShowMain() => Show(mainPanel, newGameButton);

        void Show(GameObject panel, Selectable firstSelected)
        {
            mainPanel.SetActive(panel == mainPanel);
            selectPanel.SetActive(panel == selectPanel);
            optionsPanel.gameObject.SetActive(panel == optionsPanel.gameObject);
            creditsPanel.SetActive(panel == creditsPanel);
            // Something must be selected, or the keyboard and gamepad can't move through the buttons.
            if (EventSystem.current != null && firstSelected != null)
                EventSystem.current.SetSelectedGameObject(firstSelected.gameObject);
        }

        static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
