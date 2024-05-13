using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMNet.DatabaseUpdater
{
  public class VersionList
  {
    public int Key { get; set; }
    public string ID { get; set; } = "SQLBase";
    public DateTime AsOfDate { get; set; } = new DateTime();
    public string DLLVersionCode { get; set; } = "DLLBase";
    public string Description { get; set; }
  }
}
