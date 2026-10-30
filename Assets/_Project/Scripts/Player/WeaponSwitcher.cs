using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Animas.Player
{
    // Q or the mouse wheel switch between the melee weapon and the shotgun (GDD §14).
    // Only the weapon in hand is enabled, so left click does one thing at a time.
    public class WeaponSwitcher : MonoBehaviour
    {
        public enum Slot { Melee, Shotgun }

        [SerializeField] PlayerCombat melee;
        [SerializeField] PlayerShotgun shotgun;
        [Tooltip("Models shown in the Padre's hands for each weapon (may be empty until #28).")]
        [SerializeField] GameObject meleeModel;
        [SerializeField] GameObject shotgunModel;

        public Slot Current { get; private set; } = Slot.Melee;
        public event Action<Slot> Switched;

        InputAction switchAction, scrollAction;
        bool wasScrolling;

        void Awake()
        {
            switchAction = InputSystem.actions.FindAction("Player/SwitchWeapon", true);
            scrollAction = InputSystem.actions.FindAction("Player/ScrollWeapon", true);
        }

        void Start() => Equip(Current);

        void OnDisable()
        {
            // Menus and death turn this off; the weapon in hand must stop too, or a click on a
            // menu button would also fire.
            melee.enabled = false;
            shotgun.enabled = false;
        }

        void OnEnable()
        {
            if (didStart) Equip(Current);
        }

        void Update()
        {
            // One wheel notch spans several frames: switch only when the wheel starts moving.
            bool scrolling = Mathf.Abs(scrollAction.ReadValue<float>()) > 0.1f;
            bool scrolled = scrolling && !wasScrolling;
            wasScrolling = scrolling;
            if (switchAction.WasPressedThisFrame() || scrolled)
                Equip(Current == Slot.Melee ? Slot.Shotgun : Slot.Melee);
        }

        public void Equip(Slot slot)
        {
            bool changed = slot != Current;
            Current = slot;
            melee.enabled = slot == Slot.Melee;
            shotgun.enabled = slot == Slot.Shotgun;
            if (meleeModel != null) meleeModel.SetActive(slot == Slot.Melee);
            if (shotgunModel != null) shotgunModel.SetActive(slot == Slot.Shotgun);
            if (changed) Switched?.Invoke(slot);
        }
    }
}
