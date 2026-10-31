using Animas.Items;
using UnityEngine;

namespace Animas.Player
{
    // The Padre's arms in first person (issue #28). They only play animations: they listen to what the
    // other systems already announce (a swing, the prayer, a throw, F on something) and never decide
    // anything themselves. They also hold the melee weapon in the right hand, and hide while the shotgun
    // is out (the shotgun has its own view model).
    public class FirstPersonArms : MonoBehaviour
    {
        [SerializeField] Animator animator;
        [Tooltip("Bone at the right palm where the held weapon goes (\"RightGrip\").")]
        [SerializeField] Transform grip;
        [Tooltip("The arms model; hidden while the shotgun is in the hands.")]
        [SerializeField] GameObject visuals;

        [Header("Padre")]
        [SerializeField] PlayerCombat combat;
        [SerializeField] PlayerPrayer prayer;
        [SerializeField] WeaponSwitcher weapons;
        [SerializeField] ItemUser itemUser;
        [SerializeField] Interactor interactor;

        static readonly int AttackTrigger = Animator.StringToHash("Attack");
        static readonly int ThrowTrigger = Animator.StringToHash("Throw");
        static readonly int PickUpTrigger = Animator.StringToHash("PickUp");
        static readonly int PrayingBool = Animator.StringToHash("Praying");

        Animas.Levels.RitualController ritual;
        MeleeWeaponDefinition shownWeapon;
        GameObject heldModel;
        bool prayingNormally;

        void OnEnable()
        {
            combat.Swung += OnSwung;
            prayer.ProgressChanged += OnPrayerProgress;
            weapons.Switched += OnSwitched;
            itemUser.Threw += OnThrew;
            itemUser.Placed += OnReach;
            interactor.Interacted += OnInteracted;
        }

        void OnDisable()
        {
            combat.Swung -= OnSwung;
            prayer.ProgressChanged -= OnPrayerProgress;
            weapons.Switched -= OnSwitched;
            itemUser.Threw -= OnThrew;
            itemUser.Placed -= OnReach;
            interactor.Interacted -= OnInteracted;
        }

        void Start()
        {
            OnSwitched();
            ritual = FindAnyObjectByType<Animas.Levels.RitualController>();
        }

        void Update()
        {
            // The weapon can change without an event (the chancla or the cross equipped from the inventory):
            // a reference comparison per frame is cheap.
            if (combat.Weapon != shownWeapon) ShowWeapon(combat.Weapon);

            // Hands together while praying: the normal prayer (it reports its progress) or the ritual,
            // which fills while the Padre stands inside its circle (#131).
            bool ritualPraying = ritual != null && ritual.Filling;
            animator.SetBool(PrayingBool, prayingNormally || ritualPraying);
        }

        void OnSwung() => animator.SetTrigger(AttackTrigger);
        void OnThrew() => animator.SetTrigger(ThrowTrigger);
        void OnReach() => animator.SetTrigger(PickUpTrigger);
        void OnInteracted(IInteractable used) => animator.SetTrigger(PickUpTrigger);

        // PlayerPrayer sends -1 when the prayer stops (released, cancelled or complete).
        void OnPrayerProgress(float progress) => prayingNormally = progress >= 0f;

        void OnSwitched() => visuals.SetActive(!weapons.IsGun);

        void ShowWeapon(MeleeWeaponDefinition weapon)
        {
            shownWeapon = weapon;
            if (heldModel != null) Destroy(heldModel);
            if (weapon == null || weapon.viewModel == null) return;
            heldModel = Instantiate(weapon.viewModel, grip);
            // The arms draw over the world but they are part of it; the weapon must not cast a big
            // shadow on the floor in front of the Padre.
            foreach (Renderer r in heldModel.GetComponentsInChildren<Renderer>())
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }
}
