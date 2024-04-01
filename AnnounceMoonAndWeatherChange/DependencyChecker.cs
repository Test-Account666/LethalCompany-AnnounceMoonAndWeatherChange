using System.Linq;
using BepInEx.Bootstrap;

namespace AnnounceMoonAndWeatherChange;

internal static class DependencyChecker {
    internal static bool IsWeatherTweaksInstalled() =>
        Chainloader.PluginInfos.Values.Any(metadata => metadata.Metadata.GUID.Contains("WeatherTweaks"));
}