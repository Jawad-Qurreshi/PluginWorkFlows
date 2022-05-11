using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class ProductShippedToVendorAndInventoryAdjustment : IPlugin
    {
        private IOrganizationService _service;
        //private string plugin = "Product - Post Update";
        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                // throw new InvalidPluginExecutionException("MaintainPartsPurchaseHistory");

                ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = factory.CreateOrganizationService(context.UserId);

                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    if (context.MessageName.ToLower() == "update")
                    {
                        Entity entity = (Entity)context.InputParameters["Target"];
                        if (entity.LogicalName == "msdyn_rtv")
                        {
                            tracingService.Trace("inside rtv");
                            if (entity.Contains("msdyn_systemstatus"))
                            {
                                if (context.Depth > 1) return;
                                int rtvSystemStatus = entity.GetAttributeValue<OptionSetValue>("msdyn_systemstatus").Value;

                                if (rtvSystemStatus == 690970002)//Shipped
                                {
                                    List<RTV_Product> lstRTV_Products = new List<RTV_Product>();
                                    lstRTV_Products = RTV_Product.getRtVProduct(service, entity.Id);
                                    foreach (RTV_Product rtv_Product in lstRTV_Products)
                                    {
                                        tracingService.Trace("start of for each");
                                        #region Related To Inventory
                                        // Create Inventory Adjustment
                                        Entity newInventoryAdjust = new Entity("msdyn_inventoryadjustment");
                                        if (rtv_Product.WareHouse != null)
                                        {
                                            newInventoryAdjust["msdyn_warehouse"] = new EntityReference(rtv_Product.WareHouse.LogicalName, rtv_Product.WareHouse.Id);
                                        }
                                        newInventoryAdjust["msdyn_adjustmenttime"] = DateTime.Today;
                                        Guid newInventoryAdjustId = service.Create(newInventoryAdjust);
                                        tracingService.Trace("after new id");

                                        // Create Inventory Adjustment Product
                                        Entity newInventoryAdjustProduct = new Entity("msdyn_inventoryadjustmentproduct");
                                        if (rtv_Product.product != null)
                                        {
                                            newInventoryAdjustProduct["msdyn_product"] = new EntityReference(rtv_Product.product.LogicalName, rtv_Product.product.Id);

                                        }
                                        newInventoryAdjustProduct["msdyn_unit"] = new EntityReference("uom", new Guid("361a3eac-749c-4bb3-92a2-d63f692f61ba"));
                                        double quantity = rtv_Product.Quantity;
                                        //throw new InvalidPluginExecutionException(quantity.ToString());
                                        newInventoryAdjustProduct["msdyn_quantity"] = -quantity;

                                        newInventoryAdjustProduct["msdyn_inventoryadjustment"] = new EntityReference("msdyn_inventoryadjustment", newInventoryAdjustId);
                                        Guid newInventoryAdjustProductId = service.Create(newInventoryAdjustProduct);
                                        tracingService.Trace("end of for each");

                                        #endregion
                                    }

                                }


                            }
                        }
                    }
                }
            }


            catch (Exception ex)
            {       
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }
    }
}