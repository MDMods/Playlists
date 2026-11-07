using HarmonyLib;
using Il2Cpp;

namespace Playlists.Patches;

[HarmonyPatch(typeof(WelcomeBtnLogin), nameof(WelcomeBtnLogin.OnClick))]
public class WelcomeButtonPatch
{
    // ReSharper disable once InconsistentNaming
    private static bool Prefix() => Playlists.FinishedLoading;
}
