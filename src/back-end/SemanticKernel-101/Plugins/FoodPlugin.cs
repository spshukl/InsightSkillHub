using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace SemanticKernel_101.Plugins
{
    internal class FoodPlugin
    {
        [KernelFunction("SuggestFood")]
        [Description("Suggests a popular dish based on the given cuisine type. Valid types: indian, continental, italian.")]
        public string SuggestFood(
            [Description("The cuisine type: indian, continental, or italian")] string cuisineType)
        {
            return cuisineType.Trim().ToLowerInvariant() switch
            {
                "indian" => "Biryani - A fragrant rice dish cooked with spices and meat.",
                "continental" => "Grilled Salmon - Pan-seared with herbs and lemon butter sauce.",
                "italian" => "Margherita Pizza - Classic Neapolitan pizza with fresh mozzarella and basil.",
                _ => $"Unknown cuisine type '{cuisineType}'. Please use indian, continental, or italian."
            };
        }

        [KernelFunction("GetCuisineOptions")]
        [Description("Returns the list of available cuisine types the user can choose from.")]
        public string GetCuisineOptions()
        {
            return "Available cuisine types: Indian, Continental, Italian.";
        }
    }
}
