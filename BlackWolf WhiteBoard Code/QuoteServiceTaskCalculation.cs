using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class QuoteServiceTaskCalculation : IPlugin
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
                decimal hourlyRate = 0;


                if (context.MessageName.ToLower() == "create")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        //  throw new InvalidPluginExecutionException("testing");
                        entity = (Entity)context.InputParameters["Target"];

                        if (context.Stage == 20)//Pre
                        {
                            tracingService.Trace("Pre step quoteservice calculation");
                            if (entity.LogicalName == "ap360_quoteservicetask")
                            {
                                hourlyRate = entity.GetAttributeValue<Money>("ap360_hourlyrate").Value;
                                int estimatedtime = entity.GetAttributeValue<int>("ap360_estimatedtime");                                
                                decimal estimatedTimeInDecimal = Convert.ToDecimal(estimatedtime);
                                entity["ap360_estimatedlaboramount"] = new Money((hourlyRate * estimatedTimeInDecimal) / 60);                              
                            }
                        }
                    }
                }
                if (context.MessageName.ToLower() == "update")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        //  throw new InvalidPluginExecutionException("testing");
                        entity = (Entity)context.InputParameters["Target"];

                        if (context.Stage == 40)//Post
                        {
                            tracingService.Trace("Pre step quoteservice calculation");
                            if (entity.LogicalName == "ap360_quoteservicetask")
                            {                                    
                                if (entity.Contains("ap360_estimatedtime") || entity.Contains("ap360_hourlyrate"))
                                {
                                    Entity postImage = (Entity)context.PostEntityImages["Image"];
                                    int estimatedtime = postImage.GetAttributeValue<int>("ap360_estimatedtime");
                                    hourlyRate = postImage.GetAttributeValue<Money>("ap360_hourlyrate").Value;
                                    Entity updateQuoteServiceTask = new Entity(entity.LogicalName, entity.Id);
                                    decimal estimatedTimeInDecimal = Convert.ToDecimal(estimatedtime);
                                    updateQuoteServiceTask["ap360_estimatedlaboramount"] = new Money((hourlyRate * estimatedTimeInDecimal) / 60);
                                    service.Update(updateQuoteServiceTask);
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