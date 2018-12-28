using System;
using System.Diagnostics;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WebApiClient;
using vip.dal;
using System.Collections.Generic;
using vip.common;
using System.Collections.Concurrent;
using HtmlAgilityPack;
using Less.Html;
using Less.Text;
using System.Data.SqlClient;
using System.Text;

namespace HttpClients
{
    public class Program
    {
        // 正则表达式过滤：正则表达式，要替换成的文本
        private static readonly string[][] _filters = new string[][]{
                new string[] { @"(?is)<script.*?>.*?</script>", "" },
                new string[] { @"(?is)<style.*?>.*?</style>", "" },
                new string[] { @"(?is)<!--.*?-->", "" }, // 过滤Html代码中的注释
                new string[] { @"(?is)<marquee.*?>.*?</marquee>", "" }, //过滤滚动条
                // 针对链接密集型的网站的处理，主要是门户类的网站，降低链接干扰
                new string[] { @"(?is)</a>", "</a>\n"},
                new string[] { @"style\s*=(['""\s]?)[^'""]*?\1", ""}

            };

        static int maxWorkerThreads;
        static int maxAsyncIoThreadNum;

        private static readonly ConcurrentQueue<int> ltid = new ConcurrentQueue<int>();

        public static void Main(string[] args)
        {

            //maxWorkerThreads = Environment.ProcessorCount;
            //maxAsyncIoThreadNum = Environment.ProcessorCount;
            //maxWorkerThreads = 1;
            //maxAsyncIoThreadNum = 1;
            //ThreadPool.SetMaxThreads(maxWorkerThreads, maxAsyncIoThreadNum);

            //初始化
            string strconn = "Data Source=.;Initial Catalog=demo2;uid=sa;pwd=123qwe;";


            IList<KVPair> items = DbHelper.ExecuteReaderToIListKVPair(strconn, CommandType.Text, "SELECT demo2_1.id FROM dicdata RIGHT OUTER JOIN demo2_1 ON dicdata.sb_id = demo2_1.id where sb_id is null ");

            foreach(KVPair item in items)
            {
                ltid.Enqueue(item.ToInt32("id"));

                //Console.WriteLine(item["id"]);
            }

            RunIUserApi();
            //防止窗口关闭
            Console.ReadKey();

        }

