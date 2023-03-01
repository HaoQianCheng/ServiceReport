namespace ServiceReport.Saver.Http.Options;

public class HttpServiceReportSaverOptions
{
    public const string Position = "ServiceReportSendOptions";

    /// <summary>
    /// 远程地址
    /// </summary>
    /// <value></value>
    public Uri PushAddress { get; set; }

    /// <summary>
    /// 推送秒
    /// </summary>
    /// <value></value>
    public int PushSecond { get; set; }

    /// <summary>
    /// 推送数量
    /// </summary>
    /// <value></value>
    public int PushCount { get; set; }
}