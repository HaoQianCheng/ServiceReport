using AntDesign.ProLayout;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceReport.Ui.Storage;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace ServiceReport.Ui.Core
{
    public static class ServiceReportModuleExtensions
    {
        public static IServiceCollection AddServiceReportUiService(this IServiceCollection services, IConfiguration configuration = null)
        {
      

            if (configuration == null)
            {
                configuration = services
                .BuildServiceProvider()
                .GetService<IConfiguration>() ?? throw new ArgumentNullException();
            }

            //Http
            services.AddHttpClient();
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddEndpointsApiExplorer();
            services.AddStorageModule(configuration);

            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddAntDesign();
            services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri(sp.GetService<NavigationManager>().BaseUri)
            });

            JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            };
            services.AddSingleton(jsonSerializerOptions);

            services.Configure<ProSettings>(configuration.GetSection("ProSettings"));

            //Background
            services.AddSingleton<ScheduleService>();

            ServiceContainer.Provider = services.BuildServiceProvider();

            return services;
        }

        public static IApplicationBuilder UseServiceReportUi(this IApplicationBuilder app)
        {

            app.ApplicationServices.GetService<ScheduleService>().InitAsync().Wait();

            app.UseMiddleware<ServiceReportCollectionMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });

            return app;
        }
    }
}