using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class WorkOrderProductCalculation : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();

            try
            {
                // throw new InvalidPluginExecutionException("WorkOrderProductCalculation");


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

                        if (entity.LogicalName == "msdyn_workorderproduct")
                        {
                            tracingService.Trace("Inside Update of workorder product");
                            if ((entity.Contains("ap360_originalestimateamount")) || (entity.Contains("ap360_actualamount")) || (entity.Contains("ap360_revisedestimateamount") || (entity.Contains("msdyn_linestatus"))) && context.Depth <= 2)
                            {
                                Entity workorderpro = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("ap360_originalestimateamount", "ap360_actualamount", "ap360_revisedestimateamount", "msdyn_workorder"));

                                if (entity.Contains("ap360_originalestimateamount"))//Estimated amount
                                {
                                    tracingService.Trace("ap360_originalestimateamount is updated");
                                    if (workorderpro != null)
                                    {
                                        tracingService.Trace("WorkOrderproduct  is not null");
                                        EntityReference workOrderRef = workorderpro.GetAttributeValue<EntityReference>("msdyn_workorder");
                                        if (workOrderRef != null)
                                        {
                                            tracingService.Trace("Work Order Ref is not null");
                                            WorkOrderProduct workOrderProduct = new WorkOrderProduct();
                                            workOrderProduct = WorkOrderProduct.GetWorkOrderProductAmount(service, tracingService, workOrderRef.Id);
                                            Entity workOrderEntity = new Entity("msdyn_workorder");
                                            workOrderEntity.Id = workOrderRef.Id;
                                            workOrderEntity["ap360_totaloriginalestimatepartsamount"] = new Money(workOrderProduct.TotalOriginalestimateamount.Value);
                                            workOrderEntity["ap360_totalrevisedestimatepartsamount"] = new Money(workOrderProduct.TotalRevisedestimateamount.Value + workOrderProduct.TotalOriginalestimateamount.Value);

                                            service.Update(workOrderEntity);
                                        }
                                    }
                                }

                                if (entity.Contains("ap360_actualamount"))//actual amount
                                {
                                    tracingService.Trace("msdyn_actualduration is updated");
                                    if (workorderpro != null)
                                    {
                                        tracingService.Trace("WorkOrderservicetask is not null");
                                        EntityReference workOrderRef = workorderpro.GetAttributeValue<EntityReference>("msdyn_workorder");
                                        if (workOrderRef != null)
                                        {
                                            tracingService.Trace("Work Order Ref is not null");
                                            WorkOrderProduct workOrderProduct = new WorkOrderProduct();
                                            workOrderProduct = WorkOrderProduct.GetWorkOrderProductAmount(service, tracingService, workOrderRef.Id);
                                            tracingService.Trace("GetWorkOrderProductAmount is finished");

                                            Entity workOrderEntity = new Entity("msdyn_workorder");
                                            workOrderEntity.Id = workOrderRef.Id;

                                            if (workOrderProduct.TotalActualAmount != null)
                                            {
                                                tracingService.Trace("Sum of msdyn_actualparts " + workOrderProduct.TotalActualAmount.ToString());
                                                //.TotalActualAmount.Value.ToString()
                                                workOrderEntity["ap360_totalactualpartsamount"] = new Money(workOrderProduct.TotalActualAmount.Value);

                                            }

                                            else
                                            {
                                                workOrderEntity["ap360_totalactualpartsamount"] = new Money(0);

                                            }
                                            service.Update(workOrderEntity);

                                        }
                                    }
                                }

                                if (entity.Contains("msdyn_linestatus"))//Line Status
                                {
                                    int lineStatus = entity.GetAttributeValue<OptionSetValue>("msdyn_linestatus").Value;
                                    if (lineStatus == 126300000)//Not Used
                                    {
                                        tracingService.Trace("msdyn_linestatus is updated: NOT Used");
                                        if (workorderpro != null)
                                        {
                                            tracingService.Trace("WorkOrderservicetask is not null");
                                            EntityReference workOrderRef = workorderpro.GetAttributeValue<EntityReference>("msdyn_workorder");
                                            if (workOrderRef != null)
                                            {
                                                tracingService.Trace("Work Order Ref is not null");
                                                WorkOrderProduct workOrderProduct = new WorkOrderProduct();
                                                workOrderProduct = WorkOrderProduct.GetWorkOrderProductAmount(service, tracingService, workOrderRef.Id);
                                                Entity workOrderEntity = new Entity("msdyn_workorder");
                                                workOrderEntity.Id = workOrderRef.Id;

                                                if (workOrderProduct.TotalActualAmount != null || workOrderProduct.TotalRevisedestimateamount != null)
                                                {
                                                    // throw new InvalidPluginExecutionException(" Error");
                                                    if (workOrderProduct.TotalActualAmount != null)
                                                    {
                                                        tracingService.Trace("Sum of msdyn_actualparts " + workOrderProduct.TotalActualAmount.Value.ToString());
                                                        workOrderEntity["ap360_totalactualpartsamount"] = new Money(workOrderProduct.TotalActualAmount.Value);

                                                    }
                                                    else if (workOrderProduct.TotalRevisedestimateamount != null)
                                                    {
                                                        workOrderEntity["ap360_totalrevisedestimatepartsamount"] = new Money(workOrderProduct.TotalRevisedestimateamount.Value + workOrderProduct.TotalOriginalestimateamount.Value);
                                                    }
                                                    service.Update(workOrderEntity);
                                                }
                                                else
                                                {
                                                    workOrderEntity["ap360_totalactualpartsamount"] = new Money(0);
                                                    workOrderEntity["ap360_totalrevisedestimatepartsamount"] = new Money(0);

                                                    service.Update(workOrderEntity);

                                                }
                                            }
                                        }
                                    }
                                }

                                if (entity.Contains("ap360_revisedestimateamount"))//revised amount
                                {
                                    tracingService.Trace("ap360_revisedestimatedduration is updated");
                                    if (workorderpro != null)
                                    {
                                        tracingService.Trace("WorkOrderservicetask is not null");
                                        EntityReference workOrderRef = workorderpro.GetAttributeValue<EntityReference>("msdyn_workorder");
                                        if (workOrderRef != null)
                                        {
                                            tracingService.Trace("Work Order Ref is not null");
                                            WorkOrderProduct workOrderProduct = new WorkOrderProduct();
                                            workOrderProduct = WorkOrderProduct.GetWorkOrderProductAmount(service, tracingService, workOrderRef.Id);
                                            Entity workOrderEntity = new Entity("msdyn_workorder");
                                            workOrderEntity.Id = workOrderRef.Id;
                                            // tracingService.Trace("Sum of ap360_revisedestimated amouunt " + workOrderProduct.TotalRevisedestimateamount.Value.ToString());
                                            workOrderEntity["ap360_totalrevisedestimatepartsamount"] = new Money(workOrderProduct.TotalRevisedestimateamount.Value + workOrderProduct.TotalOriginalestimateamount.Value);

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
                        entity = (Entity)context.InputParameters["Target"];

                        if (entity.LogicalName == "msdyn_workorderproduct")
                        {
                            if (context.Depth > 1) return;
                            tracingService.Trace("Create  of WorkOrder Product");

                            Entity workorderpro = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("ap360_originalestimateamount", "ap360_actualamount", "ap360_revisedestimateamount", "msdyn_workorder"));
                            if (workorderpro != null)
                            {
                                tracingService.Trace("WorkOrderproduct is not null");

                                EntityReference workOrderRef = workorderpro.GetAttributeValue<EntityReference>("msdyn_workorder");

                                if (workOrderRef != null)
                                {
                                    tracingService.Trace("Work Order Ref is not null");

                                    WorkOrderProduct workOrderProduct = new WorkOrderProduct();
                                    workOrderProduct = WorkOrderProduct.GetWorkOrderProductAmount(service, tracingService, workOrderRef.Id);

                                    tracingService.Trace("Sum of Estimated amount " + workOrderProduct.TotalOriginalestimateamount.Value.ToString());
                                    tracingService.Trace("Sum of Actuall amount " + workOrderProduct.TotalActualAmount.Value.ToString());
                                    tracingService.Trace("Sum of Revised Estimated amount " + workOrderProduct.TotalRevisedestimateamount.Value.ToString());


                                    Entity workOrderEntity = new Entity("msdyn_workorder");
                                    workOrderEntity.Id = workOrderRef.Id;
                                    workOrderEntity["ap360_totaloriginalestimatepartsamount"] = new Money(workOrderProduct.TotalOriginalestimateamount.Value);
                                    workOrderEntity["ap360_totalactualpartsamount"] = new Money(workOrderProduct.TotalActualAmount.Value);
                                    workOrderEntity["ap360_totalrevisedestimatepartsamount"] = new Money(workOrderProduct.TotalRevisedestimateamount.Value + workOrderProduct.TotalOriginalestimateamount.Value);

                                    service.Update(workOrderEntity);
                                }
                            }



                        }
                    }
                }

                if (context.MessageName.ToLower() == "delete")
                {

                    tracingService.Trace("delete Step of workOrder product");
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference)
                    {
                        tracingService.Trace("Inside Delete step");
                        EntityReference entityref = (EntityReference)context.InputParameters["Target"];
                        if (entityref.LogicalName == "msdyn_workorderproduct")
                        {


                            Entity workorderpro = service.Retrieve(entityref.LogicalName, entityref.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("ap360_originalestimateamount", "ap360_actualamount", "ap360_revisedestimateamount", "msdyn_workorder"));

                            if (workorderpro != null)
                            {
                                WorkOrderProduct workOrderProduct = new WorkOrderProduct();
                                tracingService.Trace("Work Order Product is not null");

                                EntityReference workOrderEntityRef = workorderpro.GetAttributeValue<EntityReference>("msdyn_workorder");
                                workOrderProduct = WorkOrderProduct.GetWorkOrderProductAmount(service, tracingService, workOrderEntityRef.Id);

                                Money deletingestimatedAmount = workorderpro.GetAttributeValue<Money>("ap360_originalestimateamount") != null ? workorderpro.GetAttributeValue<Money>("ap360_originalestimateamount") : null;
                                Money deletingactualAmount = workorderpro.GetAttributeValue<Money>("ap360_actualamount") != null ? workorderpro.GetAttributeValue<Money>("ap360_actualamount") : null;
                                Money deletingRevisedestimatedAmount = workorderpro.GetAttributeValue<Money>("ap360_revisedestimateamount") != null ? workorderpro.GetAttributeValue<Money>("ap360_revisedestimateamount") : null;
                                // Money sumofdeletingAmount = deletingestimatedAmount.Value + deletingactualAmount.Value + deletingRevisedestimatedAmount.Value;
                                if (workOrderEntityRef != null)
                                {
                                    Entity workOrderEntity = new Entity("msdyn_workorder");
                                    workOrderEntity.Id = workOrderEntityRef.Id;
                                    if (deletingestimatedAmount != null)
                                    {
                                        tracingService.Trace("deletingestimatedAmount  ");
                                        // tracingService.Trace("Total Original Estimated Duration " + workOrderProduct.TotalOriginalestimateamount.Value.ToString());
                                        // tracingService.Trace("Total Revised Estimated Duration " + workOrderProduct.TotalRevisedestimateamount.Value.ToString());
                                        // tracingService.Trace("Deleting estimated Amount  " + deletingestimatedAmount.Value.ToString());
                                        if (workOrderProduct.TotalOriginalestimateamount != null)
                                        {
                                            workOrderEntity["ap360_totaloriginalestimatepartsamount"] = new Money(workOrderProduct.TotalOriginalestimateamount.Value - deletingestimatedAmount.Value);
                                        }
                                        if (workOrderProduct.TotalRevisedestimateamount != null)
                                        {
                                            workOrderEntity["ap360_totalrevisedestimatepartsamount"] = new Money(workOrderProduct.TotalRevisedestimateamount.Value + (workOrderProduct.TotalOriginalestimateamount.Value - deletingestimatedAmount.Value));
                                        }
                                    }
                                    if (deletingactualAmount != null)
                                    {
                                        tracingService.Trace("deletingactualAmount  ");
                                        if (workOrderProduct.TotalActualAmount != null)
                                        {
                                            workOrderEntity["ap360_totalactualpartsamount"] = new Money(workOrderProduct.TotalActualAmount.Value - deletingactualAmount.Value);
                                            tracingService.Trace("Total Actual Duration " + workOrderProduct.TotalActualAmount.Value.ToString());
                                            tracingService.Trace("Deleting Actual Amount  " + deletingactualAmount.Value.ToString());
                                        }

                                    }
                                    if (deletingRevisedestimatedAmount != null)
                                    {
                                        tracingService.Trace("deletingRevisedestimatedAmount");
                                        if (workOrderProduct.TotalRevisedestimateamount != null)
                                        {
                                            workOrderEntity["ap360_totalrevisedestimatepartsamount"] = new Money((workOrderProduct.TotalRevisedestimateamount.Value - deletingRevisedestimatedAmount.Value) + workOrderProduct.TotalOriginalestimateamount.Value);
                                            tracingService.Trace("Total Revised Estimated Duration " + workOrderProduct.TotalRevisedestimateamount.Value.ToString());
                                            tracingService.Trace("Deleting estimated Amount  " + deletingRevisedestimatedAmount.Value.ToString());
                                        }

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