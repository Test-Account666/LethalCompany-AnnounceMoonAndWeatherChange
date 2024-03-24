using System.Linq;
using BepInEx.Bootstrap;

namespace AnnounceMoonAndWeatherChange;

internal static class DependencyChecker {
    internal static bool IsWeatherTweaksInstalled() {
        return Chainloader.PluginInfos.Values.Any(metadata => metadata.Metadata.GUID.Contains("WeatherTweaks"));
    }
}