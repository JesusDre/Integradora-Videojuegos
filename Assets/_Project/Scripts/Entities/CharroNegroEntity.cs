using System;
using Animas.Core;
using Animas.Items;
using Animas.Levels;
using Animas.Player;
using Animas.Save;
using UnityEngine;
using UnityEngine.AI;

namespace Animas.Entities
{
    // El Charro Negro (GDD §13 and §21): Chase (trots after the Padre) → (close) → Charge (a fast
    // straight run past him) → Recover → Chase. He can't enter the chapel (a SpiritBarrier): he waits
    // at its door for a moment, then gives up and rides home; when the Padre comes out of a refuge (safe
    // room, chapel) he gets a few seconds of truce before the hunt starts again (#154). When the 3 coins are on the chapel pedestal, the final phase opens: the canaletas
    // accept holy water, and charging across a full one weakens him for a few seconds. In the final phase
    // he has a life bar (#158): only blessed hits count (salt rounds, holy water, the cruz, the chancla,
    // the martillo, or any weapon under the prayer's shield), a little at any time and triple while he's
    // weakened. Three charged cross hits while weakened are enough. At 0 he dissolves and the gate opens.
    // Before the coins are placed nothing hurts him: a blessed hit only startles him for a moment.
    [RequireComponent(typeof(NavMeshAgent), typeof(SpiritBody), typeof(Health))]
    public class CharroNegroEntity : MonoBehaviour
    {
        public enum State { Idle, Chase, Charge, Recover, Weakened, Dissolved }

        [SerializeField] CharroTuning tuning;
        [Tooltip("The chapel pedestal for the 3 coins (an Anchor). Completing it opens the final phase.")]
        [SerializeField] Anchor pedestal;
        [Tooltip("Optional trigger box: the ground he rides (GDD plan: between the tienda, the panteón and the chapel). " +
                 "Outside it he lets the Padre go and returns. Empty = he hunts everywhere (test arena).")]
        [SerializeField] BoxCollider territory;
        [Tooltip("The hacienda gate: opens when he's defeated.")]
        [SerializeField] Door gate;
        [Tooltip("Cultist hordes during the final fight (#162). Optional.")]
        [SerializeField] WaveSpawner waves;
        [SerializeField] Animator animator;
        [Tooltip("Light that flares while he's weakened, so the player knows when to strike.")]
        [SerializeField] Light weakGlow;
        [SerializeField] AudioSource audioSource;
        [SerializeField] AudioClip neighClip;
        [SerializeField] AudioClip hurtClip;
        [Header("Hooves (#123)")]
        [Tooltip("Looping source for the horse's hooves: gallop while he charges, a walk while he follows.")]
        [SerializeField] AudioSource hooves;
        [SerializeField] AudioClip gallopClip;
        [SerializeField] AudioClip walkClip;
        [Tooltip("The spurs jingle when he spurs the horse into a charge.")]
        [SerializeField] AudioClip spursClip;
        [Header("Scare (#137)")]
        [Tooltip("His face on the scare screen when his charge hits the Padre before the final phase.")]
        [SerializeField] Sprite scareFace;
        [SerializeField] AudioClip scareClip;
        [SerializeField, Min(0.1f)] float dissolveSeconds = 2.5f;
        [Header("Invisible (#166)")]
        [Tooltip("Head bones: the rider's and the horse's. Their eyes stay lit while the rest vanishes.")]
        [SerializeField] Transform riderHead;
        [SerializeField] Transform horseHead;
        [SerializeField] Material eyeMaterial;

        public State Current { get; private set; } = State.Idle;
        public bool IsInvisible { get; private set; }
        public bool FinalPhase { get; private set; }

        public event Action<State> StateChanged;
        // The level listens (the gate, #48's victory flow if needed).
        public event Action Defeated;

