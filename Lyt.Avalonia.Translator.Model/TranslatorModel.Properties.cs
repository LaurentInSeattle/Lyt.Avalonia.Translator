namespace Lyt.Avalonia.Translator.Model;

public sealed partial class TranslatorModel : ModelBase
{
    #region Serialized -  No model changed event

    [JsonRequired]
    public string Language { get => this.Get<string>()!; set => this.Set(value); } 

    /// <summary> This should stay true, ==> But... Just FOR NOW !  </summary>
    [JsonRequired]
    public bool IsFirstRun { get; set; } = false;

    [JsonRequired]
    public List<Provider> Providers { get; set; } = [];

    [JsonRequired]
    public List<Project> Projects { get; set; } = []; 

    #endregion Serialized -  No model changed event


    #region Not serialized - No model changed event

    [JsonIgnore]
    public bool PingComplete { get; set; } = false;

    [JsonIgnore]
    public bool ModelLoadedNotified { get; set; } = false;

    [JsonIgnore]
    public Project ActiveProject { get; set; } = new() { Name = "Empty" } ;

    [JsonIgnore]
    public ProviderKey ActiveProvider { get; set; } = ProviderKey.Google;

    #endregion Not serialized - No model changed event


    #region NOT serialized - WITH model changed event

    [JsonIgnore]
    // Asynchronous: Must raise Model Updated events 
    public bool IsInternetConnected { get => this.Get<bool>(); set => this.Set(value); }

    #endregion NOT serialized - WITH model changed event    

    //public Provider? MaybeProviderFromKey(ProviderKey key)
    //     => (from item in this.Providers
    //         where item.Key == key
    //         select item).FirstOrDefault();

    //public void UpdateProviderSelected(Provider provider, bool isSelected)
    //{
    //    var modelProvider =
    //        (from item in this.Providers
    //         where item.Key == provider.Key
    //         select item).FirstOrDefault();
    //    if (modelProvider is null)
    //    {
    //        return;
    //    }

    //    provider.IsSelected = isSelected;
    //    this.IsDirty = true;
    //}
}
