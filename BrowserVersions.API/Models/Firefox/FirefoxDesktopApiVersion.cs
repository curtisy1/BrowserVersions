namespace BrowserVersions.API.Models.Firefox {
  using System;

  public class FirefoxDesktopApiVersion {
    public string FIREFOX_AURORA { get; set; }
    
    public string FIREFOX_DEVEDITION { get; set; }
    
    public string FIREFOX_ESR { get; set; }
    
    public string FIREFOX_ESR_NEXT { get; set; }
    
    public string FIREFOX_NIGHTLY { get; set; }
    
    public DateTime LAST_MERGE_DATE { get; set; }
    
    public DateTime LAST_RELEASE_DATE { get; set; }
    
    public DateTime LAST_SOFTFREEZE_DATE { get; set; }
    
    public string LATEST_FIREFOX_DEVEL_VERSION { get; set; }
    
    public string LATEST_FIREFOX_OLDER_VERSION { get; set; }
    
    public string LATEST_FIREFOX_RELEASED_DEVEL_VERSION { get; set; }
    
    public string LATEST_FIREFOX_VERSION { get; set; }
    
    public DateTime NEXT_MERGE_DATE { get; set; }
    
    public DateTime NEXT_RELEASE_DATE { get; set; }
    
    public DateTime NEXT_SOFTFREEZE_DATE { get; set; }
  }
}