using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMNet.FileServer
{
  public interface IFileServer : IManageDatabaseFile, IManageUserFile
  {
    /// <summary>
    /// Get and create the file path of the standard report
    /// </summary>
    /// <param name="resultkey"></param>
    /// <returns></returns>
    string GetResultReportFolder(int resultkey);

    /// <summary>
    /// Get and create the file path of the custom report
    /// </summary>
    /// <param name="resultkey"></param>
    /// <returns></returns>
    string GetResultCustomReportFolder(int resultkey);

    /// <summary>
    /// Get and create log paths for custom reports
    /// </summary>
    /// <returns></returns>
    string GetCustomReportJobLogFolder();

    /// <summary>
    /// check and create folder
    /// </summary>
    /// <param name="folderpath"></param>
    /// <returns></returns>
    string CreateFolder(string folderpath);
  }
}
