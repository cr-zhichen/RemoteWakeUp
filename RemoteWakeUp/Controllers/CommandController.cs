using System.Net.NetworkInformation;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
    /// 获取reCAPTCHA:Client的值
    /// </summary>
    /// <returns></returns>
    [HttpGet("getRecaptchaClient")]
    public async Task<IRe<object>> GetRecaptchaClient()
    {
        var client = _configuration.GetSection("reCAPTCHA:Client").Value;

        if (!string.IsNullOrEmpty(client))
        {
            return new Ok<object>()
            {
                Message = "获取reCAPTCHA:Client成功",
                Data = client
            };
        }
        else
        {
            return new Error<object>()
            {
                Message = "获取reCAPTCHA:Client失败",
            };
        }
    }

    /// <summary>
    /// 登录
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost("login")]
    public async Task<IRe<object>> Login(Req.Login data)
    {
        var secretKey = _configuration.GetSection("reCAPTCHA:Server").Value;

        if (!string.IsNullOrEmpty(secretKey))
        {
            var client = new HttpClient();
            var response = await client.GetStringAsync(
                $"https://recaptcha.net/recaptcha/api/siteverify?secret={secretKey}&response={data.RecaptchaResponse}");
            var recaptchaResult = JsonConvert.DeserializeObject<RecaptchaResult>(response);

            Console.WriteLine(
                $"reCAPTCHA验证结果：{recaptchaResult.Success},验证分数：{recaptchaResult.Score},验证时间：{recaptchaResult.ChallengeTimestamp},验证主机：{recaptchaResult.Hostname},错误代码：{recaptchaResult.ErrorCodes}");

            if (!recaptchaResult.Success)
            {
                return new Error<object>()
                {
                    Message = "reCAPTCHA验证失败",
                };
            }

            if (recaptchaResult.Score < 0.5)
            {
                return new Error<object>()
                {
                    Message = "reCAPTCHA验证失败",
                };
            }
        }


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

        return new Ok<object>()
        {
            Message = "获取设备在线状态成功",
            Data = (pingReply.Status == IPStatus.Success)
        };
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

    /// <summary>
    /// reCAPTCHA验证结果实体类
    /// </summary>
    public class RecaptchaResult
    {
        [JsonProperty("success")] public bool Success { get; set; }

        [JsonProperty("score")] public float Score { get; set; }

        [JsonProperty("challenge_ts")] public DateTime ChallengeTimestamp { get; set; }

        [JsonProperty("hostname")] public string Hostname { get; set; }

        [JsonProperty("error-codes")] public List<string> ErrorCodes { get; set; }
    }
}