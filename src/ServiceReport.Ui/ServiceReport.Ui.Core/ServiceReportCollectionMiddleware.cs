using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ServiceReport.Core.Model;
using ServiceReport.Ui.Storage;
using System.Text.Json;

namespace ServiceReport.Ui.Core
{
    public class ServiceReportCollectionMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private IDbContextFactory<ApplicationContext> DbFactory { get; set; }

        public ServiceReportCollectionMiddleware(RequestDelegate next,
            JsonSerializerOptions jsonSerializerOptions,
            IDbContextFactory<ApplicationContext> dbContextFactory)
        {
            _next = next;
            DbFactory = dbContextFactory;
            _jsonSerializerOptions = jsonSerializerOptions;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            string path = httpContext.Request.Path.Value ?? "";

            if (path.ToLower().Contains("/collection") && httpContext.Request.Method.ToLower() == "post")
            {
                var context = await DbFactory.CreateDbContextAsync();

                string requestJson = await GetRequestBodyAsync(httpContext);

                switch (path.ToLower())
                {
                    case "/collection/collectionrequest":
                        {
                            var requests = System.Text.Json.JsonSerializer.Deserialize<List<Request>>(requestJson, _jsonSerializerOptions);

                            if (requests == null)
                            {
                                break;
                            }

                            if (requests.Count == 0)
                            {
                                break;
                            }

                            var requestDetails = requests
                                .Select(s => s.RequestDetail);

                            var requestInfos = requests
                                .Select(s => s.RequestInfo);

                            await context.AddRangeAsync(requestDetails);
                            await context.AddRangeAsync(requestInfos);

                            if (await context.SaveChangesAsync() == 0)
                            {
                                //Logging
                            }
                        }; break;

                    case "/collection/gcmonitor":
                        {
                            var gcs = System.Text.Json.JsonSerializer.Deserialize<List<GcState>>(requestJson, _jsonSerializerOptions);

                            if (gcs == null)
                            {
                                break;
                            }

                            if (gcs.Count == 0)
                            {
                                break;
                            }

                            await context.AddRangeAsync(gcs);

                            if (await context.SaveChangesAsync() == 0)
                            {
                                //Log
                            }
                        }; break;

                    default:
                        break;
                }
            }

            await _next(httpContext);
        }

        private async Task<string> GetRequestBodyAsync(HttpContext context)
        {
            try
            {
                string result = string.Empty;

                context.Request.EnableBuffering();

                var requestReader = new StreamReader(context.Request.Body, System.Text.Encoding.UTF8);

                result = await requestReader.ReadToEndAsync();

                context.Request.Body.Position = 0;

                return result;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }
    }
}