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
    public class BookableResourceBookings : EntityBase
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
                    if (executionContext.Depth > 4) return;
                    tracingService.Trace("Pre create bookableResourceBookingEntity Plugin starts, Depth:" + executionContext.Depth);
                    var bookingEntity = (Entity)executionContext.InputParameters["Target"];

                    EntityReference workorderEntityRef = null;
                    EntityReference serviceTaskEntityRef = null;
                    EntityReference opportunityEntityRef = null;

                    //    if (bookingEntity.Contains("msdyn_workorder")) return;
                    if (bookingEntity.Contains("ap360_workorderservicetask"))
                        serviceTaskEntityRef = GetLookupAttributeValue(bookingEntity, "ap360_workorderservicetask");
                    //else
                    //{
                    //    var resourceRequirmentEntityRef = GetLookupAttributeValue(bookingEntity, "msdyn_resourcerequirement");
                    //    if (resourceRequirmentEntityRef == null) return;

                    //    tracingService.Trace("Fetching Resource Requirment entity");
                    //    var resourceRequirmentEntity = organizationService.Retrieve(resourceRequirmentEntityRef.LogicalName, resourceRequirmentEntityRef.Id, new ColumnSet("msdyn_workorder", "new_msdyn_workorderservicetask"));
                    //    if (resourceRequirmentEntity == null) return;

                    //    tracingService.Trace("Getting Service task id");
                    //    serviceTaskEntityRef = GetLookupAttributeValue(resourceRequirmentEntity, "new_msdyn_workorderservicetask");
                    //    if (serviceTaskEntityRef != null)
                    //    {
                    //        tracingService.Trace("Setting Service task id :" + serviceTaskEntityRef.Id);
                    //        bookingEntity["ap360_workorderservicetask"] = serviceTaskEntityRef;
                    //    }
                    //}

                    if (serviceTaskEntityRef != null)
                    {
                        var serviceTaskEntity = organizationService.Retrieve(serviceTaskEntityRef.LogicalName, serviceTaskEntityRef.Id, new ColumnSet("msdyn_workorder", "ap360_opportunityid"));
                        workorderEntityRef = GetLookupAttributeValue(serviceTaskEntity, "msdyn_workorder");
                        opportunityEntityRef = GetLookupAttributeValue(serviceTaskEntity, "ap360_opportunityid");

                        if (workorderEntityRef != null)
                        {
                            tracingService.Trace("Setting Workorder :" + workorderEntityRef.Id);
                            bookingEntity["msdyn_workorder"] = workorderEntityRef;
                        }
                        if (opportunityEntityRef != null)
                        {
                            // throw new InvalidPluginExecutionException(" opporutnity not null");
                            var reterivedOppEntity = organizationService.Retrieve(opportunityEntityRef.LogicalName, opportunityEntityRef.Id, new ColumnSet("ap360_opportunityautonumber"));
                            if (reterivedOppEntity != null)
                            {
                                bookingEntity["ap360_opportunitynumber"] = Convert.ToDecimal(reterivedOppEntity.GetAttributeValue<string>("ap360_opportunityautonumber"));
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
                                //  WorkOrder.mapWorkOrderFieldsToBooking(organizationService, tracingService, bookingEntity, workorderEntityRef.Id);


                            }
                        }
                    }

                    break;

                case "update":
                    Entity bookableResourceBookingEntity = (Entity)executionContext.InputParameters["Target"];
                    if (bookableResourceBookingEntity.Contains("msdyn_actualarrivaltime") || bookableResourceBookingEntity.Contains("bookingstatus") || bookableResourceBookingEntity.Contains("ap360_calculateactualamount"))
                    {
                        if (executionContext.Depth > 4) return;

                        tracingService.Trace("Actual Arrival , Booking Status or Calculate Amount is updated");

                        Entity entity = organizationService.Retrieve(bookableResourceBookingEntity.LogicalName, bookableResourceBookingEntity.Id, new ColumnSet("bookingstatus", "msdyn_totalbillableduration", "msdyn_actualarrivaltime", "ap360_finishtime"));
                        EntityReference bookingStatusRef = null;


                        string TimeCalculationField;// techniacally Time spent on booking and Totalbillable should be same, some time system takes time in calculation for Totalbillable duration
                                                    // which cause unexpected behavior, to overcome this problem different fields are using in different types of Calculation(Manager, Technician)

                        //Manager Job , when approver updates booking Time stamped, cacluation based on Total Billable Duration when is rollup(OOB) of Booking Journal

                        if (bookableResourceBookingEntity.Contains("ap360_calculateactualamount"))
                        {
                            tracingService.Trace("Manager Job:  Calculate Amount is updated");

                            TimeCalculationField = "msdyn_totalbillableduration";
                            bool isCalculateAmount = false;
                            isCalculateAmount = bookableResourceBookingEntity.GetAttributeValue<bool>("ap360_calculateactualamount");
                            if (isCalculateAmount)
                            {

                                /////////////////////////////////7/22/2020////////////////////////////////////////////

                                List<BookingTimeStamp> lstBookingTimeStamps = new List<BookingTimeStamp>();
                                lstBookingTimeStamps = BookingTimeStamp.getInProgressAndFinsihedbookingTimeStamp(organizationService, tracingService, bookableResourceBookingEntity.Id);
                                BookableResourceBooking.updateBookingStartTimeAndEndTime(organizationService, tracingService, lstBookingTimeStamps, bookableResourceBookingEntity.Id);

                                Entity reterivedentity = organizationService.Retrieve(bookableResourceBookingEntity.LogicalName, bookableResourceBookingEntity.Id, new ColumnSet("bookingstatus", "msdyn_totalbillableduration", "msdyn_actualarrivaltime", "ap360_finishtime"));

                                /////////////////////////////////////////////////////////////////////////////


                                tracingService.Trace("Calculate Amount updated to true");
                                bookingStatusRef = reterivedentity.GetAttributeValue<EntityReference>("bookingstatus") != null ? reterivedentity.GetAttributeValue<EntityReference>("bookingstatus") : null;
                                if (bookingStatusRef != null)
                                {
                                    tracingService.Trace("Booking Staus ID: " + bookingStatusRef.Id.ToString());
                                    if ((bookingStatusRef.Id.ToString() == "c33410b9-1abe-4631-b4e9-6e4a1113af34"))//Closed
                                    {
                                        //tracingservice.trace("is closed is true, update of brb ap360_bookingactualamounts started: manager job");
                                        //updateworkordertotalactuallaboramount(organizationservice, tracingservice, bookableresourcebookingentity.id, timecalculationfield);
                                        //tracingservice.trace("end of update workordertotalactuallaboramount ");

                                        //////////////////////////////////////////////WOST Time Stamp update 7/7/2020////////////////////////////////////
                                        tracingService.Trace("Start of caculation funcitons");
                                        List<BookingServiceTask> lstBookingServiceTask = new List<BookingServiceTask>();
                                        lstBookingServiceTask = BookingServiceTask.GetBookingServiceTaskRelatedBRB(organizationService, bookableResourceBookingEntity.Id);
                                        WorkOrderServiceTask.caculateWOSTTimeStamps(organizationService, tracingService, lstBookingServiceTask, reterivedentity);
                                        tracingService.Trace("End of caculation functions");

                                        /////////////////////////////////////////////////////////////////////////////////////

                                        tracingService.Trace(" Update of BRB ap360_bookingactualamounts Finished");
                                        //throw new InvalidPluginExecutionException();
                                    }
                                }
                                else tracingService.Trace("Booking Staus is null");

                            }
                        }

                        //Normal Behavior , when technican mark BRB as Finsished, calculation based on Time spent on booking
                        if (entity != null)
                        {
                            tracingService.Trace("Normal Behrivor");
                            TimeCalculationField = "ap360_timespentonbooking";
                            if (bookableResourceBookingEntity.Contains("bookingstatus"))
                            {

                                bookingStatusRef = entity.GetAttributeValue<EntityReference>("bookingstatus") != null ? entity.GetAttributeValue<EntityReference>("bookingstatus") : null;
                                if (bookingStatusRef != null)
                                {
                                    if ((bookingStatusRef.Id.ToString() == "17e92808-7e59-ea11-a811-000d3a33f3c3"))//Finsihed
                                    {

                                        tracingService.Trace("Is Closed is True, Update of BRB ap360_bookingactualamounts Started: Normal Behavior");

                                        //List<BookingServiceTask> lstBookingServiceTask = new List<BookingServiceTask>();
                                        //lstBookingServiceTask = BookingServiceTask.GetBookingServiceTaskRelatedBRB(organizationService, bookableResourceBookingEntity.Id);
                                        //WorkOrderServiceTask.updateWOSTPercentComplete(organizationService, tracingService, lstBookingServiceTask);


                                        updateWorkOrderTotalActualLaborAmount(organizationService, tracingService, bookableResourceBookingEntity.Id, TimeCalculationField);
                                        tracingService.Trace(" Update of BRB ap360_bookingactualamounts Finished");

                                    }
                                }
                                else tracingService.Trace("Booking Staus is NULL");
                            }

                        }
                        // }
                    }
                    if (bookableResourceBookingEntity.Contains("ap360_dividetime") || bookableResourceBookingEntity.Contains("ap360_generatewosttimestamps"))
                    {
                        List<BookingServiceTask> lstBookingServiceTask = new List<BookingServiceTask>();
                        lstBookingServiceTask = BookingServiceTask.GetBookingServiceTaskRelatedBRB(organizationService, bookableResourceBookingEntity.Id);
                        Entity BRBEntity = organizationService.Retrieve("bookableresourcebooking", bookableResourceBookingEntity.Id, new ColumnSet(true));
                        if (BRBEntity == null) return;

                        bool dividetime = bookableResourceBookingEntity.GetAttributeValue<bool>("ap360_dividetime");
                        if (dividetime)
                        {
                            tracingService.Trace("Creation of new BRB Started");
                            CreateNewBRBonODone(organizationService, tracingService, bookableResourceBookingEntity.Id, lstBookingServiceTask, BRBEntity);
                            tracingService.Trace("Creation of new BRB Ended");

                        }

                        bool generatewosttimestamps = bookableResourceBookingEntity.GetAttributeValue<bool>("ap360_generatewosttimestamps");
                        if (generatewosttimestamps)
                        {
                            tracingService.Trace("Creation caculateWOSTTimeStamps started");
                            WorkOrderServiceTask.caculateWOSTTimeStamps(organizationService, tracingService, lstBookingServiceTask, BRBEntity);
                            tracingService.Trace("Creation caculateWOSTTimeStamps Ended");


                        }
                    }
                    if (executionContext.Depth > 2) return;
                    tracingService.Trace("Post Update bookableResourceBookingEntity Plugin starts, Depth:" + executionContext.Depth);

                    if (bookableResourceBookingEntity.Contains("bookingstatus"))
                    {
                        EntityReference bookingStatusReference = bookableResourceBookingEntity.GetAttributeValue<EntityReference>("bookingstatus");
                        Guid FinishedBookingStatusGuid = new Guid("17e92808-7e59-ea11-a811-000d3a33f3c3");// Finished
                        Guid closedBookingStatusGuid = new Guid("c33410b9-1abe-4631-b4e9-6e4a1113af34"); //closed
                        if (bookingStatusReference.Id == FinishedBookingStatusGuid || bookingStatusReference.Id == closedBookingStatusGuid)// 
                        {
                            //throw new InvalidPluginExecutionException("working");
                            BookableResourceBookingRetriveMultiple(executionContext, organizationService, tracingService);
                        }
                    }

                    //if (bookableResourceBookingEntity.Contains("ap360_serviceroles"))
                    //    CalculateRolePrice(organizationService,tracingService, bookableResourceBookingEntity);

                    if (bookableResourceBookingEntity.Contains("bookingstatus"))
                    {

                        tracingService.Trace("In Booking Status:");
                        var bookingStatusEntityRef = GetLookupAttributeValue(bookableResourceBookingEntity, "bookingstatus");

                        if (bookingStatusEntityRef == null) return;
                        tracingService.Trace("BookingStatusId:" + bookingStatusEntityRef.Id);

                        if (bookingStatusEntityRef.Id == new Guid("C33410B9-1ABE-4631-B4E9-6E4A1113AF34"))//Closed
                        {
                            tracingService.Trace("Close booking");
                            var resourceBookingEntity = organizationService.Retrieve(bookableResourceBookingEntity.LogicalName, bookableResourceBookingEntity.Id, new ColumnSet("ap360_serviceroles", "ap360_bookingclassification"));

                            if (resourceBookingEntity == null) return;

                            //if (!bookableResourceBookingEntity.Contains("ap360_serviceroles"))
                            //{
                            //    if (!resourceBookingEntity.Contains("ap360_serviceroles"))
                            //        throw new InvalidPluginExecutionException("Please provide service role before closing booking.");
                            //}

                            if (!bookableResourceBookingEntity.Contains("ap360_bookingclassification"))
                            {
                                if (!resourceBookingEntity.Contains("ap360_bookingclassification"))
                                    throw new InvalidPluginExecutionException("Please provide booking classification before closing booking.");
                            }
                        }

                    }

                    break;
            }
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

            LastFinsihedBookingTimeStamp = lstBookingTimeStamps.LastOrDefault(e => e.TimeStampSystemStatus == 690970005);

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
            var resourceBookingEntity = organizationService.Retrieve(context.PrimaryEntityName, context.PrimaryEntityId, new ColumnSet("resource", "msdyn_workorder"));

            var workorderEntityRef = GetLookupAttributeValue(resourceBookingEntity, "msdyn_workorder");
            if (workorderEntityRef == null) return;

            var serviceTasks = GetWorkOrderServiceTasks(organizationService, tracingService, workorderEntityRef.Id);

            foreach (Entity serviceTask in serviceTasks)
            {
                var name = GetStringAttributeValue(serviceTask, "msdyn_name");
                var description = GetStringAttributeValue(serviceTask, "msdyn_description");
                var woserviceTaskPercentComplete = GetFloatAttributeValue(serviceTask, "msdyn_percentcomplete");

                var bookingServiceTask = new Entity("ap360_bookingservicetask");
                bookingServiceTask["ap360_bookableresourcebooking"] = new EntityReference(context.PrimaryEntityName, context.PrimaryEntityId);
                bookingServiceTask["ap360_workorderservicetask"] = new EntityReference(serviceTask.LogicalName, serviceTask.Id);
                bookingServiceTask["ap360_name"] = description;
                bookingServiceTask["ap360_completed"] = Convert.ToDouble(woserviceTaskPercentComplete);

                organizationService.Create(bookingServiceTask);
                tracingService.Trace("Service Task Created: " + name);
            }
        }

        public static Guid CreateBookAbleResourceBooking(IOrganizationService service, Entity entity)
        {
            Entity newlyBRB = new Entity("bookableresourcebooking");

            newlyBRB["starttime"] = entity.GetAttributeValue<DateTime>("starttime");
            newlyBRB["endtime"] = entity.GetAttributeValue<DateTime>("endtime");
            EntityReference serviceRole = entity.GetAttributeValue<EntityReference>("ap360_serviceroles") != null ? entity.GetAttributeValue<EntityReference>("ap360_serviceroles") : null;
            if (serviceRole != null)
                newlyBRB["ap360_serviceroles"] = new EntityReference(serviceRole.LogicalName, serviceRole.Id);
            newlyBRB["duration"] = 200;

            // newlyBRB["name"] = 200;
            EntityReference wost = entity.GetAttributeValue<EntityReference>("ap360_workorderservicetask") != null ? entity.GetAttributeValue<EntityReference>("ap360_workorderservicetask") : null;
            if (wost != null)
                newlyBRB["ap360_workorderservicetask"] = new EntityReference(wost.LogicalName, wost.Id);
            EntityReference resource = entity.GetAttributeValue<EntityReference>("resource") != null ? entity.GetAttributeValue<EntityReference>("resource") : null;
            if (resource != null)
                newlyBRB["resource"] = new EntityReference(resource.LogicalName, resource.Id);
            EntityReference bookingstatus = entity.GetAttributeValue<EntityReference>("bookingstatus") != null ? entity.GetAttributeValue<EntityReference>("bookingstatus") : null;
            if (bookingstatus != null)
                newlyBRB["bookingstatus"] = new EntityReference(bookingstatus.LogicalName, new Guid("f16d80d1-fd07-4237-8b69-187a11eb75f9"));// Scheduled
            EntityReference resourcerequirement = entity.GetAttributeValue<EntityReference>("msdyn_resourcerequirement") != null ? entity.GetAttributeValue<EntityReference>("msdyn_resourcerequirement") : null;
            if (resourcerequirement != null)
                newlyBRB["msdyn_resourcerequirement"] = new EntityReference(resourcerequirement.LogicalName, resourcerequirement.Id);
            newlyBRB["bookingtype"] = new OptionSetValue(1);//Solid

            // newlyBRB["ap360_isfinished"] = "";
            EntityReference workorder = entity.GetAttributeValue<EntityReference>("msdyn_workorder") != null ? entity.GetAttributeValue<EntityReference>("msdyn_workorder") : null;
            if (workorder != null)
                newlyBRB["msdyn_workorder"] = new EntityReference(workorder.LogicalName, workorder.Id);
            //EntityReference header = entity.GetAttributeValue<EntityReference>("header") != null ? entity.GetAttributeValue<EntityReference>("header") : null;
            //if (header != null)
            //    newlyBRB["header"] = new EntityReference(header.LogicalName, header.Id);
            EntityReference bookingsetupmetadata = entity.GetAttributeValue<EntityReference>("msdyn_bookingsetupmetadataid") != null ? entity.GetAttributeValue<EntityReference>("msdyn_bookingsetupmetadataid") : null;
            if (bookingsetupmetadata != null)
                newlyBRB["msdyn_bookingsetupmetadataid"] = new EntityReference(bookingsetupmetadata.LogicalName, bookingsetupmetadata.Id);
            Guid guid = service.Create(newlyBRB);

            return guid;

        }

        public static void UpdateOldBRBOnCreationOFNewBRB(IOrganizationService service, Guid oldBRBGuid)
        {
            Entity oldBRB = new Entity("bookableresourcebooking");
            oldBRB.Id = oldBRBGuid;
            oldBRB["bookingstatus"] = new EntityReference("bookingstatus", new Guid("17e92808-7e59-ea11-a811-000d3a33f3c3"));// Finished
            service.Update(oldBRB);

        }


        internal decimal updateWorkOrderTotalActualLaborAmount(IOrganizationService service, ITracingService tracingService, Guid brbGuid, string TimeCalculationField)
        {
            tracingService.Trace("Inside Update WorkOrderActualAmount Labor Amount Function");
            Entity entity = service.Retrieve("bookableresourcebooking", brbGuid, new ColumnSet(true));
            if (entity == null) return 0;
            BookableResourceBooking bookableResourceBooking = new BookableResourceBooking();
            bookableResourceBooking.IsFinished = entity.GetAttributeValue<bool>("ap360_isfinished");
            bookableResourceBooking.BookingStatus = entity.GetAttributeValue<EntityReference>("bookingstatus") != null ? entity.GetAttributeValue<EntityReference>("bookingstatus") : null;

            //if (bookableResourceBooking.IsFinished && bookableResourceBooking.BookingStatus.Id.ToString() == "17e92808-7e59-ea11-a811-000d3a33f3c3")//Finished
            //{
            bookableResourceBooking.BillingFactor = entity.GetAttributeValue<decimal>("ap360_billingfactor");

            //bookableResourceBooking.TimeSpentOnBooking = entity.GetAttributeValue<int>("ap360_timespentonbooking");
            //bookableResourceBooking.TotalBillableDuration = entity.GetAttributeValue<int>("msdyn_totalbillableduration");

            int timespentORBillableDuraiton = entity.GetAttributeValue<int>(TimeCalculationField);
            decimal billingPrice = 0;
            bookableResourceBooking.Resource = entity.GetAttributeValue<EntityReference>("resource") != null ? entity.GetAttributeValue<EntityReference>("resource") : null;
            bookableResourceBooking.WordOrder = entity.GetAttributeValue<EntityReference>("msdyn_workorder") != null ? entity.GetAttributeValue<EntityReference>("msdyn_workorder") : null;
            if (bookableResourceBooking.WordOrder != null)
            {
                Entity workOrder = service.Retrieve(bookableResourceBooking.WordOrder.LogicalName, bookableResourceBooking.WordOrder.Id, new ColumnSet("ap360_servicerole"));
                if (workOrder != null)
                {

                    string serviceRoleName = workOrder.GetAttributeValue<EntityReference>("ap360_servicerole") != null ? workOrder.GetAttributeValue<EntityReference>("ap360_servicerole").Name : null;
                    if (serviceRoleName != null)
                    {
                        // throw new InvalidPluginExecutionException("Service Role not null");
                        tracingService.Trace("Service Role Name " + serviceRoleName);

                        decimal price = Methods.GetResourcePriceBasedOnBRC(service, bookableResourceBooking.Resource.Id, serviceRoleName);
                        tracingService.Trace("Service Role Price " + price.ToString());
                        tracingService.Trace("Time Spent on Booking " + TimeSpentOnBooking.ToString());
                        tracingService.Trace("Total Billabe Duration " + TotalBillableDuration.ToString());

                        //var billingPrice = ((decimal)bookableResourceBooking.TimeSpentOnBooking / 60) * (bookableResourceBooking.BillingFactor * price);
                        //var billingPrice = ((decimal)bookableResourceBooking.TotalBillableDuration / 60) * (bookableResourceBooking.BillingFactor * price);
                        billingPrice = ((decimal)timespentORBillableDuraiton / 60) * (bookableResourceBooking.BillingFactor * price);

                        //  throw new InvalidPluginExecutionException(billingPrice.ToString());
                        tracingService.Trace("Billing Price " + billingPrice.ToString());
                        Entity updateBRB = new Entity("bookableresourcebooking");
                        updateBRB.Id = entity.Id;
                        updateBRB["ap360_bookingactualamounts"] = new Money(billingPrice);
                        updateBRB["ap360_workorderactualamount"] = new Money(billingPrice);
                        updateBRB["ap360_calculateactualamount"] = false;
                        service.Update(updateBRB);

                        tracingService.Trace("BRB updated");
                    }
                    else
                    {
                        throw new InvalidPluginExecutionException("Service Role is not Selected in WorkOrder");

                    }
                }
            }


            return billingPrice;
            // }

        }

        #region Private Functions
        private void CreateNewBRBonODone(IOrganizationService service, ITracingService tracingService, Guid OldbrbGuid, List<BookingServiceTask> lstBookingServiceTask, Entity BRBEntity)
        {
            List<WorkOrderServiceTask> lstworkOrderServiceTask = new List<WorkOrderServiceTask>();
            // List<BookingServiceTask> lstBookingServiceTask = new List<BookingServiceTask>();
            tracingService.Trace("Insdie  CreateNewBRBonODone");
            tracingService.Trace("Before reterving OLD BRB " + OldbrbGuid.ToString());
            // Entity BRBEntity = service.Retrieve("bookableresourcebooking", OldbrbGuid, new ColumnSet(true));
            // if (BRBEntity == null) return;
            tracingService.Trace("OlD BRB retervied Successfully");

            BookableResourceBooking bookableResourceBooking = new BookableResourceBooking();
            bookableResourceBooking.IsFinished = BRBEntity.GetAttributeValue<bool>("ap360_isfinished");
            bookableResourceBooking.NextPreferredAction = BRBEntity.GetAttributeValue<OptionSetValue>("ap360_nextpreferredaction").Value;

            if (bookableResourceBooking.IsFinished == true && bookableResourceBooking.NextPreferredAction == 126300001) // 126300001 INC - RETURN
            {
                tracingService.Trace("Is finished is True and Next Preferred  Action is INC-RETURN");

                BookableResourceBooking.UpdateOldBRBOnCreationOFNewBRB(service, OldbrbGuid);
                tracingService.Trace("Old BRB Updated");
                Guid newlyCreatedBRBGuid = BookableResourceBooking.CreateBookAbleResourceBooking(service, BRBEntity);
                tracingService.Trace("Newly Created BRB " + newlyCreatedBRBGuid.ToString());

                lstworkOrderServiceTask = WorkOrderServiceTask.GetBRBRelatedWorkOrderServiceTask(service, OldbrbGuid);
                if (lstworkOrderServiceTask.Count > 0)
                {
                    tracingService.Trace("Work Order Service Task Count " + lstworkOrderServiceTask.Count.ToString());

                    WorkOrderServiceTask.AssociateWOSTwithNewlyCeatedBRB(service, lstworkOrderServiceTask, newlyCreatedBRBGuid);
                }
                //lstBookingServiceTask = BookingServiceTask.GetBookingServiceTaskRelatedBRB(service, OldbrbGuid);
                if (lstBookingServiceTask.Count > 0)
                {
                    tracingService.Trace("Booking Service Task Count " + lstBookingServiceTask.Count.ToString());

                    BookingServiceTask.UpdateBST(service, tracingService, lstBookingServiceTask, newlyCreatedBRBGuid);
                }
                tracingService.Trace("Before updateWorkOrderFields");
                WorkOrder.updateWorkOrderFields(service, BRBEntity);
                tracingService.Trace("After updateWorkOrderFields");


                //tracingService.Trace("Before caculateWOSTTimeStamps");
                //WorkOrderServiceTask.caculateWOSTTimeStamps(service, tracingService, lstBookingServiceTask, BRBEntity);
                //tracingService.Trace("Before caculateWOSTTimeStamps");


                tracingService.Trace("Calculation of Time Stamp completed");


            }
        }
        private void BookableResourceBookingRetriveMultiple(IPluginExecutionContext executionContext, IOrganizationService organizationService, ITracingService tracingService)
        {
            if (executionContext.OutputParameters.Contains("BusinessEntity"))
            {
                //throw new InvalidPluginExecutionException("Reterive");

                Entity entity = (Entity)executionContext.OutputParameters["BusinessEntity"];
                CalculatePercentagesNew(organizationService, tracingService, entity);
            }
            else if (executionContext.OutputParameters.Contains("BusinessEntityCollection"))
            {
                var businessEntityCollection = (EntityCollection)executionContext.OutputParameters["BusinessEntityCollection"];
                tracingService.Trace("Record Count:" + businessEntityCollection.Entities.Count);
                //   tracingService.Trace(businessEntityCollection.Entities[2].GetAttributeValue<EntityReference>("msdyn_workorder").Name);
                foreach (Entity entity in businessEntityCollection.Entities)
                {
                    CalculatePercentagesNew(organizationService, tracingService, entity);
                }
                //throw new InvalidPluginExecutionException("Reterive mulitple");
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
        private void CalculatePercentagesNew(IOrganizationService organizationService, ITracingService tracingService, Entity entity)
        {
            tracingService.Trace("CalculatePercentagesNew Inside fuction ");
            EntityReference serviceRoleEntityRef = null;
            int WOTotalRevisedEstimatedDuration = 0;
            decimal WOTotalRevisedEstimatedLaborAmount = 0;
            decimal Bwastotalrevisedestimatedamount = 0;
            decimal WOSTActualAmount = 0;
            decimal WOTotalActualWOSTLaboramount = 0;
            decimal WOTotalactuallaboramount = 0;
            Entity WOSTEntity = null;
            //var workorderEntityRef =entity.GetAttributeValue<EntityReference>("msdyn_workorder");
            var workorderEntityRef = GetLookupAttributeValue(entity, "msdyn_workorder");
            if (workorderEntityRef == null)
            {
                tracingService.Trace("Work Order Ref is null");
                return;
            }
            DataCollection<Entity> bookings = null;

            var ServiceTaskEntityRef = GetLookupAttributeValue(entity, "ap360_workorderservicetask");
            if (ServiceTaskEntityRef != null)
            {
                tracingService.Trace("Work Order Service Task exists");
                bookings = GetServiceTaskBookings(organizationService, tracingService, ServiceTaskEntityRef.Id);

                //////////////////////////////////////7/18/2020 CHANGE FOR wOST $ TIME REMAINING CALCULATION
                WOSTEntity = organizationService.Retrieve(ServiceTaskEntityRef.LogicalName, ServiceTaskEntityRef.Id, new ColumnSet("ap360_actualamount"));
                if (WOSTEntity != null)
                {
                    WOSTActualAmount = WOSTEntity.GetAttributeValue<Money>("ap360_actualamount") != null ? WOSTEntity.GetAttributeValue<Money>("ap360_actualamount").Value : 0;
                }
                //////////////////////////////////////7/18/2020 CHANGE FOR wOST $ TIME REMAINING CALCULATION

            }
            else
            {
                tracingService.Trace("WOrk Orderexists");
                bookings = GetWorkOrderBookings(organizationService, tracingService, workorderEntityRef.Id);

            }
            tracingService.Trace("Work Order Entity ID :" + workorderEntityRef.Id.ToString());
            tracingService.Trace("Booking Count " + bookings.Count.ToString());


            Entity workOrder = organizationService.Retrieve(workorderEntityRef.LogicalName, workorderEntityRef.Id, new ColumnSet("ap360_servicerole", "ap360_totalrevisedestimatedduration", "ap360_totalrevisedestimatedlaboramount", "ap360_bwastotalrevisedestimatedamount", "ap360_bwastotalrevisedestimatedamount", "ap360_totalactualwostlaboramount", "ap360_totalactuallaboramount"));

            if (workOrder != null)
            {
                serviceRoleEntityRef = workOrder.GetAttributeValue<EntityReference>("ap360_servicerole") != null ? workOrder.GetAttributeValue<EntityReference>("ap360_servicerole") : null;

                WOTotalRevisedEstimatedDuration = workOrder.GetAttributeValue<int>("ap360_totalrevisedestimatedduration");
                WOTotalRevisedEstimatedLaborAmount = workOrder.GetAttributeValue<Money>("ap360_totalrevisedestimatedlaboramount") != null ? workOrder.GetAttributeValue<Money>("ap360_totalrevisedestimatedlaboramount").Value : 0;
                Bwastotalrevisedestimatedamount = workOrder.GetAttributeValue<Money>("ap360_bwastotalrevisedestimatedamount") != null ? workOrder.GetAttributeValue<Money>("ap360_bwastotalrevisedestimatedamount").Value : 0;
                WOTotalActualWOSTLaboramount = workOrder.GetAttributeValue<Money>("ap360_totalactualwostlaboramount") != null ? workOrder.GetAttributeValue<Money>("ap360_totalactualwostlaboramount").Value : 0;
                WOTotalactuallaboramount = workOrder.GetAttributeValue<Money>("ap360_totalactuallaboramount") != null ? workOrder.GetAttributeValue<Money>("ap360_totalactuallaboramount").Value : 0;

                tracingService.Trace("WO Estimated Duration " + WOTotalRevisedEstimatedDuration.ToString());
                tracingService.Trace("WO Estimated Labor Amount " + WOTotalRevisedEstimatedLaborAmount.ToString());
                tracingService.Trace("BWAS Imported: Revised Estimated Amount " + Bwastotalrevisedestimatedamount.ToString());
                tracingService.Trace("WO TotalActual WOST Laboramount " + WOTotalActualWOSTLaboramount.ToString());

            }


            decimal totalBillingPrice = 0.0m;
            int totalTimeSpentOnWorkOrder = 0;
            int estimatedTime = 0;
            decimal currentBookingRolePrice = 0;
            decimal reportedPercentComplete = 0.0m;
            decimal estimatedAmount = 0.0m;
            decimal price = 0;
            var calculatedEstimatedTime = 0;
            decimal standardPrice = 0.0m;


            bool isScheduledorInprogress = false;
            foreach (Entity booking in bookings)
            {


                tracingService.Trace("********************************");
                tracingService.Trace("Inside Foreach");
                if (serviceRoleEntityRef == null)
                {
                    serviceRoleEntityRef = GetLookupAttributeValue(booking, "ap360_serviceroles");
                    if (serviceRoleEntityRef == null) continue;
                }
                tracingService.Trace("Service Role " + serviceRoleEntityRef.Name);

                EntityReference resource = GetLookupAttributeValue(booking, "resource");
                if (resource == null) continue;
                tracingService.Trace("After Fetching Resource : " + resource.Name);
                var billingFactor = GetDecimalAttributeValue(booking, "ap360_billingfactor");
                var timespentonbooking = GetIntAttributeValue(booking, "ap360_timespentonbooking");
                Guid blackWolfPriceListGuid = new Guid("4fe16dd5-8e55-ea11-a811-000d3a33f3c3");

                //////////////////////////////
                var bookingPercentComplete = 0.0m;
                var currentBookingbillingPrice = 0.0m;
                if (ServiceTaskEntityRef != null)
                {
                    calculatedEstimatedTime = GetIntAttributeValue(entity, "ap360_estimatedtime");
                    bookingPercentComplete = GetDecimalAttributeValue(booking, "ap360_finishcomplete");
                    standardPrice = Methods.getBookAbleResourceCategoryStandardPrice(organizationService, serviceRoleEntityRef.Name);
                    price = Methods.GetResourcePriceBasedOnBRC(organizationService, resource.Id, serviceRoleEntityRef.Name);

                    currentBookingbillingPrice = ((decimal)timespentonbooking / 60) * (billingFactor * price);

                    //////////////////////////////////////7/18/2020 CHANGE FOR wOST $ TIME REMAINING CALCULATION
                    // totalBillingPrice = currentBookingbillingPrice;
                    //////////////////////////////////////7/18/2020 CHANGE FOR wOST $ TIME REMAINING CALCULATION



                }
                else
                {
                    calculatedEstimatedTime = WOTotalRevisedEstimatedDuration;
                    bookingPercentComplete = GetDecimalAttributeValue(booking, "ap360_calculatedbookingpercentcomplete");
                    price = Methods.GetResourcePriceBasedOnBRC(organizationService, resource.Id, serviceRoleEntityRef.Name);
                    currentBookingbillingPrice = ((decimal)timespentonbooking / 60) * (billingFactor * price);

                    //////////////////////////////////////7/18/2020 CHANGE FOR wOST $ TIME REMAINING CALCULATION
                    totalBillingPrice = totalBillingPrice + currentBookingbillingPrice;
                    //////////////////////////////////////7/18/2020 CHANGE FOR wOST $ TIME REMAINING CALCULATION

                }


                ////////////////////////////////
                //  totalBillingPrice = totalBillingPrice + currentBookingbillingPrice;
                ///////////////////////////////


                totalTimeSpentOnWorkOrder = totalTimeSpentOnWorkOrder + timespentonbooking;
                if (booking.Id == entity.Id)
                {
                    var bookingstatus = GetLookupAttributeValue(booking, "bookingstatus");
                    //*****************12/14/2020
                    if (bookingstatus != null)
                    {
                        if (bookingstatus.Id.ToString().ToLower() == "f16d80d1-fd07-4237-8b69-187a11eb75f9" || bookingstatus.Id.ToString().ToLower() == "53f39908-d08a-4d9c-90e8-907fd7bec07d")//scheduled || In Progress
                        {
                            isScheduledorInprogress = true;
                        }
                    }

                    totalBillingPrice = currentBookingbillingPrice;// this code moved here beacuse we need current booking price for calculation

                    tracingService.Trace("Booking Id == Entity ID");
                    currentBookingRolePrice = price;
                    //tracingService.Trace("currentBookingRolePrice :" + price);
                    reportedPercentComplete = bookingPercentComplete;
                    estimatedTime = calculatedEstimatedTime;
                    if (ServiceTaskEntityRef != null)
                    {
                        tracingService.Trace("Standard Role  price :" + standardPrice);
                        estimatedAmount = ((decimal)estimatedTime / 60) * standardPrice;

                        //////////////////////////////////////7/18/2020 CHANGE FOR wOST $ TIME REMAINING CALCULATION
                        estimatedAmount = estimatedAmount - WOSTActualAmount;
                        tracingService.Trace("WOSt Actual Amount  " + WOSTActualAmount.ToString());
                        //////////////////////////////////////7/18/2020 CHANGE FOR wOST $ TIME REMAINING CALCULATION

                        tracingService.Trace("WO Service Task Booking Time " + estimatedTime.ToString());
                        tracingService.Trace("estimatedAmount  = ((decimal)estimatedTime / 60) * currentBookingRolePrice");
                        tracingService.Trace("            " + estimatedAmount.ToString() + " = ( " + estimatedTime.ToString() + " / " + " 60) * " + standardPrice.ToString());
                        tracingService.Trace("Resource Hourly Rate :" + price.ToString());

                    }
                    else if (Bwastotalrevisedestimatedamount != 0)
                    {
                        tracingService.Trace("Resource Role  price :" + price);
                        estimatedAmount = Bwastotalrevisedestimatedamount;
                        tracingService.Trace("Imported Booking ");
                        tracingService.Trace("estimatedAmount = Bwastotalrevisedestimatedamount");
                        tracingService.Trace(estimatedAmount.ToString() + " = " + Bwastotalrevisedestimatedamount.ToString());

                    }
                    else
                    {
                        tracingService.Trace("Resource Role  price :" + price);
                        estimatedAmount = WOTotalRevisedEstimatedLaborAmount;
                        tracingService.Trace("New System Booking ");
                        tracingService.Trace("estimatedAmount = WOTotalRevisedEstimatedAmount");
                        tracingService.Trace(estimatedAmount.ToString() + " = " + WOTotalRevisedEstimatedLaborAmount.ToString());
                    }
                }
                tracingService.Trace("bookingPercentComplete:" + bookingPercentComplete);
                tracingService.Trace("billingFactor:" + billingFactor);
                tracingService.Trace("timespentonbooking:" + timespentonbooking);
                tracingService.Trace("Currrent Booking Biling  price: (TimeSpent on Booking/60)*(billing Factor) * RolePrice = " + currentBookingbillingPrice);



            }

            if (!isScheduledorInprogress) return;
            tracingService.Trace("******************************");
            ///////////////////////////Update workOrder acutal amount//////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////


            tracingService.Trace("WOTotalRevisedEstimatedLaborAmount " + WOTotalRevisedEstimatedLaborAmount.ToString());
            tracingService.Trace("WOTotalActualWOSTLaboramount " + WOTotalActualWOSTLaboramount.ToString());
            tracingService.Trace("WOTotalactuallaboramount " + WOTotalactuallaboramount.ToString());

            decimal woRemainingEstimatedAmount = 0;
            if (WOTotalactuallaboramount == WOTotalActualWOSTLaboramount)// for opportunities created after 10 Aug, the two fields will be same other wise these field might not same
            {
                tracingService.Trace("Acutal and Actual WOST amounts are same");
                woRemainingEstimatedAmount = WOTotalRevisedEstimatedLaborAmount - WOTotalActualWOSTLaboramount;
            }
            else
            {
                tracingService.Trace("Acutal and Actual WOST amounts are not same");

                woRemainingEstimatedAmount = WOTotalRevisedEstimatedLaborAmount - WOTotalactuallaboramount;

            }
            tracingService.Trace("WO remaining Estimated Amount (WOTotalRevisedEstimatedLaborAmount -WOTotalActualWOSTLaboramount) =" + woRemainingEstimatedAmount.ToString());
            var woDollarTimeRemaining = woRemainingEstimatedAmount / currentBookingRolePrice;
            tracingService.Trace("woDollarTimeRemaining = woRemainingEstimatedAmount - currentBookingRolePrice " + woDollarTimeRemaining.ToString());
            var woDollarTimeRemainginInMinutes = woDollarTimeRemaining * 60;

            tracingService.Trace("woDollarTimeRemainginInMinutes  " + woDollarTimeRemainginInMinutes.ToString());

            // tracingService.Trace(WOTotalRevisedEstimatedLaborAmount.ToString() +"-" +WOTotalActualWOSTLaboramount.ToString()+"/"+currentBookingRolePrice.ToString() +"="  + woDollarTimeRemaining.ToString());
            entity["ap360_wotimeremaining"] = woDollarTimeRemainginInMinutes;
            //entity["ap360_workordertimeremaining"] = woDollarTimeRemainginInMinutes;


            //////////////////////////////////////////////////////////////////////////////////////////////////

            tracingService.Trace("Estimated Time:" + estimatedTime);
            tracingService.Trace("Total Time Spent:" + totalTimeSpentOnWorkOrder);
            tracingService.Trace("Total Billing Price:" + totalBillingPrice);
            //tracingService.Trace("Estimated Amount:" + estimatedAmount);
            tracingService.Trace("CurrentBookingRolePrice " + price);

            if (estimatedAmount == 0.0m)
            {
                tracingService.Trace("Estimated Amount  = 0 : plugin not proceed Further");
                return;


            }


            if (entity.Contains("ap360_estimatedamount"))
                entity["ap360_estimatedamount"] = new Money(estimatedAmount);
            else
                entity.Attributes.Add("ap360_estimatedamount", new Money(estimatedAmount));


            ///////////////////////////////////////////////////
            tracingService.Trace("Total Billing Price " + totalBillingPrice);

            if (entity.Contains("ap360_workorderactualamount"))
                entity["ap360_workorderactualamount"] = new Money(totalBillingPrice);
            else
                entity.Attributes.Add("ap360_workorderactualamount", new Money(totalBillingPrice));

            /////////////////////////////////////////

            if (entity.Contains("ap360_timespentonworkorder"))
                entity["ap360_timespentonworkorder"] = totalTimeSpentOnWorkOrder;
            else
                entity.Attributes.Add("ap360_timespentonworkorder", totalTimeSpentOnWorkOrder);

            var dollarPercentSpent = (totalBillingPrice / estimatedAmount) * 100;
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
            if (estimatedTime > 0)
                startingPercentage = ((decimal)totalTimeSpentOnWorkOrder / estimatedTime) * 100;


            if (entity.Contains("ap360_workorderstartingcomplete"))
                entity["ap360_workorderstartingcomplete"] = decimal.ToInt32(startingPercentage);
            else
                entity.Attributes.Add("ap360_workorderstartingcomplete", decimal.ToInt32(startingPercentage));



            if (currentBookingRolePrice > 0)
            {
                tracingService.Trace("Remaining minutes = (estimatedAmount - TotalBillingPrice)/RolePrice");

                var remainingMinutes = (estimatedAmount - totalBillingPrice) / currentBookingRolePrice;
                tracingService.Trace("Remaining minutes = " + estimatedAmount.ToString() + " - " + totalBillingPrice.ToString() + " / " + currentBookingRolePrice.ToString());
                tracingService.Trace("Remaining Minutes * 60 =" + decimal.ToInt32(remainingMinutes * 60).ToString());

                //ap360_wosttimeremaining
                //if (entity.Contains("ap360_wosttimeremaining"))//$ time remaining
                //{
                //    entity["ap360_wosttimeremaining"] = decimal.ToInt32(remainingMinutes * 60);
                //    tracingService.Trace("IF % time remainng " + decimal.ToInt32(remainingMinutes * 60).ToString());
                //}

                //else
                //{
                //    entity.Attributes.Add("ap360_wosttimeremaining", decimal.ToInt32(remainingMinutes * 60));
                //    tracingService.Trace("ELSE % time remainng " + decimal.ToInt32(remainingMinutes * 60).ToString());
                //}

                if (entity.Contains("ap360_actualtimeremaining"))//$ time remaining
                {
                    entity["ap360_actualtimeremaining"] = decimal.ToInt32(remainingMinutes * 60);
                    tracingService.Trace("IF % time remainng " + decimal.ToInt32(remainingMinutes * 60).ToString());
                }

                else
                {
                    entity.Attributes.Add("ap360_actualtimeremaining", decimal.ToInt32(remainingMinutes * 60));
                    tracingService.Trace("ELSE % time remainng " + decimal.ToInt32(remainingMinutes * 60).ToString());
                }

                //  organizationService.Update(entity);
            }
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

        private DataCollection<Entity> GetServiceTaskBookings(IOrganizationService organizationService, ITracingService tracingService, Guid serviceTaskId)
        {
            var fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>"
               + "<entity name='bookableresourcebooking'>"
                + "<attribute name='ap360_timespentonbooking' />"
               + "<attribute name='ap360_billingfactor' />"
               + "<attribute name='ap360_serviceroles' />"
               + "<attribute name='ap360_finishcomplete' />" // Old sytesm field name is 'ap360_calculatedbookingpercentcomplete'
               + "<attribute name='ap360_estimatedtime' />"
               + "<attribute name='ap360_workorderestimatedamount' />"
               + "<attribute name='resource' />"

               + "<filter type ='and' >"
               + "<condition attribute = 'ap360_workorderservicetask' operator= 'eq'  uitype = 'msdyn_workorderservicetask' value = '" + serviceTaskId + "' />"
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
              + "</filter >"
              + "</entity >"
               + "</fetch >";

            // tracingService.Trace("XML:" + fetchXml);

            var fetchExpression = new FetchExpression(fetchXml);
            EntityCollection fetchResult = organizationService.RetrieveMultiple(fetchExpression);
            return fetchResult.Entities;
        }

        #endregion


    }

}
