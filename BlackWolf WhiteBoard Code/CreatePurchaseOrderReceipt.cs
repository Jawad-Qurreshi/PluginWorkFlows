using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class CreatePurchaseOrderReceipt : IPlugin
    {

        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();
            //  throw new InvalidPluginExecutionException("GeneratePurchaseOrderReceiptProducts");
            try
            {

                ITracingService tracingService =
                  (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                IPluginExecutionContext context = (IPluginExecutionContext)
                    serviceProvider.GetService(typeof(IPluginExecutionContext));

                IOrganizationServiceFactory factory =
                     (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = factory.CreateOrganizationService(context.UserId);

                tracingService.Trace("Plugin Started");

                string purchaseOrderGUIDString = (string)context.InputParameters["purchaseOrderGUID"];

                //string entityName = (string)context.InputParameters["entityName"];
                Guid purchaseOrderGuid = new Guid(purchaseOrderGUIDString);
                List<PurchaseOrderProduct> lstPurchaseOrderProduct = new List<PurchaseOrderProduct>();

                Entity purchaseOrderEntity = service.Retrieve("msdyn_purchaseorder", purchaseOrderGuid, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));

                tracingService.Trace("entity Retervied ");
                if (purchaseOrderEntity != null)
                {

                    int poSystemStatus = purchaseOrderEntity.GetAttributeValue<OptionSetValue>("msdyn_systemstatus").Value;

                    if (poSystemStatus == 690970000)//Draft
                    {
                        context.OutputParameters["resultMessage"] = "Receipt can only be created for Submitted Purchase Order";
                        return;
                        // throw new InvalidPluginExecutionException("Receipt can't be created for Draft Purchase Order");
                    }
                    lstPurchaseOrderProduct = PurchaseOrderProduct.GetNotReceivedAndPartiallyReceivedPOPs(service, tracingService, purchaseOrderEntity.Id);
                    if (lstPurchaseOrderProduct.Count == 0)
                    {
                        // throw new InvalidPluginExecutionException("All Purchase Order Products are Received");
                        context.OutputParameters["resultMessage"] = "All Purchase Order Products are Received";
                        return;
                    }

                    Guid newlyCreatedPurchaseOrderReceiptGuid = Guid.Empty;
                    newlyCreatedPurchaseOrderReceiptGuid = PurchaseOrderReceipt.CreatePurchaseOrderReceipt(service, tracingService, purchaseOrderEntity, context.UserId);
                    tracingService.Trace("Purchase Order Receipt Created with Guid " + newlyCreatedPurchaseOrderReceiptGuid.ToString());

                    if (newlyCreatedPurchaseOrderReceiptGuid != Guid.Empty)
                    {

                        tracingService.Trace("PO product count to update is " + lstPurchaseOrderProduct.Count.ToString());

                        if (lstPurchaseOrderProduct.Count > 0)
                        {
                            PurchaseOrderProduct.UpdatePurchaseOrderProducts(service, tracingService, lstPurchaseOrderProduct, newlyCreatedPurchaseOrderReceiptGuid);
                        }
                        //else if (lstPurchaseOrderProduct.Count == 0)
                        //{
                        //    // throw new InvalidPluginExecutionException("All Purchase Order Products are Received");
                        //    context.OutputParameters["resultMessage"] = "All Purchase Order Products are Received";
                        //    return;
                        //}


                        context.OutputParameters["resultMessage"] = "Receipt Created";

                        tracingService.Trace("Plugin Ended");
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
