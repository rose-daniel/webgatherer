using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using vip.common;
using vip.dal;
using WebApiClient;
using WebApiClient.Attributes;

namespace HttpImg
{
    class Program
    {
        //static int maxWorkerThreads;
        //static int maxAsyncIoThreadNum;
        private static readonly ConcurrentQueue<UserImg> ltid = new ConcurrentQueue<UserImg>();

        static void Main(string[] args)
        {

            //maxWorkerThreads = Environment.ProcessorCount;
            //maxAsyncIoThreadNum = Environment.ProcessorCount;
            //maxWorkerThreads = 1;
            //maxAsyncIoThreadNum = 1;
            //ThreadPool.SetMaxThreads(maxWorkerThreads, maxAsyncIoThreadNum);


            //初始化
            string strconn = "Data Source=.;Initial Catalog=demo2;uid=sa;pwd=123qwe;";

            IList<KVPair> items = DbHelper.ExecuteReaderToIListKVPair(strconn, CommandType.Text, "SELECT [sb_id],[sb_fenlei],[sb_img] FROM [dicdata] where (sb_img_url IS NULL) ");

            foreach (KVPair item in items)
            {
                ltid.Enqueue(new UserImg {sb_id = item.ToInt32("sb_id"), sb_fenlei = item["sb_fenlei"], sb_img = item["sb_img"] });

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
            SqlParameter[] parameters =
            {
                new SqlParameter("@sb_id", SqlDbType.Int),
                new SqlParameter("@sb_img_url", SqlDbType.NVarChar, 255),
            };
            UserImg userImg;
            ltid.TryDequeue(out userImg);
            parameters[0].Value = userImg.sb_id; //

            var response = await client.DownloadAsync(userImg.sb_img);
            string fileurl = string.Format("images/{0}/{1}.png", userImg.sb_fenlei, userImg.sb_id);
            await response.SaveAsync(fileurl);

            Console.WriteLine(fileurl);
            
            parameters[1].Value = fileurl; //
            string strconn = "Data Source=.;Initial Catalog=demo2;uid=sa;pwd=123qwe;";
            DbHelper.ExecuteNonQuery(strconn, CommandType.Text, "UPDATE dicdata SET sb_img_url = @sb_img_url WHERE (sb_id = @sb_id)", parameters);


        }

    }
}
