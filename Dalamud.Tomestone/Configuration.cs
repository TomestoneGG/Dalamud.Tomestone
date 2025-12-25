using Dalamud.Configuration;
using Dalamud.Tomestone.API;
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
    public bool CharacterClaimed { get; set; } = true;

    #region Plugin settings
    public bool ModifyUI { get; set; } = true;
    #endregion

    #region Data settings
    public bool Enabled { get; set; } = true;
    public bool SendActivity { get; set; } = true;
    public bool SendGear { get; set; } = true;
    public bool SendTriad { get; set; } = true;
    public bool SendOrchestrion { get; set; } = true;
    public bool SendBlueMage { get; set; } = true;
    public bool SendChocoboBarding { get; set; } = true;
    #endregion

    public string ContextMenuButtonName { get; set; } = "Open Tomestone";

    [NonSerialized]
    internal RemoteConfigDTO RemoteConfig = new RemoteConfigDTO();

    public static readonly string VersionString = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";
    
    // TODO
    public void ManualSave()
    {
        // TODO: When saving manually, we will also check if the config should be saved to the server.
        Tomestone.PluginInterface.SavePluginConfig(this);
    }

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Tomestone.PluginInterface.SavePluginConfig(this);
    }
}
