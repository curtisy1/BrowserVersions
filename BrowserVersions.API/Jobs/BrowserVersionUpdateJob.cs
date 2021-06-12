namespace BrowserVersions.API.Jobs {
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
  using BrowserVersions.Data.Constants;
  using BrowserVersions.Data.Enums;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Logging;
  using Quartz;
  using Version = BrowserVersions.Data.Entities.Version;

  public class BrowserVersionUpdateJob : IJob {
    private readonly HttpClient httpClient;
    private readonly BrowserVersionsContext browserVersionsContext;
    private readonly ILogger<BrowserVersionUpdateJob> logger;

    public BrowserVersionUpdateJob(IHttpClientFactory httpClientFactory, BrowserVersionsContext browserVersionsContext, ILogger<BrowserVersionUpdateJob> logger) {
      this.httpClient = httpClientFactory.CreateClient();
      this.browserVersionsContext = browserVersionsContext;
      this.logger = logger;
    }

    public async Task Execute(IJobExecutionContext context) {
      this.logger.LogInformation("Background job running");
      await this.GetLatestVersionsFromApis();
    }

    private async Task GetLatestVersionsFromApis() {
      var browsers = new List<TargetBrowser> {
        TargetBrowser.Firefox,
        TargetBrowser.Chrome,
        TargetBrowser.Edge,
        TargetBrowser.InternetExplorer
      };
      var platforms = new List<Platform> {
        Platform.Android,
        Platform.Desktop,
        Platform.Ios,
      };
      var channels = new List<ReleaseChannel> {
        ReleaseChannel.Nightly,
        ReleaseChannel.Beta,
        ReleaseChannel.Develop,
        ReleaseChannel.Stable,
        ReleaseChannel.Esr,
      };

      var browserEntities = await this.browserVersionsContext.Browsers.ToListAsync();
      var existingVersions = await this.browserVersionsContext.Versions
       .Include(v => v.Browsers)
       .ToListAsync();
      var versionsToAdd = new List<Version>(browsers.Count * channels.Count * platforms.Count);

      foreach (var browser in browsers) {
        foreach (var platform in platforms) {
          var apiBrowserVersions = await this.FetchAndAssignVersionFromApi(browser, platform, channels);
          foreach (var (channel, versionModel) in apiBrowserVersions) {
            if (!existingVersions.Any(v => v.ReleaseChannel == channel && v.VersionCode == versionModel.Version && v.Browsers.Any(b => b.Platform == platform && b.Type == browser))) {
              versionsToAdd.AddRange(apiBrowserVersions.Where(x => !string.IsNullOrEmpty(x.Value.Version)).Select(x => new Version {
                Browsers = browserEntities.Where(b => b.Type == browser && b.Platform == platform).ToList(),
                ReleaseChannel = channel,
                ReleaseDate = versionModel.ReleaseDate,
                VersionCode = versionModel.Version,
              }));
            }
          }
        }
      }

      this.browserVersionsContext.Versions.AddRange(versionsToAdd);
      await this.browserVersionsContext.SaveChangesAsync();
    }

    private async Task<Dictionary<ReleaseChannel, VersionModel>> FetchAndAssignVersionFromApi(TargetBrowser targetBrowser, Platform platform, List<ReleaseChannel> channels) {
      var versionsPerChannel = new Dictionary<ReleaseChannel, VersionModel>(channels.Count);
      VersionChannels versionChannels;
      switch (targetBrowser) {
        case TargetBrowser.Firefox:
          switch (platform) {
            case Platform.Desktop:
              versionChannels = ConvertFirefoxDesktopNamingToUseful(await this.GetVersionInternal<FirefoxDesktopApiVersion>(VersionUrl.firefoxUri.Replace("{0}", "firefox")));
              break;
            case Platform.Android:
            case Platform.Ios:
              versionChannels = ConvertFirefoxMobileNamingToUseful(await this.GetVersionInternal<FirefoxMobileApiVersion>(VersionUrl.firefoxUri.Replace("{0}", "mobile")), platform);
              break;
            default:
              return versionsPerChannel;
          }

          break;
        case TargetBrowser.Chrome:
          versionChannels = ConvertChromeNamingToUseful(await this.GetVersionInternal<List<ChromeApiModel>>(VersionUrl.chromeUri), platform);
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
          versionChannels = ConvertEdgeNamingToUseful(await this.GetVersionInternal<EdgeApiVersion>(VersionUrl.edgeUri));
          break;
        default:
          return versionsPerChannel;
      }

      foreach (var channel in channels) {
        versionsPerChannel[channel] = new VersionModel {
          Version = channel switch {
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