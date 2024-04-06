using RuriLib.Legacy.LS;
using RuriLib.Models.Bots;
using System.Collections.Generic;

namespace RuriLib.Legacy.Models;

public class LSGlobals
{
    public LSGlobals(BotData data)
    {
        BotData = data;
    }

    public BotData BotData { get; set; }
    public VariablesList Globals { get; set; } = new();
    public Dictionary<string, string> GlobalCookies { get; set; } = new();
}