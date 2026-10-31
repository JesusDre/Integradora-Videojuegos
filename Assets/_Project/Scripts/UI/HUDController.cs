using Animas.Core;
using Animas.Items;
using Animas.Player;
using TMPro;
using UnityEngine;

namespace Animas.UI
{
    // The always-visible HUD (GDD §16): life and stamina bottom left, the 2 weapon spaces and the 4 quick
    // slots bottom center, the weapon in hand and, with a gun, its ammo (loaded | carried) bottom right.
    // Each gun's space also shows the rounds carried for it (#152).
    // Everything updates from events (Health.Changed, PlayerStamina.Changed, Inventory.Changed), never
    // by checking every frame.
    public class HUDController : MonoBehaviour
    {
        [SerializeField] Health playerHealth;
        [SerializeField] PlayerStamina playerStamina;
        [SerializeField] Inventory playerInventory;
        [SerializeField] StatBar healthBar;
        [SerializeField] StatBar staminaBar;
        [SerializeField] ItemSlotView[] quickSlots = new ItemSlotView[Inventory.QuickSlotCount];

        [Header("Weapon")]
        [SerializeField] WeaponSwitcher weapons;
        [SerializeField] PlayerShotgun shotgun;
        [SerializeField] TMP_Text weaponLabel;
        [SerializeField] TMP_Text ammoLabel;
        [SerializeField] WeaponBag weaponBag;
        [Tooltip("The 2 weapon spaces, left of the quick slots (#152).")]
        [SerializeField] ItemSlotView[] weaponSlots = new ItemSlotView[WeaponBag.MaxWeapons];

        [Header("Prayer")]
        [SerializeField] PlayerPrayer prayer;
        [Tooltip("Fills while R is held, under the crosshair.")]
        [SerializeField] StatBar prayerBar;
        [Tooltip("\"Rezo listo\" or the seconds left, under the stamina bar.")]
        [SerializeField] TMP_Text prayerStatus;
        [Tooltip("The cruz de plata's charge reuses the prayer bar.")]
        [SerializeField] PlayerCombat combat;

        [Header("Colors")]
        [SerializeField] Color healthColor = new Color(0.70f, 0.15f, 0.12f);
        [Tooltip("Below this life the bar pulses, so the player notices before dying.")]
        [SerializeField, Range(0f, 100f)] float lowHealth = 30f;
        [SerializeField] Color lowHealthColor = new Color(1f, 0.27f, 0.21f);
        [SerializeField] Color staminaColor = new Color(0.79f, 0.64f, 0.15f);
        [Tooltip("Stamina while exhausted: the Padre can't run or attack until it refills to 25.")]
        [SerializeField] Color exhaustedColor = new Color(0.45f, 0.42f, 0.38f);

        void OnEnable()
        {
            // The prefab can't point to the scene's player; each scene must wire it.
            if (playerHealth == null || playerStamina == null || playerInventory == null)
            {
                Debug.LogError($"[HUDController] '{name}' is missing the player's Health, Stamina or Inventory.", this);
                enabled = false;
                return;
            }
            playerHealth.Changed += OnHealthChanged;
            playerStamina.Changed += OnStaminaChanged;
            playerInventory.Changed += OnInventoryChanged;
            // The bag lives on the Padre next to the switcher, so the scenes don't need to wire it too.
            if (weaponBag == null && weapons != null) weaponBag = weapons.GetComponentInParent<WeaponBag>();
            if (weapons != null) weapons.Switched += OnWeaponSwitched;
            if (weaponBag != null) weaponBag.Changed += ShowWeaponSlots;
            if (shotgun != null) shotgun.AmmoChanged += OnAmmoChanged;
            if (prayer != null) prayer.ProgressChanged += OnPrayerProgress;
            if (combat != null) combat.ChargeProgress += OnChargeProgress;
        }

        void Start()
        {
            // Draw once with the starting values; from here on only events redraw.
            OnHealthChanged(playerHealth.Current, playerHealth.Max);
            OnStaminaChanged(playerStamina.Current, playerStamina.Max);
            for (int i = 0; i < quickSlots.Length; i++) quickSlots[i].SetKey((i + 1).ToString());
            foreach (ItemSlotView slot in weaponSlots) if (slot != null) slot.SetKey("Q");
            OnInventoryChanged();
            if (weapons != null) OnWeaponSwitched();
            if (prayerBar != null) prayerBar.gameObject.SetActive(false);
        }

