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
        // Set when an instance starts from its entrance: PlayerRespawn gives the Padre his luggage.
        public static bool PendingNewInstance { get; private set; }
        // The final victory sends the player to the menu with the credits open (GDD §17).
        public static bool ShowCreditsOnMenu { get; set; }

        public static void LoadMenu(InstanceCatalog catalog)
        {
            ClearStart();
            Load(catalog.menuScene);
        }

        public static void StartInstance(InstanceCatalog.Entry instance)
        {
            PendingContinue = false;
            PendingNewInstance = true;
            Load(instance.sceneName);
        }

        // Which entry of the catalog the open scene is (-1 if it's not an instance, like the test arena).
        public static int CurrentInstanceIndex(InstanceCatalog catalog)
        {
            string scene = SceneManager.GetActiveScene().name;
            for (int i = 0; i < catalog.instances.Length; i++)
                if (catalog.instances[i].sceneName == scene) return i;
            return -1;
        }

        // Returns false if there's no save or its scene isn't in the build anymore.
        public static bool Continue()
        {
            SaveData data = SaveSystem.Load();
            if (data == null || !CanLoad(data.scene)) return false;
            PendingContinue = true;
            PendingNewInstance = false;
            Load(data.scene);
            return true;
        }

        // Called by PlayerRespawn once it used the flags, so a later scene load starts fresh.
        public static void ClearStart()
        {
            PendingContinue = false;
            PendingNewInstance = false;
        }

        public static bool CanLoad(string sceneName) =>
            !string.IsNullOrEmpty(sceneName) && Application.CanStreamedLevelBeLoaded(sceneName);

        static void Load(string sceneName)
        {
            // Leaving a paused game: time and sound must run again in the next scene.
            Time.timeScale = 1f;
            AudioListener.pause = false;
            // The input actions live in a project asset that survives scene changes: a menu that
            // switched the Padre's actions off must not leave them off in the next scene.
            UnityEngine.InputSystem.InputSystem.actions.FindActionMap("Player", true).Enable();
            SceneManager.LoadScene(sceneName);
        }
    }
}
