using System;
using UnityEngine;

namespace Animas.Core
{
    // The list the instance selection screen shows: name, description and which scene to load.
    // A ScriptableObject, so swapping a test scene for the final level is an Inspector change.
    [CreateAssetMenu(fileName = "InstanceCatalog", menuName = "Ánimas/Instance Catalog")]
    public class InstanceCatalog : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public string displayName = "Instancia";
            [TextArea(2, 4)] public string description;
            [Tooltip("Scene name as it appears in the Build Profiles scene list.")]
            public string sceneName;
        }

        public string menuScene = "MainMenu";
        public Entry[] instances = new Entry[0];
    }
}
