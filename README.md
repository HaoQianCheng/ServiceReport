# ServiceReport

![image-20230301231817726](https://user-images.githubusercontent.com/86709205/222441329-28940016-ae86-462a-b28d-40b795b7f24e.png)



## Introduce

**ServiceReport** is a monitoring plug-in. NET core services. Its main function is monitoring, analysis and tracking. It is well suited for business analysis. 

The attempt page uses the **Ant Design for Blazor** framework.

## Document ##

* Step 1: Introduce the nuget package

  ```c#
  NuGet\Install-Package ServiceReport -Version 1.0.0
  ```

  ![image-20230302202518443](https://user-images.githubusercontent.com/86709205/222441354-7b4765c2-980c-4034-8d4c-6fcec0473aef.png)

  

* Step 2:Modify the appsetting.json file of the project

  ```json
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
      "RecordCookie": true,
      "RecordHeader": true,
      "RequestFilter": [
      ]
    }
  ```

  > Parameter Specification:
  >
  > ​	ServiceReportSendOptions-
  >
  > ​		PushAddress(Service address for sending data)
  >
  > ​		PushSecond(Data sent per second)
  >
  > ​		PushCount(Amount of data sent at a time)
  >
  > ​	Server(Service address)
  >
  > ​	ServiceName(Service name)
  >
  > ​	RecordRequest(Whether to record request)
  >
  > ​	RecordResponse(Whether to record response)
  >
  > ​	RecordCookie(Whether to record cookie)
  >
  > ​	RecordHeader(Whether to record http header)
  >
  > ​	RecordHeader(Filter path)

* Step 3: 下载 tem/ServiceReport.Ui.Web 模板

  ![image-20230302212110974](https://user-images.githubusercontent.com/86709205/222441397-88ff73c5-f73c-4067-9152-72853dc1eef8.png) 

