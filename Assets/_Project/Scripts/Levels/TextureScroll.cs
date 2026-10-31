using UnityEngine;

namespace Animas.Levels
{
    // Moves a surface's texture so it looks alive (issue #103): the canals of Xochimilco flow because their
    // ripple normal map slides slowly. URP's Lit shader uses the base map's offset for the normal map too.
    [RequireComponent(typeof(Renderer))]
    public class TextureScroll : MonoBehaviour
    {
        [Tooltip("Texture repetitions per second in X and Y; small numbers (0.01–0.05) look like water.")]
        [SerializeField] Vector2 speed = new Vector2(0.02f, 0.01f);

        Material material;
        Vector2 offset;

        // renderer.material makes a copy for this object, so the shared asset is never modified while playing.
        void Awake() => material = GetComponent<Renderer>().material;

        void Update()
        {
            offset += speed * Time.deltaTime;
            // Wrap so the number never grows without limit (the texture tiles, so 1.0 looks the same as 0).
            offset.x %= 1f;
            offset.y %= 1f;
            material.SetTextureOffset("_BaseMap", offset);
        }

        void OnDestroy()
        {
            if (material != null) Destroy(material);
        }
    }
}
