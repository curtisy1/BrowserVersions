namespace BrowserVersions.Services {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Net.Http;
  using System.Net.Http.Json;
  using System.Threading.Tasks;
  using BrowserVersions.Models;
  using BrowserVersions.Models.Chrome;
  using BrowserVersions.Models.Firefox;
  using Microsoft.Extensions.Logging;

  public class BrowserVersionService : IBrowserVersionService {
    private const string firefoxUri = "https://product-details.mozilla.org/1.0/{0}_versions.json"; // can be firefox or mobile
    private const string chromeUri = "https://omahaproxy.appspot.com/all.json";

    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger<BrowserVersionService> logger;

    public BrowserVersionService(IHttpClientFactory httpClientFactory, ILogger<BrowserVersionService> logger) {
      this.httpClientFactory = httpClientFactory;
      this.logger = logger;
    }

    public async Task<BrowserVersions> GetBrowserVersion(List<string> browsers) {
      var browserVersions = new BrowserVersions();
      var client = this.httpClientFactory.CreateClient();

      if (!browsers.Any()) {
        await AssignAllVersions(client, browserVersions);
        return browserVersions;
      }
      
      foreach (var browser in browsers) {
        switch (browser.ToLower()) {
          case "firefox":
            await AssignFirefoxDesktopVersions(client, browserVersions);
            break;
          case "firefoxmobile":
            await AssignFirefoxMobileVersions(client, browserVersions);
            break;
          case "chrome":
            await AssignChromeVersions(client, browserVersions);
            break;
          case "internetexplorer":
            break;
          case "edge":
            break;
          case "opera":
            break;
          case "vivaldi":
            break;
          case "brave":
            break;
          default:
            await AssignAllVersions(client, browserVersions);
            return browserVersions;
        }
      }

      return browserVersions;
    }

    private static async Task AssignAllVersions(HttpClient client, BrowserVersions browserVersions) {
      await AssignFirefoxDesktopVersions(client, browserVersions);
      await AssignFirefoxMobileVersions(client, browserVersions);
      await AssignChromeVersions(client, browserVersions);
    }

    private static async Task AssignFirefoxDesktopVersions(HttpClient client, BrowserVersions browserVersions) {
      var response = await client.GetAsync(new Uri(firefoxUri.Replace("{0}", "firefox")));
      browserVersions.FirefoxDesktop = ConvertFirefoxDesktopNamingToUseful(await response.Content.ReadFromJsonAsync<FirefoxDesktopApiVersion>());
    }

    private static async Task AssignFirefoxMobileVersions(HttpClient client, BrowserVersions browserVersions) {
      var response = await client.GetAsync(new Uri(firefoxUri.Replace("{0}", "mobile")));
      var (android, ios) = ConvertFirefoxMobileNamingToUseful(await response.Content.ReadFromJsonAsync<FirefoxMobileApiVersion>());
      
      browserVersions.FirefoxAndroid = android;
      browserVersions.FirefoxIos = ios;
    }

    private static async Task AssignChromeVersions(HttpClient client, BrowserVersions browserVersions) {
      var response = await client.GetAsync(new Uri(chromeUri));
      var json = await response.Content.ReadFromJsonAsync<List<ChromeApiModel>>();

      var (desktop, android, ios) = ConvertChromeNamingToUseful(json);

      browserVersions.ChromeAndroid = android;
      browserVersions.ChromeDesktop = desktop;
      browserVersions.ChromeIos = ios;
    }

    private static VersionChannels ConvertFirefoxDesktopNamingToUseful(FirefoxDesktopApiVersion model) {
      return new VersionChannels {
        Develop = model.FIREFOX_DEVEDITION,
        Esr = model.FIREFOX_ESR,
        Nightly = model.FIREFOX_NIGHTLY,
        Stable = model.LATEST_FIREFOX_VERSION,
      };
    }

    private static (VersionChannels Android, VersionChannels Ios) ConvertFirefoxMobileNamingToUseful(FirefoxMobileApiVersion model) {
      var androidVersions = new VersionChannels {
        Beta = model.beta_version,
        Nightly = model.nightly_version,
        Stable = model.version,
      };
      var iosVersions = new VersionChannels {
        Beta = model.ios_beta_version,
        Stable = model.ios_version,
      };

      return (androidVersions, iosVersions);
    }

    private static (VersionChannels Desktop, VersionChannels Android, VersionChannels Ios) ConvertChromeNamingToUseful(List<ChromeApiModel> models) {
      var desktopModel = models.FirstOrDefault(m => m.os == "win64");
      var androidModel = models.FirstOrDefault(m => m.os == "android");
      var iosModel = models.FirstOrDefault(m => m.os == "ios");

      var desktopVersions = new VersionChannels {
        Beta = desktopModel?.versions.FirstOrDefault(c => c.channel == "beta")?.version,
        Develop = desktopModel?.versions.FirstOrDefault(c => c.channel == "dev")?.version,
        Nightly = desktopModel?.versions.FirstOrDefault(c => c.channel == "canary")?.version,
        Stable = desktopModel?.versions.FirstOrDefault(c => c.channel == "stable")?.version,
      };
      
      var androidVersions = new VersionChannels {
        Beta = androidModel?.versions.FirstOrDefault(c => c.channel == "beta")?.version,
        Develop = androidModel?.versions.FirstOrDefault(c => c.channel == "dev")?.version,
        Nightly = androidModel?.versions.FirstOrDefault(c => c.channel == "canary")?.version,
        Stable = androidModel?.versions.FirstOrDefault(c => c.channel == "stable")?.version,
      };
      
      var iosVersions = new VersionChannels {
        Beta = iosModel?.versions.FirstOrDefault(c => c.channel == "beta")?.version,
        Develop = iosModel?.versions.FirstOrDefault(c => c.channel == "dev")?.version,
        Nightly = iosModel?.versions.FirstOrDefault(c => c.channel == "canary")?.version,
        Stable = iosModel?.versions.FirstOrDefault(c => c.channel == "stable")?.version,
      };

      return (desktopVersions, androidVersions, iosVersions);
    }
  }
}