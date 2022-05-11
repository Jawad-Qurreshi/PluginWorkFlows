using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class ManageInventoryAdjustmentForCustomerPart : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();
            //   throw new InvalidPluginExecutionException("CreateProductFromDescriptionAndRelatetoProductFamily");

            try
            {

                ITracingService tracingService =
                  (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                IPluginExecutionContext context = (IPluginExecutionContext)
                    serviceProvider.GetService(typeof(IPluginExecutionContext));

                IOrganizationServiceFactory factory =
                     (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = factory.CreateOrganizationService(context.UserId);
               

                if (context.MessageName.ToLower() == "ap360_adjustinventoryforcustomersuppliedparts")
                {
                    string entityId = (string)context.InputParameters["entityGUID"];
                    string entityName = (string)context.InputParameters["entityName"];

                   
                    Entity reterviedWOProduct = service.Retrieve(entityName, new Guid(entityId), new Microsoft.Xrm.Sdk.Query.ColumnSet("msdyn_estimatequantity", "ap360_product", "msdyn_warehouse", "msdyn_unit", "ap360_workorderproducttype"));
                  
                   
                    if (reterviedWOProduct.GetAttributeValue<OptionSetValue>("ap360_workorderproducttype") != null)
                    {
              
                     

                        AdjustInventory(service, reterviedWOProduct);
                    }
                }



                else if (context.Depth == 2)
                {
                    Entity entity = null;
                    if (context.MessageName.ToLower() == "create")
                    {
                        if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                        {
                            entity = (Entity)context.InputParameters["Target"];
                            if (entity.LogicalName == "msdyn_workorderproduct")
                            {
                                //Customer Supplied  126,300,000
                                //BlackWolf Inventory  126,300,001
                                //Vendor Supplied  126,300,002

                                if (entity.GetAttributeValue<OptionSetValue>("ap360_workorderproducttype").Value == 126300000)//Customer Supplied
                                {
                                    if (entity.GetAttributeValue<OptionSetValue>("ap360_workorderproducttype") != null)
                                    {

                                        AdjustInventory(service, entity);

                                    }

                                }
                            }
                        }
                    }

                }



                //if (context.MessageName.ToLower() == "update")
                //{
                //    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                //    {
                //        if (context.Depth == 1)
                //        {
                //            entity = (Entity)context.InputParameters["Target"];

                //            if (entity.LogicalName == "msdyn_workorderproduct")
                //            {
                //                tracingService.Trace("Entity Name " + entity.LogicalName);

                //                if (entity.Contains("ap360_iscustomerpart"))
                //                {
                //                    tracingService.Trace("Approve Product Updated");
                //                    //  CreateProductAndUpdateEntity(service, tracingService, entity);

                //                    tracingService.Trace("Plugin Ended Successfully");

                //                }

                //            }
                //        }
                //    }
                //}



            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void AdjustInventory(IOrganizationService service, Entity WOPEntity)
        {
            double msdyn_estimatequantity = WOPEntity.GetAttributeValue<double>("msdyn_estimatequantity");
            EntityReference ap360_product = WOPEntity.GetAttributeValue<EntityReference>("ap360_product") != null ? WOPEntity.GetAttributeValue<EntityReference>("ap360_product") : null;
            EntityReference msdyn_warehouse = WOPEntity.GetAttributeValue<EntityReference>("msdyn_warehouse") != null ? WOPEntity.GetAttributeValue<EntityReference>("msdyn_warehouse") : null;
            EntityReference msdyn_unit = WOPEntity.GetAttributeValue<EntityReference>("msdyn_unit") != null ? WOPEntity.GetAttributeValue<EntityReference>("msdyn_unit") : null;

           // throw new InvalidPluginExecutionException("yes retrieved"+ ap360_product.Name.ToString());
            if (ap360_product == null)
            {
                throw new InvalidPluginExecutionException("Product is not selected ");
            }
            else if (msdyn_warehouse == null)
            {
                throw new InvalidPluginExecutionException("Ware House is not selected");
            }
            else
            {
                Guid newInventoryAdjustId = InventoryAdjustment.createInventoryAdjustment(service, msdyn_warehouse);

                InventoryAdjustmentProduct.createInventoryAdjustmentProduct(service, ap360_product, msdyn_unit, msdyn_estimatequantity, newInventoryAdjustId);

                Entity updateWorkOrderProductEntity1 = new Entity(WOPEntity.LogicalName);
                updateWorkOrderProductEntity1.Id = WOPEntity.Id;
                updateWorkOrderProductEntity1["msdyn_allocated"] = false;
                service.Update(updateWorkOrderProductEntity1);
            }
        }


    }
}


