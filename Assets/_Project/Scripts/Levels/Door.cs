using Animas.Player;
using UnityEngine;

namespace Animas.Levels
{
    // A door that swings open or closed with F. Put this on the hinge (the pivot at the door's edge).
    public class Door : MonoBehaviour, IInteractable
    {
        [SerializeField] float openAngle = 100f;
        [Tooltip("Degrees per second.")]
        [SerializeField] float swingSpeed = 180f;
        [SerializeField] bool startOpen;
        [Tooltip("A locked door can't be opened with F (the hacienda gate until the Charro is defeated).")]
        [SerializeField] bool locked;
        [SerializeField] AudioSource audioSource;
        [SerializeField] AudioClip openClip;
        [SerializeField] AudioClip closeClip;

        public bool IsOpen { get; private set; }
        public bool Locked => locked;
        public string Prompt => locked ? "Cerrado" : IsOpen ? "Cerrar puerta" : "Abrir puerta";

        Quaternion closedRotation;
        Quaternion openRotation;

        void Awake()
        {
            closedRotation = transform.localRotation;
            openRotation = closedRotation * Quaternion.Euler(0f, openAngle, 0f);
            IsOpen = startOpen;
            transform.localRotation = IsOpen ? openRotation : closedRotation;
        }

        public void Interact(Interactor interactor)
        {
            if (!locked) SetOpen(!IsOpen);
        }

        public void Unlock() => locked = false;

        // Used by the safe room, which closes its door behind the Padre.
        public void SetOpen(bool open)
        {
            if (open == IsOpen) return;
            IsOpen = open;
            AudioClip clip = IsOpen ? openClip : closeClip;
            if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
        }

        void Update()
        {
            // Swing over time instead of snapping, so the action reads clearly (feedback the rubric asks for).
            Quaternion target = IsOpen ? openRotation : closedRotation;
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, target, swingSpeed * Time.deltaTime);
        }
    }
}
