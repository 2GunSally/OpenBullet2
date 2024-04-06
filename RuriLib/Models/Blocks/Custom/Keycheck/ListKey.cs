﻿using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Conditions.Comparisons;

namespace RuriLib.Models.Blocks.Custom.Keycheck;

public class ListKey : Key
{
    public ListKey()
    {
        Left = BlockSettingFactory.CreateListOfStringsSetting(string.Empty, null, SettingInputMode.Variable);
        Right = BlockSettingFactory.CreateStringSetting(string.Empty);
    }

    public ListComparison Comparison { get; set; } = ListComparison.Contains;
}