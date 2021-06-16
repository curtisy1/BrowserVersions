namespace BrowserVersions.API.Services {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using BrowserVersions.API.Models;
  using BrowserVersions.Data;
  using BrowserVersions.Data.Enums;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Logging;
  using Version = BrowserVersions.Data.Entities.Version;

  public class BrowserVersionService : IBrowserVersionService {
    private readonly BrowserVersionsContext browserVersionDbContext;
    private readonly ILogger<BrowserVersionService> logger;

    public BrowserVersionService(BrowserVersionsContext browserVersionDbContext, ILogger<BrowserVersionService> logger) {
      this.browserVersionDbContext = browserVersionDbContext;
      this.logger = logger;
    }

    public async Task<Dictionary<TargetBrowser, Dictionary<Platform, Dictionary<ReleaseChannel, VersionModel>>>> GetBrowserVersion(List<TargetBrowser> targetBrowsers, List<Platform> targetPlatforms, List<ReleaseChannel> targetChannels, DateTime? releasesFrom, DateTime? releasesTo, DateTime? supportedUntil) {
      var browserVersions = new Dictionary<TargetBrowser, Dictionary<Platform, Dictionary<ReleaseChannel, VersionModel>>>();
      var browsers = targetBrowsers;
      var platforms = targetPlatforms;
      var channels = targetChannels;

      if (!browsers.Any()) {
        browsers = new List<TargetBrowser> {
          TargetBrowser.Firefox,
          TargetBrowser.Chrome,
          TargetBrowser.Edge,
          TargetBrowser.InternetExplorer
        };
      }

      if (!platforms.Any()) {
        platforms = new List<Platform> {
          Platform.Android,
          Platform.Desktop,
          Platform.Ios,
        };
      }

      if (!channels.Any()) {
        channels = new List<ReleaseChannel> {
          ReleaseChannel.Nightly,
          ReleaseChannel.Beta,
          ReleaseChannel.Develop,
          ReleaseChannel.Stable,
          ReleaseChannel.Esr,
        };
      }

      var savedBrowserVersions = await this.GetSavedBrowserVersions(browsers, platforms, channels, releasesFrom, releasesTo, supportedUntil);
      foreach (var browser in browsers) {
        browserVersions[browser] = new Dictionary<Platform, Dictionary<ReleaseChannel, VersionModel>>();

        foreach (var platform in platforms) {
          browserVersions[browser][platform] = new Dictionary<ReleaseChannel, VersionModel>();

          foreach (var channel in channels) {
            var savedBrowserVersion = savedBrowserVersions.FirstOrDefault(bv => bv.ReleaseChannel == channel && bv.Browsers.Any(b => b.Platform == platform && b.Type == browser));
            if (savedBrowserVersion != null) {
              browserVersions[browser][platform][channel] = new VersionModel {
                Version = savedBrowserVersion.VersionCode,
                ReleaseDate = savedBrowserVersion.ReleaseDate,
              };
            }
          }
        }
      }

      return browserVersions;
    }

    private Task<List<Version>> GetSavedBrowserVersions(ICollection<TargetBrowser> browsers, ICollection<Platform> platforms, ICollection<ReleaseChannel> channels, DateTime? startingFrom, DateTime? untilIncluding, DateTime? supportedUntil) {
      this.logger.LogInformation("Getting saved browser versions startingFrom: {0}, untilIncluding: {1}, supportedUntil: {2}", startingFrom, untilIncluding, supportedUntil);

      return this.browserVersionDbContext.Versions.Where(v =>
           v.Browsers.Any(b => browsers.Contains(b.Type) && platforms.Contains(b.Platform))
           && channels.Contains(v.ReleaseChannel)
           && ((v.ReleaseDate >= startingFrom && v.ReleaseDate <= untilIncluding) || v.EndOfSupportDate == supportedUntil))
        .Include(v => v.Browsers)
        .OrderByDescending(v => v.ReleaseDate)
        .ToListAsync();
    }
  }
}