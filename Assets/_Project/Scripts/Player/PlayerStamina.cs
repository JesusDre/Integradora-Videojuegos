using System;
using UnityEngine;

namespace Animas.Player
{
    // Stamina for running (and, from issue #13, for melee hits).
    // Kept separate from movement so the HUD and combat can use it without depending on PlayerController.
    public class PlayerStamina : MonoBehaviour
    {
        [SerializeField] PlayerTuning tuning;

        public float Current { get; private set; }
        public float Max => tuning.maxStamina;

        // True after stamina hits 0, until it recovers to the threshold. Stops the player from
        // running again with a sliver of stamina, which would feel like stuttering.
        public bool IsExhausted { get; private set; }

        public bool CanSpend => !IsExhausted && Current > 0f;

        // (current, max): the HUD listens to this instead of reading the value every frame.
        public event Action<float, float> Changed;

        float lastSpendTime = float.NegativeInfinity;

        void Awake()
        {
            Current = Max;
        }

        public void Spend(float amount)
        {
            if (amount <= 0f) return;

            SetCurrent(Current - amount);
            lastSpendTime = Time.time;
            if (Current <= 0f) IsExhausted = true;
        }

        // For one-off costs like a machete hit: refuses instead of going negative when exhausted.
        public bool TrySpend(float amount)
        {
            if (!CanSpend) return false;
            Spend(amount);
            return true;
        }

        void Update()
        {
            if (Time.time - lastSpendTime < tuning.regenDelay) return;
            if (Current >= Max) return;

            SetCurrent(Current + tuning.regenPerSecond * Time.deltaTime);
            if (IsExhausted && Current >= tuning.exhaustedRecoverThreshold) IsExhausted = false;
        }

        void SetCurrent(float value)
        {
            float clamped = Mathf.Clamp(value, 0f, Max);
            if (Mathf.Approximately(clamped, Current)) return;

            Current = clamped;
            Changed?.Invoke(Current, Max);
        }
    }
}
