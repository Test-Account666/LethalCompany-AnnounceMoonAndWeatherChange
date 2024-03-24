using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace AnnounceMoonAndWeatherChange;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("WeatherTweaks", BepInDependency.DependencyFlags.SoftDependency)]
public class AnnounceMoonAndWeatherChange : BaseUnityPlugin {
    public static AnnounceMoonAndWeatherChange Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger { get; private set; } = null!;
    private static Harmony? Harmony { get; set; }
    internal static ConfigManager? configManager;

    private void Awake() {
        Logger = base.Logger;
        Instance = this;

        Patch();

        configManager = new ConfigManager(Config);

        configManager.HandleConfig();

        if (DependencyChecker.IsWeatherTweaksInstalled()) {
            WeatherTweaksSupport.enabled = true;
            Logger.LogInfo("WeatherTweaks found! Enabling support for it :)");
        }

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
    }

    internal static void Patch() {
        Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

        Logger.LogDebug("Patching...");

        Harmony.PatchAll();

        Logger.LogDebug("Finished patching!");
    }

    internal static void Unpatch() {
        Logger.LogDebug("Unpatching...");

        Harmony?.UnpatchSelf();

        Logger.LogDebug("Finished unpatching!");
    }
}