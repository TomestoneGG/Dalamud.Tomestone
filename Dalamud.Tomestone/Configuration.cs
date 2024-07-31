using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace Dalamud.Tomestone;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public string ApiKey { get; set; } = string.Empty;

    // This is in while testing to setup the API
    public string BaseUrl { get; set; } = "https://api.tomestone.gg/";
    public string StreamPath { get; set; } = "streams";

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
