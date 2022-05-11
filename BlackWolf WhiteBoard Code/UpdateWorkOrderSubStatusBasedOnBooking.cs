using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
     class UpdateWorkOrderSubStatusBasedOnBooking : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();
            // throw new InvalidPluginExecutionException("QuoteServiceCalculation");

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
                // tracingService.Trace("UpdateWorkOrderSubStatusBasedOnBooking");

                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    entity = (Entity)context.InputParameters["Target"];
                    if (entity.LogicalName == "msdyn_workorder")
                    {

                        if (context.MessageName.ToLower() == "update")
                        {

                            if (entity.Contains("msdyn_systemstatus"))
                            {
                                tracingService.Trace("System Status updated");
                                int systemStatusValue = entity.GetAttributeValue<OptionSetValue>("msdyn_systemstatus").Value;
                                Entity upDateWorkOrder = new Entity(entity.LogicalName, entity.Id);
                                //if (systemStatusValue == 690970002)
                                //{
                                //    tracingService.Trace("Work Order updated to Inprogress");
                                //    upDateWorkOrder["msdyn_substatus"] = new EntityReference("msdyn_workordersubstatus", new Guid("ff20bd3a-a158-ea11-a811-000d3a33f3c3"));//Work in progress-oI


                                //}
                                //*********** 1/6/2021
                                //if (systemStatusValue == 690970003 || systemStatusValue == 690970000)//Open - Completed or Open - Unscheduled
                                //{
                                //    string selectedWorkOrderSubStatus = BookableResourceBooking.getLastModifiedOnBookingSelectedSubstatusForWorkOrder(service, tracingService, entity.Id);

                                //    WorkOrder.mapSubstatusToWorkOrder(upDateWorkOrder, systemStatusValue, selectedWorkOrderSubStatus);

                                //}
                                //else if (systemStatusValue == 690970001)//Open - scheduled
                                //{
                                //    upDateWorkOrder["ap360_customwosubstatusid"] = new EntityReference("msdyn_workordersubstatus", new Guid("2b43c947-feff-ea11-a813-000d3a33f3c3"));

                                //}
                                //else if (systemStatusValue == 690970002)//Open -In Progress
                                //{
                                //    upDateWorkOrder["ap360_customwosubstatusid"] = new EntityReference("msdyn_workordersubstatus", new Guid("ff20bd3a-a158-ea11-a811-000d3a33f3c3"));
                                //}
                                //**********end 1/6/2021

                                string selectedWorkOrderSubStatus = BookableResourceBooking.getLastModifiedOnBookingSelectedSubstatusForWorkOrder(service, tracingService, entity.Id);
                                if (selectedWorkOrderSubStatus == "inprogress")
                                {
                                    upDateWorkOrder["ap360_wobwstatus"] = new OptionSetValue(126300000);// In Progress
                                }

                                else if (selectedWorkOrderSubStatus == "completed") {
                                    upDateWorkOrder["ap360_wobwstatus"] = new OptionSetValue(126300001);// Completed
                                }

                                service.Update(upDateWorkOrder);
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