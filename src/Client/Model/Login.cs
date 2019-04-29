using System.ComponentModel.DataAnnotations;

namespace Client.Model
{
    public class Login
    {
        /// <summary>
        /// 帐号
        /// </summary>
        [Required(ErrorMessage = "帐号不能为空")]
        [MaxLength(20, ErrorMessage = "帐号长度最长为20位字符")]
        public string Account { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [Required(ErrorMessage = "密码不能为空")]
        [MaxLength(40, ErrorMessage = "密码长度最长为40位字符")]
        public string Password { get; set; }
    }
}