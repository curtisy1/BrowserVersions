namespace BrowserVersions.API {
  using BrowserVersions.API.Services;
  using Microsoft.AspNetCore.Builder;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Hosting;
  using Microsoft.OpenApi.Models;

  public class Startup {
    public Startup(IConfiguration configuration) {
      this.Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services) {
      services.AddHttpClient();
      services.AddScoped<IBrowserVersionService, BrowserVersionService>();
      
      services.AddControllers();
      services.AddSwaggerGen(c => {
        c.SwaggerDoc("v1", new OpenApiInfo {
          Title = "BrowserVersions.API",
          Version = "v1"
        });
      });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
      if (env.IsDevelopment()) {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BrowserVersions.API v1"));
      }

      app.UseHttpsRedirection();

      app.UseRouting();

      app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
  }
}