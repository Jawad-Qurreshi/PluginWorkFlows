using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class MaintainPartsPurchaseHistory : IPlugin
    {
        private IOrganizationService _service;
        private string plugin = "Product - Post Update";
        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
              //  throw new InvalidPluginExecutionException("MaintainPartsPurchaseHistory");

                ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = factory.CreateOrganizationService(context.UserId);

                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    if (context.MessageName.ToLower() == "update")
                    {
                        Entity entity = (Entity)context.InputParameters["Target"];

                        // Triggers in case of Updation of Purchase Date only
                        if (entity.Contains("ap360_purchasedate"))
                        {
                            if (context.Depth > 1) return;
                            tracingService.Trace("Inside Contain");
                            Entity productEntity = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("msdyn_defaultvendor", "ap360_quantity", "ap360_totalcost", "ap360_cost", "ap360_purchasedate"));
                            //if (
                            //       productEntity.GetAttributeValue<EntityReference>("msdyn_defaultvendor") != null
                            //    && productEntity.GetAttributeValue<int>("ap360_quantity") != 0
                            //    && productEntity.GetAttributeValue<Money>("ap360_cost").Value != 0
                            //    && productEntity.GetAttributeValue<Money>("ap360_totalcost").Value != 0
                            //    && productEntity.GetAttributeValue<DateTime>("ap360_purchasedate") != null
                            //    )
                            //{
                            // Set new record paraemters
                            if (productEntity != null)
                            {
                                tracingService.Trace("Poduct Entity is not null");

                                Entity partsPurchaseHistory = new Entity("ap360_partspurchasehistory");
                                partsPurchaseHistory["ap360_name"] = "MaintainPartsPurchaseHistory";
                                EntityReference vendorRef = productEntity.GetAttributeValue<EntityReference>("msdyn_defaultvendor") != null ? productEntity.GetAttributeValue<EntityReference>("msdyn_defaultvendor") : null;
                                if (vendorRef != null)
                                    partsPurchaseHistory["ap360_vendorid"] = new EntityReference("account", vendorRef.Id);
                                int quantity = productEntity.GetAttributeValue<int>("ap360_quantity");
                                partsPurchaseHistory["ap360_quantity"] = quantity;
                                decimal cost = productEntity.GetAttributeValue<Money>("ap360_cost") != null ? productEntity.GetAttributeValue<Money>("ap360_cost").Value : 0.0m;
                                partsPurchaseHistory["ap360_cost"] = new Money(cost);
                                decimal TotalCost = productEntity.GetAttributeValue<Money>("ap360_totalcost") != null ? productEntity.GetAttributeValue<Money>("ap360_totalcost").Value : 0.0m;

                                partsPurchaseHistory["ap360_totalcost"] = new Money(TotalCost);
                                partsPurchaseHistory["ap360_purchasedate"] = productEntity.GetAttributeValue<DateTime>("ap360_purchasedate");
                                partsPurchaseHistory["ap360_product"] = new EntityReference("product", entity.Id);

                                Guid partsPurchaseHistoryid = service.Create(partsPurchaseHistory);
                                tracingService.Trace("Parts purchase History Created");



                                // Make products fields empty.
                                //Entity updateproductEntity = new Entity(entity.LogicalName);
                                //updateproductEntity.Id = entity.Id;
                                //updateproductEntity["msdyn_defaultvendor"] = null;
                                //updateproductEntity["ap360_quantity"] = null;
                                //updateproductEntity["ap360_cost"] = 0;
                                //updateproductEntity["ap360_purchasedate"] = null;
                                //_service.Update(updateproductEntity);
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
