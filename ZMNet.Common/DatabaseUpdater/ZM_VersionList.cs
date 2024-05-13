using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMNet.DatabaseUpdater
{
  internal class ZM_VersionList
  {
    internal ZM_VersionList(int key, string ddlVersion)
    {
      Key = key;
      //ID = ddlVersion;
      VersionCode = ddlVersion;
      //AsOfDate = DateTime.Now;
    }

    internal int Key { get; set; }
    //internal string ID { get; set; }
    //internal DateTime AsOfDate { get; set; }
    internal string VersionCode { get; set; }

    //internal string Description
    //{
    //  get
    //  {
    //    return string.Format("Updated by OALM");
    //  }
    //}

    //internal string SQL { get; set; }

  }
}
