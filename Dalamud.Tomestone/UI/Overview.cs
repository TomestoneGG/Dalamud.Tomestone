using ImGuiNET;

namespace Dalamud.Tomestone.UI
{
    static internal class Overview
    {
        static internal void Draw()
        {
            ImGui.TextWrapped($"Welcome to Tomestone!");
            ImGui.TextWrapped($"NOTE: This Plugin is currently in an early testing phase. If any issues arise or you want to leave feedback, you can do so on the Tomestone discord.");
            if (ImGui.Button("Join Tomestone Discord Server"))
            {
                Utils.OpenLink("https://discord.gg/ufeCvadedS");
            }
            ImGui.Separator();
            ImGui.TextWrapped($"This plugin is designed to help you track your character's progress and share it with others. By using the plugin, you will be able to seamlessly synch your character with the Tomestone.gg website.");
            ImGui.Separator();
            ImGui.TextWrapped($"Features:");
            ImGui.BulletText("Ingame: Open a characters Tomestone using the Context/Right-Click menu!");
            ImGui.BulletText("Activity: Collects your current Job, Level, Zone and Traveling Status. This is currently only displayed on the Streams page.");
            ImGui.BulletText("Gear: Collects your current Gear (Including Materia, Dye and Glamour).");
            ImGui.BulletText("Triple Triad: Collects your Triple Triad Card Collection.");
            ImGui.BulletText("Orchestrion: Collects your Orchestrion Roll Collection.");
            ImGui.BulletText("Blue Mage: Collects your Blue Mage Spell Collection.");
        }
    }
}
