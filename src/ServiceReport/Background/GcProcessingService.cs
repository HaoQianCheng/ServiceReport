using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceReport.Core.Model;
using ServiceReport.Saver.Abstract;
using ServiceReport.Saver.Http;
using ServiceReport.Saver.Http.Options;
using System.Diagnostics;

namespace ServiceReport.Background
{
    public interface IGcProcessingService
    {
        Task DoWork(CancellationToken stoppingToken);
    }

    internal class GcProcessingService : IGcProcessingService
    {
        private static object obj = new Object();

        private readonly ILogger _logger;

        private readonly IServiceReportSaver _serviceReportSaver;

        private readonly HttpServiceReportSaverOptions _httpServiceReportSaverOptions;

        private readonly IServiceReportHttpClient _serviceReportHttpClient;

        public GcProcessingService(ILogger<RequestProcessingService> logger,
        IServiceReportSaver serviceReportSaver,
        IOptions<HttpServiceReportSaverOptions> options,
        IServiceReportHttpClient serviceReportHttpClient)
        {
            _logger = logger;
            _serviceReportSaver = serviceReportSaver;
            _httpServiceReportSaverOptions = options.Value ?? throw new ArgumentNullException();
            _serviceReportHttpClient = serviceReportHttpClient;
        }

        public Task DoWork(CancellationToken stoppingToken)
        {
            GcState monitorState = new GcState();
            //CPU
            Cpu(monitorState);

            //Gc
            Gc(monitorState);

            SaverData.MonitorDelayList.Add(monitorState);

            lock (obj)
            {
                var pushData = SaverData.MonitorDelayList.Take(_httpServiceReportSaverOptions.PushCount).ToList();

                if (pushData.Count() != 0)
                {
                    Cleaning(pushData);
                }
            }

            return Task.CompletedTask;
        }

        private void Cleaning(List<GcState> pushData)
        {
            if (_serviceReportHttpClient.SendAsync(pushData, "Collection/GcMonitor").GetAwaiter().GetResult())
            {
                try
                {
                    SaverData.MonitorDelayList.RemoveRange(0, _httpServiceReportSaverOptions.PushCount);
                }
                catch (ArgumentException ex)
                {
                    SaverData.MonitorDelayList.Clear();
                }
            };
        }

        private readonly Process process = Process.GetCurrentProcess();
        private readonly int _processorCount = Environment.ProcessorCount;

        private void Cpu(GcState monitorState)
        {
            var prevCpuTime = process.TotalProcessorTime.TotalMilliseconds;
            var currentCpuTime = process.TotalProcessorTime;
            var usagePercent = (currentCpuTime.TotalMilliseconds - prevCpuTime) / _processorCount * 100;
            // Interlocked.Exchange(ref _prevCpuTime, currentCpuTime.TotalMilliseconds);
            // Interlocked.Exchange(ref monitorState.CpuRate, usagePercent);
            monitorState.CpuRate = usagePercent;
        }

        private static long _prevGen0CollectCount;
        private static long _prevGen1CollectCount;
        private static long _prevGen2CollectCount;

        private static readonly int _maxGen = GC.MaxGeneration;

        private void Gc(GcState monitorState)
        {
            //0代
            var gc0Count = GC.CollectionCount(0);
            var gc0prevCount = _prevGen0CollectCount;
            Interlocked.Exchange(ref _prevGen0CollectCount, gc0Count);

            monitorState.GC0Count = gc0Count - gc0prevCount;

            //1代
            if (_maxGen > 1)
            {
                var gc1Count = GC.CollectionCount(1);
                var gc1prevCount = _prevGen1CollectCount;
                Interlocked.Exchange(ref _prevGen1CollectCount, gc1Count);

                monitorState.GC1Count = gc1Count - gc1prevCount;
            }

            //2代
            if (_maxGen > 1)
            {
                var gc2Count = GC.CollectionCount(2);
                var gc2prevCount = _prevGen2CollectCount;
                Interlocked.Exchange(ref _prevGen2CollectCount, gc2Count);

                monitorState.GC2Count = gc2Count - gc2prevCount;
            }

            //内存
            monitorState.TotalMemory = GC.GetTotalMemory(false);
        }
    }
}