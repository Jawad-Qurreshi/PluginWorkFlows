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
    public class MapNewlyCreatedWOSTToInProgressBookingForDivideTime : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
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
                Entity reterivedWOSrvTask = null;

                if (context.MessageName.ToLower() == "create")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        entity = (Entity)context.InputParameters["Target"];
                        if (entity.LogicalName == "msdyn_workorderservicetask")
                        {
                            tracingService.Trace("MapNewlyCreatedWOSTToInProgressBookingForDivideTime");
                            if (context.Stage == 40)
                            {
                                if (context.Depth > 1) return;
                                reterivedWOSrvTask = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("msdyn_workorder", "msdyn_name"));
                                if (reterivedWOSrvTask != null)
                                {
                                    tracingService.Trace("Srv task is not null");

                                    string wostName = reterivedWOSrvTask.GetAttributeValue<string>("msdyn_name");
                                    EntityReference workOrderRef = reterivedWOSrvTask.GetAttributeValue<EntityReference>("msdyn_workorder") != null ? reterivedWOSrvTask.GetAttributeValue<EntityReference>("msdyn_workorder") : null;
                                    if (workOrderRef != null)
                                    {
                                        tracingService.Trace("Work Order Ref is not null");

                                        List<BookableResourceBooking> lstBookableResourceBooking = new List<BookableResourceBooking>();
                                        lstBookableResourceBooking = BookableResourceBooking.getWorkOrdersBooking(service, tracingService, workOrderRef.Id);
                                        tracingService.Trace("booking count " + lstBookableResourceBooking.Count.ToString());

                                        foreach (BookableResourceBooking bookableResourceBooking in lstBookableResourceBooking)
                                        {
                                            Entity newBookingSrvTask = new Entity("ap360_bookingservicetask");
                                            newBookingSrvTask["ap360_name"] = wostName;
                                            newBookingSrvTask["ap360_workorderservicetask"] = new EntityReference(reterivedWOSrvTask.LogicalName, reterivedWOSrvTask.Id);
                                            newBookingSrvTask["ap360_bookableresourcebooking"] = new EntityReference("bookableresourcebooking", bookableResourceBooking.BRBGuid);
                                            service.Create(newBookingSrvTask);
                                        }
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