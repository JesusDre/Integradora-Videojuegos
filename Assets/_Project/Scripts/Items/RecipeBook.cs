using System;
using System.Collections.Generic;
using UnityEngine;

namespace Animas.Items
{
    // The recipes the Padre knows. Some are known from the start; the rest are unlocked by reading
    // notes (#32). The save system (#26) stores this list.
    public class RecipeBook : MonoBehaviour
    {
        [SerializeField] RecipeDefinition[] knownAtStart;

        readonly HashSet<RecipeDefinition> known = new HashSet<RecipeDefinition>();

        public event Action<RecipeDefinition> Unlocked;

        void Awake()
        {
            foreach (RecipeDefinition recipe in knownAtStart)
                if (recipe != null) known.Add(recipe);
        }

        public bool Knows(RecipeDefinition recipe) => known.Contains(recipe);

        public void Unlock(RecipeDefinition recipe)
        {
            // HashSet.Add returns false if it was already known: no second "new recipe" message.
            if (recipe != null && known.Add(recipe)) Unlocked?.Invoke(recipe);
        }
    }
}
