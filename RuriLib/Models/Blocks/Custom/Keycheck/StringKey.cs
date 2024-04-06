using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Conditions.Comparisons;

namespace RuriLib.Models.Blocks.Custom.Keycheck;

public class StringKey : Key
{
    public StringKey()
    {
        Left = BlockSettingFactory.CreateStringSetting(string.Empty, string.Empty, SettingInputMode.Variable);
        Left.InputVariableName = "data.SOURCE";
        Right = BlockSettingFactory.CreateStringSetting(string.Empty);
    }

    public StrComparison Comparison { get; set; } = StrComparison.Contains;
}