using System.Collections.Generic;
using Artemis.Core;
using Artemis.Core.Modules;

namespace Artemis.Plugins.ChromaSdkHacks;

[PluginFeature(Name = "Disable Device Control")]
public class DisableDeviceControlModule : Module
{
    public override void Enable()
    {
        RazerChromaUtils.DisableDeviceControlAsync().Wait();
    }

    public override void Disable()
    {
    }

    public override void Update(double deltaTime)
    {
    }

    public override List<IModuleActivationRequirement>? ActivationRequirements => null;
}