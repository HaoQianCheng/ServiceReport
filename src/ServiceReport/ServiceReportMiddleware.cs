using Microsoft.AspNetCore.Http;
using ServiceReport.Resolver;

namespace ServiceReport
{
    public class ServiceReportMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly HttpResolver _httpResolver;

        public ServiceReportMiddleware(RequestDelegate next,
        HttpResolver httpResolver)
        {
            _next = next;
            _httpResolver = httpResolver;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            if ((httpContext.Request.Path.HasValue && httpContext.Request.Path.Value == "/"))
            {
                await _next(httpContext);
                return;
            }

            if ((httpContext.Request.ContentType != null) && httpContext.Request.ContentType.Contains("application/grpc"))
            {
                // await _grpcProcessor.InvokeAsync(_next, httpContext);
            }
            else if (!httpContext.Request.Path.Value.Contains("."))
            {
                await _httpResolver.InvokeAsync(_next, httpContext);
            }
            else
            {
                await _next(httpContext);
            }
        }
    }
}