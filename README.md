# Better Mark (as) Watched plugin for Jellyfin

SenorSmartyPants made [this](https://github.com/jellyfin/jellyfin/pull/8275) pull request (and [this](https://github.com/jellyfin/jellyfin-web/pull/3805) corresponding one to expose the new settings) which adds the following options:

> * MarkResumableItemUnplayedOnPlay : Allows disabling of default behavior to mark an item as unplayed when playback starts
>
> * UpdateLastPlayedAndPlayCountOnPlayCompletion: Enable to increment play count and update last played date when item is played to completion, instead of default updating on playback start.

There's a [discussion](https://github.com/jellyfin/jellyfin-meta/discussions/56) going on regarding changing the default behaviour regarding playback status on rewatches, but given that it's now two years later and that PR was never merged I'm not holding my breath. I do not wish to maintain a fork of Jellyfin to merge SSP's PR locally so monkey patching it is. Enter this plugin.
Due to the monkey patching, using this plugin on anything newer than Jellyfin 10.9.0 is risky. For that reason, I don't provide binaries. (Although it has to be said that as I write this, the original methods remain unchanged from the time of the original PR to today's HEAD.)

`Emby.Server.Implementations.Library.UserDataManager::UpdatePlayState` is simple enough to patch.

For both `Emby.Server.Implementations.Session.SessionManager::OnPlaybackStart` and `Emby.Server.Implementations.Session.SessionManager::OnPlaybackStopped`, however, I had to pretty much copy the methods from the Jellyfin 10.9.0 source code with SenorSmartyPants's changes applied on top.

## Installation

1. Build the plugin in Release configuration

2. Shutdown Jellyfin

3. Make a q12.JellyfinPlugin.BetterMarkWatched folder in C:\ProgramData\Jellyfin\Server\plugins\

4. Copy `q12.JellyfinPlugin.BetterMarkWatched\bin\Release\net8.0\q12.JellyfinPlugin.BetterMarkWatched.dll` into said folder

5. Start Jellyfin and go to `http://%JELLYFIN_URL%/web/index.html#!/configurationpage?name=Better%20Mark%20Watched` to configure

## Credits

* SenorSmartyPants for the actual implementation

* pardeike for the wonderful [Harmony](https://harmony.pardeike.net) library, plus the authors of these guides:

    * https://outward.fandom.com/wiki/Mod_development_guide/Harmony

    * https://7d2dmods.github.io/HarmonyDocs/index.htm?PrefixandPostfix.html

* [Fody/Costura](https://github.com/Fody/Costura)
