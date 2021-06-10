namespace BrowserVersions.API.Controllers {
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using BrowserVersions.API.Services;
  using BrowserVersions.Data.Enums;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.Extensions.Logging;

  [Route("v1")]
  public class BrowserVersionController : ControllerBase {
    private readonly IBrowserVersionService browserVersionService;
    private readonly ILogger<BrowserVersionController> logger;
    
    public BrowserVersionController(IBrowserVersionService browserVersionService, ILogger<BrowserVersionController> logger) {
      this.browserVersionService = browserVersionService;
      this.logger = logger;
    }
    
    [HttpGet("")]
    public async Task<IActionResult> Get(List<TargetBrowser> browsers, List<Platform> platforms, DateTime? minSupportDate = null, DateTime? maxSupportDate = null) {
      return this.Ok(await this.browserVersionService.GetBrowserVersion(browsers, platforms, minSupportDate, maxSupportDate));
    }
    
    [HttpPost("")]
    public async Task<IActionResult> Post(List<TargetBrowser> browsers, List<Platform> platforms, DateTime? minSupportDate = null, DateTime? maxSupportDate = null) {
      return this.Ok(await this.browserVersionService.GetBrowserVersion(browsers, platforms, minSupportDate, maxSupportDate));
    }
  }
}