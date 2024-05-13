using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMNet.FileServer
{
  public class ClsFileServerFactory
  {
    public static IFileServer GetObject(int userKey, string dbFolderName)
    {
      return new ClsUserFilerServerManager(userKey, dbFolderName);
    }
  }
}
