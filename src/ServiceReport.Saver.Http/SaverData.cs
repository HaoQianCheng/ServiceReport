using ServiceReport.Core.Model;

namespace ServiceReport.Saver.Http;

public static class SaverData
{
    public static List<Request> RequestDelayList = new List<Request>();

    public static List<GcState> MonitorDelayList = new List<GcState>();
}