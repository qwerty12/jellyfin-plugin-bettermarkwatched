# Better Mark (as) Watched plugin for Jellyfin

'cause I'm *really* tired of of having my Trakt progress screwed up

SenorSmartyPants (no affiliation) made [this](https://github.com/jellyfin/jellyfin/pull/8275) pull request (and [this](https://github.com/jellyfin/jellyfin-web/pull/3805) corresponding one to expose the new settings). The PR adds the following options:

> * MarkResumableItemUnplayedOnPlay : Allows disabling of default behavior to mark an item as unplayed when playback starts
>
> * UpdateLastPlayedAndPlayCountOnPlayCompletion: Enable to increment play count and update last played date when item is played to completion, instead of default updating on playback start.

However, as I write this it's been roughly eight months since that PR was made and there's been no activity on it. It makes no changes to the defaults and the code modifications are, relatively speaking, trivial. Meanwhile, I have to keep going to Trakt and unmarking episodes that I accidentally started (or started to test plugins).
I do not wish to maintain a fork of Jellyfin to merge SSP's PR locally so monkey patching it is.

`Emby.Server.Implementations.Library.UserDataManager::UpdatePlayState` is simple to patch.

However, for both `Emby.Server.Implementations.Session.SessionManager::OnPlaybackStart` and `Emby.Server.Implementations.Session.SessionManager::OnPlaybackStopped` I pretty much copied the methods from the Jellyfin source code with SenorSmartyPants's changes applied on top.

While it has to be said that as of this writing, the original methods remain unchanged from the time of the original PRs to HEAD today, and they're simple and seemingly complete enough that it seems unlikely they will be changed further, it is one of the reasons that using this plugin on anything newer than Jellyfin 10.8.9 is risky. For that reason, I don't provide binaries. I can only hope there won't be a need for this by then.

## Installation

1. Build the plugin in Release configuration

2. Shutdown Jellyfin

3. Make a q12.JellyfinPlugin.BetterMarkWatched folder in C:\ProgramData\Jellyfin\Server\plugins\

4. Copy the following into said folder:

    * q12.JellyfinPlugin.BetterMarkWatched.dll

    * 0Harmony.dll

    * Mono.Cecil.dll

    * MonoMod.Common.dll

5. Start Jellyfin and go to http://%JELLYFIN_URL%/web/index.html#!/configurationpage?name=Better%20Mark%20Watched to configure

## Credits

* SenorSmartyPants for the actual implementation

* pardeike for the wonderful [Harmony](https://harmony.pardeike.net) library, plus the authors of these guides:

    * https://outward.fandom.com/wiki/Mod_development_guide/Harmony

    * https://7d2dmods.github.io/HarmonyDocs/index.htm?PrefixandPostfix.html
