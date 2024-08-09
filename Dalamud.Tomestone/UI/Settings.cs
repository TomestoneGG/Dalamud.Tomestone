using Dalamud.Interface.Components;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

                if (ImGui.InputText("Dalamud Access Token", ref dalamudToken, 64, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    Tomestone.T.Configuration.DalamudToken = dalamudToken;
                    Tomestone.T.Configuration.Save();
                }
                ImGuiComponents.HelpMarker("This is your Dalamud access token. You can generate it in the Tomestone settings under the 'Dalamud access token' section.");
            }

            if (Tomestone.T.Configuration.Enabled && !string.IsNullOrEmpty(Tomestone.T.Configuration.DalamudToken))
            {
                if (ImGui.CollapsingHeader("Data Settings"))
                {
                    ImGui.TextWrapped("Here you can change some settings regarding the data Tomestone will send.");
                    ImGui.Separator();

                    if (ImGui.Checkbox("Send activity data to Tomestone.gg", ref sendActivity))
                    {
                        Tomestone.T.Configuration.SendActivity = sendActivity;
                        Tomestone.T.Configuration.Save();
                    }
                    ImGuiComponents.HelpMarker("If enabled, Tomestone will send your activity data to Tomestone.gg. This includes your current job, level, zone and if you are traveling to another world.");

                    if (ImGui.Checkbox("Send gear data to Tomestone.gg", ref sendGear))
                    {
                        Tomestone.T.Configuration.SendGear = sendGear;
                        Tomestone.T.Configuration.Save();
                    }
                    ImGuiComponents.HelpMarker("If enabled, Tomestone will send your gear data to Tomestone.gg.");
                }
            }
        }
    }
}
