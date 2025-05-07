namespace Lyt.Avalonia.Translator.Model.DataObjects;

public sealed class Language(
    string cultureKey, string languageKey,
    string englishName, string localName,
    string primaryFlag, string? secondaryFlag = null)
{
    public static readonly string DefaultCultureKey = "en-US";

    public static readonly Language Default =
        new(DefaultCultureKey, "en", "English", "English", "United_Kingdom", "Canada");

    public static readonly Dictionary<string, Language> Languages = new()
    {
        {  DefaultCultureKey , Default },
        {  "fr-FR" , new Language( "fr-FR", "fr", "French", "Français", "France", "Quebec") },
        {  "it-IT" , new Language( "it-IT", "it", "Italian", "Italiano", "Italy", "San_Marino") },
        {  "es-ES" , new Language( "es-ES", "es", "Spanish", "Español", "Spain", "Mexico") },
        {  "uk-UK" , new Language( "uk-UK", "uk", "Ukrainian", "Українська мова", "Ukraine") },
        {  "ja-JA" , new Language( "ja-JA", "ja", "Japanese", "日本語", "Japan") },
        {  "bg-BG" , new Language( "bg-BG", "bg", "Bulgarian", "Български език", "Bulgaria") },
        {  "zh-CN" , new Language( "zh-CN", "zh-CN", "Chinese (Simplified)", "簡體 中文", "China") },
        {  "zh-TW" , new Language( "zh-TW", "zh-TW", "Chinese (Traditional)", "繁體 中文", "Taiwan") },
        {  "el-EL" , new Language( "el-EL", "el", "Greek", "Ελληνικά", "Greece", "Cyprus") },
        {  "da-DA" , new Language( "da-DA", "da", "Danish", "Dansk", "Denmark") },
        {  "nl-NL" , new Language( "nl-NL", "nl", "Dutch", "Hollands", "Netherlands") },
        {  "de-DE" , new Language( "de-DE", "de", "German", "Deutsch", "Germany", "Austria") },
        {  "ka-KA" , new Language( "ka-KA", "ka", "Georgian", "ქართული ენა", "Georgia") },
        {  "hi-HI" , new Language( "hi-HI", "hi", "Hindi", "हिन्दी", "India") },
        {  "bn-BN" , new Language( "bn-BN", "bn", "Bengali", "বাঙ্গালী", "Bangladesh", "India" ) },
        {  "hu-HU" , new Language( "hu-HU", "hu", "Hungarian", "Magyar", "Hungary") },
        {  "ko-KO" , new Language( "ko-KO", "ko", "Korean", "한국인 - 조선어", "South_Korea", "North_Korea") },
        {  "pl-PL" , new Language( "pl-PL", "pl", "Polish", "Polski", "Poland") },
        {  "pt-PT" , new Language( "pt-PT", "pt-PT", "Portuguese", "Português", "Portugal", "Brazil") },
        {  "ro-RO" , new Language( "ro-RO", "ro", "Romanian", "Românesc", "Romania", "Moldova") },
        {  "th-TH" , new Language( "th-TH", "th", "Thai", "ภาษาไทย", "Thailand") },

        // TODO: Add more definitions here and provide flags 
        // For flags, see: https://en.wikipedia.org/wiki/List_of_national_flags_of_sovereign_states
    };

    public string CultureKey { get; private set; } = cultureKey;

    public string LanguageKey { get; private set; } = languageKey;

    public string EnglishName { get; private set; } = englishName;

    public string LocalName { get; private set; } = localName;

    public string PrimaryFlag { get; private set; } = primaryFlag;

    public string? SecondaryFlag { get; set; } = secondaryFlag;

    public static Language FromCultureKey(string cultureKey)
    {
        if (Languages.TryGetValue(cultureKey, out var language))
        {
            if (language is not null)
            {
                return language;
            }
        }

        throw new ArgumentException("Unsupported Culture");
    }

    #region Google Keys 

    /*

    Only the ones not yet in our Language list 
    
    private static readonly Dictionary<string, string> Languages =
    new()
    {
            {"af", "Afrikaans"},
            {"sq", "Albanian"},
            {"hy", "Armenian"},
            {"eu", "Basque"},
            {"ca", "Catalan"},
            {"hr", "Croatian"},
            {"cs", "Czech"},
            {"eo", "Esperanto"},
            {"et", "Estonian"},
            {"tl", "Filipino"},
            {"fi", "Finnish"},
            {"gl", "Galician"},
            {"gu", "Gujarati"},
            {"ht", "Haitian Creole"},
            {"is", "Icelandic"},
            {"id", "Indonesian"},
            {"ga", "Irish"},
            {"kn", "Kannada"},
            {"km", "Khmer"},
            {"lo", "Lao"},
            {"la", "Latin"},
            {"lv", "Latvian"},
            {"lt", "Lithuanian"},
            {"mk", "Macedonian"},
            {"ms", "Malay"},
            {"mt", "Maltese"},
            {"no", "Norwegian"},
            {"fa", "Persian"},
            {"sr", "Serbian"},
            {"sk", "Slovak"},
            {"sl", "Slovenian"},
            {"sw", "Swahili"},
            {"sv", "Swedish"},
            {"ta", "Tamil"},
            {"te", "Telugu"},
            {"ur", "Urdu"},
            {"cy", "Welsh"},
    };

    Right to Left
            {"yi", "Yiddish"}  ( ??? ) 
            {"iw", "Hebrew"},
            {"ar", "Arabic"},

    Banned
            {"az", "Azerbaijani"},
            {"be", "Belarusian"},
            {"ru", "Russian"},
            {"tr", "Turkish"},
    
    */
    #endregion Google Keys 

}
