using System.Collections.Generic;
using System.Reflection;
using AnnounceMoonAndWeatherChange.WeatherWarningAnimations;
using BepInEx.Configuration;

namespace AnnounceMoonAndWeatherChange;

public class ConfigManager {
    private readonly ConfigFile _configFile;
    internal ConfigEntry<string> animationType = null!;
    internal ConfigEntry<string> announceMoonChangeText = null!;
    internal ConfigEntry<int> scrollSpeed = null!;
    internal ConfigEntry<bool> showMoonChangeAnnouncement = null!;
    internal ConfigEntry<bool> showWeatherWarning = null!;
    internal ConfigEntry<float> textColorBlue = null!;
    internal ConfigEntry<float> textColorGreen = null!;
    internal ConfigEntry<float> textColorRed = null!;
    internal ConfigEntry<int> textFontSize = null!;
    internal ConfigEntry<string> weatherWarningLowerText = null!;
    internal ConfigEntry<string> weatherWarningUpperText = null!;

    internal ConfigManager(ConfigFile configFile) => _configFile = configFile;

    internal void HandleConfig() {
        BindConfigValues();

        DeleteOrphanedEntries();
    }

    private void BindConfigValues() {
        announceMoonChangeText = _configFile.Bind<string>("Messages", "1. Announce Moon Change Text",
                                                          "Ship routed to: <MOON>",
                                                          "The message displayed when moon is changed. <MOON> is replaced with the moon's name");

        weatherWarningUpperText = _configFile.Bind<string>("Messages", "2. Weather Warning Upper Text",
                                                           "!!! WARNING !!!",
                                                           "The Header Text for the weather warning");

        weatherWarningLowerText = _configFile.Bind<string>("Messages", "3. Weather Warning Lower Text",
                                                           "<MOON> is currently <WEATHER>!",
                                                           "The body message displayed, if the moon has a weather. <MOON> is replaced with the moon's name. <WEATHER> is replaced with the moon's weather");

        showMoonChangeAnnouncement = _configFile.Bind("Toggles", "1. Show Moon Change Announcement", true,
                                                      "Whether or not to show the moon change announcement");

        showWeatherWarning = _configFile.Bind("Toggles", "2. Show Weather Warning", true,
                                              "Whether or not to show the weather warning");

        textFontSize = _configFile.Bind("HUD", "1. Text Font Size", 30, "The font size of the weather warning text");

        animationType = _configFile.Bind("HUD", "2. Animation Type", "Sliding",
                                         new ConfigDescription("What animation do you want for the weather warning?",
                                                               new AcceptableValueList<string>([
                                                                   ..AnimationManager.GetAnimationNames(),
                                                               ])));

        scrollSpeed = _configFile.Bind("HUD", "3. Animation Speed", 100,
                                       new ConfigDescription("The speed for the weather warning animation",
                                                             new AcceptableValueRange<int>(1, 200)));

        textColorRed = _configFile.Bind("HUD", "4. Text Color Red", 1F,
                                        new ConfigDescription("How much red do you want to have inside your text?",
                                                              new AcceptableValueRange<float>(0F, 1F)));

        textColorGreen = _configFile.Bind("HUD", "5. Text Color Green", 1F,
                                          new ConfigDescription("How much green do you want to have inside your text?",
                                                                new AcceptableValueRange<float>(0F, 1F)));

        textColorBlue = _configFile.Bind("HUD", "6. Text Color Blue", 1F,
                                         new ConfigDescription("How much blue do you want to have inside your text?",
                                                               new AcceptableValueRange<float>(0F, 1F)));

        AnnounceMoonAndWeatherChange.Logger.LogInfo("Animation set to: " + animationType.Value);
    }

    private void DeleteOrphanedEntries() {
        // Normally, old unused config entries don't get removed, so we do it with this piece of code. Credit to Kittenji.
        var orphanedEntriesProperty = _configFile.GetType()
                                                 .GetProperty("OrphanedEntries",
                                                              BindingFlags.NonPublic | BindingFlags.Instance);

        if (orphanedEntriesProperty == null) {
            AnnounceMoonAndWeatherChange.Logger.LogError("Couldn't find orphanedEntriesProp!");
            return;
        }

        var orphanedEntries =
            (Dictionary<ConfigDefinition, string>) orphanedEntriesProperty.GetValue(_configFile, null);


        if (orphanedEntries is not {
                Count: > 0,
            }) return;

        // Clear orphaned entries (Unbinded/Abandoned entries)
        orphanedEntries.Clear();
        AnnounceMoonAndWeatherChange.Logger.LogWarning(
            "Unused config entries were deleted. Make sure to check your config!");

        _configFile.Save(); // Save the config file to save these changes
    }
}