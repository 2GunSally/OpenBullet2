using RuriLib.Services;

namespace RuriLib.Providers.Puppeteer;

public class DefaultPuppeteerBrowserProvider : IPuppeteerBrowserProvider
{
    public DefaultPuppeteerBrowserProvider(RuriLibSettingsService settings)
    {
        ChromeBinaryLocation = settings.RuriLibSettings.PuppeteerSettings.ChromeBinaryLocation;
    }

    public string ChromeBinaryLocation { get; }
}