using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMNet.Common
{
  public static class TypeExtension
  {
    public static bool IsaString(this Type type)
    {
      return type.FullName == "System.String";
    }
  }
}
