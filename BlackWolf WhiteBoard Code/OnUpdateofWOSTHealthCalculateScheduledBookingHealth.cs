using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class OnUpdateofWOSTHealthCalculateScheduledBookingHealth : IPlugin
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



                if (context.MessageName.ToLower() == "update")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        entity = (Entity)context.InputParameters["Target"];

                        if (entity.LogicalName == "msdyn_workorderservicetask")
                        {
                            tracingService.Trace("update of workOrder Service task");
                            if ((entity.Contains("ap360_prewosthealth")))
                            {
                                tracingService.Trace("update of workOrder Service task health");

                                decimal wostHealth = entity.GetAttributeValue<decimal>("ap360_prewosthealth");


                                List<BookableResourceBooking> lstBookableResourceBookings = new List<BookableResourceBooking>();
                                lstBookableResourceBookings = BookableResourceBooking.getSecheduledBookingforServiceTask(service, tracingService, entity.Id);
                                tracingService.Trace("Count of scheduled Bookings related to WOST : " + lstBookableResourceBookings.Count.ToString() + " WOST Health : " + wostHealth.ToString());
                                foreach (BookableResourceBooking bookableResourceBooking in lstBookableResourceBookings)
                                {
                                    Entity updateBookableResourceBooking = new Entity("bookableresourcebooking", bookableResourceBooking.BRBGuid);
                                    updateBookableResourceBooking["ap360_prewosthealth"] = wostHealth;
                                    service.Update(updateBookableResourceBooking);
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