using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class GeneratePurchaseOrderReceiptProducts : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();
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
               // [9:31 PM] Jawad Qurreshi
                string selelctedPurchaseOrderProductGuids = (string)context.InputParameters["purchaseOrderGUID"];


               // string selelctedPurchaseOrderProductGuids = (string)context.InputParameters["selelctedPurchaseOrderProductsGuid"];

                List<string> lstPurchaseOrderProductsIds = selelctedPurchaseOrderProductGuids.Split(',').ToList<string>();
                if (lstPurchaseOrderProductsIds.Count > 0)
                {
                    tracingService.Trace("Selected Purchase Order Count " + lstPurchaseOrderProductsIds.Count.ToString());
                    EntityReference purchaseOrderRef = null;
                    string purchaseOrderProudct = lstPurchaseOrderProductsIds[0];
                   // throw new InvalidPluginExecutionException();
                    Entity retrievedPOReceipt = null;
                    foreach (string purchaseOrderProductId in lstPurchaseOrderProductsIds)
                    {
                        tracingService.Trace("Insdie Foreach");

                        Entity purchaseOrderProductEntity = service.Retrieve("msdyn_purchaseorderproduct", new Guid(purchaseOrderProductId.ToString()), new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
                        if (purchaseOrderProductEntity != null)
                        {
                            EntityReference purchaseOrderReceiptRef = purchaseOrderProductEntity.GetAttributeValue<EntityReference>("ap360_purchaseorderreceiptid") ?? null;
                            if (purchaseOrderReceiptRef != null)
                            {
                                retrievedPOReceipt = service.Retrieve(purchaseOrderReceiptRef.LogicalName, purchaseOrderReceiptRef.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("ap360_receiptused"));
                                bool receiptUsed = retrievedPOReceipt.GetAttributeValue<bool>("ap360_receiptused");
                                if (receiptUsed)
                                {
                                    context.OutputParameters["resultMessage"] = "Create new Receipt";
                                    return;
                                   // throw new InvalidPluginExecutionException("Create new Receipt.");
                                }
                                //Entity updatePOP = new Entity(purchaseOrderProductEntity.LogicalName, purchaseOrderProductEntity.Id);
                                //updatePOP["msdyn_quantity"] = purchaseOrderProductEntity.GetAttributeValue<double>("ap360_partialreceivedquantity");
                                //service.Update(updatePOP);

                                tracingService.Trace("Before Creation of PO receipt Prodcut");

                                PurchaseOrderReceiptProduct.CreatePurchaseOrderReceiptProductAndUpdatePOProduct(service, tracingService, purchaseOrderProductEntity, context.UserId, ref purchaseOrderRef);
                                tracingService.Trace("Plugin Ended");
                            }

                        }


                    }

                    if (purchaseOrderRef != null)
                    {
                        //int purchaseOrderReceiptProductCount = PurchaseOrderReceiptProduct.GetPurchaseOrderReceiptProductsRelatedTOPOCount(service, tracingService, purchaseOrderRef.Id);
                        //bool isQuantitiesSame = true;
                        //int purchaseOrderProductCount = PurchaseOrderProduct.GetPurchaseOrderProductsRelatedTOPOCount(service, tracingService, purchaseOrderRef.Id, out isQuantitiesSame);

                        //Entity updatePurchaseOrder = new Entity(purchaseOrderRef.LogicalName, purchaseOrderRef.Id);

                        //if (purchaseOrderProductCount == purchaseOrderReceiptProductCount)
                        //{
                        //    //This functionality is moved in UpdatePOSubStatusBasedOnSystemStatus  : not finazlied yet
                        //    //if (isQuantitiesSame)
                        //    //{
                        //    //    tracingService.Trace(purchaseOrderProductCount.ToString() + "Inside IF condition" + purchaseOrderReceiptProductCount.ToString());
                        //    //    updatePurchaseOrder["msdyn_substatus"] = new EntityReference("msdyn_purchaseordersubstatus", new Guid("765227fd-267d-eb11-a812-0022480299f1"));//Production Received
                        //    //    updatePurchaseOrder["msdyn_substatus"] = new EntityReference("msdyn_purchaseordersubstatus", new Guid("3e135e8f-257d-eb11-a812-002248029c1e"));// Sandbox Received
                        //    //}
                        //}
                        //else
                        //{
                        //    tracingService.Trace(purchaseOrderProductCount.ToString() + "Inside Else condition" + purchaseOrderReceiptProductCount.ToString());
                        //   //updatePurchaseOrder["msdyn_substatus"] = new EntityReference("msdyn_purchaseordersubstatus", new Guid("1565da59-09c2-ea11-a812-000d3a33f3c3"));// Partially Received
                        //}
                        ////service.Update(updatePurchaseOrder);
                        ////throw new InvalidPluginExecutionException("error");

                    }

                    Entity updatePOReceipt = new Entity(retrievedPOReceipt.LogicalName, retrievedPOReceipt.Id);
                    updatePOReceipt["ap360_receiptused"] = true;
                    service.Update(updatePOReceipt);



                }



                context.OutputParameters["resultMessage"] = "Selected Parts Received";









            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}