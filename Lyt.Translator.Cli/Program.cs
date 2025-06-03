using System; 

namespace Lyt.Translator.Cli;

internal class Program
{
    static void Main(string[] args)
    {
        System.Console.WriteLine("Welcome to Lyt.Translator! Loading...");
        System.Console.WriteLine("Current directory: " + Environment.CurrentDirectory);
        Run(args); 
        System.Console.ReadLine();
    }

    // Main cannot be async 
    static async void Run (string[] args )
    {
        try
        {
            var translator = new Translator();
            translator.Initialize();

            //#if DEBUG
            //        if (Debugger.IsAttached)
            //        {
            //            string[] testArgs =
            //            [
            //                "C:\\Users\\Laurent\\Documents\\Lyt\\Translator\\AstroPic-Hindi.json"
            //            ];
            //            await translator.RunAsync(testArgs);
            //        }
            //#else
            //            await translator.RunAsync(args);

            //#endif
            await translator.RunAsync(args);
            await translator.Shutdown();
        }
        catch (Exception ex) 
        {
            System.Console.WriteLine("Exception thrown: \n\n" + ex.ToString());
        }
    }
}
