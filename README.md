# ServiceReport

![image-20230301231817726](C:\Users\qianc\AppData\Roaming\Typora\typora-user-images\image-20230301231817726.png)



## Introduce

**ServiceReport** is a monitoring plug-in. NET core services. Its main function is monitoring, analysis and tracking. It is well suited for business analysis. 

The attempt page uses the **Ant Design for Blazor** framework.

## Document ##

* Step 1: Introduce the nuget package

  xxxxx

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

  Parameter Specification:

  ServiceReportSendOptions-

  ​	PushAddress(Service address for sending data)

  ​	PushSecond(Data sent per second)

  ​	PushCount(Amount of data sent at a time)

