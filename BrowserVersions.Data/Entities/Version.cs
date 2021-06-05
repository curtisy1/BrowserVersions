namespace BrowserVersions.Data.Entities {
  using System;
  using System.Collections.Generic;

  public class Version {
    public Version() {
      this.Browsers = new HashSet<Browser>();
    }
 
    public int VersionId { get; set; }
    
    public string VersionCode { get; set; }
    
    public DateTime ReleaseDate { get; set; }
    
    public DateTime? EndOfSupportDate { get; set; }
    
    public ICollection<Browser> Browsers { get; set; }
  }
}