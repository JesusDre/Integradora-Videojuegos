using System;
using UnityEngine;

namespace Animas.Items
{
    // One altar recipe (GDD §18): what it needs and what it makes. An ingredient can be a material
    // (sal, botella) or an item from the 8 spaces (the chancla bendita needs an agua bendita).
    [CreateAssetMenu(fileName = "RCP_", menuName = "Ánimas/Recipe")]
    public class RecipeDefinition : ScriptableObject
    {
        [Serializable]
        public struct Ingredient
        {
            public ItemDefinition item;
            [Min(1)] public int amount;
        }

        public ItemDefinition result;
        [Min(1)] public int resultAmount = 1;
        public Ingredient[] ingredients;
    }
}
