//using WorkAttend.Shared.DataServices;
//using WorkAttend.Shared.Enums;
//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Net.Mail;
//using System.Text;
//using System.Threading.Tasks;
//using Google.Apis.Auth.OAuth2;
//using System.Threading;
//using Google.Apis.Auth;

//namespace WorkAttend.Shared.Helpers
//{
//    public class EmailGeneratorHelper
//    {
//        #region Singleton
//        private static EmailGeneratorHelper _instance;
//        public static EmailGeneratorHelper Instance
//        {
//            get
//            {
//                if (_instance == null)
//                {
//                    return _instance = new EmailGeneratorHelper();
//                }
//                return _instance;
//            }
//        }
//        private EmailGeneratorHelper()
//        {
//            LoadEmailTemplate();
//        }

//        #endregion


//        //string InstanceName = "WorkAttendEmailSendingProcess";
//        List<EmailTemplate> definedTemplates = new List<EmailTemplate>();
//        string emailTemplatorURL = GlobalAppConfigs.EmailTemplatorURL;
//        //ConfigurationManager.AppSettings["EmailTemplatorURL"].ToString();
//        private void LoadEmailTemplate()
//        {
//            using (WorkAttendRepository repository = DataContextHelper.GetPOSPortalContext())
//            {
//                PetaPoco.Sql pSql = PetaPoco.Sql.Builder.Select("*").From("EmailTemplates");
//                definedTemplates = repository.Fetch<EmailTemplate>(pSql);
//            }
//        }

//        public void SendEmails()
//        {
//            if (definedTemplates == null)
//            {
//                LoadEmailTemplate();
//            }
//            ELHelper elh = new ELHelper();
//            string templateURL = string.Empty;
//            using (WorkAttendRepository repository = DataContextHelper.GetPOSPortalContext())//elh -> send this for logging
//            {
//                try
//                {
//                    PetaPoco.Sql pSql = PetaPoco.Sql.Builder.Select(" * ")
//                        .From("EmailTokens")
//                        .Where("EmailStatus NOT IN (@0, @1,@2) ", 'S', 'I', 'D');

//                    // TODO: What about email tokens that are continously processed but are not getting sent ?

//                    List<EmailToken> emTokens = repository.Fetch<EmailToken>(pSql);

//                    foreach (var emToken in emTokens)
//                    {
//                        try
//                        {
//                            StringBuilder requestUrlBuilder = new StringBuilder();
//                            EmailTemplate emTemplate = definedTemplates.FirstOrDefault(s => s.EmailTemplateID == emToken.EmailTemplateID);

//                            if (emTemplate == null)
//                            {
//                                emToken.EmailStatus = EmailStatusEnum.X.ToString();
//                                emToken.EmailSendingFailedReason = "No corresponding Email Template found !";
//                                repository.Update(emToken);
//                                continue;
//                            }

//                            requestUrlBuilder.Append(emailTemplatorURL);
//                            requestUrlBuilder.Append(emTemplate.EmailTemplateURL);
//                            if (emToken.EmailTemplateID == (int)EmailTemplateEnums.SendReceipt)
//                            {
//                                requestUrlBuilder.Append("?id=" + emToken.EmailTokenID);
//                                requestUrlBuilder.Append("&dbname=" + emToken.StoreURL);
//                            }
//                            else if (emToken.EmailTemplateID == (int)EmailTemplateEnums.ContactUs)
//                            {
//                                requestUrlBuilder.Append("?id=" + emToken.RowID);
//                            }
//                            else if (emToken.EmailTemplateID == (int)EmailTemplateEnums.PaymentSubscription)
//                            {
//                                requestUrlBuilder.Append("?id=" + emToken.RowID);
//                            }
//                            else
//                            {
//                                requestUrlBuilder.Append(emToken.EmailTokenID);
//                            }

//                            elh.AddVariable(() => requestUrlBuilder);
//                            templateURL = requestUrlBuilder.ToString();
//                            string responseHTML = GetEmailBodyHTML(templateURL);

//                            if (responseHTML.IndexOf("###DONT_SEND_IT###") == -1)
//                            {

