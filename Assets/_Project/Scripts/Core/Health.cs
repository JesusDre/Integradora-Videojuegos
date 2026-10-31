using System;
using UnityEngine;

namespace Animas.Core
{
    // Spirits (entities) take less damage from unblessed weapons; humans (the Padre, cultists) take it all.
    public enum HealthKind { Human, Spirit }

    // Life points for anything that can be hurt: the Padre, cultists and entities share this component.
    // It never goes below 0 or above its maximum (the rubric checks limits), and it never regenerates by
    // itself: only bandages and the altar heal (GDD §5).
    public class Health : MonoBehaviour
    {
        [SerializeField, Min(1f)] float maxHealth = 100f;
        [SerializeField] HealthKind kind = HealthKind.Human;

        public float Current { get; private set; }
        public float Max => maxHealth;
        public HealthKind Kind => kind;
        public bool IsDead => Current <= 0f;
        // Whether the last hit was blessed (salt, holy water, a blessed weapon, or any weapon under the
        // prayer's shield). The entities read it in their Damaged handler: only blessed hits count (#158).
        public bool LastHitBlessed { get; private set; }

        // (current, max): the HUD bar listens to this.
        public event Action<float, float> Changed;
        // Amount actually lost: feedback (red flash, hit sound) listens to this.
        public event Action<float> Damaged;
        public event Action Died;

        void Awake()
        {
            Current = maxHealth;
        }

        public void TakeDamage(float amount, bool blessed = false)
        {
            if (amount <= 0f || IsDead) return;
            LastHitBlessed = blessed;

            float before = Current;
            Current = Mathf.Clamp(Current - amount, 0f, maxHealth);
            Damaged?.Invoke(before - Current);
            Changed?.Invoke(Current, maxHealth);
            if (IsDead) Died?.Invoke();
        }

        // Returns how much was actually healed, so a bandage isn't wasted at full health.
        public float Heal(float amount)
        {
            if (amount <= 0f || IsDead) return 0f;

            float before = Current;
            Current = Mathf.Clamp(Current + amount, 0f, maxHealth);
            if (Current > before) Changed?.Invoke(Current, maxHealth);
            return Current - before;
        }

        // Back from death (respawn at the safe room). The only way to leave 0, so a dead enemy
        // can't be healed by accident.
        public void Revive(float amount)
        {
            Current = Mathf.Clamp(amount, 1f, maxHealth);
            Changed?.Invoke(Current, maxHealth);
        }
    }
}
