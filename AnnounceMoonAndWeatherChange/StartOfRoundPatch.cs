using System.Threading;
using HarmonyLib;

namespace AnnounceMoonAndWeatherChange;

[HarmonyPatch(typeof(StartOfRound))]
public static class StartOfRoundPatch {
    private static bool _gameFinishedLoading = false;

    [HarmonyPatch("Awake")]
    [HarmonyPrefix]
    public static void BeforeAwake() {
        _gameFinishedLoading = false;
    }

    [HarmonyPatch("SetShipReadyToLand")]
    [HarmonyPostfix]
    public static void AfterSetShipReadyToLand() {
        MessageSender.SendWeatherWarning();
    }

    [HarmonyPatch("ChangeLevel")]
    [HarmonyPostfix]
    public static void AfterChangeLevel() {
        if (!_gameFinishedLoading) {
            Timer? timer = null;

            timer = new Timer(_ => {
                    _gameFinishedLoading = true;
                    HandleChangeLevel();
                    timer?.Dispose();
                }, null, (long)(3.5 * 1000),
                Timeout.Infinite);
            return;
        }

        HandleChangeLevel();
    }

    private static void HandleChangeLevel() {
        MessageSender.SendWeatherWarning();
        MessageSender.SendMoonChangeAnnouncement();
    }
}