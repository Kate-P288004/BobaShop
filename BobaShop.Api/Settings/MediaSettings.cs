// -----------------------------------------------------------------------------
// File: Settings/MediaSettings.cs
// -----------------------------------------------------------------------------
namespace BobaShop.Api.Settings
{
    public record DrinkPreset(string Key, string Title, string Url);

    public class MediaSettings
    {
        public List<DrinkPreset> DrinkPresets { get; set; } = new();
    }
}
