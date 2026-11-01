using UnityEngine;
using UnityEngine.AI;

namespace Animas.Levels
{
    // A glowing arrow on the ground a few meters ahead of the Padre, pointing the way to a target (the
    // hacienda gate in the escape, #162). It follows the NavMesh path, not a straight line, so it turns
    // at corners and around walls. The arrow is a flat mesh built here, so it needs no model.
    public class GuideArrow : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] Material material;
        [Tooltip("How far ahead of the Padre the arrow floats, along the path.")]
        [SerializeField, Min(1f)] float ahead = 3.5f;
        [SerializeField, Min(0.1f)] float repathSeconds = 0.25f;

        Transform player;
        NavMeshPath navPath;
        float nextPath;
        Vector3 pointAt, aimAt;

        void Awake()
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
            navPath = new NavMeshPath();
            BuildVisual();
        }

        void Update()
        {
            if (player == null || target == null) return;
            if (Time.time >= nextPath)
            {
                nextPath = Time.time + repathSeconds;
                FindSpotOnPath();
            }
            // Glide toward the spot (no jumps when the path updates) and bob a little so it catches the eye.
            Vector3 bob = Vector3.up * (0.25f + Mathf.Sin(Time.time * 3f) * 0.08f);
            transform.position = Vector3.Lerp(transform.position, pointAt + bob, 8f * Time.deltaTime);
            Vector3 dir = aimAt - pointAt;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 8f * Time.deltaTime);
        }

        // Walks the path from the Padre `ahead` meters and remembers that spot and the next corner after it.
        void FindSpotOnPath()
        {
            Vector3 from = player.position;
            bool found = NavMesh.SamplePosition(from, out NavMeshHit start, 3f, NavMesh.AllAreas)
                && NavMesh.SamplePosition(target.position, out NavMeshHit end, 6f, NavMesh.AllAreas)
                && NavMesh.CalculatePath(start.position, end.position, NavMesh.AllAreas, navPath)
                && navPath.corners.Length >= 2;
            if (!found)
            {
                // No path: point straight at the target.
                Vector3 straight = (target.position - from).normalized;
                pointAt = from + straight * ahead;
                aimAt = target.position;
                return;
            }
            Vector3[] c = navPath.corners;
            float left = ahead;
            for (int i = 0; i < c.Length - 1; i++)
            {
                float leg = Vector3.Distance(c[i], c[i + 1]);
                if (left <= leg)
                {
                    pointAt = Vector3.Lerp(c[i], c[i + 1], left / Mathf.Max(leg, 0.001f));
                    aimAt = c[i + 1];
                    return;
                }
                left -= leg;
            }
            pointAt = c[c.Length - 1];
            aimAt = target.position;
        }

        // A flat arrow (a chevron head on a short shaft) lying on the ground, pointing +Z, 1.2 m long before scaling.
        void BuildVisual()
        {
            var mesh = new Mesh { name = "GuideArrow" };
            mesh.vertices = new[]
            {
                new Vector3(-0.15f, 0f, -0.6f), new Vector3(0.15f, 0f, -0.6f), new Vector3(0.15f, 0f, 0.1f), new Vector3(-0.15f, 0f, 0.1f),
                new Vector3(-0.45f, 0f, 0.1f), new Vector3(0.45f, 0f, 0.1f), new Vector3(0f, 0f, 0.6f),
            };
            // Both faces, so it is seen from above and below (stairs, slopes).
            mesh.triangles = new[] { 0, 3, 2, 0, 2, 1, 4, 6, 5, 0, 2, 3, 0, 1, 2, 4, 5, 6 };
            mesh.RecalculateNormals();
            var go = new GameObject("Flecha");
            go.transform.SetParent(transform, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.transform.localScale = Vector3.one * 1.5f;   // 1.8 m: easy to spot while running in the dark
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = material;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            // A warm light so the ground around it glows too.
            var glow = new GameObject("Luz").AddComponent<Light>();
            glow.transform.SetParent(transform, false);
            glow.transform.localPosition = Vector3.up * 0.4f;
            glow.type = LightType.Point;
            glow.color = new Color(1f, 0.8f, 0.4f);
            glow.range = 5f;
            glow.intensity = 3f;
            glow.shadows = LightShadows.None;
        }
    }
}
