// #define LYT_ONLY_DEBUG

namespace Lyt.Translator.Cli;

internal class Program
{
    // If main is made async, it must return a Task  
    static async Task Main(string[] args)
    {
        System.Console.WriteLine("Welcome to Lyt.Translator! Loading...");
        System.Console.WriteLine("Current directory: " + Environment.CurrentDirectory);
        await Run(args);
    }

    static async Task Run(string[] args)
    {
        try
        {
            var translator = new Translator();
            translator.Initialize();
#if LYT_ONLY_DEBUG
            string[] debugArgs = 
                [
                    @"C:\Users\Laurent\source\repos\Lyt.Avalonia.Translator\AstroPicLanguages.json"
                ];
            await translator.RunAsync(debugArgs);
#else
            await translator.RunAsync(args);
#endif
            await Task.Delay(500);
            await translator.Shutdown();
        }
        catch (Exception ex)
        {
            System.Console.WriteLine("Exception thrown: \n\n" + ex.ToString());
        }
    }
}
