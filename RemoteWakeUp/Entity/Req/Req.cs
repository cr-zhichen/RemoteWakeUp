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
}