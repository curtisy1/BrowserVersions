namespace BrowserVersions.API.Controllers {
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using BrowserVersions.API.Enums;
  using BrowserVersions.API.Services;
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
    public async Task<IActionResult> Get(List<TargetBrowser> browsers, List<Platform> platforms) {
      return this.Ok(await this.browserVersionService.GetBrowserVersion(browsers, platforms));
    }
    
    [HttpPost("")]
    public async Task<IActionResult> Post(List<TargetBrowser> browsers, List<Platform> platforms) {
      return this.Ok(await this.browserVersionService.GetBrowserVersion(browsers, platforms));
    }
  }
}