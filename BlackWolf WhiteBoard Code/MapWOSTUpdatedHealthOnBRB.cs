using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class MapWOSTUpdatedHealthOnBRB : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();
            //  throw new InvalidPluginExecutionException("ApprovedRevisedItem");

            try
            {

                ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = factory.CreateOrganizationService(context.UserId);

                Entity entity = null;
                if (context.MessageName.ToLower() == "create")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        entity = (Entity)context.InputParameters["Target"];

                        if (entity.LogicalName.ToLower() == "bookableresourcebooking")
                        {
                         //   throw new InvalidPluginExecutionException("custom errro ");
                            tracingService.Trace("Logical name is bookable resource booking Line 37");
                            EntityReference wost = entity.GetAttributeValue<EntityReference>(" ap360_workorderservicetask") ?? null;
                            if (wost != null) {
                                tracingService.Trace("wost != null");
                                Entity WOSTTimeStamp = WorkOrderServiceTaskTimeStamp.GetWorkOrderServiceTaskTimeStampRelatedtoWOST(service,tracingService, wost.Id);

                                decimal postHealth =  WOSTTimeStamp.GetAttributeValue<decimal>("ap360_wostposthealth");
                                tracingService.Trace("post health "+ postHealth.ToString());

                                Entity updateBRB = new Entity(entity.LogicalName,entity.Id);
                             //   updateBRB["ap360_prewosthealth"] = postHealth;

                                //service.Update(updateBRB);

                            }
                            tracingService.Trace("ap360_woservicetasktimestamp is created");
                           

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