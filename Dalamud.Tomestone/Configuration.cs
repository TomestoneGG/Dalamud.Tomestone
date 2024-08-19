using Dalamud.Configuration;
using Dalamud.Plugin;
using Dalamud.Utility;
using System;
using System.Reflection;

namespace Dalamud.Tomestone;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public bool IsFirstLaunch { get; set; } = true;

    public string DalamudToken { get; set; } = string.Empty;
    public bool TokenChecked { get; set; } = false;
    public bool TokenValid { get; set; } = false;
    public bool CharacterClaimed { get; set; } = false;

    // Enables/Disables all data sending to tomestone.gg
    public bool Enabled { get; set; } = true;
    // Enables/Disables sending activity data
    public bool SendActivity { get; set; } = true;
    // Enables/Disables sending current gear data
    public bool SendGear { get; set; } = true;
    // Enables/Disables sending triad card data
    public bool SendTriad { get; set; } = true;

    public static readonly string VersionString = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";
    public static readonly string DalamudVersion = Util.AssemblyVersion;

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Tomestone.PluginInterface.SavePluginConfig(this);
    }
}
