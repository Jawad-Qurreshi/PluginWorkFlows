using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class TechnicianClockOutFromBooking : IPlugin
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



                if (context.MessageName.ToLower() == "update")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        if (context.Depth > 1) return;
                        entity = (Entity)context.InputParameters["Target"];

                        if (entity.LogicalName == "bookableresourcebooking")
                        {
                            if ((entity.Contains("bookingstatus")))
                            {

                                Stopwatch stopwatch = new Stopwatch();
                                stopwatch.Start();

                                tracingService.Trace("update of bookableresourcebooking ");
                                // techniacally Time spent on booking and Totalbillable should be same, some time system takes time in calculation for Totalbillable duration
                                // which cause unexpected behavior, to overcome this problem different fields are using in different types of Calculation(Manager, Technician)

                                //if (entity.GetAttributeValue<EntityReference>("bookingstatus").Id.ToString().ToLower() != "17e92808-7e59-ea11-a811-000d3a33f3c3"//Finished
                                //    || entity.GetAttributeValue<EntityReference>("bookingstatus").Id.ToString().ToLower() != "c33410b9-1abe-4631-b4e9-6e4a1113af34"//closed
                                //    )
                                if (entity.GetAttributeValue<EntityReference>("bookingstatus").Id.ToString().ToLower() != "c33410b9-1abe-4631-b4e9-6e4a1113af34")//Closed
                                {
                                    return;
                                }


                                // the code that you want to measure comes here
                                Entity reterivedCurrentBRB = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet(true));

                                if (reterivedCurrentBRB != null)
                                {
                                    tracingService.Trace("Normal Behrivor");

                                    // EntityReference workOrderRef = reterivedCurrentBRB.GetAttributeValue<EntityReference>("msdyn_workorder") != null ? reterivedCurrentBRB.GetAttributeValue<EntityReference>("msdyn_workorder") : null;
                                    //int wobwstatus = reterivedCurrentBRB.GetAttributeValue<OptionSetValue>("ap360_wobwstatus") != null ? reterivedCurrentBRB.GetAttributeValue<OptionSetValue>("ap360_wobwstatus").Value : 0;

                                    List<BookingTimeStamp> lstBookingTimeStamps = new List<BookingTimeStamp>();
                                    lstBookingTimeStamps = BookingTimeStamp.getInProgressAndFinsihedAndClosedbookingTimeStamp(service, tracingService, entity.Id);
                                    BookableResourceBooking.updateBookingStartTimeAndEndTime(service, tracingService, lstBookingTimeStamps, entity.Id);

                                    List<BookingServiceTask> lstBookingServiceTask = new List<BookingServiceTask>();


                                    lstBookingServiceTask = BookingServiceTask.GetBookingServiceTaskRelatedBRB(service, tracingService, entity.Id);


                                    WorkOrderServiceTask.caculateWOSTTimeStamps(service, tracingService, lstBookingServiceTask, reterivedCurrentBRB, "technicianFinshed");
                                    stopwatch.Stop();

                                    TimeSpan ts = stopwatch.Elapsed;


                                   // throw new InvalidPluginExecutionException("Elapsed Time is updated "+ts.Minutes+":" +ts.Seconds);





                                }


                            }

                            if ((entity.Contains("ap360_mistakeadjustment")))
                            {
                                Entity postImage = (Entity)context.PostEntityImages["Image"];
                                DateTime mistakeadjustmentstarttime = postImage.GetAttributeValue<DateTime>("ap360_mistakeadjustmentstarttime");
                                DateTime mistakeadjustmentendtime = postImage.GetAttributeValue<DateTime>("ap360_mistakeadjustmentendtime");
                                string mistakeadjustmentreason = postImage.GetAttributeValue<string>("ap360_mistakeadjustmentreason");
                               

                                TaskActivity.CreateTaskForMistakeAdjustment(service, tracingService, entity, mistakeadjustmentstarttime, mistakeadjustmentendtime, mistakeadjustmentreason);

                               // Entity updateBooking = new Entity("bookableresourcebooking", entity.Id);
                               // updateBooking["ap360_mistakeadjustment"] = false;
                               //// updateBooking["ap360_mistakeadjustmentstarttime"] = null;
                               //// updateBooking["ap360_mistakeadjustmentendtime"] = null;

                               // if (updateBooking.Contains("ap360_mistakeadjustmentstarttime"))
                               // {
                               //     updateBooking["ap360_mistakeadjustmentstarttime"] = null;
                               // }
                               // else
                               // {
                               //     updateBooking.Attributes.Add("ap360_mistakeadjustmentstarttime", null);
                               // }
                               // if (updateBooking.Contains("ap360_mistakeadjustmentendtime"))
                               // {
                               //     updateBooking["ap360_mistakeadjustmentendtime"] = null;
                               // }
                               // else
                               // {
                               //     updateBooking.Attributes.Add("ap360_mistakeadjustmentendtime", null);
                               // }
                               // updateBooking["ap360_mistakeadjustmentreason"] = null;

                               // service.Update(updateBooking);
                            }
                            //EntityCollection journalsCollection = BookingJournal.RetrievebookingJournal(service, tracingService, entity.Id);
                            //if (journalsCollection != null && journalsCollection.Entities.Count > 0)
                            //{
                            //    foreach (Entity journal in journalsCollection.Entities)
                            //    {
                            //        service.Delete(journal.LogicalName, journal.Id);
                            //    }
                            //}





                            //List<BookingTimeStamp> lstBookingTimeStamps = BookingTimeStamp.getInProgressAndFinsihedAndClosedbookingTimeStamp(service, tracingService, entity.Id);
                            //lstBookingTimeStamps = lstBookingTimeStamps.OrderBy(x => x.TimeStampSystemStatus).ToList();
                            //foreach (BookingTimeStamp bookingTimeStamp in lstBookingTimeStamps)
                            //{



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