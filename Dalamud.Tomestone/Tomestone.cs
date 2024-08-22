using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using System;
using Dalamud.Tomestone.UI;
using System.Reflection;

namespace Dalamud.Tomestone;

public unsafe class Tomestone : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;

    public string Name => "Tomestone";
    private const string CommandName = "/ptomestone";
    private readonly Localization locManager;
    internal static Tomestone T = null!;

    public static Localization? CurrentLocManager;
    private string[] supportedLangCodes = new string[] { "de", "en" };

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

        var assemblyName = GetType().Assembly.GetName().Name;
        locManager = new Localization($"{assemblyName}.Localization.", "", true);
        locManager.SetupWithLangCode(PluginInterface.UiLanguage);
        CurrentLocManager = locManager;

        dataHandler = new DataHandler(this);

        PluginUI = new();
        FirstLaunchUI = new();

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "A useful message to display in /xlhelp"
        });
     

        // Register plugin hooks
        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenMainUi += TogglePluginUI;
        PluginInterface.LanguageChanged += OnLanguageChanged;
        PluginInterface.Create<Service>();
        Service.ClientState.TerritoryChanged += OnTerritoryChanged;
        Service.Framework.Update += OnFrameworkUpdate;

        // Register ContextMenu
        ContextMenu.Enable();

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

    private void OnLanguageChanged(string langCode)
    {
        // check if resource is available, will cause exception if trying to load empty json
        if (Array.Find(supportedLangCodes, x => x == langCode) != null)
        {
            locManager.SetupWithLangCode(langCode);
        }
        else
        {
            locManager.SetupWithFallbacks();
        }
    }

    public void Dispose()
    {
        PluginUI.Dispose();

        WindowSystem.RemoveAllWindows();

        ContextMenu.Disable();

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
            FirstLaunchUI.Toggle();
        }
    }

    private void OnFrameworkUpdate(IFramework framework)
    {
        if (!Service.ClientState.IsLoggedIn) return;

        // Grab the local player
        var localPlayer = Service.ClientState.LocalPlayer;

        // Polling the framework is required to know if we had a ClassJob change
        try {
            dataHandler.HandleFrameworkUpdate(localPlayer);
        } catch (Exception e) {
            Service.Log.Error(e, "Failed to handle framework update");
        }
    }

    private void OnTerritoryChanged(ushort ID)
    {
        // Block if we aren't ingame yet
        if (!Service.ClientState.IsLoggedIn) return;

        try {
            // dataHandler.Update();
            dataHandler.ScheduleUpdate();
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
