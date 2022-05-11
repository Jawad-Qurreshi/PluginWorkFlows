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
    //This was a commit by jawad
    public class AdjustBookingTimeOnCreationofTimeStamp : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();
            //  throw new InvalidPluginExecutionException("AdjustBookingEndTimeOnCreationofTimeStamp");
            try

            {
                //My first comment 
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
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {

                        entity = (Entity)context.InputParameters["Target"];
                        if (entity.LogicalName == "msdyn_bookingtimestamp")
                        {
                            BookingTimeStamp bookingTimeStamp = new BookingTimeStamp(); ;

                            tracingService.Trace("inside creation of BookingTime Stamp");
                            Entity timestampEntity = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet(true));
                            // timestampEntity

                            bookingTimeStamp.TimeStampSystemStatus = timestampEntity.GetAttributeValue<OptionSetValue>("msdyn_systemstatus").Value;
                            tracingService.Trace("bookingTimeStamp.TimeStampStatus " + bookingTimeStamp.TimeStampSystemStatus.ToString());
                            //690,970,003 In progress
                            //690,970,004 closed
                            //690,970,005 finsished

                            //if (bookingTimeStamp.TimeStampStatus == 690970003)  //690,970,003
                            //{

                            //    bookingTimeStamp.TimeStampTime = timestampEntity.GetAttributeValue<DateTime>("msdyn_timestamptime");
                            //    tracingService.Trace("bookingTimeStamp.TimeStampTime " + bookingTimeStamp.TimeStampTime.ToString());

                            //    tracingService.Trace("Before");
                            //    bookingTimeStamp.booking = timestampEntity.GetAttributeValue<EntityReference>("msdyn_booking") != null ? timestampEntity.GetAttributeValue<EntityReference>("msdyn_booking") : null;
                            //    if (bookingTimeStamp.booking != null)
                            //    {
                            //        tracingService.Trace("Booking is not null");
                            //        BookingTimeStamp FinishedbookingTimeStamp = new BookingTimeStamp();
                            //        FinishedbookingTimeStamp = BookingTimeStamp.getInProgressbookingTimeStamp(service, bookingTimeStamp.booking.Id);
                            //        tracingService.Trace("After reterving Finished Booking Time stamp");
                            //        if (FinishedbookingTimeStamp.guid != null && FinishedbookingTimeStamp.guid != Guid.Empty)
                            //        {
                            //            tracingService.Trace("Finihed  Booking Time Stamp exists");

                            //            Entity updateBooking = new Entity(bookingTimeStamp.booking.LogicalName);
                            //            updateBooking.Id = bookingTimeStamp.booking.Id;
                            //            updateBooking["starttime"] = FinishedbookingTimeStamp.TimeStampTime;
                            //            updateBooking["endtime"] = FinishedbookingTimeStamp.TimeStampTime.AddMinutes(30);

                            //            service.Update(updateBooking);
                            //        }
                            //    }
                            //}

                            //if (bookingTimeStamp.TimeStampSystemStatus == 690970005)//690,970,005//Finished
                            //Replaced Finsihed with Traveling in query, because technican were getting error message on Finish of Booking
                            //You have canceled this Bookable Resource Booking; canceling a Bookable Resource Booking does not cancel the related Work Order.
                            //The related Work Order has status: Open - Completed.

                            if (bookingTimeStamp.TimeStampSystemStatus == 690970001 //Traveling
                                || bookingTimeStamp.TimeStampSystemStatus == 690970005 //Finished
                                || bookingTimeStamp.TimeStampSystemStatus == 690970004)//Closed
                            {

                                bookingTimeStamp.TimeStampTime = timestampEntity.GetAttributeValue<DateTime>("msdyn_timestamptime");
                                tracingService.Trace("bookingTimeStamp.TimeStampTime " + bookingTimeStamp.TimeStampTime.ToString());

                                tracingService.Trace("Before");
                                bookingTimeStamp.booking = timestampEntity.GetAttributeValue<EntityReference>("msdyn_booking") != null ? timestampEntity.GetAttributeValue<EntityReference>("msdyn_booking") : null;
                                if (bookingTimeStamp.booking != null)
                                {
                                    tracingService.Trace("Booking is not null");
                                    BookingTimeStamp FinishedbookingTimeStamp = new BookingTimeStamp();
                                    FinishedbookingTimeStamp = BookingTimeStamp.getFinishedbookingTimeStamp(service, bookingTimeStamp.booking.Id);
                                    tracingService.Trace("After reterving Finished Booking Time stamp");
                                    if (FinishedbookingTimeStamp.guid != null && FinishedbookingTimeStamp.guid != Guid.Empty)
                                    {
                                        tracingService.Trace("Finihed  Booking Time Stamp exists");

                                        Entity updateBooking = new Entity(bookingTimeStamp.booking.LogicalName);
                                        updateBooking.Id = bookingTimeStamp.booking.Id;
                                        updateBooking["endtime"] = FinishedbookingTimeStamp.TimeStampTime;
                                        service.Update(updateBooking);
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