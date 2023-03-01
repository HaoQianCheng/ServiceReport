namespace ServiceReport.Core.Model
{
    public class GcState
    {
        public int Id { get; set; }

        /// <summary>
        /// CPU占用率
        /// </summary>
        /// <value></value>
        public double CpuRate { get; set; }

        /// <summary>
        /// 0代GC数量
        /// </summary>
        /// <value></value>
        public long GC0Count { get; set; }

        /// <summary>
        /// 0代GC数量
        /// </summary>
        /// <value></value>
        public long GC1Count { get; set; }

        /// <summary>
        /// 0代GC数量
        /// </summary>
        /// <value></value>
        public long GC2Count { get; set; }

        public long TotalMemory { get; set; }
    }
}