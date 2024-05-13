using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMNet.Common
{
  public class EmailingConfig
  {
    public emailMode EmailMode { get; set; } = emailMode.Smtp;
    public string SES_Sender { get; set; }
    public string SES_Region { get; set; }


    public static EmailingConfig LoadFromWebConfig()
    {
      try
      {
        EmailingConfig config = new EmailingConfig()
        {
          SES_Sender = System.Web.Configuration.WebConfigurationManager.AppSettings["SES_Sender"],
          SES_Region = System.Web.Configuration.WebConfigurationManager.AppSettings["SES_Region"],
          EmailMode = GetMode(System.Web.Configuration.WebConfigurationManager.AppSettings["EmailMode"])
        };

        //if no EmailMode in web.config, return default setting
        if (string.IsNullOrEmpty(System.Web.Configuration.WebConfigurationManager.AppSettings["EmailMode"]))
          config.LoadDefaultEmptySetting();

        return config;
      }
      catch (Exception exp)
      {
        throw new Exception("failed to load ses settings from config file." + exp.Message, exp);
      }
    }

    private EmailingConfig LoadDefaultEmptySetting()
    {
      return new EmailingConfig
      {
        EmailMode = emailMode.Smtp,
        SES_Sender = "",
        SES_Region = ""
      };
    }

    private static emailMode GetMode(string emailModeStr)
    {
      emailMode mode = emailMode.Smtp;
      try
      {
        if (string.IsNullOrEmpty(emailModeStr) == false)
        {
          if (!string.IsNullOrEmpty(emailModeStr) && emailModeStr.Equals("ses", StringComparison.CurrentCultureIgnoreCase))
          {
            mode = emailMode.SES;
          }
        }
      }
      catch (Exception exp)
      {
        throw new Exception("failed to parse the email model setting.", exp);
      }
      return mode;
    }
  }
}
