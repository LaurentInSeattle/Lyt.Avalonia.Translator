using Lyt.Avalonia.Translator.Model.Messaging;

namespace Lyt.Avalonia.Translator.Model;

public sealed partial class TranslatorModel : ModelBase
{
    /// <summary> Check if there is already an existing project with the same name as the provided project</summary>
    /// <returns> True if a matching name has been found. </returns>
    public bool CheckProjectExistence(Project project, out string errorMessageKey)
    {
        string projectName = project.Name;
        var alreadyExist =
            (from savedProject in this.Projects
             where savedProject.Name == projectName
             select savedProject)
            .FirstOrDefault();
        if (alreadyExist is not null)
        {
            errorMessageKey = "Model.Project.AlreadyExists";
            return true;
        }
        else
        {
            errorMessageKey = string.Empty;
            return false;
        }
    }

    /// <summary> Gets an existing project with the same name as the provided project name</summary>
    /// <returns> A project object if a matching name has been found, otherwise null. </returns>
    public Project? GetProjectByName(string projectName, out string errorMessageKey)
    {
        if (string.IsNullOrWhiteSpace(projectName))
        {
            errorMessageKey = "Model.Project.InvalidName";
            return null;
        }

        Project? alreadyExist =
            (from savedProject in this.Projects
             where savedProject.Name == projectName
             select savedProject)
            .FirstOrDefault();
        if (alreadyExist is not null)
        {
            errorMessageKey = string.Empty;
            return alreadyExist;
        }
        else
        {
            errorMessageKey = "Model.Project.DoesNotExist";
            return null;
        }
    }

    /// <summary> Adds a NON existing project.</summary>
    /// <returns> True if success . </returns>
    public bool AddNewProject(Project project, out string errorMessageKey)
    {
        if (!project.Validate(out errorMessageKey))
        {
            return false;
        }

        if (this.CheckProjectExistence(project, out errorMessageKey))
        {
            return false;
        }

        project.Created = DateTime.Now;
        project.LastUpdated = DateTime.Now;
        this.Projects.Add(project);
        this.Save();
        this.SaveProjectFile(project);
        return true;
    }

    /// <summary> Saves an existing or NON existing project.</summary>
    /// <returns> True if success. </returns>
    /// <remarks> If a project of same name exists, it will be replaced.</remarks>
    public bool SaveExistingProject(Project project, out string errorMessageKey)
    {
        if (!project.Validate(out errorMessageKey))
        {
            return false;
        }

        Project? maybeProject = this.GetProjectByName(project.Name, out errorMessageKey);
        if (maybeProject is Project existingProject)
        {
            this.Projects.Remove(existingProject);
        }
        else
        {
            return false;
        }

        project.LastUpdated = DateTime.Now;
        this.Projects.Add(project);
        this.Save();
        this.SaveProjectFile(project);
        return true;
    }

    /// <summary> Deletes an existing project with the same name as the provided project name.</summary>
    /// <returns> True is project has been deleted, otherwise false. </returns>
    public bool DeleteProject(string projectName, out string errorMessageKey)
    {
        Project? project = this.GetProjectByName(projectName, out errorMessageKey);
        if (project is not Project existingProject)
        {
            errorMessageKey = "Model.Project.DoesNotExist";
            return false;
        }

        this.Projects.Remove(existingProject);
        this.Save();
        return true;
    }

