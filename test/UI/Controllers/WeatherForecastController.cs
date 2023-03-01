using Microsoft.AspNetCore.Mvc;

namespace UI.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class HttpReportsHttpCollector : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private static List<ServiceReport.Core.Model.Request> requests = new List<ServiceReport.Core.Model.Request>();
    private static List<ServiceReport.Core.Model.GcState> MonitorStates = new List<ServiceReport.Core.Model.GcState>();

    private readonly ILogger<HttpReportsHttpCollector> _logger;

    public HttpReportsHttpCollector(ILogger<HttpReportsHttpCollector> logger)
    {
        _logger = logger;
    }

    [HttpPost("/Request")]
    public void Request([FromBody] List<ServiceReport.Core.Model.Request> _list)
    {
        requests.AddRange(_list);
    }

    [HttpGet("/GetRequestLists")]
    public List<ServiceReport.Core.Model.Request> GetRequestList()
    {
        return requests;
    }

    [HttpPost("/Monitor")]
    public void Monitor([FromBody] List<ServiceReport.Core.Model.GcState> _list)
    {
        MonitorStates.AddRange(_list);
    }

    [HttpGet("/GetMonitorLists")]
    public List<ServiceReport.Core.Model.GcState> GetMonitorList()
    {
        return MonitorStates;
    }
}