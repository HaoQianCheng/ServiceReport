using ServiceReport.Core.Model;

namespace ServiceReport.Saver.Abstract;

public interface IServiceReportSaver
{
    Task SaveRequestAsync(Request request);

    Task SaveMonitorAsync(GcState monitorState);
}