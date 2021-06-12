namespace BrowserVersions.API.Models {
  using System;

  public class VersionModel {
    public string Version { get; set; }
    
    public DateTime? ReleaseDate { get; set; }
  }
}