    public void SaveProjectFile(Project project)
    {
        try
        {
            var projectFileId =
                new FileId(FileManagerModel.Area.User, FileManagerModel.Kind.Json, project.Name);
            this.fileManager.Save(projectFileId, project);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }

    public Project? LoadProjectFile(string path)
    {
        try
        {
            string serialized = File.ReadAllText(path);
            object? deserialized = this.fileManager.Deserialize<Project>(serialized);
            if (deserialized is Project project)
            {
                return project;
            }

            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return null;
        }
    }

    public bool PrepareForRunningProject(out string errorMessage)
    {
        this.isReadyToRun = false;
        var currentProject = this.ActiveProject;
        if ((currentProject is null) || currentProject.IsInvalid)
        {
            errorMessage = "No active project";
            // errorMessage = this.Localizer.Lookup("RunProject.NoActiveProject");
            return false;
        }

        if (!this.LoadDictionaries(currentProject, out errorMessage))
        {
            return false;
        }

        if (!this.FindMissingTranslations(currentProject, out errorMessage))
        {
            return false;
        }

        this.isReadyToRun = true;
        return true;
    }

    public void AbortProject() => this.abortRequested = true;

    public int MissingEntriesCount(string key)
    {
        if (this.missingEntries.TryGetValue(key, out int entries))
        {
            return entries;
        }

        return 0;
    }

    public async Task<bool> RunProject()
    {
        var currentProject = this.ActiveProject;
        if (!this.isReadyToRun || (currentProject is null) || currentProject.IsInvalid)
        {
            // Error: no active project 
            return false;
        }

        this.abortRequested = false;

        // Loop through target languages 
        Language sourceLanguage = DataObjects.Language.Languages[currentProject.SourceLanguageCultureKey];
        string sourceLanguageKey = sourceLanguage.LanguageKey;

        // Begin source language 
        new BeginSourceLanguageMessage(sourceLanguageKey, sourceLanguage.EnglishName, sourceLanguage.LocalName).Publish();

        foreach (string cultureKey in currentProject.TargetLanguagesCultureKeys)
        {
            if (this.abortRequested)
            {
                this.isReadyToRun = false;
                new TranslationCompleteMessage(Aborted: true).Publish();
                return false;
            }

            // Loop through missing target language strings 
            Language targetLanguage = DataObjects.Language.Languages[cultureKey];

            string targetLanguageKey = targetLanguage.LanguageKey;
            var missingTranslations = this.needTranslationDictionaries[cultureKey];

            // Begin target language 
            this.Logger.Info("Begin target language " + targetLanguage.EnglishName);
            if (missingTranslations.Count > 0)
            {
                new BeginTargetLanguageMessage(cultureKey, targetLanguage.EnglishName, targetLanguage.LocalName).Publish();
            }
            else
            {
                this.Logger.Info("Target language " + targetLanguage.EnglishName + " is complete.");

                // Continue to next target language 
                continue;
            }

            void SaveAlreadyTranslated()
            {
                if (missingTranslations.Count > 0)
                {
                    _ = this.MergeAndSaveTranslations(cultureKey, missingTranslations);
                }
            }

            foreach (string targetKey in missingTranslations.Keys)
            {
                if (this.abortRequested)
                {
                    SaveAlreadyTranslated();
                    this.isReadyToRun = false;
                    new TranslationCompleteMessage(Aborted: true).Publish();
                    return false;
                }

                this.Logger.Info("Begin target Key:  " + targetKey  + "  for:  " + targetLanguage.EnglishName);
                string sourceText = this.sourceDictionary[targetKey];

                // DONT Call the translation service until the UI is complete 
                //
                // Use these lines below to debug the UI, if needed
                // bool success = true;
                // string translatedText = "Yolo - " + targetKey;

                var result =
                    await this.translatorService.Translate(
                        this.ActiveProvider, sourceText, sourceLanguageKey, targetLanguageKey);
                bool success = result.Item1;
                string translatedText = result.Item2;

                // Throttle to avoid overwhelming the service 
                await Task.Delay(666);

                if (success)
                {
                    missingTranslations[targetKey] = translatedText;

                    // Message Translation Added                    
                    new TranslationAddedMessage(
                        sourceLanguageKey, cultureKey,sourceText, translatedText).Publish();
                    // Delay so that the UI has a chance to update before the next service call
                    await Task.Delay(66);
                }
                else
                {
                    this.abortRequested = true;
                    SaveAlreadyTranslated();
                    this.isReadyToRun = false;
                    new TranslationCompleteMessage(Aborted: true).Publish();
                    return false;
                }
            }

            // Save translations in chosen format 
            if (this.MergeAndSaveTranslations(cultureKey, missingTranslations))
            {
                // Message Complete target language  
                // Dispatch.OnUiThread(() => { vm.SetComplete(0); });
                // Delay so that the UI has a chance to update before the next service call
                await Task.Delay(66);
            }
            else
            {
                this.isReadyToRun = false;
                new TranslationCompleteMessage(Aborted: true).Publish();
                return false;
            }
        }

        // Wait a bit so that last message has a chance to show up
        await Task.Delay(750); 

        // Complete 
        if (!this.abortRequested)
        {
            this.isReadyToRun = false;
            new TranslationCompleteMessage(Aborted: false).Publish();
            return true;
        }

        return false;
    }

    private bool LoadDictionaries(Project currentProject, out string errorMessage)
    {
        errorMessage = string.Empty;
        ResourceFormat resourceFormat = currentProject.Format;
        string sourcePath = currentProject.SourceFilePath();
        if (!File.Exists(sourcePath))
        {
            errorMessage = "RunProject.FailedLoadingSource";
            return false;
        }

        var sourceResult = TranslatorModel.ParseResourceFile(resourceFormat, sourcePath);
        bool sourceLoaded = sourceResult.Item1;
        if (!sourceLoaded)
        {
            errorMessage = "RunProject.FailedLoadingSource";
            return false;
        }

        this.sourceDictionary = sourceResult.Item2;
        this.targetDictionaries.Clear();

        // Loop through target languages 
        // Check whether of not we have existing translations, if we do load them 
        // if the file is not there create an empty dictionary 
        foreach (string cultureKey in currentProject.TargetLanguagesCultureKeys)
        {
            string targetPath = currentProject.TargetFilePath(cultureKey);
            Dictionary<string, string> targetDictionary = [];
            if (File.Exists(targetPath))
            {
                var targetResult = TranslatorModel.ParseResourceFile(resourceFormat, targetPath);
                if (targetResult.Item1)
                {
                    targetDictionary = targetResult.Item2;
                }
            }
            else
            {
                // This is fine: Add an empty dictionary 
            }

            this.targetDictionaries.Add(cultureKey, targetDictionary);
        }

        return true;
    }

    private bool FindMissingTranslations(Project currentProject, out string errorMessage)
    {
        errorMessage = string.Empty;
        this.needTranslationDictionaries.Clear();
        this.missingEntries.Clear();

        // Loop through the list of target languages to initialize the nested dictionary 
        foreach (string cultureKey in currentProject.TargetLanguagesCultureKeys)
        {
            this.needTranslationDictionaries.Add(cultureKey, []);
        }

        // Loop again through the keys of the source language to populate 
        foreach (string languageKey in this.sourceDictionary.Keys)
        {
            // Loop through target languages 
            foreach (string cultureKey in currentProject.TargetLanguagesCultureKeys)
            {
                var dictionary = this.targetDictionaries[cultureKey];
                var needed = this.needTranslationDictionaries[cultureKey];
                if (dictionary.TryGetValue(languageKey, out string? value))
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        // Empty translation: Add key and empty string
                        needed.Add(languageKey, string.Empty);
                    }

                    // We have a translation: dont change it
                }
                else
                {
                    // Missing translation: Add key and empty string
                    needed.Add(languageKey, string.Empty);
                }
            }
        }

