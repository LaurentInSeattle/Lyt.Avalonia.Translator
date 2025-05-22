
using Lyt.Avalonia.Interfaces.Dispatch;
using Lyt.Console;
using Lyt.Messaging;
using System.IO;

namespace Lyt.Translator.Cli;

internal sealed class Translator() : ConsoleBase(
    Organization,
    Application,
    RootNamespace,
    typeof(ApplicationModelBase), // Top level model 
    [
        // Models 
        typeof(FileManagerModel),
        typeof(TranslatorModel),
    ],
    [
        // Singletons
        typeof(TranslatorService),
    ],
    [
        // Services 
        new Tuple<Type, Type>(typeof(ILogger), typeof(BasicLogger)),
        new Tuple<Type, Type>(typeof(IDispatch), typeof(NullDispatcher)),
        new Tuple<Type, Type>(typeof(IMessenger), typeof(Messenger)),

        //new Tuple<Type, Type>(typeof(ILocalizer), typeof(LocalizerModel)),
        //new Tuple<Type, Type>(typeof(IProfiler), typeof(Profiler)),
        //new Tuple<Type, Type>(typeof(IRandomizer), typeof(Randomizer)),
    ])
{
    public const string Organization = "Lyt";
    public const string Application = "Translator.Cli";
    public const string RootNamespace = "Lyt.Translator.Cli";
    public const string AssemblyName = "Lyt.Translator.Cli";
    public const string AssetsFolder = "Assets";

    protected override async void OnStartupComplete()
    {
        // This needs to complete before all models are initialized.
        var fileManager = GetRequiredService<FileManagerModel>();
        await fileManager.Configure(
            new FileManagerConfiguration(
                Translator.Organization, Translator.Application, Translator.RootNamespace,
                Translator.AssemblyName, Translator.AssetsFolder));
    }

    protected override async void OnShutdownBegin()
    {
        IApplicationModel applicationModel = GetRequiredService<IApplicationModel>();
        await applicationModel.Shutdown();
    }

    public void Run(string[] parameters)
    {
        if (parameters is null || parameters.Length == 0)
        {
            Print("No parameters provided. (Should be a JSon path name)");
            return;
        }

        if (parameters is null || parameters.Length > 1)
        {
            Print("Too many parameters provided. (Should be only one JSon path name)");
            return;
        }

        string path = parameters[0];
        if (!File.Exists(path))
        {
            Print("Provided JSon file does not exist. Path: " + path);
            return;
        }

        try
        {
            string serialized = File.ReadAllText(path);
            var fileManager = GetRequiredService<FileManagerModel>();
            var deserialized = fileManager.Deserialize<Project>(serialized);
            if (deserialized is Project project)
            {
                this.RunProject(project);
            }
            else
            {
                Print("Failed to deserialize provided JSon file. Path: " + path);
                return;
            }
        }
        catch (Exception ex)
        {
            Print("Failed to access provided JSon file. Exception: " + ex.Message);
            Print(" - Exception Details: " + ex);
        }

        return;
    }

    private void RunProject(Project project)
    {
        string sourcePath = project.SourceFilePath();
        if (!File.Exists(sourcePath))
        {
            Print("Source language file does not exist. Path: " + sourcePath);
            return;
        }
    }
}
