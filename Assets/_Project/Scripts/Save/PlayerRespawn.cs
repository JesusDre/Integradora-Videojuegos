using System;
using System.Collections;
using Animas.Core;
using Animas.Levels;
using Animas.Player;
using UnityEngine;

namespace Animas.Save
{
    // After dying, the Padre comes back in the last safe room with what he had when it saved,
    // and at least 60 life (GDD §6). The defeat screen's "Reintentar" and the pause's "Reiniciar" call
    // Respawn. "Continuar" in the main menu uses it too, right after the level loads.
    public class PlayerRespawn : MonoBehaviour
    {
        [SerializeField] Health health;
        [SerializeField] PlayerDeathHandler deathHandler;
        [SerializeField] PlayerPersistence persistence;
        [SerializeField, Range(1f, 100f)] float minimumHealth = 60f;
        [Tooltip("0 = wait for the defeat screen (normal). More = come back by itself (scenes without menus).")]
        [SerializeField, Min(0f)] float autoRespawnSeconds = 0f;

        public event Action Respawned;

        CharacterController body;

        void Awake() => body = GetComponent<CharacterController>();

        void Start()
        {
            string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (SceneFlow.PendingContinue)
            {
                // "Continuar": back to the saved safe room, or to the entrance if the save is the
                // start of this instance (just finished the previous one).
                SceneFlow.ClearStart();
                SaveData data = SaveSystem.Load();
                if (data == null || data.scene != scene) return;
                SafeRoom room = SafeRoom.FindById(data.checkpoint);
                if (room != null)
                {
                    room.MarkAsLast();
                    Respawn();
                }
                else persistence.Restore(data, minimumHealth);
            }
            else if (SceneFlow.PendingNewInstance)
            {
                // Starting an instance from the menu or "Siguiente instancia": bring the luggage, if any.
                SceneFlow.ClearStart();
                SaveData carry = SaveSystem.LoadCarry(scene);
                if (carry != null) persistence.Restore(carry, minimumHealth);
            }
        }

        void OnEnable() => health.Died += OnDied;
        void OnDisable() => health.Died -= OnDied;

        void OnDied()
        {
            if (autoRespawnSeconds > 0f) StartCoroutine(RespawnLater());
        }

        IEnumerator RespawnLater()
        {
            yield return new WaitForSeconds(autoRespawnSeconds);
            Respawn();
        }

        public bool Respawn()
        {
            SafeRoom room = SafeRoom.Last;
            SaveData data = SaveSystem.Load();
            if (room == null || data == null)
            {
                Debug.LogWarning("[PlayerRespawn] No safe room reached yet: nothing to respawn at.");
                return false;
            }
            // The CharacterController overrides position changes while enabled.
            if (body != null) body.enabled = false;
            transform.SetPositionAndRotation(room.RespawnPoint.position, room.RespawnPoint.rotation);
            if (body != null) body.enabled = true;

            persistence.Restore(data, minimumHealth);
            deathHandler.Revive();
            Respawned?.Invoke();
            return true;
        }
    }
}
