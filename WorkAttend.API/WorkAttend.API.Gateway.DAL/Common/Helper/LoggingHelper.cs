using System;
using System.Threading.Tasks;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.DAL.Common.Helper
{
    public static class LoggingHelper
    {
        public static Task InsertException(string source, string message, string originatedAt, string stackTrace, string innerExceptionMessage)
        {
            try
            {
                DateTime now = DateTime.Now;

                appexception newException = new appexception
                {
                    source = source,
                    message = message,
                    originatedAt = originatedAt,
                    stacktrace = stackTrace,
                    innerexceptionmessage = innerExceptionMessage,
                    createdOn = now,
                    createdBy = "system",
                    updatedOn = now,
                    updatedBy = "system"
                };

                var repository = DataContextHelper.GetWorkAttendBaseContext();
                using var db = repository.GetDatabase();
                db.Insert(newException);
            }
            catch
            {
            }

            return Task.CompletedTask;
        }
    }
}