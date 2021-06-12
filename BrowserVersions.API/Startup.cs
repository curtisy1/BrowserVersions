namespace BrowserVersions.API {
  using System;
  using BrowserVersions.API.Jobs;
  using BrowserVersions.API.Services;
  using BrowserVersions.Data;
  using Microsoft.AspNetCore.Builder;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Hosting;
  using Microsoft.OpenApi.Models;
  using Quartz;

  public class Startup {
    public Startup(IConfiguration configuration) {
      this.Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services) {
      services.AddHttpClient();
      services.AddDbContext<BrowserVersionsContext>();
      services.AddScoped<IBrowserVersionService, BrowserVersionService>();
      services.AddScoped<IBrowserVersionSeedingService, BrowserVersionSeedingService>();
      services.AddTransient<BrowserVersionUpdateJob>();

      services.AddControllers();
      services.AddSwaggerGen(c => {
        c.SwaggerDoc("v1", new OpenApiInfo {
          Title = "BrowserVersions.API",
          Version = "v1"
        });
      });

      services.AddQuartz(q => {
        // base quartz scheduler, job and trigger configuration
        q.UseMicrosoftDependencyInjectionJobFactory();
        q.UseSimpleTypeLoader();
        q.UseInMemoryStore();
        q.UseDefaultThreadPool(tp => { tp.MaxConcurrency = 10; });
        
        q.ScheduleJob<BrowserVersionUpdateJob>(trigger => trigger
          .WithIdentity("Data Update Trigger")
          .StartAt(DateBuilder.EvenSecondDate(DateTimeOffset.UtcNow.AddSeconds(7)))
          .WithDailyTimeIntervalSchedule(x => x.WithInterval(60, IntervalUnit.Second))
          .WithDescription("Job that fetches the latest versions from all apis and inserts new data to the database")
        );
      });

      // ASP.NET Core hosting
      services.AddQuartzServer(options => {
        // when shutting down we want jobs to complete gracefully
        options.WaitForJobsToComplete = true;
      });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
      if (env.IsDevelopment()) {
        app.UseDeveloperExceptionPage();
      }

      app.UseSwagger();
      app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BrowserVersions v1"));

      app.UseHttpsRedirection();

      app.UseRouting();

      app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
  }
}