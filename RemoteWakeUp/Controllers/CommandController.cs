using System.Net.NetworkInformation;
using Microsoft.AspNetCore.Mvc;
using RemoteWakeUp.Attribute;
using RemoteWakeUp.Entity.Re;
using RemoteWakeUp.Entity.Req;
using RemoteWakeUp.Jwt;
using RemoteWakeUp.Utils;

namespace RemoteWakeUp.Controllers;

/// <summary>
/// 命令控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CommandController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IJwtService _jwtService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="jwtService"></param>
    public CommandController(IConfiguration configuration, IJwtService jwtService)
    {
        _configuration = configuration;
        _jwtService = jwtService;
    }

    /// <summary>
    /// 登录
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost("login")]
    public async Task<IRe<object>> Login(Req.Login data)
    {
        if (_configuration.GetSection("WakeUp:Password").Value.Length == 0)
        {
            return new Ok<object>()
            {
                Message = "登录成功",
                Data = new
                {
                    Token = "No verification required"
                }
            };
        }

        if (data.Password == _configuration.GetSection("WakeUp:Password").Value)
        {
            return new Ok<object>()
            {
                Message = "登录成功",
                Data = new
                {
                    Token = _jwtService.CreateTokenAsync("admin", "admin").Result
                }
            };
        }
        else
        {
            return new Error<object>()
            {
                Message = "密码错误",
            };
        }
    }

    /// <summary>
    /// Token验证
    /// </summary>
    /// <returns></returns>
    [Auth]
    [HttpGet("test")]
    public async Task<IRe<object>> Test()
    {
        return new Ok<object>()
        {
            Message = "Token验证成功"
        };
    }

    /// <summary>
    /// 获取可被唤醒的设备列表
    /// </summary>
    /// <returns></returns>
    [Auth]
    [HttpGet("getWakeUpList")]
    public async Task<IRe<object>> GetWakeUpList()
    {
        List<WakeUpDevice> devices = _configuration.GetSection("WakeUp:MacList").Get<List<WakeUpDevice>>();

        return new Ok<object>()
        {
            Message = "获取可被唤醒的设备列表成功",
            Code = 0,
            Data = devices
        };
    }

    /// <summary>
    /// 唤醒全部设备
    /// </summary>
    /// <returns></returns>
    [Auth]
    [HttpGet("wakeUp")]
    public async Task<IRe<object>> WakeUp()
    {
        List<WakeUpDevice> devices = _configuration.GetSection("WakeUp:MacList").Get<List<WakeUpDevice>>();
        string? subnetBroadcastAddress = _configuration.GetSection("WakeUp:SubnetBroadcastAddress").Value;

        foreach (var device in devices)
        {
            SendWakeOnLan.SendWakeOnLanPacket(device.MAC, subnetBroadcastAddress);
        }

        return new Ok<object>()
        {
            Message = "发送WOL数据包成功",
            Code = 0,
            Data = devices
        };
    }

    /// <summary>
    /// 根据传入的name唤醒设备
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    [Auth]
    [HttpGet("wakeUpByName/{name}")]
    public async Task<IRe<object>> WakeUpByName(string name)
    {
        List<WakeUpDevice> devices = _configuration.GetSection("WakeUp:MacList").Get<List<WakeUpDevice>>();
        string? subnetBroadcastAddress = _configuration.GetSection("WakeUp:SubnetBroadcastAddress").Value;

        foreach (var device in devices)
        {
            if (device.Name == name)
            {
                SendWakeOnLan.SendWakeOnLanPacket(device.MAC, subnetBroadcastAddress);
                return new Ok<object>()
                {
                    Message = "发送WOL数据包成功",
                    Code = 0,
                    Data = device
                };
            }
        }

        return new Ok<object>()
        {
            Message = "未找到该设备",
            Code = 0,
            Data = name
        };
    }

    /// <summary>
    /// 根据传入的mac唤醒设备
    /// </summary>
    /// <param name="mac"></param>
    /// <param name="subnetBroadcastAddress">子网的广播地址,为空则255.255.255.255广播</param>
    /// <returns></returns>
    [Auth]
    [HttpGet("wakeUp/{mac}")]
    public async Task<IRe<object>> WakeUp(string mac, string? subnetBroadcastAddress)
    {
        SendWakeOnLan.SendWakeOnLanPacket(mac, subnetBroadcastAddress);

        return new Ok<object>()
        {
            Message = "发送WOL数据包成功",
            Code = 0,
            Data = mac
        };
    }

    /// <summary>
    /// 获取全部设备的在线状态
    /// </summary>
    /// <returns></returns>
    [Auth]
    [HttpGet("isOnline")]
    public async Task<IRe<object>> IsOnline()
    {
        List<WakeUpDevice> devices = _configuration.GetSection("WakeUp:MacList").Get<List<WakeUpDevice>>();

        foreach (var device in devices)
        {
            Ping ping = new Ping();
            PingReply pingReply = ping.Send(device.IP);
            if (pingReply.Status == IPStatus.Success)
            {
                device.Name += " 在线";
            }
            else
            {
                device.Name += " 离线";
            }
        }

        return new Ok<object>()
        {
            Message = "获取在线状态成功",
            Code = 0,
            Data = devices
        };
    }

    /// <summary>
    /// 根据传入的name判断是否在线
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    [Auth]
    [HttpGet("isOnlineByName/{name}")]
    public async Task<IRe<object>> IsOnlineByName(string name)
    {
        List<WakeUpDevice> devices = _configuration.GetSection("WakeUp:MacList").Get<List<WakeUpDevice>>();

        foreach (var device in devices)
        {
            if (device.Name == name)
            {
                Ping ping = new Ping();
                PingReply pingReply = ping.Send(device.IP);
                if (pingReply.Status == IPStatus.Success)
                {
                    return new Ok<object>()
                    {
                        Message = "在线",
                        Code = 0,
                        Data = device
                    };
                }
                else
                {
                    return new Ok<object>()
                    {
                        Message = "离线",
                        Code = 0,
                        Data = device
                    };
                }
            }
        }

        return new Ok<object>()
        {
            Message = "未找到该设备",
            Code = 0,
            Data = name
        };
    }

    /// <summary>
    /// 根据传入的ip判断是否在线
    /// </summary>
    /// <param name="ip"></param>
    /// <returns></returns>
    [Auth]
    [HttpGet("isOnline/{ip}")]
    public async Task<IRe<object>> IsOnline(string ip)
    {
        Ping ping = new Ping();
        PingReply pingReply = ping.Send(ip);
        if (pingReply.Status == IPStatus.Success)
        {
            return new Ok<object>()
            {
                Message = "在线",
                Code = 0,
                Data = ip
            };
        }
        else
        {
            return new Ok<object>()
            {
                Message = "离线",
                Code = 0,
                Data = ip
            };
        }
    }

    /// <summary>
    /// 读取配置文件中的设备信息实体类
    /// </summary>
    public class WakeUpDevice
    {
        public string Name { get; set; }
        public string IP { get; set; }
        public string MAC { get; set; }
    }
}