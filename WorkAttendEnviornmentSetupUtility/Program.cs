using dakarPOS.API.Gateway.BLL;
using dakarPOS.API.Gateway.BLL.databaseGeneration;
using dakarPOS.API.Gateway.DAL.GeneralServices;
using dakarPOS.Shared.DataServices;
using dakarPOS.Shared.Enums;
using dakarPOS.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
namespace POSEnviornmentSetupUtility
{
    public class Program
    {
        public static void ExecutePowerShell(string powerSheScriptPath, StoreRequest storeRequest, DatabaseOnDemandGeneration storeVerification)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo(@"C:\Program Files\PowerShell\7\pwsh.exe",
                string.Format(@"""{0}""", powerSheScriptPath))
                    {
                        // WorkingDirectory = Environment.CurrentDirectory,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                    }
                };
                process.Start();
                process.WaitForExit();
                var reader = process.StandardOutput;
                if (reader != null)
                {
                    string result = reader.ReadToEnd();
                    // Console.WriteLine("PowerShell-End");
                    // Console.WriteLine("PowerShell-Result");
                    //  Console.WriteLine(result);
                    if (result.Contains("</root>"))
                    {
                        storeVerification.DataCubeActive = true;
                        storeVerification.DataCubeError = String.Empty;
                        storeVerification.DataCubeActiveOn = DateTime.Now;
                    }
                    else
                    {
                        storeVerification.DataCubeActive = false;
                        storeVerification.DataCubeError = $"{storeRequest.StoreName} - DW Cube PowerShell Database Error - ,{result}"; ;
                    }
                }

            }
            catch (Exception ex)
            {
                if ((ex.InnerException != null || ex.StackTrace != null))
                {
                    ELHelper helper = new ELHelper();
                    helper.LogException(ex, $"Database AutoGenration Exception");
                }
                storeVerification.DataCubeActive = false;
                storeVerification.DataCubeError = $"{storeRequest.StoreName} - DW Cube PowerShell Database Error - ,{ex.ToString()}"; ;
                //Console.WriteLine("Power SHell Error");
                //Console.WriteLine(storeVerification.DataCubeError);
            }
        }
        public static void GenerateDataBase()
        {
            try
            {
                using (var context = DataContextHelper.GetPOSPortalContext())
                {
                    List<DatabaseOnDemandGeneration> storeVerifications = (List<DatabaseOnDemandGeneration>)GeneralMethod.SelectAllByCondition<DatabaseOnDemandGeneration>("StoreVerifications", "IsCompleted=0", true, default, null);
                    if (storeVerifications != null && storeVerifications.Count > 0)
                    {
                        foreach (var storeVerification in storeVerifications)
                        {
                            if (storeVerification.ProcessedCount < 4)
                            {
                                var storeRequest = GeneralMethod.SelectOneRowByCondition<StoreRequest>("StoreRequests", "IsApproved=0 and StoreRequestId=@0", true, default, storeVerification.StoreRequestId);
                                if (storeRequest != null)
                                {
                                    List<string> cubeFilePath = null;
                                    //Console.WriteLine("GenerateDataBases");
                                    bool isReady = OnDemandDatabaseGenerationServicesBLL.Instance .GenerateDataBases((int)LanguageEnum.English, storeRequest, storeVerification, out cubeFilePath, true);
                                    if (isReady && cubeFilePath != null && cubeFilePath.Count == 2)
                                    {
                                        //Console.WriteLine("GenerateDataBases-Success");
                                        //Console.WriteLine("Powershell Start");
                                        ExecutePowerShell(cubeFilePath[1], storeRequest, storeVerification);
                                        OnDemandDatabaseGenerationServicesBLL.Instance.FilnalizeDataBasesGeneration((int)LanguageEnum.English, storeRequest, storeVerification);
                                    }
                                    else
                                    {
                                        //Console.WriteLine("Database is not ready");
                                    }
                                }
                                else
                                {
                                    //Console.WriteLine("Store already approved.");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if ((ex.InnerException != null || ex.StackTrace != null))
                {
                    ELHelper helper = new ELHelper();
                    helper.LogException(ex, $"Database AutoGenration Exception");
                }
                //Console.WriteLine(ex);
            }
        }
        static void Main(string[] args)
        {
            //Console.WriteLine("Service Starting");
            GenerateDataBase();
            //Console.WriteLine("Service ending");
            //Console.ReadLine();
        }
    }
}
