using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace SemanticKernel_101.Plugins
{
    internal class JokePlugin
    {
        [KernelFunction("TellJoke")]
        [Description("Tells a joke based on the given joke type. Valid types: programming, dad, one-liner.")]
        public string TellJoke(
            [Description("The joke type: programming, dad, or one-liner")] string jokeType)
        {
            return jokeType.Trim().ToLowerInvariant() switch
            {
                "programming" => "Why do programmers prefer dark mode? Because light attracts bugs.",
                "dad" => "Why don't eggs tell jokes? They'd crack each other up.",
                "one-liner" => "I told my wife she was drawing her eyebrows too high. She looked surprised.",
                _ => $"Unknown joke type '{jokeType}'. Please use programming, dad, or one-liner."
            };
        }

        [KernelFunction("GetJokeTypes")]
        [Description("Returns the list of available joke types the user can choose from.")]
        public string GetJokeTypes()
        {
            return "Available joke types: Programming, Dad Joke, One-Liner.";
        }
    }
}
