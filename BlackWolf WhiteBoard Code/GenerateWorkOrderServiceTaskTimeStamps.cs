using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class GenerateWorkOrderServiceTaskTimeStamps : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();
            //  throw new InvalidPluginExecutionException("GenerateWorkOrderServiceTaskTimeStamps");

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

                if (context.PrimaryEntityName.ToLower() != "ap360_bookingservicetask") { return; }
                if ((context.MessageName.ToLower() != "update"))//Create WO Servcie Task Time Stamp
                    return;
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    if (context.Depth > 1) return;
                    entity = (Entity)context.InputParameters["Target"];

                    Entity bookingServiceTaskEntity = service.Retrieve("ap360_bookingservicetask", entity.Id, new ColumnSet("ap360_bookableresourcebooking", "ap360_bookingpercenttimespent", "ap360_workorderservicetask"));

                    BookingServiceTask bookingServiceTask = new BookingServiceTask();
                    BookableResourceBooking bookableResourceBooking = new BookableResourceBooking();
                    WorkOrderServiceTask workOrderServiceTask = new WorkOrderServiceTask();
                    TimeSpan timespentonWorkOrderServicetask;

                    if (bookingServiceTaskEntity != null)
                    {

                        if (bookingServiceTaskEntity.Contains("ap360_bookableresourcebooking") && (bookingServiceTaskEntity["ap360_bookableresourcebooking"] is EntityReference))
                            bookingServiceTask.Booking = (EntityReference)bookingServiceTaskEntity["ap360_bookableresourcebooking"];
                        if (bookingServiceTask.Booking != null)
                        {
                            Entity BRBEntity = service.Retrieve(bookingServiceTask.Booking.LogicalName, bookingServiceTask.Booking.Id, new ColumnSet("msdyn_actualarrivaltime", "ap360_finishtime"));

                            int? currentusertimezone = RetrieveCurrentUsersTimeZoneSettings(service);
                            if (BRBEntity != null)
                            {
                                if (BRBEntity.Contains("msdyn_actualarrivaltime"))
                                {
                                    bookableResourceBooking.ActualArrivalTime = BRBEntity["msdyn_actualarrivaltime"] is DateTime ? (DateTime)BRBEntity["msdyn_actualarrivaltime"] : new DateTime();
                                    bookableResourceBooking.ActualArrivalTime = (DateTime)RetrieveLocalTimeFromUTCTime(service, bookableResourceBooking.ActualArrivalTime, currentusertimezone);
                                }
                                if (BRBEntity.Contains("ap360_finishtime"))
                                {
                                    bookableResourceBooking.FinishTime = BRBEntity["ap360_finishtime"] is DateTime ? (DateTime)BRBEntity["ap360_finishtime"] : new DateTime();
                                    bookableResourceBooking.FinishTime = (DateTime)RetrieveLocalTimeFromUTCTime(service, bookableResourceBooking.FinishTime, currentusertimezone);
                                }



                                if (bookingServiceTaskEntity.Contains("ap360_bookingpercenttimespent") && bookingServiceTaskEntity["ap360_bookingpercenttimespent"] is decimal)
                                {
                                    bookingServiceTask.PercentTimeSpent = (decimal)bookingServiceTaskEntity["ap360_bookingpercenttimespent"];
                                }
                            }
                            // bookableResourceBooking.FinishTime = (DateTime)RetrieveLocalTimeFromUTCTime(service, bookableResourceBooking.FinishTime, currentusertimezone);
                            timespentonWorkOrderServicetask = bookableResourceBooking.FinishTime - bookableResourceBooking.ActualArrivalTime;

                            tracingService.Trace("Time spent on WorkOrder Service Task " + timespentonWorkOrderServicetask.ToString() + " bookableResourceBooking.FinishTime- bookableResourceBooking.ActualArrivalTime");
                            if (bookingServiceTaskEntity.Contains("ap360_workorderservicetask") && (bookingServiceTaskEntity["ap360_workorderservicetask"] is EntityReference))
                                bookingServiceTask.WorkOrderServiceTask = (EntityReference)bookingServiceTaskEntity["ap360_workorderservicetask"];
                            if (bookingServiceTask.WorkOrderServiceTask != null)
                            {
                                Entity woservicetasktimestamp = new Entity("ap360_woservicetasktimestamp");
                                woservicetasktimestamp["ap360_workorderservicetaskid"] = new EntityReference(bookingServiceTask.WorkOrderServiceTask.LogicalName, bookingServiceTask.WorkOrderServiceTask.Id);

                                int timespent = (int)Math.Ceiling((timespentonWorkOrderServicetask.TotalMinutes / 100) * (double)bookingServiceTask.PercentTimeSpent);
                                woservicetasktimestamp["ap360_name"] = "Time Spend  " + timespent;

                                // throw new InvalidPluginExecutionException(timespent.ToString());
                                woservicetasktimestamp["ap360_timespent"] = timespent;


                                //decimal billingPrice = bookableResourceBooking.updateWorkOrderTotalActualLaborAmount(service, tracingService, BRBEntity.Id, "ap360_timespentonbooking");
                                //woservicetasktimestamp["ap360_actualAmount"] = new Money(billingPrice);

                                service.Create(woservicetasktimestamp);


                                /////////////////////////Update Booking Service Task ////////////////////////////////
                                //This field sets from Bookable Resource Booking Done button/////////////////////////
                                //

                                Entity bookingServcieTaskEntity = new Entity("ap360_bookingservicetask");
                                bookingServcieTaskEntity.Id = entity.Id;
                                bookingServcieTaskEntity["ap360_createwoservicetasktimestamp"] = false;
                                service.Update(bookingServcieTaskEntity);


                            }
                            else
                            {
                                throw new InvalidPluginExecutionException("Correponding WorkOrder Service task not exists. Contact CRM Admninistrator");
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


        public static int? RetrieveCurrentUsersTimeZoneSettings(IOrganizationService service)
        {

            var currentUserSettings = service.RetrieveMultiple(

            new QueryExpression("usersettings")

            {

                ColumnSet = new ColumnSet("localeid", "timezonecode"),

                Criteria = new FilterExpression

                {

                    Conditions =

               {

            new ConditionExpression("systemuserid", ConditionOperator.EqualUserId)

               }

                }

            }).Entities[0].ToEntity<Entity>();

            return (int?)currentUserSettings.Attributes["timezonecode"];

        }

        public static DateTime? RetrieveLocalTimeFromUTCTime(IOrganizationService _serviceProxy, DateTime utcTime, int? _timeZoneCode)

        {

            if (!_timeZoneCode.HasValue)

                return null;



            var request = new LocalTimeFromUtcTimeRequest

            {

                TimeZoneCode = _timeZoneCode.Value,

                UtcTime = utcTime.ToUniversalTime()

            };



            var response = (LocalTimeFromUtcTimeResponse)_serviceProxy.Execute(request);

            return response.LocalTime;

        }


    }
}