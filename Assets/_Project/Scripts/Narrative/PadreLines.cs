using UnityEngine;

namespace Animas.Narrative
{
    // What the Padre says when something happens (issue #33). The texts live in an asset so they can be
    // reviewed and changed without code; docs/TEXTOS.md keeps the same list for reading.
    [CreateAssetMenu(fileName = "PadreLines", menuName = "Ánimas/Padre Lines")]
    public class PadreLines : ScriptableObject
    {
        [TextArea] public string[] lowHealth = { "«Todavía no, Señor. Todavía no.»" };
        [TextArea] public string[] firstPrayer = { "«Que este círculo sea muralla.»" };
        [TextArea] public string[] respawn = { "«Levántate, Anselmo.»" };
        [TextArea] public string[] safeRoom = { "«Aquí no entran. Respira.»" };
        [Tooltip("One line per key item count: the 1st, 2nd and 3rd offering.")]
        [TextArea] public string[] keyItems = { "«Una ofrenda.»", "«Otra.»", "«Las tres. Ahora, al ancla.»" };
        [TextArea] public string[] ritualStart = { "«Padre nuestro, que estás en el cielo…»" };
        [TextArea] public string[] ritualDone = { "«Se acabó. Ahora hay que salir de aquí.»" };

        public static string Pick(string[] options) =>
            options == null || options.Length == 0 ? null : options[Random.Range(0, options.Length)];
    }
}
