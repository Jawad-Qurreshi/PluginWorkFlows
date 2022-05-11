using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{

    //Not is use because ManageProjectTasksForWorkOrder has same funtionality
    class DeleteWorkOrderProjectTask : IPlugin
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


                EntityReference entity = null;


                //  tracingService.Trace("MoveQuoteServiceItemstoDifferentQuoteService");
                if (context.MessageName.ToLower() == "delete")
                {
                    //Not is use because ManageProjectTasksForWorkOrder has same funtionality

                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference)
                        {
                            entity = (EntityReference)context.InputParameters["Target"];
                   // throw new InvalidPluginExecutionException(entity.LogicalName);
                            if (entity.LogicalName == "msdyn_workorder")
                            {
                            //Not is use because ManageProjectTasksForWorkOrder has same funtionality

                            ProjectTask projectTask = null;

                           projectTask = ProjectTask.GetProjectTaskRelatedToWorkOrder(service, tracingService, entity.Id);
                            if (projectTask != null)
                            {
                                service.Delete("msdyn_projecttask", projectTask.guid);
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