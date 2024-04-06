using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AnnounceMoonAndWeatherChange.WeatherWarningAnimations;
using UnityEngine;

namespace AnnounceMoonAndWeatherChange;

using Plugin = AnnounceMoonAndWeatherChange; // Way too annoying to type this everytime

internal static class MessageSender {
    private const string MOON_PLACEHOLDER = "<MOON>";
    private const string WEATHER_PLACEHOLDER = "<WEATHER>";
    private static readonly int _Display = Animator.StringToHash("display");
    private static MethodInfo? _playRandomClipMethod;
    private static Action? _playRandomClipExpression;

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

    private static bool IsWeatherWarningEnabled() => Plugin.configManager?.showWeatherWarning.Value == true;

    private static bool IsMoonChangeAnnouncementEnabled() =>
        Plugin.configManager?.showMoonChangeAnnouncement.Value == true;

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

        var previousAnimation = HUDManager.Instance.gameObject.GetComponent<WarningAnimation>();

        // ReSharper disable once UseNullPropagation
        if (previousAnimation is not null)
            previousAnimation.SpeedUp();

        var animationType = AnimationManager.GetCurrentAnimation();

        var animation = (WarningAnimation) HUDManager.Instance.gameObject.AddComponent(animationType);

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

        animation.textColor = new(red.Value, green.Value, blue.Value);

        animation.fontSize = (int) Plugin.configManager?.textFontSize.Value!;

        animation.animationSpeed = Plugin.configManager.scrollSpeed.Value;

        animation.text = $"{weatherWarningUpperText}\n{weatherWarningLowerText}".Trim();

        // If we already have the method, there's no need to fetch it again
        if (_playRandomClipExpression is not null) {
            _playRandomClipExpression();
            return;
        }

        var fetched = false;

        // Method body, for reference
        /*
           AudioSource audioSource,
           AudioClip[] clipsArray,
           bool randomize = true,
           float oneShotVolume = 1f,
           int audibleNoiseID = 0,
           int maxIndex = 1000 // This is new in v50
         */

        // Since v50 added some new parameter and we'd like to support v49 as well as v50, use this garbage™ to fetch the correct 'PlayRandomClip' method
        try {
            // v50 method
            fetched = FetchAndExecutePlayRandomClipMethod([
                HUDManager.Instance.UIAudio, HUDManager.Instance.warningSFX, true, 1f, 0, 1000,
            ]);
        } catch {
            // Who needs to logged exceptions anyway, right?
            // ignored
        } finally {
            // Grrrr, I'd love to have Early-Return here...
            if (!fetched)
                // v49 method
                FetchAndExecutePlayRandomClipMethod([
                    HUDManager.Instance.UIAudio, HUDManager.Instance.warningSFX, true, 1f, 0,
                ]);
        }
    }

    private static void DisplayMoonChangeAnnouncement(SelectableLevel currentLevel, string announceMoonChangeText) {
        var hudManager = HUDManager.Instance;

        hudManager.deviceChangeText.text = announceMoonChangeText.Replace(MOON_PLACEHOLDER, currentLevel.PlanetName);
        hudManager.deviceChangeAnimator.SetTrigger(_Display);
    }

    private static bool FetchAndExecutePlayRandomClipMethod(IEnumerable<object> parameters) {
        // Get the method info
        _playRandomClipMethod ??=
            typeof(RoundManager).GetMethod("PlayRandomClip", BindingFlags.Public | BindingFlags.Static);

        if (_playRandomClipMethod is null)
            return false; // Return false, as we failed to identify the method

        // ReSharper disable once CoVariantArrayConversion
        Expression[] parameterExpressions = parameters.Select(Expression.Constant).ToArray();

        var callExpression = Expression.Call(_playRandomClipMethod, parameterExpressions);

        var lambdaExpression = Expression.Lambda<Action>(callExpression);

        // Compile the lambda expression into a delegate
        _playRandomClipExpression = lambdaExpression.Compile();

        // Invoke the delegate
        _playRandomClipExpression();
        return true; // Return true, as we successfully identified the method
    }
}