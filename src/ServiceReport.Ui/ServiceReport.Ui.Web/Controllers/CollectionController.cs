using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServiceReport.Core.Model;
using ServiceReport.Ui.Core;
using ServiceReport.Ui.Storage;
using ServiceReport.Ui.Storage.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UI.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class CollectionCollector : ControllerBase
{
    private readonly ApplicationContext _applicationContext;

    private readonly ILogger<CollectionCollector> _logger;

    public CollectionCollector(ILogger<CollectionCollector> logger, ApplicationContext applicationContext)
    {
        _logger = logger;
        _applicationContext = applicationContext;
    }

    /// <summary>
    /// 收集请求信息
    /// </summary>
    /// <param name="_list"></param>
    /// <returns></returns>
    [HttpPost("/CollectionRequest")]
    public new async Task Request([FromBody] List<ServiceReport.Core.Model.Request> _list)
    {
        try
        {
            var requestDetails = _list
                .Select(s => s.RequestDetail);
            var requestInfos = _list
                .Select(s => s.RequestInfo);

            //相应时间监控
            //await MonitorResponse(requestInfos);

            await _applicationContext.AddRangeAsync(requestDetails);
            await _applicationContext.AddRangeAsync(requestInfos);

            if (await _applicationContext.SaveChangesAsync() == 0)
            {
                //Logging
            }
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex.ToString());
        }
    }

    private async Task MonitorResponse(IEnumerable<RequestInfo> requestInfos)
    {
        //响应时间监控
        var respnoseTask = MonitorTaskData.Tasks
              .Where(s => s.TimeOutIsOpen)
              .ToList();

        var instance = requestInfos.FirstOrDefault();

        if (instance != null)
        {
            if (respnoseTask.Any(s => s.InstanceName.Contains(instance.InstanceName)))
            {
                foreach (var item in respnoseTask)
                {
                    var requestTimeOut = requestInfos
                        .Where(s => s.TraceMilliseconds >= item.TimeOutTime);

                    if (requestTimeOut.Count() > 0)
                    {
                        MonitorWarning monitorWarning = new MonitorWarning()
                        {
                            Content = string.Format($"任务:【{item.TaskName}】 响应时间超时报警 --预警值：{item.TimeOutTime} --当前最大值：{requestTimeOut.Max(s => s.TraceMilliseconds)}"),
                            CreateTime = DateTime.Now
                        };

                        await _applicationContext.AddAsync(monitorWarning);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 收集GC 内存信息
    /// </summary>
    /// <param name="_list"></param>
    /// <returns></returns>
    [HttpPost("/GcMonitor")]
    public async Task Monitor([FromBody] List<GcState> _list)
    {
        try
        {
            await _applicationContext.AddRangeAsync(_list);

            if (await _applicationContext.SaveChangesAsync() == 0)
            {
                //Log
            }
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex.ToString());
        }
    }
}