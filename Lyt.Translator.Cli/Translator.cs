
namespace Lyt.Translator.Cli;

using CommunityToolkit.Mvvm.Messaging;

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
        new Tuple<Type, Type>(typeof(ILogger), typeof(ConsoleLogger)),
        new Tuple<Type, Type>(typeof(IDispatch), typeof(NullDispatcher)),
        new Tuple<Type, Type>(typeof(IProfiler), typeof(Profiler)),
    ]),
    IRecipient<BeginSourceLanguageMessage>,
    IRecipient<BeginTargetLanguageMessage>,
    IRecipient<TranslationAddedMessage>,
    IRecipient<TranslationCompleteMessage>
{
    public const string Organization = "Lyt";
    public const string Application = "Translator.Cli";
    public const string RootNamespace = "Lyt.Translator.Cli";
    public const string AssemblyName = "Lyt.Translator.Cli";
    public const string AssetsFolder = "Assets";

    protected override async void OnStartupBegin()
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

    public async Task RunAsync(string[] parameters)
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
            Print("Loading: " + path);
            string serialized = File.ReadAllText(path);
            var fileManager = GetRequiredService<FileManagerModel>();
            var deserialized = fileManager.Deserialize<Project>(serialized);
            if (deserialized is Project project)
            {
                await this.RunProjectAsync(project);
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

    private async Task RunProjectAsync(Project project)
    {
        this.Subscribe<BeginSourceLanguageMessage>();
        this.Subscribe<BeginTargetLanguageMessage>();
        this.Subscribe<TranslationAddedMessage>();
        this.Subscribe<TranslationCompleteMessage>();

        string sourcePath = project.SourceFilePath();
        if (!File.Exists(sourcePath))
        {
            Print("Source language file does not exist. Path: " + sourcePath);
            return;
        }

        var translatorModel = GetRequiredService<TranslatorModel>();
        translatorModel.ActiveProject = project;
        translatorModel.PrepareForRunningProject(out string message);
        if (!string.IsNullOrWhiteSpace(message))
        {
            Print("Failed to prepare project: " + message + "  - Path: " + sourcePath);
            return;
        }
        _ = await translatorModel.RunProject();
        return;
    }

    public void Receive(BeginSourceLanguageMessage message)
        => Print("Begin Source Language: " +
            string.Concat(message.EnglishName, "  ~  ", message.LocalName));

    public void Receive(BeginTargetLanguageMessage message)
        => Print("Begin Target Language: " +
            string.Concat(message.EnglishName, "  ~  ", message.LocalName));

    public void Receive(TranslationAddedMessage message)
        => Print("Translation Added: " + message.SourceText + "  =>  " + message.TargetText);

    public void Receive(TranslationCompleteMessage message)
        => Print((message.Aborted ? "*** Run Project: Aborted *** " : "Run  Project Complete"));
}
