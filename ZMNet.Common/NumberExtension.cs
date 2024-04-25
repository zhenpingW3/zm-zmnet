using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMNet.Common
{
  public static class NumberExtension
  {
    public static string Format(this double number, int decimalPlace, bool thousandthPlace = true)
    {
      string format = decimalPlace.GetNumberFormat(thousandthPlace);

      return string.Format(format, number);
    }

    public static string GetNumberFormat(this int decimalPlace, bool thousandthPlace = true)
    {
      return "{0:" + (thousandthPlace ? "N" : "F") + decimalPlace + "}";
    }
  }
}
