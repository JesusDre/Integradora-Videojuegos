using System;
using System.Collections.Generic;

namespace Animas.Save
{
    // Everything the game remembers (GDD §26), in a shape JsonUtility can write: plain fields and lists.
    // Items and recipes are stored by their asset name ("ITM_Venda"), and ItemDatabase turns the
    // names back into assets when loading.
    [Serializable]
    public class SaveData
    {
        public int version = 1;
        public string scene;          // which instance (Unity scene)
        public int section;           // 1 or 2
        public string checkpoint;     // safe room id, where the Padre respawns
        public float health;

        public List<string> slotItems = new List<string>();
        public List<int> slotCounts = new List<int>();
        public List<string> materialItems = new List<string>();
        public List<int> materialCounts = new List<int>();
        public List<string> recipes = new List<string>();
        // Filled by notes and key items (#32).
        public List<string> notes = new List<string>();
        public List<string> keyItems = new List<string>();        // one name per item carried
        public List<string> collectedKeyItems = new List<string>(); // ids of pickups already taken
    }
}