        void OnDisable()
        {
            if (playerHealth != null) playerHealth.Changed -= OnHealthChanged;
            if (playerStamina != null) playerStamina.Changed -= OnStaminaChanged;
            if (playerInventory != null) playerInventory.Changed -= OnInventoryChanged;
            if (weapons != null) weapons.Switched -= OnWeaponSwitched;
            if (weaponBag != null) weaponBag.Changed -= ShowWeaponSlots;
            if (shotgun != null) shotgun.AmmoChanged -= OnAmmoChanged;
            if (prayer != null) prayer.ProgressChanged -= OnPrayerProgress;
            if (combat != null) combat.ChargeProgress -= OnChargeProgress;
        }

        void Update()
        {
            // The only per-frame work: the low-life pulse, an animation rather than a state check.
            if (playerHealth.Current > 0f && playerHealth.Current <= lowHealth)
                healthBar.SetColor(Color.Lerp(healthColor, lowHealthColor, Mathf.PingPong(Time.time * 2f, 1f)));

            // The prayer cooldown is a countdown, so it is redrawn while it runs.
            if (prayer != null && prayerStatus != null)
                prayerStatus.text = prayer.IsReady ? "Rezo listo [R]" : $"Rezo en {Mathf.CeilToInt(prayer.CooldownLeft)} s";
        }

        void OnChargeProgress(float progress) => ShowBar(progress, progress >= 1f ? "¡Cruz cargada!" : "Cargando la cruz…");

        void OnPrayerProgress(float progress) => ShowBar(progress, "Rezando…");

        void ShowBar(float progress, string text)
        {
            var label = prayerBar.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = text;
            // -1 means the Padre stopped praying (finished or let go of R).
            prayerBar.gameObject.SetActive(progress >= 0f);
            if (progress >= 0f) prayerBar.Set(progress, 1f);
        }

        void OnHealthChanged(float current, float max)
        {
            healthBar.Set(current, max);
            if (current > lowHealth) healthBar.SetColor(healthColor);
        }

        void OnStaminaChanged(float current, float max)
        {
            staminaBar.Set(current, max);
            staminaBar.SetColor(playerStamina.IsExhausted ? exhaustedColor : staminaColor);
        }

        void OnInventoryChanged()
        {
            for (int i = 0; i < quickSlots.Length; i++) quickSlots[i].Show(playerInventory.Slots[i]);
            ShowWeaponSlots();   // picked-up ammo changes the count on the gun's space
        }

        // The carried weapons, the one in hand highlighted; a gun shows the rounds carried for it.
        void ShowWeaponSlots()
        {
            if (weaponBag == null) return;
            for (int i = 0; i < weaponSlots.Length; i++)
            {
                if (weaponSlots[i] == null) continue;
                ItemDefinition w = i < weaponBag.Count ? weaponBag.Weapons[i] : null;
                string rounds = "";
                if (w != null && w.rangedWeapon != null)
                    rounds = (Count(w.rangedWeapon.ammoItem) + Count(w.rangedWeapon.normalAmmoItem)).ToString();
                weaponSlots[i].ShowWeapon(w, rounds);
                weaponSlots[i].SetSelected(w != null && weapons != null && w == weapons.Current && !weapons.IsOverriding);
            }
        }

        int Count(ItemDefinition item) => item == null ? 0 : playerInventory.CountOf(item);

        void OnWeaponSwitched()
        {
            bool gun = weapons.IsGun;
            weaponLabel.text = weapons.CurrentName;
            ShowWeaponSlots();
            ammoLabel.gameObject.SetActive(gun);
            if (gun) OnAmmoChanged(shotgun.Loaded, shotgun.Reserve);
        }

        void OnAmmoChanged(int loaded, int reserve)
        {
            // Big number: rounds in the gun. Small: rounds left in the inventory. Below, which kind is
            // loaded: salt (hurts spirits) or normal (#134).
            string kind = loaded == 0 ? "" : shotgun.LoadedSalt ? "\n<size=55%>sal</size>" : "\n<size=55%>normal</size>";
            ammoLabel.text = $"{loaded} <size=70%>| {reserve}</size>{kind}";
        }
    }
}
