using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class SyncPOLineStatusWithWOProductLineStatusAndMapProductPartNumber : IPlugin
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
                Entity entity = null;

                if (context.MessageName.ToLower() == "create")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {

                        entity = (Entity)context.InputParameters["Target"];
                        // throw new InvalidPluginExecutionException("insdide Error");
                        if (entity.LogicalName == "msdyn_purchaseorderproduct")
                        {

                            tracingService.Trace("Inside creation of PO product");
                            Entity reterivedpurchaseOrderProductEntity = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("msdyn_itemstatus", "ap360_itemsubstatus", "ap360_workorderproductid", "msdyn_unitcost", "msdyn_product", "msdyn_purchaseorder"));
                            if (reterivedpurchaseOrderProductEntity != null)
                            {
                                tracingService.Trace("reterivedpurchaseOrderProductEntity != null");
                                //Pending 690970000
                                //Received    690970001
                                //Canceled    690970002


                                EntityReference workOrderProductRef = reterivedpurchaseOrderProductEntity.GetAttributeValue<EntityReference>("ap360_workorderproductid") != null ? reterivedpurchaseOrderProductEntity.GetAttributeValue<EntityReference>("ap360_workorderproductid") : null;
                                int poProductItemstatus = reterivedpurchaseOrderProductEntity.GetAttributeValue<OptionSetValue>("msdyn_itemstatus") != null ? reterivedpurchaseOrderProductEntity.GetAttributeValue<OptionSetValue>("msdyn_itemstatus").Value : 0;
                                int poProductItemsubstatus = reterivedpurchaseOrderProductEntity.GetAttributeValue<OptionSetValue>("ap360_itemsubstatus") != null ? reterivedpurchaseOrderProductEntity.GetAttributeValue<OptionSetValue>("ap360_itemsubstatus").Value : 0;
                                EntityReference msdyn_product = reterivedpurchaseOrderProductEntity.GetAttributeValue<EntityReference>("msdyn_product") != null ? reterivedpurchaseOrderProductEntity.GetAttributeValue<EntityReference>("msdyn_product") : null;
                                EntityReference purchaseOrderRef = reterivedpurchaseOrderProductEntity.GetAttributeValue<EntityReference>("msdyn_purchaseorder") != null ? reterivedpurchaseOrderProductEntity.GetAttributeValue<EntityReference>("msdyn_purchaseorder") : null;
                                // throw new InvalidPluginExecutionException("jawa");

                                if (workOrderProductRef != null)
                                {
                                    tracingService.Trace("workOrderProductRef is not null");


                                    Entity updateWorkOrderProduct = new Entity(workOrderProductRef.LogicalName, workOrderProductRef.Id);
                                    //if (poProductItemstatus == 690970000)// pending
                                    if (poProductItemsubstatus == 126300002)// pending
                                    {
                                        tracingService.Trace("poProductLinestatus is pending");
                                        updateWorkOrderProduct["ap360_workorderproductstatus"] = new OptionSetValue(126300004);//Pending
                                    }
                                    if (purchaseOrderRef != null)
                                    {
                                        tracingService.Trace("purchaseOrderRef is not null");
                                        updateWorkOrderProduct["ap360_purchaseorderid"] = purchaseOrderRef;
                                    }
                                    service.Update(updateWorkOrderProduct);
                                }
                                if (msdyn_product != null)
                                {
                                    tracingService.Trace("product exists");
                                    Entity reterivedProductEntity = service.Retrieve(msdyn_product.LogicalName, msdyn_product.Id, new ColumnSet("ap360_partnumber"));

                                    if (reterivedProductEntity != null)
                                    {
                                        tracingService.Trace("reterivedProductEntity is not null");
                                        string partNumber = reterivedProductEntity.GetAttributeValue<string>("ap360_partnumber");
                                        tracingService.Trace("Part number is " + partNumber.ToString());
                                        //throw new InvalidPluginExecutionException(workOrderProductRef.Name);
                                        if (workOrderProductRef != null)
                                        {
                                            tracingService.Trace("workOrderProductRef != null");
                                            if (workOrderProductRef.Name.ToLower().Contains("-core"))
                                            {
                                                tracingService.Trace("workOrderProductRef.Name.ToLower().Contains(");
                                                // partNumber += "-Core";
                                            }
                                        }

                                        PurchaseOrderProduct.UpdatePurchaseOrderProductPartNumber(service, tracingService, partNumber, entity.Id);
                                    }
                                }
                            }
                        }
                    }
                }

                if (context.MessageName.ToLower() == "update")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        entity = (Entity)context.InputParameters["Target"];
                        if (entity.LogicalName == "msdyn_purchaseorderproduct")
                        {
                            tracingService.Trace("update of  msdyn_purchaseorderproduct");
                            //pre-Execution
                            if (context.Stage == 20)
                            {
                                tracingService.Trace("context.stage is 20");
                                if (entity.Contains("msdyn_itemstatus"))
                                {

                                    tracingService.Trace("item status is updated");

                                    //msdyn_itemstatus
                                    ////Pending 690970000
                                    ////Received    690970001
                                    ////Canceled    690970002
                                    //ap360_itemsubstatus
                                    ////Received    126300000
                                    ////Partially Received  126300001
                                    ////Pending 126300002
                                    ////Canceled    126300003
                                    if (entity.GetAttributeValue<OptionSetValue>("msdyn_itemstatus").Value == 690970000)//Pending
                                        entity["ap360_itemsubstatus"] = new OptionSetValue(126300002);//Pending                                
                                    else if (entity.GetAttributeValue<OptionSetValue>("msdyn_itemstatus").Value == 690970002)//Canceled
                                        entity["ap360_itemsubstatus"] = new OptionSetValue(126300003);//Canceled

                                }

                            }

                            if (context.Stage == 20) return;

                            tracingService.Trace("msdyn_purchaseorderproduct entity updated inside SyncPOLineStatus360_partnumberWithWOProductLineStatusAndMapProductPartNumber");
                            if (entity.Contains("msdyn_product"))
                            {
                                Entity postImage = (Entity)context.PostEntityImages["Image"];

                                string imagePartNumber = postImage.GetAttributeValue<string>("ap360_partnumber");


                                if (imagePartNumber == null)
                                {
                                    tracingService.Trace("entity contains msdyn_product");

                                    EntityReference msdyn_product = entity.GetAttributeValue<EntityReference>("msdyn_product") != null ? entity.GetAttributeValue<EntityReference>("msdyn_product") : null;

                                    Entity reterivedProductEntity = service.Retrieve(msdyn_product.LogicalName, msdyn_product.Id, new ColumnSet("ap360_partnumber"));

                                    if (reterivedProductEntity != null)
                                    {
                                        string partNumber = reterivedProductEntity.GetAttributeValue<string>("ap360_partnumber");
                                        PurchaseOrderProduct.UpdatePurchaseOrderProductPartNumber(service, tracingService, partNumber, entity.Id);
                                    }
                                }
                            }
                            // throw new InvalidPluginExecutionException(context.Depth.ToString());
                            if (context.Depth > 4) return;
                            if (entity.Contains("msdyn_qtyreceived"))
                            {
                                tracingService.Trace("entity contains msdyn_qtyreceived");

                                Entity reterivedpurchaseOrderProductEntity = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("msdyn_quantity", "msdyn_qtyreceived"));
                                double quantity = reterivedpurchaseOrderProductEntity.GetAttributeValue<double>("msdyn_quantity");
                                double quantityReceived = reterivedpurchaseOrderProductEntity.GetAttributeValue<double>("msdyn_qtyreceived");

                                Entity updatePOP = new Entity(entity.LogicalName, entity.Id);


                                updatePOP["ap360_receivedbyid"] = new EntityReference("systemuser", context.UserId);
                                TimeZoneInfo targetZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                                DateTime newDT = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, targetZone);
                                updatePOP["ap360_receivedon"] = newDT;
                                //Received 126300000
                                //Partially Received  126300001
                                //Pending 126300002
                                //Canceled    126300003
                                if (quantity != quantityReceived)
                                {
                                    updatePOP["ap360_itemsubstatus"] = new OptionSetValue(126300001);
                                }
                                else if (quantity == quantityReceived)
                                {
                                    updatePOP["ap360_itemsubstatus"] = new OptionSetValue(126300000);
                                }

                                service.Update(updatePOP);
                            }
                            //if (entity.Contains("msdyn_itemstatus"))
                            if (entity.Contains("ap360_itemsubstatus"))
                            {
                                tracingService.Trace("entity contain msdyn_itemstatus");
                                int poPItemstatus = entity.GetAttributeValue<OptionSetValue>("msdyn_itemstatus") != null ? entity.GetAttributeValue<OptionSetValue>("msdyn_itemstatus").Value : 0;
                                int poPItemsubstatus = entity.GetAttributeValue<OptionSetValue>("ap360_itemsubstatus") != null ? entity.GetAttributeValue<OptionSetValue>("ap360_itemsubstatus").Value : 0;
                                tracingService.Trace("before getting pre Image");
                                Entity preImage = null;
                                preImage = (Entity)context.PreEntityImages["Image"];
                                tracingService.Trace("after getting pre Image");
                                if (poPItemstatus == 690970002) //Cancelled
                                    if (poPItemsubstatus == 126300003) //Cancelled
                                    {
                                        tracingService.Trace("POP Item Status is cancelled");
                                        if (preImage != null)
                                            tracingService.Trace("Pre image is not null ");
                                        EntityReference workOrderProductRef = preImage.GetAttributeValue<EntityReference>("ap360_workorderproductid") ?? null;
                                        int itemstatus = preImage.GetAttributeValue<OptionSetValue>("msdyn_itemstatus") != null ? preImage.GetAttributeValue<OptionSetValue>("msdyn_itemstatus").Value : 0;
                                        tracingService.Trace("Test 2 null ");


                                        if (workOrderProductRef != null)
                                        {
                                            //throw new InvalidPluginExecutionException("This was testewf"+ itemstatus);
                                            Entity updateWorkOrderProduct = new Entity(workOrderProductRef.LogicalName, workOrderProductRef.Id);
                                            tracingService.Trace("updateWorkOrderProduct is not null ");
                                            updateWorkOrderProduct["msdyn_linestatus"] = new OptionSetValue(690970000); // Estimated
                                            updateWorkOrderProduct["ap360_workorderproductstatus"] = new OptionSetValue(126300000); // Authorized to Order
                                            updateWorkOrderProduct["ap360_purchaseorderid"] = null;
                                            updateWorkOrderProduct["ap360_purchaseorderproductid"] = null;
                                            service.Update(updateWorkOrderProduct);
                                        }
                                    }

                            }
                        }
                        if (entity.Contains("ap360_itemsubstatus"))
                        {

                            tracingService.Trace("entity contains ap360_itemsubstatus");

                            Entity reterivedpurchaseOrderProductEntity = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("msdyn_quantity", "msdyn_itemstatus", "ap360_itemsubstatus", "ap360_workorderproductid", "msdyn_unitcost", "ap360_workordersubletid", "msdyn_purchaseorder"));
                            if (reterivedpurchaseOrderProductEntity != null)
                            {
                                //Pending 690970000
                                //Received    690970001
                                //Canceled    690970002
                                int poProductItemstatus = reterivedpurchaseOrderProductEntity.GetAttributeValue<OptionSetValue>("msdyn_itemstatus") != null ? reterivedpurchaseOrderProductEntity.GetAttributeValue<OptionSetValue>("msdyn_itemstatus").Value : 0;
                                int poProductItemsubstatus = reterivedpurchaseOrderProductEntity.GetAttributeValue<OptionSetValue>("ap360_itemsubstatus") != null ? reterivedpurchaseOrderProductEntity.GetAttributeValue<OptionSetValue>("ap360_itemsubstatus").Value : 0;

                                EntityReference workOrderProductRef = reterivedpurchaseOrderProductEntity.GetAttributeValue<EntityReference>("ap360_workorderproductid") ?? null;
                                EntityReference workordersubletRef = reterivedpurchaseOrderProductEntity.GetAttributeValue<EntityReference>("ap360_workordersubletid") ?? null;

                                Money poProductunitcost = reterivedpurchaseOrderProductEntity.GetAttributeValue<Money>("msdyn_unitcost") ?? null;
                                double poQuantity = reterivedpurchaseOrderProductEntity.GetAttributeValue<double>("msdyn_quantity");
                                tracingService.Trace("before poProductItemstatus is received");
                                //if (poProductItemstatus == 690970001)// Received
                                if (poProductItemsubstatus == 126300000 || poProductItemsubstatus == 126300001)// Received
                                {
                                    tracingService.Trace("poProductItemstatus is received");

                                    if (workOrderProductRef != null)
                                    {
                                        tracingService.Trace("workOrderProductRef is not null");

                                        Entity reterviedWOProductEntity = service.Retrieve(workOrderProductRef.LogicalName, workOrderProductRef.Id, new ColumnSet("msdyn_unitcost", "ap360_originalestimateamount", "ap360_revisedestimateamount", "ap360_isrevised", "msdyn_estimatequantity", "ap360_tyrebatterymarkup"));
                                        Entity updateWorkOrderProduct = new Entity(workOrderProductRef.LogicalName, workOrderProductRef.Id);
                                        if (reterviedWOProductEntity != null)
                                        {
                                            Money originalEstimatedAmount = reterviedWOProductEntity.GetAttributeValue<Money>("ap360_originalestimateamount") != null ? reterviedWOProductEntity.GetAttributeValue<Money>("ap360_originalestimateamount") : null;
                                            Money revisedestimateamount = reterviedWOProductEntity.GetAttributeValue<Money>("ap360_revisedestimateamount") != null ? reterviedWOProductEntity.GetAttributeValue<Money>("ap360_revisedestimateamount") : null;

                                            double quantity = reterviedWOProductEntity.GetAttributeValue<double>("msdyn_estimatequantity");
                                            bool tyreBatteryMarkUp = reterviedWOProductEntity.GetAttributeValue<bool>("ap360_tyrebatterymarkup");

                                            bool isrevised = false;
                                            isrevised = reterviedWOProductEntity.GetAttributeValue<bool>("ap360_isrevised");


                                            decimal ap360_multiplier = PriceMarkup.GetPriceMarkUpMultiplier(service, tracingService, poProductunitcost.Value);
                                            updateWorkOrderProduct["msdyn_estimateunitcost"] = new Money(poProductunitcost.Value);
                                            if (tyreBatteryMarkUp != true)
                                            {
                                                updateWorkOrderProduct["ap360_actualamount"] = Convert.ToDecimal(poQuantity) * ap360_multiplier * poProductunitcost.Value;
                                                updateWorkOrderProduct["msdyn_estimatesubtotal"] = Convert.ToDecimal(quantity) * ap360_multiplier * poProductunitcost.Value;
                                                updateWorkOrderProduct["msdyn_estimateunitamount"] = ap360_multiplier * poProductunitcost.Value;
                                            }
                                            else
                                            {
                                                updateWorkOrderProduct["ap360_actualamount"] = Convert.ToDecimal(poQuantity) * Convert.ToDecimal(1.25) * poProductunitcost.Value;
                                                updateWorkOrderProduct["msdyn_estimatesubtotal"] = Convert.ToDecimal(quantity) * Convert.ToDecimal(1.25) * poProductunitcost.Value;
                                                updateWorkOrderProduct["msdyn_estimateunitamount"] = Convert.ToDecimal(1.25) * poProductunitcost.Value;
                                            }


                                            updateWorkOrderProduct["ap360_multiplier"] = ap360_multiplier;
                                            updateWorkOrderProduct["ap360_quantityremaining"] = poQuantity;
                                            updateWorkOrderProduct["msdyn_estimatequantity"] = poQuantity;



                                        }
                                        int itemSubStatus = entity.GetAttributeValue<OptionSetValue>("ap360_itemsubstatus").Value;
                                        //Received 126300000
                                        //Partially Received  126300001
                                        //Pending 126300002
                                        //Canceled    126300003
                                        if (itemSubStatus == 126300000)
                                        {
                                            updateWorkOrderProduct["ap360_workorderproductstatus"] = new OptionSetValue(126300001);//Received}

                                            //Entity reterviedWOPMainBPFEntity = WOPMainBPF.ReteriveWOPMainBPFRelatedtoWOP(service, tracingService, workOrderProductRef);
                                            //if (reterviedWOPMainBPFEntity != null)
                                            //{
                                            //    WOPMainBPF.UpdateWOPMainBPFStage(service, tracingService, reterviedWOPMainBPFEntity, new Guid("68bb9cfd-d875-45eb-8523-6cab106313d0"));//used
                                            //}

                                        }
                                        else if (itemSubStatus == 126300001)
                                        {
                                            updateWorkOrderProduct["ap360_workorderproductstatus"] = new OptionSetValue(126300006);//Partially Received

                                        }

                                        service.Update(updateWorkOrderProduct);


                                        EntityReference poRef = reterivedpurchaseOrderProductEntity.GetAttributeValue<EntityReference>("msdyn_purchaseorder") ?? null;
                                        tracingService.Trace("before po Ref exists");

                                        if (poRef != null)
                                        {
                                            tracingService.Trace("Purchase Order Ref exists");
                                            //int purchaseOrderReceiptProductCount = PurchaseOrderReceiptProduct.GetPurchaseOrderReceiptProductsRelatedTOPOCount(service, tracingService, poRef.Id);
                                            bool isAllItemsSubStatusRecevied = true;
                                            int purchaseOrderProductCount = PurchaseOrderProduct.CheckAllPurchaseOrderProductsAreRecevied(service, tracingService, poRef.Id, out isAllItemsSubStatusRecevied);

                                            Entity updatePurchaseOrder = new Entity(poRef.LogicalName, poRef.Id);
                                            //if (purchaseOrderProductCount == purchaseOrderReceiptProductCount)
                                            tracingService.Trace(" isAllItemsSubStatusRecevied " + isAllItemsSubStatusRecevied.ToString());
                                            //    //This functionality is moved in UpdatePOSubStatusBasedOnSystemStatus  : not finazlied yet
                                            if (isAllItemsSubStatusRecevied)
                                            {

                                                tracingService.Trace("if");
                                                // tracingService.Trace(purchaseOrderProductCount.ToString() + "Inside IF condition" + purchaseOrderReceiptProductCount.ToString());
                                                //   updatePurchaseOrder["msdyn_substatus"] = new EntityReference("msdyn_purchaseordersubstatus", new Guid("765227fd-267d-eb11-a812-0022480299f1"));//Production Received
                                                //updatePurchaseOrder["msdyn_substatus"] = new EntityReference("msdyn_purchaseordersubstatus", new Guid("3e135e8f-257d-eb11-a812-002248029c1e"));// Sandbox Received
                                            }
                                            //}
                                            else
                                            {
                                                tracingService.Trace("else ");

                                                // tracingService.Trace(purchaseOrderProductCount.ToString() + "Inside Else condition" + purchaseOrderReceiptProductCount.ToString());
                                                updatePurchaseOrder["msdyn_substatus"] = new EntityReference("msdyn_purchaseordersubstatus", new Guid("1565da59-09c2-ea11-a812-000d3a33f3c3"));// Partially Received
                                            }
                                            //throw new InvalidPluginExecutionException("custom error updated ");
                                            service.Update(updatePurchaseOrder);
                                        }
                                    }

                                    //  throw new InvalidPluginExecutionException("Error in code again "+ work.LogicalName);
                                    if (workordersubletRef != null)
                                    {

                                        //Entity reterviedWOSubletEntity = service.Retrieve(workordersubletRef.LogicalName, workordersubletRef.Id, new ColumnSet("msdyn_unitcost", "ap360_originalestimateamount", "ap360_revisedestimateamount", "ap360_isrevised", "msdyn_estimatequantity"));
                                        Entity updateWorkOrderSublet = new Entity(workordersubletRef.LogicalName, workordersubletRef.Id);


                                        if (poProductunitcost != null)
                                        {
                                            decimal mulitplyValue = 1.3m;
                                            updateWorkOrderSublet["ap360_actualamount"] = new Money(poProductunitcost.Value * mulitplyValue);

                                            service.Update(updateWorkOrderSublet);
                                        }

                                    }
                                }
                            }
                            // throw new InvalidPluginExecutionException("custom error");

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