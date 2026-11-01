using Animas.Entities;
using Animas.Save;
using UnityEngine;

namespace Animas.Levels
{
    // The escape from the hacienda (#162): when the Charro dissolves, a cultist horde comes after the
    // Padre and a glowing arrow shows the way to the gate. The escape has no checkpoint (GDD §6): if he
    // dies, he comes back in the safe room and the run starts again, with a fresh horde.
    public class EscapeRun : MonoBehaviour
    {
        [SerializeField] CharroNegroEntity charro;
        [SerializeField] WaveSpawner horde;
        [SerializeField] GuideArrow arrow;

        public bool Active { get; private set; }

        PlayerRespawn respawn;

        void Start()
        {
            if (charro != null) charro.Defeated += Begin;
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) respawn = player.GetComponent<PlayerRespawn>();
            if (respawn != null) respawn.Respawned += Restart;
            if (arrow != null) arrow.gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            if (charro != null) charro.Defeated -= Begin;
            if (respawn != null) respawn.Respawned -= Restart;
        }

        void Begin()
        {
            Active = true;
            if (horde != null) horde.Begin();
            if (arrow != null) arrow.gameObject.SetActive(true);
        }

        // Died during the escape: the old horde goes away and a new one comes for the new run.
        void Restart()
        {
            if (!Active || horde == null) return;
            horde.ResetWaves();
            horde.Begin();
        }
    }
}
