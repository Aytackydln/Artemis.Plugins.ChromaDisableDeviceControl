using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceProcess;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace Artemis.Plugins.ChromaSdkHacks;

public static class RazerChromaUtils
{
    private const string DevicesXml = "Devices.xml";
    private const string FileContent = """
                                       <?xml version="1.0" encoding="utf-8"?>
                                       <devices>
                                       </devices>
                                       """;

    public static async Task DisableDeviceControlAsync()
    {
        List<Task> tasks = [];
        ReplaceDevicesXml(tasks, GetChromaPath());
        ReplaceDevicesXml(tasks, GetChromaPath64());

        if (tasks.Count == 0)
        {
            return;
        }

        await Task.WhenAll(tasks.ToArray());

        RestartChromaService();
    }

    private static void ReplaceDevicesXml(List<Task> tasks, string? chromaPath)
    {
        if (chromaPath == null) return;

        var xmlFile = Path.Combine(chromaPath, DevicesXml);
        if (File.Exists(xmlFile))
        {
            var length = File.Open(xmlFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite).Length;
            if (length <= FileContent.Length)
            {
                return;
            }
        }
        tasks.Add(File.WriteAllTextAsync(xmlFile, FileContent));
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