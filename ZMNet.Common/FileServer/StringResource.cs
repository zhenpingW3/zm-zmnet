using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMNet.FileServer
{
  public class StringResource
  {
    public static readonly string FileServer_NotFound_Format = "Cannot find the file server path: {0}";
    public static readonly string FileServer_ConfigError = "FileServer in web.config is empty, please set the folder path at first.";
  }
}
