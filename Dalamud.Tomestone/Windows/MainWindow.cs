using System;
using System.Numerics;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace Dalamud.Tomestone.Windows;

public class MainWindow : Window, IDisposable
{
    private IDalamudTextureWrap TomestoneImage;
    private Plugin Plugin;

    public MainWindow(Plugin plugin, IDalamudTextureWrap tomestoneImage) : base(
        "My Amazing Window", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.TomestoneImage = tomestoneImage;
        this.Plugin = plugin;
    }

    public void Dispose()
    {
        this.TomestoneImage.Dispose();
    }

    public override void Draw()
    {
        ImGui.Text($"The random config bool is {this.Plugin.Configuration.SomePropertyToBeSavedAndWithADefault}");

        if (ImGui.Button("Show Settings"))
        {
            this.Plugin.DrawConfigUI();
        }

        ImGui.Spacing();

        ImGui.Text("Have a tomestone:");
        ImGui.Indent(55);
        ImGui.Image(this.TomestoneImage.ImGuiHandle, new Vector2(this.TomestoneImage.Width, this.TomestoneImage.Height));
        ImGui.Unindent(55);
    }
}
