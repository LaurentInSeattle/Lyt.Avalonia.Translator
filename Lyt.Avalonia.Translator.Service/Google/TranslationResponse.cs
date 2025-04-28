namespace Lyt.Avalonia.Translator.Service.Google;

/*

Sample : 

    {
        "sentences":
        [
            {
                "trans":"Bonjour le monde",
                "orig":"Hello World",
                "backend":10
            }
        ],
        "src":"en",
        "spell":
        { 
        }
    }

*/

internal class Sentence
{
    [JsonPropertyName("trans")]
    public string? Translation { get; init; }
}

internal class TranslationResponse
{
    [JsonPropertyName("sentences")]
    public List<Sentence>? Sentences { get; init; }
}
