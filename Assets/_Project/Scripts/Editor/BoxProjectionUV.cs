using UnityEditor;
using UnityEngine;

namespace Animas.EditorTools
{
    // The props and scenery made in Blender by script were exported without UVs (they only had flat
    // colors), so a texture would have nowhere to go. While Unity imports them, this gives each vertex
    // UVs by "box projection": it looks at where the face points (its normal) and uses the other two
    // axes as U and V, in meters. Like wrapping the model in a cube and projecting the texture from each
    // side. Models that already have UVs (TRELLIS, Mixamo) are left alone.
    public class BoxProjectionUV : AssetPostprocessor
    {
        static readonly string[] Folders =
        {
            "Assets/_Project/Art/Models/Environment/",
            "Assets/_Project/Art/Models/Props/",
        };

        void OnPostprocessModel(GameObject root)
        {
            if (!InFolders(assetPath)) return;
            foreach (MeshFilter filter in root.GetComponentsInChildren<MeshFilter>(true))
                Project(filter.sharedMesh);
            foreach (SkinnedMeshRenderer skin in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                Project(skin.sharedMesh);
        }

        static bool InFolders(string path)
        {
            foreach (string folder in Folders)
                if (path.StartsWith(folder)) return true;
            return false;
        }

        static void Project(Mesh mesh)
        {
            if (mesh == null || mesh.uv.Length > 0) return;
            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;
            if (normals.Length != vertices.Length) return;
            var uv = new Vector2[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 n = normals[i];
                Vector3 p = vertices[i];
                float ax = Mathf.Abs(n.x), ay = Mathf.Abs(n.y), az = Mathf.Abs(n.z);
                // Facing mostly X: project from the side (Z, Y); mostly Y: from above (X, Z); else (X, Y).
                if (ax >= ay && ax >= az) uv[i] = new Vector2(p.z, p.y);
                else if (ay >= az) uv[i] = new Vector2(p.x, p.z);
                else uv[i] = new Vector2(p.x, p.y);
            }
            mesh.uv = uv;
        }
    }
}
