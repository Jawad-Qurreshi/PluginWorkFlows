using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class CreatePORceiptAndRelateToPOProduct : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();

            try
            {

                //  throw new InvalidPluginExecutionException("CreatePORceiptAndRelateToPOProduct");
                ITracingService tracingService =
                  (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                IPluginExecutionContext context = (IPluginExecutionContext)
                    serviceProvider.GetService(typeof(IPluginExecutionContext));

                IOrganizationServiceFactory factory =
                     (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = factory.CreateOrganizationService(context.UserId);


                Entity entity = null;


                List<PurchaseOrderProduct> lstPurchaseOrderProduct = new List<PurchaseOrderProduct>();

                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    entity = (Entity)context.InputParameters["Target"];
                    if (entity.LogicalName == "msdyn_purchaseorder")
                    {

                        if (context.MessageName.ToLower() == "update" && context.Depth == 1)
                        {
                            tracingService.Trace("Update ");


                            Guid newlyCreatedPurchaseOrderReceiptGuid = Guid.Empty;
                            if (entity.Contains("msdyn_systemstatus"))
                            {
                                int systemstatusValue = entity.GetAttributeValue<OptionSetValue>("msdyn_systemstatus").Value;
                                if (systemstatusValue == 690970001)// submitted
                                {
                                    tracingService.Trace("System  Status :  Submitted ");

                                    Entity purchaseOrderEntity = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
                                    tracingService.Trace("entity Retervied ");

                                    if (purchaseOrderEntity != null)
                                    {
                                        newlyCreatedPurchaseOrderReceiptGuid = PurchaseOrderReceipt.CreatePurchaseOrderReceipt(service, tracingService, purchaseOrderEntity,context.UserId);
                                        tracingService.Trace("Purchase Order Receipt Created with Guid " + newlyCreatedPurchaseOrderReceiptGuid.ToString());

                                        if (newlyCreatedPurchaseOrderReceiptGuid != Guid.Empty)
                                        {


                                            lstPurchaseOrderProduct = PurchaseOrderProduct.GetPurchaseOrderProducts(service, tracingService, purchaseOrderEntity.Id);
                                            tracingService.Trace("PO product count to update is " + lstPurchaseOrderProduct.Count.ToString());

                                            if (lstPurchaseOrderProduct.Count > 0)
                                            {
                                                PurchaseOrderProduct.UpdatePurchaseOrderProducts(service, tracingService, lstPurchaseOrderProduct, newlyCreatedPurchaseOrderReceiptGuid);
                                            }

                                            //Entity newPurhcaseOrderEntity = new Entity(entity.LogicalName);
                                            //newPurhcaseOrderEntity.Id = entity.Id;
                                            //purchaseOrderEntity["ap360_datestamped"] = DateTime.Now.Date;
                                            //purchaseOrderEntity["msdyn_approvalstatus"] = new OptionSetValue(690970001);

                                            //throw new InvalidPluginExecutionException("error");
                                            // service.Update(newPurhcaseOrderEntity);



                                            tracingService.Trace("Plugin Ended");
                                        }
                                    }
                                }
                                if (systemstatusValue == 690970000)// draft
                                {
                                    tracingService.Trace("System  Status :  draft ");

                                    PurchaseOrderReceipt.RetrieveAndDeletePurchaseOrderReceipt(service, tracingService, entity.Id);

                                    
                                }



                            }



                        }
                    }
                }
            }






            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
