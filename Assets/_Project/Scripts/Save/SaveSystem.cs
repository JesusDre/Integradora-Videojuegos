using System.IO;
using UnityEngine;

namespace Animas.Save
{
    // One save slot (GDD §26: MVP has one), written as JSON in Application.persistentDataPath.
    // Static because there is only one file and every system can reach it without a reference.
    // Besides the slot, finishing an instance writes a "luggage" file for the next one
    // (equipaje_<scene>.json): what the Padre carries into it (GDD P4).
    public static class SaveSystem
    {
        const string FileName = "partida.json";

        public static string FilePath => Path.Combine(Application.persistentDataPath, FileName);
        public static bool HasSave => File.Exists(FilePath);

        public static void Save(SaveData data) => Write(FilePath, data);
        public static SaveData Load() => Read(FilePath);

        static string CarryPath(string scene) => Path.Combine(Application.persistentDataPath, $"equipaje_{scene}.json");
        public static void SaveCarry(string scene, SaveData data) => Write(CarryPath(scene), data);
        public static SaveData LoadCarry(string scene) => Read(CarryPath(scene));

        static void Write(string path, SaveData data)
        {
            // Write to a temporary file first and then swap it in: if the game closes mid-write,
            // the previous save is still intact.
            string temp = path + ".tmp";
            File.WriteAllText(temp, JsonUtility.ToJson(data, true));
            if (File.Exists(path)) File.Delete(path);
            File.Move(temp, path);
        }

        static SaveData Read(string path)
        {
            if (!File.Exists(path)) return null;
            try
            {
                return JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
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
