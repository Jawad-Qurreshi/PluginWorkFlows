using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class MapIncidentTypeInQuote : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();

            try
            {

               // throw new InvalidPluginExecutionException("MapIncidentTypeInQuote");
                ITracingService tracingService =
                  (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                IPluginExecutionContext context = (IPluginExecutionContext)
                    serviceProvider.GetService(typeof(IPluginExecutionContext));

                IOrganizationServiceFactory factory =
                     (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = factory.CreateOrganizationService(context.UserId);
                Entity entity = null;

                EntityReference incidenttypeRef = null;
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    entity = (Entity)context.InputParameters["Target"];
                    if (entity.LogicalName == "quote")
                    {
                        tracingService.Trace("Entity is " + entity.LogicalName);

                        if (context.MessageName.ToLower() == "create")
                        {
                            tracingService.Trace("Create");
                            incidenttypeRef = entity.GetAttributeValue<EntityReference>("ap360_incidenttypeid") != null ? entity.GetAttributeValue<EntityReference>("ap360_incidenttypeid") : null;
                            if (incidenttypeRef != null)
                            {
                                Methods.MapIncidentTypeSrvSrvTasksSrvProds(service, tracingService, incidenttypeRef,entity.Id);
                             tracingService.Trace("End of Create");
                            }
                        }
                        if (context.MessageName.ToLower() == "update")
                        {
                            tracingService.Trace("Update");

                            if (entity.Contains("ap360_incidenttypeid"))
                            {
                                tracingService.Trace("Primary Incident Updated");
                                incidenttypeRef = entity.GetAttributeValue<EntityReference>("ap360_incidenttypeid") != null ? entity.GetAttributeValue<EntityReference>("ap360_incidenttypeid") : null;
                                if (incidenttypeRef != null)
                                {
                                    tracingService.Trace("Primary Incident is not null");
                                    Methods.MapIncidentTypeSrvSrvTasksSrvProds(service, tracingService, incidenttypeRef,entity.Id);

                                    tracingService.Trace("End of Update");
                                }

                            }

                        }
                        tracingService.Trace("Ended");

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