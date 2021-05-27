namespace BrowserVersions.API.Services {
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using BrowserVersions.API.Enums;
  using BrowserVersions.API.Models;

  public interface IBrowserVersionService {
    Task<Dictionary<TargetBrowser, Dictionary<Platform, VersionChannels>>> GetBrowserVersion(List<TargetBrowser> targetBrowsers, List<Platform> targetPlatforms);
  }
}