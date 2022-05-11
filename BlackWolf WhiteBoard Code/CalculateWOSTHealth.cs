using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class CalculateWOSTHealth : IPlugin
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
                            if ((entity.Contains("ap360_originalestimatedamount")) || (entity.Contains("ap360_revisedestimatedamount")))
                            {
                                tracingService.Trace("update of workOrder Service task");

                                Entity postImage = (Entity)context.PostEntityImages["Image"];
                                Money ap360_predictedspend = new Money();
                                Money ap360_originalestimatedamount = new Money();
                                Money ap360_revisedestimatedamount = new Money();
                                Money EstimatedLaborAmount = new Money();
                                int EstimatedDuration = 0;
                                int msdyn_estimatedduration = 0;// original esimated duration
                                int ap360_revisedestimatedduration = 0;
                                int ap360_durationpredictedspend = 0;
                                string WOSTName = null;
                                if (postImage.Contains("msdyn_name"))
                                {
                                    WOSTName = postImage.GetAttributeValue<string>("msdyn_name");
                                    tracingService.Trace(WOSTName);
                                }


                                if (postImage.Contains("ap360_predictedspend"))
                                {
                                    ap360_predictedspend = postImage.GetAttributeValue<Money>("ap360_predictedspend") != null ? postImage.GetAttributeValue<Money>("ap360_predictedspend") : null;
                                    if (ap360_predictedspend != null)
                                    {
                                        if (ap360_predictedspend.Value == 0)
                                            return;
                                    }
                                    tracingService.Trace("ap360_predictedspend " + ap360_predictedspend.Value.ToString());
                                }
                                if (postImage.Contains("ap360_durationpredictedspend"))
                                {
                                    ap360_durationpredictedspend = postImage.GetAttributeValue<int>("ap360_durationpredictedspend");
                                    if (ap360_durationpredictedspend == 0)
                                    {
                                        return;
                                    }
                                    tracingService.Trace("ap360_durationpredictedspend " + ap360_durationpredictedspend.ToString());
                                }

                                decimal postwosthealth = 0;
                                if (postImage.Contains("ap360_postwosthealth"))
                                {
                                    postwosthealth = postImage.GetAttributeValue<decimal>("ap360_postwosthealth");
                                    tracingService.Trace("Post Wost Helath " + postwosthealth.ToString());

                                }
                                //decimal durationpostwosthealth = 0;
                                //if (postImage.Contains("ap360_durationpostwosthealth"))
                                //{
                                //    durationpostwosthealth = postImage.GetAttributeValue<decimal>("ap360_durationpostwosthealth");
                                //    tracingService.Trace("ap360_durationpostwosthealth " + durationpostwosthealth.ToString());

                                //}

                                EstimatedLaborAmount = WorkOrderServiceTask.getWOSTEstimatedDollarValue(tracingService, postImage, ap360_originalestimatedamount, ap360_revisedestimatedamount, EstimatedLaborAmount);

                                //EstimatedDuration = WorkOrderServiceTask.getWOSTEstimatedDurationValue(tracingService, postImage, msdyn_estimatedduration,
                                //       ap360_revisedestimatedduration, EstimatedDuration);

                                Entity updateWorkOrderServiceTask = new Entity(entity.LogicalName, entity.Id);
                                if ((EstimatedLaborAmount != null && EstimatedLaborAmount.Value > 0) && ap360_predictedspend.Value > 0)
                                {

                                    tracingService.Trace("Estimated Labor is  " + EstimatedLaborAmount.Value.ToString());

                                    updateWorkOrderServiceTask["ap360_prewosthealth"] = postwosthealth;
                                    updateWorkOrderServiceTask["ap360_postwosthealth"] = ap360_predictedspend.Value / EstimatedLaborAmount.Value;
                                }

                                //if (EstimatedDuration > 0 && ap360_durationpredictedspend > 0)
                                //{
                                //    updateWorkOrderServiceTask["ap360_durationprewosthealth"] = durationpostwosthealth;

                                //    // throw new InvalidPluginExecutionException(ap360_durationpredictedspend.ToString()+" "+ EstimatedDuration.ToString());
                                //    updateWorkOrderServiceTask["ap360_durationpostwosthealth"] = Convert.ToDecimal(ap360_durationpredictedspend / EstimatedDuration);
                                //}
                                // updateWorkOrderServiceTask["ap360_lastbookingworkedonid"] = null;

                                service.Update(updateWorkOrderServiceTask);


                                // to update Post health on booking

                                //if (postImage.Contains("ap360_lastbookingworkedonid"))
                                //{



                                //    // GetBRBRelatedWorkOrderServiceTask()
                                //    //decimal ap360_workorderhealth = 0;
                                //    DateTime WOSTHealthUpdate = postImage.GetAttributeValue<DateTime>("ap360_wosthealthupdate");
                                //    //  throw new InvalidPluginExecutionException(" Custom Erro "+ WOSTName +" "+ WOSTHealthUpdate);
                                //    tracingService.Trace("Wost Health Update Time :" + WOSTHealthUpdate);
                                //    EntityReference lastbookingworkedon = postImage.GetAttributeValue<EntityReference>("ap360_lastbookingworkedonid") != null ? postImage.GetAttributeValue<EntityReference>("ap360_lastbookingworkedonid") : null;
                                //    if (lastbookingworkedon != null)
                                //    {
                                //        tracingService.Trace("lastBookingWorkedOn is not null");
                                //        //if (postImage.Contains("msdyn_workorder"))
                                //        //{
                                //        //    EntityReference msdyn_workorder = postImage.GetAttributeValue<EntityReference>("msdyn_workorder") != null ? postImage.GetAttributeValue<EntityReference>("msdyn_workorder") : null;
                                //        //    Entity reterivedWorkOrderEntity = service.Retrieve(msdyn_workorder.LogicalName, msdyn_workorder.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("ap360_workorderhealth"));
                                //        //    if (reterivedWorkOrderEntity != null)
                                //        //    {
                                //        //        ap360_workorderhealth = reterivedWorkOrderEntity.GetAttributeValue<decimal>("ap360_workorderhealth");
                                //        //    }
                                //        //}
                                //        if ((EstimatedLaborAmount != null && EstimatedLaborAmount.Value > 0) && ap360_predictedspend.Value > 0)
                                //        {
                                //            tracingService.Trace("Before updating Booking");
                                //            Entity updateBrb = new Entity(lastbookingworkedon.LogicalName, lastbookingworkedon.Id);
                                //            updateBrb["ap360_postwosthealth"] = ap360_predictedspend.Value / EstimatedLaborAmount.Value;
                                //            //updateBrb["ap360_postworkorderhealth"] = ap360_workorderhealth;
                                //            service.Update(updateBrb);

                                //            Entity updateWorkOrderServiceTasklastbookingworkedon = new Entity(entity.LogicalName, entity.Id);
                                //            updateWorkOrderServiceTasklastbookingworkedon["ap360_lastbookingworkedonid"] = null;
                                //            service.Update(updateWorkOrderServiceTasklastbookingworkedon);


                                //        }
                                //    }
                                //}

                                // throw new InvalidPluginExecutionException("No error");

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