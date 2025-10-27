using System;
using System.Threading;
using Emby.Server.Implementations.Library;
using Emby.Server.Implementations.Session;
using HarmonyLib;
using Jellyfin.Database.Implementations.Entities;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
#if DEBUG
using Microsoft.Extensions.Logging;
#endif

namespace q12.JellyfinPlugin.BetterMarkWatched;

internal sealed class RuntimePatcher : IDisposable
{
    private const string HarmonyId = "q12.JellyfinPlugin.BetterMarkWatched.RuntimePatcher";
    private Harmony? _harmony;

    public RuntimePatcher()
    {
        _harmony = new Harmony(HarmonyId);
        _harmony.PatchAll();
    }

    ~RuntimePatcher() => Release();

    private void Release()
    {
        if (_harmony is null)
        {
            return;
        }

        _harmony.UnpatchAll(HarmonyId);
        _harmony = null;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Release();
    }
}

[HarmonyPatch(typeof(UserDataManager), nameof(UserDataManager.UpdatePlayState))]
internal static class Patch_UserDataManager_UpdatePlayState
{
    private static void Postfix(bool __result, UserItemData data)
    {
#if DEBUG
        BetterMarkWatchedPlugin.Instance!.Logger!.LogInformation("Patch_UserDataManager_UpdatePlayState::Postfix");
#endif
        if (__result && BetterMarkWatchedPlugin.Instance!.Configuration.UpdateLastPlayedAndPlayCountOnPlayCompletion)
        {
            data.LastPlayedDate = DateTime.UtcNow;
        }
    }
}

[HarmonyPatch(typeof(SessionManager), nameof(SessionManager.OnPlaybackStart), typeof(User), typeof(BaseItem))]
internal static class Patch_SessionManager_OnPlaybackStart
{
    private static bool Prefix(IUserDataManager ____userDataManager, User user, BaseItem item)
    {
        var UpdateLastPlayedAndPlayCountOnPlayCompletion = BetterMarkWatchedPlugin.Instance!.Configuration.UpdateLastPlayedAndPlayCountOnPlayCompletion;
        var MarkResumableItemUnplayedOnPlay = BetterMarkWatchedPlugin.Instance!.Configuration.MarkResumableItemUnplayedOnPlay;
        if (!UpdateLastPlayedAndPlayCountOnPlayCompletion && MarkResumableItemUnplayedOnPlay)
        {
            return true;
        }
#if DEBUG
        BetterMarkWatchedPlugin.Instance!.Logger!.LogInformation("Patch_SessionManager_OnPlaybackStart::Prefix");
#endif

        var data = ____userDataManager.GetUserData(user, item);

        if (!UpdateLastPlayedAndPlayCountOnPlayCompletion)
        {
            data.PlayCount++;
            data.LastPlayedDate = DateTime.UtcNow;
        }

        if (item.SupportsPlayedStatus)
        {
            if (!item.SupportsPositionTicksResume)
            {
                data.Played = true;
            }
            else if (MarkResumableItemUnplayedOnPlay)
            {
                data.Played = false;
            }
        }
        else
        {
            data.Played = false;
        }

        ____userDataManager.SaveUserData(user, item, data, UserDataSaveReason.PlaybackStart, CancellationToken.None);

        return false;
    }
}

[HarmonyPatch(typeof(SessionManager), nameof(SessionManager.OnPlaybackStopped), typeof(User), typeof(BaseItem), typeof(long), typeof(bool))]
internal static class Patch_SessionManager_OnPlaybackStopped
{
    private static bool Prefix(ref bool __result, IUserDataManager ____userDataManager, User user, BaseItem item, long? positionTicks, bool playbackFailed)
    {
        if (!BetterMarkWatchedPlugin.Instance!.Configuration.UpdateLastPlayedAndPlayCountOnPlayCompletion)
        {
            return true;
        }
#if DEBUG
        BetterMarkWatchedPlugin.Instance!.Logger!.LogInformation("Patch_SessionManager_OnPlaybackStopped::Prefix");
#endif

        bool playedToCompletion = false;

        if (!playbackFailed)
        {
            var data = ____userDataManager.GetUserData(user, item);

            if (positionTicks.HasValue)
            {
                var prevLastPlayed = data.LastPlayedDate;
#if DEBUG
                BetterMarkWatchedPlugin.Instance!.Logger!.LogInformation("{0} {1} {2}", item.Id, prevLastPlayed, DateTime.UtcNow);
#endif
                playedToCompletion = ____userDataManager.UpdatePlayState(item, data, positionTicks.Value);
                if (playedToCompletion)
                {
                    var now = DateTime.UtcNow;
                    if (prevLastPlayed is null || now - prevLastPlayed.Value > TimeSpan.FromSeconds(4))
                    {
                        data.PlayCount++;
                    }

                    data.LastPlayedDate = now;
                }
            }
            else
            {
                // If the client isn't able to report this, then we'll just have to make an assumption
                data.PlayCount++;
                data.Played = item.SupportsPlayedStatus;
                data.PlaybackPositionTicks = 0;
                playedToCompletion = true;
            }

            ____userDataManager.SaveUserData(user, item, data, UserDataSaveReason.PlaybackFinished, CancellationToken.None);
        }

        __result = playedToCompletion;
        return false;
    }
}
