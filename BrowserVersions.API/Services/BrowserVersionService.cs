namespace BrowserVersions.API.Services {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Net.Http;
  using System.Net.Http.Json;
  using System.Threading.Tasks;
  using BrowserVersions.API.Models;
  using BrowserVersions.API.Models.Chrome;
  using BrowserVersions.API.Models.Edge;
  using BrowserVersions.API.Models.Firefox;
  using BrowserVersions.Data;
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

    public async Task<Dictionary<TargetBrowser, Dictionary<Platform, Dictionary<ReleaseChannel, VersionModel>>>> GetBrowserVersion(List<TargetBrowser> targetBrowsers, List<Platform> targetPlatforms, List<ReleaseChannel> targetChannels, DateTime? releasesFrom, DateTime? releasesTo, DateTime? supportedUntil) {
      var savedBrowserVersionsTask = this.GetSavedBrowserVersions(releasesFrom, releasesTo, supportedUntil);
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

      var savedBrowserVersions = await savedBrowserVersionsTask;
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

          var missingBrowserVersions = channels.Where(c => !browserVersions[browser][platform].ContainsKey(c)).ToList();
          if (missingBrowserVersions.Any()) {
            var apiBrowserVersions = await this.FetchAndAssignVersionFromApi(browser, platform, missingBrowserVersions);
            foreach (var (channel, versionModel) in apiBrowserVersions) {
              browserVersions[browser][platform][channel] = versionModel;
            }
          }
        }
      }

      return browserVersions;
    }

    private Task<List<Version>> GetSavedBrowserVersions(DateTime? startingFrom, DateTime? untilIncluding, DateTime? supportedUntil) {
      return this.browserVersionDbContext.Versions.Where(v => (v.ReleaseDate >= startingFrom && v.ReleaseDate <= untilIncluding) || v.EndOfSupportDate == supportedUntil)
        .Include(v => v.Browsers)
        .OrderByDescending(v => v.ReleaseDate)
        .ToListAsync();
    }

    private async Task<Dictionary<ReleaseChannel, VersionModel>> FetchAndAssignVersionFromApi(TargetBrowser targetBrowser, Platform platform, List<ReleaseChannel> channels) {
      var versionsPerChannel = new Dictionary<ReleaseChannel, VersionModel>(channels.Count);
      var versionChannels = new VersionChannels();
      switch (targetBrowser) {
        case TargetBrowser.Firefox:
          switch (platform) {
            case Platform.Desktop:
              versionChannels = ConvertFirefoxDesktopNamingToUseful(await this.GetVersionInternal<FirefoxDesktopApiVersion>(firefoxUri.Replace("{0}", "firefox")));
              break;
            case Platform.Android:
            case Platform.Ios:
              versionChannels = ConvertFirefoxMobileNamingToUseful(await this.GetVersionInternal<FirefoxMobileApiVersion>(firefoxUri.Replace("{0}", "mobile")), platform);
              break;
            default:
              return versionsPerChannel;
          }

          break;
        case TargetBrowser.Chrome:
          versionChannels = ConvertChromeNamingToUseful(await this.GetVersionInternal<List<ChromeApiModel>>(chromeUri), platform);
          break;
        case TargetBrowser.InternetExplorer:
          versionChannels = platform switch {
            Platform.Desktop => new VersionChannels {
              Esr = "11.0.220",
              Stable = "11.0.220",
            },
            _ => new VersionChannels()
          };
          break;
        case TargetBrowser.Edge:
          versionChannels = ConvertEdgeNamingToUseful(await this.GetVersionInternal<EdgeApiVersion>(edgeUri));
          break;
        default:
          return versionsPerChannel;
      }

      foreach (var channel in channels) {
        versionsPerChannel[channel] = new VersionModel { Version = channel switch {
            ReleaseChannel.Beta => versionChannels.Beta,
            ReleaseChannel.Esr => versionChannels.Esr,
            ReleaseChannel.Develop => versionChannels.Develop,
            ReleaseChannel.Nightly => versionChannels.Nightly,
            ReleaseChannel.Stable => versionChannels.Stable,
            _ => throw new ArgumentOutOfRangeException(nameof(channels), "Channel does not match any given type")
          }
        };
      }

      return versionsPerChannel;
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