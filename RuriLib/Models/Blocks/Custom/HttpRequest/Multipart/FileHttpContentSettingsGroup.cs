using RuriLib.Models.Blocks.Settings;

namespace RuriLib.Models.Blocks.Custom.HttpRequest.Multipart;

public class FileHttpContentSettingsGroup : HttpContentSettingsGroup
{
    public FileHttpContentSettingsGroup()
    {
        FileName = BlockSettingFactory.CreateStringSetting("fileName");
        ((StringSetting)ContentType.FixedSetting).Value = "application/octet-stream";
    }

    public BlockSetting FileName { get; set; }
}