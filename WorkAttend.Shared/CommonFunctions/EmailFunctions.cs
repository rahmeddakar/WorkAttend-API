//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Net.Mail;
//using System.Threading.Tasks;
//using WorkAttend.Shared.Enums;
//using WorkAttend.Shared.Extensions;
//using WorkAttend.Shared.Helpers;
//using System.Net;
//using WorkAttend.Shared.Helpers.HelperModel;
//using System.Text.RegularExpressions;

//namespace WorkAttend.Shared.CommonFunctions
//{
//    public class EmailFunctions
//    {
//        private async Task<bool> sendEmail(EmailModel em)
//        {
//            var emailResult = await Task.Run(() => {
//                try
//                {
//                    MailMessage message = new MailMessage();
//                    SmtpClient smtpClient = new SmtpClient();
//                    message.From = new MailAddress(em.fromEmailAddress);
//                    message.To.Add(new MailAddress(em.toEmailAddress));
//                    message.Subject = em.subject;
//                    message.IsBodyHtml = true;
//                    message.Body = em.msg;
//                    smtpClient.Port = Convert.ToInt32(GlobalVariables.port);
//                    smtpClient.Host = GlobalVariables.emailServerHostAddress;
//                    smtpClient.EnableSsl = true;
//                    smtpClient.UseDefaultCredentials = false;
//                    smtpClient.Credentials = new NetworkCredential(GlobalVariables.userName, GlobalVariables.password);
//                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
//                    smtpClient.Send(message);
//                    return true;
//                }
//                catch (Exception ex)
//                {
//                    return false;
//                }
//            });
//            return emailResult;

//        }
//        public async Task<bool> formatEmail(EmailModel em)
//        {
//            try
//            {
//                string _MessageViewHtml = String.Empty;
//                StringBuilder stringBuilder = new StringBuilder();
//                switch (em.emailType)
//                {
//                    case EmailEnums.FORGET_PASSSWORD_EMAIL:
//                        _MessageViewHtml = stringBuilder.TextToHTML(GlobalVariables.forgetPasswordFileHtmlPath).ToString();
//                        _MessageViewHtml = _MessageViewHtml.Replace("[user_name]", em.userName);
//                        _MessageViewHtml = _MessageViewHtml.Replace("[forget_password_link]", em.forgetPasswordLink);
//                        em.msg = _MessageViewHtml;
//                        em.fromEmailAddress = GlobalVariables.fromEmailAddress;
//                        em.subject = "Forget Password Reqeust";
//                        break;
//                    case EmailEnums.SIGNUP_EMAIL:

//                        _MessageViewHtml = stringBuilder.TextToHTML (GlobalVariables.singupPasswordFileHtmlPath).ToString();
//                        _MessageViewHtml = _MessageViewHtml.Replace("[Full_Name]", em.userName);
//                        _MessageViewHtml = _MessageViewHtml.Replace("[logging_link]", "https://workattend.com");
//                        _MessageViewHtml = _MessageViewHtml.Replace("[Website_Name]", "WorkAttend");
//                        em.msg = _MessageViewHtml;
//                        em.fromEmailAddress = GlobalVariables.fromEmailAddress;
//                        em.subject = "Welcome Email";
//                        break;


//                }
//                var result = sendEmail(em);
//                result.Wait();
//                return (result.IsCompletedSuccessfully) ? result.Result : result.Result;
//            }
//            catch (Exception ex)
//            {
//                return false;
//            }

//        }
//        public bool isValidEmail(string email)
//        {
//            try
//            {
//                var addr = new System.Net.Mail.MailAddress(email);

//                return addr.Address == email;
//            }
//            catch
//            {
//                return false;
//            }
//        }
//        //public bool isValidNumber(string strPhoneNumber)
//        //{
//        //    string MatchPhoneNumberPattern = @"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$";
//        //    if (strPhoneNumber != null) return Regex.IsMatch(strPhoneNumber, MatchPhoneNumberPattern);
//        //    else return false;
//        //}
//    }
//}
