using Animas.Core;
using Animas.Items;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Animas.UI
{
    // Shows a note on screen when it's read. The game doesn't pause (like the inventory); F, Tab or Esc
    // closes it.
    public class NoteUI : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        [SerializeField] TMP_Text title;
        [SerializeField] TMP_Text body;
        [SerializeField] Health playerHealth;
        [Tooltip("Turned off while reading: camera, attack, interaction, menus.")]
        [SerializeField] Behaviour[] disableWhileOpen;

        public bool IsOpen => panel.activeSelf;

        InputAction interactAction, inventoryAction, pauseAction;
        int openedFrame;

        void Awake()
        {
            interactAction = InputSystem.actions.FindAction("Player/Interact", true);
            inventoryAction = InputSystem.actions.FindAction("Player/Inventory", true);
            pauseAction = InputSystem.actions.FindAction("Player/Pause", true);
            panel.SetActive(false);
        }

        void OnEnable()
        {
            NotePickup.Opened += Open;
            if (playerHealth != null) playerHealth.Died += Hide;
        }

        void OnDisable()
        {
            NotePickup.Opened -= Open;
            if (playerHealth != null) playerHealth.Died -= Hide;
        }

        void Update()
        {
            // Not on the frame it opened: the F that opened it would close it at once.
            if (!IsOpen || Time.frameCount == openedFrame) return;
            if (interactAction.WasPressedThisFrame() || inventoryAction.WasPressedThisFrame() || pauseAction.WasPressedThisFrame())
                Close();
        }

        void Open(NoteDefinition note)
        {
            title.text = note.title;
            body.text = note.text;
            openedFrame = Time.frameCount;
            panel.SetActive(true);
            foreach (Behaviour b in disableWhileOpen) b.enabled = false;
        }

        public void Close()
        {
            if (!IsOpen) return;
            panel.SetActive(false);
            foreach (Behaviour b in disableWhileOpen) b.enabled = true;
        }

        void Hide() => panel.SetActive(false);
    }
}
