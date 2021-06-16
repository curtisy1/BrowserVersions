namespace BrowserVersions.API {
  using System;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.Extensions.Hosting;
  using Microsoft.Extensions.Logging;
  using NLog;
  using NLog.Web;
  using LogLevel = Microsoft.Extensions.Logging.LogLevel;

  public class Program {
    public static async Task Main(string[] args) {
      var logger = LogManager.Setup()
        .LoadConfigurationFromAppSettings()
        .GetCurrentClassLogger();

      try {
        logger.Debug("init main");
        await CreateHostBuilder(args).Build().RunAsync();
      } catch (Exception exception) {
        logger.Error(exception, "Stopped program because of exception");
        throw;
      } finally {
        LogManager.Shutdown();
      }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
      Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
        .ConfigureLogging(logging => {
          logging.ClearProviders();
          logging.SetMinimumLevel(LogLevel.Trace);
        })
        .UseNLog();
  }
}