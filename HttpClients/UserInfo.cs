using WebApiClient;
using WebApiClient.Attributes;

namespace HttpClients
{
    [HttpHost("http://www.tmkoo.com")]
    public interface IMyWebApi : IHttpApiClient
    {
        // GET webapi/user?account=laojiu
        // Return 原始string内容
        [HttpGet("http://www.tmkoo.com/home/detail.php")]
        ITask<string> GetUserByAccountAsync(string zch);

        // POST webapi/user  
        // Body Account=laojiu&password=123456
        // Return json或xml内容
        [HttpPost("http://www.tmkoo.com/j_spring_security_check")]
        ITask<string> LognWithFormAsync([FormContent] UserInfo user);
    }

    public class UserInfo
    {
        public string j_username { get; set; }
        public string j_password { get; set; }
    }
}
