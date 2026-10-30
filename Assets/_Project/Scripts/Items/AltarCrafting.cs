namespace Animas.Items
{
    public enum CraftResult { Crafted, Locked, MissingIngredients, NoSpace }

    // The crafting rules, kept apart from the UI so they can be tested and explained alone:
    // 1) the recipe must be known, 2) every ingredient must be there, 3) the result must fit.
    // Only then are the ingredients spent and the object made. Nothing is lost if it fails.
    public static class AltarCrafting
    {
        public static bool HasIngredients(RecipeDefinition recipe, Inventory inventory)
        {
            foreach (var ingredient in recipe.ingredients)
                if (inventory.CountOf(ingredient.item) < ingredient.amount) return false;
            return true;
        }

        public static CraftResult TryCraft(RecipeDefinition recipe, Inventory inventory, RecipeBook book)
        {
            if (!book.Knows(recipe)) return CraftResult.Locked;
            if (!HasIngredients(recipe, inventory)) return CraftResult.MissingIngredients;

            // Spending an ingredient can free a space (the agua bendita for the chancla), so the room
            // check happens after taking them out; if the result still doesn't fit, they go back.
            foreach (var ingredient in recipe.ingredients) inventory.Remove(ingredient.item, ingredient.amount);
            if (!inventory.CanFit(recipe.result, recipe.resultAmount))
            {
                foreach (var ingredient in recipe.ingredients) inventory.TryAdd(ingredient.item, ingredient.amount, false);
                return CraftResult.NoSpace;
            }
            inventory.TryAdd(recipe.result, recipe.resultAmount, false);
            return CraftResult.Crafted;
        }
    }
}
