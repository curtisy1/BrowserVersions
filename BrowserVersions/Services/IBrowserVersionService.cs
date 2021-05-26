namespace BrowserVersions.Services {
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using BrowserVersions.Enums;
  using BrowserVersions.Models;

  public interface IBrowserVersionService {
    Task<Dictionary<TargetBrowser, Dictionary<Platform, VersionChannels>>> GetBrowserVersion(List<TargetBrowser> targetBrowsers, List<Platform> targetPlatforms);
  }
}