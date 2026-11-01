using Animas.Core;
using Animas.Entities;
using Animas.Items;
using Animas.Levels;
using UnityEngine;
using UnityEngine.AI;

namespace Animas.Enemies
{
    // A chaneque (#173): a small mischievous spirit of the hacienda and the canals. It never attacks
    // head on: it Stalks the Padre at a distance, circling him and laughing (the closer, the more often,
    // so the laugh tells where it is), then suddenly Dashes at him. If it catches him there's a scare and
    // it steals one of his items, then it Flees laughing and vanishes with it. Killing it (any weapon)
    // drops what it stole. It hops instead of walking: a little bounce of the model while it moves.
    [RequireComponent(typeof(NavMeshAgent), typeof(Health))]
    public class ChanequeAI : MonoBehaviour
    {
        public enum State { Idle, Stalk, Dash, Flee }

        [SerializeField] ChanequeTuning tuning;
        [SerializeField] AudioSource voice;
        [SerializeField] AudioClip[] laughs;
        [SerializeField] AudioClip stealClip;
        [SerializeField] AudioClip deathClip;
        [SerializeField] Sprite scareFace;
        [SerializeField] AudioClip scareClip;
        [Tooltip("The model, bounced while it moves (it hops).")]
        [SerializeField] Transform model;

        public State Current { get; private set; } = State.Idle;
        public ItemDefinition Stolen { get; private set; }

        NavMeshAgent agent;
        Health health;
        Transform player;
        Health playerHealth;
        Inventory inventory;
        float stateUntil;
        float nextLaugh;
        float orbitSign = 1f;
        Vector3 modelRest;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            health = GetComponent<Health>();
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null)
            {
                player = p.transform;
                playerHealth = p.GetComponent<Health>();
                inventory = p.GetComponent<Inventory>();
            }
            if (model != null) modelRest = model.localPosition;
            orbitSign = Random.value < 0.5f ? -1f : 1f;
        }

        void OnEnable() => health.Died += Die;
        void OnDisable() => health.Died -= Die;

        void Update()
        {
            if (player == null || health.IsDead) return;
            float distance = Vector3.Distance(transform.position, player.position);
            bool reachable = !playerHealth.IsDead && !SafeRoom.PlayerInside;

            switch (Current)
            {
                case State.Idle:
                    if (reachable && distance <= tuning.wakeDistance) StartStalk();
                    break;
                case State.Stalk:
                    if (!reachable || distance > tuning.wakeDistance * 1.3f) Enter(State.Idle, 0f, tuning.stalkSpeed);
                    else if (Time.time >= stateUntil) Enter(State.Dash, Time.time + tuning.dashSeconds, tuning.dashSpeed);
                    else Orbit(distance);
                    break;
                case State.Dash:
                    if (!reachable) StartStalk();
                    else if (distance <= tuning.catchDistance) Steal();
                    else if (Time.time >= stateUntil) StartStalk();
                    else agent.SetDestination(player.position);
                    break;
                case State.Flee:
                    if (Time.time >= stateUntil) { Destroy(gameObject); return; }
                    RunAway();
                    break;
            }

            // The closer it is, the more often it laughs: the player hears it coming.
            if (Current != State.Idle && Time.time >= nextLaugh) Laugh(distance);
            Hop();
        }

        void StartStalk() => Enter(State.Stalk, Time.time + Random.Range(tuning.dashEvery.x, tuning.dashEvery.y), tuning.stalkSpeed);

        void Enter(State state, float until, float speed)
        {
            Current = state;
            stateUntil = until;
            agent.speed = speed;
            if (state == State.Idle && agent.isOnNavMesh) agent.ResetPath();
        }

        // Circles the Padre inside its distance band: a point to one side of him, a bit closer or farther.
        void Orbit(float distance)
        {
            Vector3 away = transform.position - player.position;
            away.y = 0f;
            if (away.sqrMagnitude < 0.01f) away = Vector3.forward;
            float radius = Mathf.Clamp(distance, tuning.stalkDistance.x, tuning.stalkDistance.y);
            Vector3 side = Quaternion.Euler(0f, 35f * orbitSign, 0f) * away.normalized * radius;
            if (NavMesh.SamplePosition(player.position + side, out NavMeshHit hit, 3f, NavMesh.AllAreas))
                agent.SetDestination(hit.position);
            else orbitSign = -orbitSign;   // a wall or the water: go around the other way
        }

        void Steal()
        {
            EntityScare.Trigger(scareFace, scareClip);
            playerHealth.TakeDamage(tuning.catchDamage);
            Stolen = TakeRandomItem();
            if (voice != null && stealClip != null) voice.PlayOneShot(stealClip);
            if (inventory != null)
                inventory.Notify(Stolen != null ? $"¡Un chaneque te robó: {Stolen.displayName}!" : "¡Un chaneque! No encontró nada que robarte");
            Enter(State.Flee, Time.time + tuning.fleeSeconds, tuning.fleeSpeed);
        }

        // One unit of a random occupied space (bandages, holy water, molotovs...). Ammo, materials and key
        // items live apart, so they are safe.
        ItemDefinition TakeRandomItem()
        {
            if (inventory == null) return null;
            int count = 0;
            foreach (InventorySlot slot in inventory.Slots) if (!slot.IsEmpty) count++;
            if (count == 0) return null;
            int pick = Random.Range(0, count);
            foreach (InventorySlot slot in inventory.Slots)
            {
                if (slot.IsEmpty || pick-- > 0) continue;
                ItemDefinition item = slot.item;
                return inventory.Remove(item, 1) ? item : null;
            }
            return null;
        }

        void RunAway()
        {
            Vector3 away = transform.position - player.position;
            away.y = 0f;
            Vector3 target = transform.position + away.normalized * 8f;
            if (NavMesh.SamplePosition(target, out NavMeshHit hit, 4f, NavMesh.AllAreas)) agent.SetDestination(hit.position);
        }

        void Laugh(float distance)
        {
            float t = Mathf.InverseLerp(4f, tuning.wakeDistance, distance);
            nextLaugh = Time.time + Mathf.Lerp(tuning.laughEvery.x, tuning.laughEvery.y, t) * Random.Range(0.8f, 1.2f);
            if (voice != null && laughs != null && laughs.Length > 0) voice.PlayOneShot(laughs[Random.Range(0, laughs.Length)]);
        }

        // A bounce of the model while it moves: a hop, not a walk.
        void Hop()
        {
            if (model == null) return;
            float speed = agent.velocity.magnitude;
            float bounce = speed > 0.3f ? Mathf.Abs(Mathf.Sin(Time.time * (6f + speed * 1.5f))) * 0.12f : 0f;
            model.localPosition = modelRest + Vector3.up * bounce;
        }

        // Killed: it leaves what it stole on the floor.
        void Die()
        {
            if (deathClip != null) AudioSource.PlayClipAtPoint(deathClip, transform.position, GameSettings.GetVolume(VolumeCategory.Effects));
            if (Stolen != null && inventory != null) inventory.SpawnPickup(Stolen, 1, transform.position);
            Destroy(gameObject);
        }
    }
}
