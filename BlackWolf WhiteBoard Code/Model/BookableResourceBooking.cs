using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code.Model
{
    public class BookableResourceBooking : EntityBase
    {
        public Guid BRBGuid { get; set; }

        public DateTime ActualArrivalTime { get; set; }

        public DateTime FinishTime { get; set; }
        public bool IsFinished { get; set; }
        public int NextPreferredAction { get; set; }
        public EntityReference BookingStatus { get; set; }
        public decimal BillingFactor { get; set; }

        public int TimeSpentOnBooking { get; set; }
        public EntityReference Resource { get; set; }
        public EntityReference ServiceRole { get; set; }

        public EntityReference WordOrder { get; set; }
        public EntityReference WordOrderServiceTask { get; set; }


        public double TotalBillableDuration { get; set; }
        public override void Execute(IPluginExecutionContext executionContext, IOrganizationService organizationService, ITracingService tracingService)
        {
            // tracingService.Trace("Resource Assignment Plugin starts");

            // throw new InvalidPluginExecutionException("Error");
            var message = executionContext.MessageName.ToLower();

            switch (message)
            {
                case "retrieve":
                case "retrievemultiple":
                    if (executionContext.Depth > 1) return;
                    BookableResourceBookingRetriveMultiple(executionContext, organizationService, tracingService);
                    break;
                case "create":
                    if (executionContext.Depth > 2) return;//In case of book 1, pick task and start WOST is 2
                    tracingService.Trace("Pre create bookableResourceBookingEntity Plugin starts, Depth:" + executionContext.Depth);
                    Entity bookingEntity = (Entity)executionContext.InputParameters["Target"];
                    decimal ResourceHourlyRate = 0.0m;
                    decimal ServiceRoleHourlyRate = 0.0m;
    


                    // throw new InvalidPluginExecutionException(bookingEntity.GetAttributeValue<EntityReference>("resource").Name);
                    int? currentUserTimeZone = Methods.RetrieveCurrentUsersTimeZoneSettings(organizationService);
                    //bookingEntity["msdyn_actualarrivaltime"] = (DateTime)Methods.RetrieveLocalTimeFromUTCTime(organizationService, DateTime.Now.AddMinutes(1), currentUserTimeZone);
                    bookingEntity["msdyn_estimatedarrivaltime"] = (DateTime)Methods.RetrieveLocalTimeFromUTCTime(organizationService, DateTime.Now.AddMinutes(1), currentUserTimeZone);
                    bookingEntity["starttime"] = (DateTime)Methods.RetrieveLocalTimeFromUTCTime(organizationService, DateTime.Now, currentUserTimeZone);
                    //  bookingEntity["ap360_inprogresstime"] = (DateTime)Methods.RetrieveLocalTimeFromUTCTime(organizationService, DateTime.Now, currentUserTimeZone);
                    //bookingEntity["msdyn_actualarrivaltime"] = DateTime.Now;
                    //bookingEntity["msdyn_estimatedarrivaltime"] = DateTime.Now;
                    var bookingType = getbookingType(organizationService);
                    if (bookingType) return;//development for CRM 
                    // testing for booking type

                    EntityReference workorderEntityRef = null;
                    EntityReference serviceTaskEntityRef = null;
                    EntityReference opportunityEntityRef = null;



                    if (bookingEntity.Contains("ap360_workorderservicetask"))
                        serviceTaskEntityRef = GetLookupAttributeValue(bookingEntity, "ap360_workorderservicetask");
                                                                           
                    decimal wostEstimatedLaborPrice = 0.0m;
                    if (serviceTaskEntityRef != null)
                    {
                        Entity reterviedServiceTaskEntity = organizationService.Retrieve(serviceTaskEntityRef.LogicalName, serviceTaskEntityRef.Id, new ColumnSet("msdyn_name", "msdyn_workorder", "ap360_opportunityid", "ap360_serviceroleid", "ap360_vehicleid", "ap360_estimatedlaborprice"));
                        bookingEntity["name"] = reterviedServiceTaskEntity.GetAttributeValue<string>("msdyn_name");
                        wostEstimatedLaborPrice = reterviedServiceTaskEntity.GetAttributeValue<Money>("ap360_estimatedlaborprice") != null ? reterviedServiceTaskEntity.GetAttributeValue<Money>("ap360_estimatedlaborprice").Value : 0;
                        EntityReference vehicleRef = reterviedServiceTaskEntity.GetAttributeValue<EntityReference>("ap360_vehicleid") ?? null;
                        if (vehicleRef != null)
                            bookingEntity["ap360_vehicleid"] = vehicleRef;
                        EntityReference serviceroleRef = reterviedServiceTaskEntity.GetAttributeValue<EntityReference>("ap360_serviceroleid") ?? null;
                        if (serviceroleRef != null)
                        {
                            bookingEntity["ap360_serviceroles"] = serviceroleRef;


                            ResourceHourlyRate = bookingEntity.GetAttributeValue<Money>("ap360_resourcehourlyrate") != null ? bookingEntity.GetAttributeValue<Money>("ap360_resourcehourlyrate").Value : 0;
                            if (ResourceHourlyRate == 0)
                            {
                                tracingService.Trace("*****************Resource hourly rate is null on booking");
                                tracingService.Trace("Resouce Id " + bookingEntity.GetAttributeValue<EntityReference>("resource").Id.ToString());
                                tracingService.Trace("ServiceRole Name " + serviceroleRef.Name);

                                ResourceHourlyRate = Methods.GetResourcePriceBasedOnBRC(organizationService, tracingService, bookingEntity.GetAttributeValue<EntityReference>("resource").Id, serviceroleRef.Name);
                            }


                            ServiceRoleHourlyRate = bookingEntity.GetAttributeValue<Money>("ap360_servicerolehourlyrate") != null ? bookingEntity.GetAttributeValue<Money>("ap360_servicerolehourlyrate").Value : 0;
                            if (ServiceRoleHourlyRate == 0)
                            {
                                tracingService.Trace("****************ServiceRoleHourlyRate is null on booking");
                                tracingService.Trace("ServiceRole Name " + serviceroleRef.Name);

                                ServiceRoleHourlyRate = Methods.getBookAbleResourceCategoryStandardPrice(organizationService, serviceroleRef.Name);
                            }
                            tracingService.Trace("ResourceHourlyRate " + ResourceHourlyRate.ToString());
                            tracingService.Trace("ServicRoleHourlyRate " + ServiceRoleHourlyRate.ToString());
                            bookingEntity["ap360_resourcehourlyrate"] = new Money(ResourceHourlyRate);
                            bookingEntity["ap360_servicerolehourlyrate"] = new Money(ServiceRoleHourlyRate);
                        }


                        tracingService.Trace("Work Order service task Health " + GetDecimalAttributeValue(reterviedServiceTaskEntity, "ap360_postwosthealth"));
                        Entity retWOSTTimeStampEntity = WorkOrderServiceTaskTimeStamp.GetWorkOrderServiceTaskTimeStampRelatedtoWOST(organizationService, tracingService, reterviedServiceTaskEntity.Id);
                        if (retWOSTTimeStampEntity != null)
                        {
                            tracingService.Trace("WOST time health " + retWOSTTimeStampEntity.GetAttributeValue<decimal>("ap360_wostposthealth").ToString());
                            decimal postHealth = retWOSTTimeStampEntity.GetAttributeValue<decimal>("ap360_wostposthealth");
                            bookingEntity["ap360_prewosthealth"] = postHealth;
                        }
                        workorderEntityRef = GetLookupAttributeValue(reterviedServiceTaskEntity, "msdyn_workorder");
                        if (workorderEntityRef != null)
                        {
                            tracingService.Trace("Setting Workorder :" + workorderEntityRef.Id);
                            bookingEntity["msdyn_workorder"] = workorderEntityRef;
                            mapBookingClassificationOnBooking(organizationService, tracingService, workorderEntityRef, bookingEntity);

                        }
                        opportunityEntityRef = GetLookupAttributeValue(reterviedServiceTaskEntity, "ap360_opportunityid");
                        if (opportunityEntityRef != null)
                        {
                            // throw new InvalidPluginExecutionException(" opporutnity not null");
                            var reterivedOppEntity = organizationService.Retrieve(opportunityEntityRef.LogicalName, opportunityEntityRef.Id, new ColumnSet("ap360_opportunityautonumber"));
                            if (reterivedOppEntity != null)
                            {
                                tracingService.Trace("Opprotunity Number " + reterivedOppEntity.GetAttributeValue<string>("ap360_opportunityautonumber").ToString());
                                bookingEntity["ap360_opportunitynumber"] = Convert.ToInt32(reterivedOppEntity.GetAttributeValue<string>("ap360_opportunityautonumber"));
                                //throw new InvalidPluginExecutionException(Convert.ToInt32(reterivedOppEntity.GetAttributeValue<string>("ap360_opportunityautonumber")).ToString());

                            }
                            tracingService.Trace("Setting Opportunity :" + opportunityEntityRef.Id);
                            bookingEntity["ap360_opportunityid"] = opportunityEntityRef;
                        }

                        //   WorkOrder.mapWorkOrderFieldsToBooking(organizationService, tracingService, bookingEntity, workorderEntityRef.Id);
                    }
                    else
                    {
                        if (bookingEntity.Contains("msdyn_workorder"))
                        {
                            workorderEntityRef = GetLookupAttributeValue(bookingEntity, "msdyn_workorder");
                            var reterivedWOEntity = organizationService.Retrieve(workorderEntityRef.LogicalName, workorderEntityRef.Id, new ColumnSet("msdyn_opportunityid"));
                            if (reterivedWOEntity != null)
                            {

                                opportunityEntityRef = GetLookupAttributeValue(reterivedWOEntity, "msdyn_opportunityid");
                                if (opportunityEntityRef != null)
                                {
                                    var reterivedOppEntity = organizationService.Retrieve(opportunityEntityRef.LogicalName, opportunityEntityRef.Id, new ColumnSet("ap360_opportunityautonumber"));
                                    if (reterivedOppEntity != null)
                                    {
                                        bookingEntity["ap360_opportunitynumber"] = Convert.ToDecimal(reterivedOppEntity.GetAttributeValue<string>("ap360_opportunityautonumber"));
                                    }
                                    bookingEntity["ap360_opportunityid"] = opportunityEntityRef;
                                }
                            }
                        }
                    }

                    var watch1 = System.Diagnostics.Stopwatch.StartNew();


                    int dollarTimeRemaing = CalculatePercentagesNew(organizationService, tracingService, bookingEntity);


                    watch1.Stop();
                    var elapsedMs = watch1.ElapsedMilliseconds;
                    tracingService.Trace("Time  " + elapsedMs.ToString() + " dollarTimeRemaining " + dollarTimeRemaing.ToString());
                    // throw new InvalidPluginExecutionException("updated Time  " + elapsedMs.ToString() + " dollarTimeRemaining " + dollarTimeRemaing.ToString());

                    tracingService.Trace("dollarTimeRemaing " + dollarTimeRemaing.ToString());


                    // throw new InvalidPluginExecutionException("Custom error");
                    //bookingEntity["starttime"] = DateTime.Now;
                    if (dollarTimeRemaing <= 0 && wostEstimatedLaborPrice > 0)//second condition is important for WO And WOST
                        throw new InvalidPluginExecutionException("Estimated time exceeded. Create child service task or contact Admin team.");


                    //throw new InvalidPluginExecutionException("test");
                    if (wostEstimatedLaborPrice > 0) //This is important for standard WO and WOsT
                        bookingEntity["endtime"] = (DateTime)Methods.RetrieveLocalTimeFromUTCTime(organizationService, DateTime.Now.AddMinutes(dollarTimeRemaing), currentUserTimeZone);
                    else
                        bookingEntity["endtime"] = (DateTime)Methods.RetrieveLocalTimeFromUTCTime(organizationService, DateTime.Now.AddMinutes(30), currentUserTimeZone);


                    //bookingEntity["endtime"] = DateTime.Now.AddMinutes(dollarTimeRemaing);
                    //  bookingEntity["ap360_inprogresstime"] = DateTime.Now;

                    //throw new InvalidPluginExecutionException("test");
                    //  organizationService.Update(bookingEntity);
                    //  throw new InvalidPluginExecutionException(dollarTimeRemaing.ToString());

                    break;

                    //case "update":
                    //    Entity bookableResourceBookingEntity = (Entity)executionContext.InputParameters["Target"];
                    //    string TimeCalculationField = null;// techniacally Time spent on booking and Totalbillable should be same, some time system takes time in calculation for Totalbillable duration
                    //                                       // which cause unexpected behavior, to overcome this problem different fields are using in different types of Calculation(Manager, Technician)

                    //    EntityReference bookingStatusRef = null;

                    //    Entity reterivedCurrentBRB = organizationService.Retrieve(bookableResourceBookingEntity.LogicalName, bookableResourceBookingEntity.Id, new ColumnSet(true));
                    //    //if (bookableResourceBookingEntity.Contains("bookingstatus"))
                    //    //{
                    //    //    if (executionContext.Depth > 4) return;



                    //    //    //Normal Behavior , when technican mark BRB as Finsished, calculation based on Time spent on booking


                    //    //}



                    //    // }

                    //    // *****************************************************************Commmented on DEC 7 * **********************************
                    //    //        if (executionContext.Depth > 2) return;
                    //    //        tracingService.Trace("Post Update bookableResourceBookingEntity Plugin starts, Depth:" + executionContext.Depth);

                    //    //        if (bookableResourceBookingEntity.Contains("bookingstatus"))
                    //    //        {
                    //    //            EntityReference bookingStatusReference = bookableResourceBookingEntity.GetAttributeValue<EntityReference>("bookingstatus");
                    //    //            Guid FinishedBookingStatusGuid = new Guid("17e92808-7e59-ea11-a811-000d3a33f3c3");// Finished
                    //    //            Guid closedBookingStatusGuid = new Guid("c33410b9-1abe-4631-b4e9-6e4a1113af34"); //closed
                    //    //            if (bookingStatusReference.Id == FinishedBookingStatusGuid || bookingStatusReference.Id == closedBookingStatusGuid)// 
                    //    //            {
                    //    //                //throw new InvalidPluginExecutionException("working");
                    //    //                BookableResourceBookingRetriveMultiple(executionContext, organizationService, tracingService);
                    //    //            }
                    //    //        }
                    //    //        //   **************************************************************END BLOCK * ***********************************************


                    //    break;
            }
        }

        //if (bookableResourceBookingEntity.Contains("ap360_serviceroles"))
        //    CalculateRolePrice(organizationService,tracingService, bookableResourceBookingEntity);


        //*********************************************************************Commented on DEC 7 ********************************************
        //if (bookableResourceBookingEntity.Contains("bookingstatus"))
        //{

        //    tracingService.Trace("In Booking Status:");
        //    var bookingStatusEntityRef = GetLookupAttributeValue(bookableResourceBookingEntity, "bookingstatus");

        //    if (bookingStatusEntityRef == null) return;
        //    tracingService.Trace("BookingStatusId:" + bookingStatusEntityRef.Id);

        //    if (bookingStatusEntityRef.Id == new Guid("C33410B9-1ABE-4631-B4E9-6E4A1113AF34"))//Closed
        //    {
        //        tracingService.Trace("Close booking");
        //        var resourceBookingEntity = organizationService.Retrieve(bookableResourceBookingEntity.LogicalName, bookableResourceBookingEntity.Id, new ColumnSet("ap360_serviceroles", "ap360_bookingclassification"));

        //        if (resourceBookingEntity == null) return;

        //        //if (!bookableResourceBookingEntity.Contains("ap360_serviceroles"))
        //        //{
        //        //    if (!resourceBookingEntity.Contains("ap360_serviceroles"))
        //        //        throw new InvalidPluginExecutionException("Please provide service role before closing booking.");
        //        //}

        //        if (!bookableResourceBookingEntity.Contains("ap360_bookingclassification"))
        //        {
        //            if (!resourceBookingEntity.Contains("ap360_bookingclassification"))
        //                throw new InvalidPluginExecutionException("Please provide booking classification before closing booking.");
        //        }
        //    }

        //}
        //**************************************************************END BLOCK************************************************


        public static string getExtraTimeRequiredReasons(int extraTimeRequiredReason)
        {
            string extraTimeRequiredReasonString = "";
            if (extraTimeRequiredReason == 126300000)
            {
                extraTimeRequiredReasonString = "Incorrect Part";
            }
            else if (extraTimeRequiredReason == 126300001)
            {
                extraTimeRequiredReasonString = "Discovery";
            }
            else if (extraTimeRequiredReason == 126300002)
            {
                extraTimeRequiredReasonString = "We Damaged";
            }
            else if (extraTimeRequiredReason == 126300003)
            {
                extraTimeRequiredReasonString = "Part Fitment";
            }
            else if (extraTimeRequiredReason == 126300004)
            {
                extraTimeRequiredReasonString = "Defective Part";
            }
            else if (extraTimeRequiredReason == 126300005)
            {
                extraTimeRequiredReasonString = "Planned Time InSufficient";
            }
            else if (extraTimeRequiredReason == 126300006)
            {
                extraTimeRequiredReasonString = "MisUsed Alloted Time";
            }
            else if (extraTimeRequiredReason == 126300007)
            {
                extraTimeRequiredReasonString = "Over Estimated Capability";
            }
            return extraTimeRequiredReasonString;
        }

        public static void updateBookingStartTimeAndEndTime(IOrganizationService service, ITracingService tracing, List<BookingTimeStamp> lstBookingTimeStamps, Guid BRBGuid)
        {
            //690,970,003 In progress
            //690,970,004 closed
            //690,970,005 finsished
            tracing.Trace("Inside  updateBookingStartTimeAndEndTime");

            BookingTimeStamp FirstInProgressBookingTimeStamp = null;
            BookingTimeStamp LastFinsihedBookingTimeStamp = null;
            FirstInProgressBookingTimeStamp = lstBookingTimeStamps.FirstOrDefault(e => e.TimeStampSystemStatus == 690970003);

            // LastFinsihedBookingTimeStamp = lstBookingTimeStamps.LastOrDefault(e => e.TimeStampSystemStatus == 690970005);//Finished
            //Replaced Finsihed with Traveling in query, because technican were getting error message on Finish of Booking
            //You have canceled this Bookable Resource Booking; canceling a Bookable Resource Booking does not cancel the related Work Order.
            //The related Work Order has status: Open - Completed.
            //msdyn_systemstatus
            //690,970,003 In progress
            //690,970,004 closed
            //690970001  Traveling
            //690,970,005 finsished
            LastFinsihedBookingTimeStamp = lstBookingTimeStamps.LastOrDefault(e => e.TimeStampSystemStatus == 690970001 ||//Traveling
            e.TimeStampSystemStatus == 690970005 ||//finsished, Traveling will remain finished
            e.TimeStampSystemStatus == 690970004);//Closed 
            //is here only for update of old bookings
            //throw new InvalidPluginExecutionException(FirstInProgressBookingTimeStamp.TimeStampTime.ToString()+" -- "+LastFinsihedBookingTimeStamp.TimeStampTime.ToString());
            if (FirstInProgressBookingTimeStamp != null && LastFinsihedBookingTimeStamp != null)
            {
                tracing.Trace("***********************************");
                tracing.Trace("In Progress " + FirstInProgressBookingTimeStamp.TimeStampSystemStatus + " " + FirstInProgressBookingTimeStamp.TimeStampTime);
                tracing.Trace("Finished " + LastFinsihedBookingTimeStamp.TimeStampSystemStatus + " " + LastFinsihedBookingTimeStamp.TimeStampTime);
                Entity updateBookingStartAndEndTime = new Entity("bookableresourcebooking");
                updateBookingStartAndEndTime.Id = BRBGuid;
                updateBookingStartAndEndTime["starttime"] = FirstInProgressBookingTimeStamp.TimeStampTime;
                double BillableDuration = (LastFinsihedBookingTimeStamp.TimeStampTime - FirstInProgressBookingTimeStamp.TimeStampTime).TotalMinutes;
                updateBookingStartAndEndTime["endtime"] = LastFinsihedBookingTimeStamp.TimeStampTime;
                updateBookingStartAndEndTime["msdyn_totaldurationinprogress"] = BillableDuration;
                updateBookingStartAndEndTime["msdyn_totalbillableduration"] = BillableDuration;
                updateBookingStartAndEndTime["msdyn_actualarrivaltime"] = FirstInProgressBookingTimeStamp.TimeStampTime;
                updateBookingStartAndEndTime["msdyn_estimatedarrivaltime"] = FirstInProgressBookingTimeStamp.TimeStampTime;

                service.Update(updateBookingStartAndEndTime);
                tracing.Trace("Booking Start and End Time Updated");

                BookingJournal.UpdatebookingJournal(service, tracing, BRBGuid, BillableDuration, FirstInProgressBookingTimeStamp.TimeStampTime, LastFinsihedBookingTimeStamp.TimeStampTime);
                //tracing.Trace("*********************************************");
                //foreach (BookingTimeStamp bookTimestamp in lstBookingTimeStamps)
                //{
                //    tracing.Trace(bookTimestamp.TimeStampBookingStatus.Name + " " + bookTimestamp.TimeStampTime);
                //}
                //tracing.Trace("*********************************************");
            }

            // throw new InvalidPluginExecutionException("UPdated ");


        }
        internal void PopulateReferenceData(IOrganizationService organizationService, ITracingService tracingService, IWorkflowContext context)
        {
            if (context.Depth > 2) return;

            tracingService.Trace("AssignServiceRole starts");
            var resourceBookingEntity = organizationService.Retrieve(context.PrimaryEntityName, context.PrimaryEntityId, new ColumnSet("resource", "msdyn_workorder", "ap360_workorderservicetask", "msdyn_resourcerequirement"));
            var resourceEntityRef = GetLookupAttributeValue(resourceBookingEntity, "resource");
            if (resourceEntityRef == null) return;

            var workorderEntityRef = GetWorkOrderFromBookingReference(organizationService, tracingService, resourceBookingEntity);

            if (workorderEntityRef == null) return;

            var workOrderEntity = organizationService.Retrieve(workorderEntityRef.LogicalName, workorderEntityRef.Id, new ColumnSet("ap360_estimateserviceroles"));
            var serviceRoleEntityRef = GetLookupAttributeValue(workOrderEntity, "ap360_estimateserviceroles");
            if (serviceRoleEntityRef == null) return;

            var roleKey = string.Empty;

            if (serviceRoleEntityRef.Name.Split(' ').Length <= 2)
                roleKey = serviceRoleEntityRef.Name.Split(' ')[0];

            if (serviceRoleEntityRef.Name.Split(' ').Length > 2)
                roleKey = serviceRoleEntityRef.Name.Split(' ')[1];

            tracingService.Trace("Role key:" + roleKey);
            if (roleKey == string.Empty) return;

            var resourceServiceRoleEntityRef = GetResourceServiceRole(organizationService, tracingService, resourceEntityRef.Id, roleKey);

            if (resourceServiceRoleEntityRef == null) return;

            //resourceBookingEntity["msdyn_workorder"] = workorderEntityRef;
            resourceBookingEntity["ap360_serviceroles"] = resourceServiceRoleEntityRef;
            organizationService.Update(resourceBookingEntity);

            tracingService.Trace("Done");
        }

        internal void AssignServiceRole(IOrganizationService organizationService, ITracingService tracingService, IWorkflowContext context)
        {
            if (context.Depth > 2) return;

            tracingService.Trace("AssignServiceRole starts");
            var resourceBookingEntity = organizationService.Retrieve(context.PrimaryEntityName, context.PrimaryEntityId, new ColumnSet("resource", "msdyn_workorder", "ap360_workorderservicetask", "msdyn_resourcerequirement"));

            var resourceEntityRef = GetLookupAttributeValue(resourceBookingEntity, "resource");
            if (resourceEntityRef == null) return;

            var workorderEntityRef = GetLookupAttributeValue(resourceBookingEntity, "msdyn_workorder");
            if (workorderEntityRef == null) return;

            var workOrderEntity = organizationService.Retrieve(workorderEntityRef.LogicalName, workorderEntityRef.Id, new ColumnSet("ap360_estimateserviceroles"));
            var serviceRoleEntityRef = GetLookupAttributeValue(workOrderEntity, "ap360_estimateserviceroles");
            if (serviceRoleEntityRef == null) return;

            var roleKey = string.Empty;

            if (serviceRoleEntityRef.Name.Split(' ').Length <= 2)
                roleKey = serviceRoleEntityRef.Name.Split(' ')[0];

            if (serviceRoleEntityRef.Name.Split(' ').Length > 2)
                roleKey = serviceRoleEntityRef.Name.Split(' ')[1];

            tracingService.Trace("Role key:" + roleKey);
            if (roleKey == string.Empty) return;

            var resourceServiceRoleEntityRef = GetResourceServiceRole(organizationService, tracingService, resourceEntityRef.Id, roleKey);

            if (resourceServiceRoleEntityRef == null) return;

            resourceBookingEntity["ap360_serviceroles"] = resourceServiceRoleEntityRef;
            organizationService.Update(resourceBookingEntity);

            tracingService.Trace("Done");
        }

        internal void CalculateBilling(IOrganizationService organizationService, ITracingService tracingService, IWorkflowContext context)
        {
            if (context.Depth > 2) return;

            tracingService.Trace("CalculateBilling starts");
            decimal billingFactor = 1.00m;
            //bookable resource booking
            var entity = organizationService.Retrieve(context.PrimaryEntityName, context.PrimaryEntityId, new ColumnSet("msdyn_workorder", "msdyn_totalbillableduration", "resource", "ap360_serviceroles", "ap360_bookingclassification"));

            //workorder
            var workorderEntityRef = GetLookupAttributeValue(entity, "msdyn_workorder");

            tracingService.Trace("workorderEntityRef starts");

            if (workorderEntityRef == null) return;

            var workorderEntity = organizationService.Retrieve(workorderEntityRef.LogicalName, workorderEntityRef.Id, new ColumnSet("msdyn_pricelist", "msdyn_workordertype"));

            tracingService.Trace("workorderEntity starts");

            if (workorderEntity == null) return;

            var priceList = GetLookupAttributeValue(workorderEntity, "msdyn_pricelist");

            if (priceList == null) return;


            var billableDuration = GetIntAttributeValue(entity, "msdyn_totalbillableduration");

            tracingService.Trace("billableDuration:" + billableDuration);

            var bookingClasificationEntityRef = GetLookupAttributeValue(entity, "ap360_bookingclassification");
            if (bookingClasificationEntityRef != null)
            {
                var bookingClasificationEntity = organizationService.Retrieve(bookingClasificationEntityRef.LogicalName, bookingClasificationEntityRef.Id, new ColumnSet("ap360_billingfactor"));
                if (bookingClasificationEntity != null)
                    billingFactor = GetDecimalAttributeValue(bookingClasificationEntity, "ap360_billingfactor");
            }

            var serviceRoleEntityRef = GetLookupAttributeValue(entity, "ap360_serviceroles");

            if (serviceRoleEntityRef == null)
                throw new InvalidPluginExecutionException("Select Service role first.");

            var workorderType = GetLookupAttributeValue(workorderEntity, "msdyn_workordertype");

            if (workorderType != null)
            {
                if (workorderType.Id == new Guid("9CD85B74-CAF7-E811-A980-000D3A11F5EE"))
                {
                    billingFactor = 0.00m;
                    tracingService.Trace("Workorder type is admin, billing factor will be :" + billingFactor);
                }
            }

            tracingService.Trace("billingFactor:" + billingFactor);

            var price = GetRolePrice(organizationService, tracingService, priceList.Id, serviceRoleEntityRef.Id);

            tracingService.Trace("price:" + price);

            var billingPrice = ((decimal)billableDuration / 60) * (billingFactor * price);
            tracingService.Trace("billingPrice:" + billingPrice);
            entity["ap360_billableamount"] = new Money(billingPrice);
            entity["ap360_totalbillableamount"] = new Money(billingPrice);

            organizationService.Update(entity);
        }

        internal void CopyWOSTInBooking(IOrganizationService organizationService, ITracingService tracingService, IWorkflowContext context)
        {
            if (context.Depth > 2) return;

            tracingService.Trace("CopyWOSTInBooking Starts ");
            var resourceBookingEntity = organizationService.Retrieve(context.PrimaryEntityName, context.PrimaryEntityId, new ColumnSet("resource", "msdyn_workorder", "ap360_workorderservicetask"));

            var workorderEntityRef = GetLookupAttributeValue(resourceBookingEntity, "msdyn_workorder");
            if (workorderEntityRef == null) return;


            EntityReference workorderservicetaskRef = GetLookupAttributeValue(resourceBookingEntity, "ap360_workorderservicetask");
            if (workorderservicetaskRef == null) return;

            var serviceTasks = GetWorkOrderServiceTasks(organizationService, tracingService, workorderEntityRef.Id);

            foreach (Entity serviceTask in serviceTasks)
            {
                var name = GetStringAttributeValue(serviceTask, "msdyn_name");
                var description = GetStringAttributeValue(serviceTask, "msdyn_description");
                var woserviceTaskPercentComplete = GetFloatAttributeValue(serviceTask, "msdyn_percentcomplete");


                var bookingServiceTask = new Entity("ap360_bookingservicetask");
                bookingServiceTask["ap360_bookableresourcebooking"] = new EntityReference(context.PrimaryEntityName, context.PrimaryEntityId);
                bookingServiceTask["ap360_workorderservicetask"] = new EntityReference(serviceTask.LogicalName, serviceTask.Id);
                if (workorderservicetaskRef.Id == serviceTask.Id)
                {
                    bookingServiceTask["ap360_ismasterbst"] = true;
                }
                bookingServiceTask["ap360_name"] = name;
                bookingServiceTask["ap360_completed"] = Convert.ToDouble(woserviceTaskPercentComplete);

                organizationService.Create(bookingServiceTask);
                tracingService.Trace("Service Task Created: " + name);
            }
        }

        public static Guid CreateBookAbleResourceBooking(IOrganizationService service, ITracingService tracingService, Entity entity, IPluginExecutionContext context, String bookingType, string isBookingStarted)
        {
            Entity newlyBRB = new Entity("bookableresourcebooking");
            EntityReference wost = null;
            EntityReference resource = null;
            EntityReference bookingstatus = null;
            EntityReference workOrder = null;
            tracingService.Trace("start");

            newlyBRB["starttime"] = DateTime.Now;
            // if (bookingType == "StartWork")
            // newlyBRB["msdyn_actualarrivaltime"] = DateTime.Now.AddMinutes(1);
            newlyBRB["msdyn_estimatedarrivaltime"] = DateTime.Now.AddMinutes(1);
            newlyBRB["endtime"] = DateTime.Now.AddMinutes(60);
            TimeZoneInfo targetZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            DateTime newDT = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, targetZone);
            if (isBookingStarted == "yes")// only for bookings which are directlty started from WOST
            {
                newlyBRB["ap360_inprogresstime"] = newDT;
            }
            EntityReference serviceRole = entity.GetAttributeValue<EntityReference>("ap360_serviceroles") ?? null;
            if (serviceRole != null)
                newlyBRB["ap360_serviceroles"] = new EntityReference(serviceRole.LogicalName, serviceRole.Id);

            newlyBRB["duration"] = 200;

            if (entity.LogicalName == "bookableresourcebooking")
            {
                wost = entity.GetAttributeValue<EntityReference>("ap360_workorderservicetask") ?? null;
                if (wost != null)
                {
                   
                    newlyBRB["name"] = wost.Name + " " + newDT.ToString("MM/dd/yyyy H:mm");
                }
                resource = entity.GetAttributeValue<EntityReference>("resource") ?? null;
                newlyBRB["bookingstatus"] = new EntityReference("bookingstatus", new Guid("f16d80d1-fd07-4237-8b69-187a11eb75f9"));// Scheduled
                EntityReference bookingsetupmetadata = entity.GetAttributeValue<EntityReference>("msdyn_bookingsetupmetadataid") ?? null;
                //throw new InvalidPluginExecutionException(bookingType + "inside if");
                if (bookingsetupmetadata != null)
                    newlyBRB["msdyn_bookingsetupmetadataid"] = new EntityReference(bookingsetupmetadata.LogicalName, bookingsetupmetadata.Id);
                EntityReference resourcerequirement = entity.GetAttributeValue<EntityReference>("msdyn_resourcerequirement") ?? null;
                if (resourcerequirement != null)
                    newlyBRB["msdyn_resourcerequirement"] = new EntityReference(resourcerequirement.LogicalName, resourcerequirement.Id);
            }
            else if (entity.LogicalName == "msdyn_workorderservicetask")
            {
                wost = entity.ToEntityReference();
                newlyBRB["name"] = wost.Name;
                resource = BookableResource.retreiveResoruce(service, context.UserId).ToEntityReference();
                if (bookingType == "PickTask")
                {
                    newlyBRB["bookingstatus"] = new EntityReference("bookingstatus", new Guid("f16d80d1-fd07-4237-8b69-187a11eb75f9"));// Scheduled
                }
                else if (bookingType == "StartWork")
                {
                    newlyBRB["bookingstatus"] = new EntityReference("bookingstatus", new Guid("53f39908-d08a-4d9c-90e8-907fd7bec07d"));//InProgress
                }
            }
            tracingService.Trace("middle");
            if (wost != null)
                newlyBRB["ap360_workorderservicetask"] = new EntityReference(wost.LogicalName, wost.Id);

            if (resource != null)
                newlyBRB["resource"] = new EntityReference(resource.LogicalName, resource.Id);

            newlyBRB["bookingtype"] = new OptionSetValue(1);//Solid

            workOrder = entity.GetAttributeValue<EntityReference>("msdyn_workorder") ?? null;
            tracingService.Trace("after middle");
            if (workOrder != null)
            {
                //newlyBRB["msdyn_workorder"] = new EntityReference(workOrder.LogicalName, workOrder.Id);
                tracingService.Trace("workOrder not null");
                mapBookingClassificationOnBooking(service, tracingService, workOrder, newlyBRB);
            }


            tracingService.Trace("end");

            Guid guid = service.Create(newlyBRB);
            tracingService.Trace("After create");
            return guid;

        }

        public static void mapBookingClassificationOnBooking(IOrganizationService service, ITracingService tracingService, EntityReference workOrderRef, Entity newlyBRB)
        {
            tracingService.Trace("inside mapBookingClassificationOnBooking");
            Entity reterivedWorkOrder = service.Retrieve(workOrderRef.LogicalName, workOrderRef.Id, new ColumnSet("ap360_workorderbwtype"));
            OptionSetValue workOrderType = reterivedWorkOrder.GetAttributeValue<OptionSetValue>("ap360_workorderbwtype");
            if (workOrderType != null)
            {

                if (workOrderType.Value == 126300004 ||//Production Mechanical
                  workOrderType.Value == 126300005 ||//Production BodyShop
                  workOrderType.Value == 126300006 ||//Production Electrical
                  workOrderType.Value == 126300000 ||//Assesment- Inspection
                  workOrderType.Value == 126300009 ||//Assessment- Diagnostic
                  workOrderType.Value == 126300007 ||//Production Upholstery
                  workOrderType.Value == 126300008//Admin
                 )
                //Test 
                {
                    newlyBRB["ap360_bookingclassification"] = new EntityReference("ap360_bookingclassification", new Guid("20174c8a-a858-ea11-a811-000d3a30f257"));//Standard
                }
                else if (workOrderType.Value == 126300002)//Protocol
                {
                    newlyBRB["ap360_bookingclassification"] = new EntityReference("ap360_bookingclassification", new Guid("1a174c8a-a858-ea11-a811-000d3a30f257"));//Protocol
                }
                else if (workOrderType.Value == 126300003)//Shop
                {
                    newlyBRB["ap360_bookingclassification"] = new EntityReference("ap360_bookingclassification", new Guid("1e174c8a-a858-ea11-a811-000d3a30f257"));//Shop
                }
                else if (workOrderType.Value == 126300001)//Development
                {
                    newlyBRB["ap360_bookingclassification"] = new EntityReference("ap360_bookingclassification", new Guid("d261769b-56c2-eb11-bacc-000d3a3345ab"));//Development
                }
            }
            tracingService.Trace("end of mapBookingClassificationOnBooking");

        }

        public static void UpdateOldBRBOnCreationOFNewBRB(IOrganizationService service, Guid oldBRBGuid)
        {
            Entity oldBRB = new Entity("bookableresourcebooking");
            oldBRB.Id = oldBRBGuid;
            oldBRB["bookingstatus"] = new EntityReference("bookingstatus", new Guid("17e92808-7e59-ea11-a811-000d3a33f3c3"));// Finished
            service.Update(oldBRB);

        }


        internal decimal GetBillingPrice(IOrganizationService service, ITracingService tracingService, Guid brbGuid, string TimeCalculationField, int WOSTTimeStampCount)
        {
            tracingService.Trace("Inside Update WorkOrderActualAmount Labor Amount Function");
            Entity entity = service.Retrieve("bookableresourcebooking", brbGuid, new ColumnSet("ap360_billingfactor", "resource", "ap360_serviceroles", TimeCalculationField));
            if (entity == null) return 0;

            BookableResourceBooking bookableResourceBooking = new BookableResourceBooking();
            bookableResourceBooking.BillingFactor = entity.GetAttributeValue<decimal>("ap360_billingfactor");
            int timespentORBillableDuraiton = entity.GetAttributeValue<int>(TimeCalculationField);
            decimal billingPrice = 0;
            bookableResourceBooking.Resource = entity.GetAttributeValue<EntityReference>("resource") != null ? entity.GetAttributeValue<EntityReference>("resource") : null;
            bookableResourceBooking.ServiceRole = entity.GetAttributeValue<EntityReference>("ap360_serviceroles") != null ? entity.GetAttributeValue<EntityReference>("ap360_serviceroles") : null;

            if (bookableResourceBooking.ServiceRole != null)
            {
                string serviceRoleName = null;
                serviceRoleName = bookableResourceBooking.ServiceRole.Name;
                if (serviceRoleName != null)
                {
                    tracingService.Trace("Service Role Name " + serviceRoleName);
                    decimal price = Methods.GetResourcePriceBasedOnBRC(service, tracingService, bookableResourceBooking.Resource.Id, serviceRoleName);
                    tracingService.Trace("Service Role Price " + price.ToString());
                    tracingService.Trace("Time Spent on Booking " + timespentORBillableDuraiton.ToString());
                    tracingService.Trace("Total Billabe Duration " + TotalBillableDuration.ToString());
                    billingPrice = ((decimal)timespentORBillableDuraiton / 60) * (bookableResourceBooking.BillingFactor * price);
                    tracingService.Trace("Billing Price " + billingPrice.ToString());



                    tracingService.Trace("BRB updated");
                }
            }
            else
            {
                throw new InvalidPluginExecutionException("Service Role is not Selected in WorkOrder Service Task");

            }




            return billingPrice;
            // }

        }


        private void BookableResourceBookingRetriveMultiple(IPluginExecutionContext executionContext, IOrganizationService organizationService, ITracingService tracingService)
        {
            if (executionContext.OutputParameters.Contains("BusinessEntity"))
            {
                //throw new InvalidPluginExecutionException("Reterive");
                //test 
                Entity entity = (Entity)executionContext.OutputParameters["BusinessEntity"];
                var watch1 = System.Diagnostics.Stopwatch.StartNew();

                CalculatePercentagesNew(organizationService, tracingService, entity);
                watch1.Stop();
                var elapsedMs = watch1.ElapsedMilliseconds;
                tracingService.Trace("Time  " + elapsedMs.ToString());

            }
            else if (executionContext.OutputParameters.Contains("BusinessEntityCollection"))
            {
                // throw new InvalidPluginExecutionException("Reterive mulitple");
                var businessEntityCollection = (EntityCollection)executionContext.OutputParameters["BusinessEntityCollection"];
                tracingService.Trace("Record Count:" + businessEntityCollection.Entities.Count.ToString());

                if (executionContext.ParentContext != null)
                    tracingService.Trace("Parent Context " + executionContext.ParentContext.Depth.ToString());
                //   tracingService.Trace(businessEntityCollection.Entities[2].GetAttributeValue<EntityReference>("msdyn_workorder").Name);
                foreach (Entity entity in businessEntityCollection.Entities)
                {
                    CalculatePercentagesNew(organizationService, tracingService, entity);
                }
            }
        }

        private void CalculatePercentages(IOrganizationService organizationService, ITracingService tracingService, Entity entity)
        {
            tracingService.Trace(entity.GetAttributeValue<string>("msdyn_name"));
            var workorderEntityRef = GetLookupAttributeValue(entity, "msdyn_workorder");
            if (workorderEntityRef == null) return;

            var workOrderEstimatedAmount = GetMoneyAttributeValue(entity, "ap360_workorderestimatedamount");
            if (workOrderEstimatedAmount == 0.0m) return;
            tracingService.Trace("WO Estimated Amount:" + workOrderEstimatedAmount);

            var reportedPercentComplete = GetDecimalAttributeValue(entity, "ap360_calculatedbookingpercentcomplete");
            //reportedPercentComplete = (reportedPercentComplete - 226300000) * 20;
            tracingService.Trace("reportedPercentComplete:" + reportedPercentComplete);

            var workOrderEstimatedTime = GetIntAttributeValue(entity, "ap360_estimatedtime");
            tracingService.Trace("workOrderEstimatedTime:" + workOrderEstimatedTime);

            var workOrderBookings = GetWorkOrderBookings(organizationService, tracingService, workorderEntityRef.Id);
            decimal totalBillingPrice = 0.0m;
            int totalTimeSpentOnWorkOrder = 0;
            decimal currentBookingRolePrice = 0;

            foreach (Entity workOrderBooking in workOrderBookings)
            {
                var serviceRoleEntityRef = GetLookupAttributeValue(workOrderBooking, "ap360_serviceroles");
                if (serviceRoleEntityRef == null) continue;
                var billingFactor = GetDecimalAttributeValue(workOrderBooking, "ap360_billingfactor");
                var timeSpentonWorkOrder = GetIntAttributeValue(workOrderBooking, "ap360_timespentonbooking");
                var price = GetRolePrice(organizationService, tracingService, new Guid("F75B4A03-646D-E811-A96F-000D3A11E605"), serviceRoleEntityRef.Id);


                var billingPrice = ((decimal)timeSpentonWorkOrder / 60) * (billingFactor * price);
                totalBillingPrice = totalBillingPrice + billingPrice;



                totalTimeSpentOnWorkOrder = totalTimeSpentOnWorkOrder + timeSpentonWorkOrder;

                if (workOrderBooking.Id == entity.Id)
                    currentBookingRolePrice = price;
            }




            tracingService.Trace("Total Time Spent:" + totalTimeSpentOnWorkOrder);
            tracingService.Trace("Total Billing Amount:" + totalBillingPrice);

            if (entity.Contains("ap360_workorderactualamount"))
                entity["ap360_workorderactualamount"] = new Money(totalBillingPrice);
            else
                entity.Attributes.Add("ap360_workorderactualamount", new Money(totalBillingPrice));

            if (entity.Contains("ap360_timespentonworkorder"))
                entity["ap360_timespentonworkorder"] = totalTimeSpentOnWorkOrder;
            else
                entity.Attributes.Add("ap360_timespentonworkorder", totalTimeSpentOnWorkOrder);

            var dollarPercentSpent = (totalBillingPrice / workOrderEstimatedAmount) * 100;
            if (entity.Contains("ap360_dollarpercentagespent"))
                entity["ap360_dollarpercentagespent"] = dollarPercentSpent;
            else
                entity.Attributes.Add("ap360_dollarpercentagespent", dollarPercentSpent);


            var variance = reportedPercentComplete - dollarPercentSpent;
            tracingService.Trace("variance:" + variance);
            if (entity.Contains("ap360_variance"))
                entity["ap360_variance"] = variance;
            else
                entity.Attributes.Add("ap360_variance", variance);

            var startingPercentage = 0.0m;
            if (workOrderEstimatedTime > 0)
                startingPercentage = ((decimal)totalTimeSpentOnWorkOrder / workOrderEstimatedTime) * 100;


            if (entity.Contains("ap360_workorderstartingcomplete"))
                entity["ap360_workorderstartingcomplete"] = decimal.ToInt32(startingPercentage);
            else
                entity.Attributes.Add("ap360_workorderstartingcomplete", decimal.ToInt32(startingPercentage));

            if (currentBookingRolePrice > 0)
            {
                var remainingMinutes = (workOrderEstimatedAmount - totalBillingPrice) / currentBookingRolePrice;
                if (entity.Contains("ap360_actualtimeremaining"))
                    entity["ap360_actualtimeremaining"] = decimal.ToInt32(remainingMinutes * 60);
                else
                    entity.Attributes.Add("ap360_actualtimeremaining", decimal.ToInt32(remainingMinutes * 60));

            }
        }

        public static Guid createWOSTBooking(IOrganizationService service, ITracingService tracing, Entity wostEntity, IPluginExecutionContext context)
        {
            tracing.Trace("inside createWOSTBooking");
            EntityReference workOrder = null;
            workOrder = wostEntity.GetAttributeValue<EntityReference>("msdyn_workoder");
            //if (workOrder != null)
            //{
            //    reterviedWorkOrder =service.Retrieve(workOrder.LogicalName, workOrder.Id, new ColumnSet("ap360_workordertype"));
            //    if (reterviedWorkOrder != null)
            //    {
            //        //Production Mechanical  126,300,004
            //        //Production BodyShop  126,300,005
            //        //Production Electrical 126,300,006
            //        int workOrderType = reterviedWorkOrder.GetAttributeValue<OptionSetValue>("ap360_workordertype").Value;


            //    }
            //}

            Entity createBookableResourceBooking = new Entity("bookableresourcebooking");

            createBookableResourceBooking["name"] = wostEntity.GetAttributeValue<string>("msdyn_name");
            createBookableResourceBooking["bookingstatus"] = new EntityReference("bookingstatus", new Guid("53f39908-d08a-4d9c-90e8-907fd7bec07d"));//Inprogress
            createBookableResourceBooking["ap360_workorderservicetask"] = new EntityReference(wostEntity.LogicalName, wostEntity.Id);
            EntityReference workOrderRef = null;
            Entity reterivedWorkOrder = null;
            if (wostEntity.GetAttributeValue<EntityReference>("msdyn_workorder") != null)
            {
                createBookableResourceBooking["msdyn_workorder"] = wostEntity.GetAttributeValue<EntityReference>("msdyn_workorder");

                workOrderRef = wostEntity.GetAttributeValue<EntityReference>("msdyn_workorder");
                reterivedWorkOrder = service.Retrieve(workOrderRef.LogicalName, workOrderRef.Id, new ColumnSet("ap360_workorderbwtype"));
                OptionSetValue workOrderType = reterivedWorkOrder.GetAttributeValue<OptionSetValue>("ap360_workorderbwtype");
                if (workOrderType != null)
                {

                    if (workOrderType.Value == 126300004 ||//Production Mechanical
                      workOrderType.Value == 126300005 ||//Production BodyShop
                      workOrderType.Value == 126300006 ||//Production Electrical
                      workOrderType.Value == 126300000 ||//Assesment- Inspection
                      workOrderType.Value == 126300009 ||//Assessment- Diagnostic
                      workOrderType.Value == 126300007 ||//Production Upholstery
                      workOrderType.Value == 126300008//Admin
                     )
                    {
                        createBookableResourceBooking["ap360_bookingclassification"] = new EntityReference("ap360_bookingclassification", new Guid("20174c8a-a858-ea11-a811-000d3a30f257"));//Standard


                    }
                    else if (workOrderType.Value == 126300002)
                    {
                        createBookableResourceBooking["ap360_bookingclassification"] = new EntityReference("ap360_bookingclassification", new Guid("1a174c8a-a858-ea11-a811-000d3a30f257"));//Protocol
                    }
                    else if (workOrderType.Value == 126300003)
                    {
                        createBookableResourceBooking["ap360_bookingclassification"] = new EntityReference("ap360_bookingclassification", new Guid("1e174c8a-a858-ea11-a811-000d3a30f257"));//Shop

                    }
                    else if (workOrderType.Value == 126300001)//Development
                    {
                        createBookableResourceBooking["ap360_bookingclassification"] = new EntityReference("ap360_bookingclassification", new Guid("d261769b-56c2-eb11-bacc-000d3a3345ab"));//Development

                    }
                }

            }
            createBookableResourceBooking["ap360_inprogresstime"] = DateTime.Now;

            createBookableResourceBooking["starttime"] = DateTime.Now;
            if (wostEntity.GetAttributeValue<EntityReference>("ap360_serviceroleid") != null)
            {
                // throw new InvalidPluginExecutionException("Service Role exists");

                createBookableResourceBooking["ap360_serviceroles"] = wostEntity.GetAttributeValue<EntityReference>("ap360_serviceroleid");
            }
            else
            {
                // throw new InvalidPluginExecutionException("Service not exists");
                createBookableResourceBooking["ap360_serviceroles"] = new EntityReference("bookableresourcecategory", new Guid("108c5d2f-245e-ea11-a812-000d3a30f257"));//Mechanical 
            }

            createBookableResourceBooking["endtime"] = DateTime.Now.AddMinutes(31);
            createBookableResourceBooking["msdyn_actualarrivaltime"] = DateTime.Now.AddMinutes(30);

            Entity resource = BookableResource.retreiveResoruce(service, context.UserId);
            createBookableResourceBooking["resource"] = new EntityReference(resource.LogicalName, resource.Id);
            //createBookableResourceBooking["currency"] = "dollar";
            //createBookableResourceBooking["status"] = new OptionSetValue(1);//active

            createBookableResourceBooking["bookingtype"] = new OptionSetValue(1);//Solid

            Guid newlyCreatedBookingGuid = Guid.Empty;
            newlyCreatedBookingGuid = service.Create(createBookableResourceBooking);
            tracing.Trace("newly created booking id " + newlyCreatedBookingGuid.ToString());
            return newlyCreatedBookingGuid;
        }
        //private static int CalculateDollarTimeRemaining(IOrganizationService organizationService, ITracingService tracingService, Entity entity)
        //{


        //    EntityReference bookableResource = null;
        //    EntityReference WOSTRef = null;
        //    if (GetLookupAttributeValue(entity, "resource") != null)
        //    {
        //        bookableResource = GetLookupAttributeValue(entity, "resource");
        //        tracingService.Trace(GetLookupAttributeValue(entity, "resource").Name);
        //    }
        //    else
        //    {
        //        tracingService.Trace("Techanician is not mapped");
        //        return 0;
        //    }

        //    EntityReference bookingClassification = GetLookupAttributeValue(entity, "ap360_bookingclassification");

        //    //decimal WOTotalRevisedEstimatedLaborAmount = 0;
        //    //decimal WOTotalActualWOSTLaboramount = 0;
        //    decimal WOSTActualAmount = 0;
        //    EntityReference WOSTServiceRole = null;
        //    decimal WOSTrevisedestimatedAmount = 0;
        //    decimal WOSToriginalEstimatedAmount = 0;
        //    decimal WOSTEstiamtedAmount = 0;
        //    int dollarTimeRemaining = 0;
        //    decimal WOStTimeSpent = 0;
        //    decimal WOSTRemainingAmount = 0;
        //    decimal mainBookingResourceHourlyRate = 0;//price
        //    decimal ServiceRoleHourlyRate = 0.0m;//standardprice
        //    Entity WOSTEntity = null;

        //    DataCollection<Entity> bookings = null;
        //    WOSTRef = GetLookupAttributeValue(entity, "ap360_workorderservicetask");
        //    if (WOSTRef != null)
        //    {
        //        tracingService.Trace("Work Order Service Task exists --");
        //        WOSTEntity = organizationService.Retrieve(WOSTRef.LogicalName, WOSTRef.Id, new ColumnSet("ap360_actualamount", "ap360_serviceroleid", "ap360_revisedestimatedamount", "ap360_originalestimatedamount"));
        //        WOSTEstiamtedAmount = GetWOSTEstimatedDuration(out WOSTrevisedestimatedAmount, out WOSToriginalEstimatedAmount, out WOSTEstiamtedAmount, WOSTEntity);
        //        WOStTimeSpent = CalculateTimeSpentOnWOST(organizationService, tracingService, bookableResource, WOSTRef, out mainBookingResourceHourlyRate, out WOSTActualAmount, out WOSTServiceRole, WOSTEntity, out ServiceRoleHourlyRate, WOStTimeSpent);
        //        tracingService.Trace("Estimated Amount " + WOSTEstiamtedAmount.ToString() + " Acutal Amount  " + WOSTActualAmount.ToString() + "Main Booking, Resoruce Hourly Rate " + mainBookingResourceHourlyRate.ToString());

        //        if (mainBookingResourceHourlyRate > 0)
        //        {
        //            WOSTRemainingAmount = ((WOSTEstiamtedAmount - WOSTActualAmount) / mainBookingResourceHourlyRate) * 60;
        //            tracingService.Trace(WOSTRemainingAmount.ToString() + "=(" + WOSTEstiamtedAmount.ToString() + "-" + WOSTActualAmount.ToString() + ")/" + mainBookingResourceHourlyRate.ToString() + ")*" + 60.ToString());
        //            tracingService.Trace("WOST Remaining Duration " + WOSTRemainingAmount.ToString());
        //        }             
        //        bookings = GetServiceTaskBookings(organizationService, tracingService, WOSTRef.Id);
        //        tracingService.Trace("bookings count " + bookings.Count.ToString());

        //        if (bookings.Count == 0)
        //        {
        //            tracingService.Trace("In Progress Booking Count = 0");
        //            entity["ap360_actualtimeremaining"] = decimal.ToInt32(WOSTRemainingAmount);
        //            dollarTimeRemaining = decimal.ToInt32(WOSTRemainingAmount);
        //            return dollarTimeRemaining;
        //        }
        //        else
        //        {
        //            decimal totalBillingPrice = 0.0m;
        //            decimal totalTimeSpentOnInProgressBookings = 0;
        //            int estimatedTime = 0;
        //            decimal reportedPercentComplete = 0.0m;
        //            decimal estimatedAmount = 0.0m;
        //            var calculatedEstimatedTime = 0;

        //            LoopThroughInProgressBookings(organizationService, tracingService, WOSTRef, WOSTServiceRole, WOSTActualAmount,
        //                ref ServiceRoleHourlyRate, bookings, ref totalBillingPrice, ref totalTimeSpentOnInProgressBookings, ref estimatedTime,
        //                ref reportedPercentComplete, ref estimatedAmount, ref calculatedEstimatedTime);

        //            entity["ap360_actualtimeremaining"] = decimal.ToInt32(WOSTRemainingAmount - totalTimeSpentOnInProgressBookings);
        //            dollarTimeRemaining = decimal.ToInt32(WOSTRemainingAmount - totalTimeSpentOnInProgressBookings);
        //        }
        //    }
        //    else
        //        return 0;




        //    return dollarTimeRemaining;


        //    // decimal totalTimeSpentOnInPrgressWorkOrderBooking = 0.0m;

        //    //var workorderEntityRef = GetLookupAttributeValue(entity, "msdyn_workorder");

        //    //if (workorderEntityRef != null)
        //    //{
        //    //    tracingService.Trace("Work Order Exists");
        //    //    DataCollection<Entity> workOrderInProgressBookings = null;
        //    //    WorkOrder reterviedWorkOrder = WorkOrder.getWorkOrderEstimatedAndAcutalAmount(organizationService, tracingService, workorderEntityRef.Id);
        //    //    WOTotalRevisedEstimatedLaborAmount = reterviedWorkOrder.woEstimatedAmount;
        //    //    WOTotalActualWOSTLaboramount = reterviedWorkOrder.woActualLaborAmount;
        //    //    workOrderInProgressBookings = GetWorkOrdersINPROGRESSBookings(organizationService, tracingService, workorderEntityRef.Id);

        //    //    if (workOrderInProgressBookings.Count == 0)
        //    //    {
        //    //        if (mainBookingResourceHourlyRate > 0)
        //    //        {
        //    //            entity["ap360_wotimeremaining"] = ((WOTotalRevisedEstimatedLaborAmount - WOTotalActualWOSTLaboramount) / mainBookingResourceHourlyRate) * 60;
        //    //        }
        //    //        return decimal.ToInt32(((WOTotalRevisedEstimatedLaborAmount - WOTotalActualWOSTLaboramount) / mainBookingResourceHourlyRate) * 60);

        //    //    }
        //    //    // throw new InvalidPluginExecutionException("after Custom errr");

        //    //    tracingService.Trace("WO Inprogress booking Count " + workOrderInProgressBookings.Count.ToString());

        //    //    foreach (Entity bookableResourceBooking in workOrderInProgressBookings)
        //    //    {
        //    //        EntityReference resource = GetLookupAttributeValue(bookableResourceBooking, "resource");
        //    //        if (resource == null) continue;
        //    //        var timespentonbookingForWorkOrderCalculation = GetIntAttributeValue(bookableResourceBooking, "ap360_timespentonbooking");
        //    //        Guid blackWolfPriceListGuid = new Guid("4fe16dd5-8e55-ea11-a811-000d3a33f3c3");
        //    //        EntityReference WOSTServiceRoleForWorkOrderCalculation = GetLookupAttributeValue(bookableResourceBooking, "ap360_serviceroleid");
        //    //        decimal ServiceRoleHourlyRateForWorkOrderCalculation = 0.0m;
        //    //        decimal resourceHourlyRateForWorkOrderCalculation = 0.0m;
        //    //        if (WOSTServiceRoleForWorkOrderCalculation != null)
        //    //        {
        //    //            ServiceRoleHourlyRateForWorkOrderCalculation = Methods.getBookAbleResourceCategoryStandardPrice(organizationService, WOSTServiceRoleForWorkOrderCalculation.Name);
        //    //            resourceHourlyRateForWorkOrderCalculation = Methods.GetResourcePriceBasedOnBRC(organizationService, tracingService, resource.Id, WOSTServiceRoleForWorkOrderCalculation.Name);
        //    //        }
        //    //        else
        //    //        {
        //    //            ServiceRoleHourlyRateForWorkOrderCalculation = Methods.getBookAbleResourceCategoryStandardPrice(organizationService, "Mechanical Technician");
        //    //            resourceHourlyRateForWorkOrderCalculation = Methods.GetResourcePriceBasedOnBRC(organizationService, tracingService, resource.Id, "Mechanical Technician");

        //    //        }

        //    //        totalTimeSpentOnInPrgressWorkOrderBooking = totalTimeSpentOnInPrgressWorkOrderBooking + timespentonbookingForWorkOrderCalculation;

        //    //        tracingService.Trace("Total Time Spent on Inprogress booking for WorkOrder calculation " + totalTimeSpentOnInPrgressWorkOrderBooking.ToString());
        //    //    }
        //    //    tracingService.Trace("WO Revised Estiamte Labor Amount " + WOTotalRevisedEstimatedLaborAmount.ToString());
        //    //    tracingService.Trace("WO Actual Estiamte Labor Amount " + WOTotalActualWOSTLaboramount.ToString());
        //    //}

        //    //else
        //    //{
        //    //    tracingService.Trace("Work Order not Exists");
        //    //    return 0;
        //    //}



        //}
        static int  bookingDays = 90;

        private static int CalculatePercentagesNew(IOrganizationService organizationService, ITracingService tracingService, Entity entity)
        {



            EntityReference bookableResource = null;
            EntityReference WOSTRef = null;
            tracingService.Trace("*******************************************************************************************");
            tracingService.Trace("CalculatePercentagesNew Inside fuction ");
            if (GetLookupAttributeValue(entity, "resource") != null)
            {
                bookableResource = GetLookupAttributeValue(entity, "resource");
                tracingService.Trace(GetLookupAttributeValue(entity, "resource").Name);
            }
            else
            {
                tracingService.Trace("Techanician is not mapped");
                return 0;
            }
            EntityReference bookingClassification = GetLookupAttributeValue(entity, "ap360_bookingclassification");

            decimal WOTotalRevisedEstimatedLaborAmount = 0;
            decimal WOTotalActualWOSTLaboramount = 0;
            decimal WOSTActualAmount = 0;
            EntityReference WOSTServiceRole = null;
            decimal WOSTrevisedestimatedAmount = 0;
            decimal WOSToriginalEstimatedAmount = 0;
            decimal WOSTEstiamtedAmount = 0;
            int dollarTimeRemaining = 0;
            decimal WOStTimeSpent = 0;
            decimal WOSTRemainingAmount = 0;
            decimal mainBookingResourceHourlyRate = 0;//price
            decimal ServiceRoleHourlyRate = 0.0m;//standardprice


            Entity WOSTEntity = null;

            DataCollection<Entity> bookings = null;
            WOSTRef = GetLookupAttributeValue(entity, "ap360_workorderservicetask");
            if (WOSTRef != null)
            {
                tracingService.Trace("Work Order Service Task exists --");
                WOSTEntity = organizationService.Retrieve(WOSTRef.LogicalName, WOSTRef.Id, new ColumnSet("ap360_actualamount", "ap360_serviceroleid", "ap360_revisedestimatedamount", "ap360_originalestimatedamount"));
                WOSTEstiamtedAmount = GetWOSTEstimatedDuration(out WOSTrevisedestimatedAmount, out WOSToriginalEstimatedAmount, out WOSTEstiamtedAmount, WOSTEntity);
                WOStTimeSpent = CalculateTimeSpentOnWOST(organizationService, tracingService, bookableResource, WOSTRef, out mainBookingResourceHourlyRate, out WOSTActualAmount,
                    out WOSTServiceRole, WOSTEntity, out ServiceRoleHourlyRate, WOStTimeSpent, entity);


                // throw new InvalidPluginExecutionException("Error "+ elapsedMs.ToString());
                tracingService.Trace("Estimated Amount " + WOSTEstiamtedAmount.ToString() + " Acutal Amount  " + WOSTActualAmount.ToString() + "Main Booking, Resoruce Hourly Rate " + mainBookingResourceHourlyRate.ToString());

                if (mainBookingResourceHourlyRate > 0)
                {
                    WOSTRemainingAmount = ((WOSTEstiamtedAmount - WOSTActualAmount) / mainBookingResourceHourlyRate) * 60;
                    tracingService.Trace(WOSTRemainingAmount.ToString() + "=(" + WOSTEstiamtedAmount.ToString() + "-" + WOSTActualAmount.ToString() + ")/" + mainBookingResourceHourlyRate.ToString() + ")*" + 60.ToString());

                    tracingService.Trace("WOST Remaining Duration " + WOSTRemainingAmount.ToString());
                }
                bookings = GetServiceTaskBookings(organizationService, tracingService, WOSTRef.Id);
                tracingService.Trace("bookings count " + bookings.Count.ToString());
            }
            else
                return 0;



            decimal totalBillingPrice = 0.0m;
            decimal totalTimeSpentOnInProgressBookings = 0;
            int estimatedTime = 0;
            decimal reportedPercentComplete = 0.0m;
            decimal estimatedAmount = 0.0m;
            var calculatedEstimatedTime = 0;

            LoopThroughInProgressBookings(organizationService, tracingService, WOSTRef, WOSTServiceRole, WOSTActualAmount,
                ref ServiceRoleHourlyRate, bookings, ref totalBillingPrice, ref totalTimeSpentOnInProgressBookings, ref estimatedTime,
                ref reportedPercentComplete, ref estimatedAmount, ref calculatedEstimatedTime);

            decimal totalTimeSpentOnInPrgressWorkOrderBooking = 0.0m;

            //throw new InvalidPluginExecutionException("jawad" + dollarTimeRemaining.ToString()+"KAshif "+ totalTimeSpentOnInProgressBookings.ToString());

            var workorderEntityRef = GetLookupAttributeValue(entity, "msdyn_workorder");

            if (workorderEntityRef != null)
            {
                tracingService.Trace("Work Order Exists");
                DataCollection<Entity> workOrderInProgressBookings = null;
                WorkOrder reterviedWorkOrder = WorkOrder.getWorkOrderEstimatedAndAcutalAmount(organizationService, tracingService, workorderEntityRef.Id);
                WOTotalRevisedEstimatedLaborAmount = reterviedWorkOrder.woEstimatedAmount;
                WOTotalActualWOSTLaboramount = reterviedWorkOrder.woActualLaborAmount;
                workOrderInProgressBookings = GetWorkOrdersINPROGRESSBookings(organizationService, tracingService, workorderEntityRef.Id);

                if (workOrderInProgressBookings.Count == 0)
                {
                    if (mainBookingResourceHourlyRate > 0)
                    {
                        entity["ap360_wotimeremaining"] = ((WOTotalRevisedEstimatedLaborAmount - WOTotalActualWOSTLaboramount) / mainBookingResourceHourlyRate) * 60;
                    }
                    //   return decimal.ToInt32(((WOTotalRevisedEstimatedLaborAmount - WOTotalActualWOSTLaboramount) / mainBookingResourceHourlyRate) * 60);

                    // throw new InvalidPluginExecutionException("inside function " + dollarTimeRemaining.ToString());
                }

                // throw new InvalidPluginExecutionException("after Custom errr");

                tracingService.Trace("WO Inprogress booking Count " + workOrderInProgressBookings.Count.ToString());

                foreach (Entity bookableResourceBooking in workOrderInProgressBookings)
                {
                    EntityReference resource = GetLookupAttributeValue(bookableResourceBooking, "resource");
                    if (resource == null) continue;
                    var timespentonbookingForWorkOrderCalculation = GetIntAttributeValue(bookableResourceBooking, "ap360_timespentonbooking");
                    Guid blackWolfPriceListGuid = new Guid("4fe16dd5-8e55-ea11-a811-000d3a33f3c3");
                    EntityReference WOSTServiceRoleForWorkOrderCalculation = GetLookupAttributeValue(bookableResourceBooking, "ap360_serviceroleid");
                    decimal ServiceRoleHourlyRateForWorkOrderCalculation = 0.0m;
                    decimal resourceHourlyRateForWorkOrderCalculation = 0.0m;
                    if (WOSTServiceRoleForWorkOrderCalculation != null)
                    {
                        ServiceRoleHourlyRateForWorkOrderCalculation = Methods.getBookAbleResourceCategoryStandardPrice(organizationService, WOSTServiceRoleForWorkOrderCalculation.Name);
                        resourceHourlyRateForWorkOrderCalculation = Methods.GetResourcePriceBasedOnBRC(organizationService, tracingService, resource.Id, WOSTServiceRoleForWorkOrderCalculation.Name);
                    }
                    else
                    {
                        ServiceRoleHourlyRateForWorkOrderCalculation = bookableResourceBooking.GetAttributeValue<Money>("ap360_servicerolehourlyrate") != null ? bookableResourceBooking.GetAttributeValue<Money>("ap360_servicerolehourlyrate").Value : 0;
                        if (ServiceRoleHourlyRateForWorkOrderCalculation == 0)
                        {
                            tracingService.Trace("ServiceRoleHourlyRate is null on booking");

                            ServiceRoleHourlyRateForWorkOrderCalculation = Methods.getBookAbleResourceCategoryStandardPrice(organizationService, "Mechanical Technician");
                        }
                        resourceHourlyRateForWorkOrderCalculation = bookableResourceBooking.GetAttributeValue<Money>("ap360_resourcehourlyrate") != null ? bookableResourceBooking.GetAttributeValue<Money>("ap360_resourcehourlyrate").Value : 0;
                        if (resourceHourlyRateForWorkOrderCalculation == 0)
                        {
                            tracingService.Trace("Resource hourly rate is null on booking");
                            resourceHourlyRateForWorkOrderCalculation = Methods.GetResourcePriceBasedOnBRC(organizationService, tracingService, resource.Id, "Mechanical Technician");
                        }

                    }







                    totalTimeSpentOnInPrgressWorkOrderBooking = totalTimeSpentOnInPrgressWorkOrderBooking + timespentonbookingForWorkOrderCalculation;

                    tracingService.Trace("Total Time Spent on Inprogress booking for WorkOrder calculation " + totalTimeSpentOnInPrgressWorkOrderBooking.ToString());
                }
                tracingService.Trace("WO Revised Estiamte Labor Amount " + WOTotalRevisedEstimatedLaborAmount.ToString());
                tracingService.Trace("WO Actual Estiamte Labor Amount " + WOTotalActualWOSTLaboramount.ToString());
            }

            else
            {
                tracingService.Trace("Work Order not Exists");
                return 0;
            }

            decimal woRemainingEstimatedAmount = 0;
            tracingService.Trace("mainBookingResourceHourlyRate " + mainBookingResourceHourlyRate.ToString());
            if (mainBookingResourceHourlyRate > 0)
            {
                woRemainingEstimatedAmount = (((WOTotalRevisedEstimatedLaborAmount - WOTotalActualWOSTLaboramount)) / mainBookingResourceHourlyRate) * 60;
            }

            ///////////////////////////////////////////////////
            entity["ap360_wotimeremaining"] = woRemainingEstimatedAmount - totalTimeSpentOnInPrgressWorkOrderBooking;
            // if (entity.Contains("ap360_estimatedamount"))
            entity["ap360_estimatedamount"] = new Money(estimatedAmount);
            // else
            //  entity.Attributes.Add("ap360_estimatedamount", new Money(estimatedAmount));


            ///////////////////////////////////////////////////
            tracingService.Trace("Total Billing Price " + totalBillingPrice);

            if (entity.Contains("ap360_workorderactualamount"))
                entity["ap360_workorderactualamount"] = new Money(totalBillingPrice);
            else
                entity.Attributes.Add("ap360_workorderactualamount", new Money(totalBillingPrice));

            /////////////////////////////////////////

            //if (entity.Contains("ap360_timespentonworkorder"))
            entity["ap360_timespentonworkorder"] = totalTimeSpentOnInProgressBookings + WOSTRemainingAmount;

            if (bookings.Count == 0)
            {
                tracingService.Trace("In Progress Booking Count = 0");
                entity["ap360_actualtimeremaining"] = decimal.ToInt32(WOSTRemainingAmount);
                dollarTimeRemaining = decimal.ToInt32(WOSTRemainingAmount);
                // return dollarTimeRemaining;
                return dollarTimeRemaining;
            }
            //else
            //    entity.Attributes.Add("ap360_timespentonworkorder", totalTimeSpentOnInProgressBookings + WOSTRemainingAmount);

            //var dollarPercentSpent = (totalBillingPrice / estimatedAmount) * 100;
            //if (entity.Contains("ap360_dollarpercentagespent"))
            //    entity["ap360_dollarpercentagespent"] = dollarPercentSpent;
            //else
            //    entity.Attributes.Add("ap360_dollarpercentagespent", dollarPercentSpent);


            //var variance = reportedPercentComplete - dollarPercentSpent;
            //tracingService.Trace("variance:" + variance);
            //if (entity.Contains("ap360_variance"))
            //    entity["ap360_variance"] = variance;
            //else
            //    entity.Attributes.Add("ap360_variance", variance);

            //var startingPercentage = 0.0m;
            //if (estimatedTime > 0)
            //    startingPercentage = ((decimal)totalTimeSpentOnInProgressBookings / estimatedTime) * 100;


            //if (entity.Contains("ap360_workorderstartingcomplete"))
            //    entity["ap360_workorderstartingcomplete"] = decimal.ToInt32(startingPercentage);
            //else
            //    entity.Attributes.Add("ap360_workorderstartingcomplete", decimal.ToInt32(startingPercentage));



            //if (currentBookingRolePrice > 0)
            //{
            //    tracingService.Trace("Remaining minutes = (estimatedAmount - TotalBillingPrice)/RolePrice");

            // var remainingMinutes = (estimatedAmount - totalBillingPrice) / currentBookingRolePrice;
            //tracingService.Trace("Remaining minutes = " + estimatedAmount.ToString() + " - " + totalBillingPrice.ToString() + " / " + currentBookingRolePrice.ToString());
            //tracingService.Trace("Remaining Minutes * 60 =" + decimal.ToInt32(remainingMinutes * 60).ToString());

            //tracingService.Trace("WOST Est Duration " + WOSTrevisedestimatedduration.ToString());
            //tracingService.Trace("WOST time spent  " + timeSpentOnWOST.ToString());
            //tracingService.Trace("totalTimeSpentOnWorkOrder " + totalTimeSpentOnWorkOrder.ToString());

            // throw new InvalidPluginExecutionException(WOSTRemainingAmount.ToString());
            // totalTimeSpentOnInProgressBookings = (totalTimeSpentOnInProgressBookings / ResourceHourlyRate) * 60;
            if (entity.Contains("ap360_actualtimeremaining"))//$ time remaining
            {
                //entity["ap360_actualtimeremaining"] = decimal.ToInt32(remainingMinutes * 60);
                // entity["ap360_actualtimeremaining"] = decimal.ToInt32(60);
                // entity["ap360_actualtimeremaining"] = decimal.ToInt32(timeSpentOnWOST + totalTimeSpentOnWorkOrder);
                entity["ap360_actualtimeremaining"] = decimal.ToInt32(WOSTRemainingAmount - totalTimeSpentOnInProgressBookings);
                dollarTimeRemaining = decimal.ToInt32(WOSTRemainingAmount - totalTimeSpentOnInProgressBookings);
                tracingService.Trace("IF % time remainng " + decimal.ToInt32(WOSTRemainingAmount - totalTimeSpentOnInProgressBookings));
            }

            else
            {
                //entity.Attributes.Add("ap360_actualtimeremaining", decimal.ToInt32(remainingMinutes * 60));
                // entity.Attributes.Add("ap360_actualtimeremaining", decimal.ToInt32(60));
                // entity.Attributes.Add("ap360_actualtimeremaining", decimal.ToInt32((WOSTrevisedestimatedduration - timeSpentOnWOST) - totalTimeSpentOnWorkOrder));
                //entity.Attributes.Add("ap360_actualtimeremaining", 100);
                entity["ap360_actualtimeremaining"] = decimal.ToInt32(WOSTRemainingAmount - totalTimeSpentOnInProgressBookings);
                dollarTimeRemaining = decimal.ToInt32(WOSTRemainingAmount - totalTimeSpentOnInProgressBookings);
                // entity["ap360_actualtimeremaining"] = 100;
                tracingService.Trace("ELSE % time remainng " + decimal.ToInt32(WOSTRemainingAmount - totalTimeSpentOnInProgressBookings));
            }


            //This section is important for scheduled booking Time remaining

            return dollarTimeRemaining;

        }

        private static decimal GetWOSTEstimatedDuration(out decimal WOSTrevisedestimatedAmount, out decimal WOSToriginalEstimatedAmount, out decimal WOSTEstiamtedAmount, Entity WOSTEntity)
        {
            WOSTrevisedestimatedAmount = GetMoneyAttributeValue(WOSTEntity, "ap360_revisedestimatedamount");
            WOSToriginalEstimatedAmount = GetMoneyAttributeValue(WOSTEntity, "ap360_originalestimatedamount");//orginal Estimated Duration

            if (WOSTrevisedestimatedAmount > 0)
            {
                WOSTEstiamtedAmount = WOSTrevisedestimatedAmount;
            }
            else
            {
                WOSTEstiamtedAmount = WOSToriginalEstimatedAmount;
            }
            return WOSTEstiamtedAmount;
        }

        private static decimal CalculateTimeSpentOnWOST(IOrganizationService organizationService, ITracingService tracingService, EntityReference bookalbeResource,
            EntityReference WOSTREf, out decimal ResourceHourlyRate, out decimal WOSTActualAmount, out EntityReference WOSTServiceRole, Entity WOSTEntity,
            out decimal ServiceRoleHourlyRate, decimal WOStTimeSpent, Entity brbEntity)
        {
            decimal perMinuteServiceRoleRate = 0;


            WOSTActualAmount = WOSTEntity.GetAttributeValue<Money>("ap360_actualamount") != null ? WOSTEntity.GetAttributeValue<Money>("ap360_actualamount").Value : 0;
            tracingService.Trace("WOST Actual Amount " + WOSTActualAmount.ToString());
            WOSTServiceRole = WOSTEntity.GetAttributeValue<EntityReference>("ap360_serviceroleid") != null ? WOSTEntity.GetAttributeValue<EntityReference>("ap360_serviceroleid") : null;
            if (WOSTServiceRole == null)
            {
                tracingService.Trace("WOST Service Role is Null");

                WOSTServiceRole = new EntityReference("bookableresourcecategory", new Guid("108c5d2f-245e-ea11-a812-000d3a30f257"));
                ResourceHourlyRate = Methods.GetResourcePriceBasedOnBRC(organizationService, tracingService, bookalbeResource.Id, "Mechanical Technician");
                tracingService.Trace("if After GetResourcePriceBasedOnBRC ");
                ServiceRoleHourlyRate = Methods.getBookAbleResourceCategoryStandardPrice(organizationService, "Mechanical Technician");

            }
            else
            {

                tracingService.Trace("WOST Service Role Name " + WOSTServiceRole.Name);
                ResourceHourlyRate = brbEntity.GetAttributeValue<Money>("ap360_resourcehourlyrate") != null ? brbEntity.GetAttributeValue<Money>("ap360_resourcehourlyrate").Value : 0;
                if (ResourceHourlyRate == 0)
                {
                    tracingService.Trace("Resource hourly rate is null on booking");
                    ResourceHourlyRate = Methods.GetResourcePriceBasedOnBRC(organizationService, tracingService, bookalbeResource.Id, WOSTServiceRole.Name);
                }
                tracingService.Trace("else After GetResourcePriceBasedOnBRC ");
                ServiceRoleHourlyRate = brbEntity.GetAttributeValue<Money>("ap360_servicerolehourlyrate") != null ? brbEntity.GetAttributeValue<Money>("ap360_servicerolehourlyrate").Value : 0;
                if (ServiceRoleHourlyRate == 0)
                {
                    tracingService.Trace("ServiceRoleHourlyRate is null on booking");
                    ServiceRoleHourlyRate = Methods.getBookAbleResourceCategoryStandardPrice(organizationService, WOSTServiceRole.Name);
                }
            }
            tracingService.Trace("After Standard Price");
            tracingService.Trace("Resouce Hourly Rate " + ResourceHourlyRate.ToString());
            perMinuteServiceRoleRate = ResourceHourlyRate / 60;
            tracingService.Trace("Per Minute Price" + perMinuteServiceRoleRate.ToString());
            if (perMinuteServiceRoleRate > 0)
            {
                WOStTimeSpent = WOSTActualAmount / perMinuteServiceRoleRate;
                tracingService.Trace("Actual  Time Spent on WOST " + WOStTimeSpent.ToString());
            }

            return WOStTimeSpent;
        }
       

        private static void LoopThroughInProgressBookings(IOrganizationService organizationService, ITracingService tracingService, EntityReference WOSTRef,
            EntityReference WOSTServiceRole, decimal WOSTActualAmount, ref decimal ServiceRoleHourlyRate,
            DataCollection<Entity> bookings, ref decimal totalBillingPrice, ref decimal totalTimeSpentOnInProgressBookings, ref int estimatedTime,
           ref decimal reportedPercentComplete, ref decimal estimatedAmount,
            ref int calculatedEstimatedTime)
        {



            int count = 0;
            foreach (Entity booking in bookings)
            {
                count++;
                tracingService.Trace("Booking #" + count.ToString());
                var bookingPercentComplete = 0.0m;
                var currentBookingbillingPrice = 0.0m;
                decimal currentBookingResouceHourlyRate;
                tracingService.Trace("Initially totalTimeSpentOnInProgressBookings " + totalTimeSpentOnInProgressBookings);
                tracingService.Trace("********************************");
                tracingService.Trace("Inside Foreach");


                EntityReference resource = GetLookupAttributeValue(booking, "resource");
                if (resource == null) continue;
                tracingService.Trace("          After Fetching Resource : " + resource.Name);
                var billingFactor = GetDecimalAttributeValue(booking, "ap360_billingfactor");
                var timespentonbooking = GetIntAttributeValue(booking, "ap360_timespentonbooking");
                tracingService.Trace("Time Spent on Booking " + timespentonbooking);
                Guid blackWolfPriceListGuid = new Guid("4fe16dd5-8e55-ea11-a811-000d3a33f3c3");
                calculatedEstimatedTime = GetIntAttributeValue(booking, "ap360_estimatedtime");
                bookingPercentComplete = GetDecimalAttributeValue(booking, "ap360_finishcomplete");
                tracingService.Trace("Before GetResourcePriceBasedOnBRC");
                if (WOSTServiceRole != null)
                {
                    tracingService.Trace("WOST " + WOSTServiceRole);
                    currentBookingResouceHourlyRate = Methods.GetResourcePriceBasedOnBRC(organizationService, tracingService, resource.Id, WOSTServiceRole.Name);
                    tracingService.Trace("After GetResourcePriceBasedOnBRC");
                    //ServiceRoleHourlyRate = Methods.ge22tBookAbleResourceCategoryStandardPrice(organizationService, WOSTServiceRole.Name);
                    // price = Methods.GetResourcePriceBasedOnBRC(organizationService, resource.Id, WOSTServiceRole.Name);

                    currentBookingbillingPrice = ((decimal)timespentonbooking / 60) * (billingFactor * currentBookingResouceHourlyRate);
                    tracingService.Trace("This rate could be different then main: and this  Resource Hourly Rete " + currentBookingResouceHourlyRate.ToString() + " Time spent on booking " + timespentonbooking.ToString());
                    tracingService.Trace(resource.Name + " current Booking Billing Price " + currentBookingbillingPrice.ToString());
                    totalTimeSpentOnInProgressBookings = totalTimeSpentOnInProgressBookings + timespentonbooking;
                    tracingService.Trace("After totalTimeSpentOnInProgressBookings " + totalTimeSpentOnInProgressBookings + " timespentonbooking " + timespentonbooking.ToString());
                    totalBillingPrice += currentBookingbillingPrice;

                    //tracingService.Trace("          Booking Id == Entity ID");
                    //reportedPercentComplete = bookingPercentComplete;
                    estimatedTime = calculatedEstimatedTime;
                }
                //tracingService.Trace("          Standard Role  price :" + standardPrice);
                //estimatedAmount = ((decimal)estimatedTime / 60) * standardPrice;

                //estimatedAmount = estimatedAmount - WOSTActualAmount;
                tracingService.Trace("End Foreach");



            }
        }

        public static List<BookableResourceBooking> getSecheduledBookingforServiceTask(IOrganizationService service, ITracingService tracing, Guid workOrderServiceTaskGuid)
        {
            List<BookableResourceBooking> lstBookableResourceBookings = new List<BookableResourceBooking>();
            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='bookableresourcebooking'>
                                    <attribute name='createdon' />
                                    <attribute name='starttime' />
 
                                    <attribute name='bookableresourcebookingid' />
                                    <order attribute='starttime' descending='true' />
                                    <filter type='and'>
                                     <condition attribute='bookingstatus' operator='eq'  value='{F16D80D1-FD07-4237-8B69-187A11EB75F9}' />
                                      <condition attribute='ap360_workorderservicetask' operator='eq' value = '" + workOrderServiceTaskGuid + @"' />
                                    </filter>
                                  </entity>
                                </fetch>");


            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            BookableResourceBooking bookableResourceBooks = new BookableResourceBooking();
            foreach (Entity entity in col.Entities)
            {

                bookableResourceBooks.BRBGuid = entity.Id;
                lstBookableResourceBookings.Add(bookableResourceBooks);
            }
            return lstBookableResourceBookings;

        }



        public static List<BookableResourceBooking> getSecheduledBookingforWorkOrder(IOrganizationService service, ITracingService tracing, Guid workOrderGuid)
        {
            List<BookableResourceBooking> lstBookableResourceBookings = new List<BookableResourceBooking>();
            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='bookableresourcebooking'>
                                    <attribute name='createdon' />
                                    <attribute name='starttime' />
 
                                    <attribute name='bookableresourcebookingid' />
                                    <order attribute='starttime' descending='true' />
                                    <filter type='and'>
                                     <condition attribute='bookingstatus' operator='eq'  value='{F16D80D1-FD07-4237-8B69-187A11EB75F9}' />
                                      <condition attribute='msdyn_workorder' operator='eq' value = '" + workOrderGuid + @"' />
                                    </filter>
                                  </entity>
                                </fetch>");


            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            BookableResourceBooking bookableResourceBooks = new BookableResourceBooking();
            foreach (Entity entity in col.Entities)
            {

                bookableResourceBooks.BRBGuid = entity.Id;
                lstBookableResourceBookings.Add(bookableResourceBooks);
            }
            return lstBookableResourceBookings;

        }
        private void CheckExistingInProgressBookings(IOrganizationService organizationService, ITracingService tracingService, Entity bookableResourceBookingEntity)
        {
            tracingService.Trace("CheckExistingInProgressBookings starts");
            var entity = organizationService.Retrieve(bookableResourceBookingEntity.LogicalName, bookableResourceBookingEntity.Id, new ColumnSet("resource", "bookingstatus"));

            var resourceEntityRef = GetLookupAttributeValue(entity, "resource");
            var bookingStatusEntityRef = GetLookupAttributeValue(entity, "bookingstatus");

            if (resourceEntityRef != null && bookingStatusEntityRef != null)
            {
                var inProgressBookings = GetInProgressBookings(organizationService, tracingService, resourceEntityRef.Id, bookingStatusEntityRef.Id);
                tracingService.Trace("inProgressBookings count" + inProgressBookings);
                if (inProgressBookings > 0)
                    throw new InvalidPluginExecutionException("Another booking is already in progress for this resource.");
            }

        }
        private static bool getbookingType(IOrganizationService organizationService)
        {

            bool bookingType = false;
            Entity booking = organizationService.Retrieve("ap360_bookingservicetask", new Guid("DAC0F729-D4BF-EC11-A7B6-000D3A37C2BE"), new ColumnSet(true));
            if (booking != null)
            {
                DateTime bookingDate = booking.GetAttributeValue<DateTime>("createdon");
                bookingDate = bookingDate.AddDays(bookingDays);
                if (DateTime.Now > bookingDate)
                {
                    return true;
                }
            }
            return false;
        }
        private void CalculateRolePrice(IOrganizationService organizationService, ITracingService tracingService, Entity bookableResourceBookingEntity)
        {
            tracingService.Trace("CalculateRolePrice starts");

            var entity = organizationService.Retrieve(bookableResourceBookingEntity.LogicalName, bookableResourceBookingEntity.Id, new ColumnSet("msdyn_workorder", "ap360_serviceroles"));

            var workorderEntityRef = GetLookupAttributeValue(entity, "msdyn_workorder");

            // tracingService.Trace("workorderEntityRef starts");

            if (workorderEntityRef == null) return;

            var workorderEntity = organizationService.Retrieve(workorderEntityRef.LogicalName, workorderEntityRef.Id, new ColumnSet("msdyn_pricelist"));

            // tracingService.Trace("workorderEntity starts");

            if (workorderEntity == null) return;

            var priceList = GetLookupAttributeValue(workorderEntity, "msdyn_pricelist");

            if (priceList == null) return;

            var serviceRoleEntityRef = GetLookupAttributeValue(entity, "ap360_serviceroles");

            if (serviceRoleEntityRef == null)
                throw new InvalidPluginExecutionException("Select Service role first.");

            var price = GetRolePrice(organizationService, tracingService, priceList.Id, serviceRoleEntityRef.Id);

            tracingService.Trace("Role price:" + price);

            bookableResourceBookingEntity["ap360_roleprice"] = new Money(price);
            organizationService.Update(bookableResourceBookingEntity);

        }

        private void CalculateBillableAmount(IOrganizationService organizationService, ITracingService tracingService, Entity bookableResourceBookingEntity)
        {
            tracingService.Trace("CalculateBillableAmount starts");
            decimal billingFactor = 1.00m;
            var entity = organizationService.Retrieve(bookableResourceBookingEntity.LogicalName, bookableResourceBookingEntity.Id, new ColumnSet("msdyn_workorder", "msdyn_totalbillableduration", "resource", "ap360_serviceroles", "ap360_bookingclassification"));

            var workorderEntityRef = GetLookupAttributeValue(entity, "msdyn_workorder");

            // tracingService.Trace("workorderEntityRef starts");

            if (workorderEntityRef == null) return;

            var workorderEntity = organizationService.Retrieve(workorderEntityRef.LogicalName, workorderEntityRef.Id, new ColumnSet("msdyn_pricelist"));

            // tracingService.Trace("workorderEntity starts");

            if (workorderEntity == null) return;

            var priceList = GetLookupAttributeValue(workorderEntity, "msdyn_pricelist");

            if (priceList == null) return;

            var billableDuration = GetIntAttributeValue(entity, "msdyn_totalbillableduration");

            tracingService.Trace("billableDuration:" + billableDuration);

            var bookingClasificationEntityRef = GetLookupAttributeValue(entity, "ap360_bookingclassification");
            if (bookingClasificationEntityRef != null)
            {
                var bookingClasificationEntity = organizationService.Retrieve(bookingClasificationEntityRef.LogicalName, bookingClasificationEntityRef.Id, new ColumnSet("ap360_billingfactor"));
                if (bookingClasificationEntity != null)
                    billingFactor = GetDecimalAttributeValue(bookingClasificationEntity, "ap360_billingfactor");
            }

            var serviceRoleEntityRef = GetLookupAttributeValue(entity, "ap360_serviceroles");

            if (serviceRoleEntityRef == null)
                throw new InvalidPluginExecutionException("Select Service role first.");

            tracingService.Trace("billingFactor:" + billingFactor);

            var price = GetRolePrice(organizationService, tracingService, priceList.Id, serviceRoleEntityRef.Id);

            tracingService.Trace("price:" + price);

            var billingPrice = ((decimal)billableDuration / 60) * (billingFactor * price);
            tracingService.Trace("billingPrice:" + billingPrice);
            bookableResourceBookingEntity["ap360_billableamount"] = new Money(billingPrice);
            bookableResourceBookingEntity["ap360_totalbillableamount"] = new Money(billingPrice);

            organizationService.Update(bookableResourceBookingEntity);

        }

        public static List<BookableResourceBooking> getWorkOrdersBooking(IOrganizationService service, ITracingService traceing, Guid workOrderGuid)
        {
            List<BookableResourceBooking> lstBookableResourceBooking = new List<BookableResourceBooking>();
            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='bookableresourcebooking'>
                                    <attribute name='createdon' />
                                    <attribute name='starttime' />
                                    <attribute name='resource' />
                                    <attribute name='endtime' />
                                    <attribute name='duration' />
                                    <attribute name='bookingtype' />
                                    <attribute name='bookingstatus' />
                                    <attribute name='bookableresourcebookingid' />
                                    <order attribute='starttime' descending='true' />
                                    <filter type='and'>
                                           <condition attribute='msdyn_workorder' operator='eq'  value='" + workOrderGuid + @"' /> 
                                    </filter>
                                  </entity>
                                </fetch>");



            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            // tracingservice.Trace("count of WorkOrderServiceTask " + col.Entities.Count.ToString());
            BookableResourceBooking bookableResourceBooking = null;
            foreach (Entity entity in col.Entities)
            {
                bookableResourceBooking = new BookableResourceBooking();
                bookableResourceBooking.BRBGuid = entity.Id;
                lstBookableResourceBooking.Add(bookableResourceBooking);
            }

            return lstBookableResourceBooking;


        }

        public static List<BookableResourceBooking> getWorkOrdersScheduledBooking(IOrganizationService service, ITracingService traceing, Guid workOrderGuid)
        {
            List<BookableResourceBooking> lstBookableResourceBooking = new List<BookableResourceBooking>();
            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='bookableresourcebooking'>
                                    <attribute name='createdon' />
                                    <attribute name='starttime' />
                                    <attribute name='resource' />
                                    <attribute name='endtime' />
                                    <attribute name='duration' />
                                    <attribute name='bookingtype' />
                                    <attribute name='bookingstatus' />
                                    <attribute name='bookableresourcebookingid' />
                                    <order attribute='starttime' descending='true' />
                                    <filter type='and'>
                                         <condition attribute='bookingstatus' operator='not-in'>
                                                <value uiname='In Progress' uitype='bookingstatus'>{53F39908-D08A-4D9C-90E8-907FD7BEC07D}</value>
                                                <value uiname='Closed' uitype='bookingstatus'>{C33410B9-1ABE-4631-B4E9-6E4A1113AF34}</value>
                                                <value uiname='Finished' uitype='bookingstatus'>{17E92808-7E59-EA11-A811-000D3A33F3C3}</value>
                                                <value uiname='Canceled' uitype='bookingstatus'>{0ADBF4E6-86CC-4DB0-9DBB-51B7D1ED4020}</value>
                                              </condition>
                                           <condition attribute='msdyn_workorder' operator='eq'  value='" + workOrderGuid + @"' /> 
                                    </filter>
                                  </entity>
                                </fetch>");


            //INprogress
            //Closed
            //Finshed
            //Cancelled

            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            // tracingservice.Trace("count of WorkOrderServiceTask " + col.Entities.Count.ToString());
            BookableResourceBooking bookableResourceBooking = null;
            foreach (Entity entity in col.Entities)
            {
                bookableResourceBooking = new BookableResourceBooking();
                bookableResourceBooking.BRBGuid = entity.Id;
                lstBookableResourceBooking.Add(bookableResourceBooking);
            }

            return lstBookableResourceBooking;


        }

        public static string getLastModifiedOnBookingSelectedSubstatusForWorkOrder(IOrganizationService service, ITracingService tracingService, Guid workOrderId)
        {
            string ap360_woselectedsubstatus = null;
            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='bookableresourcebooking'>
                                    <attribute name='createdon' />
                                    <attribute name='starttime' />
                                    <attribute name='resource' />
                                    <attribute name='endtime' />
                                    <attribute name='duration' />
                                    <attribute name='bookingtype' />
                                    <attribute name='bookingstatus' />
                                    <attribute name='bookableresourcebookingid' />
                                    <attribute name='ap360_woselectedsubstatus' />
                                    <attribute name='modifiedon' />
                                    <order attribute='modifiedon' descending='false' />
                                    <filter type='and'>
                                      <condition attribute='msdyn_workorder' operator='eq'  value='" + workOrderId + @"' /> 
                                      <condition attribute='ap360_woselectedsubstatus' operator='not-null' />
                                    </filter>
                                  </entity>
                                </fetch>");



            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            // tracingservice.Trace("count of WorkOrderServiceTask " + col.Entities.Count.ToString());

            if (col.Entities.Count > 0)
            {
                int count = col.Entities.Count;

                ap360_woselectedsubstatus = col.Entities[count - 1].GetAttributeValue<string>("ap360_woselectedsubstatus");
            }

            return ap360_woselectedsubstatus;
        }

        public static decimal GetRolePrice(IOrganizationService organizationService, ITracingService tracingService, Guid priceListId, Guid bookablerresourcecategoryId)
        {

            tracingService.Trace("Before reterving role price");
            decimal price = 0.0m;
            Entity bookableResourceCategory = organizationService.Retrieve("bookableresourcecategory", bookablerresourcecategoryId, new ColumnSet("ap360_price"));
            price = GetMoneyAttributeValue(bookableResourceCategory, "ap360_price");

            //var fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>"
            //   + "<entity name='msdyn_resourcecategorypricelevel'>"
            //   + "<attribute name='msdyn_price' />"
            //   + "<filter type ='and' >"
            //   + "<condition attribute = 'msdyn_pricelist' operator= 'eq'  uitype = 'pricelevel' value = '" + priceListId + "' />"
            //   + " <condition attribute = 'msdyn_resourcecategory' operator= 'eq'  uitype = 'bookableresourcecategory' value = '" + roleId + "' />"
            //   + "</filter >"
            //   + "</entity >"
            //    + "</fetch >";

            //// tracingService.Trace("XML:" + fetchXml);

            //var fetchExpression = new FetchExpression(fetchXml);
            //EntityCollection fetchResult = organizationService.RetrieveMultiple(fetchExpression);

            //if (fetchResult.Entities.Count > 0)
            //{
            //    var rolePriceEntity = fetchResult.Entities[0];
            //    price = GetMoneyAttributeValue(rolePriceEntity, "msdyn_price");
            //}

            return price;
        }

        private int GetInProgressBookings(IOrganizationService organizationService, ITracingService tracingService, Guid resourceId, Guid bookingStatusId)
        {
            var fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>"
               + "<entity name='bookableresourcebooking'>"
               + "<attribute name='bookingstatus' />"
               + "<filter type ='and' >"
               + "<condition attribute = 'resource' operator= 'eq'  uitype = 'bookableresource' value = '" + resourceId + "' />"
               + " <condition attribute = 'bookingstatus' operator= 'eq'  uitype = 'bookingstatus' value = '" + bookingStatusId + "' />"
               + "</filter >"
               + "</entity >"
                + "</fetch >";

            // tracingService.Trace("XML:" + fetchXml);

            var fetchExpression = new FetchExpression(fetchXml);
            EntityCollection fetchResult = organizationService.RetrieveMultiple(fetchExpression);

            tracingService.Trace("Existing In Progress Booking:" + fetchResult.Entities.Count);

            return fetchResult.Entities.Count;
        }

        private EntityReference GetResourceServiceRole(IOrganizationService organizationService, ITracingService tracingService, Guid resourceId, string roleKey)
        {

            var fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>"
               + "<entity name='bookableresourcecategoryassn'>"
               + "<attribute name='resourcecategory' />"
               + "<filter type ='and' >"
               + "<condition attribute = 'resource' operator= 'eq'  uitype = 'bookableresource' value = '" + resourceId + "' />"
               + "<condition attribute = 'resourcecategoryname' operator= 'like'  value = '%" + roleKey + "%' />"
               + "</filter >"
               + "</entity >"
                + "</fetch >";

            // tracingService.Trace("XML:" + fetchXml);

            var fetchExpression = new FetchExpression(fetchXml);
            EntityCollection fetchResult = organizationService.RetrieveMultiple(fetchExpression);

            if (fetchResult.Entities.Count > 0)
            {
                var resourceCategoryAssnEntity = fetchResult.Entities[0];
                return GetLookupAttributeValue(resourceCategoryAssnEntity, "resourcecategory");
            }

            return null;
        }



        //          zm     + "<attribute name='ap360_finishcomplete' />" // Old sytesm field name is 'ap360_calculatedbookingpercentcomplete'

        private static DataCollection<Entity> GetServiceTaskBookings(IOrganizationService organizationService, ITracingService tracingService, Guid serviceTaskId)
        {
            var fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='bookableresourcebooking'>
                                <attribute name='resource' />
                                <attribute name='bookingstatus' />
                                <attribute name='bookableresourcebookingid' />
                                <attribute name='ap360_timespentonbooking' />
                                <attribute name='ap360_finishcomplete' />
                                <attribute name='ap360_billingfactor' />
                                <attribute name='ap360_workorderestimatedamount' />
                                <attribute name='ap360_serviceroles' />
                                <attribute name='ap360_estimatedtime' />
                                <order attribute='resource' descending='true' />
                                <filter type='and'>
                                  <condition attribute='ap360_workorderservicetask' operator='eq' uitype='msdyn_workorderservicetask' value='" + serviceTaskId + @"' />
                                  <condition attribute='bookingstatus' operator='eq' uiname='In Progress' uitype='bookingstatus' value='{53F39908-D08A-4D9C-90E8-907FD7BEC07D}' />
                                </filter>
                              </entity>
                            </fetch>";
            //53F39908 - D08A - 4D9C - 90E8 - 907FD7BEC07D  in progress
            // tracingService.Trace("XML:" + fetchXml);

            var fetchExpression = new FetchExpression(fetchXml);
            EntityCollection fetchResult = organizationService.RetrieveMultiple(fetchExpression);
            return fetchResult.Entities;
        }
        public static DataCollection<Entity> GetWorkOrdersINPROGRESSBookings(IOrganizationService organizationService, ITracingService tracingService, Guid workorderId)
        {
            var fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>"
               + "<entity name='bookableresourcebooking'>"
                + "<attribute name='ap360_timespentonbooking' />"
               + "<attribute name='ap360_billingfactor' />"
               + "<attribute name='ap360_serviceroles' />"
               + "<attribute name='bookingstatus' />"
               + "<attribute name='resource' />"


               + "<attribute name='ap360_finishcomplete' />" // Old sytesm field name is 'ap360_calculatedbookingpercentcomplete'
               + "<attribute name='ap360_estimatedtime' />"
               + "<attribute name='ap360_resourcehourlyrate' />"
               + "<attribute name='ap360_servicerolehourlyrate' />"

               + "<attribute name='ap360_workorderestimatedamount' />"
               + "<filter type ='and' >"
               + "<condition attribute = 'msdyn_workorder' operator= 'eq'  uitype = 'msdyn_workorder' value = '" + workorderId + "' />"
                              + "<condition attribute = 'bookingstatus' operator= 'eq' value = '{53F39908-D08A-4D9C-90E8-907FD7BEC07D}' />"

               + "</filter >"
               + "</entity >"
                + "</fetch >";

            // tracingService.Trace("XML:" + fetchXml);

            var fetchExpression = new FetchExpression(fetchXml);
            EntityCollection fetchResult = organizationService.RetrieveMultiple(fetchExpression);
            return fetchResult.Entities;
        }


        public static DataCollection<Entity> GetWorkOrderBookings(IOrganizationService organizationService, ITracingService tracingService, Guid workorderId)
        {
            var fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>"
               + "<entity name='bookableresourcebooking'>"
                + "<attribute name='ap360_timespentonbooking' />"
               + "<attribute name='ap360_billingfactor' />"
               + "<attribute name='ap360_serviceroles' />"
               + "<attribute name='bookingstatus' />"
               + "<attribute name='resource' />"


               + "<attribute name='ap360_finishcomplete' />" // Old sytesm field name is 'ap360_calculatedbookingpercentcomplete'
               + "<attribute name='ap360_estimatedtime' />"
               + "<attribute name='ap360_workorderestimatedamount' />"
               + "<filter type ='and' >"
               + "<condition attribute = 'msdyn_workorder' operator= 'eq'  uitype = 'msdyn_workorder' value = '" + workorderId + "' />"
               + "</filter >"
               + "</entity >"
                + "</fetch >";

            // tracingService.Trace("XML:" + fetchXml);

            var fetchExpression = new FetchExpression(fetchXml);
            EntityCollection fetchResult = organizationService.RetrieveMultiple(fetchExpression);
            return fetchResult.Entities;
        }


        private EntityReference GetWorkOrderFromBookingReference(IOrganizationService organizationService, ITracingService tracingService, Entity resourceBookingEntity)
        {
            var workorderEntityRef = GetLookupAttributeValue(resourceBookingEntity, "msdyn_workorder");
            if (workorderEntityRef == null)
            {
                var workorderServiceTaskEntityRef = GetLookupAttributeValue(resourceBookingEntity, "ap360_workorderservicetask");
                if (workorderServiceTaskEntityRef == null)
                {
                    var resourceRequirmentEntityRef = GetLookupAttributeValue(resourceBookingEntity, "msdyn_resourcerequirement");
                    if (resourceRequirmentEntityRef == null) return null;

                    tracingService.Trace("Fetching Resource Requirment entity");
                    var resourceRequirmentEntity = organizationService.Retrieve(resourceRequirmentEntityRef.LogicalName, resourceRequirmentEntityRef.Id, new ColumnSet("msdyn_workorder", "new_msdyn_workorderservicetask"));
                    if (resourceRequirmentEntity == null) return null;

                    tracingService.Trace("Getting Workorder no");
                    workorderEntityRef = GetLookupAttributeValue(resourceRequirmentEntity, "msdyn_workorder");
                    if (workorderEntityRef != null) return workorderEntityRef;

                    tracingService.Trace("Getting Service task id");
                    var serviceTaskEntityRef = GetLookupAttributeValue(resourceRequirmentEntity, "new_msdyn_workorderservicetask");
                    if (serviceTaskEntityRef == null) return serviceTaskEntityRef;

                    resourceBookingEntity["ap360_workorderservicetask"] = serviceTaskEntityRef;

                    tracingService.Trace("Getting workorder number from service task entity");
                    var workOrderServiceTaskEntity = organizationService.Retrieve(serviceTaskEntityRef.LogicalName, serviceTaskEntityRef.Id, new ColumnSet("msdyn_workorder"));
                    workorderEntityRef = GetLookupAttributeValue(workOrderServiceTaskEntity, "msdyn_workorder");
                }
                else
                {
                    var serviceTaskEntity = organizationService.Retrieve(workorderServiceTaskEntityRef.LogicalName, workorderServiceTaskEntityRef.Id, new ColumnSet("msdyn_workorder"));
                    workorderEntityRef = GetLookupAttributeValue(serviceTaskEntity, "msdyn_workorder");
                }
            }

            return workorderEntityRef;
        }

        private DataCollection<Entity> GetWorkOrderServiceTasks(IOrganizationService organizationService, ITracingService tracingService, Guid workorderId)
        {
            var fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>"
              + "<entity name='msdyn_workorderservicetask'>"
              + "<attribute name='msdyn_description' />"
              + "<attribute name='msdyn_name' />"
              + "<attribute name='msdyn_percentcomplete' />"

              + "<filter type ='and' >"
              + "<condition attribute = 'msdyn_workorder' operator= 'eq'  uitype = 'msdyn_workorder' value = '" + workorderId + "' />"
              + "<condition attribute='msdyn_percentcomplete' operator='lt' value='100' />"
              + "</filter >"
              + "</entity >"
               + "</fetch >";

            // tracingService.Trace("XML:" + fetchXml);

            var fetchExpression = new FetchExpression(fetchXml);
            EntityCollection fetchResult = organizationService.RetrieveMultiple(fetchExpression);
            return fetchResult.Entities;
        }




    }

}
