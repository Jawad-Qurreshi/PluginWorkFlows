using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class WorkOrderEntityCalculation : IPlugin
    {

        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                //  throw new InvalidPluginExecutionException("WorkOrderEntityCalculation");

                ITracingService tracingService =
                  (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                IPluginExecutionContext context = (IPluginExecutionContext)
                    serviceProvider.GetService(typeof(IPluginExecutionContext));

                IOrganizationServiceFactory factory =
                     (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = factory.CreateOrganizationService(context.UserId);

                Entity entity = null;
                //if (context.MessageName.ToLower() == "retrieve")

                //{
                //    throw new InvalidPluginExecutionException("My Error ");

                //    WorkOrder.WorkOrderRetrieve(context, service, tracingService);

                //}
                //if (context.MessageName.ToLower() == "retrievemultiple")
                //{
                //    WorkOrder.WorkOrderRetrieve(context, service, tracingService);
                //}
                Money partsAmount;
                Money LaborAmount;
                Money SaleTax;
                Money WOSTLaborAmount;
                Money TotalOriginalEstimatedLaborAmount;
                Money TotalOriginalEstimatedPartsAmount;
                Money TotalOriginalEstimatedParstsSaleTax;
                Money TotalSubletOrginalEstimatedAmount;
                Money TotalSubletActualAmount;
                Money TotalSubletRevisedEstimatedAmount;



                if (context.MessageName.ToLower() == "update")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        entity = (Entity)context.InputParameters["Target"];
                        if (entity.LogicalName == "msdyn_workorder")
                        {
                            Entity workOrderEntity = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));

                            EntityReference serviceRole = workOrderEntity.GetAttributeValue<EntityReference>("ap360_servicerole") != null ? workOrderEntity.GetAttributeValue<EntityReference>("ap360_servicerole") : null;
                            if (serviceRole == null) return;

                            decimal serviceRolePrice = BookableResourceBooking.GetRolePrice(service, tracingService, new Guid("4fe16dd5-8e55-ea11-a811-000d3a33f3c3"), serviceRole.Id);

                            //Total actual duration
                            //Total Actual Parts Amount
                            //Total Original Estimate Parts Amount
                            //Total Original Estiamted Duration
                            //Total Revised Estimate parts Amount
                            //Total Revised Estiamted Duration
                            if (context.Stage == 20)// pre
                            {
                                tracingService.Trace("On Pre update of workOrder");
                                if (entity.Contains("ap360_totaloriginalestimatepartsamount"))
                                    entity["ap360_totaloriginalestimatedpartssaletax"] = new Money((entity.GetAttributeValue<Money>("ap360_totaloriginalestimatepartsamount").Value / 100) * 6);
                                if (entity.Contains("ap360_totalactualpartsamount"))
                                    entity["ap360_totalactualpartsaletax"] = new Money((entity.GetAttributeValue<Money>("ap360_totalactualpartsamount").Value / 100) * 6);
                                if (entity.Contains("ap360_totalrevisedestimatepartsamount"))
                                {
                                    // decimal Totaloriginalestimatedpartssaletax = entity.GetAttributeValue<Money>("ap360_totaloriginalestimatedpartssaletax") != null ? entity.GetAttributeValue<Money>("ap360_totaloriginalestimatedpartssaletax").Value : 0;

                                    entity["ap360_totalrevisedestimatedpartssaletax"] = new Money(((entity.GetAttributeValue<Money>("ap360_totalrevisedestimatepartsamount").Value / 100) * 6));
                                }

                                ////////////////////////////////////////////////////////////

                                if (entity.Contains("ap360_totaloriginalestimatedduration"))
                                    entity["ap360_totaloriginalestimatedlaboramount"] = new Money((entity.GetAttributeValue<int>("ap360_totaloriginalestimatedduration") * serviceRolePrice) / 60);
                                //This value is calculating on bookable resouce booking reterive call
                                //if (entity.Contains("ap360_totalactualduration"))
                                //    entity["ap360_totalactuallaboramount"] = new Money(entity.GetAttributeValue<int>("ap360_totalactualduration") * 3);
                                if (entity.Contains("ap360_totalrevisedestimatedduration"))
                                {
                                    // decimal Totaloriginalestimatedlaboramount = entity.GetAttributeValue<Money>("ap360_totaloriginalestimatedlaboramount") != null ? entity.GetAttributeValue<Money>("ap360_totaloriginalestimatedlaboramount").Value : 0;
                                    tracingService.Trace(("totalrevisedestimatedduration " + entity.GetAttributeValue<int>("ap360_totalrevisedestimatedduration").ToString()));
                                    entity["ap360_totalrevisedestimatedlaboramount"] = new Money(((entity.GetAttributeValue<int>("ap360_totalrevisedestimatedduration") * serviceRolePrice) / 60));
                                }
                            }
                            //////////////////////////////////////For Last final section: Total Amouns //////////////////////////

                            //Sublet Actual Amount
                            //Sublet Original Estimated Amount
                            //Sublet Revised Estimated Amount

                            //Total Actual Labor Amount
                            //Total Actual Parts Amount
                            //Totla Actual WOST Labor Amount

                            //    Total Orignial Estimate Parts Amount
                            //    Total Original Estiamted Labor Amount
                            //    Totla Revised Estiamte Parts Amount
                            //    total Revised Estimated Labor Amount

                            if (context.Stage == 40)//post
                            {


                                tracingService.Trace("On Post update of workOrder");

                                Entity NewWorkOrderEntity = new Entity("msdyn_workorder");


                                partsAmount = new Money((workOrderEntity.GetAttributeValue<Money>("ap360_totaloriginalestimatepartsamount") != null ? (workOrderEntity.GetAttributeValue<Money>("ap360_totaloriginalestimatepartsamount").Value) : 0));
                                LaborAmount = new Money((workOrderEntity.GetAttributeValue<Money>("ap360_totaloriginalestimatedlaboramount") != null ? (workOrderEntity.GetAttributeValue<Money>("ap360_totaloriginalestimatedlaboramount").Value) : 0));
                                SaleTax = new Money((workOrderEntity.GetAttributeValue<Money>("ap360_totaloriginalestimatedpartssaletax") != null ? (workOrderEntity.GetAttributeValue<Money>("ap360_totaloriginalestimatedpartssaletax").Value) : 0));
                                TotalSubletOrginalEstimatedAmount = new Money((workOrderEntity.GetAttributeValue<Money>("ap360_subletoriginalestimatedamount") != null ? (workOrderEntity.GetAttributeValue<Money>("ap360_subletoriginalestimatedamount").Value) : 0));
                                NewWorkOrderEntity["ap360_totaloriginalestimatedamount"] = new Money(partsAmount.Value + LaborAmount.Value + SaleTax.Value + TotalSubletOrginalEstimatedAmount.Value);
                           
                                partsAmount = new Money((workOrderEntity.GetAttributeValue<Money>("ap360_totalactualpartsamount") != null ? (workOrderEntity.GetAttributeValue<Money>("ap360_totalactualpartsamount").Value) : 0));
                                LaborAmount = new Money((workOrderEntity.GetAttributeValue<Money>("ap360_totalactuallaboramount") != null ? (workOrderEntity.GetAttributeValue<Money>("ap360_totalactuallaboramount").Value) : 0));
                                WOSTLaborAmount= new Money((workOrderEntity.GetAttributeValue<Money>("ap360_totalactualwostlaboramount") != null ? (workOrderEntity.GetAttributeValue<Money>("ap360_totalactualwostlaboramount").Value) : 0));
                                SaleTax = new Money((workOrderEntity.GetAttributeValue<Money>("ap360_totalactualpartsaletax") != null ? (workOrderEntity.GetAttributeValue<Money>("ap360_totalactualpartsaletax").Value) : 0));
                                TotalSubletActualAmount = new Money((workOrderEntity.GetAttributeValue<Money>("ap360_subletactualamount") != null ? (workOrderEntity.GetAttributeValue<Money>("ap360_subletactualamount").Value) : 0));
                                NewWorkOrderEntity["ap360_totalactualamount"] = new Money(partsAmount.Value + LaborAmount.Value + SaleTax.Value + TotalSubletActualAmount.Value);
                                NewWorkOrderEntity["ap360_totalwoactualamount"] = new Money(partsAmount.Value + WOSTLaborAmount.Value + SaleTax.Value + TotalSubletActualAmount.Value);

                                //TotalOriginalEstimatedLaborAmount = new Money((workOrderEntity.GetAttributeValue<Money>("ap360_totaloriginalestimatedlaboramount") != null ? (workOrderEntity.GetAttributeValue<Money>("ap360_totaloriginalestimatedlaboramount").Value) : 0));
                                //TotalOriginalEstimatedPartsAmount = new Money((workOrderEntity.GetAttributeValue<Money>("ap360_totaloriginalestimatepartsamount") != null ? (workOrderEntity.GetAttributeValue<Money>("ap360_totaloriginalestimatepartsamount").Value) : 0));
                                //TotalOriginalEstimatedParstsSaleTax = new Money((workOrderEntity.GetAttributeValue<Money>("ap360_totaloriginalestimatedpartssaletax") != null ? (workOrderEntity.GetAttributeValue<Money>("ap360_totaloriginalestimatedpartssaletax").Value) : 0));

                                partsAmount = new Money((workOrderEntity.GetAttributeValue<Money>("ap360_totalrevisedestimatepartsamount") != null ? (workOrderEntity.GetAttributeValue<Money>("ap360_totalrevisedestimatepartsamount").Value) : 0));
                                LaborAmount = new Money((workOrderEntity.GetAttributeValue<Money>("ap360_totalrevisedestimatedlaboramount") != null ? (workOrderEntity.GetAttributeValue<Money>("ap360_totalrevisedestimatedlaboramount").Value) : 0));
                                SaleTax = new Money((workOrderEntity.GetAttributeValue<Money>("ap360_totalrevisedestimatedpartssaletax") != null ? (workOrderEntity.GetAttributeValue<Money>("ap360_totalrevisedestimatedpartssaletax").Value) : 0));
                                TotalSubletRevisedEstimatedAmount = new Money((workOrderEntity.GetAttributeValue<Money>("ap360_subletrevisedestimatedamount") != null ? (workOrderEntity.GetAttributeValue<Money>("ap360_subletrevisedestimatedamount").Value) : 0));
                                NewWorkOrderEntity["ap360_totalrevisedestimatedamount"] = new Money(partsAmount.Value + LaborAmount.Value + SaleTax.Value + TotalSubletRevisedEstimatedAmount.Value);

                                NewWorkOrderEntity.Id = entity.Id;
                                service.Update(NewWorkOrderEntity);

                            }
                        }
                    }
                }


                //if (context.MessageName.ToLower() == "create")
                //{
                //    tracingService.Trace("On create of workOrder");

                //    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                //    {
                //        entity = (Entity)context.InputParameters["Target"];
                //        if (entity.LogicalName == "msdyn_workorder")
                //        {
                //            Entity workOrderEntity = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));

                //            EntityReference serviceRole = workOrderEntity.GetAttributeValue<EntityReference>("ap360_servicerole") != null ? workOrderEntity.GetAttributeValue<EntityReference>("ap360_servicerole") : null;
                //            if (serviceRole == null) return;

                //            decimal serviceRolePrice = BookableResourceBooking.GetRolePrice(service, tracingService, new Guid("4fe16dd5-8e55-ea11-a811-000d3a33f3c3"), serviceRole.Id);


                //            entity["ap360_totaloriginalestimatedpartssaletax"] = new Money((entity.GetAttributeValue<Money>("ap360_totaloriginalestimatepartsamount").Value / 100) * 6);
                //            entity["ap360_totalactualpartsaletax"] = new Money((entity.GetAttributeValue<Money>("ap360_totalactualpartsamount").Value / 100) * 6);
                //            entity["ap360_totalrevisedestimatedpartssaletax"] = new Money((entity.GetAttributeValue<Money>("ap360_totaloriginalestimatepartsamount").Value / 100) * 6);


                //            entity["ap360_totaloriginalestimatedlaboramount"] = new Money(entity.GetAttributeValue<int>("ap360_totaloriginalestimatedduration") * serviceRolePrice);
                //            entity["ap360_totalrevisedestimatedlaboramount"] = new Money(entity.GetAttributeValue<int>("ap360_totaloriginalestimatedduration") * serviceRolePrice);
                //            tracingService.Trace("Updated Estimated duration");
                //            partsAmount = new Money((entity.GetAttributeValue<Money>("ap360_totaloriginalestimatepartsamount") != null ? (entity.GetAttributeValue<Money>("ap360_totaloriginalestimatepartsamount").Value) : 0));
                //            LaborAmount = new Money((entity.GetAttributeValue<Money>("ap360_totaloriginalestimatedlaboramount") != null ? (entity.GetAttributeValue<Money>("ap360_totaloriginalestimatedlaboramount").Value) : 0));
                //            SaleTax = new Money((entity.GetAttributeValue<Money>("ap360_totaloriginalestimatedpartssaletax") != null ? (entity.GetAttributeValue<Money>("ap360_totaloriginalestimatedpartssaletax").Value) : 0));
                //            entity["ap360_totaloriginalestimatedamount"] = new Money(partsAmount.Value + LaborAmount.Value + SaleTax.Value);


                //            partsAmount = new Money((entity.GetAttributeValue<Money>("ap360_totalactualpartsamount") != null ? (entity.GetAttributeValue<Money>("ap360_totalactualpartsamount").Value) : 0));
                //            LaborAmount = new Money((entity.GetAttributeValue<Money>("ap360_totalactuallaboramount") != null ? (entity.GetAttributeValue<Money>("ap360_totalactuallaboramount").Value) : 0));
                //            SaleTax = new Money((entity.GetAttributeValue<Money>("ap360_totalactualpartsaletax") != null ? (entity.GetAttributeValue<Money>("ap360_totalactualpartsaletax").Value) : 0));
                //            entity["ap360_totalactualamount"] = new Money(partsAmount.Value + LaborAmount.Value + SaleTax.Value);


                //            TotalOriginalEstimatedLaborAmount = new Money((entity.GetAttributeValue<Money>("ap360_totaloriginalestimatedlaboramount") != null ? (entity.GetAttributeValue<Money>("ap360_totaloriginalestimatedlaboramount").Value) : 0));
                //            TotalOriginalEstimatedPartsAmount = new Money((entity.GetAttributeValue<Money>("ap360_totaloriginalestimatepartsamount") != null ? (entity.GetAttributeValue<Money>("ap360_totaloriginalestimatepartsamount").Value) : 0));
                //            TotalOriginalEstimatedParstsSaleTax = new Money((entity.GetAttributeValue<Money>("ap360_totaloriginalestimatedpartssaletax") != null ? (entity.GetAttributeValue<Money>("ap360_totaloriginalestimatedpartssaletax").Value) : 0));

                //            partsAmount = new Money((entity.GetAttributeValue<Money>("ap360_totalrevisedestimatepartsamount") != null ? (entity.GetAttributeValue<Money>("ap360_totalrevisedestimatepartsamount").Value) : 0));
                //            LaborAmount = new Money((entity.GetAttributeValue<Money>("ap360_totalrevisedestimatedlaboramount") != null ? (entity.GetAttributeValue<Money>("ap360_totalrevisedestimatedlaboramount").Value) : 0));
                //            SaleTax = new Money((entity.GetAttributeValue<Money>("ap360_totalrevisedestimatedpartssaletax") != null ? (entity.GetAttributeValue<Money>("ap360_totalrevisedestimatedpartssaletax").Value) : 0));
                //            entity["ap360_totalrevisedestimatedamount"] = new Money(partsAmount.Value + LaborAmount.Value + SaleTax.Value + TotalOriginalEstimatedLaborAmount.Value + TotalOriginalEstimatedPartsAmount.Value + TotalOriginalEstimatedParstsSaleTax.Value);



                //            service.Update(entity);
                //        }
                //    }
                //}


            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }
    }
}
