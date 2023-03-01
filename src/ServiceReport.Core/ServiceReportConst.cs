namespace ServiceReport.Core;

public class ServiceReportConst
{
    /// <summary>
    /// 请求总耗时
    /// </summary>
    public const string TraceId = "ServiceReport.TraceId";

    public const string RequestBody = "ServiceReport.RequestBody";

    public const int ResponseBodyMaxLength = 10000;
    public const string ResponseBody = "ServiceReport.ResponseBody";

    /// <summary>
    /// 请求总耗时
    /// </summary>
    public const string TraceMilliseconds = "ServiceReport.TraceMilliseconds";

    /// <summary>
    /// 请求执行时间
    /// </summary>
    public const string CreateTime = "ServiceReport.CreateTime";

    /// <summary>
    /// 异常捕获
    /// </summary>
    public const string GlobalException = "ServiceReport.GlobalException";
}