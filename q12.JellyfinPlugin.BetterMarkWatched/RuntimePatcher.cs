﻿using System;
using System.Threading;
using Emby.Server.Implementations.Library;
using Emby.Server.Implementations.Session;
using HarmonyLib;
using Jellyfin.Data.Entities;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;

namespace q12.JellyfinPlugin.BetterMarkWatched;

internal sealed class RuntimePatcher : IDisposable
{
    private Harmony? _harmony;
    private static readonly string? _harmonyId = typeof(RuntimePatcher).Namespace;

    public RuntimePatcher()
    {
        _harmony = new Harmony(_harmonyId);
        _harmony.PatchAll();
    }

    /*
     * I'd prefer to keep this DLL permanently loaded instead with tricks like:
         * GET_MODULE_HANDLE_EX_FLAG_PIN
         * storing an instance of its corresponding AssemblyLoadContext in this plugin
         * creating a circular reference between this and the BetterMarkWatchedPlugin
         * somehow attempting "object resurrection"
     * but, alas, future Jellyfin versions will be implementing in-process restarting
     */
    ~RuntimePatcher() => Release();

    private void Release()
    {
        if (_harmony is null)
        {
            return;
        }

        _harmony.UnpatchAll(_harmonyId);
        _harmony = null;
    }

    public void Dispose()
    {
        Release();
        GC.SuppressFinalize(this);
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
#if DEBUG
        BetterMarkWatchedPlugin.Instance!.Logger!.LogInformation("Patch_SessionManager_OnPlaybackStart::Prefix");
#endif
        var data = ____userDataManager.GetUserData(user, item);

        if (!BetterMarkWatchedPlugin.Instance!.Configuration.UpdateLastPlayedAndPlayCountOnPlayCompletion)
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
            else if (BetterMarkWatchedPlugin.Instance!.Configuration.MarkResumableItemUnplayedOnPlay)
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
#if DEBUG
        BetterMarkWatchedPlugin.Instance!.Logger!.LogInformation("Patch_SessionManager_OnPlaybackStopped::Prefix");
#endif
        bool playedToCompletion = false;

        if (!playbackFailed)
        {
            var data = ____userDataManager.GetUserData(user, item);

            if (positionTicks.HasValue)
            {
                playedToCompletion = ____userDataManager.UpdatePlayState(item, data, positionTicks.Value);
                if (playedToCompletion && BetterMarkWatchedPlugin.Instance!.Configuration.UpdateLastPlayedAndPlayCountOnPlayCompletion)
                {
                    data.PlayCount++;
                    data.LastPlayedDate = DateTime.UtcNow;
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
