using System.ComponentModel.DataAnnotations;

namespace RemoteWakeUp.Entity.Req;

public class Req
{
    public class Login
    {
        /// <summary>
        /// 密码
        /// </summary>
        [Required]
        public string? Password { get; set; }
    }

    public class WakeUp
    {
        /// <summary>
        /// MAC地址 (不传递则使用配置文件中的MAC地址)
        /// </summary>
        public List<string>? MacList { get; set; }

        /// <summary>
        /// 子网的广播地址 (不传递则使用配置文件中的子网广播地址)
        /// </summary>
        public string? SubnetBroadcastAddress { get; set; }
    }
}