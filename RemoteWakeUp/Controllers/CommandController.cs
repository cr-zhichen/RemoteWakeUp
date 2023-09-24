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
    [HttpPost("verifyToken")]
    public async Task<IRe<object>> VerifyToken()
    {
        Response.Headers.Add("Cache-Control", "no-store");
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
    [HttpGet("getDevices")]
    public async Task<IRe<object>> GetDevices()
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
    /// 唤醒设备，mac地址和子网地址可选
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [Auth]
    [HttpPost("wakeUp")]
    public async Task<IRe<object>> WakeUp(Req.WakeUp? data)
    {
        if (string.IsNullOrEmpty(data.SubnetBroadcastAddress))
        {
            data.SubnetBroadcastAddress = _configuration.GetSection("WakeUp:SubnetBroadcastAddress").Value;
        }

        if (data.MacList == null || data.MacList?.Count == 0)
        {
            List<WakeUpDevice> devices = _configuration.GetSection("WakeUp:MacList").Get<List<WakeUpDevice>>();
            foreach (var device in devices)
            {
                SendWakeOnLan.SendWakeOnLanPacket(device.MAC, data.SubnetBroadcastAddress);
            }

            return new Ok<object>()
            {
                Message = "发送WOL数据包成功",
                Code = 0,
                Data = devices
            };
        }
        else
        {
            List<WakeUpDevice> devices = new List<WakeUpDevice>();
            foreach (var mac in data.MacList)
            {
                devices.Add(new WakeUpDevice()
                {
                    Name = $"未知设备{devices.Count + 1}",
                    IP = "",
                    MAC = mac
                });
            }

            foreach (var device in devices)
            {
                SendWakeOnLan.SendWakeOnLanPacket(device.MAC, data.SubnetBroadcastAddress);
            }

            return new Ok<object>()
            {
                Message = "发送WOL数据包成功",
                Code = 0,
                Data = devices
            };
        }
    }

    /// <summary>
    /// 根据传入的ip判断是否在线
    /// </summary>
    /// <param name="ip"></param>
    /// <returns></returns>
    [Auth]
    [HttpGet("isOnline")]
    public async Task<IRe<object>> IsOnline(string ip)
    {
        Response.Headers.Add("Cache-Control", "no-store, no-cache, must-revalidate, proxy-revalidate");
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