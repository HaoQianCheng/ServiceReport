using Microsoft.AspNetCore.Http;
using ServiceReport.Core.Model;

namespace ServiceReport.Core.Builder;

public interface IRequestBuilder
{
    (RequestInfo, RequestDetail) Build(HttpContext context);
}