using System.Text;
using Animas.Core;
using Animas.Enemies;
using Animas.Items;
using Animas.Player;
using UnityEngine;

namespace Animas.Testing
{
    // Plain text in the corner of test scenes: the Padre's life, stamina and inventory, and what each
    // cultist is doing. The real HUD is #22; this only exists to read numbers while testing.
    public class DebugOverlay : MonoBehaviour
    {
        [SerializeField] Health playerHealth;
        [SerializeField] PlayerStamina playerStamina;
        [SerializeField] Inventory playerInventory;
        [Tooltip("How often the cultist list is refreshed (they can be spawned at any time).")]
        [SerializeField, Min(0.1f)] float refreshSeconds = 0.25f;

        readonly StringBuilder text = new StringBuilder();
        CultistAI[] cultists = new CultistAI[0];
        float nextRefresh;
        GUIStyle style;

        void Update()
        {
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + refreshSeconds;
            cultists = FindObjectsByType<CultistAI>(FindObjectsSortMode.None);
            Build();
        }

        void Build()
        {
            text.Clear();
            text.AppendLine($"Vida {playerHealth.Current:0}/{playerHealth.Max:0}   Resistencia {playerStamina.Current:0}/{playerStamina.Max:0}{(playerStamina.IsExhausted ? " (agotado)" : "")}");

            text.Append("Objetos: ");
            for (int i = 0; i < playerInventory.Slots.Count; i++)
            {
                InventorySlot slot = playerInventory.Slots[i];
                text.Append(i < Inventory.QuickSlotCount ? $"[{i + 1}] " : "[ ] ");
                text.Append(slot.IsEmpty ? "—  " : $"{slot.item.displayName} ×{slot.count}  ");
            }
            text.AppendLine();

            foreach (CultistAI cultist in cultists)
            {
                Health health = cultist.GetComponent<Health>();
                string state = health.IsDead ? "muerto" : cultist.Current.ToString();
                text.AppendLine($"{cultist.name}: {state}  vida {health.Current:0}  a {Vector3.Distance(cultist.transform.position, playerHealth.transform.position):0.0} m");
            }
        }

        void OnGUI()
        {
            style ??= new GUIStyle(GUI.skin.label) { fontSize = 16, normal = { textColor = Color.white } };
            GUI.Box(new Rect(10, 10, 760, 30 + 20 * (2 + cultists.Length)), GUIContent.none);
            GUI.Label(new Rect(20, 15, 740, 20 * (3 + cultists.Length)), text.ToString(), style);
        }
    }
}
