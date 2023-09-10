# RemoteWakeUp 💡

通过 HTTP 请求从外网使用 WOL 唤醒内网机器的项目。此项目使用 .NET 6 进行构建，并支持 Docker 容器化运行，兼容多个平台。

## 特点 🌟

- 使用 .NET 6 开发。
- 支持跨平台。
- 内置 Dockerfile，支持 Docker 构建。
- 基于配置的 MAC 地址列表进行唤醒。
- 通过Docker的方式支持软路由环境下的部署。

## 如何开始 🚀

<details>
  <summary>从 DockerHub 拉取和运行</summary>

1. 从 DockerHub 拉取最新的镜像：

```bash
docker pull ccrui/remotewakeup:latest
```

2. 运行容器（使用默认配置）：

```bash
docker run -d -p 9000:9000 ccrui/remotewakeup:latest
```

</details>

<details>
  <summary>使用 Docker 构建和运行</summary>

1. 构建 Docker 镜像：

```bash
docker build -t remotewakeup .
```

2. 运行容器（使用默认配置）：

```bash
docker run -d -p 9000:9000 remotewakeup
```

</details>

<details>
  <summary>手动运行</summary>

确保已安装.NET 6 SDK。

1. 在项目根目录中还原 NuGet 包：

```bash
dotnet restore
```

2. 构建和运行应用：

```bash
dotnet run --project RemoteWakeUp/RemoteWakeUp.csproj
```

应用现在应该在 [http://localhost:9000](http://localhost:9000) 运行。


</details>

## API 🌐

- `GET /api/command/wakeUp`：发送 WOL 数据包到配置文件中指定的所有 MAC 地址。

## 配置 ⚙️

您可以在 `appsettings.json` 中修改配置。例如，您可以添加或删除 MAC 地址，以控制哪些设备可以被唤醒。

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://*:9000"
      }
    }
  },
  "TokenOptions": {
    "SecretKey": "[输入你的鉴权Key 随便填]",
    "Issuer": "[输入发行者，随便填]",
    "Audience": "[输入Audience，随便填]",
    "ExpireMinutes": 30
  },
  "WakeUp": {
    "MacList": [
      "AB:CD:EF:12:34:56"
    ]
  }
}
```

如果您使用Docker运行，则可以使用Docker环境变量的方式覆盖配置。例如，您可以使用以下命令运行容器：

```bash
docker run -d --restart=always -e "WakeUp__MacList__0=AB:CD:EF:12:34:56" -p 9000:9000 ccrui/remotewakeup:latest
```

其中 `WakeUp__MacList__0` 是第一个 MAC 地址。如果要添加更多 MAC 地址，可以按照以下格式：

```bash
docker run -d --restart=always -e "WakeUp__MacList__0=AB:CD:EF:12:34:56" -e "WakeUp__MacList__1=GH:IJ:KL:78:90:12" -p 9000:9000 ccrui/remotewakeup:latest
```

## 依赖 📦

- .NET 6
- Docker (如果使用容器化部署)

## 文档 📖

查看 Swagger 文档以获取 API 详细信息。默认地址为 [http://localhost:9000/swagger](http://localhost:9000/swagger)。

## 贡献 💪

欢迎提供问题反馈、功能建议或直接提交 Pull Request。

## 许可证 📜

本项目采用 [MIT 许可证](LICENSE)。