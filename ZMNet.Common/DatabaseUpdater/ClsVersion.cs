using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMNet.DatabaseUpdater
{
  internal class ClsVersion
  {

    // Set these values for each build and revision.
    //      - Build numbers match the month and day of the build, MMDD
    //      - Revision number is incremented for each build.

    #region Build-O-Matic
    // The build script (Build-O-Matic) is looking for the following lines so it can update the assembly version
    private const string BuildVersion = "1.4";
    private const string BuildID = "0";
    private const string BuildNumber = "0";
    #endregion

    public const string Title = "ZMFS.DatabaseUpdater Version " + BuildVersion + " Build " + BuildID;
    public const string Description = "ZMFS.DatabaseUpdater";
    public const string Configuration = "";
    public const string Company = "Moody's Analytics.";
    public const string Product = "ZMFS.DatabaseUpdater";
    public const string Copyright = "2003-2024 Moody's Analytics. All Rights Reserved.";
    public const string Trademark = "";
    public const string Culture = "";
    public const string Version = BuildVersion + "." + BuildID + "." + BuildNumber;
    public const bool DelaySign = false;
    public const string KeyFile = "";
    public const string KeyName = "";

  }
}
