using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class DeleteCancelledQuoteProjectTasks : IPlugin
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


                //  tracingService.Trace("MoveQuoteServiceItemstoDifferentQuoteService");
                if (context.MessageName.ToLower() == "update")
                {

                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        entity = (Entity)context.InputParameters["Target"];
                        if (entity.LogicalName == "quote")
                        {
                            if ((entity.Contains("statuscode")))
                            {
                                if (entity.GetAttributeValue<OptionSetValue>("statuscode").Value == 6)//Closed-Canceled
                                {
                                    ProjectTask projectTask = null;

                                    projectTask = ProjectTask.GetProjectTaskRelatedToQuote(service, tracingService, entity.Id);
                                    if (projectTask != null)
                                        service.Delete("msdyn_projecttask", projectTask.guid);

                                    //List<QuoteService> lstQuoteServices = new List<QuoteService>();
                                    //lstQuoteServices = QuoteService.GetQuoteServices(service, tracingService, entity.Id);
                                    //ProjectTask projectTask = null;
                                    //foreach (QuoteService quoteService in lstQuoteServices)
                                    //{
                                    //    projectTask = new ProjectTask();
                                    //    projectTask = ProjectTask.GetProjectTaskRelatedToQuoteService(service, tracingService, quoteService.guid);
                                    //    service.Delete("msdyn_projecttask", projectTask.guid);

                                    //}

                                    tracingService.Trace("Plugin Ended");
                                }
                            }
                        }

                    }
                }

                //throw new InvalidPluginExecutionException("Cutom error");
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}