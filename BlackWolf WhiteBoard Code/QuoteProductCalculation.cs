using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class QuoteProductCalculation : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();
            // throw new InvalidPluginExecutionException("QuoteProductCalculation");

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

                int quantity = 0;
                decimal unitCost = 0;

                if (context.MessageName.ToLower() == "update")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        entity = (Entity)context.InputParameters["Target"];

                        if (entity.LogicalName == "ap360_quoteproduct")
                        {
                            if ((entity.Contains("ap360_quantity") || entity.Contains("ap360_unitcost")) && context.Depth == 1)
                            {

                                tracingService.Trace("Inside Update of Quote Product");
                                Entity quoteproduct = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("ap360_unitcost", "ap360_quantity"));
                                unitCost = quoteproduct.GetAttributeValue<Money>("ap360_unitcost").Value;

                                quantity = quoteproduct.GetAttributeValue<int>("ap360_quantity");
                                //throw new InvalidPluginExecutionException(quantity.ToString());


                                entity["ap360_partsaleprice"] = new Money(quantity * unitCost);


                                service.Update(entity);
                            }

                            else if ((entity.Contains("ap360_quoteserviceid")))
                            {

                                Entity quoteproduct = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("ap360_unitcost", "ap360_quantity"));
                                unitCost = quoteproduct.GetAttributeValue<Money>("ap360_unitcost").Value;

                                quantity = quoteproduct.GetAttributeValue<int>("ap360_quantity");
                                //throw new InvalidPluginExecutionException(quantity.ToString());


                                entity["ap360_partsaleprice"] = new Money(quantity * unitCost);


                                service.Update(entity);
                            }
                            //if (entity.Contains("ap360_unitprice"))
                            //{
                            //    Entity quoteproduct = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("ap360_quantity"));
                            //    quantity = quoteproduct.GetAttributeValue<int>("ap360_quantity");

                            //}
                            //if (entity.Contains("ap360_quantity") || entity.Contains("ap360_unitprice"))
                            //{
                            //    unitPrice = entity.GetAttributeValue<Money>("ap360_unitprice").Value;
                            //    quantity = entity.GetAttributeValue<int>("ap360_quantity");
                            //    entity["ap360_partsaleprice"] = new Money(quantity * unitPrice);

                            //}
                        }

                    }
                }


                if (context.MessageName.ToLower() == "create")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        //  throw new InvalidPluginExecutionException("context "+ context.Depth.ToString());
                        //  if (context.Depth > 1) return;// this is important because for Revise Quotes we want to save updated value of Quote Product
                        Money multiplier = null;
                        bool tyreBatteryMarkUp = false;
                        entity = (Entity)context.InputParameters["Target"];
                        if (entity.LogicalName == "ap360_quoteproduct")
                        {
                            tracingService.Trace("");

                            Entity quoteproduct = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("ap360_unitcost", "ap360_multiplier", "ap360_quantity", "ap360_tyrebatterymarkup", "ap360_partsaleprice", "ap360_unitprice"));

                            quantity = quoteproduct.GetAttributeValue<int>("ap360_quantity");
                            unitCost = quoteproduct.GetAttributeValue<Money>("ap360_unitcost") != null ? quoteproduct.GetAttributeValue<Money>("ap360_unitcost").Value : 0;

                            if (unitCost == 0) return;

                            multiplier = quoteproduct.GetAttributeValue<Money>("ap360_multiplier") != null ? quoteproduct.GetAttributeValue<Money>("ap360_multiplier") : null;
                            tyreBatteryMarkUp = quoteproduct.GetAttributeValue<bool>("ap360_tyrebatterymarkup");
                            if (context.Depth == 1)
                            {
                                if (multiplier != null)
                                {
                                    if (tyreBatteryMarkUp == true)
                                    {
                                        entity["ap360_partsaleprice"] = new Money(Convert.ToDecimal(1.25) * unitCost);
                                        entity["ap360_unitprice"] = new Money(Convert.ToDecimal(1.25) * unitCost * quantity);
                                    }
                                    else {
                                        entity["ap360_partsaleprice"] = new Money(multiplier.Value * unitCost);
                                        entity["ap360_unitprice"] = new Money(multiplier.Value * unitCost * quantity);
                                    }
                                   

                                    service.Update(entity);
                                }
                            }
                            else if (context.Depth == 2)//this is important because for Revise Quotes From Quotes Button we want to save updated value of Quote Product
                            {
                                Money partsSalePrice = quoteproduct.GetAttributeValue<Money>("ap360_partsaleprice") != null ? quoteproduct.GetAttributeValue<Money>("ap360_partsaleprice") : null;
                                Money unitPrice = quoteproduct.GetAttributeValue<Money>("ap360_unitprice") != null ? quoteproduct.GetAttributeValue<Money>("ap360_unitprice") : null;

                                if (partsSalePrice != null)
                                {
                                    entity["ap360_partsaleprice"] = partsSalePrice;
                                }
                                if (unitPrice != null)
                                {
                                    entity["ap360_unitprice"] = unitPrice;
                                }
                                service.Update(entity);
                            }
                            //tracingService.Trace(new Money(quantity * unitCost).Value.ToString());


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