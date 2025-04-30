namespace Lyt.Avalonia.Translator.Model.DataObjects;

public sealed class Language(
    string cultureKey, string languageKey,
    string englishName, string localName,
    string primaryFlag, string? secondaryFlag = null)
{
    public static readonly Dictionary<string, Language> Languages = new()
    {
        {  "en-US" , new Language( "en-US", "en", "English", "English", "United_Kingdom", "Canada") },
        {  "fr-FR" , new Language( "fr-FR", "fr", "French", "Français", "France", "Quebec") },
        {  "it-IT" , new Language( "it-IT", "it", "Italian", "Italiano", "Italy", "San_Marino") },
        {  "es-ES" , new Language( "es-ES", "es", "Spanish", "Español", "Spain", "Mexico") },

        {  "bg-BG" , new Language( "bg-BG", "bg", "Bulgarian", "български", "Bulgaria", "North_Macedonia") },
        {  "uk-UK" , new Language( "uk-UK", "uk", "Ukrainian", "українська мова", "Ukraine") },
        {  "zh-CN" , new Language( "zh-CN", "zh-CN", "Chinese (Simplified)", "簡體中文", "China") },
        {  "zh-TW" , new Language( "zh-TW", "zh-TW", "Chinese (Traditional)", "繁體中文", "Taiwan") },
        {  "el-EL" , new Language( "el-EL", "el", "Greek", "ελληνική", "Greece", "Cyprus") },

        // TODO: Add more definitions here and provide flags 
        // For flags, see: https://en.wikipedia.org/wiki/List_of_national_flags_of_sovereign_states
    };

    public string CultureKey { get; private set; } = cultureKey;

    public string LanguageKey { get; private set; } = languageKey;

    public string EnglishName { get; private set; } = englishName;

    public string LocalName { get; private set; } = localName;

    public string PrimaryFlag { get; private set; } = primaryFlag;

    public string? SecondaryFlag { get; set; } = secondaryFlag;

    #region Google Keys 

    /*
     
    private static readonly Dictionary<string, string> Languages =
    new()
    {
            {"af", "Afrikaans"},
            {"sq", "Albanian"},
            {"ar", "Arabic"},
            {"hy", "Armenian"},
            {"az", "Azerbaijani"},
            {"eu", "Basque"},
            {"be", "Belarusian"},
            {"bn", "Bengali"},
            {"bg", "Bulgarian"},
            {"ca", "Catalan"},
            {"zh-CN", "Chinese (Simplified)"},
            {"zh-TW", "Chinese (Traditional)"},
            {"hr", "Croatian"},
            {"cs", "Czech"},
            {"da", "Danish"},
            {"nl", "Dutch"},
            {"en", "English"},
            {"eo", "Esperanto"},
            {"et", "Estonian"},
            {"tl", "Filipino"},
            {"fi", "Finnish"},
            {"fr", "French"},
            {"gl", "Galician"},
            {"ka", "Georgian"},
            {"de", "German"},
            {"el", "Greek"},
            {"gu", "Gujarati"},
            {"ht", "Haitian Creole"},
            {"iw", "Hebrew"},
            {"hi", "Hindi"},
            {"hu", "Hungarian"},
            {"is", "Icelandic"},
            {"id", "Indonesian"},
            {"ga", "Irish"},
            {"it", "Italian"},
            {"ja", "Japanese"},
            {"kn", "Kannada"},
            {"km", "Khmer"},
            {"ko", "Korean"},
            {"lo", "Lao"},
            {"la", "Latin"},
            {"lv", "Latvian"},
            {"lt", "Lithuanian"},
            {"mk", "Macedonian"},
            {"ms", "Malay"},
            {"mt", "Maltese"},
            {"no", "Norwegian"},
            {"fa", "Persian"},
            {"pl", "Polish"},
            {"pt-PT", "Portuguese - Portugal"},
            {"pt-BR", "Portuguese - Brazil"},
            {"ro", "Romanian"},
            {"ru", "Russian"},
            {"sr", "Serbian"},
            {"sk", "Slovak"},
            {"sl", "Slovenian"},
            {"es", "Spanish"},
            {"sw", "Swahili"},
            {"sv", "Swedish"},
            {"ta", "Tamil"},
            {"te", "Telugu"},
            {"th", "Thai"},
            {"tr", "Turkish"},
            {"uk", "Ukrainian"},
            {"ur", "Urdu"},
            {"vi", "Vietnamese"},
            {"cy", "Welsh"},
            {"yi", "Yiddish"}
    };
    
    */
    #endregion Google Keys 

}
