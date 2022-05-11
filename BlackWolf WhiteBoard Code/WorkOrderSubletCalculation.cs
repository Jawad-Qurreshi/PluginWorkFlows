using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class WorkOrderSubletCalculation : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();

            try
            {
                // throw new InvalidPluginExecutionException("WorkOrderSubletCalculation");


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

                        if (entity.LogicalName == "ap360_workordersublet")
                        {
                            tracingService.Trace("Inside Update of workorder Sublet product");
                            if ((entity.Contains("ap360_originalestimatedamount")) || (entity.Contains("ap360_actualamount")) || (entity.Contains("ap360_revisedestimatedamount")) && context.Depth <= 2)
                            {


                                Entity workordersubl = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("ap360_originalestimatedamount", "ap360_actualamount", "ap360_revisedestimatedamount", "ap360_workorderid"));

                                if (entity.Contains("ap360_originalestimatedamount"))//Estimated amount
                                {
                                    tracingService.Trace("ap360_originalestimatedamount is updated");
                                    if (workordersubl != null)
                                    {
                                        tracingService.Trace("WorkOrderSublet  is not null");
                                        EntityReference workOrderRef = workordersubl.GetAttributeValue<EntityReference>("ap360_workorderid");
                                        if (workOrderRef != null)
                                        {
                                            tracingService.Trace("Work Order Ref is not null");
                                            WorkOrderSublet workOrderSublet = new WorkOrderSublet();
                                            workOrderSublet = WorkOrderSublet.GetWorkOrderSubletAmount(service, tracingService, workOrderRef.Id);
                                            Entity workOrderEntity = new Entity("msdyn_workorder");
                                            workOrderEntity.Id = workOrderRef.Id;
                                            workOrderEntity["ap360_subletoriginalestimatedamount"] = new Money(workOrderSublet.TotalOriginalestimateSubletAmount.Value);
                                            workOrderEntity["ap360_subletrevisedestimatedamount"] = new Money(workOrderSublet.TotalRevisedestimatedSubletAmount.Value + workOrderSublet.TotalOriginalestimateSubletAmount.Value);

                                            service.Update(workOrderEntity);
                                        }
                                    }
                                }

                                if (entity.Contains("ap360_actualamount"))//actual amount
                                {
                                    tracingService.Trace("actualamount is updated");
                                    if (workordersubl != null)
                                    {
                                        tracingService.Trace("WorkOrdersublet is not null");
                                        EntityReference workOrderRef = workordersubl.GetAttributeValue<EntityReference>("ap360_workorderid");
                                        if (workOrderRef != null)
                                        {
                                            tracingService.Trace("Work Order Ref is not null");
                                            WorkOrderSublet workOrderSublet = new WorkOrderSublet();
                                            workOrderSublet = WorkOrderSublet.GetWorkOrderSubletAmount(service, tracingService, workOrderRef.Id);
                                            Entity workOrderEntity = new Entity("msdyn_workorder");
                                            workOrderEntity.Id = workOrderRef.Id;
                                            tracingService.Trace("Sum of msdyn_actualparts " + workOrderSublet.TotalActualSubletAmount.Value.ToString());
                                            workOrderEntity["ap360_subletactualamount"] = new Money(workOrderSublet.TotalActualSubletAmount.Value);

                                            service.Update(workOrderEntity);
                                        }
                                    }
                                }

                                if (entity.Contains("ap360_revisedestimatedamount"))//revised amount
                                {

                                    tracingService.Trace("ap360_revisedestimatedamount is updated");
                                    if (workordersubl != null)
                                    {
                                        tracingService.Trace("WorkOrdersublet is not null");
                                        EntityReference workOrderRef = workordersubl.GetAttributeValue<EntityReference>("ap360_workorderid");
                                        if (workOrderRef != null)
                                        {
                                            tracingService.Trace("Work Order Ref is not null");
                                            WorkOrderSublet workOrderSublet = new WorkOrderSublet();
                                            workOrderSublet = WorkOrderSublet.GetWorkOrderSubletAmount(service, tracingService, workOrderRef.Id);
                                            Entity workOrderEntity = new Entity("msdyn_workorder");
                                            workOrderEntity.Id = workOrderRef.Id;
                                            tracingService.Trace("Sum of revised sublet amouunt " + workOrderSublet.TotalRevisedestimatedSubletAmount.Value.ToString());
                                            workOrderEntity["ap360_subletrevisedestimatedamount"] = new Money(workOrderSublet.TotalRevisedestimatedSubletAmount.Value + workOrderSublet.TotalOriginalestimateSubletAmount.Value);
                                            //throw new InvalidPluginExecutionException("Work not null "+ workOrderSublet.TotalRevisedestimatedSubletAmount.Value.ToString() +" "+ workOrderSublet.TotalOriginalestimateSubletAmount.Value.ToString());

                                            service.Update(workOrderEntity);
                                        }
                                    }
                                }
                            }

                        }

                    }
                }

                if (context.MessageName.ToLower() == "create")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        if (context.Depth > 1) return;
                        entity = (Entity)context.InputParameters["Target"];
                        
                        if (entity.LogicalName == "ap360_workordersublet")
                        {
                            tracingService.Trace("Create  of WorkOrder Product");

                            Entity workorderpro = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("ap360_originalestimatedamount", "ap360_actualamount", "ap360_revisedestimatedamount", "ap360_workorderid"));
                            if (workorderpro != null)
                            {
                                tracingService.Trace("WorkOrdersublet is not null");

                                EntityReference workOrderRef = workorderpro.GetAttributeValue<EntityReference>("ap360_workorderid");

                                if (workOrderRef != null)
                                {
                                    tracingService.Trace("Work Order Ref is not null");

                                    WorkOrderSublet workOrderSublet = new WorkOrderSublet();
                                    workOrderSublet = WorkOrderSublet.GetWorkOrderSubletAmount(service, tracingService, workOrderRef.Id);

                                    tracingService.Trace("Sum of Estimated Sublet amount " + workOrderSublet.TotalOriginalestimateSubletAmount.Value.ToString());
                                    tracingService.Trace("Sum of Actuall Sublet amount " + workOrderSublet.TotalActualSubletAmount.Value.ToString());
                                    tracingService.Trace("Sum of Revised Estimated Sublet amount " + workOrderSublet.TotalRevisedestimatedSubletAmount.Value.ToString());


                                    Entity workOrderEntity = new Entity("msdyn_workorder");
                                    workOrderEntity.Id = workOrderRef.Id;
                                    workOrderEntity["ap360_subletoriginalestimatedamount"] = new Money(workOrderSublet.TotalOriginalestimateSubletAmount.Value);
                                    workOrderEntity["ap360_subletactualamount"] = new Money(workOrderSublet.TotalActualSubletAmount.Value);
                                    workOrderEntity["ap360_subletrevisedestimatedamount"] = new Money(workOrderSublet.TotalRevisedestimatedSubletAmount.Value + workOrderSublet.TotalOriginalestimateSubletAmount.Value);

                                    service.Update(workOrderEntity);
                                }
                            }



                        }
                    }
                }

                if (context.MessageName.ToLower() == "delete")
                {

                    tracingService.Trace("delete Step of workOrder sublet");
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference)
                    {
                        tracingService.Trace("Inside Delete step");
                        EntityReference entityref = (EntityReference)context.InputParameters["Target"];
                        if (entityref.LogicalName == "ap360_workordersublet")
                        {


                            Entity workordersub = service.Retrieve(entityref.LogicalName, entityref.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("ap360_originalestimatedamount", "ap360_actualamount", "ap360_revisedestimatedamount", "ap360_workorderid"));

                            if (workordersub != null)
                            {
                                WorkOrderSublet workOrderSublet = new WorkOrderSublet();
                                tracingService.Trace("Work Order Sublet is not null");

                                if (workordersub.GetAttributeValue<EntityReference>("ap360_workorderid") != null)
                                {
                                    EntityReference workOrderEntityRef = workordersub.GetAttributeValue<EntityReference>("ap360_workorderid");
                                    workOrderSublet = WorkOrderSublet.GetWorkOrderSubletAmount(service, tracingService, workOrderEntityRef.Id);

                                    Money deletingestimatedAmount = workordersub.GetAttributeValue<Money>("ap360_originalestimatedamount") != null ? workordersub.GetAttributeValue<Money>("ap360_originalestimatedamount") : null;
                                    Money deletingactualAmount = workordersub.GetAttributeValue<Money>("ap360_actualamount") != null ? workordersub.GetAttributeValue<Money>("ap360_actualamount") : null;
                                    Money deletingRevisedestimatedAmount = workordersub.GetAttributeValue<Money>("ap360_revisedestimatedamount") != null ? workordersub.GetAttributeValue<Money>("ap360_revisedestimatedamount") : null;
                                    // Money sumofdeletingAmount = deletingestimatedAmount.Value + deletingactualAmount.Value + deletingRevisedestimatedAmount.Value;
                                    //tracingService.Trace("Deleting " + sumofdeletingduration.ToString());

                                    Entity workOrderEntity = new Entity("msdyn_workorder");
                                    workOrderEntity.Id = workOrderEntityRef.Id;
                                    if (deletingestimatedAmount != null)
                                    {
                                        tracingService.Trace("Total Original Estimated Sublet Amount " + workOrderSublet.TotalOriginalestimateSubletAmount.Value.ToString());
                                        tracingService.Trace("Total Revised Estimated Sublet Amount " + workOrderSublet.TotalRevisedestimatedSubletAmount.Value.ToString());
                                        tracingService.Trace("Deleting estimated Sublet Amount  " + deletingestimatedAmount.Value.ToString());

                                        workOrderEntity["ap360_subletoriginalestimatedamount"] = new Money(workOrderSublet.TotalOriginalestimateSubletAmount.Value - deletingestimatedAmount.Value);
                                        workOrderEntity["ap360_subletrevisedestimatedamount"] = new Money(workOrderSublet.TotalRevisedestimatedSubletAmount.Value + (workOrderSublet.TotalOriginalestimateSubletAmount.Value - deletingestimatedAmount.Value));
                                    }
                                    if (deletingactualAmount != null)
                                    {
                                        workOrderEntity["ap360_subletactualamount"] = new Money(workOrderSublet.TotalActualSubletAmount.Value - deletingactualAmount.Value);
                                        tracingService.Trace("Total Actual Duration " + workOrderSublet.TotalActualSubletAmount.Value.ToString());
                                        tracingService.Trace("Deleting Actual Amount  " + deletingactualAmount.Value.ToString());

                                    }
                                    if (deletingRevisedestimatedAmount != null)
                                    {
                                        tracingService.Trace("RevisedEstimate Amount");
                                        workOrderEntity["ap360_subletrevisedestimatedamount"] = new Money((workOrderSublet.TotalRevisedestimatedSubletAmount.Value - deletingRevisedestimatedAmount.Value) + workOrderSublet.TotalOriginalestimateSubletAmount.Value);
                                        tracingService.Trace("Total Revised Estimated Duration " + workOrderSublet.TotalRevisedestimatedSubletAmount.Value.ToString());
                                        tracingService.Trace("Deleting estimated Amount  " + deletingRevisedestimatedAmount.Value.ToString());

                                    }
                                    service.Update(workOrderEntity);
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