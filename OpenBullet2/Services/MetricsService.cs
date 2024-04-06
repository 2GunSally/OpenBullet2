using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace OpenBullet2.Services;

public class MetricsService
{
    private readonly DateTime startTime = DateTime.Now;

    public MetricsService()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        BuildDate = new DateTime(2000, 1, 1)
            .AddDays(version.Build).AddSeconds(version.Revision * 2);
        BuildNumber = $"{version.Build}.{version.Revision}";
    }

    public string OS
    {
        get
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return Environment.OSVersion.VersionString;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return $"macOS {Environment.OSVersion.Version}";

            return RuntimeInformation.OSDescription;
        }
    }

    public TimeSpan UpTime => DateTime.Now - startTime;

    public string CWD => Directory.GetCurrentDirectory();

    public long MemoryUsage => Process.GetCurrentProcess().WorkingSet64;
    public double CpuUsage { get; private set; }

    public long NetUpload { get; private set; }

    public long NetDownload { get; private set; }

    public DateTime ServerTime => DateTime.Now;

    public string BuildNumber { get; private set; }
    public DateTime BuildDate { get; private set; }

    public async Task UpdateCpuUsage()
    {
        var startTime = DateTime.UtcNow;
        var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
        await Task.Delay(500);

        var endTime = DateTime.UtcNow;
        var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
        var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
        var totalMsPassed = (endTime - startTime).TotalMilliseconds;
        var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
        CpuUsage = cpuUsageTotal * 100;
    }

    public async Task UpdateNetworkUsage()
    {
        if (!NetworkInterface.GetIsNetworkAvailable())
            return;

        var interfaces = NetworkInterface.GetAllNetworkInterfaces();
        var startUpload = GetCurrentNetUpload(interfaces);
        var startDownload = GetCurrentNetDownload(interfaces);

        await Task.Delay(1000);
        NetUpload = GetCurrentNetUpload(interfaces) - startUpload;
        NetDownload = GetCurrentNetDownload(interfaces) - startDownload;
    }

    private static long GetCurrentNetUpload(NetworkInterface[] interfaces)
    {
        try
        {
            return interfaces.Select(i => i.GetIPv4Statistics().BytesSent).Sum();
        }
        catch
        {
            return 0;
        }
    }

    private static long GetCurrentNetDownload(NetworkInterface[] interfaces)
    {
        try
        {
            return interfaces.Select(i => i.GetIPv4Statistics().BytesReceived).Sum();
        }
        catch
        {
            return 0;
        }
    }
}