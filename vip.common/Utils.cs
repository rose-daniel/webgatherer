using System;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Data;
using System.Security.Cryptography;
using System.Web;
using System.Web.Hosting;
using System.Web.Caching;
using System.Configuration;
using System.Collections.Generic;

namespace vip.common
{
    public class Utils
    {
        public static string saveFileHttpurl => ConfigurationManager.AppSettings["saveFileHttpurl"];

        public static string ZuoPinPath => ConfigurationManager.AppSettings["ZuoPinPath"];

        public static IDictionary<string, string> ConvertStringToIDictionary(string val, char kvSpacer, char eleSpacer)
        {
            IDictionary<string, string> iDi = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(val))
            {
                if (val.Contains(eleSpacer.ToString()))
                {
                    foreach (string item in val.Split(eleSpacer))
                    {
                        if (!string.IsNullOrWhiteSpace(item) && item.Contains(kvSpacer.ToString()))
                        {
                            if (!iDi.ContainsKey(item.Split(kvSpacer)[0]))
                            {
                                iDi.Add(item.Split(kvSpacer)[0], item.Split(kvSpacer)[1]);
                            }
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(val) && val.Contains(kvSpacer.ToString()))
                    {
                        if (!iDi.ContainsKey(val.Split(kvSpacer)[0]))
                        {
                            iDi.Add(val.Split(kvSpacer)[0], val.Split(kvSpacer)[1]);
                        }
                    }
                }
            }
            return iDi;
        }



        public static void RedirectUrl(string url)
        {
            HttpContext.Current.Response.Redirect(url);
        }

        public static string ClearHtmlTag(string html, int length = 0)
        {
            if (!string.IsNullOrWhiteSpace(html))
            {
                string strText = Regex.Replace(html, "<[^>]+>", "");
                strText = Regex.Replace(strText, "&[^;]+;", "");

                if (length > 0 && strText.Length > length)
                    return strText.Substring(0, length);

                return strText;
            } return string.Empty;
        }
        public static string GetIntList(string temp, int len)
        {
            if (!string.IsNullOrWhiteSpace(temp))
            {
                if (!string.IsNullOrWhiteSpace(temp))
                {
                    StringBuilder sb = new StringBuilder(temp.Length + 1);
                    int val = 0;
                    foreach (string item in temp.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (len <= 0) { break; }
                        val = GetInt32(item);
                        if (val > 0)
                        {
                            if (sb.Length > 0) { sb.Append(","); }
                            sb.Append(val.ToString());
                            len--;
                        }

                    } return sb.ToString();
                }
            }
            return string.Empty;
        }
        public static void RedirectPrePage(int pageIndex)
        {
            if (pageIndex > 0)
            {
                string tmp = HttpContext.Current.Request.RawUrl;
                if (!string.IsNullOrWhiteSpace(tmp))
                {

                    if (tmp.Contains("page="))
                    {
                        if (tmp.Contains("?"))
                        {
                            RedirectUrl(string.Format("{0}&page={1}", tmp.Replace("page=", string.Empty).Replace("num=", string.Empty), pageIndex.ToString()));
                        }
                        else
                        {
                            RedirectUrl(string.Format("{0}?page={1}", tmp.Replace("page=", string.Empty).Replace("num=", string.Empty), pageIndex.ToString()));
                        }
                    }
                }
            }
        }

