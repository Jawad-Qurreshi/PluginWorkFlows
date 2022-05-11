using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class CreatePurchaseOrderForWOSublets : IPlugin
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


                EntityReference workOrderRef = null;

                tracingService.Trace("Before variable");
                EntityReference preferrredSupplierId = (EntityReference)context.InputParameters["preferredSupplierId"];
                string selelctedWorkOrderSubletGuids = (string)context.InputParameters["selelctedWorkOrderSubletsGuid"];
                // throw new InvalidPluginExecutionException($"Stop: {preferrredSupplierId.Name}:{selelctedWorkOrderSubletGuids}");
                List<string> lstWorkOrderSubletsIds = selelctedWorkOrderSubletGuids.Split(',').ToList<string>();
                tracingService.Trace("Afer varialbes");
                Guid newlyCreatedpurchaseOrderGuid;
                if (lstWorkOrderSubletsIds.Count > 0)
                {
                    Entity purchaseOrder = new Entity("msdyn_purchaseorder");
                    tracingService.Trace("Lst of WOSublets " + lstWorkOrderSubletsIds.Count.ToString());

                    if (preferrredSupplierId != null)
                        purchaseOrder["msdyn_vendor"] = new EntityReference("account", preferrredSupplierId.Id);
                    purchaseOrder["msdyn_purchaseorderdate"] = DateTime.Now.Date;

                    //purchaseOrder["ap360_opportunityid"] = new EntityReference("opportunity", quote.Opportunity.Id);
                    //purchaseOrder["msdyn_workorder"] = new EntityReference("msdyn_workorder", workOrderGuid);
                    newlyCreatedpurchaseOrderGuid = service.Create(purchaseOrder);
                    tracingService.Trace("Afer Creating Purchase order");

                    foreach (string workOrderSubletId in lstWorkOrderSubletsIds)
                    {
                        Entity WorkOrderSublet = service.Retrieve("ap360_workordersublet", new Guid(workOrderSubletId.ToString()), new Microsoft.Xrm.Sdk.Query.ColumnSet(true));

                        if (WorkOrderSublet != null)
                        {
                            tracingService.Trace("Work Order Sublet");

                            //throw new InvalidPluginExecutionException("Stop 1:" + WorkOrderSublet.Id);
                            Entity purchaseOrderProduct = new Entity("msdyn_purchaseorderproduct");
                            purchaseOrderProduct["msdyn_product"] = new EntityReference("product", new Guid("e077979d-86a4-ea11-a812-000d3a33f47e"));

                            purchaseOrderProduct["msdyn_purchaseorder"] = new EntityReference("msdyn_purchaseorder", newlyCreatedpurchaseOrderGuid);
                            //throw new InvalidPluginExecutionException("Stop4:");
                            workOrderRef = WorkOrderSublet.GetAttributeValue<EntityReference>("ap360_workorderid") != null ? WorkOrderSublet.GetAttributeValue<EntityReference>("ap360_workorderid") : null;
                            if (workOrderRef != null)
                            {
                                purchaseOrderProduct["msdyn_associatetoworkorder"] = new EntityReference("msdyn_workorder", workOrderRef.Id);
                            }
                            else
                            {
                                throw new InvalidPluginExecutionException("Work Order not found.");
                            }
                            string Manufacturer = WorkOrderSublet.GetAttributeValue<string>("ap360_manufacturer") != null ? WorkOrderSublet.GetAttributeValue<string>("ap360_manufacturer") : null;
                            string BarCode = WorkOrderSublet.GetAttributeValue<string>("ap360_barcode") != null ? WorkOrderSublet.GetAttributeValue<string>("ap360_barcode") : null;
                            int sku = WorkOrderSublet.GetAttributeValue<int>("ap360_sku");
                            EntityReference vendor = WorkOrderSublet.GetAttributeValue<EntityReference>("ap360_vendorid") != null ? WorkOrderSublet.GetAttributeValue<EntityReference>("ap360_vendorid") : null;
                            EntityReference unit = WorkOrderSublet.GetAttributeValue<EntityReference>("msdyn_unit") != null ? WorkOrderSublet.GetAttributeValue<EntityReference>("msdyn_unit") : null;
                            //EntityReference product = WorkOrderSublet.GetAttributeValue<EntityReference>("ap360_product") != null ? WorkOrderSublet.GetAttributeValue<EntityReference>("ap360_product") : null;
                            EntityReference wareHouse = WorkOrderSublet.GetAttributeValue<EntityReference>("msdyn_warehouse") != null ? WorkOrderSublet.GetAttributeValue<EntityReference>("msdyn_warehouse") : null;
                            EntityReference opportunity = WorkOrderSublet.GetAttributeValue<EntityReference>("ap360_opportunityid") != null ? WorkOrderSublet.GetAttributeValue<EntityReference>("ap360_opportunityid") : null;
                            string opportunityAutoNumber = "";
                            if (opportunity != null)
                            {

                                Entity reterviedOpportunity = service.Retrieve(opportunity.LogicalName, opportunity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("ap360_opportunityautonumber"));

                                if (reterviedOpportunity != null)
                                {
                                    opportunityAutoNumber = reterviedOpportunity.GetAttributeValue<string>("ap360_opportunityautonumber");
                                }
                            }

                            // throw new InvalidPluginExecutionException("Stop7:");

                            Money UnitCost = null;
                            UnitCost = WorkOrderSublet.GetAttributeValue<Money>("ap360_subletcost") != null ? WorkOrderSublet.GetAttributeValue<Money>("ap360_subletcost") : null;

                            //if (WorkOrderSublet.GetAttributeValue<Money>("ap360_revisedestimatedamount") != null)
                            //{
                            //    UnitCost = WorkOrderSublet.GetAttributeValue<Money>("ap360_revisedestimatedamount") != null ? WorkOrderSublet.GetAttributeValue<Money>("ap360_revisedestimatedamount") : null;
                            //}
                            //else if (WorkOrderSublet.GetAttributeValue<Money>("msdyn_estimateunitcost") != null)
                            //{
                            //    UnitCost = WorkOrderSublet.GetAttributeValue<Money>("msdyn_estimateunitcost") != null ? WorkOrderSublet.GetAttributeValue<Money>("msdyn_estimateunitcost") : null;
                            //}
                            if (opportunity != null)
                            {
                                purchaseOrderProduct["ap360_opportunityid"] = new EntityReference(opportunity.LogicalName, opportunity.Id);
                                purchaseOrderProduct["ap360_opportunityautonumber"] = opportunityAutoNumber;
                            }

                            string subletdescription = WorkOrderSublet.GetAttributeValue<string>("ap360_subletdescription");
                            purchaseOrderProduct["ap360_workordersubletid"] = new EntityReference(WorkOrderSublet.LogicalName, WorkOrderSublet.Id);

                            purchaseOrderProduct["msdyn_description"] = subletdescription;

                            //throw new InvalidPluginExecutionException("Stop2:");
                            if (UnitCost != null)
                                purchaseOrderProduct["msdyn_unitcost"] = new Money(UnitCost.Value);
                            //we don"t need to map unit price in Purchase Order Product
                            double quantity = 1;
                            purchaseOrderProduct["msdyn_quantity"] = quantity; //WorkOrderSublet.GetAttributeValue<double>("msdyn_estimatequantity");
                            //throw new InvalidPluginExecutionException("Stop1:");
                            if (Manufacturer != null)
                                purchaseOrderProduct["ap360_manufacturer"] = Manufacturer;
                            if (BarCode != null)
                                purchaseOrderProduct["ap360_barcode"] = BarCode;
                            if (vendor != null)
                                purchaseOrderProduct["ap360_vendorid"] = new EntityReference("account", vendor.Id);
                            if (unit != null)
                                purchaseOrderProduct["msdyn_unit"] = new EntityReference("uom", unit.Id);
                            else
                                purchaseOrderProduct["msdyn_unit"] = new EntityReference("uom", new Guid("361a3eac-749c-4bb3-92a2-d63f692f61ba"));//UNIT
                            //if (product != null)
                            //    purchaseOrderProduct["msdyn_product"] = new EntityReference("product", product.Id);
                            //else
                            //    throw new InvalidPluginExecutionException("Product is not selected for WorkOrder Product");

                            //if (wareHouse != null)
                            purchaseOrderProduct["msdyn_associatetowarehouse"] = new EntityReference("msdyn_warehouse", new Guid("5b743789-c329-41ee-89e5-f81b83570131"));
                            //else
                            //      throw new InvalidPluginExecutionException("WareHouse is not selected for WorkOrder Product");


                            purchaseOrderProduct["ap360_sku"] = sku;

                            // throw new InvalidPluginExecutionException("Under Development....");
                            service.Create(purchaseOrderProduct);

                        }

                    }
                    tracingService.Trace("After First Foreach");

                    foreach (string workOrderSubletId in lstWorkOrderSubletsIds)
                    {
                        Entity WorkOrderSublet = new Entity("ap360_workordersublet");
                        WorkOrderSublet.Id = new Guid(workOrderSubletId);
                        WorkOrderSublet["ap360_purchaseorderid"] = new EntityReference("msdyn_purchaseorder", newlyCreatedpurchaseOrderGuid);
                        if (preferrredSupplierId != null)
                            WorkOrderSublet["ap360_vendorid"] = new EntityReference("account", preferrredSupplierId.Id);
                        service.Update(WorkOrderSublet);

                    }

                    Entity updatepurchaseOrder = new Entity("msdyn_purchaseorder");
                    EntityReference opportunityRef = null;
                    updatepurchaseOrder.Id = newlyCreatedpurchaseOrderGuid;
                    updatepurchaseOrder["msdyn_workorder"] = new EntityReference("msdyn_workorder", workOrderRef.Id);
                    updatepurchaseOrder["ap360_workorderid"] = new EntityReference("msdyn_workorder", workOrderRef.Id);

                    Entity workOrderEnt = null;
                    workOrderEnt = service.Retrieve("msdyn_workorder", workOrderRef.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("msdyn_opportunityid"));
                    if (workOrderEnt != null)
                    {
                        opportunityRef = workOrderEnt.GetAttributeValue<EntityReference>("msdyn_opportunityid") != null ? workOrderEnt.GetAttributeValue<EntityReference>("msdyn_opportunityid") : null;
                        if (opportunityRef != null)
                        {
                            updatepurchaseOrder["ap360_opportunityid"] = new EntityReference("opportunity", opportunityRef.Id);
                        }

                    }
                    service.Update(updatepurchaseOrder);
                    //throw new InvalidPluginExecutionException("Stop23");
                }

                tracingService.Trace("Output parameter " + selelctedWorkOrderSubletGuids.ToString());
                context.OutputParameters["accountName"] = selelctedWorkOrderSubletGuids.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}