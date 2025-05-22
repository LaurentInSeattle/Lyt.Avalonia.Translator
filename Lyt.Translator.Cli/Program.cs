namespace Lyt.Translator.Cli;

internal class Program
{
    static void Main(string[] args)
    {
        System.Console.WriteLine("Welcome to Lyt.Translator! Loading...");
        var translator = new Translator();

        translator.Initialize();
        translator.Run(args);
        translator.Shutdown();
    }
}