        NavMeshAgent agent;
        SpiritBody body;
        Health health;
        Transform player;
        Health playerHealth;
        PlayerRespawn playerRespawn;
        Inventory messages;
        UI.RitualBarUI lifeBar;
        bool addingWeakBonus;
        PlayerShotgun playerGun;
        Renderer[] bodyRenderers = new Renderer[0];
        GameObject eyes;   // the glow lights
        readonly System.Collections.Generic.List<GameObject> eyeBalls = new System.Collections.Generic.List<GameObject>();
        float nextVanish;
        float reappearAt;
        float provokedUntil;

        Vector3 home;
        Vector3 chargeTarget;
        Vector3 chargeStart;
        Vector3 lastPosition;
        float stateUntil;
        float huntAfter;          // the truce: no hunting before this time
        float edgeSince = -1f;    // when he started waiting at a refuge's edge (-1: not waiting)
        bool chargeHit;
        bool hasSpeedParam;

        static readonly int SpeedParam = Animator.StringToHash("Speed");

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            body = GetComponent<SpiritBody>();
            health = GetComponent<Health>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
            hasSpeedParam = animator != null && animator.runtimeAnimatorController != null
                && Array.Exists(animator.parameters, param => param.nameHash == SpeedParam);
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null)
            {
                player = p.transform;
                playerHealth = p.GetComponent<Health>();
                playerRespawn = p.GetComponent<PlayerRespawn>();
                messages = p.GetComponent<Inventory>();
                playerGun = p.GetComponent<PlayerShotgun>();
            }
            home = transform.position;
            if (weakGlow != null) weakGlow.enabled = false;
        }

        void OnEnable()
        {
            health.Damaged += OnHurt;
            if (playerRespawn != null) playerRespawn.Respawned += OnPlayerRespawned;
            if (pedestal != null) pedestal.OfferingsComplete += BeginFinalPhase;
            if (playerGun != null) playerGun.Fired += OnPlayerFired;
        }

        void OnDisable()
        {
            health.Damaged -= OnHurt;
            if (playerRespawn != null) playerRespawn.Respawned -= OnPlayerRespawned;
            if (pedestal != null) pedestal.OfferingsComplete -= BeginFinalPhase;
            if (playerGun != null) playerGun.Fired -= OnPlayerFired;
        }

        void Start()
        {
            // The hacienda has no ritual: its ritual bar shows the Charro's life in the final fight.
            lifeBar = FindAnyObjectByType<UI.RitualBarUI>(FindObjectsInactive.Include);
            BuildEyes();
            nextVanish = Time.time + UnityEngine.Random.Range(tuning.invisibleEvery.x, tuning.invisibleEvery.y);
            Enter(State.Chase, 0f);
        }

        void Update()
        {
            if (player == null || Current == State.Dissolved)
            {
                if (hooves != null && hooves.isPlaying) hooves.Stop();
                return;
            }

            switch (Current)
            {
                case State.Idle: UpdateIdle(); break;
                case State.Chase: UpdateChase(); break;
                case State.Charge: UpdateCharge(); break;
                case State.Recover: if (Time.time >= stateUntil) Enter(State.Chase, 0f); break;
                case State.Weakened: UpdateWeakened(); break;
            }
            if (hasSpeedParam) animator.SetFloat(SpeedParam, agent.velocity.magnitude);
            UpdateHooves();
            UpdateInvisibility();
        }

        // The hooves follow what the horse is really doing: silent when it stands, a walk or a gallop.
        void UpdateHooves()
        {
            if (hooves == null) return;
            AudioClip wanted = agent.velocity.magnitude < 0.5f ? null : Current == State.Charge ? gallopClip : walkClip;
            if (wanted == null)
            {
                if (hooves.isPlaying) hooves.Stop();
                return;
            }
            if (hooves.clip != wanted || !hooves.isPlaying)
            {
                hooves.clip = wanted;
                hooves.Play();
            }
        }

        // The Padre is dead, in the safe room, in the chapel or off his ground: back home until he returns.
        // While he stays sheltered the truce keeps restarting, so it counts from the moment he comes out.
        // Provoked (shot at), there's no truce: he comes at once.
        void UpdateIdle()
        {
            bool sheltered = !PlayerHuntable() || SpiritWards.TryRepel(player.position, out _);
            if (sheltered) huntAfter = Time.time + tuning.refugeGraceSeconds;
            else if (Time.time >= huntAfter || Provoked) Enter(State.Chase, 0f);
        }

        bool Provoked => Time.time < provokedUntil;

        // Shooting at him gives the Padre away, even a normal round that does him no harm (#166).
        void OnPlayerFired()
        {
            Camera cam = Camera.main;
            if (cam == null || Current == State.Dissolved) return;
            if (!Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, 80f, ~0, QueryTriggerInteraction.Ignore)) return;
            if (hit.collider.GetComponentInParent<CharroNegroEntity>() == this) Provoke();
        }

        void Provoke()
        {
            provokedUntil = Time.time + tuning.provokedSeconds;
            huntAfter = 0f;
        }

        void GoHome()
        {
            Enter(State.Idle, 0f);
            agent.SetDestination(home);
        }

        void UpdateChase()
        {
            if (!PlayerHuntable())
            {
                GoHome();
                return;
            }
            // If the Padre is somewhere he can't enter (the chapel, a prayer circle), he waits at its edge,
            // but not forever: camping the door would leave no way out, so after a while he leaves.
            bool repelled = SpiritWards.TryRepel(player.position, out Vector3 edge);
            if (!repelled) edgeSince = -1f;
            else if (edgeSince < 0f) edgeSince = Time.time;
            else if (Time.time - edgeSince >= tuning.maxEdgeWaitSeconds)
            {
                edgeSince = -1f;
                GoHome();
                return;
            }
            Vector3 target = repelled ? edge : player.position;
            agent.SetDestination(target);
            if (target == player.position && FlatDistance(transform.position, player.position) <= tuning.chargeRange)
                StartCharge();
        }

        void StartCharge()
        {
            // Aim past the Padre: the charge doesn't steer, so stepping aside at the last second dodges it.
            Vector3 dir = player.position - transform.position;
            dir.y = 0f;
            chargeTarget = player.position + dir.normalized * tuning.chargeOvershoot;
            if (NavMesh.SamplePosition(chargeTarget, out NavMeshHit hit, 4f, NavMesh.AllAreas)) chargeTarget = hit.position;
            chargeHit = false;
            chargeStart = transform.position;
            lastPosition = transform.position;
            Enter(State.Charge, Time.time + tuning.maxChargeSeconds);
            agent.SetDestination(chargeTarget);
            if (audioSource != null && neighClip != null) audioSource.PlayOneShot(neighClip);
            if (audioSource != null && spursClip != null) audioSource.PlayOneShot(spursClip);
        }

        void UpdateCharge()
        {
            if (!chargeHit && !playerHealth.IsDead && FlatDistance(transform.position, player.position) <= tuning.hitRange)
            {
                chargeHit = true;
                playerHealth.TakeDamage(tuning.chargeDamage);
                // Being run down is a scare, in the final fight too (#164); JumpscareUI's cooldown keeps it rare.
                EntityScare.Trigger(scareFace, scareClip);
            }
            // Charging across a full canaleta weakens him (only in the final phase, when they can be filled).
            // A charge that starts standing in the water doesn't count: each hit needs a new crossing.
            foreach (HolyWaterChannel channel in HolyWaterChannel.All)
            {
                if (!channel.IsFull || channel.Contains(chargeStart)) continue;
                if (!channel.Crossed(lastPosition, transform.position)) continue;
                Weaken();
                return;
            }
            lastPosition = transform.position;
            bool arrived = !agent.pathPending && agent.remainingDistance <= 0.5f;
            if (arrived || Time.time >= stateUntil) Enter(State.Recover, Time.time + tuning.recoverSeconds);
        }

        void Weaken()
        {
            Enter(State.Weakened, Time.time + tuning.weakenedSeconds);
            if (weakGlow != null) weakGlow.enabled = true;
            if (audioSource != null && hurtClip != null) audioSource.PlayOneShot(hurtClip);
            if (messages != null) messages.Notify("¡El Charro está débil! Golpéalo con la cruz cargada");
        }

        void UpdateWeakened()
        {
            if (Time.time < stateUntil) return;
            if (weakGlow != null) weakGlow.enabled = false;
            Enter(State.Chase, 0f);
        }

        // Health already took the hit; here the Charro decides whether it really counts.
        void OnHurt(float amount)
        {
            if (addingWeakBonus) return;   // the extra damage below comes back through here
            Provoke();                     // whoever hits him gets hunted, wherever he is
            // Unblessed (machete, normal rounds, fire), or before the final phase: it's undone.
            if (!health.LastHitBlessed || !FinalPhase)
            {
                health.Heal(amount);
                if (health.LastHitBlessed && Current == State.Chase) Enter(State.Recover, Time.time + tuning.flinchSeconds);
                return;
            }
            // Weakened by the holy water: the same hit hurts him several times more.
            if (Current == State.Weakened)
            {
                addingWeakBonus = true;
                health.TakeDamage(amount * (tuning.weakDamageMultiplier - 1f), true);
                addingWeakBonus = false;
            }
            if (audioSource != null && hurtClip != null) audioSource.PlayOneShot(hurtClip);
            ShowLife();
            if (health.IsDead) Dissolve();
        }

        void ShowLife()
        {
            if (lifeBar != null) lifeBar.SetProgress(health.Current / health.Max);
        }

        void BeginFinalPhase()
        {
            FinalPhase = true;
            HolyWaterChannel.Unlocked = true;
            if (waves != null) waves.Begin();
            if (lifeBar != null)
            {
                lifeBar.SetTitle("Charro Negro");
                lifeBar.Begin();
                ShowLife();
            }
            if (messages != null) messages.Notify("Llena las canaletas con agua bendita y hazlo cruzar");
        }

        void OnPlayerRespawned()
        {
            // Every death clears the fight's horde; after his defeat the escape brings its own (EscapeRun).
            if (waves != null) waves.ResetWaves();
            if (Current == State.Dissolved) return;
            // The coins went back to the bag (the pedestal empties itself): back to phase 1, full life.
            health.Revive(health.Max);
            if (lifeBar != null) lifeBar.End();
            FinalPhase = false;
            HolyWaterChannel.Unlocked = false;
            foreach (HolyWaterChannel channel in HolyWaterChannel.All) channel.Drain();
            if (weakGlow != null) weakGlow.enabled = false;
            agent.Warp(home);
            Enter(State.Chase, 0f);
        }

        void Dissolve()
        {
            SetInvisible(false);   // he is seen while he dissolves
            Enter(State.Dissolved, 0f);
            if (lifeBar != null) lifeBar.End();
            if (waves != null) waves.Stop();
            foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = false;
            if (gate != null)
            {
                gate.Unlock();
                gate.SetOpen(true);
            }
            if (messages != null) messages.Notify("El Charro Negro se disuelve. ¡Corre al portón!");
            Defeated?.Invoke();
            StartCoroutine(SinkAndFade());
        }

        System.Collections.IEnumerator SinkAndFade()
        {
            agent.enabled = false;
            Vector3 start = transform.position;
            for (float t = 0f; t < 1f; t += Time.deltaTime / dissolveSeconds)
            {
                transform.position = start + Vector3.down * (3f * t);
                yield return null;
            }
            gameObject.SetActive(false);
        }

        void Enter(State state, float until)
        {
            Current = state;
            stateUntil = until;
            if (agent.enabled && agent.isOnNavMesh)
            {
                // Weakened and recovering, he stands still. (Not isStopped: SpiritBody uses that for the
                // holy-water stun and clears it by itself.)
                if (state == State.Weakened || state == State.Recover || state == State.Dissolved)
                {
                    agent.ResetPath();
                    agent.velocity = Vector3.zero;
                }
                agent.speed = state == State.Charge ? tuning.chargeSpeed : tuning.chaseSpeed;
                agent.acceleration = state == State.Charge ? 40f : 12f;
            }
            StateChanged?.Invoke(state);
        }

        // Off his ground the Padre is safe, unless he provoked him.
        bool PlayerHuntable() => !playerHealth.IsDead && !SafeRoom.PlayerInside
            && (territory == null || Provoked || territory.bounds.Contains(player.position + Vector3.up));

        // Now and then, while he trots or charges, rider and horse vanish (#166): only their eyes glow in the
        // dark and the hooves keep sounding. Weakened he is always seen (the player must aim the cross).
        // Only the renderers go off: he still collides, charges and takes hits.
        void UpdateInvisibility()
        {
            bool canVanish = Current == State.Chase || Current == State.Charge;
            if (!IsInvisible && canVanish && Time.time >= nextVanish)
            {
                SetInvisible(true);
                reappearAt = Time.time + tuning.invisibleSeconds;
                if (audioSource != null && neighClip != null) audioSource.PlayOneShot(neighClip);
            }
            else if (IsInvisible && (!canVanish || Time.time >= reappearAt))
            {
                SetInvisible(false);
                nextVanish = Time.time + UnityEngine.Random.Range(tuning.invisibleEvery.x, tuning.invisibleEvery.y);
            }
        }

        void SetInvisible(bool on)
        {
            IsInvisible = on;
            foreach (Renderer r in bodyRenderers) if (r != null) r.enabled = !on;
            if (eyes != null) eyes.SetActive(on);
            // The eyeballs live on the head bones (so they follow the animation), not under "OjosInvisible".
            foreach (GameObject eye in eyeBalls) eye.SetActive(on);
        }

        // A pair of glowing eyes on each head bone (built once, they follow the animation) and a faint red light.
        void BuildEyes()
        {
            bodyRenderers = GetComponentsInChildren<Renderer>(true);
            eyes = new GameObject("OjosInvisible");
            eyes.transform.SetParent(transform, false);
            // The rider looks forward; the horse's head bone runs along its head, toward the nose.
            if (riderHead != null)
                AddEyes(riderHead, riderHead.position + transform.forward * 0.1f + transform.up * 0.05f, 0.05f, 0.06f);
            if (horseHead != null)
                AddEyes(horseHead, horseHead.position + horseHead.up * 0.22f + transform.up * 0.06f, 0.14f, 0.08f);
            SetInvisible(false);
        }

        void AddEyes(Transform bone, Vector3 center, float halfSpacing, float size)
        {
            foreach (float side in new[] { -1f, 1f })
            {
                GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Destroy(eye.GetComponent<Collider>());
                eye.name = "Ojo";
                eye.transform.SetParent(bone, false);
                eye.transform.position = center + transform.right * (halfSpacing * side);
                eye.transform.localScale = Vector3.one * size / Mathf.Max(bone.lossyScale.x, 0.0001f);
                var r = eye.GetComponent<Renderer>();
                r.sharedMaterial = eyeMaterial;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                eyeBalls.Add(eye);
            }
            var glow = new GameObject("Brillo").AddComponent<Light>();
            glow.transform.SetParent(eyes.transform, false);
            glow.transform.position = center + transform.forward * 0.1f;
            glow.type = LightType.Point;
            glow.color = new Color(1f, 0.1f, 0.05f);
            glow.range = 4f;
            glow.intensity = 4f;
            glow.shadows = LightShadows.None;
        }

        static float FlatDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }
    }
}
