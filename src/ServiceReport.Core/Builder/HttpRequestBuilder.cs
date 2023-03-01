using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using ServiceReport.Core.Model;
using ServiceReport.Core.Options;
using System.Text.Json;
using System.Web;

namespace ServiceReport.Core.Builder;

public class HttpRequestBuilder : IRequestBuilder
{
    private readonly ServiceReportOptions _serviceReportOptions;

    private readonly JsonSerializerOptions _jsonSetting;

    public HttpRequestBuilder(IOptions<ServiceReportOptions> serviceReportOptions, JsonSerializerOptions JsonSetting)
    {
        _serviceReportOptions = serviceReportOptions.Value ?? throw new ArgumentNullException();
        _jsonSetting = JsonSetting;
    }

    public (RequestInfo, RequestDetail) Build(HttpContext context)
    {
        var info = GetRequestInfo(context);

        var detail = GetRequestDetail(context, info.Id);

        return (info, detail);
    }

    /// <summary>
    /// 获取 Request 基本信息
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    protected RequestInfo GetRequestInfo(HttpContext context)
    {
        var request = new RequestInfo();

        //Id
        if (context.Items.ContainsKey(ServiceReportConst.TraceId))
        {
            request.Id = context.Items[ServiceReportConst.TraceId].ToString() ?? "";
        }

        //Instance
        Uri uri = new Uri(_serviceReportOptions.Server);
        request.Instance = uri.Host + ":" + uri.Port;
        request.InstanceName = _serviceReportOptions.ServiceName;

        //RequestType
        request.RequestType = (context.Request.ContentType ?? string.Empty).Contains("grpc") ? "grpc" : "http";

        request.Url = context.Request.Path;
        request.Method = context.Request.Method;
        request.StatusCode = context.Response.StatusCode;
        request.RemoteIP = context.Connection.RemoteIpAddress?.MapToIPv4()?.ToString() ?? "";

        //TraceMilliseconds
        if (context.Items.ContainsKey(ServiceReportConst.TraceMilliseconds))
        {
            request.TraceMilliseconds = Convert.ToInt32(context.Items[ServiceReportConst.TraceMilliseconds].ToString());
        }

        //CreateTime
        if (context.Items.ContainsKey(ServiceReportConst.CreateTime))
        {
            request.CreateTime = Convert.ToDateTime(context.Items[ServiceReportConst.CreateTime]);
        }

        //Account
        request.AccountName = context.User?.Identity?.Name ?? string.Empty;

        return request;
    }

    protected RequestDetail GetRequestDetail(HttpContext context, string requestId)
    {
        RequestDetail model = new RequestDetail();
        model.RequestId = requestId;

        //cookie
        var cookies = context.Request.Cookies.ToDictionary(x => x.Key, x => x.Value);

        //Header
        var headers = context.Request.Headers.ToDictionary(x => x.Key, x => x.Value.ToString());
        if (_serviceReportOptions.RecordHeader)
        {
            model.Header = HttpUtility.HtmlDecode(System.Text.Json.JsonSerializer.Serialize(headers, _jsonSetting));
        }

        //Request
        if (context.Items.ContainsKey(ServiceReportConst.RequestBody))
        {
            model.RequestBody = context.Items[ServiceReportConst.RequestBody].ToString() ?? "";

            context.Items.Remove(ServiceReportConst.RequestBody);
        }

        //Response
        if (context.Items.ContainsKey(ServiceReportConst.ResponseBody))
        {
            model.ResponseBody = context.Items[ServiceReportConst.ResponseBody].ToString() ?? "";

            context.Items.Remove(ServiceReportConst.ResponseBody);
        }

        //QueryString
        model.QueryString = HttpUtility.UrlDecode(context.Request.QueryString.Value);

        //Exception
        if (context.Items.ContainsKey(ServiceReportConst.GlobalException))
        {
            Exception? exception = context.Items[ServiceReportConst.GlobalException] as Exception;

            if (exception != null)
            {
                model.ErrorMessage = exception.Message;
                model.ErrorStack = exception.StackTrace ?? "";
            }
        }

        return model;
    }
}