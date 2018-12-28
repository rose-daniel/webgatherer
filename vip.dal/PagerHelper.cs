using System;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using vip.common;

namespace vip.dal
{
    public class PagerHelper
    {
        public PagerHelper(string _keyColName, int _pageSize, string _pageIndex)
        {
            this.KeyColName = _keyColName;
            this.PageSize = _pageSize <= 0 ? 10 : _pageSize;
            this.PageIndex = Utils.GetInt32(_pageIndex);
            if (this.PageIndex <= 0)
            {
                this.PageIndex = 1;
            }
        }
        public string KeyColName { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int PageCount { set; get; }
        public int RecordCount { set; get; }

        /// <summary>
        /// 按照ROW_NUMBER()进行排序，排序字段无需order by，条件无需where；注意：查询返回的结果集中第一个字段为排序号rowID而不是请求查询字段列表中的第一个
        /// </summary>
        /// <param name="tblName">表名</param>
        /// <param name="selectCols">查询的字段列表</param>
        /// <param name="whereString">条件，无需where</param>
        /// <param name="orderString">排序字段列表，无需order by</param>
        /// <param name="getTotal">满足条件的总记录数量</param>
        /// <returns></returns>
        public DataTable PagerByRowNumber(string tblName, string selectCols, string whereString, string orderString, int getTotal)
        {
            SqlParameter[] _Para = new SqlParameter[]{        
                new SqlParameter("@TableName", SqlDbType.NVarChar, 100), 
                new SqlParameter("@Fields", SqlDbType.NVarChar, 1500), 
                new SqlParameter("@OrderField", SqlDbType.NVarChar,200),
                new SqlParameter("@sqlWhere", SqlDbType.NVarChar,2000), 
                new SqlParameter("@pageSize", SqlDbType.Int,4),
                new SqlParameter("@pageIndex", SqlDbType.Int,4),
                new SqlParameter("@totalRecord",SqlDbType.Int)
                };
            _Para[0].Value = tblName;
            _Para[1].Value = selectCols;
            _Para[2].Value = orderString;
            _Para[3].Value = whereString;
            _Para[4].Value = PageSize <= 0 ? 10 : PageSize;
            _Para[5].Value = PageIndex <= 0 ? 1 : PageIndex;

            if (PageIndex > 1 && getTotal > 0)
            {
                _Para[6].Value = getTotal.ToString();
            }
            else
            {
                _Para[6].Direction = ParameterDirection.Output;
            }
            DataTable dt = DbHelper.ExecuteDataTable("[dbo].[ecs_pagedatarow]", _Para);
            RecordCount = (PageIndex > 1 && getTotal > 0) ? getTotal : int.Parse(_Para[6].Value.ToString());
            return dt;
        }


        /// <summary>
        /// 按照ROW_NUMBER()进行排序，排序字段无需order by，条件无需where；注意：查询返回的结果集中第一个字段为排序号rowID而不是请求查询字段列表中的第一个
        /// </summary>
        /// <param name="strconn">连接字符串</param>
        /// <param name="tblName">表名</param>
        /// <param name="selectCols">查询的字段列表</param>
        /// <param name="whereString">条件，无需where</param>
        /// <param name="orderString">排序字段列表，无需order by</param>
        /// <param name="getTotal">满足条件的总记录数量</param>
        /// <returns></returns>
        public DataTable PagerByRowNumber(string strconn, string tblName, string selectCols, string whereString, string orderString, int getTotal)
        {
            SqlParameter[] _Para = new SqlParameter[]{        
                new SqlParameter("@TableName", SqlDbType.NVarChar, 100), 
                new SqlParameter("@Fields", SqlDbType.NVarChar, 1500), 
                new SqlParameter("@OrderField", SqlDbType.NVarChar,200),
                new SqlParameter("@sqlWhere", SqlDbType.NVarChar,2000), 
                new SqlParameter("@pageSize", SqlDbType.Int,4),
                new SqlParameter("@pageIndex", SqlDbType.Int,4),
                new SqlParameter("@totalRecord",SqlDbType.Int)
                };
            _Para[0].Value = tblName;
            _Para[1].Value = selectCols;
            _Para[2].Value = orderString;
            _Para[3].Value = whereString;
            _Para[4].Value = PageSize <= 0 ? 10 : PageSize;
            _Para[5].Value = PageIndex <= 0 ? 1 : PageIndex;

            if (PageIndex > 1 && getTotal > 0)
            {
                _Para[6].Value = getTotal.ToString();
            }
            else
            {
                _Para[6].Direction = ParameterDirection.Output;
            }
            DataTable dt = DbHelper.ExecuteDataTable(strconn, CommandType.StoredProcedure, "[dbo].[ecs_pagedatarow]", _Para);
            RecordCount = (PageIndex > 1 && getTotal > 0) ? getTotal : int.Parse(_Para[6].Value.ToString());
            return dt;
        }

        /// <summary>
        /// 多列排序方法 可多表联合查询(写在表名中 )排序 不含'order by'字符，如id asc,userid desc，必须指定asc或desc记住一定要在最后加上主键
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="FieldList"></param>
        /// <param name="Where"></param>
        /// <param name="order"></param>
        /// <param name="order_as">不含'order by'字符</param> 
        /// <param name="RecorderCount"></param>
        /// <returns></returns>
        public IList<KVPair> PagerByMoreTblCol(string tableName, string FieldList, string Where, string order, string order_as, int RecorderCount, string dbname = null)
        {
            SqlParameter[] para = new SqlParameter[] {  
            new SqlParameter("@TableName",SqlDbType.VarChar,800),//表名
            new SqlParameter("@FieldList",SqlDbType.VarChar,2000),//显示列名 
            new SqlParameter("@Where",SqlDbType.VarChar,2000),//查询条件 不含'where'字符，如id>10 and len(userid)>9   
            new SqlParameter("@order",SqlDbType.VarChar,1000),//排序 不含'order by'字符，如id asc,userid desc，必须指定asc或desc 记住一定要在最后加上主键，否则会让你比较郁闷 
            new SqlParameter("@PrimaryKey",SqlDbType.VarChar,100),//单一主键或唯一值键   
            new SqlParameter("@SortType",SqlDbType.Int,4),//排序规则 多列排序方法   
            new SqlParameter("@RecorderCount",SqlDbType.Int,4),//已知记录总数 0:会返回总记录
            new SqlParameter("@PageSize",SqlDbType.Int,4),//每页输出的记录数
            new SqlParameter("@PageIndex",SqlDbType.Int,4),//当前页数
            new SqlParameter("@TotalCount",SqlDbType.Int,4),//返回总记录数
            new SqlParameter("@TotalPageCount",SqlDbType.Int,4),//返回总页数
            new SqlParameter("@order_as",SqlDbType.VarChar,1000)//作为多表标识前缀的字段的as字段别名 和 内部排序字段列对应，专用于外围排序使用
            };
            para[0].Value = tableName;
            para[1].Value = FieldList;
            para[2].Value = Where;
            para[3].Value = order;
            para[4].Value = this.KeyColName;
            para[5].Value = 3;
            para[6].Value = RecorderCount;
            para[7].Value = this.PageSize;
            para[8].Value = this.PageIndex;
            para[9].Direction = ParameterDirection.Output;
            para[10].Direction = ParameterDirection.Output;
            para[11].Value = order_as;
            IList<KVPair> data = DbHelper.ExecuteDataTableToIListKVPair(CommandType.StoredProcedure, "[dbo].[ecs_pagedatabymoretblcol]", para);

            this.RecordCount = Utils.GetInt32(para[9].Value.ToString());
            this.PageCount = Utils.GetInt32(para[10].Value.ToString());
            return data;
        }
        /// <summary>
        /// 单表主键排序分页
        /// </summary>
        /// <param name="_tblName"></param>
        /// <param name="_getCols"></param>
        /// <param name="_orderType"></param>
        /// <param name="_where"></param>
        /// <param name="getTotal"></param>
        /// <returns></returns>
        public IList<KVPair> PagerByKeyT(string _tblName, string _getCols, int _orderType, string _where, int getTotal, string dbname = null)
        {
            SqlParameter[] _PagePara = new SqlParameter[]{
                new SqlParameter("@tblName", SqlDbType.NVarChar,100),
                new SqlParameter("@getCols", SqlDbType.NVarChar,1000),
                new SqlParameter("@keyColName",SqlDbType.NVarChar,100),
                new SqlParameter("@pageSize",SqlDbType.Int,4),
                new SqlParameter("@pageIndex",SqlDbType.Int,4),
                new SqlParameter("@orderType",SqlDbType.Bit,1),
                new SqlParameter("@where",SqlDbType.NVarChar,1500),
                new SqlParameter("@totalRecord",SqlDbType.Int)
                };
            _PagePara[0].Value = _tblName;
            _PagePara[1].Value = _getCols;
            _PagePara[2].Value = KeyColName;
            _PagePara[3].Value = PageSize;
            _PagePara[4].Value = PageIndex;
            _PagePara[5].Value = _orderType;
            _PagePara[6].Value = _where;
            if (this.PageIndex > 1 && getTotal > 0)
            {
                _PagePara[7].Value = getTotal.ToString();
            }
            else
            {
                _PagePara[7].Direction = ParameterDirection.Output;
            }
            IList<KVPair> data = DbHelper.ExecuteDataTableToIListKVPair(CommandType.StoredProcedure, "[dbo].[ecs_pagedatabykeyt]", _PagePara);

            RecordCount = (PageIndex > 1 && getTotal > 0) ? getTotal : int.Parse(_PagePara[7].Value.ToString());
            this.PageCount = (this.RecordCount + PageSize - 1) / PageSize;//总页数
            return data;
        }

        /// <summary>
        /// 单表主键排序分页
        /// </summary>
        /// <param name="_tblName"></param>
        /// <param name="_getCols"></param>
        /// <param name="_orderType"></param>
        /// <param name="_where"></param>
        /// <param name="getTotal"></param>
        /// <returns></returns>
        public IList<KVPair> PagerByKeyT(string strconn, string _tblName, string _getCols, int _orderType, string _where, int getTotal, string dbname = null)
        {
            SqlParameter[] _PagePara = new SqlParameter[]{
                new SqlParameter("@tblName", SqlDbType.NVarChar,100),
                new SqlParameter("@getCols", SqlDbType.NVarChar,1000),
                new SqlParameter("@keyColName",SqlDbType.NVarChar,100),
                new SqlParameter("@pageSize",SqlDbType.Int,4),
                new SqlParameter("@pageIndex",SqlDbType.Int,4),
                new SqlParameter("@orderType",SqlDbType.Bit,1),
                new SqlParameter("@where",SqlDbType.NVarChar,1500),
                new SqlParameter("@totalRecord",SqlDbType.Int)
                };
            _PagePara[0].Value = _tblName;
            _PagePara[1].Value = _getCols;
            _PagePara[2].Value = KeyColName;
            _PagePara[3].Value = PageSize;
            _PagePara[4].Value = PageIndex;
            _PagePara[5].Value = _orderType;
            _PagePara[6].Value = _where;
            if (this.PageIndex > 1 && getTotal > 0)
            {
                _PagePara[7].Value = getTotal.ToString();
            }
            else
            {
                _PagePara[7].Direction = ParameterDirection.Output;
            }
            IList<KVPair> data = DbHelper.ExecuteDataTableToIListKVPair(strconn,CommandType.StoredProcedure, "[dbo].[ecs_pagedatabykeyt]", _PagePara);

            RecordCount = (PageIndex > 1 && getTotal > 0) ? getTotal : int.Parse(_PagePara[7].Value.ToString());
            this.PageCount = (this.RecordCount + PageSize - 1) / PageSize;//总页数
            return data;
        }

        /// <summary>
        /// 按照ROW_NUMBER()进行排序，排序字段无需order by，条件无需where；注意：查询返回的结果集中第一个字段为排序号rowID而不是请求查询字段列表中的第一个
        /// </summary>
        /// <param name="tblName">表名</param>
        /// <param name="selectCols">查询的字段列表</param>
        /// <param name="whereString">条件，无需where</param>
        /// <param name="orderString">排序字段列表，无需order by</param>
        /// <param name="getTotal">满足条件的总记录数量</param>
        /// <returns></returns>
        public IList<KVPair> PagerByRowNumber_List(string tblName, string selectCols, string whereString, string orderString, int getTotal)
        {
            SqlParameter[] _Para = new SqlParameter[]{        
                new SqlParameter("@TableName", SqlDbType.NVarChar, 100), 
                new SqlParameter("@Fields", SqlDbType.NVarChar, 1500), 
                new SqlParameter("@OrderField", SqlDbType.NVarChar,200),
                new SqlParameter("@sqlWhere", SqlDbType.NVarChar,2000), 
                new SqlParameter("@pageSize", SqlDbType.Int,4),
                new SqlParameter("@pageIndex", SqlDbType.Int,4),
                new SqlParameter("@totalRecord",SqlDbType.Int)
                };
            _Para[0].Value = tblName;
            _Para[1].Value = selectCols;
            _Para[2].Value = orderString;
            _Para[3].Value = whereString;
            _Para[4].Value = PageSize <= 0 ? 10 : PageSize;
            _Para[5].Value = PageIndex <= 0 ? 1 : PageIndex;

            if (PageIndex > 1 && getTotal > 0)
            {
                _Para[6].Value = getTotal.ToString();
            }
            else
            {
                _Para[6].Direction = ParameterDirection.Output;
            }
            IList<KVPair> data = DbHelper.ExecuteDataTableToIListKVPair(CommandType.StoredProcedure, "[dbo].[ecs_pagedatarow]", _Para);

            RecordCount = (PageIndex > 1 && getTotal > 0) ? getTotal : int.Parse(_Para[6].Value.ToString());
            this.PageCount = (this.RecordCount + PageSize - 1) / PageSize;//总页数
            return data;
        }

        /// <summary>
        /// 按照ROW_NUMBER()进行排序，排序字段无需order by，条件无需where；注意：查询返回的结果集中第一个字段为排序号rowID而不是请求查询字段列表中的第一个
        /// </summary>
        /// <param name="tblName">表名</param>
        /// <param name="selectCols">查询的字段列表</param>
        /// <param name="whereString">条件，无需where</param>
        /// <param name="orderString">排序字段列表，无需order by</param>
        /// <param name="getTotal">满足条件的总记录数量</param>
        /// <returns></returns>
        public IList<KVPair> PagerByRowNumber_List(string strconn, string tblName, string selectCols, string whereString, string orderString, int getTotal)
        {
            SqlParameter[] _Para = new SqlParameter[]{        
                new SqlParameter("@TableName", SqlDbType.NVarChar, 100), 
                new SqlParameter("@Fields", SqlDbType.NVarChar, 1500), 
                new SqlParameter("@OrderField", SqlDbType.NVarChar,200),
                new SqlParameter("@sqlWhere", SqlDbType.NVarChar,2000), 
                new SqlParameter("@pageSize", SqlDbType.Int,4),
                new SqlParameter("@pageIndex", SqlDbType.Int,4),
                new SqlParameter("@totalRecord",SqlDbType.Int)
                };
            _Para[0].Value = tblName;
            _Para[1].Value = selectCols;
            _Para[2].Value = orderString;
            _Para[3].Value = whereString;
            _Para[4].Value = PageSize <= 0 ? 10 : PageSize;
            _Para[5].Value = PageIndex <= 0 ? 1 : PageIndex;

            if (PageIndex > 1 && getTotal > 0)
            {
                _Para[6].Value = getTotal.ToString();
            }
            else
            {
                _Para[6].Direction = ParameterDirection.Output;
            }
            IList<KVPair> data = DbHelper.ExecuteDataTableToIListKVPair(strconn,CommandType.StoredProcedure, "[dbo].[ecs_pagedatarow]", _Para);

            RecordCount = (PageIndex > 1 && getTotal > 0) ? getTotal : int.Parse(_Para[6].Value.ToString());
            this.PageCount = (this.RecordCount + PageSize - 1) / PageSize;//总页数
            return data;
        }
        //public DataTable PagerByKey(string _tblName, string _getCols, int _orderType, string _where, int getTotal)
        //{
        //    SqlParameter[] _PagePara = new SqlParameter[]{
        //        new SqlParameter("@tblName", SqlDbType.NVarChar,100),
        //        new SqlParameter("@getCols", SqlDbType.NVarChar,1000),
        //        new SqlParameter("@keyColName",SqlDbType.NVarChar,100),
        //        new SqlParameter("@pageSize",SqlDbType.Int,4),
        //        new SqlParameter("@pageIndex",SqlDbType.Int,4),
        //        new SqlParameter("@orderType",SqlDbType.Bit,1),
        //        new SqlParameter("@where",SqlDbType.NVarChar,1500),
        //        new SqlParameter("@totalRecord",SqlDbType.Int)
        //        };
        //    _PagePara[0].Value = _tblName;
        //    _PagePara[1].Value = _getCols;
        //    _PagePara[2].Value = KeyColName;
        //    _PagePara[3].Value = PageSize;
        //    _PagePara[4].Value = PageIndex;
        //    _PagePara[5].Value = _orderType;
        //    _PagePara[6].Value = _where;
        //    if (this.PageIndex > 1 && getTotal > 0)
        //    {
        //        _PagePara[7].Value = getTotal.ToString();
        //    }
        //    else
        //    {
        //        _PagePara[7].Direction = ParameterDirection.Output;
        //    }
        //    DataTable dt = Data.DbHelper.ExecuteDataTable("ecs_pagedatabykey", _PagePara);
        //    RecordCount = (PageIndex > 1 && getTotal > 0) ? getTotal : int.Parse(_PagePara[7].Value.ToString());
        //    return dt;
        //}

        /// <summary>
        ///  sql语句按某字段排序分页含有order by 排序表达式
        /// </summary>
        /// <param name="_cmdText"></param>
        /// <param name="_orderName">含有order by 排序表达式</param>
        /// <returns></returns>
        /// 
        public DataTable PagerBySql(string dbConnString, string _cmdText, string _orderName)//-含有order by 排序表达式
        {
            if (!string.IsNullOrEmpty(_cmdText))
            {
                SqlParameter[] _Para = new SqlParameter[]{        
new SqlParameter("@SQLStr", SqlDbType.NVarChar, 3000), 
new SqlParameter("@PageIndex", SqlDbType.Int,4), 
new SqlParameter("@PageSize", SqlDbType.Int,4), 
new SqlParameter("@KeyID", SqlDbType.NVarChar,255), 
new SqlParameter("@SortStr", SqlDbType.NVarChar,1000) 
};
                _Para[0].Value = _cmdText;
                _Para[1].Value = this.PageIndex;
                _Para[2].Value = this.PageSize;
                _Para[3].Value = this.KeyColName;
                _Para[4].Value = _orderName;//-含有order by 排序表达式

                DataSet ds = DbHelper.ExecuteDataSet(dbConnString, CommandType.StoredProcedure, "ecs_pagedatabysql", _Para);
                RecordCount = int.Parse(ds.Tables[1].Rows[0][0].ToString());
                return ds.Tables[0];
            }
            return null;
        }

        public DataTable PagerBySql(string _cmdText, string _orderName)//-含有order by 排序表达式
        {
            return PagerBySql(DbHelper.DbConnString_main, _cmdText, _orderName);
        }

        public string PageLinker(PagerInfo obj, bool whenNo_AutoPrePage = false)
        {
            if (whenNo_AutoPrePage)
            {
                this.PageIndex = PageIndex - 1 > 0 ? PageIndex - 1 : 0;
                this.RecordCount = this.RecordCount - 1;
                Utils.RedirectPrePage(this.PageIndex);
            }
            if (this.RecordCount > 0)
            {
                this.PageCount = (this.RecordCount + PageSize - 1) / PageSize;//总页数
                if (this.PageCount > 1)
                {
                    StringBuilder t = new StringBuilder();
                    double pageStep = 9;
                    double beginPage = PageIndex - Math.Ceiling(pageStep / 2);
                    if (beginPage <= 0) { beginPage = 1; }
                    double endpage = 0;
                    if (PageIndex + Math.Ceiling(pageStep / 2) > PageCount)
                    {
                        endpage = PageCount;
                    }
                    else
                    {
                        endpage = pageStep + beginPage;
                        if (endpage > PageCount) { endpage = PageCount; }
                    }

                    if (PageIndex - 1 > 0)
                    {
                        obj.prePageIndex = (PageIndex - 1);
                    }
                    obj.beginPageIndex = beginPage;
                    obj.endPageIndex = endpage;
                    if (PageIndex < PageCount)
                    {
                        obj.nextPageIndex = (PageIndex + 1);
                    }
                }
                obj.pageTotal = this.PageCount;
                obj.currPageIndex = this.PageIndex;
                obj.recordCount = this.RecordCount;

            } return string.Empty;
        }
        public string PageLink(string hrefStr)
        {
            if (this.RecordCount > 0)
            {
                this.PageCount = (this.RecordCount + PageSize - 1) / PageSize;//总页数
                if (this.PageCount > 1)
                {
                    StringBuilder t = new StringBuilder();
                    double pageStep = 9;
                    double beginPage = PageIndex - Math.Ceiling(pageStep / 2);
                    if (beginPage <= 0) { beginPage = 1; }
                    double endpage = 0;
                    if (PageIndex + Math.Ceiling(pageStep / 2) > PageCount)
                    {
                        endpage = PageCount;
                    }
                    else
                    {
                        endpage = pageStep + beginPage;
                        if (endpage > PageCount) { endpage = PageCount; }
                    }
                    if (hrefStr.Contains("?"))
                    {
                        hrefStr = string.Format("{0}&num={1}", hrefStr, RecordCount.ToString());
                    }
                    else
                    {
                        hrefStr = string.Format("{0}?num={1}", hrefStr, RecordCount.ToString());
                    }
                    if (PageIndex - 1 > 0)
                    {
                        t.AppendFormat(" <a href='{0}&page={1}'>上一页</a> ", hrefStr, (PageIndex - 1).ToString());
                    }
                    for (double k = beginPage; k <= endpage; k++)
                    {
                        if (k == PageIndex)
                        {
                            t.AppendFormat(" <a href='javascript:void(0);' class='pagenow'>{0}</a>", k.ToString());
                        }
                        else
                        {
                            t.AppendFormat(" <a href='{0}&page={1}'>{1}</a> ", hrefStr, k.ToString());
                        }
                    }
                    if (PageIndex < PageCount)
                    {
                        t.AppendFormat(" <a href='{0}&page={1}'>下一页</a> ", hrefStr, (PageIndex + 1).ToString());
                    }
                    t.AppendFormat("<span class='totalnum'>共{0}</span>", RecordCount.ToString());
                    return t.ToString();
                }

            } return string.Empty;
        }
    }
}
