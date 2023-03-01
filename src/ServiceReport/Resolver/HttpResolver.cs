using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceReport.Core;
using ServiceReport.Core.Builder;
using ServiceReport.Core.Options;
using ServiceReport.Saver.Abstract;
using System.Diagnostics;
using System.IO.Compression;
using System.Web;

namespace ServiceReport.Resolver;

/// <summary>
/// Http 解析器
/// </summary>
public class HttpResolver
{
    private readonly ServiceReportOptions _serviceReportOptions;

    private readonly IRequestBuilder _requestBuilder;

    private readonly IServiceReportSaver _serviceReportSaver;

    private readonly ILogger<HttpResolver> _logger;

    public HttpResolver(IServiceReportSaver serviceReportSaver,
     IRequestBuilder requestBuilder,
      IOptions<ServiceReportOptions> serviceReportOptions,
      ILogger<HttpResolver> logger)
    {
        _serviceReportSaver = serviceReportSaver;
        _requestBuilder = requestBuilder;
        _serviceReportOptions = serviceReportOptions?.Value ?? throw new Exception();
        _logger = logger;
    }

    public async Task InvokeAsync(RequestDelegate next, HttpContext httpContext)
    {
        //Filter
        if (FilterRequest(httpContext))
        {
            await next(httpContext);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        stopwatch.Start();

        //Id
        Masuit.Tools.Systems.SnowFlake sf = Masuit.Tools.Systems.SnowFlake.GetInstance();
        var traceId = sf.GetUniqueId();
        httpContext.Items.Add(ServiceReportConst.TraceId, traceId);
        //SetResponse
        httpContext.Response.Headers.Add(ServiceReportConst.TraceId, traceId);

        //CreateTime
        httpContext.Items.Add(ServiceReportConst.CreateTime, DateTime.Now);

        //请求Body
        string requestBody = await GetRequestBodyAsync(httpContext);

        var bodyStream = httpContext.Response.Body;

        var responseMemoryStream = new MemoryStream();

        try
        {
            httpContext.Response.Body = responseMemoryStream;

            await next(httpContext);
        }
        catch (System.Exception ex)
        {
            httpContext.Items.Add(ServiceReportConst.GlobalException, ex);

            _logger.LogError(ex.ToString());
        }
        finally
        {
            stopwatch.Stop();
            //响应Body
            string responseBody = await GetResponseBodyAsync(httpContext);

            httpContext.Items.Add(ServiceReportConst.TraceMilliseconds, stopwatch.ElapsedMilliseconds);
            httpContext.Items.Add(ServiceReportConst.RequestBody, requestBody);
            httpContext.Items.Add(ServiceReportConst.ResponseBody, responseBody);

            if (responseMemoryStream.CanRead && responseMemoryStream.CanSeek)
            {
                await responseMemoryStream.CopyToAsync(bodyStream);

                responseMemoryStream.Dispose();
            }

            var (info, detail) = _requestBuilder.Build(httpContext);

            await _serviceReportSaver.SaveRequestAsync(new Core.Model.Request(detail, info));
        }
    }

    /// <summary>
    ///  获取Request Body请求数据
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task<string> GetRequestBodyAsync(HttpContext context)
    {
        try
        {
            if (_serviceReportOptions.RecordRequest && !string.IsNullOrEmpty(context.Request.ContentType) && !context.Request.ContentType.Contains("application/json"))
            {
                return string.Empty;
            }

            string result = string.Empty;

            context.Request.EnableBuffering();

            var requestReader = new StreamReader(context.Request.Body, System.Text.Encoding.UTF8);

            result = await requestReader.ReadToEndAsync();

            context.Request.Body.Position = 0;

            return HttpUtility.HtmlDecode(result);
        }
        catch (Exception ex)
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// 获取Response Body 中的数据
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task<string> GetResponseBodyAsync(HttpContext context)
    {
        if (_serviceReportOptions.RecordResponse && !string.IsNullOrEmpty(context.Response.ContentType) && !context.Response.ContentType.Contains("application/json") && context.Response.ContentLength >= ServiceReportConst.ResponseBodyMaxLength)
        {
            if (context.Response.Body.CanSeek)
            {
                context.Response.Body.Seek(0, SeekOrigin.Begin);
            }
            return string.Empty;
        }

        string result = string.Empty;

        context.Response.Body.Seek(0, SeekOrigin.Begin);

        Stream source = null;

        if (context.Response.Headers.ContainsKey("Content-Encoding"))
        {
            var contentEncoding = context.Response.Headers["Content-Encoding"].ToString();
            switch (contentEncoding)
            {
                case "gzip":
                    source = new GZipStream(context.Response.Body, CompressionMode.Decompress);
                    break;

                case "deflate":
                    source = new DeflateStream(context.Response.Body, CompressionMode.Decompress);
                    break;

                case "br":
                    source = new BrotliStream(context.Response.Body, CompressionMode.Decompress);
                    break;
            }
        }

        if (source == null)
        {
            source = context.Response.Body;
        }

        var responseReader = new StreamReader(source, System.Text.Encoding.UTF8);
        result = await responseReader.ReadToEndAsync();
        context.Response.Body.Seek(0, SeekOrigin.Begin);

        return HttpUtility.UrlDecode(result);
    }

    /// <summary>
    /// 过滤检测请求
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private bool FilterRequest(HttpContext context)
    {
        if (_serviceReportOptions.RequestFilter == null)
        {
            return false;
        }

        var path = context.Request.Path.ToString();

        foreach (var item in _serviceReportOptions.RequestFilter)
        {
            if (item.ToLowerInvariant().Contains(path.ToLowerInvariant()))
            {
                return true;
            }
        }
        return false;
    }
}