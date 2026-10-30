using Animas.Items;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Animas.Player
{
    // Keys 1–4 use the item in the first four inventory spaces (the quick slots).
    public class QuickSlotInput : MonoBehaviour
    {
        [SerializeField] Inventory inventory;

        readonly InputAction[] slotActions = new InputAction[Inventory.QuickSlotCount];

        void Awake()
        {
            if (inventory == null) inventory = GetComponent<Inventory>();
            for (int i = 0; i < slotActions.Length; i++)
                slotActions[i] = InputSystem.actions.FindAction($"Player/QuickSlot{i + 1}", true);
        }

        void Update()
        {
            for (int i = 0; i < slotActions.Length; i++)
                if (slotActions[i].WasPressedThisFrame()) inventory.UseSlot(i);
        }
    }
}
