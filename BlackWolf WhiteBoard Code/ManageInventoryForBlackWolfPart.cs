using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class ManageInventoryForBlackWolfPart : IPlugin
    {

        public void Execute(IServiceProvider serviceProvider)
        {
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


                Entity entity = null;


                //if (context.MessageName.ToLower() == "create")
                //{
                //    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                //    {
                //        entity = (Entity)context.InputParameters["Target"];



                //    }
                //}

                if (context.MessageName.ToLower() == "update")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        entity = (Entity)context.InputParameters["Target"];

                        if (entity.LogicalName == "msdyn_workorderproduct")
                        {
                            //  if (context.Depth > 1) return;

                            if (entity.Contains("msdyn_linestatus"))
                            {
                         //  throw new InvalidPluginExecutionException("Thiaxas test");
                                tracingService.Trace("entity.Contains(msdyn_linestatus)");

                                Entity preImage = (Entity)context.PreEntityImages["Image"];
                              

                                if (preImage.Contains("msdyn_linestatus"))
                                {
                                    tracingService.Trace("image.Contains(msdyn_linestatus)");
                                    int preImageLineStatus = preImage.GetAttributeValue<OptionSetValue>("msdyn_linestatus").Value;
                                    int preImageWOPType = preImage.GetAttributeValue<OptionSetValue>("ap360_workorderproducttype").Value;
                                    int currentLineStatus = entity.GetAttributeValue<OptionSetValue>("msdyn_linestatus").Value;

                                    if (preImageLineStatus == 690970001 && currentLineStatus == 126300000 && preImageWOPType == 126300001)//Used && Cancelled && blackwolf part
                                    {
                                        tracingService.Trace("condition true");
                                        Entity reterviedWOProduct = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("msdyn_estimatequantity", "ap360_product", "msdyn_warehouse", "msdyn_unit", "ap360_workorderproducttype"));
                                        tracingService.Trace("entity retrieved");
                                        AdjustInventory(service, reterviedWOProduct);
                                        tracingService.Trace("inventory adjusted");
                                    }

                                    
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
