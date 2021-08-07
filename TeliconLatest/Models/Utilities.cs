using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Xml;
using System.Xml.Serialization;

namespace TeliconLatest.Models
{
    public class Utilities
    {
        public static string CommonErrorMessage = "Error encountered. Please try again. If you continue to experience this error please contact us. Thank you.";
        public static string CommonSuccessMessage = "Success ! ";
        public IConfiguration _configuration;
        #region Ctor
        public Utilities(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        #endregion

        /// <summary>
        /// Email send takes 3 parameters
        /// </summary>
        /// <param name="strFrom"></param>
        /// <param name="strTo"></param>
        /// <param name="strBody"></param>
        public void SendMail(string strFrom, string strTo, string strBody, string subject)
        {
            try
            {
                MailSettings mailSettings = new();
                _configuration.Bind("mailSettings", mailSettings);
                // Create instance of mail message class.
                MailMessage objMailMessage = new MailMessage
                {
                    From = new MailAddress(strFrom, "Telicon")
                };
                objMailMessage.To.Add(strTo);
                objMailMessage.Subject = subject;
                objMailMessage.Body = strBody;

                //Set the mail format to html by default.
                objMailMessage.IsBodyHtml = true;
                NetworkCredential networkCredential = new NetworkCredential(mailSettings.smtp.network.userName, mailSettings.smtp.network.password);
                //Send mail.
                SmtpClient smtp = new SmtpClient(mailSettings.smtp.network.host, mailSettings.smtp.network.port)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = networkCredential,
                };
                smtp.Send(objMailMessage);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Select email template from xml path
        /// </summary>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public static string GetEmailTemplateValue(string xpath)
        {
            string strValue;
            try
            {
                var strPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Templates", "Template.xml");
                XmlDocument doc = new XmlDocument();
                doc.Load(strPath);
                XmlElement root = doc.DocumentElement;
                XmlNode node = doc.DocumentElement.SelectSingleNode(xpath);
                XmlNode childNode = node.ChildNodes[0];
                if (childNode is XmlCDataSection)
                {
                    XmlCDataSection cdataSection = childNode as XmlCDataSection;
                    strValue = cdataSection.Value.ToString();
                }
                else
                {
                    strValue = childNode.Value.ToString();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return strValue;
        }
        public static ProfileInfo GetProfileInfoValue(string xpath)
        {
            try
            {
                using StringReader stringReader = new StringReader(xpath);
                XmlSerializer serializer = new XmlSerializer(typeof(ProfileInfo));
                return serializer.Deserialize(stringReader) as ProfileInfo;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    public class Network
    {
        public string host { get; set; }
        public int port { get; set; }
        public string userName { get; set; }
        public string password { get; set; }
    }

    public class Smtp
    {
        public string deliveryMethod { get; set; }
        public Network network { get; set; }
    }

    public class MailSettings
    {
        public Smtp smtp { get; set; }
    }
}