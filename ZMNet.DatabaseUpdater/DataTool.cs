using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMNet.DatabaseUpdater
{
  public class DataTool
  {
    public static string GetDatabaseName(string connStr, bool withServer = false)
    {
      if (string.IsNullOrEmpty(connStr))
      {
        return null;
      }

      using (SqlConnection conn = new SqlConnection(connStr))
      {
        string database = conn.Database.ToLower();

        if (withServer)
        {
          string dataSource = conn.DataSource.ToLower();

          switch (dataSource)
          {
            case "127.0.0.1":
            case "(local)":
            case "localhost":
            case ".":
              dataSource = "localhost";
              break;
          }

          return string.Format("{0}@{1}", database, dataSource);
        }
        else
        {
          return database;
        }
      }
    }

    public static Dictionary<string, int> GetEnumDictionary(Type enumType, bool lowerKey = false)
    {
      var enumNames = Enum.GetNames(enumType).Cast<string>().ToArray();

      if (lowerKey)
      {
        enumNames = enumNames.Select(d => d.ToLower()).ToArray();
      }

      var enumValues = Enum.GetValues(enumType).Cast<int>().ToArray();

      Dictionary<string, int> enumDictionary = new Dictionary<string, int>();

      for (int i = 0; i < enumNames.Length; i++)
      {
        enumDictionary.Add(enumNames[i], enumValues[i]);
      }

      return enumDictionary;
    }
  }
}
