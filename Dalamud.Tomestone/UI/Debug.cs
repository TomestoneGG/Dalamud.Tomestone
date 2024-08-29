using ImGuiNET;

namespace Dalamud.Tomestone.UI
{
    internal static class Debug
    {
        internal static void Draw(PluginUI r)
        {
            bool firstLaunch = Tomestone.T.Configuration.IsFirstLaunch;

            ImGui.TextWrapped(r.debug_welcome);

            ImGui.Separator();

            if (ImGui.CollapsingHeader(r.debug_header_generalInformation))
            {
                ImGui.TextWrapped($"Tomestone is currently {(Tomestone.T.Configuration.Enabled ? "enabled" : "disabled")}.");
                ImGui.TextWrapped($"Tomestone is currently {(Tomestone.T.Configuration.SendActivity ? "sending activity data" : "not sending activity data")}.");
                ImGui.TextWrapped($"Tomestone is currently {(firstLaunch ? "running for the first time" : "not running for the first time")}.");
            }

            if (ImGui.CollapsingHeader(r.debug_header_dalamudInformation))
            {
                ImGui.TextWrapped($"Your Dalamud access token is currently set to: {Tomestone.T.Configuration.DalamudToken}");
            }

            if (ImGui.CollapsingHeader(r.debug_header_remoteConfig))
            {
                ImGui.TextWrapped($"Enabled?: {Tomestone.T.Configuration.RemoteConfig.enabled}");
                ImGui.TextWrapped($"Send Activity?: {Tomestone.T.Configuration.RemoteConfig.sendActivity}");
                ImGui.TextWrapped($"Send Gear?: {Tomestone.T.Configuration.RemoteConfig.sendGearSets}");
                ImGui.TextWrapped($"Send Triad?: {Tomestone.T.Configuration.RemoteConfig.sendTripleTriad}");
                ImGui.TextWrapped($"Send Orchestrion?: {Tomestone.T.Configuration.RemoteConfig.sendOrchestrionRolls}");
                ImGui.TextWrapped($"Send Blue Mage?: {Tomestone.T.Configuration.RemoteConfig.sendBlueMageSpells}");
            }

            if (ImGui.CollapsingHeader(r.debug_header_settings))
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
                ImGui.Separator();
                if (ImGui.Button("Set Character Unclaimed"))
                {
                    Tomestone.T.Configuration.CharacterClaimed = false;
                    Tomestone.T.Configuration.Save();
                }
            }

            // TODO: Add more debug actions here
            if (ImGui.CollapsingHeader(r.debug_header_actions))
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
