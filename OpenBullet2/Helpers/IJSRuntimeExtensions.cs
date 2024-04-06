using Microsoft.JSInterop;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace OpenBullet2.Helpers;

public static class IJSRuntimeExtensions
{
    public static async Task Log(this IJSRuntime js, string message)
        => await js.InvokeVoidAsync("console.log", message);

    public static async Task LogObject(this IJSRuntime js, object obj)
        => await js.InvokeVoidAsync("console.log", obj);

    public static async Task LogColored(this IJSRuntime js, string message, string foreground = "#000",
        string background = "#fff")
        => await js.InvokeVoidAsync("console.log", $"%c {message} ",
            $"background: {background}; color: {foreground}; padding: 5px 0px");

    public static async Task Alert(this IJSRuntime js, string title, string message)
        => await js.InvokeVoidAsync("Swal.fire", title, message, "info");

    public static async Task AlertSuccess(this IJSRuntime js, string title, string message)
        => await js.InvokeVoidAsync("Swal.fire", title, message, "success");

    public static async Task AlertError(this IJSRuntime js, string title, string message)
        => await js.InvokeVoidAsync("Swal.fire", title, message, "error");

    public static async Task AlertWarning(this IJSRuntime js, string title, string message)
        => await js.InvokeVoidAsync("Swal.fire", title, message, "warning");

    public static async Task AlertException(this IJSRuntime js, Exception ex)
    {
#if (DEBUG)
        await js.Log(ex.ToString());
#endif
        await js.AlertError(ex.GetType().Name, ex.Message);
    }

    public static async Task<bool> Confirm(this IJSRuntime js, string question, string message,
        string cancelText = "Cancel")
    {
        var options = new {
            title = question,
            text = message,
            icon = "warning",
            showCancelButton = true,
            confirmButtonColor = "#3085d6",
            cancelButtonColor = "#d33",
            cancelButtonText = cancelText,
            confirmButtonText = "OK"
        };

        var result = await js.InvokeAsync<object>("Swal.fire", options);
        var obj = JObject.Parse(result.ToString());

        if (obj.TryGetValue("value", out var value))
            return value.ToObject<bool>();

        return false;
    }

    public static async Task CopyToClipboard(this IJSRuntime js, string text)
        => await js.InvokeVoidAsync("navigator.clipboard.writeText", text);

    public static async Task RegisterLoliCode(this IJSRuntime js)
        => await js.InvokeVoidAsync("registerLoliCode");

    public static async Task RegisterLoliScript(this IJSRuntime js)
        => await js.InvokeVoidAsync("registerLoliScript");
}