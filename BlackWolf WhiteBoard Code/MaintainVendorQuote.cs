using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class MaintainVendorQuote : IPlugin
    {
        private IOrganizationService _service;
        private string plugin = "Product - Post Update";
        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
             //   throw new InvalidPluginExecutionException("MaintainVendorQuote");

                ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = factory.CreateOrganizationService(context.UserId);

                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    if (context.MessageName.ToLower() == "update")
                    {
                        Entity entity = (Entity)context.InputParameters["Target"];

                        if (entity.LogicalName == "ap360_quoteproduct" || entity.LogicalName == "msdyn_workorderproduct")
                        {
                            if (entity.Contains("ap360_quotevendorid"))
                            {
                                if (context.Depth > 1) return;
                                tracingService.Trace("Inside Contain");
                                Entity sourceEntity = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("ap360_estimateddeliverydate", "ap360_cost", "ap360_quotevendorid", "ap360_vendorid"));

                                if (sourceEntity != null)
                                {
                                    tracingService.Trace(" Entity is not null");

                                    Entity vendorQuote = new Entity("ap360_vendorquote");
                                    EntityReference quotevendorRef = sourceEntity.GetAttributeValue<EntityReference>("ap360_quotevendorid") != null ? sourceEntity.GetAttributeValue<EntityReference>("ap360_quotevendorid") : null;
                                    if (quotevendorRef != null)
                                        vendorQuote["ap360_vendor"] = new EntityReference("account", quotevendorRef.Id);
                                    decimal cost = sourceEntity.GetAttributeValue<Money>("ap360_cost") != null ? sourceEntity.GetAttributeValue<Money>("ap360_cost").Value : 0.0m;
                                    vendorQuote["ap360_cost"] = new Money(cost);
                                    vendorQuote["ap360_estimateddeliverydate"] = sourceEntity.GetAttributeValue<DateTime>("ap360_estimateddeliverydate");
                                    EntityReference vendorRef = sourceEntity.GetAttributeValue<EntityReference>("ap360_vendorid") != null ? sourceEntity.GetAttributeValue<EntityReference>("ap360_vendorid") : null;

                                    if (entity.LogicalName == "ap360_quoteproduct")
                                        vendorQuote["ap360_quoteproductid"] = new EntityReference("ap360_quoteproduct", entity.Id);
                                    else if (entity.LogicalName == "msdyn_workorderproduct")
                                        vendorQuote["ap360_workorderproduct"] = new EntityReference("msdyn_workorderproduct", entity.Id);

                                    Guid newlyCreatedVendorQuoteGuid = service.Create(vendorQuote);

                                    if (vendorRef == null)// attach first vendor  to Entity (Quote Product, WO product)
                                    {
                                        Entity updateEntity = new Entity(entity.LogicalName);
                                        updateEntity.Id = entity.Id;
                                        if (quotevendorRef != null)
                                            updateEntity["ap360_vendorid"] = new EntityReference("account", quotevendorRef.Id);
                                        service.Update(updateEntity);
                                    }
                                    tracingService.Trace("Vendor Quote Created");



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

            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }
    }
}
