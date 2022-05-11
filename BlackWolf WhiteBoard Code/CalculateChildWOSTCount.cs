using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class CalculateChildWOSTCount : IPlugin
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
                    tracingService.Trace("WOST created in 37 Line");
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        tracingService.Trace("context.InputParameters.Contains('Target') && context.InputParameters['Target'] is Entity");
                        entity = (Entity)context.InputParameters["Target"];

                        if (entity.LogicalName == "msdyn_workorderservicetask")
                        {
                            tracingService.Trace("entity.LogicalName == 'msdyn_workorderservicetask'");
                            if (entity.Contains("ap360_workorderservicetaskid")) {
                                tracingService.Trace("entity.Contains('ap360_workorderservicetaskid')");
                                EntityReference WOSTRef = entity.GetAttributeValue<EntityReference>("ap360_workorderservicetaskid") ?? null;
                                if (WOSTRef != null) {
                                    entity.Contains("WOST != null");
                                    int count = WorkOrderServiceTask.getWOSTCount(service, tracingService, WOSTRef);
                                    Entity WOST = new Entity(WOSTRef.LogicalName, WOSTRef.Id);
                                    WOST["ap360_noofchildwost"] = count;
                                    service.Update(WOST);
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