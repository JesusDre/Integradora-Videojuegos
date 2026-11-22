using Animas.Core;
using Animas.Entities;
using Animas.Items;
using Animas.Levels;
using Animas.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Animas.Enemies
{
    // A ceiling spider of the ex-convent (#174). It Waits on the vault; when the Padre passes under it,
    // it Drops on its thread onto his head, grabs him (a scare) and Lifts him toward the ceiling, hurting
    // him. He breaks free by hitting it (any hit) or by pressing F again and again; if it gets him all
    // the way up he takes a hard hit and falls. Then it Climbs back and rests. Any weapon kills it.
    // While it holds him, his own movement (and gravity) is switched off and it moves him itself.
    [RequireComponent(typeof(Health))]
    public class CeilingSpider : MonoBehaviour
    {
        public enum State { Waiting, Dropping, Holding, Climbing }

        [SerializeField] SpiderTuning tuning;
        [Tooltip("Looping legs tapping on stone, while it moves.")]
        [SerializeField] AudioSource legs;
        [SerializeField] AudioSource voice;
        [SerializeField] AudioClip hissClip;
        [SerializeField] AudioClip deathClip;
        [SerializeField] Sprite scareFace;
        [SerializeField] AudioClip scareClip;
        [Tooltip("The silk thread from the ceiling to the spider.")]
        [SerializeField] LineRenderer thread;

        public State Current { get; private set; } = State.Waiting;
        public int Presses { get; private set; }

        Health health;
        Transform player;
        Health playerHealth;
        CharacterController playerBody;
        PlayerController playerMovement;
        Inventory messages;
        InputAction escapeAction;
        Vector3 ceiling;
        float topY;
        float readyAt;

        void Awake()
        {
            health = GetComponent<Health>();
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null)
            {
                player = p.transform;
                playerHealth = p.GetComponent<Health>();
                playerBody = p.GetComponent<CharacterController>();
                playerMovement = p.GetComponent<PlayerController>();
                messages = p.GetComponent<Inventory>();
            }
            escapeAction = InputSystem.actions.FindAction("Player/Interact", true);
            ceiling = transform.position;
            // The Padre's head stops a little under the vault.
            topY = ceiling.y - (playerBody != null ? playerBody.height : 1.8f) - 0.2f;
        }

        void OnEnable()
        {
            health.Damaged += OnHurt;
            health.Died += Die;
        }

        void OnDisable()
        {
            health.Damaged -= OnHurt;
            health.Died -= Die;
            if (Current == State.Holding) Release();
        }

        void Update()
        {
            if (player == null || health.IsDead) return;
            switch (Current)
            {
                case State.Waiting: UpdateWaiting(); break;
                case State.Dropping: UpdateDropping(); break;
                case State.Holding: UpdateHolding(); break;
                case State.Climbing: UpdateClimbing(); break;
            }
            if (thread != null)
            {
                thread.SetPosition(0, ceiling);
                thread.SetPosition(1, transform.position);
            }
            bool moving = Current == State.Dropping || Current == State.Climbing;
            if (legs != null && moving != legs.isPlaying) { if (moving) legs.Play(); else legs.Stop(); }
        }

        void UpdateWaiting()
        {
            if (Time.time < readyAt || playerHealth.IsDead || SafeRoom.PlayerInside) return;
            Vector3 d = player.position - ceiling;
            bool below = d.y < 0f && d.y > -8f;
            d.y = 0f;
            if (below && d.magnitude <= tuning.triggerRadius)
            {
                Current = State.Dropping;
                if (voice != null && hissClip != null) voice.PlayOneShot(hissClip);
            }
        }

        // Straight down onto the Padre's head.
        void UpdateDropping()
        {
            Vector3 head = player.position + Vector3.up * (playerBody != null ? playerBody.height : 1.8f);
            transform.position = Vector3.MoveTowards(transform.position, head, tuning.dropSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, head) < 0.3f) Grab();
        }

        void Grab()
        {
            Current = State.Holding;
            Presses = 0;
            // His own movement (and its gravity) is off: the spider moves him now.
            if (playerMovement != null) playerMovement.enabled = false;
            EntityScare.Trigger(scareFace, scareClip);
            if (messages != null) messages.Notify("¡Una araña te sube al techo! Golpéala o presiona F para soltarte");
        }

        void UpdateHolding()
        {
            if (playerHealth.IsDead) { Release(); return; }
            float step = Mathf.Min(tuning.liftSpeed * Time.deltaTime, topY - player.position.y);
            if (step > 0f) playerBody.Move(Vector3.up * step);
            transform.position = player.position + Vector3.up * (playerBody.height + 0.1f);
            playerHealth.TakeDamage(tuning.damagePerSecond * Time.deltaTime);

            if (escapeAction.WasPressedThisFrame() && ++Presses >= tuning.mashToEscape)
            {
                if (messages != null) messages.Notify("Te soltaste");
                Release();
            }
            else if (player.position.y >= topY - 0.05f)
            {
                // At the vault: one hard hit, and it lets him fall.
                playerHealth.TakeDamage(tuning.topDamage);
                Release();
            }
        }

        void Release()
        {
            if (playerMovement != null) playerMovement.enabled = true;
            Current = State.Climbing;
            readyAt = Time.time + tuning.cooldown;
        }

        void UpdateClimbing()
        {
            transform.position = Vector3.MoveTowards(transform.position, ceiling, tuning.dropSpeed * 0.5f * Time.deltaTime);
            if (transform.position == ceiling) Current = State.Waiting;
        }

        // Any hit while it holds him makes it let go (the "hit it" way out).
        void OnHurt(float amount)
        {
            if (Current == State.Holding && !health.IsDead) Release();
        }

        void Die()
        {
            if (Current == State.Holding) Release();
            if (deathClip != null) AudioSource.PlayClipAtPoint(deathClip, transform.position, GameSettings.GetVolume(VolumeCategory.Effects));
            Destroy(gameObject);
        }
    }
}
