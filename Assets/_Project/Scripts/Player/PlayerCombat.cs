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
        [SerializeField] LayerMask hitLayers = ~0;
        [SerializeField, Range(0f, 5f)] float swingKickDegrees = 1.5f;

        public MeleeWeaponDefinition Weapon => weapon;

        public event Action Swung;
        public event Action<Health, float> HitLanded;

        PlayerStamina stamina;
        Health ownHealth;
        InputAction attackAction;
        float nextAttackTime;
        readonly RaycastHit[] hits = new RaycastHit[16];  // reused every swing, so no garbage

        void Awake()
        {
            stamina = GetComponent<PlayerStamina>();
            ownHealth = GetComponent<Health>();
            attackAction = InputSystem.actions.FindAction("Player/Attack", true);
        }

        void Update()
        {
            if (attackAction.WasPressedThisFrame()) TryAttack();
        }

        public bool TryAttack()
        {
            if (Time.time < nextAttackTime) return false;
            // Exhausted: no swing at all (GDD: at 0 stamina the Padre can't run or attack until 25).
            if (!stamina.TrySpend(weapon.staminaCost)) return false;

            nextAttackTime = Time.time + weapon.cooldown;
            Play(weapon.swingClip);
            if (look != null) look.AddKick(swingKickDegrees);
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
                float damage = target.Kind == HealthKind.Spirit ? weapon.damageToSpirits : weapon.damageToHumans;
                target.TakeDamage(damage);
                Play(weapon.hitClip);
                HitLanded?.Invoke(target, damage);
            }
            return true;
        }

        void Play(AudioClip clip)
        {
            if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
        }
    }
}
