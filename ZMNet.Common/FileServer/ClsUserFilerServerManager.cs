using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMNet.FileServer
{
  [Serializable]
  public class ClsUserFilerServerManager : IFileServer
  {
    private string userKey;
    private string rootPath;
    private string databasePath;

    private string reportPath;
    private string logFilePath;
    private string customReportPath;
    private string userPath;
    private string dataPath;

    public string UserKey
    {
      get { return userKey; }
      private set { userKey = value; }
    }

    public ClsUserFilerServerManager(int userkey, string dbName)
    {
      this.UserKey = userkey.ToString();

      //check root path
      this.rootPath = ConfigurationManager.AppSettings["FileServer"]?.ToString()?.Trim();
      if (string.IsNullOrEmpty(rootPath) || string.IsNullOrWhiteSpace(rootPath))
        throw new FileServerConfigException();

      if (System.IO.Directory.Exists(rootPath) == false)
        throw new FileServerNotFoundException(rootPath);

      initFolderPath(dbName);
    }

    private void initFolderPath(string dbName)
    {
      databasePath = System.IO.Path.Combine(rootPath, dbName + "\\");
      checkAndCreateFolder(databasePath);
    }

    #region
    public string DatabasePath
    {
      get
      {
        return checkAndCreateFolder(databasePath);
      }
    }

    public string DatabasePath_LogFileFolder
    {
      get
      {
        return checkAndCreateFolder(System.IO.Path.Combine(databasePath, "LogFile"));
      }
    }

    public string DatabasePath_LogFile
    {
      get
      {
        return System.IO.Path.Combine(DatabasePath_LogFileFolder, "dblog.txt");
      }
    }

    public string ReportPath
    {
      get
      {
        if (string.IsNullOrEmpty(reportPath))
          reportPath = System.IO.Path.Combine(databasePath, "Report\\");
        return checkAndCreateFolder(reportPath);
      }
    }
    public string CustomReportPath
    {
      get
      {
        if (string.IsNullOrEmpty(customReportPath))
          customReportPath = System.IO.Path.Combine(databasePath, "CustomReport\\");
        return checkAndCreateFolder(customReportPath);
      }
    }
    public string DataPath
    {
      get
      {
        if (string.IsNullOrEmpty(dataPath))
          dataPath = System.IO.Path.Combine(databasePath, "Data\\");
        return checkAndCreateFolder(dataPath);
      }
    }
    public string LogFilePath
    {
      get
      {
        if (string.IsNullOrEmpty(logFilePath))
          logFilePath = System.IO.Path.Combine(databasePath, "LogFile\\");
        return checkAndCreateFolder(logFilePath);
      }
    }

    #endregion

    #region
    public string MyPath
    {
      get
      {
        if (string.IsNullOrEmpty(userPath))
          userPath = System.IO.Path.Combine(databasePath, userKey + "\\");
        return checkAndCreateFolder(userPath);
      }
    }

    public string MyUploadFilePath
    {
      get
      {
        return checkAndCreateFolder(System.IO.Path.Combine(MyPath, "UploadFiles\\"));
      }
    }

    //[Obsolete("the log file in the folder moved into db folder/logfile")]
    public string MyUploadLogFilePath()
    {
      return checkAndCreateFolder(System.IO.Path.Combine(MyPath, "UploadLogs\\"));
    }
    public string MyExportPath
    {
      get
      {
        return checkAndCreateFolder(System.IO.Path.Combine(MyPath, "Export\\"));
      }
    }
    public string MySettingFolderPath
    {
      get
      {
        return checkAndCreateFolder(System.IO.Path.Combine(MyPath, "UserSetting\\"));
      }
    }
    public string MyErrorLogFilePath
    {
      get
      {
        return checkAndCreateFolder(System.IO.Path.Combine(MyPath, "ErrorLog\\"));
      }
    }
    public string MyLogFilePath
    {
      get
      {
        return checkAndCreateFolder(System.IO.Path.Combine(MyPath, "LogFile\\"));
      }
    }
    public string MyTempPath
    {
      get
      {
        return checkAndCreateFolder(System.IO.Path.Combine(MyPath, "Temp\\"));
      }
    }
    public string MyBondSwapPath
    {
      get
      {
        return checkAndCreateFolder(System.IO.Path.Combine(MyPath, "Bond\\"));
      }
    }

    public string MyFileLayoutUploadPath
    {
      get
      {
        return checkAndCreateFolder(System.IO.Path.Combine(MyPath, "FileLayoutUploadFiles\\"));
      }
    }
    #endregion

    public string GetResultReportFolder(int resultkey)
    {
      return checkAndCreateFolder(System.IO.Path.Combine(ReportPath, resultkey.ToString()));
    }

    public string CreateFolder(string folderpath)
    {
      return checkAndCreateFolder(folderpath);
    }

    private string checkAndCreateFolder(string folderPath)
    {
      if (System.IO.Directory.Exists(folderPath) == false)
        System.IO.Directory.CreateDirectory(folderPath);
      return folderPath;
    }


    public string GetResultCustomReportFolder(int resultkey)
    {
      return checkAndCreateFolder(System.IO.Path.Combine(CustomReportPath, resultkey.ToString()));
    }


    public string GetCustomReportJobLogFolder()
    {
      return checkAndCreateFolder(System.IO.Path.Combine(CustomReportPath, "JobLog"));
    }
  }
}
