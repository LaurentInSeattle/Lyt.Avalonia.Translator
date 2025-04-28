namespace Lyt.Avalonia.Translator.Service.Google;

/*
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
"spell":{ }}

*/

internal class Sentence
{
    [JsonPropertyName("trans")]
    public string? Translation { get; init; }
}

internal class Translation
{
    [JsonPropertyName("sentences")]
    public List<Sentence>? Sentences { get; init; }
}
