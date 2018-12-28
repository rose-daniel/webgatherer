using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using vip.common;

namespace vip.dal
{
    public class DbHelper
    {
        public static string DbConnString_main
        {get { return ConfigurationManager.ConnectionStrings["connString_main"].ToString();}}

        #region ExecuteReader---SqlDataReader
        /// <summary>
        ///  数据读取器SqlDataReader----可自定义数据库连接串，执行存储过程或SQL语句
        /// </summary>
        /// <param name="commandType">commandType类型</param>
        /// <param name="commandText">SQL语句或存储过程名称</param>
        /// <param name="commandParams">参数--无则为null</param>
        /// <returns>SqlDataReader</returns>
        public static SqlDataReader ExecuteReader(CommandType commandType, string commandText, params SqlParameter[] commandParams)
        {
            SqlCommand cmd = new SqlCommand();
            SqlConnection conn = new SqlConnection(DbConnString_main);
            try
            {
                SetCommand(cmd, conn, commandType, commandText, commandParams);
                SqlDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();
                return rdr;
            }
            catch
            {
                conn.Close();
                conn.Dispose();
                throw;
            }
        }
        public static KVPair ExecuteReaderToKVPair(CommandType commandType, string commandText, params SqlParameter[] commandParams)
        {
            using (SqlConnection conn = new SqlConnection(DbConnString_main))
            {
                KVPair info = null;
                SqlCommand cmd = new SqlCommand();
                SetCommand(cmd, conn, commandType, commandText, commandParams);
                using (SqlDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    int colsLen = rdr.FieldCount;
                    while (rdr.Read())
                    {
                        info = new KVPair();
                        for (int i = 0; i < colsLen; i++)
                        {
                            info.Add(rdr.GetName(i), rdr[i].ToString());
                        }
                    }
                }
                cmd.Parameters.Clear();
                return info;
            }
        }
        public static KVPair ExecuteReaderToKVPair(string strconn, CommandType commandType, string commandText, params SqlParameter[] commandParams)
        {
            using (SqlConnection conn = new SqlConnection(strconn))
            {
                KVPair info = null;
                SqlCommand cmd = new SqlCommand();
                SetCommand(cmd, conn, commandType, commandText, commandParams);
                using (SqlDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    int colsLen = rdr.FieldCount;
                    while (rdr.Read())
                    {
                        info = new KVPair();
                        for (int i = 0; i < colsLen; i++)
                        {
                            info.Add(rdr.GetName(i), rdr[i].ToString());
                        }
                    }
                }
                cmd.Parameters.Clear();
                return info;
            }
        }
        /// <summary>
        /// 返回IList<KVPair>数据行集合
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <param name="commandParams"></param>
        /// <returns></returns>
        public static IList<KVPair> ExecuteReaderToIListKVPair(CommandType commandType, string commandText, params SqlParameter[] commandParams)
        {
            using (SqlConnection conn = new SqlConnection(DbConnString_main))
            {
                IList<KVPair> list = new List<KVPair>();
                SqlCommand cmd = new SqlCommand();
                SetCommand(cmd, conn, commandType, commandText, commandParams);
                using (SqlDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    KVPair row = null;
                    int colsLen = rdr.FieldCount;
                    while (rdr.Read())
                    {
                        row = new KVPair();
                        for (int i = 0; i < colsLen; i++)
                        {
                            row.Add(rdr.GetName(i), rdr[i].ToString());
                        }
                        list.Add(row);
                    }
                }
                cmd.Parameters.Clear();
                return list;
            }
        }

        /// <summary>
        /// 返回IList<KVPair>数据行集合
        /// </summary>
        /// <param name="strconn"></param>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <param name="commandParams"></param>
        /// <returns></returns>
        public static IList<KVPair> ExecuteReaderToIListKVPair(string strconn, CommandType commandType, string commandText, params SqlParameter[] commandParams)
        {
            using (SqlConnection conn = new SqlConnection(strconn))
            {
                IList<KVPair> list = new List<KVPair>();
                SqlCommand cmd = new SqlCommand();
                SetCommand(cmd, conn, commandType, commandText, commandParams);
                using (SqlDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    KVPair row = null;
                    int colsLen = rdr.FieldCount;
                    while (rdr.Read())
                    {
                        row = new KVPair();
                        for (int i = 0; i < colsLen; i++)
                        {
                            row.Add(rdr.GetName(i), rdr[i].ToString());
                        }
                        list.Add(row);
                    }
                }
                cmd.Parameters.Clear();
                return list;
            }
        }
        /// <summary>
        /// 数据读取器SqlDataReader-----只执行存储过程
        /// </summary>
        /// <param name="procedureName">存储过程名称</param>
        /// <param name="commandParams">参数--无则为null</param>
        /// <returns>SqlDataReader</returns>
        public static SqlDataReader ExecuteReader(string procedureName, params SqlParameter[] commandParams)
        {
            return ExecuteReader(CommandType.StoredProcedure, procedureName, commandParams);
        }
        /// <summary>
        /// 数据读取器SqlDataReader-----只执行SQL语句
        /// </summary>
        /// <param name="commandText">SQL语句</param>
        /// <returns>SqlDataReader</returns>
        public static SqlDataReader ExecuteReader(string commandText)
        {
            return ExecuteReader(CommandType.Text, commandText, null);
        }
        #endregion

