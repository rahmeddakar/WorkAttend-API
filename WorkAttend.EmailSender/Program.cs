using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Extensions.Logging.EventLog;
 

namespace WorkAttend.EmailSender
{
    class Program
    {
        static void Main(string[] args)
        {
            string InstanceName = "POSEmailUtility";
            //try
            //{
            //    NLogging.Log(null, false, LogLevel.Info, NLogging.ServiceRun);
            //}
            //catch (Exception ex)
            //{
            //    using (EventLog eventLog = new EventLog("Application"))
            //    {
            //        eventLog.Source = "Application Start";
            //        string msg = string.Format("{0} | {1} | Msg: {2} {3}", InstanceName, "Fatal", "Email Logging Utility Failed.", ex != null ? "| Stack: " + ex.StackTrace + " | Inner Message: " + ex.InnerException : string.Empty);
            //        eventLog.WriteEntry(msg, EventLogEntryType.Error, 1002, 1);
            //    }
            //}
            Exception innerExc = new Exception();
            bool isSuccess = false;
            try
            {
                EmailGenerator es = new EmailGenerator();
                es.SendEmails();
                isSuccess = true;

            }
            catch (Exception ex)
            {
                isSuccess = false;
                innerExc = ex;
                //using (EventLog eventLog = new EventLog("Application"))
                //{
                //    eventLog.Source = "Application Process";
                //    string msg = string.Format("{0} | {1} | Msg: {2} {3}", InstanceName, "Fatal", "Recruitment Email Service Failed.", ex != null ? "| Stack: " + ex.StackTrace + " | Inner Message: " + ex.InnerException : string.Empty);
                //    eventLog.WriteEntry(msg, EventLogEntryType.Error, 1002, 1);
                //}
            }
            //finally
            //{
            //    CommonUtilityServices.Instance.SaveRecruitmentServiceLog(RecruitmentServiceEnum.EmailGenerator, innerExc, "", "", isSuccess);
            //}
        }
    }
}
