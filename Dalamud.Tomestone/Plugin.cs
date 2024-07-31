using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Dalamud.Tomestone.Windows;
using System;

namespace Dalamud.Tomestone;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;

    public string Name => "Tomestone";
    private const string CommandName = "/ptomestone";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("Dalamud.Tomestone");

    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    internal DataHandler dataHandler;

    public Plugin()
    {

        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // you might normally want to embed resources and load them from the manifest stream
        var logoPath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "tomestone.png");

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this, logoPath);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        dataHandler = new DataHandler(this);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "A useful message to display in /xlhelp"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

        // Register events for the plugin
        PluginInterface.Create<Service>();
        Service.ClientState.TerritoryChanged += OnTerritoryChanged;
        Service.Framework.Update += OnFrameworkUpdate;
    }

    public void Dispose()
    {
        // Unregister events for the plugin safely
        try { 
            Service.ClientState.TerritoryChanged -= OnTerritoryChanged;
            Service.Framework.Update -= OnFrameworkUpdate;
        } catch { }

        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
    }

    private void OnFrameworkUpdate(IFramework framework)
    {
        if (!Service.ClientState.IsLoggedIn) return;

        // Polling the framework is required to know if we had a ClassJob change
        try { 
            dataHandler.HandleFrameworkUpdate();
        } catch (Exception e) {
            Service.Log.Error(e, "Failed to handle framework update");
        }
    }

    private void OnTerritoryChanged(ushort ID)
    {
        // Block if we aren't ingame yet
        if (!Service.ClientState.IsLoggedIn) return;

        // do something when the territory changes
        Service.Log.Info("Territory changed to {0}", ID);
        try { 
            dataHandler.Update();
        } catch (Exception e) {
            Service.Log.Error(e, "Failed to update data handler");
        }
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our main ui
        ToggleMainUI();
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();
}
