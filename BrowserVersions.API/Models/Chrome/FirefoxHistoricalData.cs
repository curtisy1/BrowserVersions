namespace BrowserVersions.API.Models.Chrome {
  using System;
  using System.Collections.Generic;

  public class FirefoxHistoricalDataWrapper {
    public Dictionary<string, IEnumerable<FirefoxHistoricalData>> releases { get; set; }
  }

  public class FirefoxNightlyDataWrapper {
    public IEnumerable<FirefoxNightlyData> builds { get; set; }
  }

  public class FirefoxNightlyData {
    public string node { get; set; }
    
    public string buildid { get; set; }
    
    public string channel { get; set; }
    
    public string platform { get; set; }
    
    public string app_version { get; set; }
    
    public string files_url { get; set; }
  }

  public class FirefoxFlavorData {
    public IEnumerable<FirefoxHistoricalData> dev { get; set; } // Firefox Developer Edition

    public IEnumerable<FirefoxHistoricalData> fennec { get; set; } // Firefox Mobile Legacy

    public IEnumerable<FirefoxHistoricalData> fenix { get; set; } // Firefox Mobile Current

    public IEnumerable<FirefoxHistoricalData> firefox { get; set; } // Firefox Desktop
  }

  public class FirefoxHistoricalData {
    public int build_number { get; set; }

    public string category { get; set; }

    public DateTime date { get; set; }

    public string description { get; set; }

    public bool is_security_driven { get; set; }

    public string product { get; set; }

    public string version { get; set; }
  }
}