using Animas.Core;
using Animas.Items;
using Animas.Player;
using UnityEngine;

namespace Animas.UI
{
    // The always-visible HUD (GDD §16): life and stamina bottom left, the 4 quick slots bottom center.
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
        }

        void Start()
        {
            // Draw once with the starting values; from here on only events redraw.
            OnHealthChanged(playerHealth.Current, playerHealth.Max);
            OnStaminaChanged(playerStamina.Current, playerStamina.Max);
            for (int i = 0; i < quickSlots.Length; i++) quickSlots[i].SetKey((i + 1).ToString());
            OnInventoryChanged();
        }

        void OnDisable()
        {
            if (playerHealth != null) playerHealth.Changed -= OnHealthChanged;
            if (playerStamina != null) playerStamina.Changed -= OnStaminaChanged;
            if (playerInventory != null) playerInventory.Changed -= OnInventoryChanged;
        }

        void Update()
        {
            // The only per-frame work: the low-life pulse, an animation rather than a state check.
            if (playerHealth.Current > 0f && playerHealth.Current <= lowHealth)
                healthBar.SetColor(Color.Lerp(healthColor, lowHealthColor, Mathf.PingPong(Time.time * 2f, 1f)));
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
    }
}
