using System;
using System.Collections.Generic;
using System.Linq;

namespace AnnounceMoonAndWeatherChange.WeatherWarningAnimations;

public static class AnimationManager {
    private const string DEFAULT_ANIMATION = "Sliding";

    private static readonly AnimationDictionary _Animations = [
    ];

    public static Type? GetAnimation(string? animation) {
        if (string.IsNullOrEmpty(animation))
            return null;

        var type = _Animations.GetValueOrDefault(animation.ToLower());
        return type ?? _Animations.GetValueOrDefault(DEFAULT_ANIMATION.ToLower());
    }

    public static Type? GetCurrentAnimation() =>
        GetAnimation(AnnounceMoonAndWeatherChange.configManager?.animationType.Value.ToLower());

    public static void AddAnimation(string name, Type animationType) {
        name = name.ToLower();

        _Animations.Add(name, animationType);
    }

    public static List<string> GetAnimationNames() =>
        _Animations.Keys.ToList();
}