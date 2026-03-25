//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Text;
//using WorkAttend.Shared.DataServices;
//using WorkAttend.Shared.Extensions;

//namespace WorkAttend.Shared.Helpers
//{
//    public class ELHelper
//    {
//        public ELHelper()
//        {
//            _aeData = new Dictionary<string, object>();
//        }

//        const string SOURCE_APP_KEY = "ELHelper_SourceApp";
//        const string LAST_SQL_KEY = "LastSqlQuery";
//        const string WARNING = "W";
//        const string ERROR = "E";
//        Dictionary<string, object> _aeData;

//        public void LogException(Exception theEx, string customMessage = null, string dbName = null)
//        {
//            ELHelper elh = new ELHelper();

//            try
//            {
//                using (WorkAttendRepository repository = string.IsNullOrEmpty(dbName) ? DataContextHelper.GetPOSPortalContext() : DataContextHelper.GetStoreDataContext(dbName))
//                {
//                    repository.ELHelperInstance = elh;

//                    AppException appEx = new AppException()
//                    {
//                        SourceApp = System.Configuration.ConfigurationManager.AppSettings[SOURCE_APP_KEY],
//                        Message = theEx.Message,
//                        OriginatedAt = theEx.TargetSite != null ? theEx.TargetSite.ReflectedType + "." + theEx.TargetSite.Name + "()" : "UNKNOWN",
//                        StackTrace = theEx.StackTrace,
//                        InnerExceptionMessage = theEx.InnerException != null ? theEx.InnerException.Message : null,
//                        HostMachine = System.Environment.MachineName,
//                        OccuredOn = DateTime.Now,
//                        CustomMessage = customMessage,
//                        Level = ERROR,
//                        HelpLink = theEx.HelpLink,
//                        CustomSource = theEx.Source,
//                    };

//                    repository.Insert(appEx);

                 
//                }
//            }
//            catch (Exception ex)
//            {
//                // Ok this sucks. and should never happen...
//                throw (new ApplicationException(string.Format(
//                    "You must setup Exception Logging properly, check \"ELHelper_SourceApp\" Key in the app.config or web.config AND also check if \"ELConnectionString\" is properly defined.////ORIG=MSG/////{0}//////////ORIG-INNER/////////{1}////////ORIG STACK///////////{2}///////////LOG EX MSG///////////{3}////////////////LOG EX INNER////////////{4}///////////////LOG EX STACK///////////////{5}",
//                    theEx.Message, theEx.InnerException, theEx.StackTrace, ex.Message, ex.InnerException, ex.StackTrace)));
//            }
//        }

//        public void LogWarning(string customMessage = null)
//        {
//            ELHelper elh = new ELHelper();

//            try
//            {
//                using (WorkAttendRepository repository = DataContextHelper.GetPOSPortalContext())
//                {
//                    repository.ELHelperInstance = elh;

//                    AppException appEx = new AppException()
//                    {
//                        SourceApp = System.Configuration.ConfigurationManager.AppSettings[SOURCE_APP_KEY],
//                        Message = customMessage,
//                        OriginatedAt = null,
//                        StackTrace = null,
//                        InnerExceptionMessage = null,
//                        HostMachine = System.Environment.MachineName,
//                        OccuredOn = DateTime.Now,
//                        CustomMessage = customMessage,
//                        Level = WARNING
//                    };

//                    repository.Insert(appEx);
//                }
//            }
//            catch (Exception ex)
//            {
//                // Ok this sucks. and should never happen...
//                throw (new ApplicationException("You must setup Exception Logging properly, check \"ELHelper_SourceApp\" Key in the app.config or web.config AND also check if \"ELConnectionString\" is properly defined."));
//            }
//        }

//        public void AddVariable<T>(Expression<Func<T>> expr)
//        {
//            _aeData.AddOrUpdate(((MemberExpression)expr.Body).Member.Name, expr.Compile().Invoke());
//        }

//        public void AddVariable(string variableName, object value)
//        {
//            _aeData.AddOrUpdate(variableName, value);
//        }

//        public void AddLastExecutedPetapocoSql(PetaPoco.Sql lastSql)
//        {
//            StringBuilder sb = new StringBuilder(lastSql.SQL);

//            for (int i = 0; i < lastSql.Arguments.Count(); i++)
//            {
//                Type t = lastSql.Arguments[i].GetType();

//                if (t.Name == "String")
//                {
//                    sb.Replace("@" + i, "'" + lastSql.Arguments[i] + "'");
//                }
//                else
//                {
//                    sb.Replace("@" + i, lastSql.Arguments[i].ToString());
//                }
//                // TODO: Check for DateTime type also
//            }

//            _aeData.AddOrUpdate(LAST_SQL_KEY, sb.ToString());
//        }

//        public void AddLastExecutedSql(string lastSql)
//        {
//            _aeData.AddOrUpdate(LAST_SQL_KEY, lastSql);
//        }
//    }
//}