        // Loop through the list of target languages to initialize the count of missing entries 
        foreach (string cultureKey in currentProject.TargetLanguagesCultureKeys)
        {
            int missingEntries = this.needTranslationDictionaries[cultureKey].Values.Count;
            this.missingEntries.Add(cultureKey, missingEntries);
        }

        return true;
    }

    private bool MergeAndSaveTranslations(
        string cultureKey, Dictionary<string, string> missingTranslations)
    {
        try
        {
            // Loop through the keys of the source language 
            Dictionary<string, string> mergedDictionary = [];
            var targetDictionary = this.targetDictionaries[cultureKey];
            foreach (string key in this.sourceDictionary.Keys)
            {
                // if we already have a translation keep it 
                bool done = false;
                if (targetDictionary.TryGetValue(key, out string? maybeTranslated))
                {
                    if (!string.IsNullOrEmpty(maybeTranslated))
                    {
                        mergedDictionary.Add(key, maybeTranslated);
                        done = true;
                    }
                }

                if (!done)
                {
                    // Check if we have it in the provided translations
                    if (missingTranslations.TryGetValue(key, out string? translated))
                    {
                        if (!string.IsNullOrEmpty(translated))
                        {
                            mergedDictionary.Add(key, translated);
                            done = true;
                        }
                    }
                }
            }

            if (mergedDictionary.Count > 0)
            {

                var currentProject = this.ActiveProject;
                ResourceFormat resourceFormat = currentProject.Format;
                string destinationPath = currentProject.TargetFilePath(cultureKey);
                TranslatorModel.CreateResourceFile(resourceFormat, destinationPath, mergedDictionary);
                return true;
            }

            // Empty ? Should never happen 
            if (Debugger.IsAttached) { Debugger.Break(); }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            if (Debugger.IsAttached) { Debugger.Break(); }
            return false;
        }
    }
}
