using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WebApiClient;
using WebApiClient.Attributes;

namespace HttpImg
{
    [HttpHost("http://www.tmkoo.com")]
    public interface IMyWebApi : IHttpApiClient
    {
        ITask<HttpResponseMessage> DownloadAsync([Url] string url);
    }

    public class UserImg
    {
        public int sb_id { get; set; }
        public string sb_fenlei { get; set; }
        public string sb_img { get; set; }

    }
}
