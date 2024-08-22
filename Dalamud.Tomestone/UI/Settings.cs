using Dalamud.Interface.Components;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using System.Numerics;

namespace Dalamud.Tomestone.UI
{
    internal static class Settings
    {
        internal static void Draw()
        {
            bool enabled = Tomestone.T.Configuration.Enabled;
            string dalamudToken = Tomestone.T.Configuration.DalamudToken;

            bool sendActivity = Tomestone.T.Configuration.SendActivity;
            bool sendGear = Tomestone.T.Configuration.SendGear;
            bool sendTriad = Tomestone.T.Configuration.SendTriad;
            bool sendOrchestrion = Tomestone.T.Configuration.SendOrchestrion;

            ImGui.TextWrapped($"Here you can change some settings Tomestone will use.");
            ImGui.TextWrapped($"In order to use Tomestone, please claim your character and set your Dalamud access token first!");
            ImGui.Separator();

            if (ImGui.CollapsingHeader("General Settings"))
            {
                if (ImGui.Checkbox("Send data to Tomestone.gg", ref enabled))
                {
                    Tomestone.T.Configuration.Enabled = enabled;
                    Tomestone.T.Configuration.Save();
                }
                ImGuiComponents.HelpMarker("If enabled, Tomestone will send your character data to Tomestone.gg. This essentially enables/disables the plugin.");

                ImGui.Separator();

                var tokenText = "Dalamud Access Token";
                Vector4 textColor = new Vector4(0, 1, 0, 1);
                if (Tomestone.T.Configuration.TokenChecked)
                {
                    if (Tomestone.T.Configuration.TokenValid && Tomestone.T.Configuration.CharacterClaimed)
                    {
                        tokenText = "Dalamud Access Token (Valid)";
                    }
                    else
                    {
                        tokenText = Tomestone.T.Configuration.CharacterClaimed ? $"Dalamud Access Token (Invalid)" : $"Dalamud Access Token (Character Not Claimed).";
                        textColor = new Vector4(1, 0, 0, 1);
                    }
                } else
                {
                    tokenText = "Dalamud Access Token (Checking...)";
                    textColor = new Vector4(1, 1, 0, 1);
                }

                ImGui.PushStyleColor(ImGuiCol.Text, textColor);
                if (ImGui.InputText(tokenText, ref dalamudToken, 64, ImGuiInputTextFlags.None))
                {
                    Tomestone.T.Configuration.DalamudToken = dalamudToken;
                    Tomestone.T.Configuration.TokenChecked = false;
                    Tomestone.T.Configuration.TokenValid = false;
                    Tomestone.T.Configuration.CharacterClaimed = false;
                    Tomestone.T.Configuration.Save();
                }
                ImGui.PopStyleColor();
                ImGuiComponents.HelpMarker("This is your Dalamud access token. You can generate it in the Tomestone settings under the 'Dalamud access token' section.");
            }

            if (Tomestone.T.Configuration.Enabled && !string.IsNullOrEmpty(Tomestone.T.Configuration.DalamudToken))
            {
                if (ImGui.CollapsingHeader("Data Settings"))
                {
                    ImGui.TextWrapped("Here you can change some settings regarding the data Tomestone will collect.");
                    ImGui.Separator();

                    if (ImGui.Checkbox("Activity Data", ref sendActivity))
                    {
                        Tomestone.T.Configuration.SendActivity = sendActivity;
                        Tomestone.T.Configuration.Save();
                    }
                    ImGuiComponents.HelpMarker("If enabled, Tomestone collects your activity data. This includes your current job, level, zone and if you are traveling to another world. This is NOT displayed on your activity page, but used for the streams list.");

                    if (ImGui.Checkbox("Current gear", ref sendGear))
                    {
                        Tomestone.T.Configuration.SendGear = sendGear;
                        Tomestone.T.Configuration.Save();
                    }
                    ImGuiComponents.HelpMarker("If enabled, Tomestone collects your currently equipped gear.");

                    if (ImGui.Checkbox("Triple Triad card data", ref sendTriad))
                    {
                        Tomestone.T.Configuration.SendTriad = sendTriad;
                        Tomestone.T.Configuration.Save();
                    }
                    ImGuiComponents.HelpMarker("If enabled, Tomestone collects your Triple Triad card data.");

                    if (ImGui.Checkbox("Orchestrion roll data", ref sendOrchestrion))
                    {
                        Tomestone.T.Configuration.SendOrchestrion = sendOrchestrion;
                        Tomestone.T.Configuration.Save();
                    }
                    ImGuiComponents.HelpMarker("If enabled, Tomestone collects your Orchestrion roll data.");
                }
            }
        }
    }
}
