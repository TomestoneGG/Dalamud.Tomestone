using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace Dalamud.Tomestone;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public string DalamudToken { get; set; } = string.Empty;

    public bool Enabled { get; set; } = true;
    public bool SendActivity { get; set; } = true;

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Tomestone.PluginInterface.SavePluginConfig(this);
    }
}
