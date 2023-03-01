using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceReport.Saver.Abstract;
using ServiceReport.Saver.Http.Options;
using System.Net;
using System.Text.Json;

namespace ServiceReport.Saver.Http
{
    public class ServiceReportHttpClient : IServiceReportHttpClient
    {
        private readonly ILogger<HttpServiceReportSaver> _logger;

        private readonly IHttpClientFactory _httpClientFactory;

        private readonly JsonSerializerOptions _jsonSetting;

        private readonly HttpServiceReportSaverOptions _httpServiceReportSaverOptions;

        public ServiceReportHttpClient(ILogger<HttpServiceReportSaver> logger,
        IHttpClientFactory httpClientFactory,
        JsonSerializerOptions jsonSetting,
        IOptions<HttpServiceReportSaverOptions> options)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _jsonSetting = jsonSetting;
            _httpServiceReportSaverOptions = options.Value ?? throw new ArgumentNullException();
        }

        public async Task<bool> SendAsync<T>(IEnumerable<T> list, string action)
        {
            try
            {
                HttpContent content = new StringContent(System.Text.Json.JsonSerializer.Serialize(list, _jsonSetting));
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                var httpClient = _httpClientFactory.CreateClient();

                var response = await httpClient.PostAsync(_httpServiceReportSaverOptions.PushAddress + action, content);

                var result = await response.Content.ReadAsStringAsync();

                return response.StatusCode == HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return false;
            }
        }
    }
}