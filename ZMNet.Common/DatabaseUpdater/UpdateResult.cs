using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMNet.DatabaseUpdater
{
  public class UpdateResult
  {
    public UpdateResult(string message = "", bool success = true)
    {
      Message = message;
      Success = success;
    }

    public bool Success { get; set; }
    public string Message { get; set; }
  }
}
