using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZMNet.DatabaseUpdater
{
  public class SqlHelper
  {
    public SqlHelper()
    {
    }

    public SqlHelper(string conn, int timeout = 0)
    {
      ConnectionString = conn;
      Timeout = timeout;
    }

    public string ConnectionString { get; set; }

    public static void CallWithTimeout(Action action, int timeout)
    {
      Thread threadToKill = null;

      Action wrappedAction = () =>
      {
        threadToKill = Thread.CurrentThread;

        action?.Invoke();
      };

      IAsyncResult result = wrappedAction.BeginInvoke(null, null);

      if (result.AsyncWaitHandle.WaitOne(timeout))
      {
        wrappedAction.EndInvoke(result);
      }
      else
      {
        if (threadToKill != null)
        {
          Task.Factory.StartNew(threadToKill.Abort);
        }

        throw new TimeoutException();
      }
    }

    public static bool CanConnected(string connectionString, int timeout)
    {
      using (var sqlConn = new SqlConnection(connectionString))
      {
        try
        {
          CallWithTimeout(sqlConn.Open, timeout);
        }
        catch (TimeoutException)
        {
          return false;
        }
        catch (Exception)
        {
          return false;
        }
        finally
        {
          Dispose(sqlConn);
        }
      }

      return true;
    }

    public int Timeout { get; set; }

    void TryOpenConn(SqlConnection sqlConn)
    {
      if (sqlConn.State == ConnectionState.Open)
      {
        return;
      }

      if (Timeout > 0)
      {
        try
        {
          CallWithTimeout(sqlConn.Open, Timeout);
        }
        catch (TimeoutException timeoutException)
        {
          throw timeoutException;
        }
        catch (Exception exception)
        {
          throw exception;
        }
      }
      else
      {
        sqlConn.Open();
      }
    }

    string _databaseName;

    public string DatabaseName
    {
      get
      {
        if (string.IsNullOrEmpty(_databaseName))
        {
          _databaseName = DataTool.GetDatabaseName(ConnectionString);
        }

        return _databaseName;
      }
      set
      {
        _databaseName = null;
      }
    }

    public void ExecuteNonQuery(KeyValuePair<string, List<string>> updateItme)
    {
      SqlConnection sqlConn = null;
      SqlTransaction trans = null;

      try
      {
        sqlConn = new SqlConnection(ConnectionString);
        TryOpenConn(sqlConn);
        trans = sqlConn.BeginTransaction(IsolationLevel.Serializable);

        SqlCommand cmd = new SqlCommand
        {
          Connection = sqlConn,
          Transaction = trans,
          CommandTimeout = Timeout
        };

        foreach (var sql in updateItme.Value)
        {
          cmd.CommandText = sql;
          cmd.ExecuteNonQuery();
        }

        trans.Commit();
        trans = null;
      }
      catch (Exception exc)
      {
        if (trans != null) trans.Rollback();
        throw new UpdaterException(string.Format("Error occurs during updating {0}.sql, detail message is below: <br/>" + exc.Message, updateItme.Key));
      }
      finally
      {
        Dispose(sqlConn);
      }
    }

    public bool ExistsTable(string tableName)
    {
      string query = string.Format("Select * from Information_Schema.Tables where table_name='{0}'", tableName);

      bool existsTable = ExecuteDataTable(query).Rows.Count > 0;

      return existsTable;
    }

    SqlCommand GetSqlCommand(SqlConnection sqlConn, SqlTransaction trans, string commandText, params SqlParameter[] parameters)
    {
      TryOpenConn(sqlConn);
      SqlCommand command = new SqlCommand(commandText, sqlConn);
      if (parameters.Length > 0)
      {
        command.Parameters.AddRange(parameters);
      }
      if (trans != null)
      {
        command.Transaction = trans;
      }
      return command;
    }

    public string GetPagedOrderedSearchCommandText(string columns, string rowColumn, string table,
      int pageIndex = 0, int pageSize = 20, string condition = "", string order = "")
    {
      StringBuilder sqlBase = new StringBuilder();
      if (string.IsNullOrEmpty(order))
      {
        order = rowColumn;
      }

      #region SQL2000专用语句
      //sqlBase.AppendFormat("select top {0} {1} from {2} where {3} not in", count, columns, table, rowColumn);
      //sqlBase.AppendFormat("(select top {0} {1} from {2} where 1 = 1 {3} order by {4}){3} order by {4};", startIndex - 1, rowColumn, table, condition, order);
      #endregion

      #region SQL2005专用语句
      //sqlBase.AppendFormat("select {0} from (select row_number() over(order by {1})", columns, rowColumn);
      //sqlBase.AppendFormat("as generatedId, {0} from {1} where 1 = 1 {2}) as tempTable ", columns, table, condition);
      //sqlBase.AppendFormat(" where generatedId between {0} and {1} order by {2};", startIndex, count + startIndex - 1, order);
      #endregion

      #region 新写的
      sqlBase.AppendFormat("SELECT {0} FROM (SELECT ROW_NUMBER() OVER(ORDER BY {1}) AS rownum,", columns, order);
      sqlBase.AppendFormat("{0} FROM {1} where 1 = 1 {2} ) AS D ", columns, table, condition);
      sqlBase.AppendFormat("WHERE rownum BETWEEN {0} AND {1} ", pageIndex * pageSize + 1, ++pageIndex * pageSize);
      sqlBase.AppendFormat("ORDER BY {0} ;", order);
      /*
       SELECT id, name FROM (SELECT ROW_NUMBER() OVER(ORDER BY id asc) AS rownum, 
          id , name
          FROM Users ) AS D 
          WHERE rownum BETWEEN (3-1)*100+1 AND 3*100 
          ORDER BY id asc 
       */
      #endregion

      sqlBase.AppendFormat("select count(1) from {0} where 1 = 1 {1};", table, condition);
      return sqlBase.ToString();
    }

    public int ExecuteNonQuery(bool useTrans, string commandText, params SqlParameter[] parameters)
    {
      using (SqlConnection sqlConn = new SqlConnection(ConnectionString))
      {
        SqlTransaction trans = null;
        if (useTrans)
        {
          TryOpenConn(sqlConn);
          trans = sqlConn.BeginTransaction(IsolationLevel.ReadCommitted);
        }

        SqlCommand cmd = GetSqlCommand(sqlConn, trans, commandText, parameters);
        if (useTrans)
        {
          try
          {
            int result = cmd.ExecuteNonQuery();
            trans.Commit();
            return result;
          }
          catch (Exception exception)
          {
            trans.Rollback();
            throw exception;
          }
          finally
          {
            Dispose(sqlConn);
          }
        }
        else
        {
          return cmd.ExecuteNonQuery();
        }
      }
    }

    public SqlDataReader ExecuteReader(string commandText, params SqlParameter[] parameters)
    {
      SqlConnection conn = new SqlConnection(ConnectionString);
      SqlCommand cmd = GetSqlCommand(conn, null, commandText, parameters);
      return cmd.ExecuteReader(CommandBehavior.CloseConnection);
    }

    public T ExecuteScalar<T>(string commandText, params SqlParameter[] parameters)
    {
      using (SqlConnection conn = new SqlConnection(ConnectionString))
      {
        SqlCommand cmd = GetSqlCommand(conn, null, commandText, parameters);
        object obj = cmd.ExecuteScalar();
        T result = default(T);
        if (obj is T)
        {
          result = (T)obj;
        }
        return result;
      }
    }

    public DataSet ExecuteDataSet(string commandText, params SqlParameter[] parameters)
    {
      DataSet ds = new DataSet();
      using (SqlConnection conn = new SqlConnection(ConnectionString))
      {
        SqlCommand cmd = GetSqlCommand(conn, null, commandText, parameters);
        SqlDataAdapter da = new SqlDataAdapter(cmd);
        da.Fill(ds);
      }
      return ds;
    }

    public int ExecuteRecordCount(SqlDataReader dr)
    {
      if (dr.NextResult() && dr.Read() && int.TryParse(dr[0].ToString(), out int count))
      {
        return count;
      }
      dr.Close();
      return 0;
    }

    public DataTable ExecuteDataTable(string commandText, params SqlParameter[] parameters)
    {
      DataSet ds = ExecuteDataSet(commandText, parameters);
      if (ds.Tables.Count > 0)
      {
        return ds.Tables[0];
      }

      return new DataTable();
    }

    public bool Exists(string commandText, params SqlParameter[] parameters)
    {
      bool result = false;
      SqlDataReader dr = ExecuteReader(commandText, parameters);
      result = dr.Read();
      dr.Close();
      return result;
    }

    public static void Dispose(SqlConnection sqlConn)
    {
      if (sqlConn.State == ConnectionState.Open || sqlConn.State == ConnectionState.Broken)
      {
        sqlConn.Close();
      }
    }
  }
}
