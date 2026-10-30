using System.Text;
using Animas.Core;
using Animas.Items;
using Animas.Levels;
using Animas.Player;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Animas.UI
{
    // The altar menu (GDD §16): the list of recipes, what each needs (have / need) and what it makes.
    // Unknown recipes appear as "Receta desconocida" until their note is read (#32).
    // Opens when any Altar is used; closes with F, Tab, Esc or the Cerrar button.
    public class AltarUI : MonoBehaviour
    {
        [SerializeField] Inventory inventory;
        [SerializeField] RecipeBook recipeBook;
        [SerializeField] Health playerHealth;
        [Tooltip("Turned off while the menu is open: camera, attack, interaction, inventory screen.")]
        [SerializeField] Behaviour[] disableWhileOpen;
        [SerializeField] RecipeDefinition[] recipes;

        [SerializeField] GameObject panel;
        [SerializeField] Button[] recipeButtons;
        [SerializeField] TMP_Text resultName;
        [SerializeField] TMP_Text resultDescription;
        [SerializeField] TMP_Text ingredientsLabel;
        [SerializeField] TMP_Text materialsLabel;
        [SerializeField] Button craftButton;
        [SerializeField] Button closeButton;

        [SerializeField] Color knownColor = new Color(0.91f, 0.88f, 0.83f);
        [SerializeField] Color missingColor = new Color(0.91f, 0.88f, 0.83f, 0.45f);
        [SerializeField] Color selectedColor = new Color(0.79f, 0.64f, 0.15f);

        public bool IsOpen => panel.activeSelf;

        InputAction interactAction, inventoryAction, pauseAction;
        Altar currentAltar;
        int selected;
        int openedFrame;
        readonly StringBuilder text = new StringBuilder();

        void Awake()
        {
            interactAction = InputSystem.actions.FindAction("Player/Interact", true);
            inventoryAction = InputSystem.actions.FindAction("Player/Inventory", true);
            pauseAction = InputSystem.actions.FindAction("Player/Pause", true);
            for (int i = 0; i < recipeButtons.Length; i++)
            {
                int index = i;  // a copy for the lambda, so each button selects its own recipe
                recipeButtons[i].onClick.AddListener(() => Select(index));
                recipeButtons[i].gameObject.SetActive(i < recipes.Length);
            }
            craftButton.onClick.AddListener(CraftSelected);
            closeButton.onClick.AddListener(Close);
            panel.SetActive(false);
        }

        void OnEnable()
        {
            if (inventory == null || recipeBook == null || playerHealth == null)
            {
                Debug.LogError($"[AltarUI] '{name}' is missing the player's Inventory, RecipeBook or Health.", this);
                enabled = false;
                return;
            }
            Altar.Opened += Open;
            inventory.Changed += Refresh;
            recipeBook.Unlocked += OnUnlocked;
            playerHealth.Died += OnPlayerDied;
        }

        void OnDisable()
        {
            Altar.Opened -= Open;
            if (inventory != null) inventory.Changed -= Refresh;
            if (recipeBook != null) recipeBook.Unlocked -= OnUnlocked;
            if (playerHealth != null) playerHealth.Died -= OnPlayerDied;
        }

        void Update()
        {
            // Not on the frame it opened: the same F press that opened it would close it again.
            if (!IsOpen || Time.frameCount == openedFrame) return;
            if (interactAction.WasPressedThisFrame() || inventoryAction.WasPressedThisFrame() || pauseAction.WasPressedThisFrame())
                Close();
        }

        void Open(Altar altar, Interactor interactor)
        {
            if (playerHealth.IsDead) return;
            currentAltar = altar;
            openedFrame = Time.frameCount;
            panel.SetActive(true);
            foreach (Behaviour b in disableWhileOpen) b.enabled = false;
            Refresh();
        }

        public void Close()
        {
            if (!IsOpen) return;
            panel.SetActive(false);
            foreach (Behaviour b in disableWhileOpen) b.enabled = true;
        }

        void OnPlayerDied()
        {
            // Close without giving the controls back: the death handler just switched them off.
            panel.SetActive(false);
        }

        void OnUnlocked(RecipeDefinition recipe) => Refresh();

        void Select(int index)
        {
            selected = index;
            Refresh();
        }

        void CraftSelected()
        {
            RecipeDefinition recipe = recipes[selected];
            switch (AltarCrafting.TryCraft(recipe, inventory, recipeBook))
            {
                case CraftResult.Crafted:
                    if (currentAltar != null) currentAltar.Bless();
                    inventory.Notify(recipe.resultAmount > 1
                        ? $"Bendijiste: {recipe.result.displayName} ×{recipe.resultAmount}"
                        : $"Bendijiste: {recipe.result.displayName}");
                    break;
                case CraftResult.MissingIngredients: inventory.Notify("Te faltan materiales"); break;
                case CraftResult.NoSpace: inventory.Notify("Inventario lleno"); break;
                case CraftResult.Locked: inventory.Notify("Aún no conoces esta receta"); break;
            }
            Refresh();
        }

        void Refresh()
        {
            if (!IsOpen) return;
            for (int i = 0; i < recipes.Length && i < recipeButtons.Length; i++)
            {
                bool known = recipeBook.Knows(recipes[i]);
                var label = recipeButtons[i].GetComponentInChildren<TMP_Text>();
                label.text = known ? recipes[i].result.displayName : "Receta desconocida";
                label.color = i == selected ? selectedColor
                    : known && AltarCrafting.HasIngredients(recipes[i], inventory) ? knownColor : missingColor;
            }

            RecipeDefinition recipe = recipes[selected];
            bool isKnown = recipeBook.Knows(recipe);
            resultName.text = isKnown ? recipe.result.displayName : "Receta desconocida";
            resultDescription.text = isKnown ? recipe.result.description : "Encuentra la nota del culto que la explica.";

            text.Clear();
            if (isKnown)
            {
                text.AppendLine("Necesita:");
                foreach (var ingredient in recipe.ingredients)
                {
                    int have = inventory.CountOf(ingredient.item);
                    // Red when short, so the player sees at a glance what is missing.
                    string color = have >= ingredient.amount ? "#E8E1D3" : "#FF4536";
                    text.AppendLine($"<color={color}>  {ingredient.item.displayName}  {have}/{ingredient.amount}</color>");
                }
                if (recipe.resultAmount > 1) text.AppendLine($"Hace: {recipe.resultAmount}");
            }
            ingredientsLabel.text = text.ToString();
            craftButton.interactable = isKnown && AltarCrafting.HasIngredients(recipe, inventory);

            text.Clear();
            foreach (var pair in inventory.Materials)
                if (pair.Value > 0) text.Append($"{pair.Key.displayName} {pair.Value}   ");
            materialsLabel.text = "Materiales: " + (text.Length == 0 ? "ninguno" : text.ToString());
        }
    }
}