        #region ExecuteDataSet---DataSet
        /// <summary>
        /// 填充DataSet----可自定义数据库连接串，执行存储过程或SQL语句
        /// </summary>
        /// <param name="connectionString">数据库连接串</param>
        /// <param name="commandType">commandType类型</param>
        /// <param name="commandText">SQL语句或存储过程名称</param>
        /// <param name="commandParams">参数--无则为null</param>
        /// <returns>DataSet</returns>
        public static DataSet ExecuteDataSet(CommandType commandType, string commandText, params SqlParameter[] commandParams)
        {
            return ExecuteDataSet(DbConnString_main, commandType, commandText, commandParams);
        }
        public static DataSet ExecuteDataSet(string dbConnString, CommandType commandType, string commandText, params SqlParameter[] commandParams)
        {
            using (SqlConnection conn = new SqlConnection(dbConnString))
            {
                DataSet __ds = new DataSet();
                SqlCommand command = new SqlCommand();
                SqlDataAdapter sda = new SqlDataAdapter();

                SetCommand(command, conn, commandType, commandText, commandParams);
                sda.SelectCommand = command;
                sda.Fill(__ds);
                command.Parameters.Clear();
                return __ds;
            }
        }
        /// <summary>
        /// 填充DataSet-----只执行存储过程
        /// </summary>
        /// <param name="procedureName">存储过程名称</param>
        /// <param name="commandParams">参数--无则为null</param>
        /// <returns>DataSet</returns>
        public static DataSet ExecuteDataSet(string procedureName, params SqlParameter[] commandParams)
        {
            return ExecuteDataSet(CommandType.StoredProcedure, procedureName, commandParams);
        }
        /// <summary>
        /// 填充DataSet-----只执行SQL语句
        /// </summary>
        /// <param name="commandText">SQL语句</param>
        /// <returns>DataSet</returns>
        public static DataSet ExecuteDataSet(string commandText)
        {
            return ExecuteDataSet(CommandType.Text, commandText, null);
        }
        #endregion

        #region ExecuteDataTable----object
        /// <summary>
        /// 填充DataTable----可自定义数据库连接串，执行存储过程或SQL语句
        /// </summary>
        /// <param name="commandType">commandType类型</param>
        /// <param name="commandText">SQL语句或存储过程名称</param>
        /// <param name="commandParams">参数--无则为null</param>
        /// <returns>DataTable</returns>
        public static DataTable ExecuteDataTable(CommandType commandType, string commandText, params SqlParameter[] commandParams)
        {
            return ExecuteDataTable(DbConnString_main, commandType, commandText, commandParams);
        }
        public static DataTable ExecuteDataTable(string dbConnString, CommandType commandType, string commandText, params SqlParameter[] commandParams)
        {
            using (SqlConnection conn = new SqlConnection(dbConnString))
            {
                DataTable __dt = new DataTable();
                SqlCommand command = new SqlCommand();
                SqlDataAdapter sda = new SqlDataAdapter();

                SetCommand(command, conn, commandType, commandText, commandParams);
                sda.SelectCommand = command;
                sda.Fill(__dt);
                command.Parameters.Clear();
                return __dt;
            }
        }
        public static KVPair ExecuteDataTableToKVPair(CommandType commandType, string commandText, params SqlParameter[] commandParams)
        {
            return ExecuteDataTableToKVPair(DbConnString_main, commandType, commandText, commandParams);
        }
        public static KVPair ExecuteDataTableToKVPair(string dbConnString, CommandType commandType, string commandText, params SqlParameter[] commandParams)
        {
            using (SqlConnection conn = new SqlConnection(dbConnString))
            {
                DataTable dt = new DataTable();
                SqlCommand command = new SqlCommand();
                SqlDataAdapter sda = new SqlDataAdapter();

                SetCommand(command, conn, commandType, commandText, commandParams);
                sda.SelectCommand = command;
                sda.Fill(dt);
                command.Parameters.Clear();
                KVPair info = null;
                if (dt != null && dt.Rows.Count > 0)
                {
                    info = new KVPair();
                    DataColumnCollection cols = dt.Rows[0].Table.Columns;
                    int colsLen = cols.Count;
                    for (int k = 0; k < colsLen; k++)
                    {
                        info.Add(cols[k].ColumnName, dt.Rows[0][k].ToString());
                    }
                }
                dt.Dispose();
                return info;
            }
        }
        public static IList<KVPair> ExecuteDataTableToIListKVPair(CommandType commandType, string commandText, params SqlParameter[] commandParams)
        {
            using (SqlConnection conn = new SqlConnection(DbConnString_main))
            {
                DataTable dt = new DataTable();
                SqlCommand command = new SqlCommand();
                SqlDataAdapter sda = new SqlDataAdapter();

                SetCommand(command, conn, commandType, commandText, commandParams);
                sda.SelectCommand = command;
                sda.Fill(dt);
                command.Parameters.Clear();
                IList<KVPair> list = new List<KVPair>();
                if (dt != null && dt.Rows.Count > 0)
                {
                    int count = dt.Rows.Count;
                    KVPair row = null;
                    DataColumnCollection cols = dt.Rows[0].Table.Columns;
                    int colsLen = cols.Count;
                    for (int i = 0; i < count; i++)
                    {
                        row = new KVPair();
                        for (int k = 0; k < colsLen; k++)
                        {
                            row.Add(cols[k].ColumnName, dt.Rows[i][k].ToString());
                        }
                        list.Add(row);
                    }
                }
                dt.Dispose();
                return list;
            }
        }

