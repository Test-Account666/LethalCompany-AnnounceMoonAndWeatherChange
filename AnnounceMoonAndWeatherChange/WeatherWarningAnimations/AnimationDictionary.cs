using System;
using System.Collections.Generic;

namespace AnnounceMoonAndWeatherChange.WeatherWarningAnimations;

public class AnimationDictionary : Dictionary<string, Type> {
    public new void Add(string key, Type value) {
        if (string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Animation name can't be null or empty!");

        if (!value.IsSubclassOf(typeof(WarningAnimation)))
            throw new ArgumentException("Animation class has to be of type WarningAnimation!");

        base.Add(key, value);
    }
}