using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMNet.DatabaseUpdater
{
  public class SignalRClient
  {
    public SignalRClient(string clientID, string username)
    {
      ClientID = clientID;
      Username = username;
    }

    public string ClientID { get; set; }
    public string Username { get; set; }
  }
}
