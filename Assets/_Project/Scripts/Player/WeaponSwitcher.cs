using System;
using Animas.Items;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Animas.Player
{
    // Q or the mouse wheel go to the next weapon the Padre carries (WeaponBag, #134). Only the weapon in
    // hand is enabled, so left click does one thing at a time: PlayerCombat for melee weapons, PlayerShotgun
    // for every firearm. The chancla and the cruz are equipped from the inventory (ItemUser) and sit in the
    // hand until they are put away; then the Padre goes back to the weapon he had.
    public class WeaponSwitcher : MonoBehaviour
    {
        [SerializeField] PlayerCombat melee;
        [Tooltip("Handles every firearm (the name stayed from when the escopeta was the only one).")]
        [SerializeField] PlayerShotgun shotgun;
        [SerializeField] WeaponBag bag;
        [Tooltip("Root of the guns' first-person view (hidden while a melee weapon is in hand).")]
        [SerializeField] GameObject shotgunModel;

        // The carried weapon in hand (null with empty hands or while the chancla/cruz is out).
        public ItemDefinition Current { get; private set; }
        // The chancla or the cruz is in the hand, over the carried weapon.
        public bool IsOverriding => overriding;
        public bool IsGun => !overriding && Current != null && Current.rangedWeapon != null;
        public string CurrentName => overriding ? melee.Weapon.displayName : Current != null ? Current.displayName : "sin arma";
        public event Action Switched;

        InputAction switchAction, scrollAction;
        bool wasScrolling;
        bool overriding;

        void Awake()
        {
            switchAction = InputSystem.actions.FindAction("Player/SwitchWeapon", true);
            scrollAction = InputSystem.actions.FindAction("Player/ScrollWeapon", true);
            bag.Added += OnWeaponAdded;
        }

        void OnDestroy() => bag.Added -= OnWeaponAdded;

        void Start() => Apply();

        void OnDisable()
        {
            // Menus and death turn this off; the weapon in hand must stop too, or a click on a
            // menu button would also fire.
            melee.enabled = false;
            shotgun.enabled = false;
        }

        void OnEnable()
        {
            if (didStart) Apply();
        }

        void Update()
        {
            // One wheel notch spans several frames: switch only when the wheel starts moving.
            bool scrolling = Mathf.Abs(scrollAction.ReadValue<float>()) > 0.1f;
            bool scrolled = scrolling && !wasScrolling;
            wasScrolling = scrolling;
            if ((switchAction.WasPressedThisFrame() || scrolled) && bag.Count > 0) Next();
        }

        void Next()
        {
            int i = Current == null ? -1 : IndexOf(Current);
            Equip(bag.Weapons[(i + 1) % bag.Count]);
        }

        public void Equip(ItemDefinition weapon)
        {
            if (weapon != null && !bag.Has(weapon)) return;
            overriding = false;
            Current = weapon;
            Apply();
            Switched?.Invoke();
        }

        // The chancla or the cruz goes into the hand (ItemUser), over whatever was there.
        public void EquipOverride(MeleeWeaponDefinition weapon)
        {
            overriding = true;
            melee.SetWeapon(weapon);
            Apply();
            Switched?.Invoke();
        }

        // Put the chancla/cruz away: back to the carried weapon.
        public void ClearOverride()
        {
            if (!overriding) return;
            overriding = false;
            Apply();
            Switched?.Invoke();
        }

        // A new weapon is taken in hand right away; the very first one too.
        void OnWeaponAdded(ItemDefinition weapon) => Equip(weapon);

        void Apply()
        {
            if (!enabled) return;
            bool gun = IsGun;
            bool meleeInHand = overriding || (Current != null && Current.meleeWeapon != null);
            if (!overriding && Current != null && Current.meleeWeapon != null) melee.SetWeapon(Current.meleeWeapon);
            if (gun) shotgun.SetWeapon(Current.rangedWeapon);
            melee.enabled = meleeInHand;
            shotgun.enabled = gun;
            if (shotgunModel != null) shotgunModel.SetActive(gun);
        }

        int IndexOf(ItemDefinition weapon)
        {
            for (int i = 0; i < bag.Count; i++)
                if (bag.Weapons[i] == weapon) return i;
            return -1;
        }
    }
}
