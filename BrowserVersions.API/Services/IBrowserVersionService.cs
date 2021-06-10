namespace BrowserVersions.API.Services {
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using BrowserVersions.API.Models;
  using BrowserVersions.Data.Enums;

  public interface IBrowserVersionService {
    Task<Dictionary<TargetBrowser, Dictionary<Platform, VersionChannels>>> GetBrowserVersion(List<TargetBrowser> targetBrowsers, List<Platform> targetPlatforms, DateTime? minSupportDate, DateTime? maxSupportDate);

    Task AddHistoricalData();
  }
}