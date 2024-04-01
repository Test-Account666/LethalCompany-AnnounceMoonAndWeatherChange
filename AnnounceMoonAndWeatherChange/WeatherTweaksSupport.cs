using System;
using System.Reflection;
using HarmonyLib;

namespace AnnounceMoonAndWeatherChange;

public static class WeatherTweaksSupport {
    internal static bool enabled = false;
    private static Type? _variablesType;
    private static MethodInfo? _getPlanetCurrentWeatherMethod;

    public static string? GetCurrentWeather(SelectableLevel selectableLevel) {
        var currentWeather = selectableLevel.currentWeather.ToString();

        if (_variablesType == null && !FetchVariablesType())
            return currentWeather;

        if (_getPlanetCurrentWeatherMethod == null && !FetchGetPlanetCurrentWeatherMethod())
            return currentWeather;

        return _getPlanetCurrentWeatherMethod?.Invoke(null, [
            selectableLevel,
        ]) as string;
    }

    private static bool FetchVariablesType() {
        var variablesType = AccessTools.TypeByName("WeatherTweaks.Variables");

        if (variablesType == null) {
            AnnounceMoonAndWeatherChange.Logger.LogError("[WT Support] Couldn't find Variables type!");
            return false;
        }

        _variablesType = variablesType;
        return true;
    }

    private static bool FetchGetPlanetCurrentWeatherMethod() {
        var getPlanetCurrentWeatherMethod = AccessTools.DeclaredMethod(_variablesType, "GetPlanetCurrentWeather",
                                                                       new[] {
                                                                           typeof(SelectableLevel),
                                                                       });

        if (getPlanetCurrentWeatherMethod == null) {
            AnnounceMoonAndWeatherChange.Logger.LogError("[WT Support] Couldn't find GetPlanetCurrentWeather method!");
            return false;
        }

        _getPlanetCurrentWeatherMethod = getPlanetCurrentWeatherMethod;
        return true;
    }
}