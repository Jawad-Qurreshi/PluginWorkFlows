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
    public class WorkOrderServiceTask
    {
        public Guid WOSTGuid { get; set; }
        public int WBSID { get; set; }
        public EntityReference WorkOrderRef { get; set; }
        public decimal RevisedEstimatedDuration { get; set; }
        public decimal OriginalEstimatedDuration { get; set; }
        public string Name { get; set; }
        public int Actualduration { get; set; }
        public int TotalRevisedEstimatedDuration { get; set; }
        public int TotalOriginalEstimatedDuration { get; set; }
        public int TotalActualduration { get; set; }
        public EntityReference Booking { get; set; }
        public int PredictedSpend { get; set; }
        public EntityReference Opportunity { get; set; }
        public decimal WOSTLaborAmount { get; set; }
        public int WOSTStatus { get; set; }
        public EntityReference WorkOrder { get; set; }
        public WorkOrderServiceTaskTimeStamp WOSTTimeStamps { get; set; }


        public static void CreateStandardWorkOrderServiceTasksOnCreationofOpportunity(IOrganizationService service, ITracingService tracingservice, Guid workOrderGuid, string taskName, Guid opportuntiyGuid, EntityReference vehicleRef, decimal opportuntiyNumber, string wostType)
        {
            tracingservice.Trace("Inside CreateStandardWOServiceTAsk");
            Entity entity = new Entity("msdyn_workorderservicetask");

            entity["msdyn_name"] = taskName;

            entity["msdyn_description"] = taskName;

            if (wostType == "admin")
                entity["msdyn_estimatedduration"] = 120;
            else if (wostType == "protocol")
                entity["msdyn_estimatedduration"] = 60;


            entity["ap360_isrevised"] = false;

            if (vehicleRef != null)
                entity["ap360_vehicleid"] = new EntityReference("ap360_vehicle", vehicleRef.Id);
            else
                throw new InvalidPluginExecutionException("Vehicle is not selected");
            if (opportuntiyGuid != null && opportuntiyGuid != Guid.Empty)
                entity["ap360_opportunityid"] = new EntityReference("opportunity", opportuntiyGuid);
            entity["ap360_opportunitynumber"] = Convert.ToInt32(opportuntiyNumber);

            entity["ap360_reviseditemstatus"] = new OptionSetValue(126300001);//approved

            entity["msdyn_workorder"] = new EntityReference("msdyn_workorder", workOrderGuid);
            entity["ap360_serviceroleid"] = new EntityReference("bookableresourcecategory", new Guid("108c5d2f-245e-ea11-a812-000d3a30f257"));//Mechanical Technician



            Guid guid = service.Create(entity);
            tracingservice.Trace("WorkOrder Service Task Created Guid " + guid);




        }
        public static Money UpdateWorkOrderServiceTaskHealthByAmount(ITracingService tracingService, IOrganizationService service, Guid wostID,
            Money ap360_predictedspend, Money EstimatedLaborAmount, int WOSTpercentcomplete, Money actualAmount, decimal preWosthealth)
        {

            Entity updateWorkOrderServiceTask = new Entity("msdyn_workorderservicetask", wostID);
            //  tracingService.Trace("Wost Percentage complete " + WOSTpercentcomplete.ToString());
            if (WOSTpercentcomplete > 0)
            {
                updateWorkOrderServiceTask["ap360_predictedspend"] = new Money((actualAmount.Value * (100 - WOSTpercentcomplete) / WOSTpercentcomplete) + actualAmount.Value);
                ap360_predictedspend = new Money((actualAmount.Value * (100 - WOSTpercentcomplete) / WOSTpercentcomplete) + actualAmount.Value);
            }
            // throw new InvalidPluginExecutionException("Wost Percentage complete " + WOSTpercentcomplete.ToString());
            if (EstimatedLaborAmount != null)
            {
                if (EstimatedLaborAmount.Value > 0 && ap360_predictedspend.Value > 0)
                {

                    //tracingService.Trace("Estimated Labor is  " + EstimatedLaborAmount.Value.ToString());
                    tracingService.Trace("POST WOST Health  " + ap360_predictedspend.Value / EstimatedLaborAmount.Value);

                    // updateWorkOrderServiceTask["ap360_lastbookingworkedonid"] = null;

                    if (preWosthealth > 0)
                    {
                        updateWorkOrderServiceTask["ap360_prewosthealth"] = preWosthealth;
                    }

                    updateWorkOrderServiceTask["ap360_postwosthealth"] = ap360_predictedspend.Value / EstimatedLaborAmount.Value;
                    service.Update(updateWorkOrderServiceTask);

                }
            }
            return ap360_predictedspend;
        }
        public static decimal UpdateWorkOrderServiceTaskHealthByDuration(ITracingService tracingService, IOrganizationService service, Entity entity, decimal DurationPredictedspend, int EstimatedDuration, int WOSTpercentcomplete, decimal timeSpentOnWOSTAsJourneyMan, decimal durationPostwosthealth, decimal ServiceRolePerMinuteRate)
        {
            Entity updateWorkOrderServiceTask = new Entity(entity.LogicalName, entity.Id);
            DurationPredictedspend = 0;

            tracingService.Trace("timeSpentOnWOSTAsJourneyMan " + timeSpentOnWOSTAsJourneyMan.ToString());
            tracingService.Trace("WOSTpercentcomplete " + WOSTpercentcomplete.ToString());
            tracingService.Trace("WOSTpercentcomplete " + WOSTpercentcomplete.ToString());
            tracingService.Trace("timeSpentOnWOSTAsJourneyMan " + timeSpentOnWOSTAsJourneyMan.ToString());


            if (WOSTpercentcomplete > 0)
            {
                tracingService.Trace("Predicted Spend Duration " + (timeSpentOnWOSTAsJourneyMan * (100 - WOSTpercentcomplete) / WOSTpercentcomplete) + timeSpentOnWOSTAsJourneyMan);
                DurationPredictedspend = (timeSpentOnWOSTAsJourneyMan * (100 - WOSTpercentcomplete) / WOSTpercentcomplete) + timeSpentOnWOSTAsJourneyMan;
                updateWorkOrderServiceTask["ap360_durationpredictedspend"] = Convert.ToInt32(DurationPredictedspend);


                updateWorkOrderServiceTask["ap360_durationpredictedspenddollars"] = new Money(DurationPredictedspend * ServiceRolePerMinuteRate);
                if (durationPostwosthealth > 0)
                    updateWorkOrderServiceTask["ap360_durationprewosthealth"] = durationPostwosthealth;
                if (EstimatedDuration > 0)
                {
                    updateWorkOrderServiceTask["ap360_durationpostwosthealth"] = DurationPredictedspend / EstimatedDuration;
                }

                service.Update(updateWorkOrderServiceTask);

            }
            return DurationPredictedspend;
        }

        public static Money getWOSTEstimatedDollarValue(ITracingService tracingService, Entity postImage, Money ap360_originalestimatedamount, Money ap360_revisedestimatedamount, Money EstimatedLaborAmount)
        {
            if (postImage.Contains("ap360_originalestimatedamount"))
            {
                ap360_originalestimatedamount = postImage.GetAttributeValue<Money>("ap360_originalestimatedamount") != null ? postImage.GetAttributeValue<Money>("ap360_originalestimatedamount") : null;
                if (ap360_originalestimatedamount.Value > 0)
                {
                    EstimatedLaborAmount.Value += ap360_originalestimatedamount.Value;
                }
                tracingService.Trace("ap360_originalestimatedamount " + ap360_originalestimatedamount.Value.ToString());
            }


            if (postImage.Contains("ap360_revisedestimatedamount") && (EstimatedLaborAmount != null && EstimatedLaborAmount.Value <= 0))
            {
                ap360_revisedestimatedamount = postImage.GetAttributeValue<Money>("ap360_revisedestimatedamount") != null ? postImage.GetAttributeValue<Money>("ap360_revisedestimatedamount") : null;
                if (ap360_revisedestimatedamount.Value > 0)
                {
                    EstimatedLaborAmount.Value += ap360_revisedestimatedamount.Value;
                }
                tracingService.Trace("ap360_revisedestimatedamount " + ap360_revisedestimatedamount.Value.ToString());
            }

            return EstimatedLaborAmount;

        }

        public static int getWOSTEstimatedDurationValue(ITracingService tracingService, Entity postImage, int msdyn_estimatedduration, int ap360_revisedestimatedduration, int EstimatedDuration)
        {
            if (postImage.Contains("msdyn_estimatedduration"))
            {
                msdyn_estimatedduration = postImage.GetAttributeValue<int>("msdyn_estimatedduration"); // Original Estimated Duration

                EstimatedDuration += msdyn_estimatedduration;

                tracingService.Trace("msdyn_estimatedduration " + msdyn_estimatedduration.ToString());
            }


            if (postImage.Contains("ap360_revisedestimatedduration") && EstimatedDuration <= 0)
            {
                ap360_revisedestimatedduration = postImage.GetAttributeValue<int>("ap360_revisedestimatedduration");

                EstimatedDuration += ap360_revisedestimatedduration;

                tracingService.Trace("ap360_revisedestimatedduration " + ap360_revisedestimatedduration.ToString());
            }

            return EstimatedDuration;

        }

        public static int CreateWorkOrderServiceTasks(IOrganizationService service, ITracingService tracingservice, List<QuoteServiceTask> lstQuoteServiceTask, Guid workOrderGuid, Quote quote, QuoteService qtSrv, out Guid firstQuoteServiceTask, List<WOSTandQSTObject> lstWOSTandQSTObject)
        {
            firstQuoteServiceTask = Guid.Empty;
            int estimatedduration = 0;
            int count = 0;

            foreach (QuoteServiceTask quoteServiceTask in lstQuoteServiceTask)
            {
                WOSTandQSTObject wOSTandQSTObject = null;
                count++;
                Entity entity = new Entity("msdyn_workorderservicetask");

                entity["msdyn_description"] = quoteServiceTask.Name;

                string selectedName = GetNameFromDescription(quoteServiceTask);

                entity["msdyn_name"] = selectedName;


                entity["msdyn_estimatedduration"] = quoteServiceTask.EstimatedTime;
                estimatedduration += quoteServiceTask.EstimatedTime;
                entity["ap360_isrevised"] = false;

                if (quote.Vechicle != null)
                    entity["ap360_vehicleid"] = new EntityReference(quote.Vechicle.LogicalName, quote.Vechicle.Id);


                if (quoteServiceTask.ParentServiceTemplate != null)
                    entity["ap360_parentservicetemplateid"] = new EntityReference(quoteServiceTask.ParentServiceTemplate.LogicalName, quoteServiceTask.ParentServiceTemplate.Id);
                if (quoteServiceTask.ServiceTemplate != null)
                    entity["ap360_servicetemplateid"] = new EntityReference(quoteServiceTask.ServiceTemplate.LogicalName, quoteServiceTask.ServiceTemplate.Id);
                if (quoteServiceTask.ChildServiceTemplate != null)
                    entity["ap360_childservicetemplateid"] = new EntityReference(quoteServiceTask.ChildServiceTemplate.LogicalName, quoteServiceTask.ChildServiceTemplate.Id);
                if (quoteServiceTask.ParentServiceTask != null)
                    entity["ap360_parentservicetaskid"] = new EntityReference(quoteServiceTask.ParentServiceTask.LogicalName, quoteServiceTask.ParentServiceTask.Id);

                entity["ap360_customnotes"] = quoteServiceTask.CustomNotes;

                if (qtSrv.HourlyRate != null)
                {
                    entity["ap360_hourlyrate"] = new Money(qtSrv.HourlyRate.Value);
                    entity["ap360_estimatedlaborprice"] = new Money((qtSrv.HourlyRate.Value * quoteServiceTask.EstimatedTime) / 60);
                    entity["ap360_originalestimatedamount"] = new Money((qtSrv.HourlyRate.Value * quoteServiceTask.EstimatedTime) / 60);
                    entity["ap360_predictedspend"] = new Money((qtSrv.HourlyRate.Value * quoteServiceTask.EstimatedTime) / 60);

                }
                if (qtSrv.ServiceRole != null)
                    entity["ap360_serviceroleid"] = new EntityReference(qtSrv.ServiceRole.LogicalName, qtSrv.ServiceRole.Id);

                if (quote.Opportunity != null)
                    entity["ap360_opportunityid"] = new EntityReference(quote.Opportunity.LogicalName, quote.Opportunity.Id);

                entity["msdyn_workorder"] = new EntityReference("msdyn_workorder", workOrderGuid);
                entity["ap360_opportunitynumber"] = Convert.ToDecimal(quote.OpportunityAutoNumber);
                entity["ap360_reviseditemstatus"] = new OptionSetValue(126300001);//Approved


                Guid guid = service.Create(entity);

                wOSTandQSTObject = new WOSTandQSTObject();
                wOSTandQSTObject.WOSTGuid = guid;
                wOSTandQSTObject.QSTGuid = quoteServiceTask.guid;

                lstWOSTandQSTObject.Add(wOSTandQSTObject);
                tracingservice.Trace(count.ToString() + " WOST Guid " + guid.ToString() + " QST GUID " + quoteServiceTask.guid.ToString());

                if (count == 1)// first Quote Service Task because all tasks are assocated with first service task
                {
                    firstQuoteServiceTask = guid;
                    entity["ap360_workorderservicetaskstatus"] = new OptionSetValue(126300011);//Awaiting dependency
                }

            }
            return estimatedduration;

        }

        public static void updateWorkOrderStatus(IOrganizationService service, ITracingService tracing, EntityReference workOrderRef, int wostStatus)
        {
            tracing.Trace("wost status " + wostStatus.ToString());
            //wostStatus == 126300011)//awaiting dependency	      
            //(wostStatus == 126300010)//Completed		
            //wostStatus == 126300000)//Need Manger Decision
            //wostStatus == 126300002)//Time Used Up/ Incomplete ST
            //wostStatus == 126300003)//Needs External help/ Expert		
            //wostStatus == 126300004)//needs lead tech guidance	
            //wostStatus == 126300007)//Needs Discovery Estimated	      
            if (wostStatus == 126300011 || wostStatus == 126300010 || wostStatus == 126300000 || wostStatus == 126300002 || wostStatus == 126300003 || wostStatus == 126300004 || wostStatus == 126300007)
            {
                Entity updateWorkOrder = new Entity(workOrderRef.LogicalName, workOrderRef.Id);
                updateWorkOrder["ap360_wobwstatus"] = new OptionSetValue(126300003);//Needs Admin FollowUp
                service.Update(updateWorkOrder);
            }

        }
        private static string GetNameFromDescription(QuoteServiceTask quoteServiceTask)
        {
            string selectedName = null;
            var descptionArray = quoteServiceTask.Name.Trim().Split(' ');
            var length = descptionArray.Length;

            if (length > 5)
            {
                for (var i = 0; i < 6; i++)
                {
                    selectedName += descptionArray[i];
                    selectedName += " ";

                }
            }
            else
            {
                selectedName = quoteServiceTask.Name;
            }

            return selectedName;
        }

        public static WorkOrderServiceTask GetWorkOrderServiceTaskDuration(IOrganizationService service, ITracingService tracingservice, Guid workOrderGuid)
        {

            List<WorkOrderServiceTask> lstWorkOrderServiceTask = new List<WorkOrderServiceTask>();

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='msdyn_workorderservicetask'>
                                <attribute name='msdyn_workorderservicetaskid' />
                                <attribute name='ap360_revisedestimatedduration' />
                                <attribute name='msdyn_actualduration' />


                                <attribute name='msdyn_estimatedduration' />

                                <attribute name='createdon' />
                                <order attribute='createdon' descending='false' />
                                    <filter type='and'>
                                      <condition attribute='msdyn_workorder' operator='eq'  value='" + workOrderGuid + @"' /> 
                                    </filter>
                                  </entity>
                                </fetch>");


            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            WorkOrderServiceTask workOrderServiceTask = new WorkOrderServiceTask();
            tracingservice.Trace("count of WorkOrderServiceTask " + col.Entities.Count.ToString());
            foreach (Entity entity in col.Entities)
            {
                // workOrderServiceTask.WOSTGuid.Id = entity.Id;

                int intOriginalEstimatedDuration = entity.GetAttributeValue<int>("msdyn_estimatedduration");
                int intActualduration = entity.GetAttributeValue<int>("msdyn_actualduration");
                int intRevisedEstimatedDuration = entity.GetAttributeValue<int>("ap360_revisedestimatedduration");

                workOrderServiceTask.TotalOriginalEstimatedDuration += intOriginalEstimatedDuration;
                workOrderServiceTask.TotalActualduration += intActualduration;
                workOrderServiceTask.TotalRevisedEstimatedDuration += intRevisedEstimatedDuration;


            }
            tracingservice.Trace("Sum of Total Orginal Estimated duration " + workOrderServiceTask.TotalOriginalEstimatedDuration.ToString());
            tracingservice.Trace("Sum of Total  Actual duration " + workOrderServiceTask.TotalActualduration.ToString());
            tracingservice.Trace("Sum of Total Revised Estimated duration " + workOrderServiceTask.TotalRevisedEstimatedDuration.ToString());
            return workOrderServiceTask;

        }
        public static void GetWOSTCumulativePredictedSpend(IOrganizationService service, ITracingService tracingservice, Guid workOrderGuid, ref decimal cumulativeSumofPredictedSpend, ref decimal workOrderRevisedEstimatedLaborAmount)
        {

            List<WorkOrderServiceTask> lstWorkOrderServiceTask = new List<WorkOrderServiceTask>();

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='msdyn_workorderservicetask'>
                              <attribute name='createdon' />
                                <attribute name='msdyn_name' />
                                <attribute name='msdyn_lineorder' />
                                <attribute name='msdyn_description' />
                                <attribute name='ap360_originalestimatedamount' />
                                <attribute name='ap360_revisedestimatedamount' />
                                <attribute name='msdyn_actualduration' />
                                <attribute name='msdyn_percentcomplete' />
                                <attribute name='msdyn_workorderservicetaskid' />
                                <attribute name='ap360_predictedspend' />
                                <order attribute='createdon' descending='false' />
                                    <filter type='and'>
                                      <condition attribute='msdyn_workorder' operator='eq'  value='" + workOrderGuid + @"' /> 
                                    </filter>
                                  </entity>
                                </fetch>");

            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            WorkOrderServiceTask workOrderServiceTask = null;
            tracingservice.Trace("count of WorkOrderServiceTask " + col.Entities.Count.ToString());
            // decimal cumulativeSumofPredictedSpend = 0;
            foreach (Entity entity in col.Entities)
            {
                // workOrderServiceTask.WOSTGuid.Id = entity.Id;
                workOrderServiceTask = new WorkOrderServiceTask();
                //  throw new InvalidPluginExecutionException("new custom error");
                decimal OriginalEstimatedDuration = 0;
                decimal RevisedEstimatedDuration = 0;
                OriginalEstimatedDuration = entity.GetAttributeValue<Money>("ap360_originalestimatedamount") != null ? entity.GetAttributeValue<Money>("ap360_originalestimatedamount").Value : 0;
                RevisedEstimatedDuration = entity.GetAttributeValue<Money>("ap360_revisedestimatedamount") != null ? entity.GetAttributeValue<Money>("ap360_revisedestimatedamount").Value : 0;
                if (OriginalEstimatedDuration != 0)
                {
                    workOrderRevisedEstimatedLaborAmount += OriginalEstimatedDuration;
                }
                else if (RevisedEstimatedDuration != 0)
                {
                    workOrderRevisedEstimatedLaborAmount += RevisedEstimatedDuration;

                }

                Money predictedSpend = entity.GetAttributeValue<Money>("ap360_predictedspend") != null ? entity.GetAttributeValue<Money>("ap360_predictedspend") : null;
                if (predictedSpend != null)
                {
                    cumulativeSumofPredictedSpend += predictedSpend.Value;
                }


            }



        }

        public static List<WorkOrderServiceTask> GetWorkOrderServiceTasksRelatedBRB(IOrganizationService service, Guid brbGuid)
        {

            List<WorkOrderServiceTask> lstWorkOrderServiceTask = new List<WorkOrderServiceTask>();

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='msdyn_workorderservicetask'>
                                <attribute name='msdyn_workorderservicetaskid' />
                                <attribute name='ap360_revisedestimatedduration' />
                                <attribute name='msdyn_actualduration' />
                                <attribute name='msdyn_booking' />



                                <attribute name='msdyn_estimatedduration' />

                                <attribute name='createdon' />
                                <order attribute='createdon' descending='false' />
                                    <filter type='and'>
                                      <condition attribute='msdyn_booking' operator='eq'  value='" + brbGuid + @"' /> 
                                    </filter>
                                  </entity>
                                </fetch>");


            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            WorkOrderServiceTask workOrderServiceTask = null;
            // tracingservice.Trace("count of WorkOrderServiceTask " + col.Entities.Count.ToString());
            foreach (Entity entity in col.Entities)
            {
                workOrderServiceTask = new WorkOrderServiceTask();
                workOrderServiceTask.WOSTGuid = entity.Id;
                workOrderServiceTask.Booking = entity.GetAttributeValue<EntityReference>("msdyn_booking") != null ? entity.GetAttributeValue<EntityReference>("msdyn_booking") : null;
                //workOrderServiceTask.RevisedEstimatedDuration = entity.GetAttributeValue<int>("ap360_revisedestimatedduration");
                //workOrderServiceTask.OriginalEstimatedDuration = entity.GetAttributeValue<int>("msdyn_estimatedduration");
                //workOrderServiceTask.Actualduration = entity.GetAttributeValue<int>("msdyn_actualduration");

                //workOrderServiceTask.TotalOriginalEstimatedDuration += workOrderServiceTask.OriginalEstimatedDuration;
                //workOrderServiceTask.TotalActualduration += workOrderServiceTask.Actualduration;
                //workOrderServiceTask.TotalRevisedEstimatedDuration += workOrderServiceTask.RevisedEstimatedDuration;
                lstWorkOrderServiceTask.Add(workOrderServiceTask);

            }
            return lstWorkOrderServiceTask;

        }
        public static void updateWOSTPercentComplete(IOrganizationService service, ITracingService tracing, List<BookingServiceTask> lstBookingServiceTask)
        {
            tracing.Trace("Inside updateWOSTPercentComplete. Booking service Count is " + lstBookingServiceTask.Count.ToString());
            int count = 0;
            foreach (BookingServiceTask bookingSrvTask in lstBookingServiceTask)
            {
                count++;
                tracing.Trace("Inside Foreach loop ");
                tracing.Trace("Count " + count.ToString());

                if (bookingSrvTask.WorkOrderServiceTask != null)
                {
                    tracing.Trace("Work Order Service Task is not null ");

                    Entity updateWOST = new Entity(bookingSrvTask.WorkOrderServiceTask.LogicalName, bookingSrvTask.WorkOrderServiceTask.Id);

                    //  updateWOST["msdyn_description"] = "updated from plugin";

                    // updateWOST["msdyn_percentcomplete"] = bookingSrvTask.PercentComplete;
                    service.Update(updateWOST);
                }
            }
        }
        public static void AssociateWOSTwithNewlyCeatedBRB(IOrganizationService service, List<WorkOrderServiceTask> lstWorkOrderServiceTask, Guid newlyCreatedBRBGuid)
        {
            foreach (WorkOrderServiceTask wost in lstWorkOrderServiceTask)
            {
                Entity entity = new Entity("msdyn_workorderservicetask");
                entity.Id = wost.WOSTGuid;
                entity["msdyn_booking"] = new EntityReference("bookableresourcebooking", newlyCreatedBRBGuid);
                service.Update(entity);
            }

        }

        public static void AssociateCurrentBookingwithWOST(IOrganizationService service, Guid WorkOrderServiceTaskGuid, Guid BRBGuid)
        {

            Entity entity = new Entity("msdyn_workorderservicetask");
            entity.Id = WorkOrderServiceTaskGuid;
            entity["ap360_lastbookingworkedonid"] = new EntityReference("bookableresourcebooking", BRBGuid);
            service.Update(entity);

        }
        public static string getWOSTStatusText(List<OptionSetValue> lstWorkOrderBWStatus)
        {


            string str = null;
            var groupedlstWorkOrderBWStatus = lstWorkOrderBWStatus.GroupBy(x => x.Value);
            //lstWorkOrderBWStatus = groupedlstWorkOrderBWStatus.ToList();
            foreach (var workOrderbwstatus in groupedlstWorkOrderBWStatus)
            {
                str += workOrderbwstatus.Key;
                str += ",";

            }
            return str;

        }

        public static string getWOSTStatusFormatedValue(List<string> lstWorkOrderBWStatus)
        {


            string str = null;
            var groupedlstWorkOrderBWStatus = lstWorkOrderBWStatus.GroupBy(x => x);
            //lstWorkOrderBWStatus = groupedlstWorkOrderBWStatus.ToList();
            foreach (var workOrderbwstatus in groupedlstWorkOrderBWStatus)
            {
                str += workOrderbwstatus.Key;
                str += ",";

            }
            return str;

        }
        public static List<Entity> GetWOSTrelatedToWorkOrder(IOrganizationService service, ITracingService tracingService, Guid workOrderGuid)
        {



            List<Entity> lstEntities = new List<Entity>();

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='msdyn_workorderservicetask'>
                                    <attribute name='createdon' />
                                    <attribute name='msdyn_name' />
                                    <attribute name='ap360_wbsid' />
                                    <attribute name='msdyn_description' />
                                    <attribute name='ap360_originalestimatedamount' />
                                    <attribute name='ap360_actualamount' />
                                    <attribute name='ap360_revisedestimatedamount' />
                                    <attribute name='ap360_workorderservicetaskstatus' />
                                    <attribute name='msdyn_percentcomplete' />
                                    <order attribute='createdon' descending='false' />
                                    <filter type='and'>
                                      <condition attribute='msdyn_workorder' operator='eq'  value='" + workOrderGuid + @"' /> 
                                    </filter>    
                                  </entity>
                                </fetch>");




            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));

            foreach (Entity ent in col.Entities)
            {

                lstEntities.Add(ent);

            }
            return lstEntities;
        }


        public static void GetWOSTrelatedToOpportunity(IOrganizationService service, ITracingService tracingService, Guid opportunityGuid, Guid workOrderGuid,
            Guid wostGuid, ref decimal OpportunityTimeStampEstimatedAmount, ref decimal workOrderTimeStampEstimatedAmount, ref decimal WOSTTimeStampEstimatedAmount)
        {
            List<Entity> lstEntities = new List<Entity>();



            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='msdyn_workorderservicetask'>
                                    <attribute name='createdon' />
                                    <attribute name='msdyn_name' />
                                    <attribute name='ap360_wbsid' />
                                    <attribute name='msdyn_description' />
                                    <attribute name='ap360_originalestimatedamount' />
                                    <attribute name='ap360_actualamount' />
                                    <attribute name='ap360_revisedestimatedamount' />
                                    <attribute name='ap360_workorderservicetaskstatus' />
                                    <attribute name='ap360_opportunityid' />
                                    <attribute name='msdyn_workorder' />
                                    <attribute name='msdyn_percentcomplete' />
                                    <order attribute='createdon' descending='false' />
                                
                                    <filter type='and'>
                                      <condition attribute='ap360_opportunityid' operator='eq'  value='" + opportunityGuid + @"' /> 
                                      <condition attribute='ap360_workorderservicetaskstatus' operator='in'>
                                            <value> 126300010 </value>
                                            <value> 126300009 </value>
                                            <value> 126300008 </value> 
                                            <value> 126300007 </value>
                                            <value> 126300000 </value>
                                            <value> 126300002 </value> 
                                            <value> 126300003 </value>
                                            <value> 126300004 </value>
                                            <value> 126300011 </value>
                                          </condition>
                                    </filter>    
                                   
                                  </entity>
                                </fetch>");

            //< link - entity name = 'ap360_woservicetasktimestamp' from = 'ap360_workorderservicetaskid' to = 'msdyn_workorderservicetaskid' link - type = 'inner' alias = 'ae' />

            //Incomplete - Return  126300008
            //Incomplete  126300009
            //Completed   126300010
            //Needs Discovery Estimated   126300007
            //Need Manger Decision    126300000
            //Time Used Up / Incomplete ST 126300002
            //Needs External help / Expert 126300003
            //Needs Lead Tech Guidance    126300004
            //Awaiting dependency 126300011
            //Not Started 126300001
            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            foreach (Entity entity in col.Entities)
            {
                Money estimatedAmount = entity.GetAttributeValue<Money>("ap360_originalestimatedamount") != null ?
                    entity.GetAttributeValue<Money>("ap360_originalestimatedamount") : entity.GetAttributeValue<Money>("ap360_revisedestimatedamount");

                if (estimatedAmount != null)
                {
                    // OpportunityTimeStampEstimatedAmount += estimatedAmount.Value;

                    EntityReference workOrderRef = entity.GetAttributeValue<EntityReference>("msdyn_workorder") ?? null;
                    if (workOrderRef != null && estimatedAmount != null && workOrderRef.Id == workOrderGuid)
                    {
                        //  workOrderTimeStampEstimatedAmount += estimatedAmount.Value;
                    }

                    Guid wostGUID = entity.Id;
                    //if ( estimatedAmount != null && wostGUID == wostGuid)
                    //{
                    //    WOSTTimeStampEstimatedAmount += estimatedAmount.Value;
                    //}

                }


            }


        }



        public static List<WorkOrderServiceTask> GetlstOfWOSTrelatedToWorkOrder(IOrganizationService service, ITracingService tracingService, Guid workOrderGuid)
        {
            List<WorkOrderServiceTask> lstWOST = new List<WorkOrderServiceTask>();
            List<Entity> lstEntites = GetWOSTrelatedToWorkOrder(service, tracingService, workOrderGuid);


            foreach (Entity reterviedWOST in lstEntites)
            {
                WorkOrderServiceTask WOST = new WorkOrderServiceTask();

                WOST.WOSTGuid = reterviedWOST.Id;
                //msdyn_name
                WOST.Name = reterviedWOST.GetAttributeValue<string>("msdyn_name");
                WOST.WBSID = reterviedWOST.GetAttributeValue<int>("ap360_wbsid");

                //if (wrkOrder.WBSID > 0)//only for workorders where id is not assigned
                lstWOST.Add(WOST);

            }


            return lstWOST;
        }



        public static IDictionary<OptionSetValue, string> GetWorkOrderBWStatusCollection(List<Entity> lstWOSTs, ITracingService tracingService, IDictionary<OptionSetValue, string> lstAlreadyWorkOrderBWStatusValue)
        {
            tracingService.Trace("***************************inside GetWorkOrderBWStatus : List of WOST Count " + lstWOSTs.Count.ToString());
            IDictionary<OptionSetValue, string> lstWorkOrderBWStatus = new Dictionary<OptionSetValue, string>();
            if (lstAlreadyWorkOrderBWStatusValue.ContainsKey(new OptionSetValue(126300008)))//Work Ordre 'Available for Booking-InProgress'
            {
                lstWorkOrderBWStatus.Add(new OptionSetValue(126300008), "Available For Booking-InProgress");

            }
            int count = 0;
            foreach (Entity workOrderServiceTask in lstWOSTs)
            {
                if (workOrderServiceTask.GetAttributeValue<OptionSetValue>("ap360_workorderservicetaskstatus") != null)
                {
                    int wostStatus = workOrderServiceTask.GetAttributeValue<OptionSetValue>("ap360_workorderservicetaskstatus").Value;
                    tracingService.Trace("WOST status " + wostStatus.ToString());


                    if (wostStatus == 126300010) //Completed
                    {
                        count++;

                        if (count == lstWOSTs.Count)
                        {
                            tracingService.Trace("Completed");

                            lstWorkOrderBWStatus.Remove(new OptionSetValue(126300008));///Work Order 'Available for Booking-InProgress'
                            //if all WOST's are complete

                            if (!lstWorkOrderBWStatus.ContainsKey(new OptionSetValue(126300001)))
                                lstWorkOrderBWStatus.Add(new OptionSetValue(126300001), "Completed");
                        }
                        //Have to check All workorderST
                    }
                    else if (
                    wostStatus == 126300008 || //Incomplete - Return 
                    wostStatus == 126300001  //Not Started	
                    ) 
                    {
                        tracingService.Trace("available/incomplete");
                        if (!lstWorkOrderBWStatus.ContainsKey(new OptionSetValue(126300008)))
                            lstWorkOrderBWStatus.Add(new OptionSetValue(126300008), "Available For Booking-InProgress");

                        // lstWorkOrderBWStatus.Add(new OptionSetValue(126300008));//Available For Booking-InProgress
                    }                           
                    else if (
                    wostStatus == 126300000 || //Incomplete - Need Manger Decision   126300000
                    wostStatus == 126300005 || //Incomplete - Needs billed child Service Task    
                    wostStatus == 126300006 || //Incomplete - Needs Unbilled child Service task
                    wostStatus == 126300004 || //Incomplete - Needs Lead Tech Guidance
                    wostStatus == 126300009 ||  //Incomplete - Incorrect Part
                    wostStatus == 126300007)   //Incomplete - Needs Discovery Estimated
                    {
                        tracingService.Trace("need manager/time used/Needs eternal/Needs lead/Needs discovery");
                        if (!lstWorkOrderBWStatus.ContainsKey(new OptionSetValue(126300003)))
                            lstWorkOrderBWStatus.Add(new OptionSetValue(126300003), "Needs Admin FollowUp");

                        //lstWorkOrderBWStatus.Add(new OptionSetValue(126300003));//Need Admin FollowUp
                    }
                    else if (wostStatus == 126300011) //awaiting dependency
                    {
                        tracingService.Trace("awaiting dependency");
                        if (!lstWorkOrderBWStatus.ContainsKey(new OptionSetValue(126300007)))
                            lstWorkOrderBWStatus.Add(new OptionSetValue(126300007), "Awaiting Dependency");

                        //  lstWorkOrderBWStatus.Add(new OptionSetValue(126300007));//Awaiting Dependency
                    }

                }
            }
            tracingService.Trace("New WorkOrderBW Status");
            foreach (var item in lstWorkOrderBWStatus)
            {
                tracingService.Trace(item.Key.Value.ToString());
            }
            tracingService.Trace("Already WorkOrderBW Status");
            //foreach (var item in lstAlreadyWorkOrderBWStatusValue)
            //{
            //    tracingService.Trace(item.Key.Value.ToString());
            //}


            //tracingService.Trace("Final WorkOrderBW Status");

            //foreach (var item in lstWorkOrderBWStatus)
            //{
            //    tracingService.Trace( item.Key.Value.ToString());
            //}
            //throw new InvalidPluginExecutionException("error");
            tracingService.Trace("***************************end GetWorkOrderBWStatus");
            return lstWorkOrderBWStatus;
        }

        public static List<OptionSetValue> GetWorkOrderBWStatus(List<Entity> lstWOSTs, ITracingService tracingService)
        {
            tracingService.Trace("***************************inside GetWorkOrderBWStatus : List of WOST Count " + lstWOSTs.Count.ToString());
            List<OptionSetValue> lstWorkOrderBWStatus = new List<OptionSetValue>();
            int count = 0;
            foreach (Entity workOrderServiceTask in lstWOSTs)
            {
                if (workOrderServiceTask.GetAttributeValue<OptionSetValue>("ap360_workorderservicetaskstatus") != null)
                {
                    int wostStatus = workOrderServiceTask.GetAttributeValue<OptionSetValue>("ap360_workorderservicetaskstatus").Value;
                    tracingService.Trace("WOST status " + wostStatus.ToString());

                    if (wostStatus == 126300010) //wost completed
                    {
                        count++;

                        if (count == lstWOSTs.Count)
                        {
                            //throw new InvalidPluginExecutionException(lstWOSTs.Count.ToString());
                            tracingService.Trace("Completed");
                            lstWorkOrderBWStatus.Add(new OptionSetValue(126300001));//workOrder completed
                        }
                        //Have to check All workorderST
                    }
                    else if (
                    wostStatus == 126300008 || //incomplete - return
                    wostStatus == 126300001 || //Not Started	
                    wostStatus == 126300009) //incomplete
                    {
                        tracingService.Trace("available/incomplete");
                        lstWorkOrderBWStatus.Add(new OptionSetValue(126300008));//Available For Booking-InProgress
                    }
                    else if (
                    wostStatus == 126300000 || //need manager Decision
                    wostStatus == 126300002 || //time used up/incomplete ST
                    wostStatus == 126300003 || //Needs eternal help/expert
                    wostStatus == 126300004 || //Needs lead tech guidance
                    wostStatus == 126300007)   //Needs discovery estimated
                    {
                        tracingService.Trace("need manager/time used/Needs eternal/Needs lead/Needs discovery");
                        lstWorkOrderBWStatus.Add(new OptionSetValue(126300003));//Need Admin FollowUp
                    }
                    else if (wostStatus == 126300011) //awaiting dependency
                    {
                        tracingService.Trace("awaiting dependency");
                        lstWorkOrderBWStatus.Add(new OptionSetValue(126300007));//Awaiting Dependency
                    }

                }
            }
            foreach (OptionSetValue optsvalue in lstWorkOrderBWStatus)
            {
                tracingService.Trace("WorkORDER Status selected based on WOST's " + optsvalue.Value.ToString());
            }
            tracingService.Trace("***************************end GetWorkOrderBWStatus");
            return lstWorkOrderBWStatus;
        }


        public static void caculateWOSTTimeStamps(IOrganizationService service, ITracingService tracing, List<BookingServiceTask> lstBookingServiceTask, Entity BRBEntity, string techOrAdmin)
        {

            BookableResourceBooking bookableResourceBooking = new BookableResourceBooking();
            TimeSpan timespentonBooking = TimeSpan.MinValue;

            bookableResourceBooking.TotalBillableDuration = BRBEntity.GetAttributeValue<int>("msdyn_totalbillableduration");

            string shopeTaskClassificationId = "1e174c8a-a858-ea11-a811-000d3a30f257";

            int? currentusertimezone = Methods.RetrieveCurrentUsersTimeZoneSettings(service);

            if (BRBEntity.Contains("msdyn_actualarrivaltime"))
            {
                bookableResourceBooking.ActualArrivalTime = BRBEntity["msdyn_actualarrivaltime"] is DateTime ? (DateTime)BRBEntity["msdyn_actualarrivaltime"] : new DateTime();
                bookableResourceBooking.ActualArrivalTime = (DateTime)Methods.RetrieveLocalTimeFromUTCTime(service, bookableResourceBooking.ActualArrivalTime, currentusertimezone);
            }
            if (BRBEntity.Contains("ap360_finishtime"))
            {
                bookableResourceBooking.FinishTime = BRBEntity["ap360_finishtime"] is DateTime ? (DateTime)BRBEntity["ap360_finishtime"] : new DateTime();
                bookableResourceBooking.FinishTime = (DateTime)Methods.RetrieveLocalTimeFromUTCTime(service, bookableResourceBooking.FinishTime, currentusertimezone);
            }

            timespentonBooking = bookableResourceBooking.FinishTime - bookableResourceBooking.ActualArrivalTime;
            bookableResourceBooking.WordOrderServiceTask = BRBEntity.GetAttributeValue<EntityReference>("ap360_workorderservicetask") != null ? BRBEntity.GetAttributeValue<EntityReference>("ap360_workorderservicetask") : null;

            

            decimal billingPrice = 0;
            tracing.Trace("******************************************Before***********************************: BST Count" + lstBookingServiceTask.Count.ToString());
            int count = 0;
            List<WorkOrderServiceTaskTimeStamp> lstWorKOrderServiceTaskTimeStampsForHealthUpdate = new List<WorkOrderServiceTaskTimeStamp>();
            int bstloopCount = 0;
            int WOSTTimeStampCount = 0;
           //throw new InvalidPluginExecutionException("error "+lstBookingServiceTask.Count.ToString());
            foreach (BookingServiceTask bookingServiceTask in lstBookingServiceTask)
            {

                bstloopCount++;
                count++;
                tracing.Trace("***************inside for each item processing number is " + count.ToString());
                decimal timespent = 0;

                EntityReference bookingResource = BRBEntity.GetAttributeValue<EntityReference>("resource") != null ? BRBEntity.GetAttributeValue<EntityReference>("resource") : null;
                EntityReference bookingServiceRole = BRBEntity.GetAttributeValue<EntityReference>("ap360_serviceroles") != null ? BRBEntity.GetAttributeValue<EntityReference>("ap360_serviceroles") : null;
                decimal ServiceRoleHourlyRate = 0;
                decimal ResourceHourlyRate = 0;
                Methods.getResourseAndServiceRoleRates(service, tracing, bookingServiceRole, bookingResource, ref ServiceRoleHourlyRate, ref ResourceHourlyRate, BRBEntity);

                if (bookableResourceBooking.TotalBillableDuration == 0)
                {
                    timespent = (int)Math.Ceiling((timespentonBooking.TotalMinutes / 100) * (double)bookingServiceTask.PercentTimeSpent);
                    tracing.Trace("if: " + timespent.ToString());
                    billingPrice = bookableResourceBooking.GetBillingPrice(service, tracing, BRBEntity.Id, "ap360_timespentonbooking", WOSTTimeStampCount);

                }
                else
                {

                    double result = (bookableResourceBooking.TotalBillableDuration / 100);
                    timespent = (decimal)Math.Ceiling((result) * (int)bookingServiceTask.PercentTimeSpent);
                    billingPrice = bookableResourceBooking.GetBillingPrice(service, tracing, BRBEntity.Id, "msdyn_totalbillableduration", WOSTTimeStampCount);
                    tracing.Trace("else: Total Billable Duraiton not  eq 0");
                }
                if (bookingServiceTask.IsMasterBST && BRBEntity.GetAttributeValue<EntityReference>("ap360_bookingclassification").Id.ToString().ToLower() == shopeTaskClassificationId)
                {
                    double result = (bookableResourceBooking.TotalBillableDuration / 100);
                    timespent = (decimal)Math.Ceiling((result) * 100);
                }
                WorkOrderServiceTaskTimeStamp workOrderServiceTaskTimeStamp = null;

                tracing.Trace("Before GetWorkOrderServiceTaskTimeStamp");
                workOrderServiceTaskTimeStamp = WorkOrderServiceTaskTimeStamp.GetWorkOrderServiceTaskTimeStamp(service, tracing, bookingServiceTask.BSTGuid, BRBEntity.Id);

                tracing.Trace("after GetWorkOrderServiceTaskTimeStamp");

                if (bookingServiceTask.PercentTimeSpent == 0 && workOrderServiceTaskTimeStamp != null)
                {
                    tracing.Trace(count.ToString() + "******************booking percent time spent is 0 and WorkOrderServieTaskTimeStamp is not null");
                    service.Delete(workOrderServiceTaskTimeStamp.LogicalName, workOrderServiceTaskTimeStamp.guid);
                    // WOSTTimeStampCount--;
                    continue;
                }
                tracing.Trace("before BST percenttimespent is not 0");
                if (bookingServiceTask.PercentTimeSpent != 0 || (bookingServiceTask.IsMasterBST && BRBEntity.GetAttributeValue<EntityReference>("ap360_bookingclassification").Id.ToString().ToLower()== shopeTaskClassificationId))
                {

                   
                    tracing.Trace(count.ToString() + "******************bookingServiceTask.PercentTimeSpent == 0");


                    if (workOrderServiceTaskTimeStamp == null)
                    {
                        ///////////////////////////////////////new code 5/19/2021//////////////////////////

                        tracing.Trace("IF : inside workOrderServiceTaskTimeStamp not null");

                        //Entity lastBookingRelatedWOST = BookableResourceBooking.GetLastFinsihedBrbOfWOST(service, tracing,bookingServiceTask.WorkOrderServiceTask.Id);
                        //if (lastBookingRelatedWOST != null)
                        //{
                        if (BRBEntity.GetAttributeValue<EntityReference>("ap360_workorderservicetask") != null)
                        {
                            if (bookingServiceTask.WorkOrderServiceTask.Id == BRBEntity.GetAttributeValue<EntityReference>("ap360_workorderservicetask").Id)
                            {
                                tracing.Trace("Creating new bookings only for the master task");
                                tracing.Trace("Same bookingServiceTask.WorkOrderServiceTask and BRBEntity.GetAttributeValue<EntityReference>(ap360_workorderservicetask)");
                                if (BRBEntity.GetAttributeValue<OptionSetValue>("ap360_workorderservicetaskstatus") != null)
                                {
                                    OptionSetValue wostStatus = BRBEntity.GetAttributeValue<OptionSetValue>("ap360_workorderservicetaskstatus");
                                    if (wostStatus != null)
                                    {
                                        tracing.Trace("wost status is not null");
                                        if (wostStatus.Value == 126300008)//Incompete Return
                                        {
                                            tracing.Trace("woststatus 126300008(incomplete return) CreateNewBRBonODone");
                                            BookableResourceBooking.CreateBookAbleResourceBooking(service, tracing, BRBEntity, null, null,"no");
                                        }
                                    }
                                }
                            }
                        }

                        tracing.Trace("Technician mark job Done : WO Service Task Time Stamp is null: Billing price " + billingPrice.ToString());
                        WorkOrderServiceTaskTimeStamp.CreateWOSTTimeStamp(service, tracing, BRBEntity, billingPrice, bookingServiceTask,
                        timespent, ServiceRoleHourlyRate, ResourceHourlyRate, lstWorKOrderServiceTaskTimeStampsForHealthUpdate);
                        WOSTTimeStampCount++;
                        WorkOrderServiceTask.updateWOSTActuals(service, tracing, bookingServiceTask, BRBEntity);
                        tracing.Trace("End of If ");
                    }
                    else
                    {
                        tracing.Trace("ELSE: Admin mark job Closed: WO Service Task Time Stamp is not null");

                        WorkOrderServiceTaskTimeStamp.UpdateWOSTTimeStamp(service, tracing, BRBEntity, billingPrice, bookingServiceTask, timespent, bookingResource,
                            bookingServiceRole, ref ServiceRoleHourlyRate, ref ResourceHourlyRate, workOrderServiceTaskTimeStamp,
                            lstWorKOrderServiceTaskTimeStampsForHealthUpdate, bstloopCount);
                        WOSTTimeStampCount++;
                        WorkOrderServiceTask.updateWOSTActuals(service, tracing, bookingServiceTask, BRBEntity);
                    }
                }
                tracing.Trace("after BST percenttimespent is not 0");
            }

            if (techOrAdmin == "technicianFinshed")
            {
                if (bookableResourceBooking.WordOrderServiceTask != null)
                {
                    TaskActivity.CreateTaskForTechnicianClockOut(service, tracing, BRBEntity, bookableResourceBooking.WordOrderServiceTask);
                }
            }
            Entity updateBRB = new Entity("bookableresourcebooking");
            updateBRB.Id = BRBEntity.Id;
            updateBRB["ap360_bookingactualamounts"] = new Money(billingPrice);
            updateBRB["ap360_workorderactualamount"] = new Money(billingPrice);
            updateBRB["ap360_wosttimestampcount"] = WOSTTimeStampCount;

            updateBRB["ap360_calculateactualamount"] = false;
            service.Update(updateBRB);

          //  throw new InvalidPluginExecutionException("Error");



            tracing.Trace("Count of WOSTTimestamp " + lstWorKOrderServiceTaskTimeStampsForHealthUpdate.Count.ToString());

            if (lstWorKOrderServiceTaskTimeStampsForHealthUpdate.Count > 1)
            {
                tracing.Trace("Last Opportunity Health " + lstWorKOrderServiceTaskTimeStampsForHealthUpdate[lstWorKOrderServiceTaskTimeStampsForHealthUpdate.Count - 1]
                    .opportunityPOSTHealth.ToString());
                WorkOrderServiceTaskTimeStamp lastworkserviceTaskTimeStamp = lstWorKOrderServiceTaskTimeStampsForHealthUpdate[lstWorKOrderServiceTaskTimeStampsForHealthUpdate.Count - 1];

                //Tmeporarily disabled
                lstWorKOrderServiceTaskTimeStampsForHealthUpdate.RemoveAt(lstWorKOrderServiceTaskTimeStampsForHealthUpdate.Count - 1);

                ///////////////////////////////////////////////
                //WorkOrderServiceTaskTimeStamp workOrderServiceTaskTimeStamp  = lstWorKOrderServiceTaskTimeStampsForHealthUpdate.OrderBy(x => x.PredictedSpend).First();

                //if (workOrderServiceTaskTimeStamp != null)
                //{
                //    foreach (WorkOrderServiceTaskTimeStamp wostTimeStampHealhUpdate in lstWorKOrderServiceTaskTimeStampsForHealthUpdate)
                //    {
                //        Entity updateWOSTTimeStampHealth = new Entity("ap360_woservicetasktimestamp");
                //        updateWOSTTimeStampHealth["ap360_opportunityposthealth"] = workOrderServiceTaskTimeStamp.opportunityPOSTHealth;
                //        updateWOSTTimeStampHealth["ap360_workorderposthealth"] = workOrderServiceTaskTimeStamp.workOrderPOSTHealth;

                //        tracing.Trace("guid: " + wostTimeStampHealhUpdate.guid.ToString());
                //        updateWOSTTimeStampHealth.Id = wostTimeStampHealhUpdate.guid;
                //        service.Update(updateWOSTTimeStampHealth);
                //    }
                //}
                //////////////////////////////////////////////////
                foreach (WorkOrderServiceTaskTimeStamp wostTimeStampHealhUpdate in lstWorKOrderServiceTaskTimeStampsForHealthUpdate)
                {

                    Entity updateWOSTTimeStampHealth = new Entity("ap360_woservicetasktimestamp");
                    updateWOSTTimeStampHealth["ap360_opportunityposthealth"] = lastworkserviceTaskTimeStamp.opportunityPOSTHealth;
                    updateWOSTTimeStampHealth["ap360_workorderposthealth"] = lastworkserviceTaskTimeStamp.workOrderPOSTHealth;
                    updateWOSTTimeStampHealth["ap360_opportunitydurationhealth"] = lastworkserviceTaskTimeStamp.opportunityPOSTHealth;
                    updateWOSTTimeStampHealth["ap360_workorderdurationhealth"] = lastworkserviceTaskTimeStamp.workOrderPOSTHealth;

                    tracing.Trace("guid: " + wostTimeStampHealhUpdate.guid.ToString());
                    updateWOSTTimeStampHealth.Id = wostTimeStampHealhUpdate.guid;
                    service.Update(updateWOSTTimeStampHealth);


                }
            }
            tracing.Trace("******************************************After***********************************");
            //  throw new InvalidPluginExecutionException("Error ");
        }


        public static void updateWOSTActuals(IOrganizationService service, ITracingService tracing, BookingServiceTask bookingServiceTask, Entity BRBEntity)
        {
            if (bookingServiceTask.WorkOrderServiceTask != null)
            {
                int TotalWOSTTimeStampDuration = 0;
                decimal TotalWOSTTimeStampActualAmount = 0.0M;
                int TotalWOSTJourneyManActualTimeSpent = 0;

                Entity updateWOST = new Entity(bookingServiceTask.WorkOrderServiceTask.LogicalName, bookingServiceTask.WorkOrderServiceTask.Id);
                WorkOrderServiceTaskTimeStamp.GetSumOfWorkOrderServiceTaskTimeStampRelatedtoWOST(service, tracing, bookingServiceTask.WorkOrderServiceTask.Id, ref TotalWOSTTimeStampDuration, ref TotalWOSTTimeStampActualAmount, ref TotalWOSTJourneyManActualTimeSpent);
                //  throw new InvalidPluginExecutionException("Custom error new ");
                updateWOST["msdyn_actualduration"] = Convert.ToInt32(TotalWOSTTimeStampDuration);
                updateWOST["ap360_actualamount"] = new Money(TotalWOSTTimeStampActualAmount);
                updateWOST["ap360_journeymanactualduration"] = TotalWOSTJourneyManActualTimeSpent;
                if ( bookingServiceTask.IsMasterBST)
                {
                    updateWOST["ap360_workorderservicetaskstatus"] = BRBEntity.GetAttributeValue<OptionSetValue>("ap360_workorderservicetaskstatus") ?? null;
                    updateWOST["ap360_workorderservicetasksubstatus"] = BRBEntity.GetAttributeValue<OptionSetValue>("ap360_workorderservicetasksubstatus") ?? null;
                }
                service.Update(updateWOST);
            }
        }
        public static void mapWOServiceTasktoRevisedItem(IOrganizationService service, Entity reterviedEntity, RevisedItem revisedItem)
        {
            revisedItem.Name = reterviedEntity.GetAttributeValue<string>("msdyn_name");
            revisedItem.ExtendedPrice = reterviedEntity.GetAttributeValue<Money>("ap360_revisedestimatedamount") != null ? reterviedEntity.GetAttributeValue<Money>("ap360_revisedestimatedamount") : null;
            revisedItem.WorkOrder = reterviedEntity.GetAttributeValue<EntityReference>("msdyn_workorder") != null ? reterviedEntity.GetAttributeValue<EntityReference>("msdyn_workorder") : null;
            revisedItem.Opportunity = reterviedEntity.GetAttributeValue<EntityReference>("ap360_opportunityid") != null ? reterviedEntity.GetAttributeValue<EntityReference>("ap360_opportunityid") : null;
            revisedItem.RevisedItemStatus = reterviedEntity.GetAttributeValue<OptionSetValue>("ap360_reviseditemstatus").Value;
            revisedItem.WOServiceTask = reterviedEntity.ToEntityReference();

        }
        public static string getWOSTStatusName(int statusOption)
        {
            string statusName = "";
            if (statusOption == 126300011)
            {
                statusName = "awaiting dependency";
            }
            else if (statusOption == 126300010)
            {
                statusName = "Completed";
            }
            else if (statusOption == 126300000)
            {
                statusName = "Need Manger Decision";
            }
            else if (statusOption == 126300001)
            {
                statusName = "In Progress/ Underway";
            }
            else if (statusOption == 126300002)
            {
                statusName = "Time Used Up/ Incomplete ST";
            }
            else if (statusOption == 126300003)
            {
                statusName = "Needs External help/ Expert";
            }
            else if (statusOption == 126300004)
            {
                statusName = "needs lead tech guidance";
            }
            else if (statusOption == 126300005)
            {
                statusName = "available for booking";
            }
            else if (statusOption == 126300006)
            {
                statusName = "awaiting predecessor";
            }
            else if (statusOption == 126300007)
            {
                statusName = "Needs Discovery Estimated";
            }

            else if (statusOption == 126300008)
            {
                statusName = "Incomplete- Return";
            }
            else if (statusOption == 126300009)
            {
                statusName = "Incomplete- Not Returning";
            }
            return statusName;
        }
        public static string getupdatedWOSTStatusName(int statusOption)
        {
            string statusName = "";
            if (statusOption == 126300002)
            {
                statusName = "Time Used Up/ Incomplete ST";
            }
            else if (statusOption == 126300003)
            {
                statusName = "Needs External help/ Expert";
            }
            else if (statusOption == 126300010)
            {
                statusName = "Completed";
            }
            else if (statusOption == 126300008)
            {
                statusName = "Incomplete - Return";
            }
            else if (statusOption == 126300005)
            {
                statusName = "Incomplete - Needs billed child Service Task";
            }
            else if (statusOption == 126300006)
            {
                statusName = "Incomplete - Needs Unbilled child Service task";
            }
            else if (statusOption == 126300000)
            {
                statusName = "Incomplete - Need Manger Decision";
            }
            else if (statusOption == 126300004)
            {
                statusName = "Incomplete - Needs Lead Tech Guidance";
            }
            else if (statusOption == 126300009)
            {
                statusName = "Incomplete - Incorrect Part";
            }
            else if (statusOption == 126300007)
            {
                statusName = "Incomplete - Needs Discovery Estimated";
            }

            else if (statusOption == 126300011)
            {
                statusName = "Awaiting dependency";
            }
            else if (statusOption == 126300001)
            {
                statusName = "Not Started";
            }
            return statusName;
        }
        public static string getupdatedWOSTSubStatusName(int subStatusOption)
        {
            string statusName = "";
            if (subStatusOption == 126300004)
            {
                statusName = "Efficiency Improvement Opportunity";
            }
            else if (subStatusOption == 126300005)
            {
                statusName = "Waiting on Part";
            }
            else if (subStatusOption == 126300006)
            {
                statusName = "Inc-Needs Lead Tech Guidance";
            }
            else if (subStatusOption == 126300008)
            {
                statusName = "Planned Time Insufficient";
            }
            else if (subStatusOption == 126300012)
            {
                statusName = "Misused Allotted Time";
            }
            else if (subStatusOption == 126300013)
            {
                statusName = "Overestimated Capability";
            }
            else if (subStatusOption == 126300014)
            {
                statusName = "Should have asked for help sooner";
            }
            else if (subStatusOption == 126300015)
            {
                statusName = "We damaged";
            }
            else if (subStatusOption == 126300016)
            {
                statusName = "For Clean up";
            }
            else if (subStatusOption == 126300009)
            {
                statusName = "Poor OOB part fitment";
            }

            else if (subStatusOption == 126300010)
            {
                statusName = "Defective New Part Provided";
            }
            else if (subStatusOption == 126300011)
            {
                statusName = "Incorrect Part Buried";
            }
            else if (subStatusOption == 126300007)
            {
                statusName = "Inc- Needs MGR Decision";
            }
            else if (subStatusOption == 126300003)
            {
                statusName = "Assist CoWorker";
            }
            else if (subStatusOption == 126300000)
            {
                statusName = "Personal";
            }
            else if (subStatusOption == 126300001)
            {
                statusName = "Lunch";
            }
            else if (subStatusOption == 126300002)
            {
                statusName = "End of Day";
            }
            else if (subStatusOption == 126300017)
            {
                statusName = "Other- Description Required";
            }
            return statusName;
        }


        public static int getWOSTCount(IOrganizationService service, ITracingService tracing, EntityReference WOSTRef)
        {
            tracing.Trace("Inside getWOSTCOunt function");
            List<Entity> lstEntities = new List<Entity>();
            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='msdyn_workorderservicetask'>
                                <attribute name='msdyn_name' />
                                <attribute name='msdyn_workorderservicetaskid' />
                                <order attribute='msdyn_name' descending='false' />
                                <filter type='and'>
                                  <condition attribute='ap360_workorderservicetaskid' operator='eq' value='" + WOSTRef.Id + @"' />
                                </filter>
                              </entity>
                            </fetch>");
            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));


            return col.Entities.Count;
        }
    }

}