        public static string FormatStringLength(string str, int displayLength)
        {
            //截取后的字符串
            string subStr = string.Empty;
            //字符串生成的默认编码的字节长度
            int nameLenth = Encoding.Unicode.GetByteCount(str);// Encoding.Default.GetByteCount(str);
            //字符串字节长度大于能显示的字节长度，进行截取
            if (nameLenth > displayLength)
            {
                //减去将要附加到尾部的"..."的长度,得到要截取的字节长度
                displayLength = displayLength - 3;
                //当前遍历到的字节数,是按displayLength计算字节数的,
                //即汉字算两个字节,英文数字等一个,用来与displayLength比较,好退出循环
                int CurrentLength = 0;
                //要截取的字节长度,该长度不同于displayLength,这里是Unicode(USC2)编码,
                //不区分汉字还是字母,每个字符占两个字节长度
                int subLength = 0;
                //字符串生成的Unicode(USC2)编码的字节数组
                byte[] strBytes = Encoding.Unicode.GetBytes(str);
                //
                for (; subLength < strBytes.GetLength(0) && CurrentLength < displayLength; subLength++)
                {
                    //因为Unicode(USC2)编码时,不区分汉字还是字母,每个字符占两个字节长度,
                    //这里subLength做下标,为0或每次为偶数时,正好是UCS2编码中两个字节的第一个字节,
                    //对于一个英文或数字字符，UCS2编码的第一个字节是相应的ASCII，第二个字节是0，如a的UCS2编码是97 0，而汉字两个字节都不为0
                    //除2的余数为0，表示这是每个字符的第一个字节,字母数字等只在这里对CurrentLength加1,汉字等则在第二个字节处判断出后再加1
                    if (subLength % 2 == 0)
                    {
                        CurrentLength++;
                    }
                    else//除2的余数不为0，表明是一个字符的第二个字节,检查字符的第二个字节
                    {
                        //汉字需要再加上1,以符合默认编码占两个字节
                        if (strBytes[subLength] > 0)
                        {
                            CurrentLength++;
                        }
                    }
                }
                //如果subLength为奇数时,即截取的最后一个字符，两个字节中只截取了1个即一般,需处理成偶数
                if (subLength % 2 == 1)
                {
                    //对字符的第二个字节进行判断(使用自身做下标,因为下标从0开始,实际检查的就是自己后面的一个字节)
                    //该UCS2字符是汉字时,第二个字节在默认编码中占1个字节，补全的话，长度超限,所以去掉这个截一半的汉字 
                    if (strBytes[subLength] > 0)
                    {
                        subLength = subLength - 1;
                    }
                    else//该UCS2字符是字母或数字,第二个字节在默认编码中不存在，并不占空间,补全该字符
                    {
                        subLength = subLength + 1;
                    }
                }
                subStr = Encoding.Unicode.GetString(strBytes, 0, subLength) + "...";
            }
            else//长度未超限,不作格式化
            {
                subStr = str;
            }
            return subStr;
        }
        public static string GetSomeText(string text, int length)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                if (text.Length > length)
                {
                    return text.Substring(0, length);
                } return text;
            } return string.Empty;
        }
        public static string GetItemSmallImg(string src)
        {
            if (!string.IsNullOrWhiteSpace(src) && src.Contains("/") && src.Contains("-") && src.Contains("_"))
            {
                string tmp = src.Split('/')[src.Split('/').Length - 1];
                if (!string.IsNullOrWhiteSpace(tmp) && !src.Contains("/s/"))
                {
                    return src.Replace("/" + tmp, "/s/" + tmp);
                }
            } return src;
        }
        public static string Reverse(string original)
        {
            if (!string.IsNullOrWhiteSpace(original))
            {
                if (original.Length <= 25)
                    return ReverseByCharBuffer(original);
                else
                    return ReverseByArray(original);
            } return string.Empty;
        }
        public static string ReverseByCharBuffer(string original)
        {
            if (!string.IsNullOrWhiteSpace(original))
            {
                char[] c = original.ToCharArray();
                int l = original.Length;
                for (int i = 0; i < l / 2; i++)
                {
                    char t = c[i];
                    c[i] = c[l - i - 1];
                    c[l - i - 1] = t;
                }
                return new string(c);
            } return string.Empty;
        }
        public static string ReverseByArray(string original)
        {
            if (!string.IsNullOrWhiteSpace(original))
            {
                char[] c = original.ToCharArray();
                Array.Reverse(c);
                return new string(c);
            } return string.Empty;
        }

        public static string GetReplaceAndSomeString(string text, int maxLength)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                text = text.Trim();
                if (string.IsNullOrWhiteSpace(text))
                {
                    return string.Empty;
                }
                else
                {
                    text = Regex.Replace(text, "[\\s]{2,}", " ");
                    text = Regex.Replace(text, "<META(.|\\n)*?>.*?</META>", string.Empty, RegexOptions.IgnoreCase);
                    text = Regex.Replace(text, "<style(.|\\n)*?>.*?</style>", string.Empty, RegexOptions.IgnoreCase);
                    text = Regex.Replace(text, "<w:WordDocument(.|\\n)*?>.*?</w:WordDocument>", string.Empty, RegexOptions.IgnoreCase);
                    text = Regex.Replace(text, "&mdash;", string.Empty, RegexOptions.IgnoreCase);
                    text = Regex.Replace(text, "&shy;", string.Empty, RegexOptions.IgnoreCase);
                    text = Regex.Replace(text, "(<[b|B][r|R]/*>)+|(<[p|P](.|\\n)*?>)", "\n");
                    text = Regex.Replace(text, "(\\s*&[n|N][b|B][s|S][p|P];\\s*)+", " ");
                    text = Regex.Replace(text, "(&[l|L|r|R][d|D][q|Q][u|U][o|O];)+", " ");
                    text = Regex.Replace(text, "&\\w+;", " ");
                    text = Regex.Replace(text, "<(.|\\n)*?>", string.Empty);
                    text = Regex.Replace(text, "<", string.Empty);
                    text = Regex.Replace(text, ">", string.Empty);
                    text = Regex.Replace(text, "\"", string.Empty);
                    text = text.Replace("'", string.Empty);
                }
                if (text.Length > maxLength)
                    text = text.Substring(0, maxLength);
            }
            return text;
        }

        public static string ConvertDateString(string val, string format)
        {
            if (!string.IsNullOrWhiteSpace(val))
            {
                try
                {
                    return DateTime.Parse(val).ToString(format);
                }
                catch { return string.Empty; }
            }
            return string.Empty;
        }
        /// <summary>
        /// 获取一个时间与当前时间的差(秒)
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static double GetDateTimeSpan(string dateTime)
        {
            if (!string.IsNullOrWhiteSpace(dateTime))
            {
                DateTime endDate = Convert.ToDateTime(Convert.ToDateTime(dateTime).ToString("yyyy-MM-dd HH:mm:ss"));
                DateTime nowDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                if (endDate > nowDate)
                {
                    return (endDate - nowDate).TotalSeconds;
                    //TimeSpan ts = endDate.Subtract(nowDate);
                    //int ms = 0;
                    //ms += Convert.ToInt32(ts.Days) * 86400;
                    //ms += Convert.ToInt32(ts.Hours) * 3600;
                    //ms += Convert.ToInt32(ts.Minutes) * 60;
                    //ms += Convert.ToInt32(ts.Seconds);
                    //return ms;
                }
            } return 0;

        }
        public static bool IsImgExt(string src)
        {
            if (!string.IsNullOrWhiteSpace(src))
            {
                if (src.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) || src.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || src.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) || src.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || src.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) || src.EndsWith(".tif", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            } return false;
        }
 

        /// <summary>
        ///format：D有短横,N无间隔符,B短横及前后花括号,P短横及前后小括号
        /// </summary>
        /// <returns></returns>
        public static string GetNewGuid(string format = "N")
        {
            return Guid.NewGuid().ToString(format);
        }
        /// <summary>
        /// 将字符数字转换为Int32
        /// </summary>
        /// <param name="nStr"></param>
        /// <returns></returns>
        public static int GetInt32(string nStr)
        {
            if (!string.IsNullOrWhiteSpace(nStr))
            {
                if (nStr == "0")
                { return 0; }
                else
                {
                    int ret = 0;
                    int.TryParse(nStr, out ret);
                    return ret;
                }
            }
            return 0;
        }
        public static decimal GetDecimal(string nStr)
        {
            if (!string.IsNullOrWhiteSpace(nStr))
            {
                if (nStr == "0")
                { return 0; }
                else
                {
                    decimal ret = 0;
                    decimal.TryParse(nStr, out ret);
                    return ret;
                }
            }
            return 0;
        }
        public static DateTime GetDateTime(string date)
        {
            if (!string.IsNullOrWhiteSpace(date))
            {
                DateTime now = DateTime.Now;
                DateTime.TryParse(date, out now);
                return now;
            } return DateTime.Now;
        }

        /// <summary>
        /// 将字符数字转换为double
        /// </summary>
        /// <param name="nStr"></param>
        /// <returns></returns>
        public static double GetDouble(string nStr)
        {
            if (!string.IsNullOrWhiteSpace(nStr))
            {
                if (nStr == "0")
                { return 0; }
                else
                {
                    double ret = 0;
                    double.TryParse(nStr, out ret);
                    return ret;
                }
            }
            return 0;
        }
        /// <summary>
        /// 将一段文本内容中的换行符替换成指定字符
        /// </summary>
        /// <param name="val">文本内容</param>
        /// <param name="span">新指定的字符</param>
        /// <returns></returns>
        public static string FormatEnterStringToSep(ref string val, string span)
        {
            using (StringReader sr = new StringReader(val))
            {
                StringBuilder sb = new StringBuilder(val.Length);
                while (sr.Peek() >= 0)
                {
                    val = sr.ReadLine();
                    if (!string.IsNullOrWhiteSpace(val))
                    {
                        sb.Append(val);
                    } sb.Append(span);
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// 从字符串中过滤正则匹配的字符（返回值方式）
        /// </summary>
        /// <param name="text"></param>
        /// <param name="regex"></param>
        public static string FilterStringByRegex(string text, string regex)
        {
            FilterStringByRegex(ref text, regex);
            return text;
        }

        /// <summary>
        /// 从字符串中过滤正则匹配的字符(引用方式)
        /// </summary>
        /// <param name="text"></param>
        /// <param name="regex"></param>
        public static void FilterStringByRegex(ref string text, string regex)
        {
            if (!string.IsNullOrWhiteSpace(text) && !string.IsNullOrWhiteSpace(regex))
            {
                if (Regex.IsMatch(text, @"" + regex))
                {
                    try
                    {
                        text = Regex.Replace(text, @"" + regex, string.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    }
                    catch { }
                }
            }
        }
        /// <summary>
        /// 正则匹配替换换行符
        /// </summary>
        /// <param name="val">内容</param>
        /// <param name="newString">新指定的字符</param>
        /// <returns></returns>
        public static string ReplaceEnterLine(string val, string newString)
        {
            if (!string.IsNullOrWhiteSpace(val))
            {
                string regStr = "((\r\n)|(\u0085)|(\u2028)|(\u2029)|\n|\r)";
                if (Regex.IsMatch(val, regStr))
                {
                    return Regex.Replace(val, regStr, newString, RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Multiline);
                } return val;
            } return string.Empty;
        }

        /// <summary>
        /// 缓存数据操作
        /// </summary>
        /// <param name="name">缓存key名称</param>
        /// <param name="obj">缓存的对象</param>
        /// <param name="expires_Minutes">缓存多少分钟</param>
        /// <param name="isAbsoluteExpiration">是否绝对过期</param>
        /// <param name="priority">优先级别</param>
        /// <param name="filePath">关联的文件物理路径</param>
        public static void HostingCacheInsert(string name, object obj, int expires_Minutes = 0, bool isAbsoluteExpiration = true, CacheItemPriority priority = CacheItemPriority.High, string filePath = null)
        {
            CacheDependency depFile = null;
            if (filePath != null)
            {
                depFile = new CacheDependency(filePath);
            }
            if (expires_Minutes > 0)
            {
                if (isAbsoluteExpiration)
                {
                    //绝对过期时间
                    HostingEnvironment.Cache.Insert(name, obj, depFile, DateTime.Now.AddMinutes(expires_Minutes), Cache.NoSlidingExpiration, priority, null);
                }
                else
                {
                    //弹性过期时间
                    HostingEnvironment.Cache.Insert(name, obj, depFile, Cache.NoAbsoluteExpiration, new TimeSpan(0, expires_Minutes, 0), priority, null);
                }
            }
            else
            {
                HostingEnvironment.Cache.Insert(name, obj, depFile, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, priority, null);
            }
        }
        /// <summary>
        /// 获取缓存内容
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="name">缓存key名称</param>
        /// <returns>数据</returns>
        public static T HostingCacheGet<T>(string name)
        {
            return (T)HostingEnvironment.Cache.Get(name);
        }
        /// <summary>
        /// 获取客户端IP地址
        /// </summary>
        /// <returns></returns>
        public static string GetIP()
        {
            string tmp = string.Empty;
            try
            {
                if (HttpContext.Current.Request.ServerVariables["HTTP_VIA"] != null)
                {
                    tmp = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"].Split(new char[] { ',' })[0];
                }
                else
                {
                    tmp = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                }
            }
            catch
            {
                tmp = HttpContext.Current.Request.UserHostAddress;
            }
            if (string.IsNullOrWhiteSpace(tmp))
            {
                return "127.0.0.1";
            }
            else
            {
                FilterStringByRegex(ref tmp, @"[^0-9\.]");
                return tmp;
            }

        }
        public static string HtmlEncode(string val)
        {
            return HttpUtility.HtmlEncode(val);
        }
        public static string HtmlDecode(string val)
        {
            return HttpUtility.HtmlDecode(val);
        }
        public static string UrlEncode(string val)
        {
            return HttpUtility.UrlEncode(val);
        }
        public static string UrlDecode(string val)
        {
            return HttpUtility.UrlDecode(val);
        }


        public static void WriteCookie(string name, string value, string domain, int expiresMinutes = 0, bool isOnly = true)
        {
            WriteCookie(ref  name, ref  value, domain, expiresMinutes, isOnly);
        }

        public static void WriteCookie(ref string name, ref string value, string domain, int expiresMinutes = 0, bool isOnly = true)
        {
            HttpCookie cookie = cookie = new HttpCookie(name);
            if (!string.IsNullOrWhiteSpace(domain))
            {
                cookie.Domain = domain;
            }
            if (expiresMinutes > 0)
            {
                cookie.Expires = DateTime.Now.AddMinutes(expiresMinutes);
            }
            cookie.Value = HtmlEncode(value);
            cookie.HttpOnly = isOnly;
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        public static string GetCookie(string name)
        {
            return GetCookie(ref name);
        }
        public static string GetCookie(ref string name)
        {
            if (HttpContext.Current.Request.Cookies != null && HttpContext.Current.Request.Cookies[name] != null)
            {
                return HtmlDecode(HttpContext.Current.Request.Cookies[name].Value);
            }
            return string.Empty;
        }

        public static void RemoveCookie(ref string name, string domain)
        {
            HttpCookie cookie = new HttpCookie(name);
            if (!string.IsNullOrWhiteSpace(domain))
            {
                cookie.Domain = domain;
            }
            cookie.Expires = DateTime.Now.AddDays(-1d);
            HttpContext.Current.Response.Cookies.Add(cookie);
        }


        public static string DESEncrypt(string val, string key = "?!@$%?&?", string vi = "?!@$??&?")
        {
            if (!string.IsNullOrWhiteSpace(val))
            {
                if (key.Length < 8) { key = string.Format("{0}?!@$%?&?", key); }
                else { if (key.Length > 8) { key = key.Substring(0, 8); } }
                if (vi.Length < 8) { vi = string.Format("{0}?!@$%?&?", vi); }
                else { if (vi.Length > 8) { vi = vi.Substring(0, 8); } }
                SymmetricAlgorithm sa = new DESCryptoServiceProvider();
                sa.Key = Encoding.UTF8.GetBytes(key);
                sa.IV = Encoding.UTF8.GetBytes(vi);
                ICryptoTransform ct = sa.CreateEncryptor();
                byte[] _byte = Encoding.UTF8.GetBytes(val);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, ct, CryptoStreamMode.Write);
                cs.Write(_byte, 0, _byte.Length);
                cs.FlushFinalBlock();
                ms.Dispose();
                cs.Dispose();
                sa.Dispose();
                ct.Dispose();
                return Convert.ToBase64String(ms.ToArray());
            }
            return string.Empty;
        }

        public static string DESDecrypt(string val, string key = "?!@$%?&?", string vi = "?!@$??&?")
        {
            if (!string.IsNullOrWhiteSpace(val))
            {
                if (key.Length < 8) { key = string.Format("{0}?!@$%?&?", key); }
                else { if (key.Length > 8) { key = key.Substring(0, 8); } }
                if (vi.Length < 8) { vi = string.Format("{0}?!@$%?&?", vi); }
                else { if (vi.Length > 8) { vi = vi.Substring(0, 8); } }
                SymmetricAlgorithm sa = new DESCryptoServiceProvider();
                sa.Key = Encoding.UTF8.GetBytes(key);
                sa.IV = Encoding.UTF8.GetBytes(vi);
                ICryptoTransform ct = sa.CreateDecryptor();
                byte[] byt = Convert.FromBase64String(val);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, ct, CryptoStreamMode.Write);
                cs.Write(byt, 0, byt.Length);
                cs.FlushFinalBlock();
                ms.Dispose();
                cs.Dispose();
                sa.Dispose();
                ct.Dispose();
                return Encoding.UTF8.GetString(ms.ToArray());
            }
            return string.Empty;
        }

        /// <summary>
        /// MD5---16
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns>加密后的字符串</returns>
        public static string HashGetMD5_16(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                System.Security.Cryptography.MD5 md = System.Security.Cryptography.MD5.Create();
                var data = md.ComputeHash(Encoding.Default.GetBytes(text));
                var sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                    sBuilder.Append(data[i].ToString("x2"));
                //获取16位MD5加密数据
                return sBuilder.ToString().Substring(8, 16);
            }
            return string.Empty;
        }
        /// <summary>
        /// MD5---32
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string HashGetMD5_32(string text)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            bytes = md5.ComputeHash(bytes);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }
        /// <summary>
        /// SHA1--40
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string HashGetSHA1(string text)
        {
            SHA1Managed sha = new SHA1Managed();
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            bytes = sha.ComputeHash(bytes);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }
        /// <summary>
        /// SHA256---64
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string HashGetSHA256(string text)
        {
            SHA256Managed sha = new SHA256Managed();
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            bytes = sha.ComputeHash(bytes);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }
        /// <summary>
        /// SHA512---128
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string HashGetSHA512(string text)
        {
            SHA512Managed sha = new SHA512Managed();
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            bytes = sha.ComputeHash(bytes);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }
        public static SqlParameter CreateSqlParameter(string paraName, SqlDbType sqlDbType, int size, object value, ParameterDirection paramDirection = ParameterDirection.Input)
        {
            SqlParameter param = new SqlParameter(paraName, sqlDbType, size);
            param.Value = value;
            param.Direction = paramDirection;
            return param;
        }

        public static string GetReplaceScriptStr(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                text = Regex.Replace(text, "<SCRIPT(.|\\n)*?>.*?</SCRIPT>", string.Empty, RegexOptions.IgnoreCase);
                text = Regex.Replace(text, "<iframe(.|\\n)*?>.*?</iframe>", string.Empty, RegexOptions.IgnoreCase);
                text = Regex.Replace(text, "<iframe(.|\\n)*?/>", string.Empty, RegexOptions.IgnoreCase);
                return text;
            } return string.Empty;
        }

        /// <summary>
        /// 是否为手机访问
        /// </summary>
        /// <param name="strUserAgent">获取机型、浏览器等型号</param>
        /// <returns>true 是手机访问；false 不是手机访问</returns>
        public static bool IsMobile(string strUserAgent)
        {
            if (strUserAgent != null)
            {
                if (System.Web.HttpContext.Current.Request.Browser.IsMobileDevice == true ||
                    strUserAgent.Contains("iphone") ||
                strUserAgent.Contains("blackberry") ||
                strUserAgent.Contains("mobile") ||
                strUserAgent.Contains("windows ce") ||
                strUserAgent.Contains("opera mini") ||
                strUserAgent.Contains("palm") ||
                    strUserAgent.Contains("mobi") ||
                    strUserAgent.Contains("nokia") ||
                    strUserAgent.Contains("samsung") ||
                    strUserAgent.Contains("sonyericsson") ||
                    strUserAgent.Contains("mot") ||
                    strUserAgent.Contains("lg") ||
                    strUserAgent.Contains("htc") ||
                    strUserAgent.Contains("j2me") ||
                    strUserAgent.Contains("ucweb")
                    )
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static string Get_Http(string tUrl)
        {
            bool result = true;
            return Get_Http(tUrl, ref result);
        }
        public static string Get_Http(string tUrl, ref bool result)
        {
            try
            {
                HttpWebRequest hwr = (HttpWebRequest)HttpWebRequest.Create(tUrl);
                hwr.Timeout = 1960000;
                using (HttpWebResponse hwrs = (HttpWebResponse)hwr.GetResponse())
                {
                    Stream myStream = hwrs.GetResponseStream();
                    StreamReader sr = new StreamReader(myStream, Encoding.GetEncoding("GB2312"));
                    //StringBuilder sb = new StringBuilder();
                    //while (-1 != sr.Peek())
                    //{ 
                    //    sb.Append(sr.ReadLine() + "\r\n");
                    //}
                    result = true;
                    //return sb.ToString();
                    return sr.ReadToEnd();
                }
            }
            catch (Exception ee)
            {
                result = false;
                return ee.Message;
            }
        }

        public static String Get_Http(String a_strUrl, int timeout, ref bool isOk)
        {
            string strResult;
            try
            {
                HttpWebRequest myReq = (HttpWebRequest)HttpWebRequest.Create(a_strUrl);
                myReq.Timeout = timeout;
                using (HttpWebResponse HttpWResp = (HttpWebResponse)myReq.GetResponse())
                {
                    Stream myStream = HttpWResp.GetResponseStream();
                    StreamReader sr = new StreamReader(myStream, Encoding.UTF8);
                    StringBuilder strBuilder = new StringBuilder();
                    while (-1 != sr.Peek())
                    {
                        strBuilder.Append(sr.ReadLine());
                    }
                    strResult = strBuilder.ToString();
                    isOk = true;
                }
            }
            catch (Exception exp)
            {
                isOk = false;
                strResult = "错误：" + exp.ToString();
            }
            return strResult;
        }

        /// <summary>
        /// 匹配的第一个关键词加链接
        /// </summary>
        /// <param name="pain"></param>
        /// <param name="keyword"></param>
        /// <param name="strlink"></param>
        /// <returns></returns>
        public static string HighLightKeyWord(string pain, string keyword, string strlink)
        {
            MatchCollection m = Regex.Matches(pain, keyword, RegexOptions.IgnoreCase);
            if (m.Count > 0)
            {
                pain = pain.Insert((m[0].Index + keyword.Length), "</a>");//关键字后插入html标签
                pain = pain.Insert((m[0].Index), "<a href='" + strlink + "' target='_blank'>");//关键字前插入html标签
            }
            return pain;
        }

        /// <summary>
        /// 获取间隔时间
        /// </summary>
        /// <param name="strAddTime">添加时间</param>
        /// <returns></returns>
        public static string GetIntervalTime(string strAddTime)
        {

            DateTime dtmAddTime = GetDateTime(strAddTime);

            TimeSpan timeSpan = DateTime.Now - dtmAddTime;
            if (dtmAddTime < DateTime.Today.AddYears(-5))
            {
                return "5年前";
            }
            if (dtmAddTime < DateTime.Today.AddYears(-4))
            {
                return "4年前";
            }
            else if (dtmAddTime < DateTime.Today.AddYears(-3))
            {
                return "3年前";
            }
            else if (dtmAddTime < DateTime.Today.AddYears(-2))
            {
                return "2年前";
            }
            else if (dtmAddTime < DateTime.Today.AddYears(-1))
            {
                return "1年前";
            }
            else if (dtmAddTime < DateTime.Today.AddMonths(-6))
            {
                return "半年前";
            }
            else if (dtmAddTime < DateTime.Today.AddMonths(-3))
            {
                return "3个月前";
            }
            else if (dtmAddTime < DateTime.Today.AddMonths(-2))
            {
                return "2个月前";
            }
            else if (timeSpan.TotalDays > 30)
            {
                return "1个月前";
            }
            else if (timeSpan.TotalDays > 14)
            {
                return "2周前";
            }
            else if (timeSpan.TotalDays > 7)
            {
                return "1周前";
            }
            else if (timeSpan.TotalDays > 1)
            {
                return string.Format("{0}天前", (int)Math.Floor(timeSpan.TotalDays));
            }
            else if (timeSpan.TotalHours > 1)
            {
                return string.Format("{0}小时前", (int)Math.Floor(timeSpan.TotalHours));
            }
            else if (timeSpan.TotalMinutes > 1)
            {
                return string.Format("{0}分钟前", (int)Math.Floor(timeSpan.TotalMinutes));
            }
            else if (timeSpan.TotalSeconds >= 1)
            {
                return string.Format("{0}秒前", (int)Math.Floor(timeSpan.TotalSeconds));
            }
            else
            {
                return "1秒前";
            }

        }
    }
}
