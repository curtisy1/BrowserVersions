namespace BrowserVersions.API.Models.Chrome {
  using System;
  using System.Collections.Generic;

  public class ChromeHistoricalDataWrapper {
    public IEnumerable<ChromeHistoricalData> releases { get; set; }
  }

  public class ChromeHistoricalData {
    public string name { get; set; }
    
    public ServingTime serving { get; set; }
    
    public decimal fraction { get; set; }
    
    public string version { get; set; }
  }

  public class ServingTime {
    public DateTime startTime { get; set; }
    
    public DateTime? endTime { get; set; }
  }
}