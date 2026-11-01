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
    // Her eyes (#168): while she's invisible on the ceiling only two red eyes glow up there, and in her
    // final form she vanishes now and then, eyes and all but the body, like the Llorona and the Charro.
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
        [Header("Eyes (#168)")]
        [Tooltip("Head bone of the model: the eyes follow it.")]
        [SerializeField] Transform head;
        [SerializeField] Material eyeMaterial;

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
        Renderer[] bodyRenderers = new Renderer[0];
        Transform eyes;
        bool vanished;
        float nextVanish;
        float reappearAt;

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

        void Start()
        {
            BuildEyes();
            Enter(State.Dormant);
        }

        // The eyes live outside the model (hidden on the ceiling with SetActive), so they copy the head
        // bone's position every frame after the animation moves it.
        void LateUpdate()
        {
            if (eyes == null || head == null) return;
            eyes.SetPositionAndRotation(head.position + head.forward * 0.12f + head.up * 0.05f, head.rotation);
        }

        void BuildEyes()
        {
            if (visuals != null) bodyRenderers = visuals.GetComponentsInChildren<Renderer>(true);
            if (head == null) return;
            eyes = new GameObject("OjosInvisible").transform;
            eyes.SetParent(transform, false);
            foreach (float side in new[] { -1f, 1f })
            {
                GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Destroy(eye.GetComponent<Collider>());
                eye.name = "Ojo";
                eye.transform.SetParent(eyes, false);
                eye.transform.localPosition = new Vector3(0.06f * side, 0f, 0f);
                eye.transform.localScale = Vector3.one * 0.07f;
                var r = eye.GetComponent<Renderer>();
                r.sharedMaterial = eyeMaterial;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            var glow = new GameObject("Brillo").AddComponent<Light>();
            glow.transform.SetParent(eyes, false);
            glow.transform.localPosition = Vector3.forward * 0.1f;
            glow.type = LightType.Point;
            glow.color = new Color(1f, 0.1f, 0.05f);
            glow.range = 4f;
            glow.intensity = 4f;
            glow.shadows = LightShadows.None;
            eyes.gameObject.SetActive(false);
        }

        void ShowEyes(bool on)
        {
            if (eyes != null) eyes.gameObject.SetActive(on);
        }

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
            ShowEyes(!revealed);   // hidden on the ceiling: only her eyes give her away
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
            nextVanish = Time.time + UnityEngine.Random.Range(tuning.invisibleEvery.x, tuning.invisibleEvery.y);
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
            UpdateVanish();
        }

        // Final form: now and then the body vanishes (only the renderers: she keeps moving and striking).
        void UpdateVanish()
        {
            if (!vanished && Time.time >= nextVanish)
            {
                SetVanished(true);
                reappearAt = Time.time + tuning.invisibleSeconds;
                if (audioSource != null && screechClip != null) audioSource.PlayOneShot(screechClip);
            }
            else if (vanished && Time.time >= reappearAt)
            {
                SetVanished(false);
                nextVanish = Time.time + UnityEngine.Random.Range(tuning.invisibleEvery.x, tuning.invisibleEvery.y);
            }
        }

        void SetVanished(bool on)
        {
            vanished = on;
            foreach (Renderer r in bodyRenderers) if (r != null) r.enabled = !on;
            if (finalGlow != null) finalGlow.enabled = !on;
            ShowEyes(on);
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
            if (vanished) SetVanished(false);
            ShowEyes(false);
            if (visuals != null) visuals.SetActive(state == State.FinalForm);
            if (finalGlow != null) finalGlow.enabled = state == State.FinalForm;
            StateChanged?.Invoke(state);
        }
    }
}
