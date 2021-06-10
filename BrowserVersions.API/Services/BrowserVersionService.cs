namespace BrowserVersions.API.Services {
  using System;
  using System.Collections.Generic;
  using System.Globalization;
  using System.IO;
  using System.Linq;
  using System.Net.Http;
  using System.Net.Http.Json;
  using System.Text.Json;
  using System.Threading.Tasks;
  using BrowserVersions.API.Models;
  using BrowserVersions.API.Models.Chrome;
  using BrowserVersions.API.Models.Edge;
  using BrowserVersions.API.Models.Firefox;
  using BrowserVersions.Data;
  using BrowserVersions.Data.Entities;
  using BrowserVersions.Data.Enums;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Logging;
  using Version = BrowserVersions.Data.Entities.Version;

  public class BrowserVersionService : IBrowserVersionService {
    private const string firefoxUri = "https://product-details.mozilla.org/1.0/{0}_versions.json"; // can be firefox or mobile
    private const string chromeUri = "https://omahaproxy.appspot.com/all.json";
    private const string edgeUri = "https://www.microsoftedgeinsider.com/api/versions";

    private readonly HttpClient httpClient;
    private readonly BrowserVersionsContext browserVersionDbContext;
    private readonly ILogger<BrowserVersionService> logger;

    public BrowserVersionService(IHttpClientFactory httpClientFactory, BrowserVersionsContext browserVersionDbContext, ILogger<BrowserVersionService> logger) {
      this.httpClient = httpClientFactory.CreateClient();
      this.browserVersionDbContext = browserVersionDbContext;
      this.logger = logger;
    }

    public async Task<Dictionary<TargetBrowser, Dictionary<Platform, VersionChannels>>> GetBrowserVersion(List<TargetBrowser> targetBrowsers, List<Platform> targetPlatforms, DateTime? minSupportDate, DateTime? maxSupportDate) {
      var savedBrowserVersionsTask = this.GetSavedBrowserVersions(minSupportDate, maxSupportDate);
      var browserVersions = new Dictionary<TargetBrowser, Dictionary<Platform, VersionChannels>>();
      var browsers = targetBrowsers;
      var platforms = targetPlatforms;

      if (!browsers.Any()) {
        platforms = new List<Platform> {
          Platform.Android,
          Platform.Desktop,
          Platform.Ios,
        };
        browsers = new List<TargetBrowser> {
          TargetBrowser.Firefox,
          TargetBrowser.Chrome,
          TargetBrowser.Edge,
          TargetBrowser.InternetExplorer
        };
      }

      var savedBrowserVersions = await savedBrowserVersionsTask;
      foreach (var browser in browsers) {
        browserVersions[browser] = new Dictionary<Platform, VersionChannels>();
        foreach (var platform in platforms) {
          var savedBrowserVersion = savedBrowserVersions.FirstOrDefault(bv => bv.Browsers.Any(b => b.Platform == platform && b.Type == browser));
          if (savedBrowserVersion != null) {
            browserVersions[browser][platform] = AssignDatabaseVersionToChannel(savedBrowserVersion);
          } else {
            browserVersions[browser][platform] = await this.FetchAndAssignVersionFromApi(browser, platform);
            // TODO: Insert into database here? Would have the benefit of saving a sync but the risk of duplicates if 2 of the same requests run at once
          }
        }
      }

      return browserVersions;
    }

    public async Task AddHistoricalData() {
      var workingDir = Directory.GetCurrentDirectory();
      var filePath = $"{workingDir}/../BrowserVersions.Data/firefoxreleases.json";
      var firefoxVersions = await JsonSerializer.DeserializeAsync<FirefoxHistoricalDataWrapper>(File.OpenRead(filePath));
      if (!(firefoxVersions?.releases?.Any() ?? true)) {
        return;
      }

      var existingBrowsers = await this.browserVersionDbContext.Browsers.Where(b => b.Type == TargetBrowser.Firefox).ToListAsync();
      var versionsToInsert = new List<Version>();
      foreach (var (flavor, flavorVersions) in firefoxVersions.releases) {
        var targetPlatforms = new List<Platform>();
        if (flavor is "fennec" or "fenix") {
          targetPlatforms.AddRange(new [] { Platform.Android , Platform.Ios});
        } else {
          targetPlatforms.Add(Platform.Desktop);
        }

        foreach (var version in flavorVersions) {
          if (!versionsToInsert.Any(v => v.VersionCode == version.version && v.Browsers.Any(b => targetPlatforms.Contains(b.Platform)))) {
            versionsToInsert.Add(this.FirefoxReleaseToVersion(version, existingBrowsers.Where(b => targetPlatforms.Contains(b.Platform)).ToList()));
          }
        }
      }
      
      filePath = $"{workingDir}/../BrowserVersions.Data/nightlyreleases.json";
      var nightlyVersions = await JsonSerializer.DeserializeAsync<FirefoxNightlyDataWrapper>(File.OpenRead(filePath));
      if (!(nightlyVersions?.builds?.Any() ?? true)) {
        return;
      }

      foreach (var nightlyVersion in nightlyVersions.builds) {
        if (!versionsToInsert.Any(v => v.VersionCode == nightlyVersion.app_version && v.ReleaseChannel == ReleaseChannel.Nightly)) {
          versionsToInsert.Add(this.NightlyReleaseToVersion(nightlyVersion, existingBrowsers.ToList()));
        }
      }

      await this.browserVersionDbContext.Versions.AddRangeAsync(versionsToInsert);
      await this.browserVersionDbContext.SaveChangesAsync();
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

    private Task<List<Version>> GetSavedBrowserVersions(DateTime? startingFrom, DateTime? untilIncluding) {
      return this.browserVersionDbContext.Versions.Where(v => v.ReleaseDate >= startingFrom && v.ReleaseDate <= untilIncluding)
        .Include(v => v.Browsers)
        .ToListAsync();
    }

    private static VersionChannels AssignDatabaseVersionToChannel(Version version) {
      return version.ReleaseChannel switch {
        ReleaseChannel.Beta => new VersionChannels { Beta = version.VersionCode },
        ReleaseChannel.Nightly => new VersionChannels { Nightly = version.VersionCode },
        ReleaseChannel.Develop => new VersionChannels { Develop = version.VersionCode },
        ReleaseChannel.Stable => new VersionChannels { Stable = version.VersionCode },
        ReleaseChannel.Esr => new VersionChannels { Esr = version.VersionCode },
        _ => new VersionChannels()
      };
    }

    private async Task<VersionChannels> FetchAndAssignVersionFromApi(TargetBrowser targetBrowser, Platform platform) {
      switch (targetBrowser) {
        case TargetBrowser.Firefox:
          switch (platform) {
            case Platform.Desktop:
              return ConvertFirefoxDesktopNamingToUseful(await this.GetVersionInternal<FirefoxDesktopApiVersion>(firefoxUri.Replace("{0}", "firefox")));
            case Platform.Android:
            case Platform.Ios:
              return ConvertFirefoxMobileNamingToUseful(await this.GetVersionInternal<FirefoxMobileApiVersion>(firefoxUri.Replace("{0}", "mobile")), platform);
            default:
              return new VersionChannels();
          }
        case TargetBrowser.Chrome:
          return ConvertChromeNamingToUseful(await this.GetVersionInternal<List<ChromeApiModel>>(chromeUri), platform);
        case TargetBrowser.InternetExplorer:
          return platform switch {
            Platform.Desktop => new VersionChannels {
              Stable = "11.0.220",
            },
            _ => new VersionChannels()
          };
        case TargetBrowser.Edge:
          return ConvertEdgeNamingToUseful(await this.GetVersionInternal<EdgeApiVersion>(edgeUri));
        default:
          return new VersionChannels();
      }
    }

    private async Task<T> GetVersionInternal<T>(string uriString) {
      var response = await this.httpClient.GetAsync(new Uri(uriString));
      return await response.Content.ReadFromJsonAsync<T>();
    }

    private static VersionChannels ConvertFirefoxDesktopNamingToUseful(FirefoxDesktopApiVersion model) {
      return new() {
        Develop = model.FIREFOX_DEVEDITION,
        Esr = model.FIREFOX_ESR,
        Nightly = model.FIREFOX_NIGHTLY,
        Stable = model.LATEST_FIREFOX_VERSION,
      };
    }

    private static VersionChannels ConvertFirefoxMobileNamingToUseful(FirefoxMobileApiVersion model, Platform platform) {
      return platform switch {
        Platform.Android => new VersionChannels {
          Beta = model.beta_version,
          Nightly = model.nightly_version,
          Stable = model.version,
        },
        Platform.Ios => new VersionChannels {
          Beta = model.ios_beta_version,
          Stable = model.ios_version,
        },
        _ => new VersionChannels()
      };
    }

    private static VersionChannels ConvertChromeNamingToUseful(List<ChromeApiModel> models, Platform platform) {
      switch (platform) {
        case Platform.Android:
          var androidModel = models.FirstOrDefault(m => m.os == "android");
          return new VersionChannels {
            Beta = androidModel?.versions.FirstOrDefault(c => c.channel == "beta")?.version,
            Develop = androidModel?.versions.FirstOrDefault(c => c.channel == "dev")?.version,
            Nightly = androidModel?.versions.FirstOrDefault(c => c.channel == "canary")?.version,
            Stable = androidModel?.versions.FirstOrDefault(c => c.channel == "stable")?.version,
          };
        case Platform.Desktop:
          var desktopModel = models.FirstOrDefault(m => m.os == "win64");
          return new VersionChannels {
            Beta = desktopModel?.versions.FirstOrDefault(c => c.channel == "beta")?.version,
            Develop = desktopModel?.versions.FirstOrDefault(c => c.channel == "dev")?.version,
            Nightly = desktopModel?.versions.FirstOrDefault(c => c.channel == "canary")?.version,
            Stable = desktopModel?.versions.FirstOrDefault(c => c.channel == "stable")?.version,
          };
        case Platform.Ios:
          var iosModel = models.FirstOrDefault(m => m.os == "ios");
          return new VersionChannels {
            Beta = iosModel?.versions.FirstOrDefault(c => c.channel == "beta")?.version,
            Develop = iosModel?.versions.FirstOrDefault(c => c.channel == "dev")?.version,
            Nightly = iosModel?.versions.FirstOrDefault(c => c.channel == "canary")?.version,
            Stable = iosModel?.versions.FirstOrDefault(c => c.channel == "stable")?.version,
          };
        default:
          return new VersionChannels();
      }
    }

    private static VersionChannels ConvertEdgeNamingToUseful(EdgeApiVersion model) {
      return new() {
        Beta = model.Beta,
        Develop = model.Dev,
        Nightly = model.Canary,
        Stable = model.Stable,
      };
    }
  }
}