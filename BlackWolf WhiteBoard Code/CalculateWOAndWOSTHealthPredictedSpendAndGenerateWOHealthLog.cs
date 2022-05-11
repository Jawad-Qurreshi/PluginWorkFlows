using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;



namespace BlackWolf_WhiteBoard_Code
{
    public class CalculateWOAndWOSTHealthPredictedSpendAndGenerateWOHealthLog : IPlugin
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
                       // throw new InvalidPluginExecutionException("this is in context "+context.Depth.ToString());
                        if (entity.LogicalName == "msdyn_workorderservicetask")
                        {

                            if (entity.Contains("ap360_actualamount") || entity.Contains("msdyn_actualduration"))
                            {

                             //test
                                tracingService.Trace("actual amount of wost updated");
                                Entity postImage = (Entity)context.PostEntityImages["Image"];
                                Entity preImage = (Entity)context.PreEntityImages["Image"];

                                Money DollarPredictedspend = new Money();
                                Decimal DurationPredictedspend = 0;

                                Money ap360_originalestimatedamount = new Money();
                                Money ap360_revisedestimatedamount = new Money();
                                Money EstimatedLaborAmount = new Money();

                                int msdyn_estimatedduration = 0;// original esimated duration
                                int ap360_revisedestimatedduration = 0;
                                int EstimatedDuration = 0;

                                string WOSTName = null;
                                if (postImage.Contains("msdyn_name"))
                                {
                                    WOSTName = postImage.GetAttributeValue<string>("msdyn_name");
                                    tracingService.Trace(WOSTName);
                                }

                                //EntityReference recentlyWorkedBooking = postImage.GetAttributeValue<EntityReference>("ap360_recentlyworkedbookingid") != null ? postImage.GetAttributeValue<EntityReference>("ap360_recentlyworkedbookingid") : null;
                                EntityReference recentlyWorkedResource = postImage.GetAttributeValue<EntityReference>("ap360_recentlyworkedresourceid") != null ? postImage.GetAttributeValue<EntityReference>("ap360_recentlyworkedresourceid") : null;
                                tracingService.Trace("recentlyWorkedResource");
                                EntityReference workOrderRef = postImage.GetAttributeValue<EntityReference>("msdyn_workorder");
                                tracingService.Trace("workOrderRef");
                                int WOSTpercentcomplete = Convert.ToInt32(postImage["msdyn_percentcomplete"]);

                                Money postActualAmount = postImage.GetAttributeValue<Money>("ap360_actualamount") != null ? postImage.GetAttributeValue<Money>("ap360_actualamount") : null;
                                Money preActualAmount = preImage.GetAttributeValue<Money>("ap360_actualamount") != null ? preImage.GetAttributeValue<Money>("ap360_actualamount") : null;



                                tracingService.Trace("getting  values");
                                // throw new InvalidPluginExecutionException(actualAmount.Value.ToString() + "post and pre amount " + preActualAmount.Value.ToString());
                                //////////////////////////////*Newly added fields/////////////////
                                EntityReference WOSTServiceRole = postImage.GetAttributeValue<EntityReference>("ap360_serviceroleid") != null ? postImage.GetAttributeValue<EntityReference>("ap360_serviceroleid") : null;
                                Money WOSTHourlyRate = preImage.GetAttributeValue<Money>("ap360_hourlyrate") != null ? postImage.GetAttributeValue<Money>("ap360_hourlyrate") : null;
                                tracingService.Trace("WOSTHourlyRate");


                                int preActualduration = preImage.GetAttributeValue<int>("msdyn_actualduration");
                                int postActualduration = postImage.GetAttributeValue<int>("msdyn_actualduration");
                                tracingService.Trace("postActualduration");

                                int preJourneyManActualDuration = preImage.GetAttributeValue<int>("ap360_journeymanactualduration");
                                int postJourneyManActualDuration = postImage.GetAttributeValue<int>("ap360_journeymanactualduration");
                                decimal timeSpentOnLastBookingAsJourneyMan = postJourneyManActualDuration - preJourneyManActualDuration;
                                tracingService.Trace("timeSpentOnLastBookingAsJourneyMan");

                                decimal timeSpentOnLastBooking = postActualduration - preActualduration;
                                tracingService.Trace("timeSpentOnLastBooking " + timeSpentOnLastBooking.ToString());

                                //if (postActualAmount != null && preActualAmount != null)
                                //{
                                //    decimal moneySpentOnLastBooking = postActualAmount.Value - preActualAmount.Value;
                                //    tracingService.Trace("After getting all values");
                                //}

                                //if (preActualAmount != null)
                                //{
                                //    if (postActualAmount.Value == preActualAmount.Value)
                                //    {
                                //        return;
                                //    }
                                //}

                                decimal postwosthealth = 0;
                                if (postImage.Contains("ap360_postwosthealth"))
                                {
                                    postwosthealth = postImage.GetAttributeValue<decimal>("ap360_postwosthealth");
                                    tracingService.Trace("Post Wost Helath " + postwosthealth.ToString());

                                }
                                decimal durationPostwosthealth = 0;
                                if (postImage.Contains("ap360_durationpostwosthealth"))
                                {
                                    durationPostwosthealth = postImage.GetAttributeValue<decimal>("ap360_durationpostwosthealth");
                                    tracingService.Trace("Duration Post Wost Helath " + durationPostwosthealth.ToString());

                                }

                                //if (entity.Contains("ap360_journeymanactualduration"))
                                //{

                                //    if (postImage.Contains("ap360_lastbookingworkedonid"))
                                //    {
                                //        tracingService.Trace("Last Booking Worked On Exist ");
                                //        int estimatedDuration = postImage.GetAttributeValue<int>("msdyn_estimatedduration") != 0 ? postImage.GetAttributeValue<int>("msdyn_estimatedduration") : postImage.GetAttributeValue<int>("ap360_revisedestimatedduration");
                                //        if (estimatedDuration > postJourneyManActualDuration)
                                //        {
                                //            EntityReference bookableResourceBookingRef = postImage.GetAttributeValue<EntityReference>("ap360_lastbookingworkedonid") ?? null;
                                //            if (bookableResourceBookingRef != null)
                                //            {
                                //                tracingService.Trace(bookableResourceBookingRef.Name + " ref");
                                //                OptionSetValue wostStatus = postImage.GetAttributeValue<OptionSetValue>("ap360_workorderservicetaskstatus");
                                //                if (wostStatus != null)
                                //                {
                                //                    tracingService.Trace("wost status is not null");
                                //                    if (wostStatus.Value == 126300008)//Incompete Return
                                //                    {
                                //                        Entity reterivedBRB = service.Retrieve(bookableResourceBookingRef.LogicalName, bookableResourceBookingRef.Id, new ColumnSet(true));
                                //                        if (reterivedBRB != null)
                                //                        {
                                //                            tracingService.Trace("woststatus 126300008(incomplete return) CreateNewBRBonODone");
                                //                            BookableResourceBooking.CreateNewBRBonODone(service, tracingService, reterivedBRB);
                                //                        }
                                //                    }
                                //                }

                                //            }


                                //        }

                                //    }

                                //}

                                tracingService.Trace("wost percent complete is " + WOSTpercentcomplete.ToString());
                                if (postActualAmount != null)
                                {

                                    //    /////////////////////////////////////////////////////
                                    //   decimal ResourceHourlyRate = 0.0M;
                                    //    decimal ResourceMinuteRate = 0.0M;

                                    decimal ServiceRoleHourlyRate = 0.0M;
                                    decimal ServiceRolePerMinuteRate = 0;

                                    if (WOSTServiceRole == null)
                                    {
                                        tracingService.Trace("WOST Service Role is Null");

                                        // WOSTServiceRole = new EntityReference("bookableresourcecategory", new Guid("108c5d2f-245e-ea11-a812-000d3a30f257"));
                                        // ResourceHourlyRate = Methods.GetResourcePriceBasedOnBRC(service, tracingService, recentlyWorkedResource.Id, "Mechanical Technician");
                                        //   tracingService.Trace("After GetResourcePriceBasedOnBRC ");
                                        ServiceRoleHourlyRate = Methods.getBookAbleResourceCategoryStandardPrice(service, "Mechanical Technician");

                                    }
                                    else
                                    {

                                        //  tracingService.Trace("WOST Service Role Name " + WOSTServiceRole.Name);
                                        // ResourceHourlyRate = Methods.GetResourcePriceBasedOnBRC(service, tracingService, recentlyWorkedResource.Id, WOSTServiceRole.Name);
                                        // tracingService.Trace("After GetResourcePriceBasedOnBRC ");

                                        ServiceRoleHourlyRate = Methods.getBookAbleResourceCategoryStandardPrice(service, WOSTServiceRole.Name);
                                    }

                                    ServiceRolePerMinuteRate = ServiceRoleHourlyRate / 60;
                                    //    ResourceMinuteRate = ResourceHourlyRate / 60;

                                    //    decimal timeSpentOnLastBookingAsJourneyMan = 0;
                                    //    // (((120 / 60) * 155) / 195) * 60
                                    //    //timeSpentOnLastBookingAsJourneyMan = (((timeSpentOnLastBooking / 60) * ServiceRoleHourlyRate) / ResourceHourlyRate) * 60;
                                    //    timeSpentOnLastBookingAsJourneyMan = (( ResourceHourlyRate/ServiceRoleHourlyRate)* timeSpentOnLastBooking);
                                    //   // throw new InvalidPluginExecutionException("(((" + timeSpentOnLastBooking.ToString() + "/60)*" + ServiceRoleHourlyRate.ToString() + ")/" + ResourceHourlyRate.ToString() + ")*60 = " + timeSpentOnLastBookingAsJourneyMan.ToString());

                                    //    decimal timeSpentOnWOSTBeforeLastBooking = preActualAmount.Value / ServiceRolePerMinuteRate;

                                    //    decimal totalTimeSpentOnWOSTAsJourneyMan = timeSpentOnLastBookingAsJourneyMan + timeSpentOnWOSTBeforeLastBooking;

                                    // //   throw new InvalidPluginExecutionException(totalTimeSpentOnWOSTAsJourneyMan.ToString());


                                    //    //throw new InvalidPluginExecutionException(timeSpentOnLastBookingAsJourneyMan.ToString() + "--" + timeSpentOnLastBooking.ToString());

                                    ////////////////////////////////////////////////////////////////////////////
                                    tracingService.Trace("Actual Amount " + postActualAmount.Value.ToString());
                                    EstimatedLaborAmount = WorkOrderServiceTask.getWOSTEstimatedDollarValue(tracingService, postImage, ap360_originalestimatedamount,
                                        ap360_revisedestimatedamount, EstimatedLaborAmount);
                                    tracingService.Trace("Estimated Labor Amount " + EstimatedLaborAmount.Value.ToString());

                                    DollarPredictedspend = WorkOrderServiceTask.UpdateWorkOrderServiceTaskHealthByAmount(tracingService, service, entity.Id,
                                        DollarPredictedspend, EstimatedLaborAmount, WOSTpercentcomplete, postActualAmount, postwosthealth);

                                    ////////////////////////


                                    EstimatedDuration = WorkOrderServiceTask.getWOSTEstimatedDurationValue(tracingService, postImage, msdyn_estimatedduration,
                                    ap360_revisedestimatedduration, EstimatedDuration);
                                    tracingService.Trace("Estimated Labor Amount " + EstimatedDuration.ToString());

                                    DurationPredictedspend = WorkOrderServiceTask.UpdateWorkOrderServiceTaskHealthByDuration(tracingService, service, entity,
                                        DurationPredictedspend, EstimatedDuration, WOSTpercentcomplete, postJourneyManActualDuration, durationPostwosthealth, ServiceRolePerMinuteRate);
                                    ////////////////////////

                                    tracingService.Trace("AFTER UpdateWorkOrderServiceTaskHealthByDuration");

                                    EntityReference lastbookingworkedon = postImage.GetAttributeValue<EntityReference>("ap360_lastbookingworkedonid") != null ? postImage.GetAttributeValue<EntityReference>("ap360_lastbookingworkedonid") : null;

                                    if (lastbookingworkedon != null && EstimatedLaborAmount.Value > 0)
                                    {
                                        Entity reterivedBRB = service.Retrieve(lastbookingworkedon.LogicalName, lastbookingworkedon.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("ap360_workorderservicetask"));
                                        if (reterivedBRB != null)
                                        {
                                            tracingService.Trace("reterivedBRB is not null");
                                            EntityReference reterviedWOST = reterivedBRB.GetAttributeValue<EntityReference>("ap360_workorderservicetask") ?? null;
                                            if (reterviedWOST == null)
                                                return;
                                            if (reterviedWOST.Id == entity.Id)// only matches if Booking was created for reterviedWost
                                            {

                                                tracingService.Trace("Before updating Booking");
                                                Entity updateBrb = new Entity(lastbookingworkedon.LogicalName, lastbookingworkedon.Id);
                                                updateBrb["ap360_prewosthealth"] = postwosthealth;
                                                updateBrb["ap360_postwosthealth"] = DollarPredictedspend.Value / EstimatedLaborAmount.Value;
                                                //updateBrb["ap360_postworkorderhealth"] = ap360_workorderhealth;
                                                service.Update(updateBrb);

                                                //COMMENT ON 2/27/2021
                                                //Entity updateWorkOrderServiceTasklastbookingworkedon = new Entity(entity.LogicalName, entity.Id);
                                                //updateWorkOrderServiceTasklastbookingworkedon["ap360_lastbookingworkedonid"] = null;
                                                //service.Update(updateWorkOrderServiceTasklastbookingworkedon);
                                            }
                                        }
                                    }///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                                    if (workOrderRef != null)
                                    {
                                        tracingService.Trace("WO Ref is not null");

                                        WorkOrderServiceTaskHealthLog.CreateWOSTHealthLog(service, tracingService, entity, DollarPredictedspend, EstimatedLaborAmount,
                                            lastbookingworkedon, recentlyWorkedResource, workOrderRef, WOSTpercentcomplete, postwosthealth, WOSTServiceRole,
                                            WOSTHourlyRate, timeSpentOnLastBooking, timeSpentOnLastBookingAsJourneyMan, durationPostwosthealth, DurationPredictedspend, EstimatedDuration);
                                        decimal postWorkOrderHealth = 0;
                                        Money workOrderEstimatedLaborAmount = new Money();
                                        decimal cumulativeSumofPredictedSpend = 0;
                                        decimal workOrderRevisedEstimatedLaborAmount = 0;
                                        tracingService.Trace("Before GetWOSTCumulativePredictedSpend");
                                        WorkOrderServiceTask.GetWOSTCumulativePredictedSpend(service, tracingService, workOrderRef.Id, ref cumulativeSumofPredictedSpend, ref workOrderRevisedEstimatedLaborAmount);
                                        tracingService.Trace("Cumulatvie Spend " + cumulativeSumofPredictedSpend.ToString());

                                        Entity reterivedWorkOrder = service.Retrieve(workOrderRef.LogicalName, workOrderRef.Id,
                                            new Microsoft.Xrm.Sdk.Query.ColumnSet("msdyn_name",
                                            "ap360_preworkorderhealth", "ap360_totaloriginalestimatedlaboramount", "ap360_totalrevisedestimatedlaboramount",
                                            "ap360_postworkorderhealth"));

                                        if (reterivedWorkOrder != null)
                                        {
                                            tracingService.Trace("Reterived Work Order not null");
                                            //workOrderEstimatedLaborAmount = WorkOrder.getWorkOrderEstimatedValue(tracingService, reterivedWorkOrder, workOrderEstimatedLaborAmount);

                                            // throw new InvalidPluginExecutionException(workOrderEstimatedLaborAmount.Value.ToString()+" Old : New " +workOrderRevisedEstimatedLaborAmount.ToString());
                                            postWorkOrderHealth = reterivedWorkOrder.GetAttributeValue<decimal>("ap360_postworkorderhealth");
                                            WorkOrder.UpdateWorkOrderForHealth(tracingService, service, workOrderRef, new Money(workOrderRevisedEstimatedLaborAmount), cumulativeSumofPredictedSpend, postWorkOrderHealth);
                                            WorkOrderHealthLog.CreateWorkOrderLog(service, entity, lastbookingworkedon, recentlyWorkedResource, workOrderRef, new Money(workOrderRevisedEstimatedLaborAmount), cumulativeSumofPredictedSpend, reterivedWorkOrder);
                                        }
                                    }

                                }

                           
                            }
                            //if (entity.Contains("ap360_workorderservicetaskstatus")) { 

                            //}
                        }
                    }

                }





            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //private static decimal GetWOSTEstimatedDuration(Entity WOSTEntityRef)
        //{

        //    var WOSTrevisedestimatedAmount = EntityBase.GetMoneyAttributeValue(WOSTEntity, "ap360_revisedestimatedamount");
        //    var WOSToriginalEstimatedAmount = EntityBase.GetMoneyAttributeValue(WOSTEntity, "ap360_originalestimatedamount");//orginal Estimated Duration
        //    decimal WOSTEstiamtedAmount = 0;
        //    if (WOSTrevisedestimatedAmount > 0)
        //    {
        //        WOSTEstiamtedAmount = WOSTrevisedestimatedAmount;
        //    }
        //    else
        //    {
        //        WOSTEstiamtedAmount = WOSToriginalEstimatedAmount;
        //    }
        //    return WOSTEstiamtedAmount;
        //}


    }
}