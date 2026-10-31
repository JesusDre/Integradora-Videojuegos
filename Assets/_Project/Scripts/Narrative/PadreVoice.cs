using System.Collections.Generic;
using Animas.Core;
using Animas.Items;
using Animas.Levels;
using Animas.Player;
using Animas.Save;
using Animas.UI;
using UnityEngine;

namespace Animas.Narrative
{
    // The Padre's comments (GDD §16: subtitles of what he says). It only listens to events the game already
    // sends and shows a line from PadreLines; it never changes the game.
    public class PadreVoice : MonoBehaviour
    {
        [SerializeField] PadreLines lines;
        [SerializeField] Health health;
        [SerializeField] PlayerPrayer prayer;
        [SerializeField] PlayerRespawn respawn;
        [SerializeField] KeyItemBag keyItems;
        [Tooltip("Life at or below this triggers a low-health line.")]
        [SerializeField, Min(1f)] float lowHealthAt = 30f;
        [Tooltip("Seconds before the low-health line can play again.")]
        [SerializeField, Min(0f)] float lowHealthCooldown = 60f;

        SubtitleUI subtitles;
        RitualController ritual;
        readonly HashSet<string> visitedRooms = new HashSet<string>();
        float nextLowHealth;
        bool prayedOnce;
        int lastKeyCount;

        void Start()
        {
            // The HUD and the ritual are separate objects in each scene; found once here.
            subtitles = FindAnyObjectByType<SubtitleUI>(FindObjectsInactive.Include);
            ritual = FindAnyObjectByType<RitualController>();
            health.Changed += OnHealthChanged;
            prayer.Completed += OnPrayed;
            respawn.Respawned += OnRespawned;
            keyItems.Changed += OnKeyItems;
            SafeRoom.Saved += OnSafeRoom;
            if (ritual != null) ritual.StageChanged += OnRitualStage;
            lastKeyCount = TotalKeyItems();
        }

        void OnDestroy()
        {
            if (health != null) health.Changed -= OnHealthChanged;
            if (prayer != null) prayer.Completed -= OnPrayed;
            if (respawn != null) respawn.Respawned -= OnRespawned;
            if (keyItems != null) keyItems.Changed -= OnKeyItems;
            SafeRoom.Saved -= OnSafeRoom;
            if (ritual != null) ritual.StageChanged -= OnRitualStage;
        }

        void Say(string line)
        {
            if (subtitles != null && !string.IsNullOrEmpty(line)) subtitles.Show(line);
        }

        void OnHealthChanged(float current, float max)
        {
            if (current <= 0f || current > lowHealthAt || Time.time < nextLowHealth) return;
            nextLowHealth = Time.time + lowHealthCooldown;
            Say(PadreLines.Pick(lines.lowHealth));
        }

        void OnPrayed()
        {
            if (prayedOnce) return;
            prayedOnce = true;
            Say(PadreLines.Pick(lines.firstPrayer));
        }

        void OnRespawned() => Say(PadreLines.Pick(lines.respawn));

        // Only the first save in each room: entering and leaving both save, and repeating would tire.
        void OnSafeRoom(SafeRoom room)
        {
            if (!visitedRooms.Add(room.Id)) return;
            Say(PadreLines.Pick(lines.safeRoom));
        }

        // A new key item (the count went up): the line for the 1st, 2nd or 3rd. Placing them in the anchor
        // lowers the count, and a respawn may restore it; neither should talk.
        void OnKeyItems()
        {
            int count = TotalKeyItems();
            // "Continuar" restores the bag right after loading: that isn't a new find.
            bool justLoaded = Time.timeSinceLevelLoad < 1f;
            if (!justLoaded && count > lastKeyCount && count <= lines.keyItems.Length) Say(lines.keyItems[count - 1]);
            lastKeyCount = count;
        }

        void OnRitualStage(RitualController.Stage stage)
        {
            if (stage == RitualController.Stage.Praying) Say(PadreLines.Pick(lines.ritualStart));
            else if (stage == RitualController.Stage.Complete) Say(PadreLines.Pick(lines.ritualDone));
        }

        int TotalKeyItems()
        {
            int total = 0;
            foreach (var pair in keyItems.KeyItems) total += pair.Value;
            return total;
        }
    }
}
