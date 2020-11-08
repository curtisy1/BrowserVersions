namespace BrowserVersions.Controllers {
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using BrowserVersions.Services;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.Extensions.Logging;

  [Route("[controller]")]
  public class BrowserVersionController : ControllerBase {
    private readonly IBrowserVersionService browserVersionService;
    private readonly ILogger<BrowserVersionController> logger;
    
    public BrowserVersionController(IBrowserVersionService browserVersionService, ILogger<BrowserVersionController> logger) {
      this.browserVersionService = browserVersionService;
      this.logger = logger;
    }
    
    [HttpGet("")]
    public async Task<IActionResult> Get(List<string> browsers) {
      return this.Ok(await this.browserVersionService.GetBrowserVersion(browsers));
    }
    
    [HttpPost("")]
    public async Task<IActionResult> Post([FromBody] List<string> browsers = null) {
      return this.Ok(await this.browserVersionService.GetBrowserVersion(browsers ?? new List<string>()));
    }
  }
}