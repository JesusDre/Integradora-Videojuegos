using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Animas.EditorTools
{
    // The levels are built from Unity cubes, whose UVs go 0..1 on every face whatever its size: a texture
    // would stretch along a 20 m wall. This tool gives each scaled cube a copy of the mesh whose UVs are
    // in METERS (box projection: each face uses the two axes it spans), so the material's tiling says how
    // many meters one repetition covers and every wall shows the same texel size. Editor only: the copies
    // are saved as assets, the game just renders ordinary meshes.
    public static class WorldUVBaker
    {
        const string Folder = "Assets/_Project/Art/Meshes/UVMundo";

        [MenuItem("Ánimas/Texturas/Recalcular UV por metro (escenas abiertas)")]
        static void BakeOpenScenes()
        {
            int count = 0;
            for (int i = 0; i < SceneManager.sceneCount; i++) count += Bake(SceneManager.GetSceneAt(i), null);
            Debug.Log($"[WorldUVBaker] {count} objetos con UV por metro.");
        }

        // Only renderers using one of these materials are baked (null = every cube).
        public static int Bake(Scene scene, ICollection<string> materialNames)
        {
            Directory.CreateDirectory(Folder);
            var cache = new Dictionary<string, Mesh>();
            int count = 0;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (MeshFilter filter in root.GetComponentsInChildren<MeshFilter>(true))
                {
                    var renderer = filter.GetComponent<MeshRenderer>();
                    if (renderer == null || filter.sharedMesh == null) continue;
                    if (materialNames != null && (renderer.sharedMaterial == null || !materialNames.Contains(renderer.sharedMaterial.name))) continue;
                    Mesh source = SourceMesh(filter.sharedMesh);
                    if (source == null) continue;

                    Vector3 scale = filter.transform.lossyScale;
                    scale = new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
                    string key = $"{source.name}_{scale.x:0.00}x{scale.y:0.00}x{scale.z:0.00}";
                    if (!cache.TryGetValue(key, out Mesh baked)) cache[key] = baked = LoadOrCreate(source, scale, key);
                    if (filter.sharedMesh != baked)
                    {
                        Undo.RecordObject(filter, "UV por metro");
                        filter.sharedMesh = baked;
                        count++;
                    }
                }
            }
            return count;
        }

        // A built-in primitive, or a mesh this tool already baked (its name starts with the primitive's).
        static Mesh SourceMesh(Mesh mesh)
        {
            string path = AssetDatabase.GetAssetPath(mesh);
            if (path == "Library/unity default resources") return mesh.name is "Cube" or "Plane" or "Quad" ? mesh : null;
            if (!path.StartsWith(Folder)) return null;
            string primitive = mesh.name.Split('_')[0];
            return Resources.GetBuiltinResource<Mesh>(primitive + ".fbx");
        }

        static Mesh LoadOrCreate(Mesh source, Vector3 scale, string key)
        {
            string path = $"{Folder}/{key}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null) return existing;

            Mesh mesh = Object.Instantiate(source);
            mesh.name = key;
            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;
            var uvs = new Vector2[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 p = Vector3.Scale(vertices[i], scale);     // the vertex in meters
                Vector3 n = normals[i];
                float ax = Mathf.Abs(n.x), ay = Mathf.Abs(n.y), az = Mathf.Abs(n.z);
                if (ax >= ay && ax >= az) uvs[i] = new Vector2(p.z, p.y);      // side faces facing X
                else if (ay >= az) uvs[i] = new Vector2(p.x, p.z);             // floor and ceiling
                else uvs[i] = new Vector2(p.x, p.y);                           // faces facing Z
            }
            mesh.uv = uvs;
            // Normal maps need tangents that follow the new UVs.
            mesh.RecalculateTangents();
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }
    }
}
