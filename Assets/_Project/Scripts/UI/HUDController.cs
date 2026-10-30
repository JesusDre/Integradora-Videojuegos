using Animas.Core;
using Animas.Items;
using Animas.Player;
using TMPro;
using UnityEngine;

namespace Animas.UI
{
    // The always-visible HUD (GDD §16): life and stamina bottom left, the 4 quick slots bottom center,
    // the weapon in hand and, with the shotgun, its ammo (loaded | in the inventory) bottom right.
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
            if (weapons != null) weapons.Switched += OnWeaponSwitched;
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
            OnInventoryChanged();
            if (weapons != null) OnWeaponSwitched(weapons.Current);
            if (prayerBar != null) prayerBar.gameObject.SetActive(false);
        }

        void OnDisable()
        {
            if (playerHealth != null) playerHealth.Changed -= OnHealthChanged;
            if (playerStamina != null) playerStamina.Changed -= OnStaminaChanged;
            if (playerInventory != null) playerInventory.Changed -= OnInventoryChanged;
            if (weapons != null) weapons.Switched -= OnWeaponSwitched;
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
        }

        void OnWeaponSwitched(WeaponSwitcher.Slot slot)
        {
            bool gun = slot == WeaponSwitcher.Slot.Shotgun;
            weaponLabel.text = gun ? shotgun.Weapon.displayName : "machete";
            ammoLabel.gameObject.SetActive(gun);
            if (gun) OnAmmoChanged(shotgun.Loaded, shotgun.Reserve);
        }

        void OnAmmoChanged(int loaded, int reserve)
        {
            // Big number: shells in the gun. Small: balas de sal left in the inventory.
            ammoLabel.text = $"{loaded} <size=70%>| {reserve}</size>";
        }
    }
}
