using System.Collections.Generic;
using System.Reflection;
using BepInEx.Configuration;

namespace AnnounceMoonAndWeatherChange;

public class ConfigManager {
    internal ConfigEntry<string> announceMoonChangeText = null!;
    internal ConfigEntry<string> weatherWarningUpperText = null!;
    internal ConfigEntry<string> weatherWarningLowerText = null!;
    internal ConfigEntry<bool> showWeatherWarning = null!;
    internal ConfigEntry<bool> showMoonChangeAnnouncement = null!;
    internal ConfigEntry<int> textFontSize = null!;
    internal ConfigEntry<int> scrollSpeed = null!;
    internal ConfigEntry<float> textColorRed = null!;
    internal ConfigEntry<float> textColorBlue = null!;
    internal ConfigEntry<float> textColorGreen = null!;
    private readonly ConfigFile _configFile;

    internal ConfigManager(ConfigFile configFile) => _configFile = configFile;

    internal void HandleConfig() {
        BindConfigValues();

        DeleteOrphanedEntries();
    }

    private void BindConfigValues() {
        announceMoonChangeText = _configFile.Bind<string>("Messages", "Announce Moon Change Text",
            "Ship routed to: <MOON>",
            "The message displayed when moon is changed. <MOON> is replaced with the moon's name");

        weatherWarningUpperText = _configFile.Bind<string>("Messages", "Weather Warning Upper Text", "!!! WARNING !!!",
            "The Header Text for the weather warning");

        weatherWarningLowerText = _configFile.Bind<string>("Messages", "Weather Warning Lower Text",
            "<MOON> is currently <WEATHER>!",
            "The body message displayed, if the moon has a weather. <MOON> is replaced with the moon's name. <WEATHER> is replaced with the moon's weather");

        showMoonChangeAnnouncement = _configFile.Bind<bool>("Toggles", "Show Moon Change Announcement", true,
            "Wether or not to show the moon change announcement");

        showWeatherWarning = _configFile.Bind<bool>("Toggles", "Show Weather Warning", true,
            "Wether or not to show the weather warning");

        textFontSize = _configFile.Bind<int>("HUD", "Text Font Size", 30, "The font size of the weather warning text");

        scrollSpeed = _configFile.Bind<int>("HUD", "Text Scroll Speed", 100,
            "The speed at which the weather warning text scrolls");

        textColorRed = _configFile.Bind<float>("HUD", "Text Color Red", 1F,
            new ConfigDescription("How much red do you want to have inside your text?",
                new AcceptableValueRange<float>(0F, 1F)));

        textColorGreen = _configFile.Bind<float>("HUD", "Text Color Green", 1F,
            new ConfigDescription("How much green do you want to have inside your text?",
                new AcceptableValueRange<float>(0F, 1F)));

        textColorBlue = _configFile.Bind<float>("HUD", "Text Color Blue", 1F,
            new ConfigDescription("How much blue do you want to have inside your text?",
                new AcceptableValueRange<float>(0F, 1F)));
    }

    private void DeleteOrphanedEntries() {
        // Normally, old unused config entries don't get removed, so we do it with this piece of code. Credit to Kittenji.
        var orphanedEntriesProperty = _configFile.GetType()
            .GetProperty("OrphanedEntries", BindingFlags.NonPublic | BindingFlags.Instance);

        if (orphanedEntriesProperty == null) {
            AnnounceMoonAndWeatherChange.Logger.LogError("Couldn't find orphanedEntriesProp!");
            return;
        }

        var orphanedEntries = (Dictionary<ConfigDefinition, string>)orphanedEntriesProperty.GetValue(_configFile, null);
        orphanedEntries.Clear(); // Clear orphaned entries (Unbinded/Abandoned entries)

        _configFile.Save(); // Save the config file to save these changes
    }
}