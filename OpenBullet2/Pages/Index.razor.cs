using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using OpenBullet2.Auth;
using OpenBullet2.Services;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace OpenBullet2.Pages;

public partial class Index : IDisposable
{
    private string announcement = string.Empty;
    private Timer cpuTimer;
    private Timer netTimer;

    private Timer timer;
    [Inject] private MetricsService Metrics { get; set; }
    [Inject] private AuthenticationStateProvider Auth { get; set; }
    [Inject] private NavigationManager Nav { get; set; }
    [Inject] private IHttpContextAccessor HttpAccessor { get; set; }
    [Inject] private AnnouncementService AnnouncementService { get; set; }
    [Inject] private PersistentSettingsService PersistentSettings { get; set; }

    public IPAddress IP { get; set; } = IPAddress.Parse("127.0.0.1");

    public void Dispose()
    {
        timer?.Dispose();
        cpuTimer?.Dispose();
        netTimer?.Dispose();
    }

    protected override void OnInitialized()
    {
        if (!PersistentSettings.SetupComplete) Nav.NavigateTo("/setup");

        StartPeriodicRefresh();

        Task.Run(() => announcement = AnnouncementService.FetchAnnouncement().Result);
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
            try
            {
                IP = HttpAccessor.HttpContext.Connection?.RemoteIpAddress;
            }
            catch
            {
                Console.WriteLine(
                    "Could not get the IP from the HttpAccessor. This might happen when OB2 is set up behind a reverse proxy");
            }
    }

    private void StartPeriodicRefresh()
    {
        timer = new Timer(async _ =>
        {
            await InvokeAsync(StateHasChanged);
        }, null, 1000, 1000);

        cpuTimer = new Timer(async _ =>
        {
            await Metrics.UpdateCpuUsage();
        }, null, 500, 500);

        netTimer = new Timer(async _ =>
        {
            await Metrics.UpdateNetworkUsage();
        }, null, 1000, 1000);
    }

    private async Task Logout()
    {
        await ((OBAuthenticationStateProvider)Auth).Logout();
        Nav.NavigateTo("/", true);
    }
}