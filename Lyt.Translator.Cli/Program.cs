namespace Lyt.Translator.Cli;

internal class Program
{
    static void Main(string[] args)
    {
        System.Console.WriteLine("Welcome to Lyt.Translator! Loading...");
        System.Console.WriteLine("Current directory: " + Environment.CurrentDirectory);
        Run(args); 
    }

    // Main cannot be async 
    static async void Run (string[] args )
    {
        try
        {
            var translator = new Translator();
            translator.Initialize();
            await translator.RunAsync(args);
            await Task.Delay(500);
            await translator.Shutdown();
        }
        catch (Exception ex) 
        {
            System.Console.WriteLine("Exception thrown: \n\n" + ex.ToString());
        }
    }
}
