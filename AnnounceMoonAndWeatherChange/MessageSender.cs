using UnityEngine;

namespace AnnounceMoonAndWeatherChange;

using Plugin = AnnounceMoonAndWeatherChange; // Way too annoying to type this everytime

internal static class MessageSender {
    private static readonly int _Display = Animator.StringToHash("display");
    private const string MOON_PLACEHOLDER = "<MOON>";
    private const string WEATHER_PLACEHOLDER = "<WEATHER>";

    internal static void SendWeatherWarning() {
        if (!IsWeatherWarningEnabled())
            return;

        var currentLevel = StartOfRound.Instance.currentLevel;
        if (currentLevel == null || IsWeatherOnMoonNone(currentLevel))
            return;

        var weather = GetCurrentWeather(currentLevel);

        DisplayWeatherWarning(currentLevel, weather);
    }

    internal static void SendMoonChangeAnnouncement() {
        if (!IsMoonChangeAnnouncementEnabled())
            return;

        var currentLevel = StartOfRound.Instance.currentLevel;
        if (currentLevel == null)
            return;

        var announceMoonChangeText = Plugin.configManager?.announceMoonChangeText.Value;

        if (announceMoonChangeText == null)
            return;

        DisplayMoonChangeAnnouncement(currentLevel, announceMoonChangeText);
    }

    private static bool IsWeatherWarningEnabled() {
        return Plugin.configManager?.showWeatherWarning.Value == true;
    }

    private static bool IsMoonChangeAnnouncementEnabled() {
        return Plugin.configManager?.showMoonChangeAnnouncement.Value == true;
    }

    private static string GetCurrentWeather(SelectableLevel currentLevel) {
        var weather = currentLevel.currentWeather.ToString();

        if (!WeatherTweaksSupport.enabled)
            return weather;


        weather = WeatherTweaksSupport.GetCurrentWeather(currentLevel) ?? weather;
        return weather;
    }

    private static bool IsWeatherOnMoonNone(SelectableLevel selectableLevel) {
        var isClear = selectableLevel.currentWeather == LevelWeatherType.None;

        if (!WeatherTweaksSupport.enabled)
            return isClear;

        var currentWeather = WeatherTweaksSupport.GetCurrentWeather(selectableLevel);
        if (currentWeather == null)
            return isClear;

        return currentWeather.ToLower().Contains("none") && isClear;
    }

    private static void DisplayWeatherWarning(SelectableLevel currentLevel, string weather) {
        var weatherWarningUpperText = Plugin.configManager?.weatherWarningUpperText.Value;
        var weatherWarningLowerText = Plugin.configManager?.weatherWarningLowerText.Value
            .Replace(MOON_PLACEHOLDER, currentLevel.PlanetName)
            .Replace(WEATHER_PLACEHOLDER, weather);

        var slidingText = HUDManager.Instance.gameObject.AddComponent<SlidingText>();

        var red = Plugin.configManager?.textColorRed.Value;
        var green = Plugin.configManager?.textColorGreen.Value;
        var blue = Plugin.configManager?.textColorBlue.Value;

        if (red is null || green is null || blue is null) {
            Plugin.Logger.LogError("Couldn't set text color!");
            Plugin.Logger.LogError($"Red: {red}");
            Plugin.Logger.LogError($"Green: {green}");
            Plugin.Logger.LogError($"Blue: {blue}");
            return;
        }

        slidingText.textColor = new Color(red.Value, green.Value, blue.Value);

        slidingText.fontSize = (int)Plugin.configManager?.textFontSize.Value!;

        slidingText.slideSpeed = (float)Plugin.configManager?.scrollSpeed.Value!;

        slidingText.text = $"{weatherWarningUpperText}\n{weatherWarningLowerText}".Trim();

        RoundManager.PlayRandomClip(HUDManager.Instance.UIAudio, HUDManager.Instance.warningSFX, true);
    }

    private static void DisplayMoonChangeAnnouncement(SelectableLevel currentLevel, string announceMoonChangeText) {
        var hudManager = HUDManager.Instance;

        hudManager.deviceChangeText.text = announceMoonChangeText.Replace(MOON_PLACEHOLDER, currentLevel.PlanetName);
        hudManager.deviceChangeAnimator.SetTrigger(_Display);
    }
}