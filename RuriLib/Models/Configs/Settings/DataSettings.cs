using RuriLib.Models.Data.Resources.Options;
using RuriLib.Models.Data.Rules;
using System.Collections.Generic;

namespace RuriLib.Models.Configs.Settings;

public class DataSettings
{
    public string[] AllowedWordlistTypes { get; set; } = { "Default" };
    public bool UrlEncodeDataAfterSlicing { get; set; } = false;
    public List<DataRule> DataRules { get; set; } = new();
    public List<ConfigResourceOptions> Resources { get; set; } = new();

    public string AllowedWordlistTypesString => string.Join(", ", AllowedWordlistTypes);
}