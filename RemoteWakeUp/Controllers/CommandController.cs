using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Mvc;
using RemoteWakeUp.Entity.Re;

namespace RemoteWakeUp.Controllers;

/// <summary>
/// 命令控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CommandController : ControllerBase
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="configuration"></param>
    public CommandController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// 远程唤醒电脑
    /// </summary>
    /// <returns></returns>
    [HttpGet("wakeUp")]
    public async Task<IRe<object>> WakeUp()
    {
        List<string> macList = _configuration.GetSection("WakeUp:MacList").Get<List<string>>();

        foreach (var mac in macList)
        {
            SendWakeOnLanPacket(mac);
        }

        return new Ok<object>()
        {
            Message = "发送WOL数据包成功",
            Code = 0,
            Data = macList
        };
    }

    private void SendWakeOnLanPacket(string macAddress)
    {
        const int WOL_PORT = 9;

        // 将MAC地址转换为字节数组
        byte[] macBytes = MacAddressToByteArray(macAddress);

        // 创建魔术包
        byte[] magicPacket = new byte[6 + (16 * macBytes.Length)];
        for (int i = 0; i < 6; i++)
        {
            magicPacket[i] = 0xFF;
        }

        for (int i = 6; i < magicPacket.Length; i += macBytes.Length)
        {
            macBytes.CopyTo(magicPacket, i);
        }

        // 使用UDP发送魔术包
        using (UdpClient client = new UdpClient())
        {
            client.Send(magicPacket, magicPacket.Length, new IPEndPoint(IPAddress.Broadcast, WOL_PORT));
        }
    }

    private byte[] MacAddressToByteArray(string macAddress)
    {
        return macAddress.Split(':')
            .Select(part => Convert.ToByte(part, 16))
            .ToArray();
    }
}