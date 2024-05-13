using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace ZMNet.DatabaseUpdater
{
  public class Updater
  {
    /// <summary>
    /// SQL file's directory
    /// </summary>
    string ResourcePath { get; set; }

    public Action<string, bool, string, bool, int> OnUpdate { get; set; }

    SignalRClient CurrentUser { get; set; }

    public UpdateConfig[] UpdateConfigs { get; set; }

    public Updater(string resourcePath, Action<string, bool, string, bool, int> onUpdate, SignalRClient currentUser, params UpdateConfig[] userConfigs)
    {
      ResourcePath = resourcePath;
      OnUpdate = onUpdate;
      CurrentUser = currentUser;

      UpdateConfigs = userConfigs;
    }

    /// <summary>
    /// Check the database, whether it's need to update
    /// </summary>
    /// <returns></returns>
    public bool Need2Update()
    {
      bool need2Update = false;

      foreach (var config in UpdateConfigs)
      {
        config.Reset(ResourcePath);

        need2Update = config.Need2Update;

        if (need2Update)
        {
          return need2Update;
        }
      }

      return need2Update;
    }

    /// <summary>
    /// Auto update without exception
    /// </summary>
    /// <returns></returns>
    public UpdateResult AutoUpdate()
    {
      try
      {
        foreach (var config in UpdateConfigs)
        {
          config.Reset(ResourcePath);

          TryUpdate(config);
        }

        return new UpdateResult();
      }
      catch (Exception exception)
      {
        return new UpdateResult(exception.Message, false);
      }
    }

    void PopMessage(bool isException, string message, bool appendDot = true, int appendLineCount = 0)
    {
      OnUpdate?.Invoke(CurrentUser.ClientID, isException, message, appendDot, appendLineCount);

      HasException = HasException || isException;

      if (isException)
      {
        throw new UpdaterException(message);
      }
    }

    bool HasException { get; set; }

    void Update(UpdateConfig config)
    {
      int currentVersionKey = config.CurrentVersion.Key;

      if (currentVersionKey < config.LowestVersionKey)
      {
        string MessageClsDBC132 = string.Format("The {0} version is older than {1} and it cannot be updated automatically. Please contact ZMFS support.",
          config.FullDatabaseName,
          config.VersionList.First(d => d.Value == config.LowestVersionKey).Key);

        PopMessage(true, MessageClsDBC132, false, 1);
      }

      var versionList = config.VersionList.Where(d => d.Value > currentVersionKey).OrderBy(d => d.Value).Select(d => new ZM_VersionList(d.Value, d.Key)).ToList();

      if (versionList.Count == 0)
      {
        PopMessage(false, string.Format("The {0} is up to date!", config.FullDatabaseName), false, 1);

        return;
      }

      if (config.IsProtected)
      {
        PopMessage(true, string.Format("The {0} is protected, update cancelled.", config.FullDatabaseName), false, 1);
      }

      var currentVersion = config.VersionList.Where(d => d.Value <= currentVersionKey).LastOrDefault();

      string versionChange = string.Format("Start updating {0} from version {1} to {2}",
        config.FullDatabaseName,
        currentVersion.Key,
        versionList.Last().VersionCode
        );

      PopMessage(false, versionChange, true, 1);

      var versionCodeList = versionList.Select(d => d.VersionCode).ToList();

      var updateList = GetUpdateList(config, versionCodeList);

      foreach (var verion in versionList)
      {
        var updateItme = updateList.FirstOrDefault(d => d.Key.Equals(verion.VersionCode, StringComparison.CurrentCultureIgnoreCase));

        if (updateItme.Value == null)
        {
          PopMessage(true, string.Format("Cannot find {0}.sql, please update {1}", verion.VersionCode, Path.GetFileName(config.SQLFilePath)), false, 1);
        }
        else if (updateItme.Value.Count == 0)
        {
          PopMessage(true, string.Format("There is no SQL statements in {0}.sql", verion.VersionCode), false, 1);
        }

        PopMessage(false, string.Format("Updating {0}.sql", updateItme.Key), true, 0);

        config.ExecuteNonQuery(updateItme);

        string sql = @"If not exists(select * from ZM_VersionList where [Key] = {0})
Insert into ZM_VersionList ([Key],[ID],[AsOfDate],[DLLVersionCode],[Description]) values ({0}, '{1}', '{2}', '{3}', '{4}')
Else
  update ZM_VersionList set /*[AsOfDate] = '{2}', */[Description] = '{4}' where [Key] = {0}
";
        //11/27/2018
        //string asOfDate = Regex.Replace(verion.VersionCode, @"^(\d{4})(\d{2})(\d{2})(_\w+)?$", "$2/$3/$1");

        config.ExecuteNonQuery(false, string.Format(sql, verion.Key,
          "SQL" + verion.VersionCode,
          DateTime.Now.ToString(),
          "DLL" + verion.VersionCode,
          string.Format("Updated by online user {0} at {1}", CurrentUser.Username, DateTime.Now.ToString())
          ));
      }

      config.ResetCurrentVersion();

      PopMessage(false, string.Format("The {0} has been updated successfully!", config.FullDatabaseName), false, 2);
    }

    Dictionary<string, List<string>> GetUpdateList(UpdateConfig config, List<string> versionCodeList)
    {
      if (!File.Exists(config.SQLFilePath))
      {
        PopMessage(true, "Cannot find SQLs.zip ", true, 1);
      }

      Dictionary<string, List<string>> updateList = new Dictionary<string, List<string>>();

      using (var archive = ZipArchive.Open(config.SQLFilePath))
      {
        foreach (var entry in archive.Entries)
        {
          if (entry.IsDirectory || !Path.GetExtension(entry.Key).Equals(".sql", StringComparison.CurrentCultureIgnoreCase))
          {
            continue;
          }

          string versionCode = Path.GetFileNameWithoutExtension(entry.Key);

          if (!versionCodeList.Contains(versionCode))
          {
            continue;
          }

          using (var outStream = new MemoryStream())
          {
            entry.WriteTo(outStream);

            var bytes = outStream.ToArray();

            using (var stream = new MemoryStream(bytes))
            {
              using (var reader = new StreamReader(stream))
              {
                #region Read SQL statements

                List<string> sqlList = new List<string>();

                StringBuilder sqlBuilder = new StringBuilder();

                if (config.DatabaseType == DatabaseType.Admin || config.DatabaseType == DatabaseType.NetAdmin)
                {
                  while (!reader.EndOfStream)
                  {
                    string sql = reader.ReadLine().Trim();

                    if (sql.Equals("go", StringComparison.CurrentCultureIgnoreCase))
                    {
                      sqlList.Add(sqlBuilder.ToString());
                      sqlBuilder.Clear();
                    }
                    else if (sql.Equals("BEGIN TRANSACTION", StringComparison.CurrentCultureIgnoreCase) || sql.Equals("COMMIT", StringComparison.CurrentCultureIgnoreCase))
                    {
                      continue;
                    }
                    else
                    {
                      sqlBuilder.AppendLine(sql);
                    }
                  }

                  string lastSQL = sqlBuilder.ToString();

                  if (lastSQL.Length > 0)
                  {
                    sqlList.Add(lastSQL);
                  }
                }
                else
                {
                  string currRow_0 = reader.ReadLine().ToUpper().Trim();
                  string currRow = "";

                  //first get rid of the first two lines of codes: BEGIN TRANSACTION, SET NOCOUNT ON
                  while (null != (currRow = reader.ReadLine()))
                  {
                    if (currRow_0 == "" && currRow.ToUpper().Trim() == "BEGIN TRANSACTION")
                      currRow_0 = "BEGIN TRANSACTION";
                    if (currRow_0 == "BEGIN TRANSACTION" && currRow.ToUpper().Trim() == "SET NOCOUNT ON")
                      break;
                  }

                  //then read in query file
                  while (null != (currRow = reader.ReadLine()))
                  {
                    if (currRow.ToUpper().Contains("ZMFS_DB_UPDATE_END"))
                      break; //Everything here-after skipped
                    if (currRow != "")
                    {
                      if (currRow.Trim().ToUpper() != "GO")
                        sqlBuilder.Append("\r\n" + currRow);
                      if (currRow.Trim().ToUpper() == "GO")
                      {
                        sqlList.Add(sqlBuilder.ToString());
                        sqlBuilder.Remove(0, sqlBuilder.Length);
                      }
                    }
                  }
                }

                updateList.Add(versionCode, sqlList);

                #endregion
              }
            }

          }

        }
      }

      return updateList;
    }

    [Obsolete]
    public List<UpdateConfig> GetRelatedDatabaseList(UpdateConfig config)
    {
      var list = new List<UpdateConfig>();

      if (config.DatabaseType != DatabaseType.User || config.UserDatabaseTypes.Length == 0)
      {
        return list;
      }

      var settingKeys = config.UserDatabaseTypes.Select(d => Convert.ToInt32(d)).ToList();

      var query = string.Format("Select * From UserSetting Where UserKey = 1 And SettingKey in ({0})", string.Join(",", settingKeys));

      try
      {
        var dtSettings = config.ExecuteDataTable(query);

        var databaseList = dtSettings.AsEnumerable().ToDictionary(dr => dr.Field<int>("SettingKey"), dr => dr.Field<string>("SettingValue"));

        var userDBName = DataTool.GetDatabaseName(config.ConnectionString, true);

        foreach (var database in databaseList)
        {
          var otherDBName = DataTool.GetDatabaseName(database.Value, true);

          if (userDBName.Equals(otherDBName, StringComparison.CurrentCultureIgnoreCase))
          {
            continue;
          }

          var otherConfig = new UpdateConfig((DatabaseType)database.Key, database.Value, config);

          list.Add(otherConfig);
        }
      }
      catch (TimeoutException)
      {
        PopException(config);
      }
      catch (Exception exception)
      {
        PopException(exception.Message);
      }

      return list;
    }

    public void PopException(UpdateConfig config)
    {
      string message = string.Format("The {0} cannot be connected, please check the database and permissions.", config.FullDatabaseName);

      PopMessage(true, message, false, 1);
    }

    public void PopException(string message)
    {
      PopMessage(true, message, false, 1);
    }

    void TryUpdate(UpdateConfig config)
    {
      TryDo(Update, config);
    }

    public void TryDo(Action<UpdateConfig> action, UpdateConfig config)
    {
      try
      {
        action(config);
      }
      catch (TimeoutException)
      {
        PopException(config);
      }
      catch (UpdaterException updaterException)
      {
        PopException(updaterException.Message);
      }
      catch (Exception exception)
      {
        PopException(exception.Message);
      }
    }

  }
}