//                                if (ConfigurationManager.AppSettings["SendEmailAsync"] == "0")
//                                {
//                                    SendAnEmail(emToken, emTemplate.EmailSubject, responseHTML, repository);
//                                }
//                                else
//                                {
//                                    SendAnEmailAsync(emToken, emTemplate.EmailSubject, responseHTML, repository);
//                                }
//                            }
//                            else
//                            {
//                                emToken.EmailSendingFailedReason = responseHTML.Substring(responseHTML.IndexOf("[R]"), responseHTML.IndexOf("[/R]"));
//                                repository.Update(emToken);
//                            }
//                        }
//                        catch (Exception ex)
//                        {
//                            string customMsg = string.Format("EmailTokenId: {0} - URL: {1} ", emToken.EmailTokenID, templateURL);
//                            elh.LogException(ex, customMsg);

//                            string msg = string.Format("{0} | {1} | Msg: {2} {3}", "EmailSendingOrTemplatingProcess", "Fatal", "Recruitment Email Service Failed.", ex != null ? "| Stack: " + ex.StackTrace + " | Inner Message: " + ex.InnerException : string.Empty);
//                            //    using (EventLog eventLog = new EventLog("Application"))
//                            //    {
//                            //        eventLog.Source = "Email Sending Failed.";
//                            //        eventLog.WriteEntry(msg, EventLogEntryType.Error, 1002, 1);
//                            //    }
//                        }
//                    }
//                }
//                catch (Exception ex)
//                {
//                    string msg = string.Format("{0} | {1} | Msg: {2} {3}", "EmailSending", "Fatal", "Recruitment Email Service Failed.", ex != null ? "| Stack: " + ex.StackTrace + " | Inner Message: " + ex.InnerException : string.Empty);
//                    string customMsg = string.Format("Database connection prob may be - URL: {0} ", templateURL);
//                    elh.LogException(ex, customMsg);
//                    //NLogging.Log(null, false, LogLevel.Info, msg);

//                    //using (EventLog eventLog = new EventLog("Application"))
//                    //{
//                    //    eventLog.Source = "Email Sending Failed.";
//                    //    eventLog.WriteEntry(msg, EventLogEntryType.Error, 1002, 1);
//                    //}
//                }
//            }
//        }
//        public EmailToken GetEmailToken(EmailToken emToken)
//        {
//            using (WorkAttendRepository context = DataContextHelper.GetPOSPortalContext()) //elh -> send this for logging
//            {
//                emToken.FromEmailAddress = GlobalAppConfigs.FromEmail;
//                emToken.FromName = GlobalAppConfigs.FromNameEmail;
//                emToken.EnqueuedOn = DateTime.Now;
//                emToken.EmailStatus = "P";
//                emToken.EmailTokenID = int.Parse(context.Insert(emToken).ToString());
//                return emToken;
//            }
//        }
//        public void SendEmail(EmailToken emToken)
//        {

//            ELHelper elh = new ELHelper();
//            string templateURL = string.Empty;
//            using (WorkAttendRepository repository = DataContextHelper.GetPOSPortalContext()) //elh -> send this for logging
//            {
//                try
//                {
//                    StringBuilder requestUrlBuilder = new StringBuilder();
//                    EmailTemplate emTemplate = definedTemplates.FirstOrDefault(s => s.EmailTemplateID == emToken.EmailTemplateID);

//                    if (emTemplate == null)
//                    {
//                        emToken.EmailStatus = EmailStatusEnum.X.ToString();
//                        emToken.EmailSendingFailedReason = "No corresponding Email Template found !";
//                        repository.Update(emToken);
//                    }
//                    else
//                    {
//                        requestUrlBuilder.Append(emailTemplatorURL);
//                        requestUrlBuilder.Append(emTemplate.EmailTemplateURL);
//                        if (emToken.EmailTemplateID == (int)EmailTemplateEnums.SendReceipt)
//                        {
//                            requestUrlBuilder.Append("?id=" + emToken.EmailTokenID);
//                            requestUrlBuilder.Append("&dbname=" + emToken.StoreURL);
//                        }
//                        else if (emToken.EmailTemplateID == (int)EmailTemplateEnums.ContactUs)
//                        {
//                            requestUrlBuilder.Append("?id=" + emToken.RowID);
//                        }
//                        else if (emToken.EmailTemplateID == (int)EmailTemplateEnums.PaymentSubscription)
//                        {
//                            requestUrlBuilder.Append("?id=" + emToken.RowID);
//                        }
//                        else
//                        {
//                            requestUrlBuilder.Append(emToken.EmailTokenID);
//                        }

