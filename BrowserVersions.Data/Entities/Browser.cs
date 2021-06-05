namespace BrowserVersions.Data.Entities {
  using System.Collections.Generic;
  using BrowserVersions.Data.Enums;

  public class Browser {
    public Browser() {
      this.Versions = new HashSet<Version>();
    }

    public int BrowserId { get; set; }
    
    public TargetBrowser Type { get; set; }
    
    public Platform Platform { get; set; }
    
    public ICollection<Version> Versions { get; set; }
  }
}