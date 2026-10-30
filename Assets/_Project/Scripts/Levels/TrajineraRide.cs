using System;
using Animas.Core;
using Animas.Player;
using UnityEngine;

namespace Animas.Levels
{
    // The trajinera (GDD §13, P1): a scripted ride on a fixed route. F boards it; it moves by
    // itself through its waypoints and leaves the Padre at the other shore. On board he can't walk,
    // but he can look around, attack and use quick slots. One way only.
    public class TrajineraRide : MonoBehaviour, IInteractable
    {
        [SerializeField] Transform[] path;
        [SerializeField, Min(0.5f)] float speed = 3f;
        [Tooltip("Degrees per second when the boat turns to follow the route.")]
        [SerializeField, Min(10f)] float turnSpeed = 60f;
        [Tooltip("Where the Padre stands on the boat.")]
        [SerializeField] Transform seat;
        [Tooltip("Where he is left when the ride ends (on land).")]
        [SerializeField] Transform exitPoint;
        [Tooltip("The escape boat waits for the ritual (#34) to unlock it.")]
        [SerializeField] bool locked;
        [SerializeField] string lockedPrompt = "La trajinera no se mueve todavía";

        public bool Riding { get; private set; }
        public bool Arrived { get; private set; }
        public string Prompt => Arrived ? "" : locked ? lockedPrompt : "Subir a la trajinera";

        public event Action RideStarted;
        public event Action RideFinished;

        Transform rider;
        CharacterController riderBody;
        PlayerController riderMovement;
        Health riderHealth;
        int nextPoint;
        Vector3 startPosition;
        Quaternion startRotation;

        void Awake()
        {
            startPosition = transform.position;
            startRotation = transform.rotation;
        }

        public void Unlock() => locked = false;

        public void Interact(Interactor interactor)
        {
            if (Riding || Arrived || locked || path.Length == 0) return;
            rider = interactor.transform.root;
            riderBody = rider.GetComponent<CharacterController>();
            riderMovement = rider.GetComponent<PlayerController>();
            riderHealth = rider.GetComponent<Health>();
            // The CharacterController would undo position changes made from outside, so it's off on board.
            riderBody.enabled = false;
            riderMovement.enabled = false;
            Riding = true;
            nextPoint = 0;
            RideStarted?.Invoke();
        }

        void Update()
        {
            if (!Riding) return;
            if (riderHealth != null && riderHealth.IsDead)
            {
                Abort();
                return;
            }

            Vector3 target = path[nextPoint].position;
            target.y = transform.position.y;
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            Vector3 heading = target - transform.position;
            if (heading.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(heading), turnSpeed * Time.deltaTime);

            // Carry the Padre: keep him on the seat (his own look rotation is untouched).
            rider.position = seat.position;

            if ((transform.position - target).sqrMagnitude < 0.04f && ++nextPoint >= path.Length) Finish();
        }

        void Finish()
        {
            Riding = false;
            Arrived = true;
            rider.position = exitPoint.position;
            riderBody.enabled = true;
            riderMovement.enabled = true;
            RideFinished?.Invoke();
        }

        // The Padre died on board (the escape has no checkpoint): the boat goes back to its dock.
        // His controls stay off; respawning gives them back.
        void Abort()
        {
            Riding = false;
            riderBody.enabled = true;
            transform.SetPositionAndRotation(startPosition, startRotation);
        }
    }
}
