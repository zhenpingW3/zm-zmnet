using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMNet.FileServer
{
  public interface IManageUserFile
  {
    /// <summary>
    /// FS:\{DbFolder}\{UserKey}
    /// </summary>
    string MyPath { get; }

    /// <summary>
    /// FS:\{DbFolder}\{UserKey}\UploadFiles
    /// </summary>
    string MyUploadFilePath { get; }

    /// <summary>
    /// FS:\{DbFolder}\{UserKey}\Export
    /// </summary>
    string MyExportPath { get; }

    /// <summary>
    /// FS:\{DbFolder}\{UserKey}\UserSetting
    /// </summary>
    string MySettingFolderPath { get; }

    /// <summary>
    /// FS:\{DbFolder}\{UserKey}\ErrorLog
    /// </summary>
    string MyErrorLogFilePath { get; }

    /// <summary>
    /// FS:\{DbFolder}\{UserKey}\LogFile
    /// </summary>
    string MyLogFilePath { get; }

    /// <summary>
    /// FS:\{DbFolder}\{UserKey}\Temp
    /// </summary>
    string MyTempPath { get; }

    /// <summary>
    /// FS:\{DbFolder}\{UserKey}\Bond
    /// </summary>
    string MyBondSwapPath { get; }

    /// <summary>
    /// FS:\{DbFolder}\{UserKey}\FileLayoutUploadFiles
    /// </summary>
    string MyFileLayoutUploadPath { get; }
  }
}
