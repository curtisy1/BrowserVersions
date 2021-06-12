namespace BrowserVersions.API.Services {
  using System.Threading.Tasks;

  public interface IBrowserVersionSeedingService {
    Task SeedBrowserData();
  }
}