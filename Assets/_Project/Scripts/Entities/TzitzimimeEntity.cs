using System;
using Animas.Core;
using Animas.Items;
using Animas.Levels;
using Animas.Player;
using Animas.Save;
using UnityEngine;

namespace Animas.Entities
{
    // La Tzitzimime, the final entity (GDD §13 and §21). She follows the ritual's bar:
    // Phase 1 (Dormant): the ritual starts and only cultist waves come.
    // Phase 2 (Ceiling): at half the bar she appears on the ceiling, invisible, and strikes areas of the
    //   floor (a red circle warns first). The bar only fills while copal smoke reveals her, and twice as
    //   fast with the flashlight on her: "light and prayer".
    // Phase 3 (FinalForm): the bar is full; she comes down in her final form and walks after the Padre,
    //   still striking, while he escapes to the exit. Reaching the exit ends the campaign.
    // She can't be killed with weapons; the ritual is the only way.
    [RequireComponent(typeof(SpiritBody), typeof(Health))]
    public class TzitzimimeEntity : MonoBehaviour
    {
        public enum State { Dormant, Ceiling, FinalForm }

        [SerializeField] TzitzimimeTuning tuning;
        [SerializeField] RitualController ritual;
        [Tooltip("The model: hidden while she's invisible, shown when revealed and in her final form.")]
        [SerializeField] GameObject visuals;
        [Tooltip("A flat red disc on the floor: the warning before each area attack.")]
        [SerializeField] Transform strikeMarker;
        [SerializeField] Light finalGlow;
        [SerializeField] AudioSource audioSource;
        [SerializeField] AudioClip screechClip;
        [SerializeField] AudioClip strikeClip;

        public State Current { get; private set; } = State.Dormant;
        public event Action<State> StateChanged;

        SpiritBody body;
        Health health;
        Collider bodyCollider;
        Transform player;
        Health playerHealth;
        Flashlight flashlight;
        PlayerRespawn playerRespawn;
        Inventory messages;

        Vector3 home;
        float floorY;
        float nextAttack;
        float strikeAt = -1f;
        bool warnedHidden;

        void Awake()
        {
            body = GetComponent<SpiritBody>();
            health = GetComponent<Health>();
            bodyCollider = GetComponent<Collider>();
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null)
            {
                player = p.transform;
                playerHealth = p.GetComponent<Health>();
                flashlight = p.GetComponentInChildren<Flashlight>(true);
                playerRespawn = p.GetComponent<PlayerRespawn>();
                messages = p.GetComponent<Inventory>();
            }
            home = transform.position;
            floorY = home.y;
            // The marker lives in the prefab for convenience, but it marks the floor, not her: let it go.
            if (strikeMarker != null) strikeMarker.SetParent(null, true);
        }

        void OnEnable()
        {
            health.Damaged += OnHurt;
            if (ritual != null) ritual.Completed += EnterFinalForm;
            if (playerRespawn != null) playerRespawn.Respawned += ResetToDormant;
        }

        void OnDisable()
        {
            health.Damaged -= OnHurt;
            if (ritual != null) ritual.Completed -= EnterFinalForm;
            if (playerRespawn != null) playerRespawn.Respawned -= ResetToDormant;
        }

        void Start() => Enter(State.Dormant);

        void Update()
        {
            if (player == null) return;
            switch (Current)
            {
                case State.Dormant: UpdateDormant(); break;
                case State.Ceiling: UpdateCeiling(); break;
                case State.FinalForm: UpdateFinalForm(); break;
            }
            UpdateStrike();
        }

        void UpdateDormant()
        {
            if (ritual.Current == RitualController.Stage.Praying && ritual.Progress >= tuning.phaseTwoAt)
                EnterCeiling();
        }

        void EnterCeiling()
        {
            Enter(State.Ceiling);
            transform.position = new Vector3(home.x, floorY + tuning.ceilingHeight, home.z);
            // Upside down, feet on the ceiling and head toward the Padre.
            transform.rotation = Quaternion.Euler(180f, 0f, 0f);
            nextAttack = Time.time + tuning.attackInterval * 0.5f;
            if (audioSource != null && screechClip != null) audioSource.PlayOneShot(screechClip);
            if (messages != null) messages.Notify("Algo se arrastra por el techo... Revélala con copal");
        }

