using System;
using Animas.Core;
using Animas.Items;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Animas.Player
{
    // Left click swings the melee weapon: costs stamina, has a cooldown and hits the first thing with
    // Health in front of the camera. The shotgun (#24) will be a second weapon switched with Q.
    [RequireComponent(typeof(PlayerStamina))]
    public class PlayerCombat : MonoBehaviour
    {
        [SerializeField] MeleeWeaponDefinition weapon;
        [Tooltip("Where the swing comes from: the camera, so it hits what the player is looking at.")]
        [SerializeField] Transform aim;
        [SerializeField] PlayerLook look;
        [SerializeField] AudioSource audioSource;
        [Tooltip("The prayer's shield makes every blow stronger (#131).")]
        [SerializeField] PlayerPrayer prayer;
        [SerializeField] LayerMask hitLayers = ~0;
        [SerializeField, Range(0f, 5f)] float swingKickDegrees = 1.5f;

        public MeleeWeaponDefinition Weapon => weapon;

        public event Action Swung;
        public event Action<Health, float> HitLanded;
        // Cruz de plata: a charged hit landed (the Charro Negro, #39, only counts these).
        public event Action<Health> ChargedHitLanded;
        // 0..1 while holding the click with a chargeable weapon; -1 when not charging (HUD bar).
        public event Action<float> ChargeProgress;

        PlayerStamina stamina;
        Health ownHealth;
        InputAction attackAction;
        float nextAttackTime;
        float chargeStarted = -1f;
        readonly RaycastHit[] hits = new RaycastHit[16];  // reused every swing, so no garbage

        void Awake()
        {
            stamina = GetComponent<PlayerStamina>();
            ownHealth = GetComponent<Health>();
            attackAction = InputSystem.actions.FindAction("Player/Attack", true);
        }

        void Update()
        {
            if (weapon == null) return;
            if (weapon.chargeSeconds <= 0f)
            {
                if (attackAction.WasPressedThisFrame()) TryAttack();
                return;
            }
            // Chargeable weapon: hold the click; letting go swings, charged if held long enough.
            if (attackAction.WasPressedThisFrame()) chargeStarted = Time.time;
            if (chargeStarted < 0f) return;
            float progress = (Time.time - chargeStarted) / weapon.chargeSeconds;
            if (attackAction.IsPressed())
            {
                ChargeProgress?.Invoke(Mathf.Clamp01(progress));
                return;
            }
            chargeStarted = -1f;
            ChargeProgress?.Invoke(-1f);
            TryAttack(progress >= 1f);
        }

        void OnDisable()
        {
            if (chargeStarted >= 0f) ChargeProgress?.Invoke(-1f);
            chargeStarted = -1f;
        }

        // WeaponSwitcher sets the melee weapon in hand: a carried one (machete, martillo) or the chancla/cruz.
        public void SetWeapon(MeleeWeaponDefinition newWeapon)
        {
            if (newWeapon == null) return;
            weapon = newWeapon;
            chargeStarted = -1f;
        }

        public bool TryAttack(bool charged = false)
        {
            if (Time.time < nextAttackTime) return false;
            // Exhausted: no swing at all (GDD: at 0 stamina the Padre can't run or attack until 25).
            if (!stamina.TrySpend(weapon.staminaCost)) return false;

            nextAttackTime = Time.time + weapon.cooldown;
            Play(weapon.swingClip);
            if (look != null) look.AddKick(charged ? swingKickDegrees * 2.5f : swingKickDegrees);
            Swung?.Invoke();

            // A thick ray (sphere cast) is more forgiving than a thin one: grazing a cultist still counts.
            // The nearest thing it touches is what the machete hits, skipping the Padre's own body (aiming
            // down at a crawling enemy, the cast starts inside his collider).
            int count = Physics.SphereCastNonAlloc(aim.position, weapon.hitRadius, aim.forward, hits,
                weapon.range, hitLayers, QueryTriggerInteraction.Ignore);
            Collider nearest = null;
            float nearestDistance = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                if (hits[i].collider.transform.IsChildOf(transform)) continue;
                if (hits[i].distance < nearestDistance)
                {
                    nearest = hits[i].collider;
                    nearestDistance = hits[i].distance;
                }
            }

            Health target = nearest != null ? nearest.GetComponentInParent<Health>() : null;
            if (target != null && target != ownHealth && !target.IsDead)
            {
                float damage = target.Kind == HealthKind.Spirit
                    ? (charged ? weapon.chargedDamageToSpirits : weapon.damageToSpirits)
                    : (charged ? weapon.chargedDamageToHumans : weapon.damageToHumans);
                if (prayer != null) damage *= prayer.DamageMultiplier;   // the prayer's shield (#131)
                target.TakeDamage(damage);
                Play(weapon.hitClip);
                HitLanded?.Invoke(target, damage);
                if (charged) ChargedHitLanded?.Invoke(target);
            }
            return true;
        }

        void Play(AudioClip clip)
        {
            if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
        }
    }
}
