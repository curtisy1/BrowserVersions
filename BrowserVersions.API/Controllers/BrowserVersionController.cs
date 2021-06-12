namespace BrowserVersions.API.Controllers {
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using BrowserVersions.API.Services;
  using BrowserVersions.Data.Enums;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.Extensions.Hosting;
  using Microsoft.Extensions.Logging;

  [Route("v1")]
  public class BrowserVersionController : ControllerBase {
    private readonly IBrowserVersionService browserVersionService;
    private readonly IBrowserVersionSeedingService browserVersionSeedingService;
    private readonly IWebHostEnvironment webHostEnvironment;
    private readonly ILogger<BrowserVersionController> logger;
    
    public BrowserVersionController(IBrowserVersionService browserVersionService, IBrowserVersionSeedingService browserVersionSeedingService, IWebHostEnvironment webHostEnvironment, ILogger<BrowserVersionController> logger) {
      this.browserVersionService = browserVersionService;
      this.browserVersionSeedingService = browserVersionSeedingService;
      this.webHostEnvironment = webHostEnvironment;
      this.logger = logger;
    }
    
    [HttpGet("")]
    public async Task<IActionResult> Get(List<TargetBrowser> browsers, List<Platform> platforms, List<ReleaseChannel> channels, DateTime? releasesFrom = null, DateTime? releasesTo = null, DateTime? supportedUntil = null) {
      return this.Ok(await this.browserVersionService.GetBrowserVersion(browsers, platforms, channels, releasesFrom, releasesTo, supportedUntil));
    }
    
    [HttpPost("")]
    public async Task<IActionResult> Post(List<TargetBrowser> browsers, List<Platform> platforms, List<ReleaseChannel> channels, DateTime? releasesFrom = null, DateTime? releasesTo = null, DateTime? supportedUntil = null) {
      return this.Ok(await this.browserVersionService.GetBrowserVersion(browsers, platforms, channels, releasesFrom, releasesTo, supportedUntil));
    }

    [HttpGet("history")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> History() {
      if (this.webHostEnvironment.IsDevelopment()) {
        await this.browserVersionSeedingService.SeedBrowserData();
        return this.Ok();
      }

      return this.Forbid("For internal use and development purposes only");
    }
  }
}