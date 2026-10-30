using System;
using Animas.Player;
using UnityEngine;

namespace Animas.Levels
{
    // One of the fixed candles of an anchor (GDD P6: the Llorona's anchor has 3, lit with F,
    // and they don't spend inventory). The ritual can't start until all are lit.
    public class RitualCandle : MonoBehaviour, IInteractable
    {
        [SerializeField] Light flame;
        [SerializeField] AudioClip lightClip;

        public bool Lit { get; private set; }
        public string Prompt => Lit ? "" : "Encender veladora";

        public event Action<RitualCandle> LitUp;

        void Awake() => SetLit(false);

        public void Interact(Interactor interactor)
        {
            if (Lit) return;
            SetLit(true);
            if (lightClip != null) AudioSource.PlayClipAtPoint(lightClip, transform.position);
            LitUp?.Invoke(this);
        }

        public void SetLit(bool lit)
        {
            Lit = lit;
            if (flame != null) flame.enabled = lit;
        }
    }
}
