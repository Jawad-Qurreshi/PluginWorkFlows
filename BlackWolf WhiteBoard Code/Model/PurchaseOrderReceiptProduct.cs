using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using Microsoft.Crm.Sdk.Messages;
//using Microsoft.Xrm.Client;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code.Model
{
    public class PurchaseOrderReceiptProduct
    {

        public static void CreatePurchaseOrderReceiptProductAndUpdatePOProduct(IOrganizationService service, ITracingService tracing, Entity purchaseOrderProductEntity, Guid userId, ref EntityReference poRef)
        {
            tracing.Trace("Inside Creation of PO Recript Product");
            if (purchaseOrderProductEntity != null)
            {
                tracing.Trace("purchaseOrderProductEntity != null");
                // if (newlyCreatedProduct.Id == Guid.Empty || newlyCreatedpurchaseOrderGuid == Guid.Empty || newlyCreatedPurchaseOrderReceiptGuid == Guid.Empty) return;
                Entity newPurchaseOrderReceiptProduct = new Entity("msdyn_purchaseorderreceiptproduct");
                EntityReference purchaseorderRef = purchaseOrderProductEntity.GetAttributeValue<EntityReference>("msdyn_purchaseorder") != null ? purchaseOrderProductEntity.GetAttributeValue<EntityReference>("msdyn_purchaseorder") : null;
                if (purchaseorderRef != null)
                {
                    newPurchaseOrderReceiptProduct["msdyn_purchaseorder"] = new EntityReference("msdyn_purchaseorder", purchaseorderRef.Id);
                    poRef = purchaseorderRef;

                }
                EntityReference warehouseRef = purchaseOrderProductEntity.GetAttributeValue<EntityReference>("msdyn_associatetowarehouse") != null ? purchaseOrderProductEntity.GetAttributeValue<EntityReference>("msdyn_associatetowarehouse") : null;
                if (warehouseRef != null)
                    newPurchaseOrderReceiptProduct["msdyn_associatetowarehouse"] = new EntityReference("msdyn_warehouse", warehouseRef.Id);

                EntityReference poReceipt = purchaseOrderProductEntity.GetAttributeValue<EntityReference>("ap360_purchaseorderreceiptid");
                if (poReceipt == null) throw new InvalidPluginExecutionException("Purchase Order Receipt not exisits ");
                newPurchaseOrderReceiptProduct["msdyn_purchaseorderreceipt"] = poReceipt;
                newPurchaseOrderReceiptProduct["msdyn_purchaseorderproduct"] = new EntityReference("msdyn_purchaseorderproduct", purchaseOrderProductEntity.Id);
                newPurchaseOrderReceiptProduct["msdyn_quantity"] = purchaseOrderProductEntity.GetAttributeValue<double>("ap360_partialreceivedquantity");
                double quantity = purchaseOrderProductEntity.GetAttributeValue<double>("msdyn_quantity");
                // throw new InvalidPluginExecutionException("GeneratePurchaseOrderReceipthello" + quantity);
                //newPurchaseOrderReceiptProduct["msdyn_quantity"] = quantity;

                double partialReceivedQuantity = purchaseOrderProductEntity.GetAttributeValue<double>("ap360_partialreceivedquantity");

                //throw new InvalidPluginExecutionException("Quantity "+quantity+ " partial "+ partialReceivedQuantity);

                //newPurchaseOrderReceiptProduct["msdyn_quantity"] = 0;
                //if (quoteproduct.UnitCost != null)
                //newPurchaseOrderReceiptProduct["msdyn_unitcost"] = new Money(purchaseOrderProductEntity.GetAttributeValue<Money>("msdyn_unitcost").Value);

                newPurchaseOrderReceiptProduct["msdyn_name"] = " PO Receipt Product";
                EntityReference workorderRef = purchaseOrderProductEntity.GetAttributeValue<EntityReference>("msdyn_associatetoworkorder") != null ? purchaseOrderProductEntity.GetAttributeValue<EntityReference>("msdyn_associatetoworkorder") : null;
                //if (workorderRef != null)
                //  newPurchaseOrderReceiptProduct["msdyn_associatetoworkorder"] = new EntityReference("msdyn_workorder", workorderRef.Id);

                service.Create(newPurchaseOrderReceiptProduct);
                //throw new InvalidPluginExecutionException("after creat ion Error ");

               // Entity updatePurchaseOrderProduct = new Entity("msdyn_purchaseorderproduct", purchaseOrderProductEntity.Id);

               // updatePurchaseOrderProduct["ap360_receivedbyid"] = new EntityReference("systemuser", userId);
               // TimeZoneInfo targetZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
               // DateTime newDT = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, targetZone);
               // updatePurchaseOrderProduct["ap360_receivedon"] = newDT;
               // double notReceivedPOPQuantity = quantity - partialReceivedQuantity;
               //// updatePurchaseOrderProduct["ap360_partialreceivedquantity"] = partialReceivedQuantity;



               // service.Update(updatePurchaseOrderProduct);
               // tracing.Trace(quantity + " Quantity and partial Received Product" + partialReceivedQuantity);



                //////////////////////////////////////////////////////////////////////
                if (quantity > partialReceivedQuantity)
                {
                    tracing.Trace("inside if");
                  //  notReceivedPOPQuantity = quantity - partialReceivedQuantity;




                   // Entity retrievedPOP = (Entity)service.Retrieve(purchaseOrderProductEntity.LogicalName, purchaseOrderProductEntity.Id, new ColumnSet(true));
                   // EntityReference wopRef = retrievedPOP.GetAttributeValue<EntityReference>("ap360_workorderproductid") ?? null;

                    //if (wopRef != null)
                    //{




                        //retrievedPOP.EntityState = null;
                        ////remove the PrimaryId of the record otherwise will show you error
                        //retrievedPOP.Attributes.Remove(retrievedPOP.LogicalName + "id");
                        //retrievedPOP.Attributes.Remove("msdyn_lineorder");
                        //retrievedPOP.Attributes["ap360_partialreceivedquantity"] = notReceivedPOPQuantity;
                        //retrievedPOP.Attributes["msdyn_quantity"] = notReceivedPOPQuantity;
                        //retrievedPOP.Attributes["msdyn_itemstatus"] = new OptionSetValue(690970000);//pending
                        //retrievedPOP.Attributes["ap360_receivedon"] = null;
                        //retrievedPOP.Attributes["ap360_receivedbyid"] = null;
                        //retrievedPOP.Id = Guid.NewGuid();

                        ////Create the new cloned record
                        //service.Create(retrievedPOP);


                        //Entity retrievedWOP = (Entity)service.Retrieve(wopRef.LogicalName, wopRef.Id, new ColumnSet(true));
                        //retrievedWOP.EntityState = null;
                        //retrievedWOP.Attributes.Remove(retrievedWOP.LogicalName + "id");
                        //retrievedWOP.Attributes.Remove("msdyn_lineorder");

                        //retrievedWOP.Attributes["msdyn_estimatequantity"] = notReceivedPOPQuantity;

                        //retrievedWOP.Attributes["ap360_workorderproductstatus"] = new OptionSetValue(126300004);//pending
                        //retrievedWOP.Id = Guid.NewGuid();
                        //service.Create(retrievedWOP);

                        //Entity updateWOP = new Entity(wopRef.LogicalName,wopRef.Id);
                        //updateWOP["msdyn_quantity"] = notReceivedPOPQuantity + quantity;
                        //updateWOP["ap360_workorderproductstatus"] = new OptionSetValue(126300004);//pending
                        //service.Update(updateWOP);
                    //}
                    //else
                    //{
                    //    throw new InvalidCastException("WorkOrder Product is null in Purcahse Order Product");
                    //}

                    //Entity createOnOrderInventoryJournal = new Entity("msdyn_inventoryjournal");
                    //createOnOrderInventoryJournal["msdyn_transactiontype"] = new OptionSetValue(690970006);//Manual
                    ////Journal Type
                    ////On Hand     690970000
                    ////On Order    690970001
                    ////Allocated   690970002
                    //createOnOrderInventoryJournal["msdyn_journaltype"] = new OptionSetValue(690970001);
                    //createOnOrderInventoryJournal["msdyn_product"] = purchaseOrderProductEntity.GetAttributeValue<EntityReference>("msdyn_product") ?? null;
                    //createOnOrderInventoryJournal["msdyn_warehouse"] = purchaseOrderProductEntity.GetAttributeValue<EntityReference>("msdyn_associatetowarehouse") ?? null;
                    //createOnOrderInventoryJournal["msdyn_unit"] = purchaseOrderProductEntity.GetAttributeValue<EntityReference>("msdyn_unit") ?? null;
                    //createOnOrderInventoryJournal["msdyn_quantity"] =notReceivedPOPQuantity;
                    ////service.Create(createOnOrderInventoryJournal);

                    //Entity createAllocatedInventoryJournal = new Entity("msdyn_inventoryjournal");
                    //createAllocatedInventoryJournal["msdyn_transactiontype"] = new OptionSetValue(690970006);//Manual
                    ////Journal Type
                    ////On Hand     690970000
                    ////On Order    690970001
                    ////Allocated   690970002
                    //createAllocatedInventoryJournal["msdyn_journaltype"] = new OptionSetValue(690970002);
                    //createAllocatedInventoryJournal["msdyn_product"] = purchaseOrderProductEntity.GetAttributeValue<EntityReference>("msdyn_product") ?? null;
                    //createAllocatedInventoryJournal["msdyn_warehouse"] = purchaseOrderProductEntity.GetAttributeValue<EntityReference>("msdyn_associatetowarehouse") ?? null;
                    //createAllocatedInventoryJournal["msdyn_unit"] = purchaseOrderProductEntity.GetAttributeValue<EntityReference>("msdyn_unit") ?? null;
                    //createAllocatedInventoryJournal["msdyn_quantity"] = notReceivedPOPQuantity;
                    ////service.Create(createAllocatedInventoryJournal);

                }


            }
        }

        public static int GetPurchaseOrderReceiptProductsRelatedTOPOCount(IOrganizationService service, ITracingService tracingservice, Guid purchaseOrderGuid)
        {

            List<PurchaseOrderProduct> lstPurchaseOrderProducts = new List<PurchaseOrderProduct>();

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='msdyn_purchaseorderreceiptproduct'>
                                    <filter type='and'>
                                      <condition attribute='msdyn_purchaseorder' operator='eq'  value='" + purchaseOrderGuid + @"' /> 
                                    </filter>  
                                  </entity>
                                </fetch>");


            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            tracingservice.Trace("Data Reterived successfully");

            return col.Entities.Count;
        }

    }
}
