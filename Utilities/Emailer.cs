using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Mail;
using System.Web.Configuration;

namespace Utilities
{
    public class Emailer
    {
        #region Private Members
        SmtpClient  smtp    = new SmtpClient();
        MailMessage message = new MailMessage();
        #endregion
        
        #region Private Methods
        private void WriteAddressToDebug()
        {
            Trace.WriteLine("Sending email(s) to:");
                    
            Trace.WriteLine("TO:");
            Trace.WriteLine(String.Format("  -- has {0} addresses", TOList.Count.ToString()));
            TOList.ForEach(x => Trace.WriteLine(string.Format("    -- {0} ", x.ToString())));
                    
            Trace.WriteLine("CC:");
            Trace.WriteLine(String.Format("  -- has {0} addresses", CCList.Count.ToString()));
            CCList.ForEach(x => Trace.WriteLine(string.Format("    -- {0}", x.ToString())));
                    
            Trace.WriteLine("Bcc:");
            Trace.WriteLine(String.Format("  -- has {0} addresses", BCList.Count.ToString()));
            BCList.ForEach(x => Trace.WriteLine(string.Format("    -- {0}", x.ToString())));
                    
            Trace.WriteLine("... ... ...");
            Trace.WriteLine(message.ToString());
            Trace.WriteLine("... ... ...");
        }
        #endregion

        #region Public Members
        #region Message Settings
        public string subject      { get; set; }
        public string body         { get; set; }
        public List<string> TOList { get; set; }
        public List<string> CCList { get; set; }
        public List<string> BCList { get; set; }
        #endregion
        #endregion

        #region Public Methods
        /// <summary>
        /// Sends and email to a single supplied address
        /// </summary>
        /// <param name="emailAddress">the email address to send to</param>
        /// <returns></returns>
        public bool SendEmail(string emailAddress)
        {
            this.TOList.Add(emailAddress);
            return SendEmail();
        }
        
        /// <summary>
        /// Sends an email to the addresses supplied in the TO, CC and BCC lists
        /// </summary>
        /// <returns>true if the message was sent successfully</returns>
        public bool SendEmail()
        {
            // Setup Email Client here
            if ((TOList.Count > 0) || (CCList.Count > 0) || (BCList.Count > 0))
            {
                message.Subject = this.subject;
                message.Body    = this.body;
                message.From = new MailAddress("clockingsystem@nvestholdings.co.za");
 
                // Add addresses
                TOList.ForEach(x => message.To.Add(x));
                CCList.ForEach(x => message.CC.Add(x));
                BCList.ForEach(x => message.Bcc.Add(x));
                
                // Try sending the actual mail....
                try
                {
                    /*

                    #region Debug info
                    WriteAddressToDebug();
                    #endregion
                    #if DEBUG
                        Trace.WriteLine("Debug on. Not sending emails");
                    #else
                        Trace.WriteLine("Debug off. SENDING EMAILS!!!!!");
                        smtp.Send(message);
                    #endif         */

                    Trace.WriteLine("Debug off. SENDING EMAILS!!!!!");
                    message.IsBodyHtml = true;


                 if (Convert.ToBoolean(WebConfigurationManager.AppSettings["RolledOut"]))
                 {
                    smtp.Send(message);
                 }
                return true;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.ToString());
                    throw;
                    //TODO: Need to check if the sending fails here. HAd a case where the email address doesn't exist, but C# throws an exception an exception on that case. We need to log it and re-throwing is not required.
                }
            }
            return false;
        }
        #endregion

        #region Constructor
        public Emailer()
        {
            TOList = new List<string>();
            CCList = new List<string>();
            BCList = new List<string>();
        }
        #endregion
    }
}
