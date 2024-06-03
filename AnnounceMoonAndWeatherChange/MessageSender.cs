using System;
using System.Linq.Expressions;
using System.Reflection;
using AnnounceMoonAndWeatherChange.WeatherWarningAnimations;
using HarmonyLib;
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

        var previousAnimation = HUDManager.Instance.gameObject.GetComponent<WarningAnimation>();

        previousAnimation?.SpeedUp();

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
        if (currentWeather != null)
            return currentWeather.ToLower().Equals("none") && isClear;

        Plugin.Logger.LogInfo("Current Weather is null???");
        return isClear;
    }

    private static void DisplayWeatherWarning(SelectableLevel currentLevel, string weather) {
        var weatherWarningUpperText = Plugin.configManager?.weatherWarningUpperText.Value;
        var weatherWarningLowerText = Plugin.configManager?.weatherWarningLowerText.Value
                                            .Replace(MOON_PLACEHOLDER, currentLevel.PlanetName)
                                            .Replace(WEATHER_PLACEHOLDER, weather);

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

        // Since we don't know the method to execute yet, let's search for it 
        FetchAndExecutePlayRandomClipMethod();
    }

    private static void DisplayMoonChangeAnnouncement(SelectableLevel currentLevel, string announceMoonChangeText) {
        var hudManager = HUDManager.Instance;

        hudManager.deviceChangeText.text = announceMoonChangeText.Replace(MOON_PLACEHOLDER, currentLevel.PlanetName);
        hudManager.deviceChangeAnimator.SetTrigger(_Display);
    }

    private static void FetchAndExecutePlayRandomClipMethod() {
        // Get the method info
        _playRandomClipMethod ??= typeof(RoundManager).GetMethod("PlayRandomClip", BindingFlags.Public | BindingFlags.Static);

        if (_playRandomClipMethod is null)
            return;

        // Let's get our parameters
        var parameterExpressions = GetRequiredParameters();

        var callExpression = Expression.Call(_playRandomClipMethod, parameterExpressions);

        var lambdaExpression = Expression.Lambda<Action>(callExpression);

        // Compile the lambda expression into a delegate
        _playRandomClipExpression = lambdaExpression.Compile();

        // Invoke the delegate
        _playRandomClipExpression();
    }

    private static Expression[] GetRequiredParameters() {
        Plugin.Logger.LogInfo(_playRandomClipMethod?.GetParameters().Length);

        var getUIAudioMethod = AccessTools.Method(typeof(MessageSender), nameof(GetUIAudio));
        var getWarningSfxMethod = AccessTools.Method(typeof(MessageSender), nameof(GetWarningSfx));

        // 'PlayRandomClip' Method Parameters, for reference
        /*
           AudioSource audioSource,
           AudioClip[] clipsArray,
           bool randomize = true,
           float oneShotVolume = 1f,
           int audibleNoiseID = 0,
           int maxIndex = 1000 // This is new in v50
         */

        // We use some Call Expressions to ensure returned objects always exist
        Expression[] parameters = _playRandomClipMethod?.GetParameters().Length switch {
            // v49
            5 => [
                Expression.Call(getUIAudioMethod), Expression.Call(getWarningSfxMethod), Expression.Constant(true), Expression.Constant(1f),
                Expression.Constant(0),
            ],
            // v50
            6 => [
                Expression.Call(getUIAudioMethod), Expression.Call(getWarningSfxMethod), Expression.Constant(true), Expression.Constant(1f),
                Expression.Constant(0), Expression.Constant(1000),
            ],
            // Unsupported version
            var _ => throw new NotImplementedException("You're using an unsupported version of Lethal Company. Please report this.") {
                HelpLink = "https://github.com/Test-Account666/LethalCompany-AnnounceMoonAndWeatherChange/issues/",
                Source = "https://github.com/Test-Account666/LethalCompany-AnnounceMoonAndWeatherChange",
            },
        };

        return parameters;
    }

    private static AudioSource GetUIAudio() =>
        HUDManager.Instance.UIAudio;

    private static AudioClip[] GetWarningSfx() =>
        HUDManager.Instance.warningSFX;
}