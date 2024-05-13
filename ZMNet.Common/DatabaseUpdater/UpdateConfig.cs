using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZMNet.Common;

namespace ZMNet.DatabaseUpdater
{
  public class UpdateConfig : SqlHelper
  {
    public UpdateConfig(DatabaseType databaseType, string connectionString, UpdateConfig config)
    {
      DatabaseType = databaseType;
      ConnectionString = connectionString;

      SQLFilePath = config.SQLFilePath;
      SQLVersion = config.SQLVersion;
      LowestVersionKey = config.LowestVersionKey;
      Timeout = config.Timeout;

      VersionList = config.VersionList;
    }

    public UpdateConfig(DatabaseType databaseType, string connectionString, string sqlFilePath, Type sqlVersion,
      int lowestVersionKey, int timeout, params DatabaseType[] userDatabaseType)
    {
      DatabaseType = databaseType;
      ConnectionString = connectionString;

      SQLFilePath = sqlFilePath;
      SQLVersion = sqlVersion;
      LowestVersionKey = lowestVersionKey;
      Timeout = timeout;

      UserDatabaseTypes = userDatabaseType;
    }

    internal void Reset(string resourcePath)
    {
      if (!Path.IsPathRooted(SQLFilePath))
      {
        SQLFilePath = Path.Combine(resourcePath, SQLFilePath);
      }

      if (VersionList == null)
      {
        VersionList = DataTool.GetEnumDictionary(SQLVersion).ToDictionary(d => d.Key.Replace("SQL", ""), d => d.Value);
      }
    }

    bool? _isProtected { get; set; }

    public bool IsProtected
    {
      get
      {
        if (_isProtected.HasValue)
        {
          return _isProtected.Value;
        }

        if (ExistsTable("ZM_Protection"))
        {
          string query = "select IsProtected from ZM_Protection";

          _isProtected = ExecuteScalar<bool>(query);
        }
        else
        {
          string sql = @"
create table ZM_Protection
(
[IsProtected] bit not null default(0),
[IsRestricted] bit not null default(0),
)

insert into ZM_Protection values (0, 0)
";

          ExecuteNonQuery(false, sql);

          _isProtected = false;
        }

        return _isProtected.Value;
      }
    }

    VersionList _currentVersion { get; set; }

    public VersionList CurrentVersion
    {
      get
      {
        if (_currentVersion != null)
        {
          return _currentVersion;
        }

        if (ExistsTable("ZM_VersionList"))
        {
          string query = "select top 1 * from ZM_VersionList order by [Key] desc";

          var dt = ExecuteDataTable(query);

          if (dt.Rows.Count > 0)
          {
            var row = dt.Rows[0];

            _currentVersion = new VersionList()
            {
              Key = row.Field<int>("Key"),
              ID = row.Field<string>("ID"),
              AsOfDate = row.Field<DateTime>("AsOfDate"),
              DLLVersionCode = row.Field<string>("DLLVersionCode"),
              Description = row.Field<string>("Description")
            };
          }
          else
          {
            _currentVersion = new VersionList();
          }
        }
        else
        {
          string sql = @"
create table ZM_VersionList
(
[Key] int not null,
[ID] nvarchar(20) not null,
[AsOfDate] datetime not null,
[DLLVersionCode] nvarchar(50) not null,
[Description] nvarchar(300) null,
)";

          ExecuteNonQuery(false, sql);

          _currentVersion = new VersionList();
        }

        return _currentVersion;
      }
      set
      {
        _currentVersion = value;
      }
    }

    internal void ResetCurrentVersion()
    {
      _currentVersion = null;

      _currentVersion = CurrentVersion;
    }

    bool? _need2Update { get; set; }

    public bool Need2Update
    {
      get
      {
        if (_need2Update.HasValue)
        {
          return _need2Update.Value;
        }

        int currentVersionKey = CurrentVersion.Key;

        var versionListCount = VersionList.Count(d => d.Value > currentVersionKey);

        _need2Update = versionListCount > 0;

        return _need2Update.Value;
      }
    }

    string _fullDatabaseName { get; set; }

    public bool DisplayAdminDabaseName { get; set; }

    public string FullDatabaseName
    {
      get
      {
        if (!string.IsNullOrEmpty(_fullDatabaseName))
        {
          return _fullDatabaseName;
        }

        if (!DisplayAdminDabaseName)
        {
          switch (DatabaseType)
          {
            case DatabaseType.Admin:
              _fullDatabaseName = "Admin database";
              break;
            case DatabaseType.NetAdmin:
              _fullDatabaseName = "NetAdmin database";
              break;
          }
        }

        if (string.IsNullOrEmpty(_fullDatabaseName))
        {
          _fullDatabaseName = string.Format("{0} database [{1}]", DatabaseType.ToString(), DatabaseName);
        }

        return _fullDatabaseName;
      }
    }

    public DatabaseType DatabaseType { get; set; }

    /// <summary>
    /// Input the zip file name, convert it to absolute path when using it.
    /// </summary>
    public string SQLFilePath { get; set; }

    public Type SQLVersion { get; set; }

    /// <summary>
    /// Convert from SQLVersion
    /// </summary>
    public Dictionary<string, int> VersionList { get; set; }

    public int LowestVersionKey { get; set; }

    public DatabaseType[] UserDatabaseTypes { get; set; }
  }
}