//                        elh.AddVariable(() => requestUrlBuilder);
//                        templateURL = requestUrlBuilder.ToString();

//                        AppConfig app = new AppConfig()
//                        {
//                            AppConfigKey = "TITU-TEST",
//                            Description = templateURL,
//                            AppConfigValue = templateURL,
//                        };
//                        // repository.Insert(app);

//                        string responseHTML = GetEmailBodyHTML(templateURL);

//                        if (responseHTML.IndexOf("###DONT_SEND_IT###") == -1)
//                        {
//                            if (ConfigurationManager.AppSettings["SendEmailAsync"] == "0")
//                            {
//                                SendAnEmail(emToken, emTemplate.EmailSubject, responseHTML, repository);
//                            }
//                            else
//                            {
//                                SendAnEmailAsync(emToken, emTemplate.EmailSubject, responseHTML, repository);
//                            }
//                        }
//                        else
//                        {
//                            emToken.EmailSendingFailedReason = responseHTML.Substring(responseHTML.IndexOf("[R]"), responseHTML.IndexOf("[/R]"));
//                            repository.Update(emToken);
//                        }
//                    }
//                }
//                catch (Exception ex)
//                {
//                    elh.LogException(ex);
//                }

//            }
//        }
//        protected void SendAnEmail(EmailToken emToken, string emailSubject, string emailBodyHTML, WorkAttendRepository repository)
//        {
//            ELHelper elh = new ELHelper();
//            MailMessage mm = null;
//            SmtpClient smtpClient = new SmtpClient();

//            try
//            {
//                System.Net.ServicePointManager.SecurityProtocol |=
//    SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
//                if (UsingExchangeServer)
//                {
//                    smtpClient.Credentials = CredentialCache.DefaultNetworkCredentials;
//                }
//                else
//                {
//                    var credentials = GetAccessToken(GlobalAppConfigs.UserName, GlobalAppConfigs.PassEmail);
//                    string email = GlobalAppConfigs.UserName;
//                    string pass = GlobalAppConfigs.PassEmail;
//                    if (credentials != null && credentials.Result != null && credentials.Result.Count > 0)
//                    {
//                        email = credentials.Result[0];
//                        pass = credentials.Result.Count > 1 ? credentials.Result[1] : pass;
//                    }
//                    smtpClient.UseDefaultCredentials = bool.Parse(GlobalAppConfigs.UseDefaultCredentials);
//                    NetworkCredential NetworkCred = new NetworkCredential(email, pass);
//                    smtpClient.Credentials = NetworkCred;
//                    smtpClient.EnableSsl = bool.Parse(GlobalAppConfigs.EnableSSL);
//                    smtpClient.Port = Convert.ToInt32(GlobalAppConfigs.PortEmail);
//                    smtpClient.Host = GlobalAppConfigs.HostEmail;
//                    //smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

//                }

//                //smtpClient.EnableSsl = EnableSSL;

//                mm = ComposeMailMessage(emToken, !string.IsNullOrEmpty(emailSubject) ? emailSubject : emToken.CustomSubject, emailBodyHTML);
//                //if (isSubscriptionRequest)
//                //{
//                //    string currenDirectory = BaseFolderPath;
//                //    string resourceFolderName = ResourceFolderPath;
//                //    string resourceDir = string.Format(@"{0}{1}", currenDirectory, resourceFolderName);
//                //    string[] files = Directory.GetFiles(resourceDir);
//                //    foreach (var file in files)
//                //    {
//                //        // Create  the file attachment for this e-mail message.
//                //        Attachment attachment = new Attachment(file, MediaTypeNames.Application.Octet);
//                //        // Add time stamp information for the file.
//                //        ContentDisposition disposition = attachment.ContentDisposition;
//                //        disposition.CreationDate = System.IO.File.GetCreationTime(file);
//                //        disposition.ModificationDate = System.IO.File.GetLastWriteTime(file);
//                //        disposition.ReadDate = System.IO.File.GetLastAccessTime(file);
//                //        // Add the file attachment to this e-mail message.
//                //        mm.Attachments.Add(attachment);
//                //    }
//                //    // mm.Attachments.Add(
//                //}



