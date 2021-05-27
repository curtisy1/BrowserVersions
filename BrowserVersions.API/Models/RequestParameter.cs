namespace BrowserVersions.API.Models {
  using Enums;

  public class RequestParameter {
    public TargetBrowser TargetBrowser { get; set; }

    public Platform Platform { get; set; }
  }
}