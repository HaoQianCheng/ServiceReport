# ServiceReport

简体中文 | [English](README_EN.md)

ServiceReport 是面向 ASP.NET Core 服务的请求监控项目，包含采集中间件、HTTP 批量上报、MySQL 存储，以及基于 Blazor Server 和 Ant Design 的可视化界面。可用于查看服务请求、耗时和异常信息，辅助业务分析与问题排查。

> 当前源码面向 .NET 7。项目仍有未完成的功能和部署准备工作，详情见「当前状态与限制」。本文命令根据项目文件整理，未进行构建或端到端运行验证。

![ServiceReport 界面](https://user-images.githubusercontent.com/86709205/222441329-28940016-ae86-462a-b28d-40b795b7f24e.png)

## 功能与数据流

- 采集 HTTP 请求路径、方法、状态码、耗时、服务标识及异常详情。
- 生成请求标识，并通过响应头返回，便于定位单次请求。
- 将请求数据先保存在进程内，再按间隔和批量大小发送到采集端。
- 使用 Entity Framework Core 和 Pomelo MySQL 提供程序保存请求及相关数据。
- 提供请求总览、图表、请求列表与详情，以及按时间和服务筛选的界面。
- 包含监控任务、告警和 GC 指标的相关代码；这些功能的完成情况见下文。

数据流：业务 API → ServiceReport 中间件 → HTTP 上报 → UI 主机的采集端点 → MySQL → Blazor 页面。

采集端接收 `POST /Collection/CollectionRequest` 和 `POST /Collection/GcMonitor`。发送器会把端点路径拼接到 `PushAddress` 后，因此该地址应保留末尾的 `/`。

## 项目结构

```text
ServiceReport.sln
src/
├── ServiceReport/                    # ASP.NET Core 中间件与后台上报
├── ServiceReport.Core/               # 请求模型、构建器与配置
├── ServiceReport.Saver.Abstract/     # 存储和发送接口
├── ServiceReport.Saver.Http/         # HTTP 发送器与内存缓冲
└── ServiceReport.Ui/
    ├── ServiceReport.Ui.Core/        # 服务注册、采集端点与调度
    ├── ServiceReport.Ui.Storage/     # EF Core DbContext 与 MySQL 配置
    ├── ServiceReport.Ui.Web.Core/    # Blazor 页面和组件
    └── ServiceReport.Ui.Web/         # 使用源码项目引用的 UI 主机
test/
├── ApiService/                      # 接入 ServiceReport 的示例 API
├── Console/                         # 控制台占位示例
└── UI/                              # 使用 HttpReports 包的独立示例
tmp/
└── ServiceReport.Ui.Web/             # 使用 NuGet UI 包的主机模板
```

`test/UI` 使用的是 `HttpReports.Dashboard` 和 `HttpReports.MySQL`，不是本文启动的 ServiceReport UI。模板的正确路径是 [`tmp/ServiceReport.Ui.Web`](tmp/ServiceReport.Ui.Web)，不是 `tem/ServiceReport.Ui.Web`。

## 环境要求

- 能构建 `net7.0` 项目的 .NET SDK；所有已提交的 C# 项目均以 `net7.0` 为目标。
- 可访问依赖包源的 NuGet 环境。
- MySQL 数据库及与 [`ApplicationContext`](src/ServiceReport.Ui/ServiceReport.Ui.Storage/ApplicationContext.cs) 模型匹配的表结构。
- 使用示例 HTTPS 地址时，需要本机信任 ASP.NET Core 开发证书。

存储层使用 EF Core `7.0.2`、Pomelo MySQL `7.0.0`，并在代码中以 `ServerVersion.Parse("5.5")` 配置服务器版本。这是现有实现的设置，不代表已验证所有 MySQL 版本；连接自己的数据库前应核对兼容性。

**数据库准备限制：** 仓库没有提交 EF Core migrations 或 SQL 初始化脚本，已检查的启动路径也没有自动建表逻辑。仅填写连接字符串不足以完成首次部署，需要先准备并验证数据库结构。本文不提供未经验证的自动初始化命令。

## 从源码运行

### 1. 获取代码并还原依赖

```bash
git clone https://github.com/HaoQianCheng/ServiceReport.git
cd ServiceReport
dotnet restore src/ServiceReport.Ui/ServiceReport.Ui.Web/ServiceReport.Ui.Web.csproj
dotnet restore test/ApiService/ApiService.csproj
```

这里单独还原源码 UI 与示例 API，避免把包引用模板和其他示例混作同一启动入口。

### 2. 配置 UI 数据库

在 UI 主机的配置中设置 `MySqlOption:ConnectionString`。以下仅展示键名及占位格式，请通过环境变量或本地机密配置提供真实值，不要提交真实凭据：

```json
{
  "MySqlOption": {
    "ConnectionString": "server=localhost;port=3306;database=servicereport;uid=YOUR_USER;pwd=YOUR_PASSWORD;CharSet=utf8"
  }
}
```

对应的环境变量名为 `MySqlOption__ConnectionString`。数据库连接和表结构就绪后，启动源码 UI：

```bash
dotnet run --project src/ServiceReport.Ui/ServiceReport.Ui.Web/ServiceReport.Ui.Web.csproj --launch-profile ServiceReport.Ui.Web
```

该配置文件中的地址为 `https://localhost:5003` 和 `http://localhost:5004`。以实际控制台输出为准。

### 3. 配置并启动示例 API

编辑 [`test/ApiService/appsettings.json`](test/ApiService/appsettings.json) 中的 `ServiceReportOptions`，让 `PushAddress` 指向刚启动的 UI。完整配置示例见下一节。随后在另一个终端运行：

```bash
dotnet run --project test/ApiService/ApiService.csproj --launch-profile https
```

该 profile 使用 `https://localhost:7082` 和 `http://localhost:5169`。可打开 `https://localhost:7082/swagger` 调用示例接口，再查看 UI 中的请求数据。若修改监听端口，也应同步核对 `PushAddress` 和 `Server`。

## 接入自己的 ASP.NET Core 服务

### 安装与注册

原有文档提供的 NuGet 安装示例是：

```powershell
NuGet\Install-Package ServiceReport -Version 1.0.0
```

这是历史包版本示例；当前 [`ServiceReport.csproj`](src/ServiceReport/ServiceReport.csproj) 的源码版本为 `1.0.1`，不等同于已确认发布到包源的版本。使用 NuGet 时请自行核对可用版本及依赖；研究当前源码时可参考 `test/ApiService` 的项目引用方式。

在现有 `Program.cs` 中加入服务注册和中间件：

```csharp
using ServiceReport.Saver.Http;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddServiceReportModule()
    .AddServiceReportSendHttpModule();

var app = builder.Build();
app.UseHttpReports();
app.MapControllers();
app.Run();
```

这是最小接入示例，需按现有应用调整认证、授权、异常处理和中间件顺序。

![原有接入示例](https://user-images.githubusercontent.com/86709205/222441354-7b4765c2-980c-4034-8d4c-6fcec0473aef.png)

### 业务服务配置

将下面的节合并到业务服务的 `appsettings.json`。这是本地测试用配置；请只使用不含敏感信息的测试流量，并先阅读下方的采集限制。

```json
{
  "ServiceReportOptions": {
    "ServiceReportSendOptions": {
      "PushAddress": "https://localhost:5003/",
      "PushSecond": 1,
      "PushCount": 1000
    },
    "Server": "http://localhost:5169/",
    "ServiceName": "ApiServer",
    "RecordRequest": true,
    "RecordResponse": true,
    "RecordCookie": false,
    "RecordHeader": false,
    "RequestFilter": []
  }
}
```

| 配置 | 含义与注意事项 |
| --- | --- |
| `ServiceReportSendOptions.PushAddress` | UI/采集主机的基地址，保留末尾 `/`。 |
| `ServiceReportSendOptions.PushSecond` | 每轮请求上报完成后的等待秒数，并非每秒发送条数；应设置为正数。 |
| `ServiceReportSendOptions.PushCount` | 每批最多取出的记录数；应设置为正数。 |
| `Server` | 用于生成服务实例标识的绝对 URI，不负责设置应用监听地址。 |
| `ServiceName` | 页面上使用的服务名称。 |
| `RecordRequest` / `RecordResponse` | 请求/响应正文相关选项；当前实现不能把 `false` 当作可靠的禁用采集开关。 |
| `RecordCookie` | 配置类中存在该字段，但当前请求构建器未根据它持久化 Cookie 字段。 |
| `RecordHeader` | 是否序列化请求头。启用后可能包含 Cookie、授权信息等敏感内容。 |
| `RequestFilter` | 路径过滤列表。当前逻辑是不区分大小写的 `配置项.Contains(请求路径)`；不是通配符或通常的路径前缀匹配。 |

### 使用独立 UI 模板

可复制 [`tmp/ServiceReport.Ui.Web`](tmp/ServiceReport.Ui.Web) 作为包引用方式的 UI 主机。它引用 `ServiceReport.Ui.Core` 和 `ServiceReport.Ui.Web.Core` 的 `1.0.0` 包，需另外核对包源可用性；数据库准备和配置要求与源码 UI 相同。不要与源码 UI 同时占用默认端口。

![原有 UI 模板示例](https://user-images.githubusercontent.com/86709205/222441397-88ff73c5-f73c-4067-9152-72853dc1eef8.png)

## 当前状态与限制

- **采集隐私：** `HttpResolver` 中的正文判断逻辑不会在 `RecordRequest=false` 或 `RecordResponse=false` 时直接停止读取正文，查询字符串也会保存。接入真实数据前，应审查并修正采集逻辑、敏感字段脱敏和数据保留策略。
- **异常与响应：** 请求解析器会缓冲响应正文，并捕获下游异常而不重新抛出。请验证它与现有异常处理、大响应或流式接口的兼容性。
- **路由与 gRPC：** 根路径和包含 `.` 的路径会跳过采集。gRPC 分支尚未实现，而且该分支没有调用下一个中间件；不要直接用于 gRPC 服务。
- **可靠性与指标：** 待发送数据保存在进程内，没有持久化队列。GC 后台服务中的实际采集调用仍被注释，不能据此声称 GC/CPU 监控已启用。
- **告警：** 监控任务页面和邮件相关类已存在，但 `MonitorBackgroundJob.Execute` 尚未实现告警检测，邮件正文也留有 TODO。
- **访问控制：** 当前 UI 启动和采集路径未配置身份认证/授权保护。部署前应自行完善访问控制、网络隔离和传输安全，不要直接暴露在公网。
- **验证范围：** `test/` 下主要是示例应用，未发现自动化测试项目。本次文档整理未执行构建、测试、数据库初始化或 NuGet 包可用性验证。

## 贡献与许可证

欢迎通过 [Issues](https://github.com/HaoQianCheng/ServiceReport/issues) 反馈可复现的问题，或提交说明清楚的 Pull Request。提交前请移除凭据和真实请求数据，并注明使用的 .NET、数据库版本及复现步骤。

仓库目前未包含许可证文件。使用、修改或再分发前，请与维护者确认授权条件。
