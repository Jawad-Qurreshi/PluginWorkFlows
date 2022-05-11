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
    public class AdminCloseBookings : IPlugin
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

                entity = (Entity)context.InputParameters["Target"];

                if (context.MessageName.ToLower() == "update")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        if (entity.LogicalName == "bookableresourcebooking")
                        {

                            //Manager Job , when approver updates booking Time stamped, cacluation based on Total Billable Duration when is rollup(OOB) of Booking Journal

                            //if (entity.Contains("ap360_calculateactualamount") || entity.Contains("bookingstatus"))
                            if (entity.Contains("ap360_calculateactualamount") || entity.Contains("ap360_bookingverified") || entity.Contains("ap360_bookingclassification"))

                            {
                                tracingService.Trace("ap360_calculateactualamount updated ");
                                Entity postImage = (Entity)context.PostEntityImages["Image"];
                                EntityReference bookingstatus = postImage.GetAttributeValue<EntityReference>("bookingstatus");

                                //if (entity.Contains("bookingstatus"))
                                //{
                                //    tracingService.Trace("Booking status updated updated ");
                                if (bookingstatus != null)
                                {
                                    if (bookingstatus.Id.ToString().ToLower() != "c33410b9-1abe-4631-b4e9-6e4a1113af34")//closed
                                    {
                                        return;
                                    }
                                }
                                //    tracingService.Trace("Booking status is closed ");
                                //}

                                List<BookingTimeStamp> lstBookingTimeStamps = new List<BookingTimeStamp>();
                                lstBookingTimeStamps = BookingTimeStamp.getInProgressAndFinsihedAndClosedbookingTimeStamp(service, tracingService, entity.Id);
                                BookableResourceBooking.updateBookingStartTimeAndEndTime(service, tracingService, lstBookingTimeStamps, entity.Id);
                                Entity reterivedCurrentBRB = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet(true));
                                tracingService.Trace("Manager Job:  Calculate Amount is updated");

                                bool isCalculateAmount = false;
                                isCalculateAmount = entity.GetAttributeValue<bool>("ap360_calculateactualamount");
                                if (isCalculateAmount || entity.Contains("ap360_bookingclassification"))
                                {

                                    tracingService.Trace("Start of caculation funcitons");
                                    List<BookingServiceTask> lstBookingServiceTask = new List<BookingServiceTask>();
                                    lstBookingServiceTask = BookingServiceTask.GetBookingServiceTaskRelatedBRB(service, tracingService, entity.Id);
                                    WorkOrderServiceTask.caculateWOSTTimeStamps(service, tracingService, lstBookingServiceTask, reterivedCurrentBRB, "adminclosed");
                                    tracingService.Trace("End of caculation functions");

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