        static async void RunIUserApi()
        {
            var watch = new Stopwatch();
            watch.Start();

            using (var client = HttpApiClient.Create<IMyWebApi>())
            {
                //登陆
                await client.LognWithFormAsync(new UserInfo { j_username = "13880947825", j_password = "vk680680" });
                //队列空则停止
                while (!ltid.IsEmpty)
                {
                    try
                    {
                        await RunApisAsync(client);
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    
                }
            }

            watch.Stop();
            Console.WriteLine($"总共耗时：{watch.Elapsed}");
        }



        public static async Task RunApisAsync(IMyWebApi client)
        {
            StringBuilder strsql = new StringBuilder();
            strsql.Append("INSERT INTO dicdata ");
            strsql.Append("                (sb_id, sb_name, sb_zhuceid, sb_fenlei, sb_sqrq, sb_sqr_cn, sb_sqdz_cn, sb_sqr_en, sb_sqdz_en, sb_img, sb_tpys, ");
            strsql.Append("                aa1, aa2, aa3, aa4, aa5, aa6, aa7, aa8, aa9, aa10, aa11, aa12, aa13) ");
            strsql.Append("VALUES(@sb_id, @sb_name, @sb_zhuceid, @sb_fenlei, @sb_sqrq, @sb_sqr_cn, @sb_sqdz_cn, @sb_sqr_en, @sb_sqdz_en, @sb_img, @sb_tpys, @aa1, @aa2, @aa3, @aa4, @aa5, @aa6, @aa7, @aa8, @aa9, @aa10, @aa11, @aa12, @aa13) ");

            SqlParameter[] parameters =
            {
                new SqlParameter("@sb_id", SqlDbType.Int),
                new SqlParameter("@sb_name", SqlDbType.NVarChar, 500),
                new SqlParameter("@sb_zhuceid", SqlDbType.NVarChar, 500),
                new SqlParameter("@sb_fenlei", SqlDbType.NVarChar, 500),
                new SqlParameter("@sb_sqrq", SqlDbType.NVarChar, 500),
                new SqlParameter("@sb_sqr_cn", SqlDbType.NVarChar, 500),
                new SqlParameter("@sb_sqdz_cn", SqlDbType.NVarChar, 500),
                new SqlParameter("@sb_sqr_en", SqlDbType.NVarChar, 500),
                new SqlParameter("@sb_sqdz_en", SqlDbType.NVarChar, 500),
                new SqlParameter("@sb_img", SqlDbType.NVarChar, 500),
                new SqlParameter("@sb_tpys", SqlDbType.NVarChar, 500),
                new SqlParameter("@aa1", SqlDbType.NVarChar, 500),
                new SqlParameter("@aa2", SqlDbType.NVarChar, 500),
                new SqlParameter("@aa3", SqlDbType.NVarChar, 500),
                new SqlParameter("@aa4", SqlDbType.NVarChar, 500),
                new SqlParameter("@aa5", SqlDbType.NVarChar, 500),
                new SqlParameter("@aa6", SqlDbType.NVarChar, 500),
                new SqlParameter("@aa7", SqlDbType.NVarChar, 500),
                new SqlParameter("@aa8", SqlDbType.NVarChar, 500),
                new SqlParameter("@aa9", SqlDbType.NVarChar, 500),
                new SqlParameter("@aa10", SqlDbType.NVarChar, 500),
                new SqlParameter("@aa11", SqlDbType.NVarChar, 500),
                new SqlParameter("@aa12", SqlDbType.NVarChar, 500),
                new SqlParameter("@aa13", SqlDbType.NVarChar, 500)
            };



            int sb_id = 0;
            ltid.TryDequeue(out sb_id);
            parameters[0].Value = sb_id; //
            var detail = await client.GetUserByAccountAsync(sb_id.ToString());

            //过滤无用代码
            foreach (var filter in _filters)
            {
                detail = Regex.Replace(detail, filter[0], filter[1], RegexOptions.IgnoreCase);
            }
            var doc = new HtmlDocument();
            doc.LoadHtml(detail);


            #region 第一部分
            var sb_info = doc.DocumentNode.SelectSingleNode("/html/body/div[5]/div[2]/div[1]").InnerHtml;
            var q = HtmlParser.Query(sb_info);

            string sb_name = q(".tmSbwz")[0].textContent.Trim();
            parameters[1].Value = sb_name; //
            Console.WriteLine("商标名称:" + sb_name);

            string sb_zhuceid = q("font")[0].textContent.Trim();
            parameters[2].Value = sb_zhuceid; //
            Console.WriteLine("注册号:" + sb_zhuceid);

            string sb_fenlei = q("font")[1].textContent.Trim();
            parameters[3].Value = sb_fenlei; //
            Console.WriteLine("分类:" + sb_fenlei);

            string sb_sqrq = q("td")[4].textContent.Trim();
            parameters[4].Value = sb_sqrq; //
            Console.WriteLine("申请日期:" + sb_sqrq);

            string sb_sqr_cn = q("td")[6].textContent.Trim();
            parameters[5].Value = sb_sqr_cn; //
            Console.WriteLine("申请人名称(中文):" + sb_sqr_cn);

            string sb_sqdz_cn = q("td")[8].textContent.Trim();
            parameters[6].Value = sb_sqdz_cn; //
            Console.WriteLine("申请人地址(中文):" + sb_sqdz_cn);

            string sb_sqr_en = q("td")[10].textContent.Trim();
            parameters[7].Value = sb_sqr_en; //
            Console.WriteLine("申请人名称(英文):" + sb_sqr_en);

            string sb_sqdz_en = q("td")[12].textContent.Trim();
            parameters[8].Value = sb_sqdz_en; //
            Console.WriteLine("申请人地址(英文):" + sb_sqdz_en);

            var q_img = HtmlParser.Query(q("td")[16].innerHTML);
            string sb_img = q_img("img").attr("src").Trim();
            parameters[9].Value = sb_img; //
            Console.WriteLine("商标图片:" + sb_img);

            string sb_tpys = q("td")[17].textContent.Replace("图形要素：", "").Trim();
            parameters[10].Value = sb_tpys; //
            Console.WriteLine("图形要素:" + sb_tpys);
            #endregion

            #region 第二部分
            //商品/服务列表
            var q2 = HtmlParser.Query(doc.DocumentNode.SelectNodes("//table")[4].InnerHtml);
            foreach (var item in q2("tr"))
            {
                string[] oitem = item.textContent.Split("——");
                Additem(sb_id, Utils.GetInt32(oitem[0]), oitem[1]);
            }
            #endregion

            #region 第三部分

            string aa1 = q("tr[bgcolor='#FFFFFF'] td")[15].textContent.Trim();
            parameters[11].Value = aa1; //
            Console.WriteLine("初审公告期号：	" + aa1);

            string aa2 = q("tr[bgcolor='#FFFFFF'] td")[17].textContent.Trim();
            parameters[12].Value = aa2; //
            Console.WriteLine("注册公告期号：	" + aa2);

            string aa3 = q("tr[bgcolor='#FFFFFF'] td")[19].textContent.Trim();
            parameters[13].Value = aa3; //
            Console.WriteLine("初审公告日期：	" + aa3);

            string aa4 = q("tr[bgcolor='#FFFFFF'] td")[21].textContent.Trim();
            parameters[14].Value = aa4; //
            Console.WriteLine("注册公告日期：	" + aa4);

            string aa5 = q("tr[bgcolor='#FFFFFF'] td")[23].textContent.Trim();
            parameters[15].Value = aa5; //
            Console.WriteLine("专用权期限：" + aa5);

            string aa6 = q("tr[bgcolor='#FFFFFF'] td")[25].textContent.Trim();
            parameters[16].Value = aa6; //
            Console.WriteLine("是否共有商标" + aa6);

            string aa7 = q("tr[bgcolor='#FFFFFF'] td")[27].textContent.Trim();
            parameters[17].Value = aa7; //
            Console.WriteLine("后期指定日期：" + aa7);

            string aa8 = q("tr[bgcolor='#FFFFFF'] td")[29].textContent.Trim();
            parameters[18].Value = aa8; //
            Console.WriteLine("国际注册日期：" + aa8);

            string aa9 = q("tr[bgcolor='#FFFFFF'] td")[31].textContent.Trim();
            parameters[19].Value = aa9; //
            Console.WriteLine("优先权日期：" + aa9);

            string aa10 = q("tr[bgcolor='#FFFFFF'] td")[33].textContent.Trim();
            parameters[20].Value = aa10; //
            Console.WriteLine("代理人名称：" + aa10);

            string aa11 = q("tr[bgcolor='#FFFFFF'] td")[35].textContent.Trim();
            parameters[21].Value = aa11; //
            Console.WriteLine("指定颜色：" + aa11);

            string aa12 = q("tr[bgcolor='#FFFFFF'] td")[37].textContent.Trim();
            parameters[22].Value = aa12; //
            Console.WriteLine("商标类型：" + aa12);

            string aa13 = q("tr[bgcolor='#FFFFFF'] td")[41].textContent.Trim();
            parameters[23].Value = aa13; //
            Console.WriteLine("电子公告：" + aa13);

            #endregion


            string strconn = "Data Source=.;Initial Catalog=demo2;uid=sa;pwd=123qwe;";
            DbHelper.ExecuteNonQuery(strconn, CommandType.Text, strsql.ToString(), parameters);



            //var sb_info1 = q("tbody");
            //int i = 0;
            //foreach (Element item in sb_info1)
            //{
            //    Console.WriteLine(string.Format("{0}---{1}", i, item.innerHTML));
            //    i++;
            //}

            //int j = 0;
            //foreach (Element item in sb_info1)
            //{
            //    Console.WriteLine(string.Format("{0}---{1}", j, item.textContent));
            //    j++;
            //}



            //Console.WriteLine(sb_info.html());


            //var title = q("table.child_forum tr td.title");

            //foreach (Element i in title)
            //{
            //    q(i).find(".forum_link").remove();

            //    Console.WriteLine(i.textContent);
            //}

            //foreach (Element i in q(".nav-main li"))
            //{

            //    Console.WriteLine(i.textContent);

            //    //if (!q(i).find("table").hasElement)
            //    //{
            //    //    Console.WriteLine(i.textContent);
            //    //}
            //}

            //Console.WriteLine(user1);





            //WebClient client1 = new WebClient();

            //client1.Encoding = Encoding.UTF8;

            //string aspDotNet = client1.DownloadString(
            //    "http://bbs.csdn.net/forums/ASPDotNET");

            //Console.WriteLine(aspDotNet);

            //var q1 = HtmlParser.Query(aspDotNet);

            //var title = q1("table.child_forum tr td.title");

            //foreach (Element i in title)
            //{
            //    q(i).find(".forum_link").remove();

            //    Console.WriteLine(i.textContent);
            //}


        }


        public static void Additem(int intpid, int intsid, string strsname)
        {
            //SELECT TOP 1000 [id],[pid],[sid],[sname] FROM[demo1].[dbo].[serverlist]
            //

            SqlParameter[] parameters =
            {
                new SqlParameter("@pid", SqlDbType.Int),
                new SqlParameter("@sid", SqlDbType.Int),
                new SqlParameter("@sname", SqlDbType.NVarChar, 50)
            };
            parameters[0].Value = intpid; //
            parameters[1].Value = intsid; //
            parameters[2].Value = strsname; //

            string strconn = "Data Source=.;Initial Catalog=demo2;uid=sa;pwd=123qwe;";
            DbHelper.ExecuteNonQuery(strconn, CommandType.Text, "INSERT INTO serverlist(pid, sid, sname) VALUES(@pid, @sid, @sname)", parameters);
        }
    }
}
