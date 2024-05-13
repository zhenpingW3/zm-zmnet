using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMNet.Common
{
  public class ClsAmazonEmail
  {
    private string ses_region;
    private string ses_sender;

    public ClsAmazonEmail(string ses_region, string ses_sender)
    {
      this.ses_region = ses_region;
      this.ses_sender = ses_sender;
    }

    private RegionEndpoint GetEndPoint(string ses_region)
    {

      bool matched = false;
      RegionEndpoint matchEndpoint = RegionEndpoint.USEast1;
      try
      {
        if (!string.IsNullOrEmpty(ses_region))
        {
          matched = true;
          matchEndpoint = RegionEndpoint.GetBySystemName(ses_region);
        }
      }
      catch (Exception exp)
      {
        throw new Exception("failed to parse endpoint via RegionEndpoint.GetBySystemName by " + ses_region + "." + exp.Message, exp);
      }
      if (matched)
        return matchEndpoint;
      else
        return Amazon.RegionEndpoint.USEast1;

    }

    public void Send(string receiver, string subject, string htmlBody)
    {
      Send(new List<string>() { receiver }, null, null, subject, htmlBody);
    }

    public void Send(List<string> receivers, List<string> cc, List<string> bcc, string subject, string htmlBody)
    {
      RegionEndpoint sesEndPoint = GetEndPoint(ses_region);

      using (var client = new AmazonSimpleEmailServiceClient(sesEndPoint))
      {
        var sendRequest = new SendEmailRequest()
        {
          Source = ses_sender,
          Destination = new Destination
          {
            ToAddresses = receivers,
            CcAddresses = cc,
            BccAddresses = bcc
          },
          Message = new Message
          {
            Subject = new Content(subject),
            Body = new Body
            {
              Html = new Content
              {
                Charset = "UTF-8",
                Data = htmlBody
              }
            }
          }
        };

        try
        {
          var response = client.SendEmail(sendRequest);
        }
        catch (Exception exp)
        {
          throw new Exception("Failed to send email via ses." + exp.Message, exp);
        }
      }
    }

    public void SendRaw(List<string> receivers, List<string> cc, List<string> bcc, string subject, string htmlBody, List<string> zipAttachments)
    {
      RegionEndpoint sesEndPoint = GetEndPoint(ses_region);

      //Client SES instantiated
      using (var client = new AmazonSimpleEmailServiceClient(sesEndPoint))
      {
        var mimeMessage = new MimeMessage();

        //Add sender e-mail address
        //Note: this e-mail address must to be allowed and checked by AWS SES
        mimeMessage.From.Add(MailboxAddress.Parse(ses_sender));

        //Add  e-mail address destiny

        if (receivers != null)
        {
          foreach (var tmpReceiver in receivers)
          {
            mimeMessage.To.Add(MailboxAddress.Parse(tmpReceiver));
          }
        }

        //subject
        mimeMessage.Subject = subject ?? "(empty subject)";


        var bodyBuilder = new BodyBuilder();

        //htmlBody = "send email via aws ses.\r\n" + htmlBody;

        bodyBuilder.HtmlBody = htmlBody;

        //You must to inform the mime-type of the attachment and his name
        if (zipAttachments != null && zipAttachments.Count > 0)
        {
          foreach (var fileItem in zipAttachments)
          {
            //Getting attachment stream
            if (System.IO.File.Exists(fileItem) == false) continue;

            FileInfo fi = new FileInfo(fileItem);

            var fileBytes = File.ReadAllBytes(fileItem);
            bodyBuilder.Attachments.Add(fi.Name, fileBytes, new ContentType("application", "zip"));

          }
        }

        mimeMessage.Body = bodyBuilder.ToMessageBody();

        //Map MimeMessage to MemoryStream, that is what SenRawEmailRequest accepts
        var rawMessage = new MemoryStream();
        mimeMessage.WriteTo(rawMessage);

        client.SendRawEmail(new SendRawEmailRequest(new RawMessage(rawMessage)));
      }
    }
  }
}
