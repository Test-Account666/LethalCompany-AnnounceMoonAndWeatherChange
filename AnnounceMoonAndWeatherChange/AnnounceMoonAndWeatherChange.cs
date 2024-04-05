using System;
using AnnounceMoonAndWeatherChange.WeatherWarningAnimations.Impl;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace AnnounceMoonAndWeatherChange;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("WeatherTweaks", BepInDependency.DependencyFlags.SoftDependency)]
public class AnnounceMoonAndWeatherChange : BaseUnityPlugin {
    internal static ConfigManager? configManager;

    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    // ReSharper disable once MemberCanBePrivate.Global
    public static EventHandler registerNowEvent = null!;
    public static AnnounceMoonAndWeatherChange Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger { get; private set; } = null!;
    private static Harmony? Harmony { get; set; }

    private void Awake() {
        Logger = base.Logger;
        Instance = this;

        InitializeDefaultAnimations();

        registerNowEvent?.Invoke(this, null);

        Patch();

        if (DependencyChecker.IsWeatherTweaksInstalled()) {
            WeatherTweaksSupport.enabled = true;
            Logger.LogInfo("WeatherTweaks found! Enabling support for it :)");
        }

        configManager = new(Config);

        configManager.HandleConfig();

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
    }

    internal static void Patch() {
        Harmony ??= new(MyPluginInfo.PLUGIN_GUID);

        Logger.LogDebug("Patching...");

        Harmony.PatchAll();

        Logger.LogDebug("Finished patching!");
    }

    internal static void Unpatch() {
        Logger.LogDebug("Unpatching...");

        Harmony?.UnpatchSelf();

        Logger.LogDebug("Finished unpatching!");
    }

    private static void InitializeDefaultAnimations() {
        SlidingAnimation.Initialize();
        FadeAnimation.Initialize();
        BlinkingAnimation.Initialize();
    }
}