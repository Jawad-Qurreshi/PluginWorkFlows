
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code.Model
{
    public class WorkOrderServiceTaskTimeStamp
    {

        public Guid guid { get; set; }

        public string LogicalName { get; set; }
        public string Name { get; set; }

        public decimal WOSTTimeStampActualAmount { get; set; }
        public decimal TotalWOSTTimeStampPredictedSpendAmount { get; set; }
        public decimal TotalWOSTTimeStampDurationPredictedSpendAmount { get; set; }

        public int TotalWOSTTimeStampDuration { get; set; }
        public DateTime CreatedON { get; set; }
        public decimal opportunityDurationHealth { get; set; }
        public decimal workOrderDurationHealth { get; set; }
        public decimal wostDurationHealth { get; set; }
        public decimal opportunityPOSTHealth { get; set; }
        public decimal workOrderPOSTHealth { get; set; }
        public decimal wostPOSTHealth { get; set; }
        public decimal PredictedSpend { get; set; }
        public decimal WOSTTimeStampDurationDollar { get; set; }

        public decimal WOSTPreDurationHealth { get; set; }
        public decimal WOSTPreHealth { get; set; }
        public decimal WorkOrderPreDurationHealth { get; set; }
        public decimal WorkOrderPreHealth { get; set; }
        public decimal OpportunityPreDurationHealth { get; set; }
        public decimal OpportunityPreHealth { get; set; }

        public static WorkOrderServiceTaskTimeStamp GetWorkOrderServiceTaskTimeStamp(IOrganizationService service, ITracingService tracing, Guid bookingServiceTaskGuid, Guid brbGuid)
        {


            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='ap360_woservicetasktimestamp'>
                                    <attribute name='ap360_woservicetasktimestampid' />
                                    <attribute name='ap360_name' />
                                    <attribute name='createdon' />
                                    <order attribute='createdon' descending='false' />
                                    <filter type='and'>
                                      <condition attribute='ap360_bookingservicetaskid' operator='eq'  value='" + bookingServiceTaskGuid + @"' /> 
                                      <condition attribute='ap360_bookableresourcebookingid' operator='eq'  value='" + brbGuid + @"' /> 
                                  
                                    </filter>
                                  </entity>
                                </fetch>");



            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            tracing.Trace("inside GetWorkOrderServiceTaskTimeStamp Col count " + col.Entities.Count.ToString());
            WorkOrderServiceTaskTimeStamp workOrderServiceTaskTimeStamp = null;
            foreach (Entity entity in col.Entities)
            {
                workOrderServiceTaskTimeStamp = new WorkOrderServiceTaskTimeStamp();
                workOrderServiceTaskTimeStamp.guid = entity.Id;

                workOrderServiceTaskTimeStamp.LogicalName = entity.LogicalName;
                workOrderServiceTaskTimeStamp.Name = entity.GetAttributeValue<string>("ap360_name");
                workOrderServiceTaskTimeStamp.CreatedON = entity.GetAttributeValue<DateTime>("createdon");
                int? currentusertimezone = Methods.RetrieveCurrentUsersTimeZoneSettings(service);
                workOrderServiceTaskTimeStamp.CreatedON = (DateTime)Methods.RetrieveLocalTimeFromUTCTime(service, workOrderServiceTaskTimeStamp.CreatedON, currentusertimezone);

                //   throw new InvalidPluginExecutionException("workOrderServiceTaskTimeStamp.CreatedON" + workOrderServiceTaskTimeStamp.CreatedON.ToString());
            }
            return workOrderServiceTaskTimeStamp;

        }



        public static void GetSumOfWorkOrderServiceTaskTimeStampRelatedtoWOST(IOrganizationService service, ITracingService tracing, Guid wostID,
            ref int TotalWOSTTimeStampDuration, ref decimal TotalWOSTTimeStampActualAmount, ref int TotalWOSTJourneyManActualTimeSpent)
        {
            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                <entity name='ap360_woservicetasktimestamp'>
                                <attribute name='ap360_woservicetasktimestampid' />
                                <attribute name='ap360_name' />
                                <attribute name='ap360_timespent' />
                                <attribute name='ap360_actualamount' />
                                <attribute name='ap360_journeymantimespent' />
                                <attribute name='createdon' />
                                <order attribute='createdon' descending='false' />
                                <filter type='and'>
                                <condition attribute='ap360_workorderservicetaskid' operator='eq' value='" + wostID + @"' />
                                </filter>
                                </entity>
                                </fetch>");

            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            foreach (Entity entity in col.Entities)
            {
                Money wostTimeStampActualAmount = entity.GetAttributeValue<Money>("ap360_actualamount") ?? null;
                if (wostTimeStampActualAmount != null)
                {
                    TotalWOSTTimeStampActualAmount += wostTimeStampActualAmount.Value;
                }
                TotalWOSTJourneyManActualTimeSpent += entity.GetAttributeValue<int>("ap360_journeymantimespent");
                TotalWOSTTimeStampDuration += entity.GetAttributeValue<int>("ap360_timespent");
            }

        }



        public static List<WorkOrderServiceTask> GetWorkOrderServiceTaskTimeStampCollectionOnUpdate(IOrganizationService service,
            ITracingService tracing, Guid opportunityID, Guid currentWOSTTimeStampGuid, string message)
        {
            string fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
                                    <entity name='msdyn_workorderservicetask'>
                                    <attribute name='createdon' />
                                    <attribute name='msdyn_name' />
                                    <attribute name='msdyn_description' />
                                    <attribute name='msdyn_workorderservicetaskid' />
                                    <attribute name='ap360_estimatedlaborprice' />
                                    <attribute name='ap360_actualamount' />
                                    <attribute name='msdyn_workorder' />

                                    <attribute name='ap360_workorderservicetaskstatus' />
                                     <filter type='and'>
                                      <condition attribute='ap360_opportunityid' operator='eq'  value='" + opportunityID + @"' /> 

                                    </filter>
                                    <link-entity name='ap360_woservicetasktimestamp' from='ap360_workorderservicetaskid' to='msdyn_workorderservicetaskid' link-type='outer' alias='wosttimestamp' >
                                            <attribute name='ap360_woservicetasktimestampid' />
                                            <attribute name='ap360_name' />

                                            <attribute name='ap360_timespent' />
                                            <attribute name='ap360_predictedspend' />
                                            <attribute name='ap360_durationpredictedspend' />

                                            <attribute name='ap360_actualamount' />
                                            <attribute name='ap360_durationdollar' />

                                            <attribute name='ap360_wostdurationhealth' />
                                            <attribute name='ap360_wostposthealth' />
                                            <attribute name='ap360_workorderdurationhealth' />
                                            <attribute name='ap360_workorderposthealth' />
                                            <attribute name='ap360_opportunitydurationhealth' />
                                            <attribute name='ap360_opportunityposthealth' />



                                            <attribute name='createdon' />
                                            <order attribute='createdon' descending='true' />";
            if (message == "update")
            {
                fetchXml += @"<filter type='and'>
                                                     <condition attribute = 'ap360_woservicetasktimestampid' operator= 'ne' value = '" + currentWOSTTimeStampGuid + @"' />     
                                                    </filter> ";
            }
            fetchXml += @"</link-entity>
                                        </entity>
                                   </fetch>";
            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            //  tracing.Trace(fetchXml.ToString());
            tracing.Trace(" Count of records " + col.Entities.Count.ToString());
            //    throw new InvalidPluginExecutionException(" Count of records " + col.Entities.Count.ToString());
            List<WorkOrderServiceTask> lstWOSTs = new List<WorkOrderServiceTask>();

            WorkOrderServiceTask workOrderServiceTask = null;
            int loopCount = 0;
            foreach (Entity entity in col.Entities)
            {
                //tracing.Trace("inside loop " + loopCount.ToString());
                loopCount++;
                workOrderServiceTask = new WorkOrderServiceTask();
                // workOrderServiceTask.WOSTTimeStamps = new WorkOrderServiceTaskTimeStamp();
                workOrderServiceTask.WOSTGuid = entity.Id;
                workOrderServiceTask.WOSTLaborAmount = entity.GetAttributeValue<Money>("ap360_estimatedlaborprice") != null ? entity.GetAttributeValue<Money>("ap360_estimatedlaborprice").Value : 0;
                workOrderServiceTask.WOSTStatus = entity.GetAttributeValue<OptionSetValue>("ap360_workorderservicetaskstatus") != null ? entity.GetAttributeValue<OptionSetValue>("ap360_workorderservicetaskstatus").Value : 0;
                workOrderServiceTask.WorkOrder = entity.GetAttributeValue<EntityReference>("msdyn_workorder") != null ? entity.GetAttributeValue<EntityReference>("msdyn_workorder") : null;
                //workOrderServiceTask.WOSTTimeStamps.WOSTTimeStampPredictedSpend = entity.FormattedValues["wosttimestamp.ap360_predictedspend"];


                string strPredictedSpent = entity.FormattedValues.ContainsKey("wosttimestamp.ap360_predictedspend") ? entity.FormattedValues["wosttimestamp.ap360_predictedspend"] : null;
                if (strPredictedSpent != null)
                {
                    workOrderServiceTask.WOSTTimeStamps = new WorkOrderServiceTaskTimeStamp();

                    strPredictedSpent = strPredictedSpent.Remove(0, 1);
                    decimal predictedSpend = Convert.ToDecimal(strPredictedSpent);
                    workOrderServiceTask.WOSTTimeStamps.TotalWOSTTimeStampPredictedSpendAmount = predictedSpend;
                    workOrderServiceTask.WOSTTimeStamps.Name = entity.GetAttributeValue<AliasedValue>("wosttimestamp.ap360_name").Value.ToString();
                    workOrderServiceTask.WOSTTimeStamps.guid = new Guid(entity.GetAttributeValue<AliasedValue>("wosttimestamp.ap360_woservicetasktimestampid").Value.ToString());

                    string durationPredictedSpend = entity.FormattedValues.ContainsKey("wosttimestamp.ap360_durationpredictedspend") ? entity.FormattedValues["wosttimestamp.ap360_durationpredictedspend"] : null;
                    if (durationPredictedSpend != null)
                    {
                        durationPredictedSpend = durationPredictedSpend.Remove(0, 1);
                        decimal durationPredictedSpendDec = Convert.ToDecimal(durationPredictedSpend);
                        workOrderServiceTask.WOSTTimeStamps.TotalWOSTTimeStampDurationPredictedSpendAmount = durationPredictedSpendDec;
                    }

                    string actualAmount = entity.FormattedValues.ContainsKey("wosttimestamp.ap360_actualamount") ? entity.FormattedValues["wosttimestamp.ap360_actualamount"] : null;
                    if (actualAmount != null)
                    {
                        actualAmount = actualAmount.Remove(0, 1);
                        decimal acutalAmountDec = Convert.ToDecimal(actualAmount);
                        workOrderServiceTask.WOSTTimeStamps.WOSTTimeStampActualAmount = acutalAmountDec;
                    }

                    string durationDollar = entity.FormattedValues.ContainsKey("wosttimestamp.ap360_durationdollar") ? entity.FormattedValues["wosttimestamp.ap360_durationdollar"] : null;
                    if (durationDollar != null)
                    {
                        durationDollar = durationDollar.Remove(0, 1);
                        decimal durationAmountDec = Convert.ToDecimal(durationDollar);
                        workOrderServiceTask.WOSTTimeStamps.WOSTTimeStampDurationDollar = durationAmountDec;
                    }

                    string strCreatedOn = entity.FormattedValues.ContainsKey("wosttimestamp.createdon") ? entity.FormattedValues["wosttimestamp.createdon"] : null;

                    if (strCreatedOn != null)
                    {
                        DateTime tsCreatedOn = Convert.ToDateTime(strCreatedOn);
                        int? currentusertimezone = Methods.RetrieveCurrentUsersTimeZoneSettings(service);
                        tsCreatedOn = (DateTime)Methods.RetrieveLocalTimeFromUTCTime(service, tsCreatedOn, currentusertimezone);

                        workOrderServiceTask.WOSTTimeStamps.CreatedON = tsCreatedOn;
                    }
                    //////////////////////////////////////////////
                    string wostdurationhealth = entity.FormattedValues.ContainsKey("wosttimestamp.ap360_wostdurationhealth") ? entity.FormattedValues["wosttimestamp.ap360_wostdurationhealth"] : null;
                    if (wostdurationhealth != null)
                    {
                        decimal wostdurationhealthDec = Convert.ToDecimal(wostdurationhealth);
                        workOrderServiceTask.WOSTTimeStamps.wostDurationHealth = wostdurationhealthDec;
                    }
                    string wostposthealth = entity.FormattedValues.ContainsKey("wosttimestamp.ap360_wostposthealth") ? entity.FormattedValues["wosttimestamp.ap360_wostposthealth"] : null;
                    if (wostposthealth != null)
                    {
                        decimal wostposthealthDec = Convert.ToDecimal(wostposthealth);
                        workOrderServiceTask.WOSTTimeStamps.wostPOSTHealth = wostposthealthDec;
                    }
                    string workorderdurationhealth = entity.FormattedValues.ContainsKey("wosttimestamp.ap360_workorderdurationhealth") ? entity.FormattedValues["wosttimestamp.ap360_workorderdurationhealth"] : null;
                    if (workorderdurationhealth != null)
                    {
                        decimal workorderdurationhealthDec = Convert.ToDecimal(workorderdurationhealth);
                        workOrderServiceTask.WOSTTimeStamps.workOrderDurationHealth = workorderdurationhealthDec;
                    }
                    string ap360_workorderposthealth = entity.FormattedValues.ContainsKey("wosttimestamp.ap360_workorderposthealth") ? entity.FormattedValues["wosttimestamp.ap360_workorderposthealth"] : null;
                    if (ap360_workorderposthealth != null)
                    {
                        decimal ap360_workorderposthealthDec = Convert.ToDecimal(ap360_workorderposthealth);
                        workOrderServiceTask.WOSTTimeStamps.workOrderPOSTHealth = ap360_workorderposthealthDec;
                    }
                    string opportunitydurationhealth = entity.FormattedValues.ContainsKey("wosttimestamp.ap360_opportunitydurationhealth") ? entity.FormattedValues["wosttimestamp.ap360_opportunitydurationhealth"] : null;
                    if (opportunitydurationhealth != null)
                    {
                        decimal opportunitydurationhealthDec = Convert.ToDecimal(opportunitydurationhealth);
                        workOrderServiceTask.WOSTTimeStamps.opportunityDurationHealth = opportunitydurationhealthDec;
                    }
                    string ap360_opportunityposthealth = entity.FormattedValues.ContainsKey("wosttimestamp.ap360_opportunityposthealth") ? entity.FormattedValues["wosttimestamp.ap360_opportunityposthealth"] : null;
                    if (ap360_opportunityposthealth != null)
                    {
                        decimal ap360_opportunityposthealthDec = Convert.ToDecimal(ap360_opportunityposthealth);
                        workOrderServiceTask.WOSTTimeStamps.opportunityPOSTHealth = ap360_opportunityposthealthDec;
                    }
                }
                lstWOSTs.Add(workOrderServiceTask);



            }

            // var groupedWOST = lstWOSTs.GroupBy(x => x.WOSTGuid);

            return lstWOSTs;
        }

        public static EntityCollection GetWorkOrderServiceTaskTimeStampCollectionOnCreate(IOrganizationService service,
          ITracingService tracing, Guid opportunityID, Guid currentWOSTTimeStampGuid)
        {
            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
                                    <entity name='msdyn_workorderservicetask'>
                                    <attribute name='createdon' />
                                    <attribute name='msdyn_name' />
                                    <attribute name='msdyn_description' />
                                    <attribute name='msdyn_workorderservicetaskid' />
                                    <attribute name='ap360_estimatedlaborprice' />
                                    <attribute name='ap360_actualamount' />
                                    <attribute name='msdyn_workorder' />

                                    <attribute name='ap360_workorderservicetaskstatus' />
                                     <filter type='and'>
                                      <condition attribute='ap360_opportunityid' operator='eq'  value='" + opportunityID + @"' /> 

                                    </filter>
                                    <link-entity name='ap360_woservicetasktimestamp' from='ap360_workorderservicetaskid' to='msdyn_workorderservicetaskid' link-type='outer' alias='wosttimestamp' >
                                            <attribute name='ap360_woservicetasktimestampid' />
                                            <attribute name='ap360_timespent' />
                                            <attribute name='ap360_predictedspend' />
                                            <attribute name='ap360_actualamount' />

                                            <attribute name='createdon' />
                                            <order attribute='createdon' descending='true' />
                                    </link-entity>
                                    </entity>
                                    </fetch>");
            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            return col;

        }

        public static void getWOSTActualAmount(string message, ITracingService tracingService, List<WorkOrderServiceTask> lstWOSTs,
            ref decimal WOSTTimeStampActualAmount, Guid wostID, DateTime createdOn)
        {
            //DateTime tempDate = new DateTime(0,0,0)
            var groupedWOST = lstWOSTs.GroupBy(x => x.WOSTGuid);
            int outerloopCount = 0;
            //foreach (var eachWOSTInGroup in groupedWOST)
            //{
            //    lstWOSTs = eachWOSTInGroup.ToList();

            foreach (WorkOrderServiceTask eachWOSTAndWOSTTimeStamp in lstWOSTs)
            {
                int count = 0;

                if (lstWOSTs[count].WOSTGuid != null)
                {
                    if (lstWOSTs[count].WOSTGuid.ToString() == wostID.ToString())
                    {
                        tracingService.Trace("WOST Guid match " + wostID.ToString());
                        if (lstWOSTs[count].WOSTTimeStamps != null)
                        {
                            if (message == "update")
                            {
                                if (lstWOSTs[count].WOSTTimeStamps.CreatedON < createdOn)
                                {
                                    WOSTTimeStampActualAmount += lstWOSTs[count].WOSTTimeStamps.WOSTTimeStampActualAmount;
                                    tracingService.Trace("current tS created On " + createdOn.ToString());
                                    tracingService.Trace("loop created On " + lstWOSTs[count].WOSTTimeStamps.CreatedON.ToString());

                                }
                            }
                            else
                            {
                                WOSTTimeStampActualAmount += lstWOSTs[count].WOSTTimeStamps.WOSTTimeStampActualAmount;
                            }
                        }
                    }

                }

                count++;
            }
            //     throw new InvalidPluginExecutionException("error");

            outerloopCount++;
            // }
            tracingService.Trace("End");
        }
        public static void newGetSumOfWorkOrderServiceTaskTimeStampRelatedtoOpportunityExceptCurrentWOSTTimeStamp(List<WorkOrderServiceTask> lstWOSTs, IOrganizationService service,
            ITracingService tracing, Guid opportunityID, Guid workorderID, Guid wostID,
           ref decimal opportunityTimeStampActualAmount, ref decimal workOrderTimeStampActualAmount,
           ref decimal opportunityTimeStampPredictedAmount, ref decimal workOrderTimeStampPredictedAmount, ref decimal WOSTTimeStampPredictedAmount,
           ref decimal opportunityTimeStampDurationPredictedAmount, ref decimal workOrderTimeStampDurationPredictedAmount,
           ref decimal WOSTTimeStampDurationPredictedAmount,

              ref decimal opportunityLabourEstimatedAmount, ref decimal workOrderLabourEstimatedAmount, ref decimal wostLabourEstimatedAmount,
           Guid currentWOSTTimeStampGuid, Money currentpredictedspend, Money currentDurationPredictedspend, string message)
        {
            var groupedWOST = lstWOSTs.GroupBy(x => x.WOSTGuid);

            foreach (var eachWOSTInGroup in groupedWOST)
            {
                lstWOSTs = eachWOSTInGroup.ToList();
                opportunityLabourEstimatedAmount += lstWOSTs[0].WOSTLaborAmount;
                if (lstWOSTs[0].WOSTTimeStamps == null)
                {
                    if (lstWOSTs[0].WOSTGuid.ToString() == wostID.ToString())
                    {
                        if (currentpredictedspend != null)
                            opportunityTimeStampPredictedAmount += currentpredictedspend.Value;
                        if (currentDurationPredictedspend != null)
                            opportunityTimeStampDurationPredictedAmount += currentDurationPredictedspend.Value;

                    }
                    else
                    {
                        opportunityTimeStampPredictedAmount += lstWOSTs[0].WOSTLaborAmount;
                        opportunityTimeStampDurationPredictedAmount += lstWOSTs[0].WOSTLaborAmount;
                    }

                }
                else
                {
                    if (lstWOSTs[0].WOSTGuid.ToString() == wostID.ToString())
                    {
                        if (currentpredictedspend != null)
                            opportunityTimeStampPredictedAmount += currentpredictedspend.Value;
                        if (currentDurationPredictedspend != null)
                            opportunityTimeStampDurationPredictedAmount += currentDurationPredictedspend.Value;
                    }
                    else
                    {
                        opportunityTimeStampPredictedAmount += lstWOSTs[0].WOSTTimeStamps.TotalWOSTTimeStampPredictedSpendAmount;
                        opportunityTimeStampDurationPredictedAmount += lstWOSTs[0].WOSTTimeStamps.TotalWOSTTimeStampDurationPredictedSpendAmount;

                    }
                }


                if (lstWOSTs[0].WorkOrder != null)
                {
                    if (lstWOSTs[0].WorkOrder.Id.ToString() == workorderID.ToString())
                    {
                        workOrderLabourEstimatedAmount += lstWOSTs[0].WOSTLaborAmount;
                        if (lstWOSTs[0].WOSTTimeStamps == null)
                        {
                            if (lstWOSTs[0].WOSTGuid.ToString() == wostID.ToString())
                            {
                                if (currentpredictedspend != null)
                                    workOrderTimeStampPredictedAmount += currentpredictedspend.Value;
                                if (currentDurationPredictedspend != null)
                                    workOrderTimeStampDurationPredictedAmount += currentDurationPredictedspend.Value;
                            }
                            else
                            {
                                workOrderTimeStampPredictedAmount += lstWOSTs[0].WOSTLaborAmount;
                                workOrderTimeStampDurationPredictedAmount += lstWOSTs[0].WOSTLaborAmount;
                            }
                        }
                        else
                        {
                            if (lstWOSTs[0].WOSTGuid.ToString() == wostID.ToString())
                            {
                                if (currentpredictedspend != null)
                                    workOrderTimeStampPredictedAmount += currentpredictedspend.Value;
                                if (currentDurationPredictedspend != null)
                                    workOrderTimeStampDurationPredictedAmount += currentDurationPredictedspend.Value;
                            }
                            else
                            {
                                workOrderTimeStampPredictedAmount += lstWOSTs[0].WOSTTimeStamps.TotalWOSTTimeStampPredictedSpendAmount;
                                workOrderTimeStampDurationPredictedAmount += lstWOSTs[0].WOSTTimeStamps.TotalWOSTTimeStampDurationPredictedSpendAmount;
                            }

                        }
                    }

                }
                if (lstWOSTs[0].WOSTGuid != null)
                {
                    if (lstWOSTs[0].WOSTGuid.ToString() == wostID.ToString())
                    {
                        // wostAcutalAmount += lstWOSTs[0].WOSTTimeStamps.TotalAcutalAmount;
                        wostLabourEstimatedAmount += lstWOSTs[0].WOSTLaborAmount;
                        // if (message == "create")
                        // {
                        //WOSTTimeStampPredictedAmount += lstWOSTs[0].WOSTLaborAmount;
                        if (currentpredictedspend != null)
                            WOSTTimeStampPredictedAmount += currentpredictedspend.Value;
                        if (currentDurationPredictedspend != null)
                            WOSTTimeStampPredictedAmount += currentDurationPredictedspend.Value;
                        //}
                        //else
                        //{
                        //    WOSTTimeStampPredictedAmount += lstWOSTs[0].WOSTTimeStamps.TotalWOSTTimeStampPredictedSpendAmount;

                        //}
                    }

                }


            }

            tracing.Trace("Oppr Est " + opportunityLabourEstimatedAmount.ToString() + " Oppr Pred " + opportunityTimeStampPredictedAmount.ToString());
            tracing.Trace("WO Est " + workOrderLabourEstimatedAmount.ToString() + " WO Pred " + workOrderTimeStampPredictedAmount.ToString());
            tracing.Trace("WOST Est " + wostLabourEstimatedAmount.ToString() + " WOST Pred " + WOSTTimeStampPredictedAmount.ToString());
            tracing.Trace("-------------------------");

            tracing.Trace("Oppr Est " + opportunityLabourEstimatedAmount.ToString() + " Oppr DUR Pred " + opportunityTimeStampDurationPredictedAmount.ToString());
            tracing.Trace("WO Est " + workOrderLabourEstimatedAmount.ToString() + " WO DUR Pred " + workOrderTimeStampDurationPredictedAmount.ToString());
            tracing.Trace("WOST Est " + wostLabourEstimatedAmount.ToString() + " WOST DUR Pred " + WOSTTimeStampPredictedAmount.ToString());
            //  throw new InvalidPluginExecutionException("error ");

        }

        public static Entity GetWorkOrderServiceTaskTimeStampRelatedtoWOST(IOrganizationService service, ITracingService tracing, Guid wostID)
        {


            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='ap360_woservicetasktimestamp'>
                                    <attribute name='ap360_woservicetasktimestampid' />
                                    <attribute name='ap360_name' />
                                    <attribute name='ap360_wostposthealth' />

                                    <attribute name='createdon' />
                                    <order attribute='createdon' descending='false' />
                                    <filter type='and'>
                                      <condition attribute='ap360_workorderservicetaskid' operator='eq'  value='" + wostID + @"' /> 
                                  
                                    </filter>
                                  </entity>
                                </fetch>");



            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            tracing.Trace("WOST Time Stamp " + col.Entities.Count.ToString());
            if (col.Entities.Count > 0)
            {
                return col.Entities[col.Entities.Count - 1];
            }
            return null;
        }

        public static void DeleteWOSTTimeStampRelatedBooking(IOrganizationService service, Guid bookingGuid)
        {


            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                      <entity name='ap360_woservicetasktimestamp'>
                        <attribute name='ap360_woservicetasktimestampid' />
                        <attribute name='ap360_name' />
                        <attribute name='createdon' />
                        <order attribute='ap360_name' descending='false' />
                        <link-entity name='ap360_bookingservicetask' from='ap360_bookingservicetaskid' to='ap360_bookingservicetaskid' link-type='inner' alias='ao'>
                          <link-entity name='bookableresourcebooking' from='bookableresourcebookingid' to='ap360_bookableresourcebooking' link-type='inner' alias='aq'>
                            <filter type='and'>
                              <condition attribute='bookableresourcebookingid' operator='eq'  value='" + bookingGuid + @"' />
                            </filter>
                          </link-entity>
                        </link-entity>
                      </entity>
                    </fetch>");

            // <condition attribute='bookableresourcebookingid' operator='eq'  value='" + bookingGuid + @"' /> 
            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            //throw new InvalidPluginExecutionException("Updated Error "+col.Entities.Count.ToString());
            foreach (Entity entity in col.Entities)
            {
                service.Delete(entity.LogicalName, entity.Id);

            }


        }


        public static void CreateWOSTTimeStamp(IOrganizationService service, ITracingService tracing, Entity BRBEntity, decimal billingPrice, BookingServiceTask bookingServiceTask, decimal timespent, decimal ServiceRoleHourlyRate, decimal ResourceHourlyRate, List<WorkOrderServiceTaskTimeStamp> lstWorKOrderServiceTaskTimeStampsForHealthUpdate)
        {
            tracing.Trace("insdie CreateWOSTTimeStamp ");
            Entity newWOservicetasktimestamp = new Entity("ap360_woservicetasktimestamp");
            if (bookingServiceTask.WorkOrderServiceTask != null)
            {
                newWOservicetasktimestamp["ap360_workorderservicetaskid"] = new EntityReference(bookingServiceTask.WorkOrderServiceTask.LogicalName, bookingServiceTask.WorkOrderServiceTask.Id);
                if (BRBEntity.GetAttributeValue<EntityReference>("ap360_workorderservicetask") != null)
                {
                    if (bookingServiceTask.WorkOrderServiceTask.Id == BRBEntity.GetAttributeValue<EntityReference>("ap360_workorderservicetask").Id)
                    {

                        tracing.Trace(bookingServiceTask.WorkOrderServiceTask.Name + " is a master task for booking");
                        newWOservicetasktimestamp["ap360_tsformasterbookingservicetask"] = true;
                        newWOservicetasktimestamp["ap360_workorderservicetaskstatus"] = BRBEntity.GetAttributeValue<OptionSetValue>("ap360_workorderservicetaskstatus")??null;
                        newWOservicetasktimestamp["ap360_workorderservicetasksubstatus"] = BRBEntity.GetAttributeValue<OptionSetValue>("ap360_workorderservicetasksubstatus")??null;


                    }
                }
            }
            tracing.Trace("before ap360_workorderservicetaskid mapping ");

            if (BRBEntity.GetAttributeValue<EntityReference>("resource") != null)
                newWOservicetasktimestamp["ap360_bookableresourceid"] = new EntityReference("bookableresource", BRBEntity.GetAttributeValue<EntityReference>("resource").Id);
            newWOservicetasktimestamp["ap360_bookableresourcebookingid"] = new EntityReference(BRBEntity.LogicalName, BRBEntity.Id);
            newWOservicetasktimestamp["ap360_workorderid"] = BRBEntity.GetAttributeValue<EntityReference>("msdyn_workorder");
            tracing.Trace("before ap360_opportunityid mapping ");

            newWOservicetasktimestamp["ap360_opportunityid"] = BRBEntity.GetAttributeValue<EntityReference>("ap360_opportunityid");
            tracing.Trace("after ap360_opportunityid mapping ");

            newWOservicetasktimestamp["ap360_bookingservicetaskid"] = new EntityReference(bookingServiceTask.LogicalName, bookingServiceTask.BSTGuid);
            newWOservicetasktimestamp["ap360_wostprehealth"] = BRBEntity.GetAttributeValue<decimal>("ap360_prewosthealth");
            tracing.Trace("before MapWOSTTimeStampFields ");


            WorkOrderServiceTaskTimeStamp workOrderServiceTaskTimeStampForHealthUpdate = null;
            MapWOSTTimeStampFields("create", Guid.Empty, DateTime.Now, service, tracing, BRBEntity, billingPrice, bookingServiceTask, timespent, ServiceRoleHourlyRate,
                ResourceHourlyRate, newWOservicetasktimestamp, lstWorKOrderServiceTaskTimeStampsForHealthUpdate,
                ref workOrderServiceTaskTimeStampForHealthUpdate, 0);
            //throw new InvalidPluginExecutionException("Custome Error");

            Guid newlyCreatedWOServiceTaskTimeStampGuid = service.Create(newWOservicetasktimestamp);
            if (workOrderServiceTaskTimeStampForHealthUpdate != null)
            {
                workOrderServiceTaskTimeStampForHealthUpdate.guid = newlyCreatedWOServiceTaskTimeStampGuid;
                lstWorKOrderServiceTaskTimeStampsForHealthUpdate.Add(workOrderServiceTaskTimeStampForHealthUpdate);

            }

        }
        public static void UpdateWOSTTimeStamp(IOrganizationService service, ITracingService tracing, Entity BRBEntity, decimal billingPrice,
            BookingServiceTask bookingServiceTask, decimal timespent, EntityReference bookingResource, EntityReference bookingServiceRole,
            ref decimal ServiceRoleHourlyRate, ref decimal ResourceHourlyRate, WorkOrderServiceTaskTimeStamp workOrderServiceTaskTimeStamp,
            List<WorkOrderServiceTaskTimeStamp> lstWorKOrderServiceTaskTimeStampsForHealthUpdate, int bstloopCount)
        {
            Entity updateWOservicetasktimestamp = new Entity("ap360_woservicetasktimestamp");

            Methods.getResourseAndServiceRoleRates(service, tracing, bookingServiceRole, bookingResource, ref ServiceRoleHourlyRate, ref ResourceHourlyRate, BRBEntity);

            WorkOrderServiceTaskTimeStamp workOrderServiceTaskTimeStampForHealthUpdate = null;

            MapWOSTTimeStampFields("update", workOrderServiceTaskTimeStamp.guid, workOrderServiceTaskTimeStamp.CreatedON, service, tracing, BRBEntity, billingPrice, bookingServiceTask, timespent,
                ServiceRoleHourlyRate, ResourceHourlyRate, updateWOservicetasktimestamp,
                lstWorKOrderServiceTaskTimeStampsForHealthUpdate, ref workOrderServiceTaskTimeStampForHealthUpdate, bstloopCount);

            updateWOservicetasktimestamp.Id = workOrderServiceTaskTimeStamp.guid;

            service.Update(updateWOservicetasktimestamp);

            if (workOrderServiceTaskTimeStampForHealthUpdate != null)
            {
                workOrderServiceTaskTimeStampForHealthUpdate.guid = workOrderServiceTaskTimeStamp.guid;
                // lstWorKOrderServiceTaskTimeStampsForHealthUpdate.Add(workOrderServiceTaskTimeStampForHealthUpdate);
            }

            //    throw new InvalidPluginExecutionException("updated Custom error "+ workOrderServiceTaskTimeStamp.guid.ToString());
        }

        public static void MapWOSTTimeStampFields(string message, Guid existingWOSTStampGuid, DateTime existingWOSTStampCreatedOnDate,
            IOrganizationService service, ITracingService tracing, Entity BRBEntity,
            decimal billingPrice, BookingServiceTask bookingServiceTask, decimal timespent, decimal ServiceRoleHourlyRate,
            decimal ResourceHourlyRate, Entity WOservicetasktimestamp, List<WorkOrderServiceTaskTimeStamp> lstWorKOrderServiceTaskTimeStampsForHealthUpdate,
           ref WorkOrderServiceTaskTimeStamp workOrderServiceTaskTimeStampForHealthUpdate, int bstloopCount)
        {
            tracing.Trace("***************************}}}}}******** *************************** ");
            decimal wostLabourEstimatedAmount = 0;
            decimal opportunityLabourEstimatedAmount = 0;
            decimal workOrderLabourEstimatedAmount = 0;

            WOservicetasktimestamp["ap360_name"] = "Time Spent  " + timespent;
            decimal durationDollar = new Money(((decimal)timespent / 60) * (1 * ResourceHourlyRate)).Value;
            WOservicetasktimestamp["ap360_durationdollar"] = new Money(durationDollar);
            WOservicetasktimestamp["ap360_timespent"] = timespent;

            if (bookingServiceTask.WorkOrderServiceTask == null) return;
            Guid wOSTEntityID = bookingServiceTask.WorkOrderServiceTask.Id;
            EntityReference opportunityRef = BRBEntity.GetAttributeValue<EntityReference>("ap360_opportunityid") ?? null;
            EntityReference workorderRef = BRBEntity.GetAttributeValue<EntityReference>("msdyn_workorder") ?? null;
            if (BRBEntity.GetAttributeValue<EntityReference>("ap360_bookingclassification") != null)
                WOservicetasktimestamp["ap360_bookingclassificationid"] = new EntityReference(BRBEntity.GetAttributeValue<EntityReference>("ap360_bookingclassification").LogicalName, BRBEntity.GetAttributeValue<EntityReference>("ap360_bookingclassification").Id);

            if (opportunityRef == null || workorderRef == null)
                return;

            tracing.Trace("before GetWOSTrelatedToOpportunity ");



            if (BRBEntity.GetAttributeValue<OptionSetValue>("ap360_extratimerequiredreasons") != null)
                WOservicetasktimestamp["ap360_needsmoretime"] = BRBEntity.FormattedValues["ap360_extratimerequiredreasons"].ToString();


            WOservicetasktimestamp["ap360_extratimerequireddescription"] = BRBEntity.GetAttributeValue<string>("ap360_extratimerequireddescription");
            WOservicetasktimestamp["ap360_extratimerequired"] = BRBEntity.GetAttributeValue<int>("ap360_extratimerequired");
            WOservicetasktimestamp["ap360_startingpercentcomplete"] = bookingServiceTask.StartingPercentCompleted;

            DateTime timestampneedmoretime = BRBEntity.GetAttributeValue<DateTime>("ap360_timestampneedmoretime");
            if (timestampneedmoretime != DateTime.MinValue)
                WOservicetasktimestamp["ap360_timestampneedmoretime"] = BRBEntity.GetAttributeValue<DateTime>("ap360_timestampneedmoretime");



            WOservicetasktimestamp["ap360_journeymantimespent"] = ((ResourceHourlyRate / ServiceRoleHourlyRate) * timespent);
          

            //tracing.Trace("ap360_journeymantimespent"+ ((ResourceHourlyRate / ServiceRoleHourlyRate) * timespent).ToString());
            //decimal timeSpentOnWOSTAsJourneyMan = ((ResourceHourlyRate / ServiceRoleHourlyRate) * timespent);
            // decimal billingPrice = bookableResourceBooking.updateWorkOrderTotalActualLaborAmount(service, tracing, BRBEntity.Id, "ap360_timespentonbooking");
            //    throw new InvalidPluginExecutionException("test "+ billingPrice.ToString() +"---"+ bookingServiceTask.PercentTimeSpent.ToString());
            WOservicetasktimestamp["ap360_actualamount"] = new Money((billingPrice / 100) * (int)bookingServiceTask.PercentTimeSpent);


            decimal currentActualAmount = (billingPrice / 100) * (int)bookingServiceTask.PercentTimeSpent;
            tracing.Trace("currentActualAmount is " + currentActualAmount.ToString());
            if (bookingServiceTask.EndingTaskPercentCompleted == null)
                return;
            WOservicetasktimestamp["ap360_endingpercentcompleted"] = bookingServiceTask.EndingTaskPercentCompleted;


            tracing.Trace("After at  " + currentActualAmount.ToString());
            decimal WOSTTimeStampActualAmount = 0;
            decimal OpportunityTimeStampActualAmount = 0;
            decimal workOrderTimeStampActualAmount = 0;

            decimal opportunityTimeStampPredictedAmount = 0;
            decimal workOrderTimeStampPredictedAmount = 0;
            decimal WOSTTimeStampPredictedAmount = 0;

            decimal opportunityTimeStampDurationPredictedAmount = 0;
            decimal workOrderTimeStampDurationPredictedAmount = 0;
            decimal WOSTTimeStampDurationPredictedAmount = 0;


            int WOSTpercentcomplete = BookingServiceTask.getBSTWOSTPercentComplete(bookingServiceTask.EndingTaskPercentCompleted.Value);
            Money currentpredictedspend = new Money();
            Money currentDurationPredictedspend = new Money();

            if (message == "create")
            {
                List<WorkOrderServiceTask> lstWorkOrderServiceTask = WorkOrderServiceTaskTimeStamp.GetWorkOrderServiceTaskTimeStampCollectionOnUpdate(service,
                    tracing, opportunityRef.Id, existingWOSTStampGuid, "create");
                tracing.Trace("After GetWorkOrderServiceTaskTimeStampCollectionOnUpdate: Create");

                Methods.GetPreHealth(tracing, lstWorkOrderServiceTask, bookingServiceTask.WorkOrderServiceTask, "create", WOservicetasktimestamp);

                WorkOrderServiceTaskTimeStamp.getWOSTActualAmount("create", tracing, lstWorkOrderServiceTask, ref WOSTTimeStampActualAmount, wOSTEntityID, existingWOSTStampCreatedOnDate);
                tracing.Trace("********************** WOSTTimeStamp ActualAmount" + WOSTTimeStampActualAmount.ToString() + " CurrentActualAmount " + currentActualAmount.ToString());
                //   WOSTTimeStampActualAmount = WOSTTimeStampActualAmount - currentActualAmount
                currentpredictedspend = new Money(((WOSTTimeStampActualAmount + currentActualAmount) * (100 - WOSTpercentcomplete) / WOSTpercentcomplete)
                    + (WOSTTimeStampActualAmount + currentActualAmount));
                currentDurationPredictedspend = new Money(((WOSTTimeStampActualAmount + durationDollar) * (100 - WOSTpercentcomplete) / WOSTpercentcomplete) +
                    (WOSTTimeStampActualAmount + durationDollar));
                if (bstloopCount > 0)
                    throw new InvalidPluginExecutionException("Current actual " + currentActualAmount + " Total Actual " + WOSTTimeStampActualAmount + " upda " + WOSTpercentcomplete.ToString() + " Prdi sEpent" + currentpredictedspend.Value.ToString());
                if (currentpredictedspend != null)
                    tracing.Trace("********************** Current Predicted Spend " + currentpredictedspend.Value.ToString());
                if (currentDurationPredictedspend != null)
                    tracing.Trace("********************** Current Duration Predicted Spend " + currentDurationPredictedspend.Value.ToString());

                WorkOrderServiceTaskTimeStamp.newGetSumOfWorkOrderServiceTaskTimeStampRelatedtoOpportunityExceptCurrentWOSTTimeStamp(lstWorkOrderServiceTask,
                    service, tracing, opportunityRef.Id, workorderRef.Id, wOSTEntityID,
              ref OpportunityTimeStampActualAmount, ref workOrderTimeStampActualAmount,
              ref opportunityTimeStampPredictedAmount, ref workOrderTimeStampPredictedAmount, ref WOSTTimeStampPredictedAmount,
           ref opportunityTimeStampDurationPredictedAmount, ref workOrderTimeStampDurationPredictedAmount, ref WOSTTimeStampDurationPredictedAmount,

             ref opportunityLabourEstimatedAmount, ref workOrderLabourEstimatedAmount, ref wostLabourEstimatedAmount,
            existingWOSTStampGuid, currentpredictedspend, currentDurationPredictedspend, "create");




            }
            else if (message == "update")
            {
                // throw new InvalidPluginExecutionException("update");
                tracing.Trace("update: before   GetSumOfWorkOrderServiceTaskTimeStampRelatedtoWOSTExceptCurrentWOSTTimeStamp");
                List<WorkOrderServiceTask> lstWorkOrderServiceTask = WorkOrderServiceTaskTimeStamp.GetWorkOrderServiceTaskTimeStampCollectionOnUpdate(service,
                    tracing, opportunityRef.Id, existingWOSTStampGuid, "update");


                WorkOrderServiceTaskTimeStamp.getWOSTActualAmount("update", tracing, lstWorkOrderServiceTask, ref WOSTTimeStampActualAmount, wOSTEntityID, existingWOSTStampCreatedOnDate);
                currentpredictedspend = new Money(((WOSTTimeStampActualAmount + currentActualAmount) * (100 - WOSTpercentcomplete) / WOSTpercentcomplete) +
                    (WOSTTimeStampActualAmount + currentActualAmount));
                //if (bstloopCount > 1)
                //    throw new InvalidPluginExecutionException("Current actual "+currentActualAmount+" Total Actual "+WOSTTimeStampActualAmount + " upda " + WOSTpercentcomplete.ToString() + " Prdi sEpent" + currentpredictedspend.Value.ToString());
                currentDurationPredictedspend = new Money(((WOSTTimeStampActualAmount + durationDollar) * (100 - WOSTpercentcomplete) / WOSTpercentcomplete) +
                (WOSTTimeStampActualAmount + durationDollar));


                WorkOrderServiceTaskTimeStamp.newGetSumOfWorkOrderServiceTaskTimeStampRelatedtoOpportunityExceptCurrentWOSTTimeStamp(lstWorkOrderServiceTask, service,
                    tracing, opportunityRef.Id, workorderRef.Id, wOSTEntityID,
                  ref OpportunityTimeStampActualAmount, ref workOrderTimeStampActualAmount,
                  ref opportunityTimeStampPredictedAmount, ref workOrderTimeStampPredictedAmount, ref WOSTTimeStampPredictedAmount,
                  ref opportunityTimeStampDurationPredictedAmount, ref workOrderTimeStampDurationPredictedAmount, ref WOSTTimeStampDurationPredictedAmount,

                 ref opportunityLabourEstimatedAmount, ref workOrderLabourEstimatedAmount, ref wostLabourEstimatedAmount,
                existingWOSTStampGuid, currentpredictedspend, currentDurationPredictedspend, "update");



                tracing.Trace("update: after   GetSumOfWorkOrderServiceTaskTimeStampRelatedtoWOSTExceptCurrentWOSTTimeStamp");

            }
    


            tracing.Trace("After update of Booking and subtracting current predicted spent ");
            tracing.Trace("opportunityTimeStampPredictedAmount " + opportunityTimeStampPredictedAmount.ToString());
            tracing.Trace("workOrderTimeStampPredictedAmount " + workOrderTimeStampPredictedAmount.ToString());
            tracing.Trace("WOSTTimeStampPredictedAmount " + WOSTTimeStampPredictedAmount.ToString());
            tracing.Trace("*********************************");

            WOservicetasktimestamp["ap360_workorderid"] = BRBEntity.GetAttributeValue<EntityReference>("msdyn_workorder");
            WOservicetasktimestamp["ap360_opportunityid"] = BRBEntity.GetAttributeValue<EntityReference>("ap360_opportunityid");

            decimal prewosthealth = 0;
            prewosthealth = BRBEntity.GetAttributeValue<decimal>("ap360_prewosthealth");
            workOrderServiceTaskTimeStampForHealthUpdate = new WorkOrderServiceTaskTimeStamp();

            if (opportunityTimeStampPredictedAmount > 0 && opportunityLabourEstimatedAmount > 0)
            {
                tracing.Trace("opportunityTimeStampPredictedAmount is " + opportunityTimeStampPredictedAmount.ToString() +
                    "  and opportunityLabourEstimatedAmount is " + opportunityLabourEstimatedAmount.ToString());
                WOservicetasktimestamp["ap360_opportunityposthealth"] = opportunityTimeStampPredictedAmount / opportunityLabourEstimatedAmount;
                WOservicetasktimestamp["ap360_opportunitydurationhealth"] = opportunityTimeStampDurationPredictedAmount / opportunityLabourEstimatedAmount;
                workOrderServiceTaskTimeStampForHealthUpdate.opportunityDurationHealth = opportunityTimeStampDurationPredictedAmount / opportunityLabourEstimatedAmount;

                workOrderServiceTaskTimeStampForHealthUpdate.opportunityPOSTHealth = opportunityTimeStampPredictedAmount / opportunityLabourEstimatedAmount;
            }
            if (workOrderTimeStampPredictedAmount > 0 && workOrderLabourEstimatedAmount > 0)
            {
                tracing.Trace(workOrderTimeStampPredictedAmount.ToString() + " is workOrderTimeStampPredictedAmount  and " + workOrderLabourEstimatedAmount.ToString() + " is workOrderLabourEstimatedAmount >0");
                WOservicetasktimestamp["ap360_workorderposthealth"] = workOrderTimeStampPredictedAmount / workOrderLabourEstimatedAmount;
                WOservicetasktimestamp["ap360_workorderdurationhealth"] = workOrderTimeStampDurationPredictedAmount / workOrderLabourEstimatedAmount;

                workOrderServiceTaskTimeStampForHealthUpdate.workOrderDurationHealth = workOrderTimeStampDurationPredictedAmount / workOrderLabourEstimatedAmount;

                workOrderServiceTaskTimeStampForHealthUpdate.workOrderPOSTHealth = workOrderTimeStampPredictedAmount / workOrderLabourEstimatedAmount;

                //   tracing.Trace(WOSTTimeStampPrekddictedAmount.ToString() + "is WOSTTimeStampPredictedAmount " + wostLabourEstimatedAmount.ToString() + " is wostLabourEstimatedAmount");
            }
            if (currentpredictedspend != null)
            {
                //    WOservicetasktimestamp["ap360_predictedspend"] = new Money(WOSTTimeStampPredictedAmount / wostLabourEstimatedAmount);

                WOservicetasktimestamp["ap360_predictedspend"] = currentpredictedspend;

                tracing.Trace("currentpredictedspend " + currentpredictedspend.Value.ToString());
                workOrderServiceTaskTimeStampForHealthUpdate.PredictedSpend = currentpredictedspend.Value;
            }
            if (currentDurationPredictedspend != null)
            {
                //    WOservicetasktimestamp["ap360_predictedspend"] = new Money(WOSTTimeStampPredictedAmount / wostLabourEstimatedAmount);

                WOservicetasktimestamp["ap360_durationpredictedspend"] = currentDurationPredictedspend;

                tracing.Trace("currentDurationPredictedspend " + currentDurationPredictedspend.Value.ToString());
                //workOrderServiceTaskTimeStampForHealthUpdate.PredictedSpend = currentDurationPredictedspend.Value;
            }
            if (wostLabourEstimatedAmount != 0 && currentpredictedspend != null)
            {
                //throw new InvalidPluginExecutionException(currentpredictedspend.Value.ToString() + " "+ wostLabourEstimatedAmount.ToString());
                WOservicetasktimestamp["ap360_wostposthealth"] = currentpredictedspend.Value / wostLabourEstimatedAmount;
                WOservicetasktimestamp["ap360_wostdurationhealth"] = currentDurationPredictedspend.Value / wostLabourEstimatedAmount;

                workOrderServiceTaskTimeStampForHealthUpdate.wostPOSTHealth = currentpredictedspend.Value / wostLabourEstimatedAmount;

            }
            if (wostLabourEstimatedAmount != 0 && bookingServiceTask.PercentTimeSpent != 0)
            {
                // WOSTpercentcomplete
                double percentWorked = Convert.ToDouble(WOSTpercentcomplete) - bookingServiceTask.StartingPercentCompleted;
                decimal currentBookingPercent = (Convert.ToDecimal(percentWorked) / 100);
                decimal currentBSTAmount = wostLabourEstimatedAmount * currentBookingPercent;
                if (percentWorked > 0 && currentBSTAmount != 0.0m)
                {
                    WOservicetasktimestamp["ap360_woservicetasktimestamphealth"] = currentActualAmount / currentBSTAmount;
                }
                else if (percentWorked < 0)
                {
                    //throw new InvalidPluginExecutionException("Custom");
                    if (wostLabourEstimatedAmount > 0)
                    {
                        var currentBookingAmountPercent = currentActualAmount / wostLabourEstimatedAmount;
                        decimal NegatepercentWorked = decimal.Negate(Convert.ToDecimal(percentWorked));
                        var percentWorkedStampedByTechnicain = NegatepercentWorked / 100m;
                        var totalBookingAmountWork = Decimal.Add(currentBookingAmountPercent, percentWorkedStampedByTechnicain);
                        decimal amountSpentAndBurntOnCurrentBooking = wostLabourEstimatedAmount * totalBookingAmountWork;
                        decimal finalresult = amountSpentAndBurntOnCurrentBooking / wostLabourEstimatedAmount;
                        WOservicetasktimestamp["ap360_woservicetasktimestamphealth"] = 1 + finalresult;
                    }
                }
                else if (percentWorked == 0)
                {
                    if (wostLabourEstimatedAmount > 0)
                    {
                        WOservicetasktimestamp["ap360_woservicetasktimestamphealth"] = 1 + (currentActualAmount / wostLabourEstimatedAmount);
                    }
                }


                //  throw new InvalidPluginExecutionException("Error ");
                WOservicetasktimestamp["ap360_bookingstarttime"] = BRBEntity.GetAttributeValue<DateTime>("starttime");
                WOservicetasktimestamp["ap360_bookingendtime"] = BRBEntity.GetAttributeValue<DateTime>("endtime");

                lstWorKOrderServiceTaskTimeStampsForHealthUpdate.Add(workOrderServiceTaskTimeStampForHealthUpdate);
                //throw new InvalidPluginExecutionException("error");
            }


        }
    }
}
