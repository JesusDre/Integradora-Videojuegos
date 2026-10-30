using System;
using System.Collections.Generic;
using Animas.Items;
using Animas.Player;
using UnityEngine;

namespace Animas.Levels
{
    // A canaleta in front of the chapel (GDD §13, P8): F + 1 holy water fills it, it glows and works for
    // 20 s. When the Charro Negro charges across a full one, he's weakened. It only accepts water once
    // the final phase opens (the coins are on the pedestal); before that it's just a dry ditch.
    public class HolyWaterChannel : MonoBehaviour, IInteractable
    {
        [SerializeField] ItemDefinition holyWater;
        [Tooltip("Trigger box along the ditch: the Charro is 'crossing' while inside it.")]
        [SerializeField] BoxCollider area;
        [Tooltip("The water surface and its glow, shown while full.")]
        [SerializeField] GameObject water;
        [SerializeField] Light glow;
        [SerializeField, Min(1f)] float activeSeconds = 20f;
        [SerializeField] AudioSource audioSource;
        [SerializeField] AudioClip fillClip;

        // Every channel registers here, so the Charro asks one place instead of searching the scene.
        static readonly List<HolyWaterChannel> all = new List<HolyWaterChannel>();
        public static IReadOnlyList<HolyWaterChannel> All => all;

        // Set by CharroNegroEntity when the coins are placed (and cleared when the Padre respawns).
        public static bool Unlocked { get; set; }

        public bool IsFull => Time.time < fullUntil;
        public string Prompt => IsFull ? "" : Unlocked ? "Llenar con agua bendita" : "Canaleta seca";

        public event Action Filled;

        float fullUntil = -1f;
        float baseIntensity;

        void Awake()
        {
            if (glow != null) baseIntensity = glow.intensity;
            Show(false);
        }

        void OnEnable() => all.Add(this);
        void OnDisable() => all.Remove(this);

        public void Interact(Interactor interactor)
        {
            var inventory = interactor.GetComponentInParent<Inventory>();
            if (IsFull || inventory == null) return;
            if (!Unlocked)
            {
                inventory.Notify("Coloca primero las monedas en el pedestal de la capilla");
                return;
            }
            if (!inventory.Remove(holyWater, 1))
            {
                inventory.Notify("Necesitas 1 agua bendita");
                return;
            }
            Fill();
        }

        public void Fill()
        {
            fullUntil = Time.time + activeSeconds;
            Show(true);
            if (audioSource != null && fillClip != null) audioSource.PlayOneShot(fillClip);
            Filled?.Invoke();
        }

        public void Drain()
        {
            fullUntil = -1f;
            Show(false);
        }

        public bool Contains(Vector3 point) => area.bounds.Contains(point + Vector3.up * 0.5f);

        // Did something moving from 'from' to 'to' this frame pass over the ditch? Checking the whole step,
        // not just the end point, matters: at 12 m/s and a low frame rate the horse can jump over a 1 m ditch.
        public bool Crossed(Vector3 from, Vector3 to)
        {
            Bounds b = area.bounds;
            Vector3 lift = Vector3.up * 0.5f;
            if (Contains(to)) return true;
            Vector3 step = to - from;
            float length = step.magnitude;
            if (length < 0.001f) return false;
            return b.IntersectRay(new Ray(from + lift, step / length), out float hit) && hit <= length;
        }

        void Update()
        {
            if (water == null || !water.activeSelf) return;
            float left = fullUntil - Time.time;
            if (left <= 0f)
            {
                Show(false);
                return;
            }
            // The glow fades in the last 3 s, so the player sees it's about to run out.
            if (glow != null) glow.intensity = baseIntensity * Mathf.Clamp01(left / 3f);
        }

        void Show(bool full)
        {
            if (water != null) water.SetActive(full);
            if (glow != null)
            {
                glow.enabled = full;
                glow.intensity = baseIntensity;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetOnPlay()
        {
            all.Clear();
            Unlocked = false;
        }
    }
}
