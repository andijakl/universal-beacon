using UniversalBeacon.Library.Core.Entities;

namespace UniversalBeacon.Sample.Views;

/// <summary>
/// Selects the data template to display a beacon frame, based on the frame type.
/// </summary>
public class FrameTemplateSelector : DataTemplateSelector
{
    public DataTemplate? UidTemplate { get; set; }
    public DataTemplate? UrlTemplate { get; set; }
    public DataTemplate? TlmTemplate { get; set; }
    public DataTemplate? EncryptedTlmTemplate { get; set; }
    public DataTemplate? EidTemplate { get; set; }
    public DataTemplate? FindHubTemplate { get; set; }
    public DataTemplate? ProximityTemplate { get; set; }
    public DataTemplate? ManufacturerDataTemplate { get; set; }
    public DataTemplate? ServiceDataTemplate { get; set; }
    public DataTemplate? UnknownTemplate { get; set; }

    protected override DataTemplate? OnSelectTemplate(object item, BindableObject container) => item switch
    {
        UidEddystoneFrame => UidTemplate,
        UrlEddystoneFrame => UrlTemplate,
        TlmEddystoneFrame => TlmTemplate,
        EncryptedTlmEddystoneFrame => EncryptedTlmTemplate,
        EidEddystoneFrame => EidTemplate,
        FindHubNetworkFrame => FindHubTemplate,
        ProximityBeaconFrame => ProximityTemplate,
        ManufacturerDataFrame => ManufacturerDataTemplate,
        ServiceDataFrame => ServiceDataTemplate,
        _ => UnknownTemplate
    };
}