        void UpdateCeiling()
        {
            // She crawls above the Padre, upside down, so she's always close enough to strike.
            Vector3 above = new Vector3(player.position.x, floorY + tuning.ceilingHeight, player.position.z);
            transform.position = Vector3.MoveTowards(transform.position, above, tuning.ceilingSpeed * Time.deltaTime);

            bool revealed = body.IsRevealed;
            bool lit = revealed && flashlight != null && flashlight.Illuminates(bodyCollider);
            if (visuals != null) visuals.SetActive(revealed);
            ritual.FillMultiplier = lit ? tuning.litFill : revealed ? tuning.revealedFill : 0f;

            // Tell the player once why the bar stopped, while he's actually praying.
            if (!revealed && !warnedHidden && ritual.Current == RitualController.Stage.Praying)
            {
                warnedHidden = true;
                if (messages != null) messages.Notify("La barra no avanza: usa copal para revelarla");
            }
            if (revealed) warnedHidden = false;

            if (Time.time >= nextAttack) BeginStrike(tuning.attackInterval);
        }

        void EnterFinalForm()
        {
            Enter(State.FinalForm);
            ritual.FillMultiplier = 1f;
            transform.SetPositionAndRotation(new Vector3(transform.position.x, floorY, transform.position.z), Quaternion.identity);
            if (finalGlow != null) finalGlow.enabled = true;
            nextAttack = Time.time + tuning.finalAttackInterval;
            if (audioSource != null && screechClip != null) audioSource.PlayOneShot(screechClip);
            if (messages != null) messages.Notify("La Tzitzimime toma su forma final. ¡Huye a la salida!");
        }

        void UpdateFinalForm()
        {
            // No NavMesh: she glides straight after him, like a star falling; walls don't stop her.
            Vector3 target = new Vector3(player.position.x, floorY, player.position.z);
            Vector3 to = target - transform.position;
            if (to.sqrMagnitude > 4f)
            {
                transform.position += Vector3.ClampMagnitude(to, tuning.finalSpeed * Time.deltaTime);
                transform.rotation = Quaternion.LookRotation(new Vector3(to.x, 0f, to.z));
            }
            if (Time.time >= nextAttack && !playerHealth.IsDead) BeginStrike(tuning.finalAttackInterval);
        }

        // The warning: a red circle where the Padre stands now. He has warningSeconds to get out.
        void BeginStrike(float interval)
        {
            nextAttack = Time.time + interval;
            if (playerHealth.IsDead || SafeRoom.PlayerInside) return;
            strikeAt = Time.time + tuning.warningSeconds;
            if (strikeMarker != null)
            {
                strikeMarker.position = new Vector3(player.position.x, floorY + 0.05f, player.position.z);
                strikeMarker.localScale = new Vector3(tuning.attackRadius * 2f, strikeMarker.localScale.y, tuning.attackRadius * 2f);
                strikeMarker.gameObject.SetActive(true);
            }
        }

        void UpdateStrike()
        {
            if (strikeAt < 0f || Time.time < strikeAt) return;
            strikeAt = -1f;
            if (strikeMarker == null) return;
            strikeMarker.gameObject.SetActive(false);
            if (audioSource != null && strikeClip != null) audioSource.PlayOneShot(strikeClip);
            Vector3 d = player.position - strikeMarker.position;
            d.y = 0f;
            if (d.magnitude <= tuning.attackRadius && !playerHealth.IsDead) playerHealth.TakeDamage(tuning.attackDamage);
        }

        // Weapons don't wear her down: only the ritual does.
        void OnHurt(float amount) => health.Heal(amount);

        void ResetToDormant()
        {
            if (Current == State.FinalForm) return;
            Enter(State.Dormant);
        }

        void Enter(State state)
        {
            Current = state;
            strikeAt = -1f;
            if (strikeMarker != null) strikeMarker.gameObject.SetActive(false);
            if (state == State.Dormant)
            {
                transform.SetPositionAndRotation(home, Quaternion.identity);
                if (ritual != null) ritual.FillMultiplier = 1f;
            }
            if (visuals != null) visuals.SetActive(state == State.FinalForm);
            if (finalGlow != null) finalGlow.enabled = state == State.FinalForm;
            StateChanged?.Invoke(state);
        }
    }
}
