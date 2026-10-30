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
            if (Physics.SphereCast(aim.position, weapon.hitRadius, aim.forward, out RaycastHit hit,
                    weapon.range, hitLayers, QueryTriggerInteraction.Ignore))
            {
                Health target = hit.collider.GetComponentInParent<Health>();
                if (target != null && target != ownHealth && !target.IsDead)
                {
                    float damage = target.Kind == HealthKind.Spirit ? weapon.damageToSpirits : weapon.damageToHumans;
                    target.TakeDamage(damage);
                    Play(weapon.hitClip);
                    HitLanded?.Invoke(target, damage);
                }
            }
            return true;
        }

        void Play(AudioClip clip)
        {
            if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
        }
    }
}
