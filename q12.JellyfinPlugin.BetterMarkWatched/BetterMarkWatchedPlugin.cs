using System;
using System.Collections.Generic;
using System.Globalization;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;
using q12.JellyfinPlugin.BetterMarkWatched.Configuration;

namespace q12.JellyfinPlugin.BetterMarkWatched;

public sealed class BetterMarkWatchedPlugin : BasePlugin<PluginConfiguration>, IHasWebPages, IDisposable
{
    private readonly RuntimePatcher _patcher;

    public BetterMarkWatchedPlugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer, ILogger<BetterMarkWatchedPlugin> logger)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
        Logger = logger;
        _patcher = new RuntimePatcher();
    }

    public void Dispose()
    {
        _patcher.Dispose();
    }

    public override string Name => "Better Mark Watched";

    public override Guid Id => Guid.Parse("d14e432b-2bc8-4ebf-bc41-a9082cbba0ef");

    public static BetterMarkWatchedPlugin? Instance { get; private set; }

    public IEnumerable<PluginPageInfo> GetPages()
    {
        return
        [
            new PluginPageInfo
            {
                Name = this.Name,
                EmbeddedResourcePath = string.Format(CultureInfo.InvariantCulture, "{0}.Configuration.configPage.html", GetType().Namespace),
            }
        ];
    }

    public ILogger<BetterMarkWatchedPlugin> Logger { get; }
}
