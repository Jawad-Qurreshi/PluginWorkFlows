using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class CreatePurchaseOrderforPreferredSupplier : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();
            //  throw new InvalidPluginExecutionException("CreatePurchaseOrderforPreferredSupplier");

            try
            {

                ITracingService tracingService =
                  (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                IPluginExecutionContext context = (IPluginExecutionContext)
                    serviceProvider.GetService(typeof(IPluginExecutionContext));

                IOrganizationServiceFactory factory =
                     (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = factory.CreateOrganizationService(context.UserId);

                EntityReference workOrderRef = null;

                EntityReference preferrredSupplierId = (EntityReference)context.InputParameters["preferredSupplierId"];
                string selelctedWorkOrderProductGuids = (string)context.InputParameters["selelctedWorkOrderProductsGuid"];

                List<string> lstWorkOrderProductsIds = selelctedWorkOrderProductGuids.Split(',').ToList<string>();
                Guid newlyCreatedpurchaseOrderGuid;
                if (lstWorkOrderProductsIds.Count > 0)
                {
                    tracingService.Trace("Count is > 0");
                    List<WorkOrderProduct> lstWorkOrderProduct = new List<WorkOrderProduct>();
                    WorkOrderProduct workOrderProduct;
                    Entity purchaseOrder = new Entity("msdyn_purchaseorder");
                    if (preferrredSupplierId != null)
                        purchaseOrder["msdyn_vendor"] = new EntityReference("account", preferrredSupplierId.Id);
                    purchaseOrder["msdyn_purchaseorderdate"] = DateTime.Now.Date;
                    purchaseOrder["transactioncurrencyid"] = new EntityReference("transactioncurrency", new Guid("d513407b-f54f-ea11-a814-000d3a30fcff"));//US Dollar
                    purchaseOrder["ownerid"] = new EntityReference("systemuser", context.UserId);


                    //purchaseOrder["ap360_opportunity"] = new EntityReference("systemuser", context.UserId);
                    //purchaseOrder["ap360_opportunityautonumber"] = "12";



                    //purchaseOrder["ap360_opportunityid"] = new EntityReference("opportunity", quote.Opportunity.Id);
                    //purchaseOrder["msdyn_workorder"] = new EntityReference("msdyn_workorder", workOrderGuid);
                    newlyCreatedpurchaseOrderGuid = service.Create(purchaseOrder);
                    tracingService.Trace("Before foreach");
                    foreach (string workOrderProductId in lstWorkOrderProductsIds)
                    {
                        Entity reterviedWorkOrderProduct = service.Retrieve("msdyn_workorderproduct", new Guid(workOrderProductId.ToString()), new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
                        workOrderProduct = new WorkOrderProduct();
                        if (reterviedWorkOrderProduct != null)
                        {
                            tracingService.Trace("Retervied WorkOrder product is not nul");
                            Entity purchaseOrderProduct = new Entity("msdyn_purchaseorderproduct");
                            //purchaseOrderProduct["msdyn_product"] = new EntityReference("product", newlyCreatedProductGuid);

                            purchaseOrderProduct["msdyn_purchaseorder"] = new EntityReference("msdyn_purchaseorder", newlyCreatedpurchaseOrderGuid);

                            workOrderRef = reterviedWorkOrderProduct.GetAttributeValue<EntityReference>("msdyn_workorder") ?? null;
                            if (workOrderRef != null)
                            {
                                purchaseOrderProduct["msdyn_associatetoworkorder"] = new EntityReference("msdyn_workorder", workOrderRef.Id);

                            }
                            string Manufacturer = reterviedWorkOrderProduct.GetAttributeValue<string>("ap360_manufacturer")?? null;
                            string BarCode = reterviedWorkOrderProduct.GetAttributeValue<string>("ap360_barcode")?? null;
                            int sku = reterviedWorkOrderProduct.GetAttributeValue<int>("ap360_sku");
                            EntityReference vendor = reterviedWorkOrderProduct.GetAttributeValue<EntityReference>("ap360_vendorid") ?? null;
                            EntityReference unit = reterviedWorkOrderProduct.GetAttributeValue<EntityReference>("msdyn_unit") ?? null;
                            EntityReference product = reterviedWorkOrderProduct.GetAttributeValue<EntityReference>("ap360_product") ??null;
                            EntityReference wareHouse = reterviedWorkOrderProduct.GetAttributeValue<EntityReference>("msdyn_warehouse")?? null;
                            Money UnitCost = reterviedWorkOrderProduct.GetAttributeValue<Money>("msdyn_estimateunitcost")?? null;
                            bool ap360_iscustomerpart = reterviedWorkOrderProduct.GetAttributeValue<bool>("ap360_iscustomerpart");



                            if (ap360_iscustomerpart)
                            {

                                throw new InvalidPluginExecutionException("Purchase Order can't be created for Customer Parts");
                            }

                            if (UnitCost != null)
                                purchaseOrderProduct["msdyn_unitcost"] = new Money(UnitCost.Value);
                            //we don"t need to map unit price in Purchase Order Product
                            purchaseOrderProduct["msdyn_quantity"] = reterviedWorkOrderProduct.GetAttributeValue<double>("msdyn_estimatequantity");
                            purchaseOrderProduct["ap360_workorderproductid"] = new EntityReference("msdyn_workorderproduct", reterviedWorkOrderProduct.Id);


                            EntityReference opportunity = reterviedWorkOrderProduct.GetAttributeValue<EntityReference>("ap360_opportunityid") ?? null;
                            if (opportunity != null)
                            {
                                Entity reterviedOpportunity = service.Retrieve(opportunity.LogicalName, opportunity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("ap360_opportunityautonumber"));
                                string opportunityAutoNumber = reterviedOpportunity.GetAttributeValue<string>("ap360_opportunityautonumber");

                                purchaseOrderProduct["ap360_opportunityautonumber"] = opportunityAutoNumber;
                                purchaseOrderProduct["ap360_opportunityid"] = new EntityReference(opportunity.LogicalName, opportunity.Id);

                            }
                            if (Manufacturer != null)
                                purchaseOrderProduct["ap360_manufacturer"] = Manufacturer;
                            if (BarCode != null)
                                purchaseOrderProduct["ap360_barcode"] = BarCode;
                            if (vendor != null)
                            {
                                if (vendor.Id.ToString().ToLower() == "a89044d7-9b9a-ea11-a811-000d3a33f3c3" || vendor.Id.ToString().ToLower() == "28b07b2b-4928-eb11-a813-000d3a368915")//blackWolf Inventory  or Customer Supplied
                                {
                                    throw new InvalidPluginExecutionException("Purchase Order Can't be created for BlackWolf or Customer Supplied Parts");
                                }

                                purchaseOrderProduct["ap360_vendorid"] = new EntityReference("account", vendor.Id);
                            }
                            purchaseOrderProduct["ap360_itemsubstatus"] = new OptionSetValue(126300002);//Pending

                            if (unit != null)
                                purchaseOrderProduct["msdyn_unit"] = new EntityReference("uom", unit.Id);
                            else
                                purchaseOrderProduct["msdyn_unit"] = new EntityReference("uom", new Guid("361a3eac-749c-4bb3-92a2-d63f692f61ba"));//UNIT
                            if (product != null)
                                purchaseOrderProduct["msdyn_product"] = new EntityReference("product", product.Id);
                            else
                                throw new InvalidPluginExecutionException("Product is not selected for WorkOrder Product");

                            if (wareHouse != null)
                                purchaseOrderProduct["msdyn_associatetowarehouse"] = new EntityReference("msdyn_warehouse", wareHouse.Id);
                            else
                                throw new InvalidPluginExecutionException("WareHouse is not selected for WorkOrder Product");



                            purchaseOrderProduct["ap360_sku"] = sku;

                            Guid newlyCreatedPOProductGuid = service.Create(purchaseOrderProduct);
                            workOrderProduct.guid = new Guid(workOrderProductId);
                            workOrderProduct.PurchaseOrderProductGuid = newlyCreatedPOProductGuid;
                            lstWorkOrderProduct.Add(workOrderProduct);
                            // Entity updateWorkOrderProduct = new Entity();

                        }

                    }

                    //foreach (string workOrderProductId in lstWorkOrderProductsIds)
                    //{
                    //    Entity updateWorkOrderProduct = new Entity("msdyn_workorderproduct");
                    //    updateWorkOrderProduct.Id = new Guid(workOrderProductId);
                    //    updateWorkOrderProduct["ap360_purchaseorderid"] = new EntityReference("msdyn_purchaseorder", newlyCreatedpurchaseOrderGuid);
                    //    if (preferrredSupplierId != null)
                    //        updateWorkOrderProduct["ap360_vendorid"] = new EntityReference("account", preferrredSupplierId.Id);

                    //    service.Update(updateWorkOrderProduct);

                    //}

                    foreach (WorkOrderProduct woProduct in lstWorkOrderProduct)
                    {
                        Entity updateWorkOrderProduct = new Entity("msdyn_workorderproduct");
                        updateWorkOrderProduct.Id = woProduct.guid;
                        if (woProduct.PurchaseOrderProductGuid != Guid.Empty && woProduct.PurchaseOrderProductGuid != null)
                        {
                            updateWorkOrderProduct["ap360_purchaseorderproductid"] = new EntityReference("msdyn_purchaseorderproduct", woProduct.PurchaseOrderProductGuid);
                        }
                        updateWorkOrderProduct["ap360_purchaseorderid"] = new EntityReference("msdyn_purchaseorder", newlyCreatedpurchaseOrderGuid);
                        if (preferrredSupplierId != null)
                            updateWorkOrderProduct["ap360_vendorid"] = new EntityReference("account", preferrredSupplierId.Id);

                        service.Update(updateWorkOrderProduct);

                        //Entity reterviedWOPMainBPFEntity = WOPMainBPF.ReteriveWOPMainBPFRelatedtoWOP(service, tracingService, new EntityReference("msdyn_workorderproduct",woProduct.guid));
                        //if (reterviedWOPMainBPFEntity != null)
                        //{
                        //    WOPMainBPF.UpdateWOPMainBPFStage(service, tracingService, reterviedWOPMainBPFEntity,new Guid("18c8448f-3e4b-4351-9d4e-407f9a8a9e6d"));//ProductStatus
                        //}

                    }
                    //Entity updatepurchaseOrder = new Entity("msdyn_purchaseorder");
                    //EntityReference opportunityRef = null;
                    //updatepurchaseOrder.Id = newlyCreatedpurchaseOrderGuid;
                    ////updatepurchaseOrder["msdyn_workorder"] = new EntityReference("msdyn_workorder", workOrderRef.Id);//Not using because of product inventory issue
                    //updatepurchaseOrder["ap360_workorderid"] = new EntityReference("msdyn_workorder", workOrderRef.Id);
                    //Entity workOrderEnt = null;
                    //workOrderEnt = service.Retrieve("msdyn_workorder", workOrderRef.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("msdyn_opportunityid"));
                    //if (workOrderEnt != null)
                    //{
                    //    opportunityRef = workOrderEnt.GetAttributeValue<EntityReference>("msdyn_opportunityid") != null ? workOrderEnt.GetAttributeValue<EntityReference>("msdyn_opportunityid") : null;
                    //    if (opportunityRef != null)
                    //        updatepurchaseOrder["ap360_opportunityid"] = new EntityReference("opportunity", opportunityRef.Id);




                    //}
                    //service.Update(updatepurchaseOrder);
                }


                context.OutputParameters["accountName"] = selelctedWorkOrderProductGuids.ToString();










            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}