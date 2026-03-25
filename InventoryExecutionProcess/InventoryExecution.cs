using dakarPOS.API.Gateway.BLL.InterfaceBLL;
using dakarPOS.API.Gateway.BLL.ServicesBLL;
using dakarPOS.Model.DataServices.CustomEntities;
using dakarPOS.Shared.DataServices;
using dakarPOS.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace InventoryExecutionProcess
{
    public class InventoryExecutionClass
    {
        private readonly IInventoryServicesBLL _inventoryServicesBLL;

        public InventoryExecutionClass(IInventoryServicesBLL inventoryServicesBLL)
        {
            _inventoryServicesBLL = inventoryServicesBLL;
        }

        public void InventoryExecutionProcess()
        {

            using (DakarPOSRepository repository = DataContextHelper.GetPOSPortalContext())//elh -> send this for logging
            {
                try
                {
                    PetaPoco.Sql pSql = PetaPoco.Sql.Builder.Select(@"*").From("InventoryExecutions IE").Where("IE.IsCompleted = 0 AND IE.TryCount<=3");

                    List<InventoryExecution> inventoryExecution = repository.Fetch<InventoryExecution>(pSql);

                    foreach (var item in inventoryExecution)
                    {
                        try
                        {
                            var result = _inventoryServicesBLL.InventoryExecutionProcessBLL(item).Result;

                        }
                        catch (Exception ex)
                        {

                            InventoryLogException(ex, item);

                            string msg = string.Format("{0} | {1} | Msg: {2} {3}", "Inventory Execution ", "Fatal", "Inventory Execution  Failed.", ex != null ? "| Stack: " + ex.StackTrace + " | Inner Message: " + ex.InnerException : string.Empty);
                            using (EventLog eventLog = new EventLog("Application"))
                            {
                                eventLog.Source = "Email Sending Failed.";
                                eventLog.WriteEntry(msg, EventLogEntryType.Error, 1002, 1);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    string msg = string.Format("{0} | {1} | Msg: {2} {3}", "InventoryExecutionProcess", "Fatal", "Inventory Execution Process  Service Failed.", ex != null ? "| Stack: " + ex.StackTrace + " | Inner Message: " + ex.InnerException : string.Empty);

                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        eventLog.Source = "Inventory Execution Process  Service Failed.";
                        eventLog.WriteEntry(msg, EventLogEntryType.Error, 1002, 1);
                    }
                }
            }
        }

        public void InventoryLogException(Exception theEx, InventoryExecution inventoryExecution)
        {
            try
            {
                using (DakarPOSRepository repository = DataContextHelper.GetPOSPortalContext())
                {

                    InventoryExecution invObj = inventoryExecution;
                    invObj.StackTrace = theEx.StackTrace;
                    invObj.OrginatedAt = theEx.TargetSite != null ? theEx.TargetSite.ReflectedType + "." + theEx.TargetSite.Name + "()" : "UNKNOWN";
                    invObj.ErrorOcurredOn = DateTime.Now;
                    invObj.ErrorMessage = theEx.Message;
                    invObj.HostMachine = System.Environment.MachineName;
                    invObj.TryCount = inventoryExecution.TryCount + 1;
                    repository.Update(invObj);
                }
            }
            catch (Exception ex)
            {

                throw (new ApplicationException("You must setup Exception Logging properly, check \"ELHelper_SourceApp\" Key in the app.config or web.config AND also check if \"ELConnectionString\" is properly defined."));
            }
        }

    }
}


