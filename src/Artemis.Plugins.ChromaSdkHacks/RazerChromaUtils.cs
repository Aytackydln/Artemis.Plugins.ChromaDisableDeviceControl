using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceProcess;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace Artemis.Plugins.ChromaSdkHacks;

public static class RazerChromaUtils
{
    public static async Task DisableDeviceControlAsync()
    {
        const string file = """
                            <?xml version="1.0" encoding="utf-8"?>
                            <devices>
                            </devices>
                            """;

        List<Task> tasks = [];
        var chromaPath = GetChromaPath();
        if (chromaPath != null && File.Exists(chromaPath))
        {
            var length = File.Open(Path.Combine(chromaPath, "Devices.xml"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite).Length;
            if (length <= file.Length)
            {
                tasks.Add(File.WriteAllTextAsync(Path.Combine(chromaPath, "Devices.xml"), file));
            }
        }

        var chromaPath64 = GetChromaPath64();
        if (chromaPath64 != null && File.Exists(chromaPath64))
        {
            var length64 = File.Open(Path.Combine(chromaPath64, "Devices.xml"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite).Length;
            if (length64 <= file.Length)
            {
                tasks.Add(File.WriteAllTextAsync(Path.Combine(chromaPath64, "Devices.xml"), file));
            }
        }

        if (tasks.Count > 0)
        {
            return;
        }
        await Task.WhenAll(tasks.ToArray());

        RestartChromaService();
    }

    public static void DisableChromaBloat()
    {
        DisableService("Razer Chroma SDK Server");
        DisableService("Razer Chroma Stream Server");
        DisableService("Razer Central Service");
        DisableService("Razer Game Manager Service");
        DisableService("Razer Synapse Service");
    }

    private static void DisableService(string serviceName)
    {
        using var service = new ServiceController(serviceName);
        ServiceHelper.ChangeStartMode(service, ServiceStartMode.Manual);
        if (!service.CanStop) return;
        service.Stop();
        service.WaitForStatus(ServiceControllerStatus.Stopped);
    }

    private static void RestartChromaService()
    {
        using var service = new ServiceController("Razer Chroma SDK Service");
        if (service.Status == ServiceControllerStatus.Running)
        {
            try
            {
                service.Stop(true);
                service.WaitForStatus(ServiceControllerStatus.Stopped);
            }
            catch(Exception)
            {
                service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(2));
            }
        }

        if (service.Status != ServiceControllerStatus.Stopped) return;
        service.Start();
        service.WaitForStatus(ServiceControllerStatus.Running);
    }

    private static string? GetChromaPath()
    {
        using var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
        var key = hklm.OpenSubKey(@"Software\Razer Chroma SDK");
        var path = key?.GetValue("InstallPath", null) as string;
        return path;
    }

    private static string? GetChromaPath64()
    {
        using var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
        var key = hklm.OpenSubKey(@"Software\Razer Chroma SDK");
        var path = key?.GetValue("InstallPath64", null) as string;
        return path;
    }
}