using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class MakeQuoteFirstWorkOrderAvailableForBooking : IPlugin
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

                IOrganizationServiceFactory factory =
                     (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = factory.CreateOrganizationService(context.UserId);

                Entity entity = null;

                if (context.MessageName.ToLower() == "create")
                {
                    tracingService.Trace("MakeQuoteFirstWorkOrderAvailableForBooking");
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        entity = (Entity)context.InputParameters["Target"];
                        if (entity.LogicalName == "msdyn_workorder")
                        {

                            EntityReference opprRef = entity.GetAttributeValue<EntityReference>("msdyn_opportunityid");
                            if (opprRef != null)
                            {
                                int countOFWOInProgress = 0;

                                List<WorkOrder> lstWorkOrders = new List<WorkOrder>();
                                lstWorkOrders = WorkOrder.getListofWorkOrderRelatedToOpportunity(service, tracingService, opprRef.Id);

                                countOFWOInProgress = WorkOrder.getNumberofWorkOrderCountInProgress(lstWorkOrders, tracingService);

                                if (countOFWOInProgress == 0)
                                {
                                    // Entity updateWorkOrder = new Entity(entity.LogicalName, entity.Id);
                                    List<OptionSetValue> lstWorkOrderBWStatus = new List<OptionSetValue>();
                                    lstWorkOrderBWStatus.Add(new OptionSetValue(126300008));//Available For Booking-InProgress

                                    entity["ap360_workorderbwstatus"] = new OptionSetValueCollection(lstWorkOrderBWStatus);

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