        public static IList<KVPair> ExecuteDataTableToIListKVPair(string strconn, CommandType commandType, string commandText, params SqlParameter[] commandParams)
        {
            using (SqlConnection conn = new SqlConnection(strconn))
            {
                DataTable dt = new DataTable();
                SqlCommand command = new SqlCommand();
                SqlDataAdapter sda = new SqlDataAdapter();

                SetCommand(command, conn, commandType, commandText, commandParams);
                sda.SelectCommand = command;
                sda.Fill(dt);
                command.Parameters.Clear();
                IList<KVPair> list = new List<KVPair>();
                if (dt != null && dt.Rows.Count > 0)
                {
                    int count = dt.Rows.Count;
                    KVPair row = null;
                    DataColumnCollection cols = dt.Rows[0].Table.Columns;
                    int colsLen = cols.Count;
                    for (int i = 0; i < count; i++)
                    {
                        row = new KVPair();
                        for (int k = 0; k < colsLen; k++)
                        {
                            row.Add(cols[k].ColumnName, dt.Rows[i][k].ToString());
                        }
                        list.Add(row);
                    }
                }
                dt.Dispose();
                return list;
            }
        }
        /// <summary>
        /// 填充DataTable-----只执行存储过程
        /// </summary>
        /// <param name="procedureName">存储过程名称</param>
        /// <param name="commandParams">参数--无则为null</param>
        /// <returns>DataTable</returns>
        public static DataTable ExecuteDataTable(string procedureName, params SqlParameter[] commandParams)
        {
            return ExecuteDataTable(CommandType.StoredProcedure, procedureName, commandParams);
        }
        /// <summary>
        /// 填充DataTable-----只执行SQL语句
        /// </summary>
        /// <param name="commandText">SQL语句</param>
        /// <returns>DataTable</returns>
        public static DataTable ExecuteDataTable(string commandText)
        {
            return ExecuteDataTable(CommandType.Text, commandText, null);
        }
        /// <summary>
        /// 填充DataTable-----只执行SQL语句
        /// </summary>
        /// <param name="commandText">SQL语句</param>
        /// <returns>DataTable</returns>
        public static DataTable ExecuteDataTable(string strconn, string commandText)
        {
            return ExecuteDataTable(strconn, CommandType.Text, commandText, null);
        }

        #endregion

        #region ExecuteScalar----object
        /// <summary>
        /// ExecuteScalar----可自定义数据库连接串，执行存储过程或SQL语句
        /// </summary>
        /// <param name="connectionString">数据库连接串</param>
        /// <param name="commandType">commandType类型</param>
        /// <param name="commandText">SQL语句或存储过程名称</param>
        /// <param name="commandParams">参数--无则为null</param>
        /// <returns>object</returns>
        public static object ExecuteScalar(string dbConnString, CommandType commandType, string commandText, params SqlParameter[] commandParams)
        {
            using (SqlConnection connection = new SqlConnection(dbConnString))
            {
                SqlCommand command = new SqlCommand();
                SetCommand(command, connection, commandType, commandText, commandParams);
                object val = command.ExecuteScalar();
                command.Parameters.Clear();
                return val;
            }
        }
        /// <summary>
        /// ExecuteScalar----可自定义数据库连接串，执行存储过程或SQL语句
        /// </summary>
        /// <param name="connectionString">数据库连接串</param>
        /// <param name="commandType">commandType类型</param>
        /// <param name="commandText">SQL语句或存储过程名称</param>
        /// <param name="commandParams">参数--无则为null</param>
        /// <returns>object</returns>
        public static object ExecuteScalar(CommandType commandType, string commandText, params SqlParameter[] commandParams)
        {
            using (SqlConnection connection = new SqlConnection(DbConnString_main))
            {
                SqlCommand command = new SqlCommand();
                SetCommand(command, connection, commandType, commandText, commandParams);
                object val = command.ExecuteScalar();
                command.Parameters.Clear();
                return val;
            }
        }
        /// <summary>
        /// ExecuteScalar----只执行存储过程
        /// </summary>
        /// <param name="procedureName">存储过程名称</param>
        /// <param name="commandParams">参数--无则为null</param>
        /// <returns>object</returns>
        public static object ExecuteScalar(string procedureName, params SqlParameter[] commandParams)
        {
            return ExecuteScalar(CommandType.StoredProcedure, procedureName, commandParams);
        }

