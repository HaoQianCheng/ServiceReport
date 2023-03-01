namespace ServiceReport.Core.Model
{
    public class Request
    {
        public Request()
        {
        }

        public Request(RequestDetail requestDetail, RequestInfo requestInfo)
        {
            RequestDetail = requestDetail;
            RequestInfo = requestInfo;
        }

        public RequestDetail RequestDetail { get; set; } = new RequestDetail();

        public RequestInfo RequestInfo { get; set; } = new RequestInfo();
    }
}