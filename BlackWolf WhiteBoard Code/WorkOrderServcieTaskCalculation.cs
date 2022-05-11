using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class WorkOrderServcieTaskCalculation : IPlugin
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
                            Entity workordersrvtask = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("msdyn_estimatedduration", "msdyn_actualduration", "ap360_revisedestimatedduration", "msdyn_workorder", "msdyn_percentcomplete", "ap360_actualamount", "ap360_originalestimatedamount", "ap360_revisedestimatedamount"));
                            if ((entity.Contains("ap360_revisedestimatedduration")) || (entity.Contains("msdyn_actualduration")) || (entity.Contains("msdyn_estimatedduration")) && context.Depth <= 3)
                            {

                                tracingService.Trace("Inside Update of Quote ServicTask");

                                if (entity.Contains("msdyn_estimatedduration"))//Estimated Duration
                                {
                                    tracingService.Trace("msdyn_estimatedduration is updated");
                                    if (workordersrvtask != null)
                                    {
                                        tracingService.Trace("WorkOrderservicetask is not null");
                                        EntityReference workOrderRef = workordersrvtask.GetAttributeValue<EntityReference>("msdyn_workorder");
                                        if (workOrderRef != null)
                                        {
                                            tracingService.Trace("Work Order Ref is not null");
                                            WorkOrderServiceTask workOrderServiceTask = new WorkOrderServiceTask();
                                            workOrderServiceTask = WorkOrderServiceTask.GetWorkOrderServiceTaskDuration(service, tracingService, workOrderRef.Id);
                                            Entity workOrderEntity = new Entity("msdyn_workorder");
                                            workOrderEntity.Id = workOrderRef.Id;
                                            workOrderEntity["ap360_totaloriginalestimatedduration"] = workOrderServiceTask.TotalOriginalEstimatedDuration;
                                            workOrderEntity["ap360_totalrevisedestimatedduration"] = workOrderServiceTask.TotalRevisedEstimatedDuration + workOrderServiceTask.TotalOriginalEstimatedDuration;

                                            service.Update(workOrderEntity);
                                        }
                                    }
                                }

                                if (entity.Contains("msdyn_actualduration"))//actual Duration
                                {
                                    tracingService.Trace("msdyn_actualduration is updated");
                                    if (workordersrvtask != null)
                                    {
                                        tracingService.Trace("WorkOrderservicetask is not null");
                                        EntityReference workOrderRef = workordersrvtask.GetAttributeValue<EntityReference>("msdyn_workorder");
                                        if (workOrderRef != null)
                                        {
                                            tracingService.Trace("Work Order Ref is not null");
                                            WorkOrderServiceTask workOrderServiceTask = new WorkOrderServiceTask();
                                            workOrderServiceTask = WorkOrderServiceTask.GetWorkOrderServiceTaskDuration(service, tracingService, workOrderRef.Id);
                                            Entity workOrderEntity = new Entity("msdyn_workorder");
                                            workOrderEntity.Id = workOrderRef.Id;
                                            tracingService.Trace("Sum of msdyn_actualduration " + workOrderServiceTask.TotalActualduration);
                                            workOrderEntity["ap360_totalactualduration"] = workOrderServiceTask.TotalActualduration;

                                            service.Update(workOrderEntity);
                                        }
                                    }
                                }

                                if (entity.Contains("ap360_revisedestimatedduration"))//revised Duration
                                {
                                    tracingService.Trace("ap360_revisedestimatedduration is updated");
                                    if (workordersrvtask != null)
                                    {
                                        tracingService.Trace("WorkOrderservicetask is not null");
                                        EntityReference workOrderRef = workordersrvtask.GetAttributeValue<EntityReference>("msdyn_workorder");
                                        if (workOrderRef != null)
                                        {
                                            tracingService.Trace("Work Order Ref is not null");
                                            WorkOrderServiceTask workOrderServiceTask = new WorkOrderServiceTask();
                                            workOrderServiceTask = WorkOrderServiceTask.GetWorkOrderServiceTaskDuration(service, tracingService, workOrderRef.Id);
                                            Entity workOrderEntity = new Entity("msdyn_workorder");
                                            workOrderEntity.Id = workOrderRef.Id;
                                            tracingService.Trace("Sum of ap360_revisedestimatedduration " + workOrderServiceTask.TotalRevisedEstimatedDuration);
                                            workOrderEntity["ap360_totalrevisedestimatedduration"] = workOrderServiceTask.TotalRevisedEstimatedDuration + workOrderServiceTask.TotalOriginalEstimatedDuration;

                                            service.Update(workOrderEntity);
                                        }
                                    }
                                }

                            }


                            ////////////////////
                            //if (entity.Contains("ap360_actualamount") || entity.Contains("msdyn_percentcomplete"))
                            //{
                            //    //calculate Predicted spend 
                            //    tracingService.Trace("actual amount of wost updated");
                            //    int WOSTpercentcomplete = Convert.ToInt32(workordersrvtask["msdyn_percentcomplete"]);

                            //    tracingService.Trace("wost percent complete is " + WOSTpercentcomplete.ToString());
                            //    Money actualAmount = workordersrvtask.GetAttributeValue<Money>("ap360_actualamount") != null ? workordersrvtask.GetAttributeValue<Money>("ap360_actualamount") : null;
                            //    if (actualAmount != null)
                            //    {

                            //        tracingService.Trace("Actual Amount " + actualAmount.Value.ToString());
                            //        Entity workOrderServiceTask = new Entity(entity.LogicalName);
                            //        workOrderServiceTask.Id = entity.Id;
                            //        if (WOSTpercentcomplete > 0)
                            //        {
                            //            workOrderServiceTask["ap360_predictedspend"] = new Money((actualAmount.Value * (100 - WOSTpercentcomplete) / WOSTpercentcomplete) + actualAmount.Value);
                            //        }
                            //        //else
                            //        //{
                            //        //    Money ap360_originalestimatedamount = workordersrvtask.GetAttributeValue<Money>("ap360_originalestimatedamount") != null ? workordersrvtask.GetAttributeValue<Money>("ap360_originalestimatedamount") : null;
                            //        //    Money ap360_revisedestimatedamount = workordersrvtask.GetAttributeValue<Money>("ap360_revisedestimatedamount") != null ? workordersrvtask.GetAttributeValue<Money>("ap360_revisedestimatedamount") : null;
                            //        //    if (ap360_originalestimatedamount != null && ap360_originalestimatedamount.Value > 0)
                            //        //        workOrderServiceTask["ap360_predictedspend"] = new Money(ap360_originalestimatedamount.Value);
                            //        //    else
                            //        //        workOrderServiceTask["ap360_predictedspend"] = new Money(ap360_revisedestimatedamount.Value);

                            //        //    // throw new InvalidPluginExecutionException();
                            //        //}
                            //        service.Update(workOrderServiceTask);
                            //        //CalculateWOSTHealth  is next plugin triggers as a result of current update 
                            //        //        throw new InvalidPluginExecutionException("Error ");


                            //        EntityReference workOrderRef = workordersrvtask.GetAttributeValue<EntityReference>("msdyn_workorder");
                            //        if (workOrderRef != null)
                            //        {
                            //            tracingService.Trace("WO Ref is not null");
                            //            decimal cumulativeSumofPredictedSpend = 0;
                            //            cumulativeSumofPredictedSpend = WorkOrderServiceTask.GetWOSTCumulativePredictedSpend(service, tracingService, workOrderRef.Id);
                            //            tracingService.Trace("Cumulatvie Spend " + cumulativeSumofPredictedSpend.ToString());

                            //            Entity reterivedWorkOrder = service.Retrieve(workOrderRef.LogicalName, workOrderRef.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("ap360_totaloriginalestimatedlaboramount", "ap360_totalrevisedestimatedlaboramount"));
                            //            if (reterivedWorkOrder != null)
                            //            {
                            //                tracingService.Trace("Reterived Work Order not null");



                            //                Entity updateWorkOrder = new Entity(workOrderRef.LogicalName, workOrderRef.Id);
                            //                updateWorkOrder["ap360_predictedspend"] = new Money(cumulativeSumofPredictedSpend);
                            //                service.Update(updateWorkOrder);
                            //                //CalculateWorkOrderHealth  is next plugin triggers as a result of current update 
                            //            }
                            //        }

                            //    }
                            //    // (Fields!msdyn_workorder1_ap360_totalbillableamount.Value * (100 - Fields!PercentComplete.Value)
                            //    /// Fields!PercentComplete.Value) +Fields!msdyn_workorder1_ap360_totalbillableamount.Value

                            //    // }

                            //}

                            ///////////////////
                        }
                    }

                }




                if (context.MessageName.ToLower() == "create")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        tracingService.Trace("Create  of work Order ServicTask");
                        entity = (Entity)context.InputParameters["Target"];

                        if (entity.LogicalName == "msdyn_workorderservicetask")
                        {
                            if (context.Stage == 20)
                            {
                                decimal preWOSThealth = 1;
                                entity["ap360_prewosthealth"] = preWOSThealth;
                            }

                            if (context.Stage == 40)
                            {

                                if (context.Depth > 1) return;
                                Entity workordersrvtask = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("msdyn_estimatedduration", "msdyn_actualduration", "ap360_revisedestimatedduration", "msdyn_workorder"));
                                if (workordersrvtask != null)
                                {
                                    tracingService.Trace("WorkOrderservicetask is not null");

                                    EntityReference workOrderRef = workordersrvtask.GetAttributeValue<EntityReference>("msdyn_workorder");

                                    if (workOrderRef != null)
                                    {
                                        tracingService.Trace("Work Order Ref is not null");

                                        WorkOrderServiceTask workOrderServiceTask = new WorkOrderServiceTask();
                                        workOrderServiceTask = WorkOrderServiceTask.GetWorkOrderServiceTaskDuration(service, tracingService, workOrderRef.Id);

                                        tracingService.Trace("Sum of Estimated Duration " + workOrderServiceTask.TotalOriginalEstimatedDuration);
                                        tracingService.Trace("Sum of Actuall Duration " + workOrderServiceTask.TotalActualduration);
                                        tracingService.Trace("Sum of Revised Estimated Duration " + workOrderServiceTask.TotalRevisedEstimatedDuration);


                                        Entity workOrderEntity = new Entity("msdyn_workorder");
                                        workOrderEntity.Id = workOrderRef.Id;
                                        workOrderEntity["ap360_totaloriginalestimatedduration"] = workOrderServiceTask.TotalOriginalEstimatedDuration;
                                        workOrderEntity["ap360_totalactualduration"] = workOrderServiceTask.TotalActualduration;
                                        workOrderEntity["ap360_totalrevisedestimatedduration"] = workOrderServiceTask.TotalRevisedEstimatedDuration + workOrderServiceTask.TotalOriginalEstimatedDuration;

                                        service.Update(workOrderEntity);
                                    }
                                }
                            }
                        }



                    }
                }


                if (context.MessageName.ToLower() == "delete")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference)
                    {
                        tracingService.Trace("Inside Delete step");
                        EntityReference entityref = (EntityReference)context.InputParameters["Target"];
                        if (entityref.LogicalName == "msdyn_workorderservicetask")
                        {



                            Entity workordersrvtask = service.Retrieve(entityref.LogicalName, entityref.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("msdyn_estimatedduration", "msdyn_actualduration", "ap360_revisedestimatedduration", "msdyn_workorder"));
                            if (workordersrvtask != null)
                            {
                                WorkOrderServiceTask workOrderServiceTask = new WorkOrderServiceTask();
                                tracingService.Trace("Work Order Service Task is not null");

                                EntityReference workOrderEntityRef = workordersrvtask.GetAttributeValue<EntityReference>("msdyn_workorder");
                                workOrderServiceTask = WorkOrderServiceTask.GetWorkOrderServiceTaskDuration(service, tracingService, workOrderEntityRef.Id);

                                int deletingestimatedduration = workordersrvtask.GetAttributeValue<int>("msdyn_estimatedduration");
                                int deletingactualduration = workordersrvtask.GetAttributeValue<int>("msdyn_actualduration");
                                int deletingRevisedestimatedduration = workordersrvtask.GetAttributeValue<int>("ap360_revisedestimatedduration");
                                int sumofdeletingduration = deletingestimatedduration + deletingactualduration + deletingRevisedestimatedduration;
                                tracingService.Trace("Deleting " + sumofdeletingduration.ToString());

                                Entity workOrderEntity = new Entity("msdyn_workorder");
                                workOrderEntity.Id = workOrderEntityRef.Id;
                                if (deletingestimatedduration > 0)
                                {
                                    tracingService.Trace("Total Original Estimated Duration " + workOrderServiceTask.TotalOriginalEstimatedDuration.ToString());
                                    tracingService.Trace("Total Revised Estimated Duration " + workOrderServiceTask.TotalRevisedEstimatedDuration.ToString());
                                    tracingService.Trace("Deleting estimated Amount  " + deletingestimatedduration.ToString());

                                    workOrderEntity["ap360_totaloriginalestimatedduration"] = workOrderServiceTask.TotalOriginalEstimatedDuration - deletingestimatedduration;
                                    workOrderEntity["ap360_totalrevisedestimatedduration"] = workOrderServiceTask.TotalRevisedEstimatedDuration + (workOrderServiceTask.TotalOriginalEstimatedDuration - deletingestimatedduration);
                                }
                                if (deletingactualduration > 0)
                                {
                                    workOrderEntity["ap360_totalactualduration"] = workOrderServiceTask.TotalActualduration - deletingactualduration;
                                    tracingService.Trace("Total Actual Duration " + workOrderServiceTask.TotalActualduration.ToString());
                                    tracingService.Trace("Deleting Actual Amount  " + deletingactualduration.ToString());

                                }
                                if (deletingRevisedestimatedduration > 0)
                                {
                                    workOrderEntity["ap360_totalrevisedestimatedduration"] = (workOrderServiceTask.TotalRevisedEstimatedDuration - deletingRevisedestimatedduration) + workOrderServiceTask.TotalOriginalEstimatedDuration;
                                    tracingService.Trace("Total Revised Estimated Duration " + workOrderServiceTask.TotalRevisedEstimatedDuration.ToString());
                                    tracingService.Trace("Deleting estimated Amount  " + deletingRevisedestimatedduration.ToString());

                                }
                                service.Update(workOrderEntity);
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