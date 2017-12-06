using System;
using System.Configuration;
using System.Data;
using System.Data.OracleClient;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Generic;

    /// <summary>
    /// A helper class used to execute queries against an Oracle database
    /// </summary>
public abstract class OracleHelper
{
    // Read the connection strings from the configuration fileWEBDBGUID
    public static readonly string B2BTESTDBConnection = ConfigurationManager.AppSettings["B2BTEST"];
    public static readonly string WEBDBConnection = ConfigurationManager.AppSettings["WEBDB"];
    public static readonly string WEBDBGUIDConnection = ConfigurationManager.AppSettings["WEBDBGUID"];
    public static readonly string ERPConnection = ConfigurationManager.AppSettings["ERP"];
    public static readonly string BPMDBConnection = ConfigurationManager.AppSettings["BPM"];
    public static readonly string WEBGUIDConnection = ConfigurationManager.AppSettings["WEBGUID"];
    

    //Create a hashtable for the parameter cached
    private static Hashtable parmCache = Hashtable.Synchronized(new Hashtable());
    /// <summary>
    /// 执行DML语句(可含参数)
    /// </summary>
    /// <param name="connectionString">数据库连接字符串</param>
    /// <param name="cmdType">SQL类型</param>
    /// <param name="cmdText">SQL语句</param>
    /// <param name="commandParameters">SQL语句参数</param>
    /// <returns>返回受影响的行数</returns>
    public static int ExecuteNonQuery(string connectionString, CommandType cmdType, string cmdText, params OracleParameter[] commandParameters)
    {
        OracleCommand cmd = new OracleCommand();
        using (OracleConnection connection = new OracleConnection(connectionString))
        {
            try
            {
                PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
                int val = cmd.ExecuteNonQuery();
                connection.Close();
                cmd.Parameters.Clear();
                return val;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
    }
    /// <summary>
    /// 执行SQL查询语句(不含参数)
    /// </summary>
    /// <param name="connectionString">数据库连接字符串</param>
    /// <param name="SQLString">SQL语句</param>
    /// <returns>返回查询结果DataSet</returns>
    public static DataSet Query(string connectionString, string SQLString)
    {
        using (OracleConnection connection = new OracleConnection(connectionString))
        {
            DataSet ds = new DataSet();
            try
            {
                connection.Open();
                OracleDataAdapter command = new OracleDataAdapter(SQLString, connection);
                command.Fill(ds, "ds");
            }
            catch (OracleException ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
            }
            return ds;
        }
    }
    /// <summary>
    /// 执行SQL查询语句(可含参数)
    /// </summary>
    /// <param name="connectionString">数据库连接字符串</param>
    /// <param name="SQLString">SQL语句</param>
    /// <param name="cmdParms">SQL语句参数</param>
    /// <returns>返回查询结果DataSet</returns>
    public static DataSet Query(string connectionString, string SQLString, params OracleParameter[] cmdParms)
    {
        using (OracleConnection connection = new OracleConnection(connectionString))
        {
            OracleCommand cmd = new OracleCommand();
            PrepareCommand(cmd, connection, null, SQLString, cmdParms);
            using (OracleDataAdapter da = new OracleDataAdapter(cmd))
            {
                DataSet ds = new DataSet();
                try
                {
                    da.Fill(ds, "ds");
                    cmd.Parameters.Clear();
                }
                catch (System.Data.OracleClient.OracleException ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    if (connection.State != ConnectionState.Closed)
                    {
                        connection.Close();
                    }
                }
                return ds;
            }
        }
    }
    /// <summary>
    /// 执行SQL查询语句(可含参数)
    /// </summary>
    /// <param name="connectionString">数据库连接字符串</param>
    /// <param name="cmdType">SQL语句类型</param>
    /// <param name="cmdText">SQL语句</param>
    /// <param name="cmdParms">SQL语句参数</param>
    /// <returns>返回查询结果DataSet</returns>
    public static DataSet Query(string connectionString, CommandType cmdType, string cmdText, params OracleParameter[] cmdParms)
    {
        using (OracleConnection connection = new OracleConnection(connectionString))
        {
            OracleCommand cmd = new OracleCommand();
            PrepareCommand(cmd, connection, null, cmdType, cmdText, cmdParms);
            using (OracleDataAdapter da = new OracleDataAdapter(cmd))
            {
                DataSet ds = new DataSet();
                try
                {
                    da.Fill(ds, "ds");
                    cmd.Parameters.Clear();
                }
                catch (System.Data.OracleClient.OracleException ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    if (connection.State != ConnectionState.Closed)
                    {
                        connection.Close();
                    }
                }
                return ds;
            }
        }
    }
    /// <summary>
    /// 初始化Command对象
    /// </summary>
    /// <param name="cmd">OracleCommand对象</param>
    /// <param name="conn">OracleConnection对象</param>
    /// <param name="trans">OracleTransaction对象</param>
    /// <param name="cmdText">SQL语句</param>
    /// <param name="cmdParms">SQL语句参数</param>
    private static void PrepareCommand(OracleCommand cmd, OracleConnection conn, OracleTransaction trans, string cmdText, OracleParameter[] cmdParms)
    {
        if (conn.State != ConnectionState.Open)
        {
            conn.Open();
        }
        cmd.Connection = conn;
        cmd.CommandText = cmdText;
        if (trans != null)
        {
            cmd.Transaction = trans;
        }
        cmd.CommandType = CommandType.Text;
        if (cmdParms != null)
        {
            foreach (OracleParameter parameter in cmdParms)
            {
                if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) && (parameter.Value == null))
                {
                    parameter.Value = DBNull.Value;
                }
                cmd.Parameters.Add(parameter);
            }
        }
    }
    /// <summary>
    /// 获取首行首列的值
    /// </summary>
    /// <param name="connectionString">数据库连接字符串</param>
    /// <param name="SQLString">SQL语句</param>
    /// <returns>返回结果对象</returns>
    public static object GetSingle(string connectionString, string SQLString)
    {
        using (OracleConnection connection = new OracleConnection(connectionString))
        {
            using (OracleCommand cmd = new OracleCommand(SQLString, connection))
            {
                try
                {
                    connection.Open();
                    object obj = cmd.ExecuteScalar();
                    if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                    {
                        return null;
                    }
                    else
                    {
                        return obj;
                    }
                }
                catch (OracleException ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    if (connection.State != ConnectionState.Closed)
                    {
                        connection.Close();
                    }
                }
            }
        }
    }
    /// <summary>
    /// 查询是否存在
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="strOracle"></param>
    /// <returns></returns>
    public static bool Exists(string connectionString, string strOracle)
    {
        try
        {
            object obj = OracleHelper.GetSingle(connectionString, strOracle);
            int cmdresult;
            if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
            {
                cmdresult = 0;
            }
            else
            {
                cmdresult = int.Parse(obj.ToString());
            }
            if (cmdresult == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public static bool Exists(string connectionString, string strSql, params OracleParameter[] cmdParms)
    {
        try
        {
            object obj = GetSingle(connectionString, strSql, cmdParms);
            int cmdresult;
            if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
            {
                cmdresult = 0;
            }
            else
            {
                cmdresult = int.Parse(obj.ToString());
            }
            if (cmdresult == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public static object GetSingle(string connectionString, string SQLString, params OracleParameter[] cmdParms)
    {
        using (OracleConnection connection = new OracleConnection(connectionString))
        {
            using (OracleCommand cmd = new OracleCommand())
            {
                try
                {
                    PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                    object obj = cmd.ExecuteScalar();
                    cmd.Parameters.Clear();
                    if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                    {
                        return null;
                    }
                    else
                    {
                        return obj;
                    }
                }
                catch (System.Data.OracleClient.OracleException e)
                {
                    throw new Exception(e.Message);
                }
            }
        }
    }

    /// <summary>
    /// Execute an OracleCommand (that returns no resultset) against an existing database transaction 
    /// using the provided parameters.
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "PublishOrders", new OracleParameter(":prodid", 24));
    /// </remarks>
    /// <param name="trans">an existing database transaction</param>
    /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
    /// <param name="commandText">the stored procedure name or PL/SQL command</param>
    /// <param name="commandParameters">an array of OracleParamters used to execute the command</param>
    /// <returns>an int representing the number of rows affected by the command</returns>
    public static int ExecuteNonQuery(OracleTransaction trans, CommandType cmdType, string cmdText, params OracleParameter[] commandParameters)
    {
        try
        {
            OracleCommand cmd = new OracleCommand();
            PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdText, commandParameters);
            int val = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return val;
        }
        catch (Exception ex)
        {
            return 0;
        }
    }

    /// <summary>
    /// Execute an OracleCommand (that returns no resultset) against an existing database connection 
    /// using the provided parameters.
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new OracleParameter(":prodid", 24));
    /// </remarks>
    /// <param name="conn">an existing database connection</param>
    /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
    /// <param name="commandText">the stored procedure name or PL/SQL command</param>
    /// <param name="commandParameters">an array of OracleParamters used to execute the command</param>
    /// <returns>an int representing the number of rows affected by the command</returns>
    public static int ExecuteNonQuery(OracleConnection connection, CommandType cmdType, string cmdText, params OracleParameter[] commandParameters)
    {
        try
        {
            OracleCommand cmd = new OracleCommand();
            PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
            int val = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return val;
        }
        catch (Exception ex)
        {
            return 0;
        }
    }
    /// <summary>
    /// Execute an OracleCommand (that returns no resultset) against an existing database connection 
    /// using the provided parameters.
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new OracleParameter(":prodid", 24));
    /// </remarks>
    /// <param name="conn">an existing database connection</param>
    /// <param name="commandText">the stored procedure name or PL/SQL command</param>
    /// <returns>an int representing the number of rows affected by the command</returns>
    public static int ExecuteNonQuery(string connectionString, string cmdText)
    {
        try
        {
            OracleCommand cmd = new OracleCommand();
            OracleConnection connection = new OracleConnection(connectionString);
            PrepareCommand(cmd, connection, null, CommandType.Text, cmdText, null);
            int val = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return val;
        }
        catch (Exception ex)
        {
            return 0;
        }
    }

    /// <summary>
    /// Execute a select query that will return a result set
    /// </summary>
    /// <param name="connString">Connection string</param>
    //// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
    /// <param name="commandText">the stored procedure name or PL/SQL command</param>
    /// <param name="commandParameters">an array of OracleParamters used to execute the command</param>
    /// <returns></returns>
    public static OracleDataReader ExecuteReader(string connectionString, CommandType cmdType, string cmdText, params OracleParameter[] commandParameters)
    {
        OracleCommand cmd = new OracleCommand();
        OracleConnection conn = new OracleConnection(connectionString);
        try
        {
            PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
            OracleDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            cmd.Parameters.Clear();
            return rdr;
        }
        catch (Exception ex)
        {
            conn.Close();
            throw;
        }
    }

    /// <summary>
    /// Execute an OracleCommand that returns the first column of the first record against the database specified in the connection string 
    /// using the provided parameters.
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  Object obj = ExecuteScalar(connString, CommandType.StoredProcedure, "PublishOrders", new OracleParameter(":prodid", 24));
    /// </remarks>
    /// <param name="connectionString">a valid connection string for a SqlConnection</param>
    /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
    /// <param name="commandText">the stored procedure name or PL/SQL command</param>
    /// <param name="commandParameters">an array of OracleParamters used to execute the command</param>
    /// <returns>An object that should be converted to the expected type using Convert.To{Type}</returns>
    public static object ExecuteScalar(string connectionString, CommandType cmdType, string cmdText, params OracleParameter[] commandParameters)
    {
        OracleCommand cmd = new OracleCommand();
        using (OracleConnection conn = new OracleConnection(connectionString))
        {
            try
            {
                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                object val = cmd.ExecuteScalar();
                cmd.Parameters.Clear();
                return val;
            }
            catch (Exception ex)
            {
                RabbitMQTest.Utils.LogHelper.Write(ex);
                return "ThrowException";
            }
        }
    }

    ///	<summary>
    ///	Execute	a OracleCommand (that returns a 1x1 resultset)	against	the	specified SqlTransaction
    ///	using the provided parameters.
    ///	</summary>
    ///	<param name="transaction">A	valid SqlTransaction</param>
    ///	<param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    ///	<param name="commandText">The stored procedure name	or PL/SQL command</param>
    ///	<param name="commandParameters">An array of	OracleParamters used to execute the command</param>
    ///	<returns>An	object containing the value	in the 1x1 resultset generated by the command</returns>
    public static object ExecuteScalar(OracleTransaction transaction, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
    {
        try
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked	or commited, please	provide	an open	transaction.", "transaction");
            }
            OracleCommand cmd = new OracleCommand();
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters);
            object retval = cmd.ExecuteScalar();
            cmd.Parameters.Clear();
            return retval;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    /// <summary>
    /// Execute an OracleCommand that returns the first column of the first record against an existing database connection 
    /// using the provided parameters.
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  Object obj = ExecuteScalar(conn, CommandType.StoredProcedure, "PublishOrders", new OracleParameter(":prodid", 24));
    /// </remarks>
    /// <param name="conn">an existing database connection</param>
    /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
    /// <param name="commandText">the stored procedure name or PL/SQL command</param>
    /// <param name="commandParameters">an array of OracleParamters used to execute the command</param>
    /// <returns>An object that should be converted to the expected type using Convert.To{Type}</returns>
    public static object ExecuteScalar(OracleConnection connectionString, CommandType cmdType, string cmdText, params OracleParameter[] commandParameters)
    {
        try
        {
            OracleCommand cmd = new OracleCommand();
            PrepareCommand(cmd, connectionString, null, cmdType, cmdText, commandParameters);
            object val = cmd.ExecuteScalar();
            cmd.Parameters.Clear();
            return val;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    /// <summary>
    /// Add a set of parameters to the cached
    /// </summary>
    /// <param name="cacheKey">Key value to look up the parameters</param>
    /// <param name="commandParameters">Actual parameters to cached</param>
    public static void CacheParameters(string cacheKey, params OracleParameter[] commandParameters)
    {
        parmCache[cacheKey] = commandParameters;
    }

    /// <summary>
    /// Fetch parameters from the cache
    /// </summary>
    /// <param name="cacheKey">Key to look up the parameters</param>
    /// <returns></returns>
    public static OracleParameter[] GetCachedParameters(string cacheKey)
    {
        OracleParameter[] cachedParms = (OracleParameter[])parmCache[cacheKey];
        if (cachedParms == null)
        {
            return null;
        }
        // If the parameters are in the cache
        OracleParameter[] clonedParms = new OracleParameter[cachedParms.Length];
        // return a copy of the parameters
        for (int i = 0, j = cachedParms.Length; i < j; i++)
        {
            clonedParms[i] = (OracleParameter)((ICloneable)cachedParms[i]).Clone();
        }
        return clonedParms;
    }
    /// <summary>
    /// Internal function to prepare a command for execution by the database
    /// </summary>
    /// <param name="cmd">Existing command object</param>
    /// <param name="conn">Database connection object</param>
    /// <param name="trans">Optional transaction object</param>
    /// <param name="cmdType">Command type, e.g. stored procedure</param>
    /// <param name="cmdText">Command test</param>
    /// <param name="commandParameters">Parameters for the command</param>
    private static void PrepareCommand(OracleCommand cmd, OracleConnection conn, OracleTransaction trans, CommandType cmdType, string cmdText, OracleParameter[] commandParameters)
    {

        //Open the connection if required
        if (conn.State != ConnectionState.Open)
        {
            conn.Open();
        }
        //Set up the command
        cmd.Connection = conn;
        cmd.CommandText = cmdText;
        cmd.CommandType = cmdType;

        //Bind it to the transaction if it exists
        if (trans != null)
        {
            cmd.Transaction = trans;
        }
        // Bind the parameters passed in
        if (commandParameters != null)
        {
            foreach (OracleParameter parm in commandParameters)
            {
                cmd.Parameters.Add(parm);
            }
        }
    }

    /// <summary>
    /// Converter to use boolean data type with Oracle
    /// </summary>
    /// <param name="value">Value to convert</param>
    /// <returns></returns>
    public static string OraBit(bool value)
    {
        if (value)
        {
            return "Y";
        }
        else
        {
            return "N";
        }
    }

    /// <summary>
    /// Converter to use boolean data type with Oracle
    /// </summary>
    /// <param name="value">Value to convert</param>
    /// <returns></returns>
    public static bool OraBool(string value)
    {
        if (value.Equals("Y"))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="conStr"></param>
    /// <param name="SQLStringList"></param>
    /// <returns></returns>
    public static bool ExecuteSqlTran(string conStr, List<String> SQLStringList)
    {
        using (OracleConnection conn = new OracleConnection(conStr))
        {
            conn.Open();
            OracleCommand cmd = new OracleCommand();
            cmd.Connection = conn;
            OracleTransaction tx = conn.BeginTransaction();
            cmd.Transaction = tx;
            string strSql = "";
            try
            {

                foreach (string sql in SQLStringList)
                {
                    if (!String.IsNullOrEmpty(sql))
                    {
                        strSql = sql;
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();
                    }
                }
                tx.Commit();
                return true;
            }
            catch (System.Data.OracleClient.OracleException E)
            {
                tx.Rollback();
                throw new Exception(E.Message);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                {
                    conn.Close();
                }
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="conStr"></param>
    /// <param name="SqlString"></param>
    /// <returns></returns>
    public static bool ExecuteSqlTranParams(string conStr, Hashtable SqlString)
    {
        using (OracleConnection conn = new OracleConnection(conStr))
        {
            conn.Open();
            using (OracleTransaction tran = conn.BeginTransaction())
            {
                string strSql = "";
                OracleParameter[] param = null;
                OracleCommand cmd = new OracleCommand();
                try
                {
                    foreach (DictionaryEntry de in SqlString)
                    {
                        OracleParameter[] parms = (OracleParameter[])de.Value;
                        strSql = de.Key.ToString();
                        param = parms;
                        PrepareCommand(cmd, conn, tran, de.Key.ToString(), parms);
                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                    }
                    tran.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    return false;
                }
                finally
                {
                    if (conn.State != ConnectionState.Closed)
                    {
                        conn.Close();
                    }
                }
            }
        }
    }

    public static DataTable QueryByPagination(string ConnectionString, string TableName, string WhereCondition, string OrderColumns, string OrderStyle, int CurrentPage, int PageSize, string CacheParmName, ref int RecordCount)
    {
        OracleParameter[] parms = GetParms(CacheParmName);
        parms[0].Value = TableName;
        parms[1].Value = WhereCondition;
        parms[2].Value = OrderColumns;
        parms[3].Value = OrderStyle;
        parms[4].Value = CurrentPage;
        parms[5].Value = PageSize;

        parms[0].Direction = ParameterDirection.Input;
        parms[1].Direction = ParameterDirection.Input;
        parms[2].Direction = ParameterDirection.Input;
        parms[3].Direction = ParameterDirection.Input;
        parms[4].Direction = ParameterDirection.InputOutput;
        parms[5].Direction = ParameterDirection.InputOutput;
        parms[6].Direction = ParameterDirection.Output;
        parms[7].Direction = ParameterDirection.Output;
        parms[8].Direction = ParameterDirection.Output;

        DataTable dt = Query(ConnectionString, CommandType.StoredProcedure, "pkg_pagination.proc_Pagination", parms).Tables[0];
        RecordCount = Convert.ToInt32(parms[6].Value);
        return dt;
    }

    public static OracleParameter[] GetParms(string CacheParmName)
    {
        OracleParameter[] parms = GetCachedParameters(CacheParmName);
        if (parms == null)
        {
            parms = new OracleParameter[] { new OracleParameter("p_tableName",OracleType.VarChar), 
                                                                    new OracleParameter("p_strWhere",OracleType.VarChar),
                                                                    new OracleParameter("p_orderColumn",OracleType.VarChar),
                                                                    new OracleParameter("p_orderStyle",OracleType.VarChar),
                                                                    new OracleParameter("p_curPage",OracleType.Number),
                                                                    new OracleParameter("p_pageSize",OracleType.Number),
                                                                    new OracleParameter("p_totalRecords",OracleType.Number),
                                                                    new OracleParameter("p_totalPages",OracleType.Number),
                                                                    new OracleParameter("v_cur",OracleType.Cursor)
                };
        }
        CacheParameters("INQUIRY_BY_PROC", parms);
        return parms;
    }
}