//                smtpClient.Send(mm);

//                emToken.EmailStatus = EmailStatusEnum.S.ToString();
//                emToken.ProcessCount += 1;
//                emToken.ProcessedOn = emToken.SentOn = DateTime.Now;

//                repository.Update(emToken);

//                //if (System.Diagnostics.Debugger.IsAttached)
//                //{
//                //    smtpClient.Send(mm);
//                //}
//                //else
//            }
//            catch (Exception ex)
//            {
//                emToken.EmailStatus = EmailStatusEnum.X.ToString();
//                emToken.EmailSendingFailedReason = ex.Message;
//                emToken.ProcessCount += 1;
//                emToken.ProcessedOn = DateTime.Now;

//                repository.Update(emToken);

//                elh.LogException(ex);

//                //string msg = string.Format("{0} | {1} | Msg: {2} {3}", "EmailSendingProcess", "Fatal", "Recruitment Email Sending Process Failed.", ex != null ? "| Stack: " + ex.StackTrace + " | Inner Message: " + ex.InnerException : string.Empty);

//                //using (EventLog eventLog = new EventLog("Application"))
//                //{
//                //    eventLog.Source = "Email Sending Failed.";
//                //    eventLog.WriteEntry(msg, EventLogEntryType.Error, 1002, 1);
//                //}
//            }
//        }

//        //protected void SendAnEmail(EmailToken emToken, string emailSubject, string emailBodyHTML, WorkAttendRepository repository)
//        //{
//        //    ELHelper elh = new ELHelper();
//        //    MailMessage mm = null;
//        //    SmtpClient smtpClient = new SmtpClient();

//        //    try
//        //    {
//        //        if (UsingExchangeServer)
//        //        {
//        //            smtpClient.Credentials = CredentialCache.DefaultNetworkCredentials;
//        //        }

//        //        smtpClient.EnableSsl = EnableSSL;

//        //        mm = ComposeMailMessage(emToken, !string.IsNullOrEmpty(emailSubject) ? emailSubject : emToken.CustomSubject, emailBodyHTML);
//        //        //if (isSubscriptionRequest)
//        //        //{
//        //        //    string currenDirectory = BaseFolderPath;
//        //        //    string resourceFolderName = ResourceFolderPath;
//        //        //    string resourceDir = string.Format(@"{0}{1}", currenDirectory, resourceFolderName);
//        //        //    string[] files = Directory.GetFiles(resourceDir);
//        //        //    foreach (var file in files)
//        //        //    {
//        //        //        // Create  the file attachment for this e-mail message.
//        //        //        Attachment attachment = new Attachment(file, MediaTypeNames.Application.Octet);
//        //        //        // Add time stamp information for the file.
//        //        //        ContentDisposition disposition = attachment.ContentDisposition;
//        //        //        disposition.CreationDate = System.IO.File.GetCreationTime(file);
//        //        //        disposition.ModificationDate = System.IO.File.GetLastWriteTime(file);
//        //        //        disposition.ReadDate = System.IO.File.GetLastAccessTime(file);
//        //        //        // Add the file attachment to this e-mail message.
//        //        //        mm.Attachments.Add(attachment);
//        //        //    }
//        //        //    // mm.Attachments.Add(
//        //        //}

//        //        smtpClient.Port = Convert.ToInt32(GlobalAppConfigs.PortEmail);
//        //        smtpClient.Host = GlobalAppConfigs.HostEmail;
//        //        smtpClient.EnableSsl = bool.Parse(GlobalAppConfigs.EnableSSL);
//        //        smtpClient.UseDefaultCredentials = bool.Parse(GlobalAppConfigs.UseDefaultCredentials);
//        //        smtpClient.Credentials = new NetworkCredential(GlobalAppConfigs.UserName, GlobalAppConfigs.PassEmail);
//        //        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
//        //        smtpClient.Send(mm);

//        //        emToken.EmailStatus = EmailStatusEnum.S.ToString();
//        //        emToken.ProcessCount += 1;
//        //        emToken.ProcessedOn = emToken.SentOn = DateTime.Now;

