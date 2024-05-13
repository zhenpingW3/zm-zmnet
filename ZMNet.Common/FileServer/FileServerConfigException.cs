using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMNet.FileServer
{
  public class FileServerConfigException : Exception
  {
    public FileServerConfigException()
      : base(StringResource.FileServer_ConfigError)
    {

    }
  }
}
