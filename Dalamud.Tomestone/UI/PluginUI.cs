using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;
using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using System;
using System.IO;

using ThreadLoadImageHandler = ECommons.ImGuiMethods.ThreadLoadImageHandler;
using ECommons.ImGuiMethods;

namespace Dalamud.Tomestone.UI
{
    internal class PluginUI : Window
    {
        public OpenWindow OpenWindow { get; set; }

        private string? imageHint;
        internal string? debug_welcome;
        internal string? debug_header_generalInformation;
        internal string? debug_header_remoteConfig;
        internal string? debug_header_dalamudInformation;
        internal string? debug_header_settings;
        internal string? debug_header_actions;
        private bool hasCachedLocStrings;

        public PluginUI() : base($"{Tomestone.T.Name} {Tomestone.T.GetType().Assembly.GetName().Version}###Tomestone")
        {
            this.RespectCloseHotkey = false;
            this.SizeConstraints = new()
            {
                MinimumSize = new(1000, 500),
                MaximumSize = new(9999, 9999)
            };
            Tomestone.T.WindowSystem.AddWindow(this);

            // Make sure we are on "Overview" when opening the UI
            OpenWindow = OpenWindow.Overview;

            if (Tomestone.CurrentLocManager != null)
            {
                Tomestone.CurrentLocManager.LocalizationChanged += (langCode) => { hasCachedLocStrings = false; };
            }
        }

        public void Dispose() { }

        private void UpdateLocalizationCache()
        {
            if (hasCachedLocStrings) { return; }
            hasCachedLocStrings = true;

            // Update the strings
            imageHint = Localization.Localize("MAIN_ImageHint", "Have a Tomestone!");
            debug_welcome = Localization.Localize("MAIN_DebugWelcome", "Welcome to the debug menu! Here you can find some useful information and settings for debugging purposes.");
            debug_header_generalInformation = Localization.Localize("MAIN_DebugHeaderGeneralInformation", "General Information");
            debug_header_remoteConfig = Localization.Localize("MAIN_DebugHeaderRemoteConfig", "Remote Configuration");
            debug_header_dalamudInformation = Localization.Localize("MAIN_DebugHeaderDalamudInformation", "Dalamud Information");
            debug_header_settings = Localization.Localize("MAIN_DebugHeaderSettings", "Debug Settings");
            debug_header_actions = Localization.Localize("MAIN_DebugHeaderActions", "Debug Actions");
        }

        public override void Draw()
        {
            UpdateLocalizationCache();

            var region = ImGui.GetContentRegionAvail();
            var itemSpacing = ImGui.GetStyle().ItemSpacing;

            var topLeftSideHeight = region.Y;

            ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(5f, 0));
            try
            {
                using (var table = ImRaii.Table($"TomestoneTableContainer", 2, ImGuiTableFlags.Resizable))
                {
                    if (!table)
                        return;

                    ImGui.TableSetupColumn("##LeftColumn", ImGuiTableColumnFlags.WidthFixed, ImGui.GetWindowWidth() / 2);

                    ImGui.TableNextColumn();

                    var regionSize = ImGui.GetContentRegionAvail();

                    ImGui.PushStyleVar(ImGuiStyleVar.SelectableTextAlign, new Vector2(0.5f, 0.5f));
                    using (var leftChild = ImRaii.Child($"###TomestoneLeftSide", regionSize with { Y = topLeftSideHeight }, false, ImGuiWindowFlags.NoDecoration))
                    {
                        var imagePath = Path.Combine(Service.PluginInterface.AssemblyLocation.Directory?.FullName!, "Images\\tomestone.png");

                        var logo = Tomestone.TextureProvider.GetFromFile(imagePath).GetWrapOrDefault();
                        if (logo != null)
                        {
                            ImGuiEx.LineCentered("###TomestoneLogo", () =>
                            {
                                ImGui.Image(logo.Handle, new(125f, 125f));
                                if (ImGui.IsItemHovered())
                                    ImGui.SetTooltip(imageHint);
                            });
                        }

                        ImGui.Spacing();
                        ImGui.Separator();
                        if (ImGui.Selectable("Overview", OpenWindow == OpenWindow.Overview))
                        {
                            OpenWindow = OpenWindow.Overview;
                        }
                        ImGui.Spacing();
                        if (ImGui.Selectable("Settings", OpenWindow == OpenWindow.Settings))
                        {
                            OpenWindow = OpenWindow.Settings;
                        }
#if DEBUG
                        ImGui.Spacing();
                        if (ImGui.Selectable("Debug", OpenWindow == OpenWindow.Debug))
                        {
                            OpenWindow = OpenWindow.Debug;
                        }
                        ImGui.Spacing();
#endif
                    }

                    ImGui.PopStyleVar();
                    ImGui.TableNextColumn();
                    using (var rightChild = ImRaii.Child($"###TomestoneRightSide", Vector2.Zero, false))
                    {
                        switch (OpenWindow)
                        {
                            case OpenWindow.Overview:
                                Overview.Draw();
                                break;
                            case OpenWindow.Settings:
                                Settings.Draw();
                                break;
                            case OpenWindow.Debug:
                                Debug.Draw(this);
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Service.Log.Error(ex, "Error in Tomestone UI");
            }
            ImGui.PopStyleVar();
        }
    }

    public enum OpenWindow
    {
        None = 0,
        Debug = 1,
        Overview = 2,
        Settings = 3,
    }
}
