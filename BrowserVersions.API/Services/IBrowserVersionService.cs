namespace BrowserVersions.API.Services {
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using BrowserVersions.API.Models;
  using BrowserVersions.Data.Enums;

  public interface IBrowserVersionService {
    Task<Dictionary<TargetBrowser, Dictionary<Platform, Dictionary<ReleaseChannel, VersionModel>>>> GetBrowserVersion(List<TargetBrowser> targetBrowsers, List<Platform> targetPlatforms, List<ReleaseChannel> targetChannels, DateTime? releasesFrom, DateTime? releasesTo, DateTime? supportedUntil);
  }
}