        /// <summary>
        /// ExecuteScalar-----只执行SQL语句
        /// </summary>
        /// <param name="commandText">SQL语句</param>
        /// <returns>object</returns>
        public static object ExecuteScalar(string commandText)
        {
            return ExecuteScalar(CommandType.Text, commandText, null);
        }

        /// <summary>
        /// ExecuteScalar-----只执行SQL语句
        /// </summary>
        /// <param name="commandText">SQL语句</param>
        /// <returns>object</returns>
        public static object ExecuteScalar(string strconn,string commandText)
        {
            return ExecuteScalar(strconn,CommandType.Text, commandText, null);
        }

        #endregion

        #region ExecuteNonQuery ---int
        public static int ExecuteNonQuery(string dbConn, CommandType commandType, string commandText, params SqlParameter[] commandParams)
        {
            using (SqlConnection conn = new SqlConnection(dbConn))
            {
                SqlCommand command = new SqlCommand();
                SetCommand(command, conn, commandType, commandText, commandParams);
                int val = command.ExecuteNonQuery();
                command.Parameters.Clear();
                return val;
            }
        }
        /// <summary>
        /// ExecuteNonQuery----执行存储过程或SQL语句
        /// </summary>
        /// <param name="commandType">commandType类型</param>
        /// <param name="commandText">SQL语句或存储过程名称</param>
        /// <param name="commandParams">参数--无则为null</param>
        /// <returns>int</returns>
        public static int ExecuteNonQuery(CommandType commandType, string commandText, params SqlParameter[] commandParams)
        {
            return ExecuteNonQuery(DbConnString_main, commandType, commandText, commandParams);
        }
        /// <summary>
        /// ExecuteNonQuery-----只执行存储过程
        /// </summary>
        /// <param name="procedureName">存储过程名称</param>
        /// <param name="commandParams">参数--无则为null</param>
        /// <returns>int</returns>
        public static int ExecuteNonQuery(string procedureName, params SqlParameter[] commandParams)
        {
            return ExecuteNonQuery(CommandType.StoredProcedure, procedureName, commandParams);
        }
        /// <summary>
        /// ExecuteNonQuery-----只执行SQL语句
        /// </summary>
        /// <param name="commandText">SQL语句</param>
        /// <returns>int</returns>
        public static int ExecuteNonQuery(string commandText)
        {
            return ExecuteNonQuery(CommandType.Text, commandText, null);
        }

        public static int ExecuteNonQuery(string strconn, string commandText)
        {
            return ExecuteNonQuery(strconn,CommandType.Text, commandText, null);
        }

        #endregion

        #region SqlCommand处理
        /// <summary>
        ///SqlCommand预处理
        /// </summary>
        /// <param name="command">SqlCommand对象</param>
        /// <param name="conn">数据库连接对象</param>
        /// <param name="commandType">commandType类型</param>
        /// <param name="commandText">SQL语句或存储过程名称</param>
        /// <param name="commandParms">参数--无则为null</param>
        private static void SetCommand(SqlCommand command, SqlConnection conn, CommandType commandType, string commandText, SqlParameter[] commandParms)
        {
            //try
            //{
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            //}
            //catch (Exception ex)
            //{

            //    LogHelper.WriteLog(string.Format("【{0}】异常：{1}；连接串：{2}", DateTime.Now.ToString(), ex.ToString(), conn.ConnectionString + " | " + conn.Database + " | " + conn.DataSource + " | " + conn + "；sql语句：" + commandText), LogHelper.LogLevel.Info);

            //}
            command.Connection = conn;
            command.CommandText = commandText;
            command.CommandType = commandType;
            if (commandParms != null)
            {
                command.Parameters.AddRange(commandParms);
            }
        }
        #endregion
    }
}
