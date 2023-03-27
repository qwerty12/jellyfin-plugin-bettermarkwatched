using MediaBrowser.Model.Plugins;

namespace q12.JellyfinPlugin.BetterMarkWatched.Configuration;

public class PluginConfiguration : BasePluginConfiguration
{
    public PluginConfiguration()
    {
        MarkResumableItemUnplayedOnPlay = true;
        UpdateLastPlayedAndPlayCountOnPlayCompletion = false;
    }

    /// <summary>
    /// Gets or sets a value indicating whether to mark a resumable item being played as unplayed when the play starts.
    /// </summary>
    /// <value><c>true</c> if this resumable item should be marked unplayed; otherwise, <c>false</c> item play state will not change when play starts.</value>
    public bool MarkResumableItemUnplayedOnPlay { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to set LastPlayed time and increment playcount only when played to completion, or on every play start.
    /// </summary>
    /// <value><c>true</c> if playcount and lastplayed update on play completion; otherwise, <c>false</c> playcount and lastplayed update on play start.</value>
    public bool UpdateLastPlayedAndPlayCountOnPlayCompletion { get; set; }
}
