namespace ServiceReport.Saver.Abstract
{
    public interface IServiceReportHttpClient
    {
        Task<bool> SendAsync<T>(IEnumerable<T> list, string action);
    }
}