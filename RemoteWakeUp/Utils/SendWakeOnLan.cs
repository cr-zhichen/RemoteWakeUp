using System.Net;
using System.Net.Sockets;

namespace RemoteWakeUp.Utils;

public static class SendWakeOnLan
{
    /// <summary>
    /// 发送WOL数据包
    /// </summary>
    /// <param name="macAddress">目标设备的MAC地址</param>
    /// <param name="subnetBroadcastAddress">子网的广播地址</param>
    public static void SendWakeOnLanPacket(string macAddress, string? subnetBroadcastAddress)
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
            client.EnableBroadcast = true;
            client.Send(magicPacket, magicPacket.Length,
                new IPEndPoint(IPAddress.Parse(subnetBroadcastAddress ?? "255.255.255.255"), WOL_PORT));
        }
    }

    /// <summary>
    /// 将MAC地址转换为字节数组
    /// </summary>
    /// <param name="macAddress"></param>
    /// <returns></returns>
    private static byte[] MacAddressToByteArray(string macAddress)
    {
        return macAddress.Split(':')
            .Select(part => Convert.ToByte(part, 16))
            .ToArray();
    }
}