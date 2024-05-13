using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMNet.FileServer
{
  public class FileServerNotFoundException : Exception
  {
    public FileServerNotFoundException(string path)
      : base(string.Format(StringResource.FileServer_NotFound_Format, path))
    {

    }
  }
}
