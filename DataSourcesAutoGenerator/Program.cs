using dakarPOS.API.Gateway.BLL;
using dakarPOS.API.Gateway.BLL.databaseGeneration;
using dakarPOS.API.Gateway.BLL.InterfaceBLL;
using dakarPOS.API.Gateway.DAL.GeneralServices;
using dakarPOS.API.Gateway.DAL.services;
using dakarPOS.API.Gateway.DAL.services.storeServices;
using dakarPOS.Shared.DataServices;
using dakarPOS.Shared.Enums;
using dakarPOS.Shared.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DataSourcesAutoGenerator
{
    public class Program
    {
        public static void ExecutePowerShell(string powerSheScriptPath, DatabaseAutoGeneration databaseAutoGeneration, DakarPOSRepository context)
        {
            try
            {
                // Console.WriteLine("NEW PWS7");
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo($@"{GlobalAppConfigs.PowerShellPath}",
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
                    //// Console.WriteLine("PowerShell-End");
                    // Console.WriteLine($"PowerShell-Result for database {databaseAutoGeneration.DatabaseName}");
                    // Console.WriteLine(result);
                    if (result.Contains("</root>"))
                    {
                        databaseAutoGeneration.DataCubeActive = true;
                        databaseAutoGeneration.DataCubeError = String.Empty;
                        databaseAutoGeneration.DataCubeActiveOn = DateTime.Now;
                    }
                    else
                    {
                        databaseAutoGeneration.DataCubeActive = false;
                        databaseAutoGeneration.DataCubeError = $"{databaseAutoGeneration.DatabaseName} - DW Cube PowerShell Database Error - ,{result}"; ;
                    }
                }

            }
            catch (Exception ex)
            {
                //if ((ex.InnerException != null || ex.StackTrace != null))
                //{
                //    ELHelper helper = new ELHelper();
                //    helper.LogException(ex, $"Database AutoGenration Exception");
                //}
                databaseAutoGeneration.DataCubeActive = false;
                databaseAutoGeneration.DataCubeError = $"{databaseAutoGeneration.DatabaseName} - DW Cube PowerShell Database Error - ,{ex.ToString()}"; ;
                context.Update(databaseAutoGeneration);
                // Console.WriteLine("Power SHell Error");
                // Console.WriteLine(databaseAutoGeneration.DataCubeError);
            }
        }
        public static void GenerateAutomaticDataBase(IAutoDatabaseGenerationServicesBLL autoDbService)
        {
            int DBToProduceCount = GlobalAppConfigs.AutoDataBaseCount;
            DBToProduceCount = DBToProduceCount > 0 ? DBToProduceCount : 20;
            try
            {
                Console.Write("Reading key from appconfig");

                using (var context = DataContextHelper.GetPOSPortalContext())
                {
                    List<DatabaseAutoGeneration> autoDatabases = GeneralMethod.SelectAllByCondition<DatabaseAutoGeneration>("DatabaseAutoGenerations", "IsCompleted=0", true, default, null).ToList();
                    if (autoDatabases == null)
                    {
                        // Console.WriteLine($"DB Table DatabaseAutoGeneration IS NULL");
                    }
                    else
                    {
                        // Console.WriteLine($"DB Table DatabaseAutoGeneration HAS {autoDatabases.Count} ROWS");
                    }
                    if (autoDatabases == null || (autoDatabases != null && autoDatabases.Count == 0))
                    {
                        int newAutoDBs = autoDatabases == null ? DBToProduceCount : (DBToProduceCount - autoDatabases.Count);
                        if (newAutoDBs > 0)
                        {
                            // Console.WriteLine($"UTILITY TO PRODUCE TOTAL {newAutoDBs} DBs");

                            if (autoDatabases == null)
                            {
                                autoDatabases = new List<DatabaseAutoGeneration>();
                            }
                            for (int i = 0; i < newAutoDBs; i++)
                            {
                                // Console.WriteLine($"UTILITY PRODUCING DB FOR INDEX {i}");

                                try
                                {
                                    string guid = Guid.NewGuid().ToString();
                                    guid = guid.Substring(guid.Length - 6, 5);
                                    DatabaseAutoGeneration generation = new DatabaseAutoGeneration();
                                    generation.DatabaseName = $"auto_{guid}";
                                    generation.ProcessedCount = 0;
                                    generation.CreatedOn = DateTime.Now;
                                    var obj = context.Insert(generation);
                                    generation.DatabaseAutoGenerationId = int.Parse(obj.ToString());
                                    generation.DatabaseName = $"auto_{generation.DatabaseAutoGenerationId}";
                                    context.Update(generation);
                                    autoDatabases.Add(generation);
                                }
                                catch (Exception ex)
                                {
                                    // Console.WriteLine($"UTILITY ERROR FOR CREATING AUTO DB ENTRY");
                                    // Console.WriteLine(ex.ToString());
                                }
                            }
                        }
                        else
                        {
                            // Console.WriteLine($"ZERO DB TO PRODUCE");
                        }
                    }
                    if (autoDatabases != null && autoDatabases.Count > 0)
                    {
                        // Console.WriteLine($"UTILITY START PRODUCING DBs");

                        foreach (var autoDatabase in autoDatabases)
                        {
                            // Console.WriteLine($"UTILITY TO PRODUCE DB FOR {autoDatabase.DatabaseName} and ID {autoDatabase.DatabaseAutoGenerationId}");

                            //if (autoDatabase.ProcessedCount < 10)
                            {
                                //var result = await AutoDatabaseGenerationServicesBLL.Instance.GenerateAutoDataBases((int)LanguageEnum.English, autoDatabase);
                                var result = autoDbService.GenerateAutoDataBases((int)LanguageEnum.English, autoDatabase);
                                if (result != null && result.Result)
                                {
                                    // Console.WriteLine($"READY TO RUN POWER SHELL");
                                    if (result.DWCubeFilePaths != null && result.DWCubeFilePaths.Count == 2)
                                    {

                                        // Console.WriteLine("Powershell Start for cube {autoDatabase.DatabaseName}");
                                        AppException app = new AppException()
                                        {
                                            CustomMessage = "powershell started",
                                            OccuredOn = DateTime.Now
                                        };
                                        //context.Insert(app);
                                        ExecutePowerShell(result.DWCubeFilePaths[1], autoDatabase, context);
                                        string res = autoDatabase.DataCubeActive ? "true" : "false";

                                        AppException apsp = new AppException()
                                        {
                                            CustomMessage = "powershell finished -> " + res,
                                            OccuredOn = DateTime.Now
                                        };
                                        //context.Insert(apsp);
                                    }
                                    if (!autoDatabase.IsSeedValueDone && autoDatabase.DataBaseActive && autoDatabase.DatawareHouseActive && autoDatabase.DataCubeActive)
                                    {
                                        AppException app = new AppException()
                                        {
                                            CustomMessage = "FilnalizeAutoDataBasesGeneration started",
                                            OccuredOn = DateTime.Now
                                        };
                                        //context.Insert(app);
                                        // Console.WriteLine($"READY TO FINALIZE AND SEED VALUES FOR {autoDatabase.DatabaseName} ");
                                        //await AutoDatabaseGenerationServicesBLL.Instance.FilnalizeAutoDataBasesGeneration((int)LanguageEnum.English, autoDatabase);
                                        bool finalRes = autoDbService.FilnalizeAutoDataBasesGeneration((int)LanguageEnum.English, autoDatabase);
                                        string res = finalRes ? "true" : "false";
                                        AppException apsp = new AppException()
                                        {
                                            CustomMessage = "powershell finished ->" + res,
                                            OccuredOn = DateTime.Now
                                        };
                                        //context.Insert(apsp);
                                        //if (finalRes)
                                        // Console.WriteLine($"FINALIZE HAS ISSUE");
                                        //else
                                        // Console.WriteLine($"FINALIZE HAS DONE SUCCESSFULLY");
                                    }
                                }
                                if (result != null && !string.IsNullOrEmpty(result.Error))
                                {
                                    // Console.WriteLine($"ERROR FOR GENERATING DATABASES {result.Error}");
                                }
                                if (result != null && result.Result)
                                {
                                    // Console.WriteLine($"DB FOR {autoDatabase.DatabaseName} and ID {autoDatabase.DatabaseAutoGenerationId} DONE");
                                }
                                else
                                {
                                    // Console.WriteLine($"DB FOR {autoDatabase.DatabaseName} and ID {autoDatabase.DatabaseAutoGenerationId} NOT DONE");
                                }
                            }
                        }
                    }
                    else
                    {
                        // Console.WriteLine($"NO RECORD FOUND TO PRODUCE AUTO DBs");
                    }
                }

                List<DatabaseAutoGeneration> autoDatabaseSeedValues = GeneralMethod.SelectAllByCondition<DatabaseAutoGeneration>("DatabaseAutoGenerations", "IsCompleted=1 and StoreRequestId > 0 and DataBaseActive=1 and IsSeedValueDone=0", true, default, null).ToList();
                if (autoDatabaseSeedValues != null && autoDatabaseSeedValues.Count > 0)
                {
                    foreach (var item in autoDatabaseSeedValues)
                    {
                        if (item.IsCompleted && item.DataBaseActive && !item.IsSeedValueDone)
                        {
                            // Console.WriteLine($"Setting seed values in rechecking");

                            //var result = await AutoDatabaseGenerationServicesBLL.Instance.SetOnlySeedValueForDatabase((int)LanguageEnum.English, item);
                            var seedRes = autoDbService.SetOnlySeedValueForDatabase((int)LanguageEnum.English, item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // // Console.WriteLine(ex);
                //if ((ex.InnerException != null || ex.StackTrace != null))
                //{
                //    ELHelper helper = new ELHelper();
                //    helper.LogException(ex, $"Database AutoGenration Exception");
                //}
                // Console.WriteLine("ERROR HAPPENED");
                // Console.WriteLine(ex);
                // Console.ReadLine();
            }
        }
        static void Main(string[] args)
        {
            var services = new ServiceCollection();

            // Register DAL/BLL dependencies
            services.AddSingleton<IStoreServices, StoreServices>(); 
            services.AddSingleton<IAutoDatabaseGenerationServicesBLL, AutoDatabaseGenerationServicesBLL>();

            var provider = services.BuildServiceProvider();

            var autoDbService = provider.GetRequiredService<IAutoDatabaseGenerationServicesBLL>();

            try
            {
                GenerateAutomaticDataBase(autoDbService);
            }
            catch (Exception ex)
            {
                // log
            }
        }
    }
}
