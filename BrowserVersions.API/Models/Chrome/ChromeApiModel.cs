namespace BrowserVersions.API.Models.Chrome {
  using System.Collections.Generic;

  public class ChromeApiModel {
    public string os { get; set; }
    
    public List<ChromeApiVersion> versions { get; set; }
  }
}