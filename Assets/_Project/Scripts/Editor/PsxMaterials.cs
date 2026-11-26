using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Animas.EditorTools
{
    // Menu "Ánimas/Aplicar texturas PS1": gives the Blender-made props and scenery a PS1-style texture.
    // Their FBX files carry flat-colored materials named after what they are (Madera, Piedra, Acero...).
    // For each one this creates a URP Lit material in Art/Materials/PSX with the same color and the
    // matching T_PSX_* texture on top (the texture is near white, so it multiplies the color), and tells
    // the FBX to use it (a material "remap"). Glowing materials (eyes, flames, holy water) are left as
    // they are. Run it again after adding a new prop: it only creates what is missing.
    public static class PsxMaterials
    {
        const string TextureFolder = "Assets/_Project/Art/Textures/PSX";
        const string MaterialFolder = "Assets/_Project/Art/Materials/PSX";

        // Words in the material name -> texture. The first match wins, so the order matters.
        static readonly (string word, string kind)[] Kinds =
        {
            ("Pintura", "Pintura"), ("Juguete", "Pintura"), ("Trajinera", "Pintura"), ("Hule", "Pintura"),
            ("Madera", "Madera"), ("Tabla", "Madera"), ("Corteza", "Madera"), ("Mango", "Madera"),
            ("Cacha", "Madera"), ("Corcho", "Madera"), ("Carton", "Madera"),
            ("Piedra", "Piedra"), ("Adobe", "Piedra"), ("Teja", "Piedra"), ("Barro", "Piedra"), ("Obsidiana", "Piedra"),
            ("Acero", "Metal"), ("Hierro", "Metal"), ("Pavon", "Metal"), ("Plata", "Metal"), ("Laton", "Metal"),
            ("Moneda", "Metal"),
            ("Costal", "Tela"), ("Toldo", "Tela"), ("Trapo", "Tela"), ("Venda", "Tela"), ("Correa", "Tela"),
            ("Vestido", "Tela"), ("Cuerda", "Tela"), ("Cordel", "Tela"), ("Mecate", "Tela"), ("Pelo", "Tela"),
            ("Follaje", "Vegetal"), ("Arbusto", "Vegetal"), ("Pasto", "Vegetal"), ("Tule", "Vegetal"),
            ("Heno", "Vegetal"), ("Zacate", "Vegetal"), ("Pluma", "Vegetal"),
        };

        // Meters per texture repeat: small props show finer detail than a whole hut.
        const float PropTiling = 4f;
        const float SceneryTiling = 1.5f;

        [MenuItem("Ánimas/Aplicar texturas PS1")]
        public static void Apply()
        {
            Directory.CreateDirectory(MaterialFolder);
            int models = 0, materials = 0;
            foreach (string folder in new[] { "Assets/_Project/Art/Models/Environment", "Assets/_Project/Art/Models/Props" })
            {
                foreach (string guid in AssetDatabase.FindAssets("t:Model", new[] { folder }))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (path.Contains("/Armas/")) continue;   // the TRELLIS weapons have their own textures
                    float tiling = path.Contains("/Props/") ? PropTiling : SceneryTiling;
                    int made = ApplyTo(path, tiling);
                    if (made > 0) { models++; materials += made; }
                }
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"[PsxMaterials] {materials} materials on {models} models");
        }

        static int ApplyTo(string path, float tiling)
        {
            var importer = (ModelImporter)AssetImporter.GetAtPath(path);
            var remapped = new HashSet<string>();
            foreach (var pair in importer.GetExternalObjectMap()) remapped.Add(pair.Key.name);
            int made = 0;
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (!(asset is Material source) || remapped.Contains(source.name)) continue;
                if (source.IsKeywordEnabled("_EMISSION")) continue;   // eyes, flames, holy water: keep the glow
                string kind = KindOf(source.name);
                Material material = MaterialFor(source, kind, tiling);
                importer.AddRemap(new AssetImporter.SourceAssetIdentifier(source), material);
                made++;
            }
            if (made > 0) importer.SaveAndReimport();
            return made;
        }

        static string KindOf(string name)
        {
            foreach (var (word, kind) in Kinds)
                if (name.Contains(word)) return kind;
            return "Mugre";   // wax, porcelain, glass, copal... a soft grime
        }

        // One material per original name and tiling, shared by every model that uses that name.
        static Material MaterialFor(Material source, string kind, float tiling)
        {
            string path = $"{MaterialFolder}/M_PSX_{source.name}_{tiling:0.#}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.SetColor("_BaseColor", source.HasProperty("_BaseColor") ? source.GetColor("_BaseColor") : Color.white);
            material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>($"{TextureFolder}/T_PSX_{kind}.png"));
            material.SetTextureScale("_BaseMap", Vector2.one * tiling);
            material.SetFloat("_Smoothness", source.HasProperty("_Smoothness") ? Mathf.Min(source.GetFloat("_Smoothness"), 0.3f) : 0.1f);
            if (source.HasProperty("_Metallic")) material.SetFloat("_Metallic", source.GetFloat("_Metallic"));
            AssetDatabase.CreateAsset(material, path);
            return material;
        }
    }
}
