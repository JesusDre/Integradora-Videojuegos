using System.Text;
using Animas.Core;
using Animas.Items;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Animas.UI
{
    // The inventory screen, opened and closed with Tab (GDD §16-17). Shows the 8 spaces, the selected
    // item's description, the materials, and lets the player use or drop the selected item.
    // The game keeps running while it is open (it isn't the pause), so opening it in a fight is risky.
    public class InventoryUI : MonoBehaviour
    {
        [SerializeField] Inventory inventory;
        [SerializeField] Health playerHealth;
        [Tooltip("Optional: lists the key items under the materials.")]
        [SerializeField] KeyItemBag keyItems;
        [Tooltip("Where dropped items fall: the player's camera, so they land in front of him.")]
        [SerializeField] Transform dropPoint;
        [Tooltip("Turned off while the screen is open: camera, attack, interaction, quick slot keys.")]
        [SerializeField] Behaviour[] disableWhileOpen;

        [SerializeField] GameObject panel;
        [SerializeField] ItemSlotView[] slots = new ItemSlotView[Inventory.SlotCount];
        [SerializeField] TMP_Text itemName;
        [SerializeField] TMP_Text itemDescription;
        [SerializeField] TMP_Text materialsLabel;
        [SerializeField] Button useButton;
        [SerializeField] Button dropButton;

        public bool IsOpen => panel.activeSelf;

        InputAction toggleAction;
        int selected;
        readonly StringBuilder text = new StringBuilder();

        void Awake()
        {
            toggleAction = InputSystem.actions.FindAction("Player/Inventory", true);
            for (int i = 0; i < slots.Length; i++)
            {
                int index = i;  // a copy for the lambda; using i would make every button select the last slot
                slots[i].GetComponent<Button>().onClick.AddListener(() => Select(index));
                slots[i].SetKey(i < Inventory.QuickSlotCount ? (i + 1).ToString() : "");
            }
            useButton.onClick.AddListener(UseSelected);
            dropButton.onClick.AddListener(DropSelected);
            panel.SetActive(false);
        }

        void OnEnable()
        {
            if (inventory == null || playerHealth == null)
            {
                Debug.LogError($"[InventoryUI] '{name}' is missing the player's Inventory or Health.", this);
                enabled = false;
                return;
            }
            inventory.Changed += Refresh;
            playerHealth.Died += OnPlayerDied;
        }

        void OnDisable()
        {
            if (inventory != null) inventory.Changed -= Refresh;
            if (playerHealth != null) playerHealth.Died -= OnPlayerDied;
        }

        void Update()
        {
            if (toggleAction.WasPressedThisFrame() && !playerHealth.IsDead) SetOpen(!IsOpen);
        }

        public void SetOpen(bool open)
        {
            panel.SetActive(open);
            foreach (Behaviour b in disableWhileOpen) b.enabled = !open;
            // PlayerLook frees the cursor when disabled and locks it again when enabled.
            if (open) Refresh();
        }

        void OnPlayerDied()
        {
            // Close without turning the player's controls back on: the death handler just switched them off.
            panel.SetActive(false);
        }

        void Select(int index)
        {
            selected = index;
            Refresh();
        }

        void UseSelected() => inventory.UseSlot(selected);

        void DropSelected() => inventory.Drop(selected, dropPoint);

        void Refresh()
        {
            if (!IsOpen) return;
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].Show(inventory.Slots[i]);
                slots[i].SetSelected(i == selected);
            }

            InventorySlot slot = inventory.Slots[selected];
            itemName.text = slot.IsEmpty ? "Espacio vacío" : slot.item.displayName;
            itemDescription.text = slot.IsEmpty ? "" : slot.item.description;
            useButton.interactable = !slot.IsEmpty;
            dropButton.interactable = !slot.IsEmpty;

            text.Clear();
            foreach (var pair in inventory.Materials)
                if (pair.Value > 0) text.Append($"{pair.Key.displayName} {pair.Value}   ");
            materialsLabel.text = "Materiales: " + (text.Length == 0 ? "ninguno" : text.ToString());

            if (keyItems != null && keyItems.KeyItems.Count > 0)
            {
                text.Clear();
                foreach (var pair in keyItems.KeyItems) text.Append($"{pair.Key.displayName} ×{pair.Value}   ");
                materialsLabel.text += "\nObjetos clave: " + text;
            }
        }
    }
}