//        //        repository.Update(emToken);

//        //        //if (System.Diagnostics.Debugger.IsAttached)
//        //        //{
//        //        //    smtpClient.Send(mm);
//        //        //}
//        //        //else
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        emToken.EmailStatus = EmailStatusEnum.X.ToString();
//        //        emToken.EmailSendingFailedReason = ex.Message;
//        //        emToken.ProcessCount += 1;
//        //        emToken.ProcessedOn = DateTime.Now;

//        //        repository.Update(emToken);

//        //        elh.LogException(ex);

//        //        string msg = string.Format("{0} | {1} | Msg: {2} {3}", "EmailSendingProcess", "Fatal", "Recruitment Email Sending Process Failed.", ex != null ? "| Stack: " + ex.StackTrace + " | Inner Message: " + ex.InnerException : string.Empty);

//        //        //using (EventLog eventLog = new EventLog("Application"))
//        //        //{
//        //        //    eventLog.Source = "Email Sending Failed.";
//        //        //    eventLog.WriteEntry(msg, EventLogEntryType.Error, 1002, 1);
//        //        //}
//        //    }
//        //}

//        protected void SendAnEmailAsync(EmailToken emToken, string emailSubject, string emailBodyHTML, WorkAttendRepository repository)
//        {
//            ELHelper elh = new ELHelper();
//            MailMessage mm = null;
//            SmtpClient smtpClient = new SmtpClient();

//            try
//            {
//                if (UsingExchangeServer)
//                {
//                    smtpClient.Credentials = CredentialCache.DefaultNetworkCredentials;
//                }

//                smtpClient.EnableSsl = EnableSSL;

//                mm = ComposeMailMessage(emToken, emToken.CustomSubject ?? emailSubject, emailBodyHTML);
//                //if (isSubscriptionRequest)
//                //{
//                //    string currenDirectory = BaseFolderPath;
//                //    string resourceFolderName = ResourceFolderPath;
//                //    string resourceDir = string.Format(@"{0}{1}", currenDirectory, resourceFolderName);
//                //    string[] files = Directory.GetFiles(resourceDir);
//                //    foreach (var file in files)
//                //    {
//                //        // Create  the file attachment for this e-mail message.
//                //        Attachment attachment = new Attachment(file, MediaTypeNames.Application.Octet);
//                //        // Add time stamp information for the file.
//                //        ContentDisposition disposition = attachment.ContentDisposition;
//                //        disposition.CreationDate = System.IO.File.GetCreationTime(file);
//                //        disposition.ModificationDate = System.IO.File.GetLastWriteTime(file);
//                //        disposition.ReadDate = System.IO.File.GetLastAccessTime(file);
//                //        // Add the file attachment to this e-mail message.
//                //        mm.Attachments.Add(attachment);
//                //    }
//                //    // mm.Attachments.Add(
//                //}

//                var credentials = GetAccessToken(GlobalAppConfigs.UserName, GlobalAppConfigs.PassEmail);
//                string email = GlobalAppConfigs.UserName;
//                string pass = GlobalAppConfigs.PassEmail;
//                if (credentials != null && credentials.Result.Count > 0)
//                {
//                    email = credentials.Result[0];
//                    pass = credentials.Result.Count > 1 ? credentials.Result[1] : pass;
//                }
//                smtpClient.Port = Convert.ToInt32(GlobalAppConfigs.PortEmail);
//                smtpClient.Host = GlobalAppConfigs.HostEmail;
//                smtpClient.EnableSsl = bool.Parse(GlobalAppConfigs.EnableSSL);
//                smtpClient.UseDefaultCredentials = bool.Parse(GlobalAppConfigs.UseDefaultCredentials);
//                smtpClient.Credentials = new NetworkCredential(email, pass);
//                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

//                smtpClient.SendCompleted += new SendCompletedEventHandler(smtpClient_SendCompleted);

//                emToken.EmailStatus = EmailStatusEnum.I.ToString(); ; // In-Progress
//                repository.Update(emToken);

