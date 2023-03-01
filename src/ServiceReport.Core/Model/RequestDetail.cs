namespace ServiceReport.Core.Model;

/// <summary>
/// 请求详情
/// </summary>
public class RequestDetail
{
    public int Id { get; set; }
    public string RequestId { get; set; } = string.Empty;
    public string Cookie { get; set; } = string.Empty;
    public string Header { get; set; } = string.Empty;
    public string QueryString { get; set; } = string.Empty;
    public string RequestBody { get; set; } = string.Empty;
    public string ResponseBody { get; set; } = string.Empty;
    public string Scheme { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string ErrorStack { get; set; } = string.Empty;
}