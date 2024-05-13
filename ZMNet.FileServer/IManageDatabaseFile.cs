using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMNet.FileServer
{
  public interface IManageDatabaseFile
  {
    /// <summary>
    /// FS:\{DbName}
    /// </summary>
    string DatabasePath { get; }

    /// <summary>
    /// FS:\{DbName}\Report
    /// </summary>
    string ReportPath { get; }

    /// <summary>
    /// FS:\{DbName}\CustomReport
    /// </summary>
    string CustomReportPath { get; }

    /// <summary>
    /// FS:\{DbName}\Data
    /// </summary>
    string DataPath { get; }

    /// <summary>
    /// FS:\{DbName}\LogFile
    /// </summary>
    string LogFilePath { get; }
  }
}
