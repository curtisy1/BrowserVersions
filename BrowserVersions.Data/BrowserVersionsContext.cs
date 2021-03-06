namespace BrowserVersions.Data {
  using System.Collections.Generic;
  using System.IO;
  using BrowserVersions.Data.Entities;
  using BrowserVersions.Data.Enums;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Logging;
  using Version = BrowserVersions.Data.Entities.Version;

  public class BrowserVersionsContext : DbContext {
    public DbSet<Version> Versions { get; set; }
    public DbSet<Browser> Browsers { get; set; }

    private readonly ILoggerFactory loggerFactory;
    private readonly string databasePath;
    
    public BrowserVersionsContext(ILoggerFactory loggerFactory) {
      this.loggerFactory = loggerFactory;
#if DEBUG
      this.databasePath = $"{Directory.GetCurrentDirectory()}/../BrowserVersions.Data/";
#else
      this.databasePath = "";
#endif
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options) 
      => options.UseSqlite($"Data Source={this.databasePath}browserVersions.db")
        .UseLoggerFactory(this.loggerFactory);

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
      AddBrowsers(modelBuilder);

      base.OnModelCreating(modelBuilder);
    }

    private static void AddBrowsers(ModelBuilder modelBuilder) {
      modelBuilder.Entity<Browser>().HasData(new List<Browser> {
        new() {
          BrowserId = 1,
          Type = TargetBrowser.Firefox,
          Platform = Platform.Desktop,
        },
        new() {
          BrowserId = 2,
          Type = TargetBrowser.Firefox,
          Platform = Platform.Android,
        },
        new() {
          BrowserId = 3,
          Type = TargetBrowser.Firefox,
          Platform = Platform.Ios,
        },
        new() {
          BrowserId = 4,
          Type = TargetBrowser.Chrome,
          Platform = Platform.Desktop,
        },
        new() {
          BrowserId = 5,
          Type = TargetBrowser.Chrome,
          Platform = Platform.Android,
        },
        new() {
          BrowserId = 6,
          Type = TargetBrowser.Chrome,
          Platform = Platform.Ios,
        },
        new() {
          BrowserId = 7,
          Type = TargetBrowser.Edge,
          Platform = Platform.Desktop,
        },
        new() {
          BrowserId = 8,
          Type = TargetBrowser.Edge,
          Platform = Platform.Android,
        },
        new() {
          BrowserId = 9,
          Type = TargetBrowser.Edge,
          Platform = Platform.Ios,
        },
        new() {
          BrowserId = 10,
          Type = TargetBrowser.InternetExplorer,
          Platform = Platform.Desktop,
        },
      });
    }
  }
}