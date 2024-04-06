using BlazorMonaco;
using OpenBullet2.Core.Models.Settings;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenBullet2.Helpers;

public static class MonacoThemeSetter
{
    public static async Task SetLoliCodeTheme(CustomizationSettings settings)
    {
        await MonacoEditorBase.DefineTheme("vs-lolicode",
            new StandaloneThemeData {
                Base = settings.MonacoTheme,
                Inherit = true,
                Rules = new List<TokenThemeRule> {
                    new() { Token = "block", Foreground = "98CFFF" },
                    new() { Token = "block.end", Foreground = "98CFFF" },
                    new() { Token = "block.disabled", Foreground = "EC7063" },
                    new() { Token = "block.safe", Foreground = "BCFF70" },
                    new() { Token = "block.label", Foreground = "58D68D" },
                    new() { Token = "block.var", Foreground = "F7DC6F" },
                    new() { Token = "block.cap", Foreground = "F1948A" },
                    new() { Token = "block.arrow", Foreground = "BB8FCE" },
                    new() { Token = "block.interp", Foreground = "BB8FCE" },
                    new() { Token = "block.variable", Foreground = "FAD7A0" },
                    new() { Token = "block.customparam", Foreground = "A8FFD2" },
                    new() { Token = "string.variable", Foreground = "FAD7A0" },
                    new() { Token = "jumplabel", Foreground = "F78686" },
                    new() { Token = "key", Foreground = "6F81FF" },
                    new() { Token = "false", Foreground = "FF6347" },
                    new() { Token = "true", Foreground = "9ACD32" },
                    new() { Token = "keychain.success", Foreground = "9ACD32" },
                    new() { Token = "keychain.fail", Foreground = "FF6347" },
                    new() { Token = "keychain.retry", Foreground = "FFFF00" },
                    new() { Token = "keychain.ban", Foreground = "DDA0DD" },
                    new() { Token = "keychain.none", Foreground = "87CEEB" },
                    new() { Token = "keychain.default", Foreground = "FFA500" },
                    new() { Token = "script.delimiter", Foreground = "6F81FF" },
                    new() { Token = "script.output", Foreground = "FFBC80" }
                }
            });

        await MonacoEditorBase.SetTheme("vs-lolicode");
    }

    public static async Task SetLoliScriptTheme(CustomizationSettings settings)
    {
        await MonacoEditorBase.DefineTheme("vs-loliscript",
            new StandaloneThemeData {
                Base = settings.MonacoTheme,
                Inherit = true,
                Rules = new List<TokenThemeRule> {
                    new() { Token = "comment", Foreground = "808080" },
                    new() { Token = "disabled", Foreground = "FF6347" },
                    new() { Token = "variable", Foreground = "FFFF00" },
                    new() { Token = "capture", Foreground = "FF6347" },
                    new() { Token = "arrow", Foreground = "FF00FF" },
                    new() { Token = "number", Foreground = "8FBC8B" },
                    new() { Token = "string", Foreground = "ADD8E6" },
                    new() { Token = "block.label", Foreground = "FFDAB9" },
                    new() { Token = "block.function", Foreground = "ADFF2F" },
                    new() { Token = "block.keycheck", Foreground = "1E90FF" },
                    new() { Token = "block.keycheck.keychain", Foreground = "DDA0DD" },
                    new() { Token = "block.keycheck.keychain.key", Foreground = "87CEEB" },
                    new() { Token = "block.recaptcha", Foreground = "40E0D0" },
                    new() { Token = "block.request", Foreground = "32CD32" },
                    new() { Token = "block.request.header", Foreground = "DDA0DD" },
                    new() { Token = "block.request.cookie", Foreground = "87CEEB" },
                    new() { Token = "block.parse", Foreground = "FFD700" },
                    new() { Token = "block.parse.mode", Foreground = "FFA500" },
                    new() { Token = "block.captcha", Foreground = "FF8C00" },
                    new() { Token = "block.tcp", Foreground = "9370DB" },
                    new() { Token = "block.utility", Foreground = "F5DEB3" },
                    new() { Token = "block.navigate", Foreground = "4169E1" },
                    new() { Token = "block.browseraction", Foreground = "008000" },
                    new() { Token = "block.elementaction", Foreground = "B22222" },
                    new() { Token = "block.executejs", Foreground = "4B0082" },
                    new() { Token = "keyword", Foreground = "FF4500" },
                    new() { Token = "command", Foreground = "FFA500" }
                }
            });

        await MonacoEditorBase.SetTheme("vs-loliscript");
    }
}