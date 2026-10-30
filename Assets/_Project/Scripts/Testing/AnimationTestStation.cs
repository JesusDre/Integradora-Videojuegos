using System;
using Animas.Player;
using UnityEngine;

namespace Animas.Testing
{
    // A lectern in front of an entity on display: each F press plays the next animation on the list,
    // so the entities' animations can be checked before their AI exists.
    // Step format: "Grab" fires a trigger, "Speed=2" sets a float, "OnCeiling=true" sets a bool,
    // "Reset" puts the entity back in its starting pose (after Vanish or Dissolve).
    // Each step is sent to every animator that has that parameter (the Charro and his horse share a lectern).
    public class AnimationTestStation : MonoBehaviour, IInteractable
    {
        [SerializeField] string entityName = "entidad";
        [SerializeField] Animator[] animators;
        [SerializeField] string[] steps;

        int next;

        public string Prompt => steps.Length == 0 ? entityName : $"{entityName}: {steps[next]}";

        public void Interact(Interactor interactor)
        {
            if (steps.Length == 0) return;
            Apply(steps[next]);
            next = (next + 1) % steps.Length;
        }

        void Apply(string step)
        {
            string[] parts = step.Split('=');
            string parameter = parts[0].Trim();
            foreach (Animator animator in animators)
            {
                if (parameter == "Reset")
                {
                    animator.Rebind();
                    continue;
                }
                if (!Has(animator, parameter)) continue;
                if (parts.Length == 1) animator.SetTrigger(parameter);
                else if (bool.TryParse(parts[1], out bool flag)) animator.SetBool(parameter, flag);
                else if (float.TryParse(parts[1], System.Globalization.NumberStyles.Float,
                             System.Globalization.CultureInfo.InvariantCulture, out float value))
                    animator.SetFloat(parameter, value);
                else Debug.LogWarning($"[AnimationTestStation] Can't read step '{step}'.", this);
            }
        }

        static bool Has(Animator animator, string parameter) =>
            Array.Exists(animator.parameters, p => p.name == parameter);
    }
}
