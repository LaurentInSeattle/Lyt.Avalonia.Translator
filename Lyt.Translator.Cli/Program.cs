namespace Lyt.Translator.Cli;

internal class Program
{
    static void Main(string[] args)
    {
        System.Console.WriteLine("Welcome to Lyt.Translator! Loading...");
        Run(args); 
        System.Console.ReadLine();
    }

    // Main cannot be async 
    static async void Run (string[] _ /* args*/ )
    {
        var translator = new Translator();
        string[] testArgs =
        [
            "C:\\Users\\Laurent\\Documents\\Lyt\\Translator\\AstroPic-Hindi.json"
        ];

        translator.Initialize();
        await translator.RunAsync(testArgs);
        await translator.Shutdown();
    }
}
