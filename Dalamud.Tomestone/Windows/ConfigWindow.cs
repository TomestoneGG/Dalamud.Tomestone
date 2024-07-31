using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace Dalamud.Tomestone.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;
    private DataHandler dataHandler;

    // We give this window a constant ID using ###
    // This allows for labels being dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public ConfigWindow(Plugin plugin) : base("TomestoneGG###Settings")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(350, 150);
        SizeCondition = ImGuiCond.Always;

        Configuration = plugin.Configuration;
        dataHandler = plugin.dataHandler;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
            //Flags |= ~ImGuiWindowFlags.Move;
    }

    public override void Draw()
    {
        // can't ref a property, so use a local copy
        var apiKeyValue = Configuration.ApiKey;
        var _keyInputBuf = new byte[64];
        System.Text.Encoding.UTF8.GetBytes(apiKeyValue, 0, apiKeyValue.Length, _keyInputBuf, 0);
        if (ImGui.InputText("API Key", _keyInputBuf, (uint)_keyInputBuf.Length, ImGuiInputTextFlags.EnterReturnsTrue))
        {
            apiKeyValue = System.Text.Encoding.UTF8.GetString(_keyInputBuf).TrimEnd('\0');
            Configuration.ApiKey = apiKeyValue;
            Configuration.Save();
        }

        var _baseUrlInputBuf = new byte[64];
        System.Text.Encoding.UTF8.GetBytes(Configuration.BaseUrl, 0, Configuration.BaseUrl.Length, _baseUrlInputBuf, 0);
        if (ImGui.InputText("Base URL", _baseUrlInputBuf, (uint)_baseUrlInputBuf.Length, ImGuiInputTextFlags.EnterReturnsTrue))
        {
            Configuration.BaseUrl = System.Text.Encoding.UTF8.GetString(_baseUrlInputBuf).TrimEnd('\0');
            Configuration.Save();

            // remember to update the dataHandler with the new URL
            dataHandler.httpClient.BaseAddress = new Uri(Configuration.BaseUrl);
        }

        var _streamPathInputBuf = new byte[64];
        System.Text.Encoding.UTF8.GetBytes(Configuration.StreamPath, 0, Configuration.StreamPath.Length, _streamPathInputBuf, 0);
        if (ImGui.InputText("Stream Path", _streamPathInputBuf, (uint)_streamPathInputBuf.Length, ImGuiInputTextFlags.EnterReturnsTrue))
        {
            Configuration.StreamPath = System.Text.Encoding.UTF8.GetString(_streamPathInputBuf).TrimEnd('\0');
            Configuration.Save();
        }
    }
}
