using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dalamud.Tomestone.UI
{
    internal static class Debug
    {
        internal static void Draw()
        {
            bool firstLaunch = Tomestone.T.Configuration.IsFirstLaunch;

            ImGui.TextWrapped($"Welcome to the debug menu! Here you can find some useful information and settings for debugging purposes.");

            ImGui.Separator();

            if (ImGui.CollapsingHeader("General Information"))
            {
                ImGui.TextWrapped($"Tomestone is currently {(Tomestone.T.Configuration.Enabled ? "enabled" : "disabled")}.");
                ImGui.TextWrapped($"Tomestone is currently {(Tomestone.T.Configuration.SendActivity ? "sending activity data" : "not sending activity data")}.");
                ImGui.TextWrapped($"Tomestone is currently {(firstLaunch ? "running for the first time" : "not running for the first time")}.");
            }

            if (ImGui.CollapsingHeader("Dalamud Information"))
            {
                ImGui.TextWrapped($"Your Dalamud access token is currently set to: {Tomestone.T.Configuration.DalamudToken}");
            }

            if (ImGui.CollapsingHeader("Debug Settings"))
            {
                if (ImGui.Button("Reset First Launch"))
                {
                    Tomestone.T.Configuration.IsFirstLaunch = true;
                    Tomestone.T.Configuration.Save();
                }
                ImGui.Separator();
                if (ImGui.Button("Reset Dalamud Token"))
                {
                    Tomestone.T.Configuration.DalamudToken = string.Empty;
                    Tomestone.T.Configuration.Save();
                }
                ImGui.Separator();
                if (ImGui.Button("Reset All Settings"))
                {
                    Tomestone.T.Configuration.Enabled = true;
                    Tomestone.T.Configuration.SendActivity = true;
                    Tomestone.T.Configuration.IsFirstLaunch = true;
                    Tomestone.T.Configuration.DalamudToken = string.Empty;
                    Tomestone.T.Configuration.Save();
                }
            }

            // TODO: Add more debug actions here
            if (ImGui.CollapsingHeader("Debug Actions"))
            {
                if (ImGui.Button("Send Test Activity"))
                {
                    //Tomestone.T.dataHandler....
                }
                ImGui.Separator();
                if (ImGui.Button("Send Test Gear"))
                {
                    //Tomestone.T.SendTestGear();
                }
            }
        }
    }
}
