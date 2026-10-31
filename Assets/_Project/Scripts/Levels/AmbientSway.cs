using System.Collections.Generic;
using UnityEngine;

namespace Animas.Levels
{
    // Makes the level breathe (issue #103): grass, trees and agave lean with the wind and the hanging dolls
    // swing on their ropes. ONE component moves a whole group (hundreds of objects) in a single Update
    // instead of one script per blade of grass, and only what is near the Padre moves: the fog hides the
    // rest anyway. Each object turns around its own pivot, which the models have at the base (plants)
    // or at the top of the rope (hanging dolls).
    public class AmbientSway : MonoBehaviour
    {
        [Tooltip("Objects whose mesh or material name starts with one of these are moved (e.g. \"Pasto\", \"MunecaColgada\").")]
        [SerializeField] string[] names;
        [SerializeField, Range(0f, 30f)] float degrees = 4f;
        [Tooltip("Swings per second.")]
        [SerializeField, Min(0.01f)] float speed = 0.8f;
        [Tooltip("0 = each object swings on its own (dolls); 1 = a gust that travels across the field (grass).")]
        [SerializeField, Range(0f, 1f)] float wave = 0f;
        [SerializeField, Min(1f)] float activeRadius = 45f;

        struct Item
        {
            public Transform transform;
            public Quaternion rest;
            public float phase;
        }

        readonly List<Item> items = new List<Item>();
        Transform player;
        // The wind blows from one direction; the sway axis is perpendicular to it.
        static readonly Vector3 WindAxis = new Vector3(0.8f, 0f, 0.6f).normalized;

        void Start()
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;

            foreach (MeshFilter filter in FindObjectsByType<MeshFilter>(FindObjectsSortMode.None))
            {
                if (!Matches(filter)) continue;
                Transform t = filter.transform;
                // With a wave the phase depends on the position along the wind, so neighbors move together.
                float alongWind = Vector3.Dot(t.position, WindAxis) * 0.15f;
                float own = Random.value * Mathf.PI * 2f;
                items.Add(new Item { transform = t, rest = t.rotation, phase = Mathf.Lerp(own, alongWind, wave) });
            }
        }

        bool Matches(MeshFilter filter)
        {
            string mesh = filter.sharedMesh != null ? filter.sharedMesh.name : "";
            var renderer = filter.GetComponent<Renderer>();
            string material = renderer != null && renderer.sharedMaterial != null ? renderer.sharedMaterial.name : "";
            foreach (string n in names)
                if (mesh.StartsWith(n) || material.StartsWith(n)) return true;
            return false;
        }

        void Update()
        {
            if (player == null) return;
            Vector3 center = player.position;
            float radiusSqr = activeRadius * activeRadius;
            float time = Time.time * speed * Mathf.PI * 2f;
            Vector3 across = Vector3.Cross(Vector3.up, WindAxis);

            foreach (Item item in items)
            {
                if ((item.transform.position - center).sqrMagnitude > radiusSqr) continue;
                // A main push along the wind plus a smaller, faster wobble across it: never a perfect pendulum.
                float push = Mathf.Sin(time + item.phase) * degrees;
                float wobble = Mathf.Sin(time * 1.7f + item.phase * 2.3f) * degrees * 0.35f;
                item.transform.rotation = Quaternion.AngleAxis(push, across) * Quaternion.AngleAxis(wobble, WindAxis) * item.rest;
            }
        }
    }
}
