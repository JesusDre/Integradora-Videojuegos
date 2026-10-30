using System.IO;
using UnityEngine;

namespace Animas.Save
{
    // One save slot (GDD §26: MVP has one), written as JSON in Application.persistentDataPath.
    // Static because there is only one file and every system can reach it without a reference.
    public static class SaveSystem
    {
        const string FileName = "partida.json";

        public static string FilePath => Path.Combine(Application.persistentDataPath, FileName);
        public static bool HasSave => File.Exists(FilePath);

        public static void Save(SaveData data)
        {
            // Write to a temporary file first and then swap it in: if the game closes mid-write,
            // the previous save is still intact.
            string temp = FilePath + ".tmp";
            File.WriteAllText(temp, JsonUtility.ToJson(data, true));
            if (File.Exists(FilePath)) File.Delete(FilePath);
            File.Move(temp, FilePath);
        }

        public static SaveData Load()
        {
            if (!HasSave) return null;
            try
            {
                return JsonUtility.FromJson<SaveData>(File.ReadAllText(FilePath));
            }
            catch (System.Exception e)
            {
                // A broken file must not crash the game: report it and behave as if there were no save.
                Debug.LogWarning($"[SaveSystem] Could not read the save: {e.Message}");
                return null;
            }
        }

        public static void Delete()
        {
            if (HasSave) File.Delete(FilePath);
        }
    }
}
