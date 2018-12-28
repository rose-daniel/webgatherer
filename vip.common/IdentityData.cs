using System.Web;

namespace vip.common
{
    public class IdentityData
    {
        public static bool CheckLoginStatus(out int tmp_Uid)
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies["mygrv"];
            if (cookie != null)
            {
                if (string.IsNullOrWhiteSpace(cookie.Values["username"]))
                {
                    tmp_Uid = 0;
                    return false;
                }
                tmp_Uid = Utils.GetInt32(HttpContext.Current.Server.UrlDecode(cookie.Values["userid"]));

                if (tmp_Uid > 0)
                {
                    return true;
                }
            }
            tmp_Uid = 0;
            return false;
        }


    }
}
