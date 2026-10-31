using Animas.Player;
using UnityEngine;

namespace Animas.Audio
{
    // One step every so many meters walked (shorter strides when running), with a clip that depends on
    // the floor under the Padre. Distance instead of a timer: steps match the speed exactly, and
    // standing still or praying makes no sound.
    public class PlayerFootsteps : MonoBehaviour
    {
        [SerializeField] PlayerController controller;
        [SerializeField] AudioSource audioSource;
        [SerializeField, Min(0.3f)] float walkStride = 1.6f;
        [SerializeField, Min(0.3f)] float runStride = 2.3f;
        [Tooltip("Floor sound where no FootstepSurface says otherwise.")]
        [SerializeField] SurfaceKind defaultSurface = SurfaceKind.Dirt;
        [SerializeField] AudioClip[] dirt;
        [SerializeField] AudioClip[] wood;
        [SerializeField] AudioClip[] stone;

        public SurfaceKind DefaultSurface { get => defaultSurface; set => defaultSurface = value; }

        Vector3 lastPosition;
        float walked;
        int lastIndex = -1;

        void Start() => lastPosition = transform.position;

        void Update()
        {
            Vector3 delta = transform.position - lastPosition;
            lastPosition = transform.position;
            delta.y = 0f;
            // A teleport (respawn, the trajinera) is not walking.
            if (!controller.IsMoving || delta.magnitude > 2f) return;

            walked += delta.magnitude;
            float stride = controller.IsRunning ? runStride : walkStride;
            if (walked < stride) return;
            walked = 0f;
            Step();
        }

        void Step()
        {
            AudioClip[] clips = ClipsFor(SurfaceUnderFoot());
            if (clips == null || clips.Length == 0) return;
            // Never the same clip twice in a row: repeated identical steps sound mechanical.
            int index = Random.Range(0, clips.Length);
            if (index == lastIndex && clips.Length > 1) index = (index + 1) % clips.Length;
            lastIndex = index;
            audioSource.pitch = Random.Range(0.92f, 1.08f);
            audioSource.PlayOneShot(clips[index], controller.IsRunning ? 1f : 0.7f);
        }

        SurfaceKind SurfaceUnderFoot()
        {
            if (Physics.Raycast(transform.position + Vector3.up * 0.3f, Vector3.down, out RaycastHit hit, 1f,
                    ~0, QueryTriggerInteraction.Ignore))
            {
                FootstepSurface surface = hit.collider.GetComponentInParent<FootstepSurface>();
                if (surface != null) return surface.kind;
            }
            return defaultSurface;
        }

        AudioClip[] ClipsFor(SurfaceKind kind) => kind switch
        {
            SurfaceKind.Wood => wood,
            SurfaceKind.Stone => stone,
            _ => dirt,
        };
    }
}
