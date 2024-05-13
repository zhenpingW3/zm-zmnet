using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMNet.DatabaseUpdater
{
  public enum DatabaseType
  {
    User,
    Admin,
    NetAdmin,

    Assumption = 83,
    Market = 86,
    Security = 72,
    Historical = 84
  }
}
