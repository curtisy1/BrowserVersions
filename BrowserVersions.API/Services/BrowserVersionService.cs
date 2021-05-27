namespace BrowserVersions.API.Services {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Net.Http;
  using System.Net.Http.Json;
  using System.Threading.Tasks;
  using BrowserVersions.API.Enums;
  using BrowserVersions.API.Models;
  using BrowserVersions.API.Models.Chrome;
  using BrowserVersions.API.Models.Edge;
  using BrowserVersions.API.Models.Firefox;
  using Microsoft.Extensions.Logging;

  public class BrowserVersionService : IBrowserVersionService {
    private const string firefoxUri = "https://product-details.mozilla.org/1.0/{0}_versions.json"; // can be firefox or mobile
    private const string chromeUri = "https://omahaproxy.appspot.com/all.json";
    private const string edgeUri = "https://www.microsoftedgeinsider.com/api/versions";

    private readonly HttpClient httpClient;
    private readonly ILogger<BrowserVersionService> logger;

    public BrowserVersionService(IHttpClientFactory httpClientFactory, ILogger<BrowserVersionService> logger) {
      this.httpClient = httpClientFactory.CreateClient();
      this.logger = logger;
    }

    public async Task<Dictionary<TargetBrowser, Dictionary<Platform, VersionChannels>>> GetBrowserVersion(List<TargetBrowser> targetBrowsers, List<Platform> targetPlatforms) {
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

      foreach (var browser in browsers) {
        browserVersions[browser] = new Dictionary<Platform, VersionChannels>();
        foreach (var platform in platforms) {
          browserVersions[browser][platform] = await this.AssignVersion(browser, platform);
        }
      }

      return browserVersions;
    }

    private async Task<VersionChannels> AssignVersion(TargetBrowser targetBrowser, Platform platform) {
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
          switch (platform) {
            case Platform.Desktop:
            case Platform.Android:
            case Platform.Ios:
              return ConvertChromeNamingToUseful(await this.GetVersionInternal<List<ChromeApiModel>>(chromeUri), platform);
            default:
              return new VersionChannels();
          }
        case TargetBrowser.InternetExplorer:
          return platform switch {
            Platform.Desktop => new VersionChannels {
              Stable = "11.0.220",
            },
            _ => new VersionChannels()
          };
        case TargetBrowser.Edge:
          return this.ConvertEdgeNamingToUseful(await this.GetVersionInternal<EdgeApiVersion>(edgeUri));
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

    private VersionChannels ConvertEdgeNamingToUseful(EdgeApiVersion model) {
      return new() {
        Beta = model.Beta,
        Develop = model.Dev,
        Nightly = model.Canary,
        Stable = model.Stable,
      };
    }
  }
}