using System;
using System.Collections.Generic;
using Animas.Core;
using Animas.Enemies;
using Animas.Items;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Animas.Player
{
    // Every firearm the Padre carries (it started as the escopeta's script, hence the name; since #134 it
    // fires whichever gun WeaponSwitcher puts in the hands). Left click fires (held down for automatic
    // guns), right click aims (light zoom), C reloads. Each gun remembers how many rounds it has loaded
    // and of which kind: salt rounds hit harder and hurt spirits, normal rounds only hurt people. A reload
    // uses salt rounds whenever the inventory has some. The shot is loud: cultists within noiseRadius
    // hear it and come to look (the ballesta is silent).
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
        [Tooltip("Where the held gun model goes (the guns' first-person view).")]
        [SerializeField] Transform viewRoot;
        [Tooltip("The prayer's shield makes every pellet stronger (#131).")]
        [SerializeField] PlayerPrayer prayer;
        [SerializeField] LayerMask hitLayers = ~0;

        // What one gun has inside: rounds and their kind. Kept per gun, so switching doesn't lose them.
        class Magazine
        {
            public int loaded;
            public bool salt;
        }

        public RangedWeaponDefinition Weapon => weapon;
        public int Loaded => Mag.loaded;
        public bool LoadedSalt => Mag.salt;
        public int Reserve => Count(weapon.ammoItem) + Count(weapon.normalAmmoItem);
        public bool IsReloading => reloadEndsAt > 0f;

        // (loaded, reserve): the HUD shows "2 | 10".
        public event Action<int, int> AmmoChanged;
        public event Action Fired;

        InputAction fireAction, aimAction, reloadAction;
        Health ownHealth;
        float nextFireTime;
        float reloadEndsAt = -1f;
        float normalFieldOfView;
        float flashUntil;
        GameObject heldModel;
        readonly Dictionary<RangedWeaponDefinition, Magazine> magazines = new Dictionary<RangedWeaponDefinition, Magazine>();
        readonly RaycastHit[] hits = new RaycastHit[16];
        readonly Collider[] heard = new Collider[32];
        readonly Dictionary<Health, float> pelletDamage = new Dictionary<Health, float>();

        Magazine Mag
        {
            get
            {
                if (!magazines.TryGetValue(weapon, out Magazine m)) magazines[weapon] = m = new Magazine();
                return m;
            }
        }

        void Awake()
        {
            fireAction = InputSystem.actions.FindAction("Player/Attack", true);
            aimAction = InputSystem.actions.FindAction("Player/Aim", true);
            reloadAction = InputSystem.actions.FindAction("Player/Reload", true);
            ownHealth = GetComponent<Health>();
            normalFieldOfView = viewCamera.fieldOfView;
            if (muzzleFlash != null) muzzleFlash.enabled = false;
        }

        // WeaponSwitcher puts a gun in the hands: show its model; a different gun cancels the reload.
        public void SetWeapon(RangedWeaponDefinition newWeapon)
        {
            if (newWeapon == null) return;
            if (newWeapon != weapon) reloadEndsAt = -1f;
            weapon = newWeapon;
            ShowModel();
            if (isActiveAndEnabled)
            {
                if (Loaded == 0) StartReload();
                Notify();
            }
        }

        void ShowModel()
        {
            if (viewRoot == null || weapon.viewModel == null) return;
            if (heldModel != null && heldModel.name == weapon.viewModel.name) return;
            if (heldModel != null) Destroy(heldModel);
            heldModel = Instantiate(weapon.viewModel, viewRoot);
            heldModel.name = weapon.viewModel.name;
        }

        void OnEnable()
        {
            inventory.Changed += OnInventoryChanged;
            ShowModel();
            // Taking out an empty gun starts a reload if there is ammo.
            if (Loaded == 0) StartReload();
            Notify();
        }

        void OnDisable()
        {
            inventory.Changed -= OnInventoryChanged;
            // Switching weapons cancels the reload and the zoom; nothing is lost (rounds are taken at the end).
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
            bool trigger = weapon.automatic ? fireAction.IsPressed() : fireAction.WasPressedThisFrame();
            if (trigger) TryFire();
            if (reloadAction.WasPressedThisFrame()) TryReload();
        }

        // C: reload before the gun is empty, to be ready for the next fight. With nothing to load, the
        // empty click tells the player why nothing happens.
        public bool TryReload()
        {
            if (IsReloading || !CanLoadMore()) return false;
            if (Reserve == 0)
            {
                Play(weapon.emptyClip);
                return false;
            }
            StartReload();
            return true;
        }

        public bool TryFire()
        {
            if (Time.time < nextFireTime || IsReloading) return false;
            if (Loaded == 0)
            {
                // The empty click once per press, not every frame of a held automatic trigger.
                if (fireAction.WasPressedThisFrame()) Play(weapon.emptyClip);
                StartReload();
                return false;
            }

            Mag.loaded--;
            nextFireTime = Time.time + weapon.fireCooldown;
            Play(weapon.fireClip);
            if (look != null) look.AddKick(weapon.kickDegrees);
            if (muzzleFlash != null)
            {
                muzzleFlash.enabled = true;
                flashUntil = Time.time + 0.05f;
            }
            HitTarget(Mag.salt);
            AlertCultists();
            Fired?.Invoke();
            Notify();
            if (Loaded == 0) StartReload();
            return true;
        }

        // Each pellet flies in its own direction inside the spread cone. Pellets that hit the same body add
        // up and land as ONE hit: a point-blank shot is a single strong blow (it kills a cultist), and the
        // ritual, the hit flash and the Charro see one hit, not eight small ones.
        void HitTarget(bool salt)
        {
            pelletDamage.Clear();
            float perPellet = 1f / weapon.pellets;
            float spread = Mathf.Tan(weapon.spreadDegrees * Mathf.Deg2Rad);
            for (int p = 0; p < weapon.pellets; p++)
            {
                Vector2 offset = UnityEngine.Random.insideUnitCircle * spread;
                Vector3 direction = (aim.forward + aim.right * offset.x + aim.up * offset.y).normalized;
                if (!NearestHit(direction, out RaycastHit hit)) continue;

                Health target = hit.collider.GetComponentInParent<Health>();
                if (target == null || target == ownHealth || target.IsDead) continue;
                float damage = (hit.distance <= weapon.nearDistance ? weapon.damageNear : weapon.damageFar) * perPellet;
                if (salt) damage *= weapon.saltDamageMultiplier;
                // Spirits: salt hurts them (a part of it, GDD §12); a normal round goes right through.
                if (target.Kind == HealthKind.Spirit) damage *= salt ? weapon.spiritMultiplier : 0f;
                if (prayer != null) damage *= prayer.DamageMultiplier;   // the prayer's shield (#131)
                if (damage <= 0f) continue;
                pelletDamage.TryGetValue(target, out float sum);
                pelletDamage[target] = sum + damage;
            }
            foreach (var pair in pelletDamage) pair.Key.TakeDamage(pair.Value);
        }

        // Nearest thing a pellet touches, skipping the Padre's own body (same rule as the machete).
        bool NearestHit(Vector3 direction, out RaycastHit nearest)
        {
            int count = Physics.SphereCastNonAlloc(aim.position, weapon.hitRadius, direction, hits,
                weapon.maxRange, hitLayers, QueryTriggerInteraction.Ignore);
            nearest = default;
            bool found = false;
            for (int i = 0; i < count; i++)
            {
                if (hits[i].collider.transform.IsChildOf(transform)) continue;
                if (!found || hits[i].distance < nearest.distance)
                {
                    nearest = hits[i];
                    found = true;
                }
            }
            return found;
        }

        void AlertCultists()
        {
            if (weapon.noiseRadius <= 0f) return;   // the ballesta makes no noise
            // A HashSet so a cultist with several colliders only hears the shot once.
            var alerted = new HashSet<CultistAI>();
            int count = Physics.OverlapSphereNonAlloc(transform.position, weapon.noiseRadius, heard, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                CultistAI cultist = heard[i].GetComponentInParent<CultistAI>();
                if (cultist != null && alerted.Add(cultist)) cultist.HearNoise(transform.position);
            }
        }

        // There is something worth loading: room in the gun and rounds of the kind that would go in.
        bool CanLoadMore()
        {
            bool full = Loaded >= weapon.shellCapacity;
            // With salt rounds available: fill up, or swap the normal rounds inside for salt.
            if (Count(weapon.ammoItem) > 0) return !full || (!Mag.salt && Loaded > 0);
            // Only normal rounds: they can't be added on top of salt rounds (kinds never mix).
            return !full && Count(weapon.normalAmmoItem) > 0 && (!Mag.salt || Loaded == 0);
        }

        void StartReload()
        {
            if (IsReloading || !CanLoadMore()) return;
            reloadEndsAt = Time.time + weapon.reloadSeconds;
            Play(weapon.reloadClip);
        }

        void FinishReload()
        {
            reloadEndsAt = -1f;
            Magazine m = Mag;
            bool useSalt = Count(weapon.ammoItem) > 0;
            // Kinds never mix: switching to salt gives the normal rounds back to the inventory first.
            if (useSalt && !m.salt && m.loaded > 0)
            {
                inventory.TryAdd(weapon.normalAmmoItem, m.loaded);
                m.loaded = 0;
            }
            ItemDefinition ammo = useSalt ? weapon.ammoItem : weapon.normalAmmoItem;
            int take = Mathf.Min(weapon.shellCapacity - m.loaded, Count(ammo));
            // Rounds go in first: Remove fires Inventory.Changed, and with an empty gun that would start
            // another reload. take never exceeds the inventory, so Remove can't fail.
            m.loaded += take;
            m.salt = useSalt;
            if (take > 0) inventory.Remove(ammo, take);
            Notify();
        }

        void OnInventoryChanged()
        {
            // New ammo picked up or crafted while empty: reload right away.
            if (Loaded == 0) StartReload();
            Notify();
        }

        int Count(ItemDefinition item) => item == null ? 0 : inventory.CountOf(item);

        void Notify() => AmmoChanged?.Invoke(Loaded, Reserve);

        void Play(AudioClip clip)
        {
            if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
        }
    }
}
