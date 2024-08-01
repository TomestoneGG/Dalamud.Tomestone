using System;
using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace Dalamud.Tomestone.Windows;

public class MainWindow : Window, IDisposable
{
    private string LogoImagePath;
    private Tomestone plugin;

    // We give this window a hidden ID using ##
    // So that the user will see "TomestoneGG" as window title,
    // but for ImGui the ID is "TomestoneGG##Main"
    public MainWindow(Tomestone _plugin, string logoImagePath)
        : base("TomestoneGG##Main", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        LogoImagePath = logoImagePath;
        plugin = _plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        ImGui.Text("Have a tomestone:");
        var goatImage = Tomestone.TextureProvider.GetFromFile(LogoImagePath).GetWrapOrDefault();
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

        ImGui.Spacing();

        // Display Status information from plugin.dataHandler (UpdateError, LastUpdate, UpdateMessage)
        ImGui.Text($"Update in progress?: {plugin.dataHandler.status.updating}");
        ImGui.Text($"Last Update: {plugin.dataHandler.status.lastUpdate}");
        ImGui.Text($"Update Error: {plugin.dataHandler.status.UpdateError}");
        ImGui.Text($"Update Message: {plugin.dataHandler.status.UpdateMessage}");
    }
}