//                //if (System.Diagnostics.Debugger.IsAttached)
//                //{
//                //    smtpClient.Send(mm);
//                //}
//                //else
//                {
//                    smtpClient.SendAsync(mm, emToken);
//                }
//            }
//            catch (Exception ex)
//            {
//                elh.LogException(ex);
//            }
//        }

//        void smtpClient_SendCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
//        {
//            EmailToken emToken = (EmailToken)e.UserState;

//            if (e.Error != null)
//            {
//                emToken.EmailStatus = EmailStatusEnum.X.ToString(); ;
//                emToken.EmailSendingFailedReason = e.Error.Message;
//                emToken.ProcessCount += 1;
//                emToken.ProcessedOn = DateTime.Now;
//            }
//            else
//            {
//                emToken.EmailStatus = "S";
//                emToken.ProcessCount += 1;
//                emToken.ProcessedOn = emToken.SentOn = DateTime.Now;
//            }

//            using (WorkAttendRepository repository = DataContextHelper.GetPOSPortalContext())
//            {
//                repository.Update(emToken);
//            }
//        }

//        protected MailMessage ComposeMailMessage(EmailToken emToken, string emailSubject, string emailBodyHTML)
//        {
//            MailMessage mm = new MailMessage();
//            if (emToken.ToEmailAddress.Contains(";"))
//            {
//                foreach (string em in emToken.ToEmailAddress.Split(";".ToCharArray()).ToList())
//                {
//                    mm.To.Add(em);
//                }
//            }
//            else
//            {
//                mm.To.Add(emToken.ToEmailAddress);
//            }
//            mm.From = new MailAddress(emToken.FromEmailAddress, emToken.FromName);
//            mm.BodyEncoding = Encoding.UTF8;
//            mm.IsBodyHtml = true;
//            //mm.Priority.
//            mm.Subject = emailSubject;
//            mm.Body = emailBodyHTML;
//            if (!string.IsNullOrEmpty(emToken.CCAddress))
//            {
//                mm.CC.Add(emToken.CCAddress);
//            }
//            return (mm);
//        }

//        protected string GetEmailBodyHTML(string requestURL)
//        {
//            System.Net.HttpWebRequest request = (HttpWebRequest)System.Net.WebRequest.Create(requestURL);

//            // TODO: Log time taken to receive the response
//            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

//            StreamReader sr = new StreamReader(response.GetResponseStream());

//            string result = sr.ReadToEnd();

//            sr.Close();

//            return (result);
//        }

//        bool UsingExchangeServer
//        {
//            get
//            {
//                return (ConfigurationManager.AppSettings["USING_EXCHANGE_SERVER"] != null
//                    && ConfigurationManager.AppSettings["USING_EXCHANGE_SERVER"] == "true");
//            }
//        }

//        bool EnableSSL
//        {
//            get
//            {
//                return (ConfigurationManager.AppSettings["ENABLE_SSL"] != null
//                    && ConfigurationManager.AppSettings["ENABLE_SSL"] == "true");
//            }
//        }

//        private static string BaseFolderPath
//        {
//            get { return ConfigurationManager.AppSettings["BaseFolderPath"].ToString(); }
//        }
//        private static string ResourceFolderPath
//        {
//            get { return ConfigurationManager.AppSettings["ResourceFolderPath"].ToString(); }
//        }

//        private async Task<List<string>> GetAccessToken(string id, string secret)
//        {
//            List<string> token = null;
//            try
//            {
//                //if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(secret))
//                //{
//                //    var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
//                //                        new ClientSecrets
//                //                        {
//                //                            ClientId = id,
//                //                            ClientSecret = secret
//                //                        },
//                //                        new[] { "email", "profile", "https://mail.google.com/" },
//                //                        "user",
//                //                        CancellationToken.None
//                //                        );


//                //    var jwtPayload = GoogleJsonWebSignature.ValidateAsync(credential.Token.IdToken).Result;
//                //    if (jwtPayload != null)
//                //    {
//                //        string username = jwtPayload.Email;
//                //        string access_token = credential.Token.AccessToken;
//                //        token = new List<string>();
//                //        token.Add(username);
//                //        token.Add(access_token);
//                //    }
//                //}
//            }
//            catch (Exception ex)
//            {
//                throw;
//            }
//            return token;
//        }
//    }
//}
