using Animas.Save;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Animas.Core
{
    // Moving between the menu and the instances. Static so any button can call it without references.
    public static class SceneFlow
    {
        // Set by "Continuar": when the level loads, PlayerRespawn puts the Padre in the saved safe room.
        public static bool PendingContinue { get; private set; }

        public static void LoadMenu(InstanceCatalog catalog)
        {
            PendingContinue = false;
            Load(catalog.menuScene);
        }

        public static void StartInstance(InstanceCatalog.Entry instance)
        {
            PendingContinue = false;
            Load(instance.sceneName);
        }

        // Returns false if there's no save or its scene isn't in the build anymore.
        public static bool Continue()
        {
            SaveData data = SaveSystem.Load();
            if (data == null || !CanLoad(data.scene)) return false;
            PendingContinue = true;
            Load(data.scene);
            return true;
        }

        // Called by PlayerRespawn once it used the flag, so a later scene load starts fresh.
        public static void ClearContinue() => PendingContinue = false;

        public static bool CanLoad(string sceneName) =>
            !string.IsNullOrEmpty(sceneName) && Application.CanStreamedLevelBeLoaded(sceneName);

        static void Load(string sceneName)
        {
            // Leaving a paused game: time and sound must run again in the next scene.
            Time.timeScale = 1f;
            AudioListener.pause = false;
            SceneManager.LoadScene(sceneName);
        }
    }
}
