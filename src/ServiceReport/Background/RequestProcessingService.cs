using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceReport.Core.Model;
using ServiceReport.Saver.Abstract;
using ServiceReport.Saver.Http;
using ServiceReport.Saver.Http.Options;

namespace ServiceReport.Background;

public interface IRequestProcessingService
{
    Task DoWork(CancellationToken stoppingToken);
}

internal class RequestProcessingService : IRequestProcessingService
{
    private static object obj = new Object();

    private readonly ILogger _logger;

    private readonly IServiceReportSaver _serviceReportSaver;

    private readonly HttpServiceReportSaverOptions _httpServiceReportSaverOptions;

    private readonly IServiceReportHttpClient _serviceReportHttpClient;

    public RequestProcessingService(ILogger<RequestProcessingService> logger,
        IServiceReportSaver serviceReportSaver,
        IOptions<HttpServiceReportSaverOptions> option,
        IServiceReportHttpClient serviceReportHttpClient)
    {
        _logger = logger;
        _serviceReportSaver = serviceReportSaver;
        _httpServiceReportSaverOptions = option.Value ?? throw new ArgumentNullException(nameof(HttpServiceReportSaverOptions));
        _serviceReportHttpClient = serviceReportHttpClient;
    }

    public async Task DoWork(CancellationToken stoppingToken)
    {
        var pushDataRequestData = SaverData.RequestDelayList.Take(_httpServiceReportSaverOptions.PushCount).ToList();

        if (pushDataRequestData.Count() != 0)
        {
            await Cleaning(pushDataRequestData);
        }
    }

    private async Task Cleaning(List<Request> pushData)
    {
        if (await _serviceReportHttpClient.SendAsync(pushData, "Collection/CollectionRequest"))
        {
            try
            {
                SaverData.RequestDelayList.RemoveRange(0, _httpServiceReportSaverOptions.PushCount);
            }
            catch (ArgumentException ex)
            {
                SaverData.RequestDelayList.Clear();
            }
        };
    }
}