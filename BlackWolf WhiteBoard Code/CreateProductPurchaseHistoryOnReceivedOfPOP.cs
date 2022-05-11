using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk.Query;

namespace BlackWolf_WhiteBoard_Code
{
    public class CreateProductPurchaseHistoryOnReceivedOfPOP : IPlugin
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

                if (context.MessageName.ToLower() == "update")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        entity = (Entity)context.InputParameters["Target"];

                        if (entity.LogicalName.ToLower() == "msdyn_purchaseorderproduct")
                        {
                            if (entity.Contains("ap360_itemsubstatus"))
                            {

                                var itemSubStatus = entity.GetAttributeValue<OptionSetValue>("ap360_itemsubstatus").Value;
                                if (itemSubStatus == 126300000) // Received
                                {
                                    //throw new InvalidPluginExecutionException("inside if ");
                                    Entity purchaseOrderProductEntity = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("msdyn_unitcost", "msdyn_product", "msdyn_quantity", "msdyn_totalcost", "msdyn_purchaseorder"));
                                    var unitCost = purchaseOrderProductEntity.GetAttributeValue<Money>("msdyn_unitcost") != null ? purchaseOrderProductEntity.GetAttributeValue<Money>("msdyn_unitcost").Value : 0;
                                    var productRef = purchaseOrderProductEntity.GetAttributeValue<EntityReference>("msdyn_product") ?? null;
                                    var quantity = purchaseOrderProductEntity.GetAttributeValue<double>("msdyn_quantity");
                                    var TotalCost = purchaseOrderProductEntity.GetAttributeValue<Money>("msdyn_totalcost") != null ? purchaseOrderProductEntity.GetAttributeValue<Money>("msdyn_totalcost").Value : 0;
                                    var purchaseOrderRef = purchaseOrderProductEntity.GetAttributeValue<EntityReference>("msdyn_purchaseorder") ?? null;
                                    Entity purchaseOrderEntity = service.Retrieve(purchaseOrderRef.LogicalName, purchaseOrderRef.Id, new ColumnSet("msdyn_vendor"));
                                    var vendor = purchaseOrderEntity.GetAttributeValue<EntityReference>("msdyn_vendor") ?? null;
                                    tracingService.Trace("Before creation of Purchase History");
                                    // Create Purcahse History Record.
                                    Entity purchaseHistory = new Entity("ap360_partspurchasehistory");
                                    if (productRef != null)
                                        purchaseHistory["ap360_product"] = new EntityReference(productRef.LogicalName, productRef.Id);
                                    if (unitCost != 0)
                                        purchaseHistory["ap360_cost"] = new Money(unitCost);
                                    purchaseHistory["ap360_quantity"] = quantity;
                                    if (TotalCost != 0)
                                        purchaseHistory["ap360_totalcost"] = new Money(TotalCost);
                                    purchaseHistory["ap360_purchasedate"] = DateTime.Now;
                                    if (vendor != null)
                                        purchaseHistory["ap360_vendorid"] = new EntityReference(vendor.LogicalName, vendor.Id);

                                    service.Create(purchaseHistory);

                                    Entity productEntity = new Entity(productRef.LogicalName, productRef.Id);
                                    if (unitCost != 0)
                                    {
                                        // productEntity["ap360_product"] = new EntityReference(product.LogicalName, product.Id);
                                        if (vendor != null)
                                            productEntity["msdyn_defaultvendor"] = new EntityReference(vendor.LogicalName, vendor.Id);
                                        productEntity["ap360_quantity"] = quantity;
                                        if (TotalCost != 0)
                                            productEntity["ap360_totalcost"] = new Money(TotalCost); ;
                                        productEntity["ap360_purchasedate"] = DateTime.Now;
                                        productEntity["ap360_cost"] = new Money(unitCost);
                                        productEntity["currentcost"] = new Money(unitCost);

                                        service.Update(productEntity);
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
    }
}