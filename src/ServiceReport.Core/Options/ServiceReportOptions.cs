namespace ServiceReport.Core.Options;

public class ServiceReportOptions
{
    public const string Position = "ServiceReportOptions";

    /// <summary>
    /// 服务地址
    /// </summary>
    /// <value></value>
    public string Server { get; set; }

    /// <summary>
    /// 服务地址
    /// </summary>
    /// <value></value>
    public string ServiceName { get; set; }

    /// <summary>
    /// 请求过滤
    /// </summary>
    /// <value></value>
    public string[] RequestFilter { get; set; }

    /// <summary>
    /// 是否记录请求信息
    /// </summary>
    /// <value></value>
    public bool RecordRequest { get; set; }

    /// <summary>
    /// 是否记录响应信息
    /// </summary>
    /// <value></value>
    public bool RecordResponse { get; set; }

    /// <summary>
    /// 是否记录Cookie 信息
    /// </summary>
    /// <value></value>
    public bool RecordCookie { get; set; }

    /// <summary>
    /// 是否记录Header 头部信息
    /// </summary>
    /// <value></value>
    public bool RecordHeader { get; set; }
}