using System;
using System.Collections.Generic;
using Animas.Core;
using Animas.Enemies;
using Animas.Items;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Animas.Player
{
    // The shotgun: left click fires, right click aims (light zoom). It holds 2 shells; when both are
    // spent it reloads by itself (2 s) from the "balas de sal" in the inventory. The shot is loud:
    // every cultist within 20 m hears it and comes to look (CultistAI.HearNoise).
    // Enabled only while the shotgun is the weapon in hand (WeaponSwitcher).
    public class PlayerShotgun : MonoBehaviour
    {
        [SerializeField] RangedWeaponDefinition weapon;
        [SerializeField] Inventory inventory;
        [Tooltip("Where the shot comes from: the camera, so it hits what the player is looking at.")]
        [SerializeField] Transform aim;
        [SerializeField] Camera viewCamera;
        [SerializeField] PlayerLook look;
        [SerializeField] AudioSource audioSource;
        [Tooltip("Short flash at the barrel when firing.")]
        [SerializeField] Light muzzleFlash;
        [SerializeField] LayerMask hitLayers = ~0;

        public RangedWeaponDefinition Weapon => weapon;
        public int Loaded { get; private set; }
        public int Reserve => inventory.CountOf(weapon.ammoItem);
        public bool IsReloading => reloadEndsAt > 0f;

        // (loaded, reserve): the HUD shows "2 | 10".
        public event Action<int, int> AmmoChanged;
        public event Action Fired;

        InputAction fireAction, aimAction;
        Health ownHealth;
        float nextFireTime;
        float reloadEndsAt = -1f;
        float normalFieldOfView;
        float flashUntil;
        readonly RaycastHit[] hits = new RaycastHit[16];
        readonly Collider[] heard = new Collider[32];

        void Awake()
        {
            fireAction = InputSystem.actions.FindAction("Player/Attack", true);
            aimAction = InputSystem.actions.FindAction("Player/Aim", true);
            ownHealth = GetComponent<Health>();
            normalFieldOfView = viewCamera.fieldOfView;
            if (muzzleFlash != null) muzzleFlash.enabled = false;
        }

        void OnEnable()
        {
            inventory.Changed += OnInventoryChanged;
            // Picking the shotgun up with empty shells starts a reload if there is ammo.
            if (Loaded == 0) StartReload();
            Notify();
        }

        void OnDisable()
        {
            inventory.Changed -= OnInventoryChanged;
            // Switching weapons cancels the reload and the zoom; nothing is lost (shells are taken at the end).
            reloadEndsAt = -1f;
            if (viewCamera != null) viewCamera.fieldOfView = normalFieldOfView;
            if (muzzleFlash != null) muzzleFlash.enabled = false;
        }

        void Update()
        {
            // Aim: ease the field of view toward the zoom while right click is held.
            float targetFov = aimAction.IsPressed() ? weapon.aimFieldOfView : normalFieldOfView;
            viewCamera.fieldOfView = Mathf.MoveTowards(viewCamera.fieldOfView, targetFov, 120f * Time.deltaTime);

            if (muzzleFlash != null && muzzleFlash.enabled && Time.time >= flashUntil) muzzleFlash.enabled = false;

            if (IsReloading && Time.time >= reloadEndsAt) FinishReload();
            if (fireAction.WasPressedThisFrame()) TryFire();
        }

        public bool TryFire()
        {
            if (Time.time < nextFireTime || IsReloading) return false;
            if (Loaded == 0)
            {
                Play(weapon.emptyClip);
                StartReload();
                return false;
            }

            Loaded--;
            nextFireTime = Time.time + weapon.fireCooldown;
            Play(weapon.fireClip);
            if (look != null) look.AddKick(weapon.kickDegrees);
            if (muzzleFlash != null)
            {
                muzzleFlash.enabled = true;
                flashUntil = Time.time + 0.05f;
            }
            HitTarget();
            AlertCultists();
            Fired?.Invoke();
            Notify();
            if (Loaded == 0) StartReload();
            return true;
        }

        void HitTarget()
        {
            // Nearest thing the shot touches, skipping the Padre's own body (same rule as the machete).
            int count = Physics.SphereCastNonAlloc(aim.position, weapon.hitRadius, aim.forward, hits,
                weapon.maxRange, hitLayers, QueryTriggerInteraction.Ignore);
            RaycastHit? nearest = null;
            for (int i = 0; i < count; i++)
            {
                if (hits[i].collider.transform.IsChildOf(transform)) continue;
                if (nearest == null || hits[i].distance < nearest.Value.distance) nearest = hits[i];
            }
            if (nearest == null) return;

            Health target = nearest.Value.collider.GetComponentInParent<Health>();
            if (target == null || target == ownHealth || target.IsDead) return;
            float damage = nearest.Value.distance <= weapon.nearDistance ? weapon.damageNear : weapon.damageFar;
            if (target.Kind == HealthKind.Spirit) damage *= weapon.spiritMultiplier;
            target.TakeDamage(damage);
        }

        void AlertCultists()
        {
            // A HashSet so a cultist with several colliders only hears the shot once.
            var alerted = new HashSet<CultistAI>();
            int count = Physics.OverlapSphereNonAlloc(transform.position, weapon.noiseRadius, heard, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                CultistAI cultist = heard[i].GetComponentInParent<CultistAI>();
                if (cultist != null && alerted.Add(cultist)) cultist.HearNoise(transform.position);
            }
        }

        void StartReload()
        {
            if (IsReloading || Loaded >= weapon.shellCapacity || Reserve == 0) return;
            reloadEndsAt = Time.time + weapon.reloadSeconds;
            Play(weapon.reloadClip);
        }

        void FinishReload()
        {
            reloadEndsAt = -1f;
            int wanted = weapon.shellCapacity - Loaded;
            int take = Mathf.Min(wanted, Reserve);
            // Shells go in first: Remove fires Inventory.Changed, and with Loaded still at 0 that would
            // start another reload. take never exceeds the inventory, so Remove can't fail.
            Loaded += take;
            if (take > 0) inventory.Remove(weapon.ammoItem, take);
            Notify();
        }

        void OnInventoryChanged()
        {
            // New ammo picked up or crafted while empty: reload right away.
            if (Loaded == 0) StartReload();
            Notify();
        }

        void Notify() => AmmoChanged?.Invoke(Loaded, Reserve);

        void Play(AudioClip clip)
        {
            if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
        }
    }
}
