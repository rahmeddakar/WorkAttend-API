using dakarPOS.API.Gateway.BLL.InterfaceBLL;
using dakarPOS.API.Gateway.BLL.ServicesBLL;
using dakarPOS.Model;
using dakarPOS.Model.DataServices.CustomEntities;
using dakarPOS.Shared.DataServices;
using dakarPOS.Shared.Enums;
using dakarPOS.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace BulkImportExecutionProcess
{
    public class BulkImportExecutionClass
    {
        private readonly IProductServicesBLL _productServicesBLL;
        private readonly IPriceBookServicesBLL _priceBookServicesBLL;
        private readonly ICustomerServicesBLL _customerServicesBLL;

        public BulkImportExecutionClass(IProductServicesBLL productServicesBLL, IPriceBookServicesBLL priceBookServicesBLL, ICustomerServicesBLL customerServicesBLL)
        {
            _productServicesBLL = productServicesBLL;
            _priceBookServicesBLL = priceBookServicesBLL;
            _customerServicesBLL = customerServicesBLL;
        }

        public void BulkImportExecutionProcess()
        {

            using (DakarPOSRepository repository = DataContextHelper.GetPOSPortalContext())//elh -> send this for logging
            {
                try
                {
                    PetaPoco.Sql pSql = PetaPoco.Sql.Builder.Select(@"*").From("BulkImportExecutions BIE").Where("BIE.TryCount<=3 AND BIE.ImportStatusID =@0", (int)ImportStatusesEnums.InProgress);

                    List<BulkImportExecution> bulkImportExecutions = repository.Fetch<BulkImportExecution>(pSql);

                    foreach (var item in bulkImportExecutions)
                    {
                        try
                        {
                            ProductImportValidateModel model = new ProductImportValidateModel();

                            if (item.ImportTypeID == (int)ImportTypesEnums.Products)//Products
                            {
                               model = _productServicesBLL.ProductImportExecutionProcessBLL(item).Result;
                              
                            }
                            if (item.ImportTypeID == (int)ImportTypesEnums.Customers)//Customers
                            {
                                 model = _customerServicesBLL.CustomerImportExecutionProcessBLL(item).Result;
                            }
                            if (item.ImportTypeID == (int)ImportTypesEnums.Pricebooks)//Price Book
                            {
                                model = _priceBookServicesBLL.PBImportExecutionProcessBLL(item).Result;
                            }
                            if (model != null)
                            {
                                UpdateImportStatusandCount(item, model);
                            }

                        }
                        catch (Exception ex)
                        {

                            ImportLogException(ex, item);

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
                    string msg = string.Format("{0} | {1} | Msg: {2} {3}", "BulkImportExecutionProcess", "Fatal", "Bulk Import Execution Process  Service Failed.", ex != null ? "| Stack: " + ex.StackTrace + " | Inner Message: " + ex.InnerException : string.Empty);

                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        eventLog.Source = "Bulk Import Execution Process  Service Failed.";
                        eventLog.WriteEntry(msg, EventLogEntryType.Error, 1002, 1);
                    }
                }
            }
        }

        public void ImportLogException(Exception theEx, BulkImportExecution bulkImportExecution)
        {
            try
            {
                using (DakarPOSRepository repository = DataContextHelper.GetPOSPortalContext())
                {
                    BulkImportExecution importObj = bulkImportExecution;
                    importObj.StackTrace = theEx.StackTrace;
                    importObj.OriginatedAt = theEx.TargetSite != null ? theEx.TargetSite.ReflectedType + "." + theEx.TargetSite.Name + "()" : "UNKNOWN";
                    importObj.ErrorOccurredOn = DateTime.Now;
                    importObj.ErrorMessage = theEx.Message;
                    importObj.HostMachine = System.Environment.MachineName;
                    importObj.TryCount = bulkImportExecution.TryCount + 1;
                    repository.Update(importObj);
                }
            }
            catch (Exception ex)
            {

                throw (new ApplicationException("You must setup Exception Logging properly, check \"ELHelper_SourceApp\" Key in the app.config or web.config AND also check if \"ELConnectionString\" is properly defined."));
            }
        }
        public void UpdateImportStatusandCount(BulkImportExecution bulkImportExecution, ProductImportValidateModel  model)
        {
            try
            {
                using (DakarPOSRepository repository = DataContextHelper.GetPOSPortalContext())
                {
                    BulkImportExecution importObj = bulkImportExecution;
                    int importStatusID = (int)ImportStatusesEnums.InProgress;
                    bool isShowMsg = false;
                    if (bulkImportExecution.TryCount >= 3)
                    {
                        importStatusID = (int)ImportStatusesEnums.Failed;
                    }
                    else
                    {
                        if (model.MessageCode == Convert.ToString(messagesEnum.GENERAL_SUCCESS))
                        {
                            if (model.IsImported)
                            {
                                importStatusID = (int)ImportStatusesEnums.Completed;
                                isShowMsg = true;
                            }
                        }
                    }
                   
                    importObj.TryCount = bulkImportExecution.TryCount + 1;
                    importObj.ImportStatusID = importStatusID;
                    importObj.IsShowMsg = isShowMsg;
                    repository.Update(importObj);
                }
            }
            catch (Exception ex)
            {

                throw (new ApplicationException("You must setup Exception Logging properly, check \"ELHelper_SourceApp\" Key in the app.config or web.config AND also check if \"ELConnectionString\" is properly defined."));
            }
        }

    }
}


