namespace BrowserVersions.API.Services {
  using System;
  using System.Collections.Generic;
  using System.Globalization;
  using System.IO;
  using System.Linq;
  using System.Text.Json;
  using System.Threading.Tasks;
  using BrowserVersions.API.Models;
  using BrowserVersions.Data;
  using BrowserVersions.Data.Entities;
  using BrowserVersions.Data.Enums;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Logging;
  using Version = BrowserVersions.Data.Entities.Version;

  public class BrowserVersionSeedingService : IBrowserVersionSeedingService {
    private readonly BrowserVersionsContext browserVersionsContext;
    private readonly ILogger<BrowserVersionSeedingService> logger;

    public BrowserVersionSeedingService(BrowserVersionsContext browserVersionsContext, ILogger<BrowserVersionSeedingService> logger) {
      this.browserVersionsContext = browserVersionsContext;
      this.logger = logger;
    }

    public async Task SeedBrowserData() {
      var workingDir = Directory.GetCurrentDirectory();
      var filePath = $"{workingDir}/../BrowserVersions.Data/firefoxreleases.json";
      var firefoxVersions = await JsonSerializer.DeserializeAsync<FirefoxHistoricalDataWrapper>(File.OpenRead(filePath));
      if (!(firefoxVersions?.releases?.Any() ?? true)) {
        return;
      }

      var existingBrowsers = await this.browserVersionsContext.Browsers.ToListAsync();
      var firefoxBrowsers = existingBrowsers.Where(b => b.Type == TargetBrowser.Firefox).ToList();
      var versionsToInsert = new List<Version>();
      foreach (var (flavor, flavorVersions) in firefoxVersions.releases) {
        var targetPlatforms = new List<Platform>();
        if (flavor is "fennec" or "fenix") {
          targetPlatforms.AddRange(new[] {
            Platform.Android,
            Platform.Ios
          });
        } else {
          targetPlatforms.Add(Platform.Desktop);
        }

        foreach (var version in flavorVersions) {
          if (!versionsToInsert.Any(v => v.VersionCode == version.version && v.Browsers.Any(b => targetPlatforms.Contains(b.Platform)))) {
            versionsToInsert.Add(this.FirefoxReleaseToVersion(version, firefoxBrowsers.Where(b => targetPlatforms.Contains(b.Platform)).ToList()));
          }
        }
      }

      var esrVersions = versionsToInsert.Where(v => v.ReleaseChannel == ReleaseChannel.Esr).ToList();
      for (var i = 1; i < esrVersions.Count; i++) {
        // Might not be accurate since this is the minimal supported time before EOL
        versionsToInsert[versionsToInsert.IndexOf(esrVersions[i - 1])].EndOfSupportDate = versionsToInsert[versionsToInsert.IndexOf(esrVersions[i])].ReleaseDate.AddMonths(15);
      }

      var modifiedEsr = versionsToInsert.Where(v => v.ReleaseChannel == ReleaseChannel.Esr).ToList();

      filePath = $"{workingDir}/../BrowserVersions.Data/nightlyreleases.json";
      var nightlyVersions = await JsonSerializer.DeserializeAsync<FirefoxNightlyDataWrapper>(File.OpenRead(filePath));
      if (!(nightlyVersions?.builds?.Any() ?? true)) {
        return;
      }

      foreach (var nightlyVersion in nightlyVersions.builds) {
        if (!versionsToInsert.Any(v => v.VersionCode == nightlyVersion.app_version && v.ReleaseChannel == ReleaseChannel.Nightly)) {
          versionsToInsert.Add(this.NightlyReleaseToVersion(nightlyVersion, firefoxBrowsers.ToList()));
        }
      }

      filePath = $"{workingDir}/../BrowserVersions.Data/chromereleases.json";
      var chromeVersions = await JsonSerializer.DeserializeAsync<ChromeHistoricalDataWrapper>(File.OpenRead(filePath));
      if (!(chromeVersions?.releases?.Any() ?? true)) {
        return;
      }

      var chromeBrowsers = existingBrowsers.Where(b => b.Type == TargetBrowser.Chrome).ToList();
      foreach (var release in chromeVersions.releases) {
        if (!versionsToInsert.Any(v => v.VersionCode == release.version)) {
          versionsToInsert.Add(this.ChromeReleaseToVersion(release, chromeBrowsers));
        }
      }

      await this.browserVersionsContext.Versions.AddRangeAsync(versionsToInsert);
      await this.browserVersionsContext.SaveChangesAsync();
    }

    private Version ChromeReleaseToVersion(ChromeHistoricalData release, IList<Browser> existingBrowsers) {
      var platformAndChannelInformation = release.name.Split("/");
      var platform = platformAndChannelInformation[2] switch {
        "win" => Platform.Desktop,
        "win64" => Platform.Desktop,
        "lacros" => Platform.Desktop,
        "linux" => Platform.Desktop,
        "mac" => Platform.Desktop,
        "mac_arm64" => Platform.Desktop,
        "webview" => Platform.Desktop,
        "ios" => Platform.Ios,
        "android" => Platform.Android,
        _ => throw new ArgumentException("No platform detected that would match")
      };

      var channel = platformAndChannelInformation[4] switch {
        "canary" => ReleaseChannel.Nightly,
        "dev" => ReleaseChannel.Develop,
        "beta" => ReleaseChannel.Beta,
        "stable" => ReleaseChannel.Stable,
        _ => throw new ArgumentException("No version detected that would match")
      };

      return new Version {
        Browsers = existingBrowsers.Where(b => b.Platform == platform).ToList(),
        ReleaseChannel = channel,
        ReleaseDate = release.serving.startTime,
        EndOfSupportDate = release.serving.endTime,
        VersionCode = release.version,
      };
    }

    private Version NightlyReleaseToVersion(FirefoxNightlyData release, IList<Browser> existingBrowsers) {
      var platform = release.platform switch {
        "win32" => Platform.Desktop,
        "win64" => Platform.Desktop,
        "linux32" => Platform.Desktop,
        "linux64" => Platform.Desktop,
        "mac" => Platform.Desktop,
        "mac_arm64" => Platform.Desktop,
        "webview" => Platform.Desktop,
        "ios" => Platform.Ios,
        "android" => Platform.Android,
        _ => throw new ArgumentException("No platform detected that would match")
      };

      var hasDate = DateTime.TryParseExact(release.buildid, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var releaseDate);
      if (!hasDate) {
        throw new ArgumentException("No valid date");
      }

      return new Version {
        Browsers = existingBrowsers.Where(b => b.Platform == platform).ToList(),
        ReleaseChannel = ReleaseChannel.Nightly,
        ReleaseDate = releaseDate,
        VersionCode = release.app_version,
      };
    }

    private Version FirefoxReleaseToVersion(FirefoxHistoricalData release, List<Browser> existingBrowsers) {
      var channel = release.category switch {
        "dev" => ReleaseChannel.Beta,
        "major" => ReleaseChannel.Stable,
        "stability" => ReleaseChannel.Stable,
        "esr" => ReleaseChannel.Esr,
        _ => throw new ArgumentException("No version detected that would match")
      };

      return new Version {
        Browsers = existingBrowsers,
        ReleaseChannel = channel,
        ReleaseDate = release.date,
        VersionCode = release.version,
      };
    }
  }
}