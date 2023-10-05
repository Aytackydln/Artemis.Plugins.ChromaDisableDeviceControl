using System.Collections.Generic;
using Artemis.Core;
using Artemis.Core.Modules;

namespace Artemis.Plugins.ChromaSdkHacks;

[PluginFeature(Name = "Disable Bloat")]
public class DisableBloatModule : Module
{
    private bool _afterLaunch;

    private readonly PluginSettings _pluginSettings;

    public DisableBloatModule(PluginSettings pluginSettings)
    {
        _pluginSettings = pluginSettings;
    }

    public override void Enable()
    {
        var manuallyEnabled = _pluginSettings.GetSetting("DisableBloat", false);
        if (!manuallyEnabled.Value && !_afterLaunch)
        {
            _afterLaunch = true;
            throw new ArtemisPluginException("Not manually enabled");
        }

        manuallyEnabled.Value = true;
        manuallyEnabled.Save();

        RazerChromaUtils.DisableChromaBloat();
    }

    public override void Disable()
    {
    }

    public override void Update(double deltaTime)
    {
    }

    public override List<IModuleActivationRequirement>? ActivationRequirements => null;
}