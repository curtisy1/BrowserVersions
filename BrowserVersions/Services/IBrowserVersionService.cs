namespace BrowserVersions.Services {
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using BrowserVersions.Models;

  public interface IBrowserVersionService {
    Task<BrowserVersions> GetBrowserVersion(List<string> browsers);
  }
}