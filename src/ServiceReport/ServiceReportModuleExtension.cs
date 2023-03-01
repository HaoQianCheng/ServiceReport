using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using ServiceReport;
using ServiceReport.Background;
using ServiceReport.Core.Builder;
using ServiceReport.Core.Options;
using ServiceReport.Resolver;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceReportModuleExtension
{
    /// <summary>
    /// 添加基本解析服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddServiceReportModule(this IServiceCollection services, IConfiguration configuration = null)
    {
        if (configuration == null)
        {
            configuration = services
            .BuildServiceProvider()
            .GetService<IConfiguration>() ?? throw new ArgumentNullException();
        }

        //Config
        services.AddOptions()
        .Configure<ServiceReportOptions>(configuration.GetSection(ServiceReportOptions.Position));

        //JsonConvert
        JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };
        services.AddSingleton(jsonSerializerOptions);

        //Http处理器
        services.AddSingleton<HttpResolver>();

        //Http 解析
        services.AddSingleton<IRequestBuilder, HttpRequestBuilder>();

        //Background
        services.AddHostedService<RequestServiceReportBackgroundScopedService>();
        services.AddSingleton<IRequestProcessingService, RequestProcessingService>();

        services.AddHostedService<GcServiceReportBackgroundScopedService>();
        services.AddSingleton<IGcProcessingService, GcProcessingService>();

        return services;
    }

    public static IApplicationBuilder UseHttpReports(this IApplicationBuilder app)
    {
        app.UseMiddleware<ServiceReportMiddleware>();

        return app;
    }
}