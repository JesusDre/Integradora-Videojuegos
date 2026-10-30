using Animas.Core;
using UnityEngine;

namespace Animas.Player
{
    // When the Padre's health reaches 0, stop every player input and free the cursor.
    // The defeat screen with "Reintentar" (#48) will listen to the same Died event.
    [RequireComponent(typeof(Health))]
    public class PlayerDeathHandler : MonoBehaviour
    {
        [SerializeField] Behaviour[] disableOnDeath;

        Health health;

        void Awake()
        {
            health = GetComponent<Health>();
        }

        void OnEnable() => health.Died += OnDied;
        void OnDisable() => health.Died -= OnDied;

        void OnDied()
        {
            foreach (var behaviour in disableOnDeath)
                if (behaviour != null) behaviour.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Debug.Log("[PlayerDeathHandler] El Padre murió.");
        }
    }
}
