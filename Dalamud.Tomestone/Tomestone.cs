using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Dalamud.Tomestone.Windows;
using System;
using Dalamud.Tomestone.UI;

namespace Dalamud.Tomestone;

public unsafe class Tomestone : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;

    public string Name => "Tomestone";
    private const string CommandName = "/ptomestone";
    internal static Tomestone T = null!;

    internal PluginUI PluginUI;
    internal FirstLaunchUI FirstLaunchUI;

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("Dalamud.Tomestone");

    internal DataHandler dataHandler;

    public Tomestone()
    {
        T = this;

        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // you might normally want to embed resources and load them from the manifest stream
        var logoPath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "tomestone.png");

        dataHandler = new DataHandler(this);

        PluginUI = new();

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "A useful message to display in /xlhelp"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += TogglePluginUI;

        // Register events for the plugin
        PluginInterface.Create<Service>();
        Service.ClientState.TerritoryChanged += OnTerritoryChanged;
        Service.Framework.Update += OnFrameworkUpdate;

        // Check if we are already logged in
        if (Service.ClientState.IsLoggedIn)
        {
            // If we are already logged in, we can just call the login event
            OnLogin();
        }
        else
        {
            // Else we need to wait for the login event
            Service.ClientState.Login += OnLogin;
        }
    }

    public void Dispose()
    {
        PluginUI.Dispose();

        WindowSystem.RemoveAllWindows();

        // Unregister events for the plugin safely
        try { 
            Service.ClientState.TerritoryChanged -= OnTerritoryChanged;
            Service.Framework.Update -= OnFrameworkUpdate;
            Service.ClientState.Login -= OnLogin;
        } catch { }

        CommandManager.RemoveHandler(CommandName);
    }

    private void OnLogin()
    {
        // If this is the first launch, show the FirstLaunchUI
        if (Configuration.IsFirstLaunch)
        {
            FirstLaunchUI = new FirstLaunchUI();
            FirstLaunchUI.Toggle();
        }
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

        try { 
            dataHandler.Update();
        } catch (Exception e) {
            Service.Log.Error(e, "Failed to update data handler");
        }
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our main ui
        TogglePluginUI();
    }

    private void DrawUI() => WindowSystem.Draw();

    public void TogglePluginUI() => PluginUI.Toggle();
}
