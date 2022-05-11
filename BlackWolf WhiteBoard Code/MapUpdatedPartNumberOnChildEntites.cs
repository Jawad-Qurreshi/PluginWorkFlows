using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class MapUpdatedPartNumberOnChildEntites : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                //  throw new InvalidPluginExecutionException("WorkOrderServcieTaskCalculation");

                ITracingService tracingService =
                  (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                IPluginExecutionContext context = (IPluginExecutionContext)
                    serviceProvider.GetService(typeof(IPluginExecutionContext));



                Entity entity = null;



                if (context.MessageName.ToLower() == "update")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        entity = (Entity)context.InputParameters["Target"];

                        if (entity.LogicalName == "product")
                        {

                            if (entity.Contains("ap360_partnumber"))
                            {
                                IOrganizationServiceFactory factory =
                                     (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                                IOrganizationService service = factory.CreateOrganizationService(context.UserId);

                                //  throw new InvalidPluginExecutionException("Custom error ");
                                var partNumber = entity.GetAttributeValue<string>("ap360_partnumber");
                                Guid productid = entity.Id;

                                //Update part number on purchase order product
                                EntityCollection popCollection = PurchaseOrderProduct.RetrivePOPBasedOnProduct(service, tracingService, productid);

                                if (popCollection != null)
                                {
                                    if (popCollection.Entities.Count > 0)
                                    {
                                        foreach (Entity pop in popCollection.Entities)
                                        {
                                            Entity updatePOProduct = new Entity(pop.LogicalName);
                                            updatePOProduct.Id = pop.Id;
                                            updatePOProduct["ap360_partnumber"] = partNumber;                                            
                                            service.Update(updatePOProduct);
                                        }
                                    }
                                }

                                //Update part number on Product Inventory
                                EntityCollection inventoryCollection = ProductInventory.RetrieveProductInventoryBasedOnProduct(service, tracingService, productid);

                                if (inventoryCollection != null)
                                {
                                    if (inventoryCollection.Entities.Count > 0)
                                    {
                                        foreach (Entity productInventory in inventoryCollection.Entities)
                                        {
                                            Entity updateProductInventory = new Entity(productInventory.LogicalName);
                                            updateProductInventory.Id = productInventory.Id;
                                            updateProductInventory["ap360_partnumber"] = partNumber;                                        
                                            service.Update(updateProductInventory);
                                        }
                                    }
                                }

                                EntityCollection quoteProdutCollection = QuoteProduct.RetriveQuoteProductBasedOnProduct(service, tracingService, productid);
                                tracingService.Trace(quoteProdutCollection.ToString());
                                if (quoteProdutCollection != null)
                                {
                                    if (quoteProdutCollection.Entities.Count > 0)
                                    {
                                        foreach (Entity quoteProduct in quoteProdutCollection.Entities)
                                        {
                                            Entity updateQuoteProduct = new Entity(quoteProduct.LogicalName);
                                            updateQuoteProduct.Id = quoteProduct.Id;
                                            updateQuoteProduct["ap360_partnumber"] = partNumber;

                                            service.Update(updateQuoteProduct);
                                        }
                                    }
                                }
                                tracingService.Trace("getting  values");
                                // throw new InvalidPluginExecutionException(actualAmount.Value.ToString() + "post and pre amount " + preActualAmount.Value.ToString());

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
