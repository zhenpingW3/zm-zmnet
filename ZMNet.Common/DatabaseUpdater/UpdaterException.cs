using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMNet.DatabaseUpdater
{
  internal class UpdaterException : Exception
  {
    public UpdaterException(string message) : base(message)
    {

    }
  }
}
