using System;
using System.Numerics;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ImGuiNET;

namespace SamplePlugin.Windows;

public class MainWindow : Window, IDisposable
{
    private string LogoImagePath;
    private Plugin Plugin;

    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(Plugin plugin, string logoImagePath)
        : base("My Amazing Window##With a hidden ID", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        LogoImagePath = logoImagePath;
        Plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        ImGui.Text("Have a tomestone:");
        var goatImage = Plugin.TextureProvider.GetFromFile(LogoImagePath).GetWrapOrDefault();
        if (goatImage != null)
        {
            ImGuiHelpers.ScaledIndent(55f);
            ImGui.Image(goatImage.ImGuiHandle, new Vector2(goatImage.Width, goatImage.Height));
            ImGuiHelpers.ScaledIndent(-55f);
        }
        else
        {
            ImGui.Text("Image not found.");
        }

        if (ImGui.Button("Show Settings"))
        {
            Plugin.ToggleConfigUI();
        }

        ImGui.Spacing();

        // Display Status information from Plugin.dataHandler (UpdateError, LastUpdate, UpdateMessage)
        ImGui.Text($"Last Update: {Plugin.dataHandler.lastUpdate}");
        ImGui.Text($"Update Error: {Plugin.dataHandler.UpdateError}");
        ImGui.Text($"Update Message: {Plugin.dataHandler.UpdateMessage}");

        ImGui.Spacing();

        // Display benchmark information (times are in microseconds)
        ImGui.Text($"Player: {Plugin.dataHandler.status.basePlayerUpdate}μs");
        ImGui.Text($"Mounts: {Plugin.dataHandler.status.mountUpdate}μs");
        ImGui.Text($"Minions: {Plugin.dataHandler.status.minionUpdate}μs");
        ImGui.Text($"Achievements: {Plugin.dataHandler.status.achievementUpdate}μs");
        ImGui.Text($"Gearsets: {Plugin.dataHandler.status.gearsetUpdate}μs");
        ImGui.Text($"Lodestone: {Plugin.dataHandler.status.lodestoneUpdate}μs");
        ImGui.Text($"Saving: {Plugin.dataHandler.status.backendUpdate}μs");

        // Calculate the total time taken for the last update
        var total = Plugin.dataHandler.status.basePlayerUpdate +
                    Plugin.dataHandler.status.mountUpdate +
                    Plugin.dataHandler.status.minionUpdate +
                    Plugin.dataHandler.status.achievementUpdate +
                    Plugin.dataHandler.status.gearsetUpdate +
                    Plugin.dataHandler.status.lodestoneUpdate +
                    Plugin.dataHandler.status.backendUpdate;

        ImGui.Text($"Total: {total}μs");
    }
}
