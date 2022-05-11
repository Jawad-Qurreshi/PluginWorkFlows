using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class CountQuoteServiceItems : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();

            try
            {
                //  throw new InvalidPluginExecutionException("WorkOrderServcieTaskCalculation");

                ITracingService tracingService =
                  (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                IPluginExecutionContext context = (IPluginExecutionContext)
                    serviceProvider.GetService(typeof(IPluginExecutionContext));

                IOrganizationServiceFactory factory =
                     (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = factory.CreateOrganizationService(context.UserId);


                Entity entity = null;                

                if (context.MessageName.ToLower() == "create")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                              //  throw new InvalidPluginExecutionException("testing");
                        entity = (Entity)context.InputParameters["Target"];

                        if (context.Stage == 20)//Pre
                        {
                            tracingService.Trace("Pre step CountQuoteServiceItems");
                            if (entity.LogicalName == "ap360_quoteservicetask" || entity.LogicalName == "ap360_quoteproduct" || entity.LogicalName == "ap360_quotesublet")
                            {
                                EntityReference quoteServiceRef = null;


                                if (entity.LogicalName == "ap360_quoteservicetask")
                                {

                                    quoteServiceRef = entity.GetAttributeValue<EntityReference>("ap360_quoteserviceid") != null ? entity.GetAttributeValue<EntityReference>("ap360_quoteserviceid") : null;
                                    Entity reterviedQuoteService = service.Retrieve(quoteServiceRef.LogicalName, quoteServiceRef.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("ap360_quoteserviceitemscount"));
                                    if (quoteServiceRef == null) return;
                                    
                                    int quoteserviceitemscount = reterviedQuoteService.GetAttributeValue<int>("ap360_quoteserviceitemscount");
                                    if (quoteserviceitemscount >= 45)
                                    {
                                        throw new InvalidPluginExecutionException("Can't create QuoteService Task,Maximum number of QuoteService items are 45");
                                    }
                                    else
                                    {                                        
                                        tracingService.Trace("items are less than 60");
                                        Entity updateQuoteService = new Entity(quoteServiceRef.LogicalName, quoteServiceRef.Id);
                                        updateQuoteService["ap360_quoteserviceitemscount"] = ++quoteserviceitemscount;                                        
                                        service.Update(updateQuoteService);
                                    }

                                }
                                else if (entity.LogicalName == "ap360_quoteproduct")
                                {
                                    tracingService.Trace("Pre step ap360_quoteproduct");


                                    quoteServiceRef = entity.GetAttributeValue<EntityReference>("ap360_quoteserviceid") != null ? entity.GetAttributeValue<EntityReference>("ap360_quoteserviceid") : null;
                                    if (quoteServiceRef == null) return;

                                    Entity reterviedQuoteService = service.Retrieve(quoteServiceRef.LogicalName, quoteServiceRef.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("ap360_quoteserviceitemscount"));
                                    int quoteserviceitemscount = reterviedQuoteService.GetAttributeValue<int>("ap360_quoteserviceitemscount");
                                    if (quoteserviceitemscount >= 45)
                                    {
                                        throw new InvalidPluginExecutionException("Can't create Quote Product,Maximum number of QuoteService items are 45");
                                    }
                                    else
                                    {
                                        tracingService.Trace("items are less than 60");
                                        Entity updateQuoteService = new Entity(quoteServiceRef.LogicalName, quoteServiceRef.Id);
                                        updateQuoteService["ap360_quoteserviceitemscount"] = ++quoteserviceitemscount;
                                        service.Update(updateQuoteService);
                                    }


                                }
                                else if (entity.LogicalName == "ap360_quotesublet")
                                {
                                    tracingService.Trace("Pre step ap360_quotesublet");


                                    quoteServiceRef = entity.GetAttributeValue<EntityReference>("ap360_quoteserviceid") != null ? entity.GetAttributeValue<EntityReference>("ap360_quoteserviceid") : null;
                                    if (quoteServiceRef == null) return;
                                    Entity reterviedQuoteService = service.Retrieve(quoteServiceRef.LogicalName, quoteServiceRef.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("ap360_quoteserviceitemscount"));
                                    int quoteserviceitemscount = reterviedQuoteService.GetAttributeValue<int>("ap360_quoteserviceitemscount");
                                    if (quoteserviceitemscount >= 45)
                                    {
                                        throw new InvalidPluginExecutionException("Can't create Quote Sublet,Maximum number of QuoteService items are 45");
                                    }
                                    else
                                    {
                                        tracingService.Trace("items are less than 60");
                                        Entity updateQuoteService = new Entity(quoteServiceRef.LogicalName, quoteServiceRef.Id);
                                        updateQuoteService["ap360_quoteserviceitemscount"] = ++quoteserviceitemscount;
                                        service.Update(updateQuoteService);
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