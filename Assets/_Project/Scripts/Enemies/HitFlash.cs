using System.Collections;
using Animas.Core;
using UnityEngine;

namespace Animas.Enemies
{
    // The body flashes red for an instant when it takes damage: the player sees the hit landed.
    // Uses a MaterialPropertyBlock, so the shared materials of the model are not changed.
    [RequireComponent(typeof(Health))]
    public class HitFlash : MonoBehaviour
    {
        [SerializeField] Color flashColor = new Color(1f, 0.15f, 0.1f);
        [SerializeField, Min(0.01f)] float duration = 0.12f;

        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");  // URP/Lit's main color

        Health health;
        Renderer[] renderers;
        MaterialPropertyBlock block;

        void Awake()
        {
            health = GetComponent<Health>();
            renderers = GetComponentsInChildren<Renderer>();
            block = new MaterialPropertyBlock();
        }

        void OnEnable() => health.Damaged += OnDamaged;
        void OnDisable() => health.Damaged -= OnDamaged;

        void OnDamaged(float amount)
        {
            StopAllCoroutines();
            StartCoroutine(Flash());
        }

        IEnumerator Flash()
        {
            block.SetColor(BaseColor, flashColor);
            foreach (var r in renderers) r.SetPropertyBlock(block);
            yield return new WaitForSeconds(duration);
            // An empty block puts every material back to its own color.
            block.Clear();
            foreach (var r in renderers) r.SetPropertyBlock(block);
        }
    }
}

