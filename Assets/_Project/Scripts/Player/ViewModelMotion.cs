using UnityEngine;
using UnityEngine.InputSystem;

namespace Animas.Player
{
    // Small motions of what the Padre holds in first person, on top of its own animation:
    //  - while a gun reloads, it dips down and tilts (until now the reload was only a sound);
    //  - if the player leaves the Padre alone for a while (AFK), now and then he looks at his weapon:
    //    brings it toward the center, turns it to see it, and puts it back.
    // It only moves one Transform (the arms model or the gun holder) away from its rest pose and back,
    // so it never fights the Animator, which moves the bones inside.
    public class ViewModelMotion : MonoBehaviour
    {
        [Tooltip("What moves: the arms model, or the transform that holds the gun models.")]
        [SerializeField] Transform target;
        [Tooltip("Optional: the guns script, for the reload pose.")]
        [SerializeField] PlayerShotgun guns;

        [Header("AFK: looking at the weapon")]
        [Tooltip("Seconds without touching anything before the first look.")]
        [Min(1f)] [SerializeField] float idleSeconds = 8f;
        [Tooltip("Seconds between looks while still AFK.")]
        [Min(1f)] [SerializeField] float lookEvery = 12f;
        [SerializeField] Vector3 inspectPosition = new Vector3(-0.07f, 0.05f, 0.03f);
        [SerializeField] Vector3 inspectRotation = new Vector3(-10f, -30f, 35f);
        [Tooltip("Ease in, hold, ease out (seconds).")]
        [SerializeField] Vector3 inspectTiming = new Vector3(0.45f, 1.4f, 0.5f);

        [Header("Reload pose")]
        [SerializeField] Vector3 reloadPosition = new Vector3(0.02f, -0.07f, -0.02f);
        [SerializeField] Vector3 reloadRotation = new Vector3(25f, 10f, -30f);
        [Tooltip("How fast it goes into and out of the reload pose.")]
        [Min(0.1f)] [SerializeField] float reloadSpeed = 8f;

        InputAction move, look, attack, aim, interact, pray, reload;
        Vector3 restPosition;
        Quaternion restRotation;
        float lastInput;
        float inspectStart = -999f;
        float reloadWeight;

        void Awake()
        {
            if (target == null) target = transform;
            restPosition = target.localPosition;
            restRotation = target.localRotation;
            move = InputSystem.actions.FindAction("Player/Move");
            look = InputSystem.actions.FindAction("Player/Look");
            attack = InputSystem.actions.FindAction("Player/Attack");
            aim = InputSystem.actions.FindAction("Player/Aim");
            interact = InputSystem.actions.FindAction("Player/Interact");
            pray = InputSystem.actions.FindAction("Player/Pray");
            reload = InputSystem.actions.FindAction("Player/Reload");
            lastInput = Time.time;
        }

        void OnDisable()
        {
            // Hidden (e.g. the arms when a gun comes out): leave it at rest for next time.
            if (target != null) target.SetLocalPositionAndRotation(restPosition, restRotation);
            inspectStart = -999f;
        }

        void Update()
        {
            if (AnyInput())
            {
                lastInput = Time.time;
                inspectStart = -999f;   // touching anything interrupts the look at once
            }
            else if (Time.time - lastInput >= idleSeconds && Time.time - inspectStart >= lookEvery)
            {
                inspectStart = Time.time;
            }

            bool reloading = guns != null && guns.IsReloading;
            reloadWeight = Mathf.MoveTowards(reloadWeight, reloading ? 1f : 0f, reloadSpeed * Time.deltaTime);
            float inspectWeight = reloading ? 0f : InspectWeight(Time.time - inspectStart);

            Vector3 offset = inspectPosition * inspectWeight + reloadPosition * reloadWeight;
            Quaternion turn = Quaternion.Euler(inspectRotation * inspectWeight + reloadRotation * reloadWeight);
            target.SetLocalPositionAndRotation(restPosition + offset, restRotation * turn);
        }

        // 0 -> 1 -> hold -> 0, smoothed at both ends.
        float InspectWeight(float t)
        {
            float inT = inspectTiming.x, hold = inspectTiming.y, outT = inspectTiming.z;
            if (t < 0f || t > inT + hold + outT) return 0f;
            if (t < inT) return Mathf.SmoothStep(0f, 1f, t / inT);
            if (t < inT + hold) return 1f;
            return Mathf.SmoothStep(1f, 0f, (t - inT - hold) / outT);
        }

        bool AnyInput()
        {
            if (move != null && move.ReadValue<Vector2>().sqrMagnitude > 0.01f) return true;
            if (look != null && look.ReadValue<Vector2>().sqrMagnitude > 0.5f) return true;
            return Pressed(attack) || Pressed(aim) || Pressed(interact) || Pressed(pray) || Pressed(reload);
        }

        static bool Pressed(InputAction action) => action != null && action.IsPressed();
    }
}
