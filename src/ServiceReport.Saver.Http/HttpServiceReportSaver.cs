using ServiceReport.Core.Model;
using ServiceReport.Saver.Abstract;

namespace ServiceReport.Saver.Http;

public class HttpServiceReportSaver : IServiceReportSaver
{
    public HttpServiceReportSaver()
    {
    }

    public async Task SaveRequestAsync(Request request)
    {
        SaverData.RequestDelayList.Add(request);

        await Task.CompletedTask;
    }

    public async Task SaveMonitorAsync(GcState monitorState)
    {
        SaverData.MonitorDelayList.Add(monitorState);

        await Task.CompletedTask;
    }
}