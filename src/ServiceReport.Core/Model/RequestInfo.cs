using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ServiceReport.Core.Model;

public class RequestInfo
{
    public RequestInfo()
    {
    }

    /// <summary>
    /// 唯一Id
    /// </summary>
    /// <value></value>
    [Key]
    public string Id { get; set; } = string.Empty;

    [DisplayName("实例地址")]
    public string Instance { get; set; } = string.Empty;

    [DisplayName("实例名称")]
    public string InstanceName { get; set; }

    [DisplayName("请求类型")]
    public string RequestType { get; set; } = string.Empty;

    [DisplayName("请求路径")]
    public string Url { get; set; } = string.Empty;

    [DisplayName("行为")]
    public string Method { get; set; } = string.Empty;

    [DisplayName("请求状态")]
    public int StatusCode { get; set; } = 0;

    [DisplayName("远程IP")]
    public string RemoteIP { get; set; } = string.Empty;

    [DisplayName("执行时长")]
    public int TraceMilliseconds { get; set; } = 0;

    [DisplayName("创建时间")]
    public DateTime CreateTime { get; set; }

    [DisplayName("创建时间")]
    public string CreateTimeStr { get; set; }

    [DisplayName("账户名称")]
    public string? AccountName { get; set; } = string.Empty;
}