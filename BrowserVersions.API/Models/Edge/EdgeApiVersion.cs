namespace BrowserVersions.API.Models.Edge {
  public class EdgeApiVersion : ApiVersion {
    public string Canary { get; set; }
    
    public string Dev { get; set; }
    
    public string Beta { get; set; }
    
    public string Stable { get; set; }
  }
}