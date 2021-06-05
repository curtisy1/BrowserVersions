namespace BrowserVersions.API.Models {
  using BrowserVersions.Data.Enums;

  public class RequestParameter {
    public TargetBrowser TargetBrowser { get; set; }

    public Platform Platform { get; set; }
  }
}