using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace SemanticKernel_101.Plugins
{
    public class SportPlugin
    {
        [KernelFunction("SuggestSport")]
        [Description("Suggests a sport based on type. Valid types: indoor, outdoor.")]
        public string SuggestSport(
            [Description("The sport type: indoor or outdoor")] string sportType)
        {
            return sportType.Trim().ToLowerInvariant() switch
            {
                "indoor" => "Badminton - A fast-paced racquet sport perfect for indoor courts.",
                "outdoor" => "Cricket - A strategic team sport played on an open field.",
                _ => $"Unknown sport type '{sportType}'. Please use indoor or outdoor."
            };
        }

        [KernelFunction("GetSportTypes")]
        [Description("Returns the list of available sport categories the user can choose from.")]
        public string GetSportTypes()
        {
            return "Available sport types: Indoor, Outdoor.";
        }
    }
}
