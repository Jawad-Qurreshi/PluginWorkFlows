using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class ManageRevisedItem : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();
            //   throw new InvalidPluginExecutionException("CreateProductFromDescriptionAndRelatetoProductFamily");

            try
            {

                ITracingService tracingService =
                  (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                IPluginExecutionContext context = (IPluginExecutionContext)
                    serviceProvider.GetService(typeof(IPluginExecutionContext));

                IOrganizationServiceFactory factory =
                     (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = factory.CreateOrganizationService(context.UserId);


                Entity entity = null;
                                   // throw new InvalidPluginExecutionException("custom error");

                if (context.MessageName.ToLower() == "create")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        entity = (Entity)context.InputParameters["Target"];
                        if (entity.LogicalName == "msdyn_workorderservicetask" || entity.LogicalName == "msdyn_workorderproduct" || entity.LogicalName == "ap360_workordersublet")
                        {
                            // throw new InvalidPluginExecutionException("Error");
                            tracingService.Trace(entity.LogicalName);
                            Microsoft.Xrm.Sdk.Query.ColumnSet columset = null;

                            if (entity.LogicalName == "msdyn_workorderservicetask")
                            {
                                if (context.Depth > 1) return;
                                columset = new Microsoft.Xrm.Sdk.Query.ColumnSet("msdyn_name", "ap360_revisedestimatedduration", "ap360_revisedestimatedamount", "msdyn_workorder", "ap360_opportunityid", "ap360_isrevised","ap360_reviseditemstatus");
                            }
                            else if (entity.LogicalName == "msdyn_workorderproduct")
                            {
                                if (context.Depth > 2) return;// for cores depth 2 is required
                                tracingService.Trace("Work Order Product "+context.Depth.ToString());
                                columset = new Microsoft.Xrm.Sdk.Query.ColumnSet("ap360_name", "msdyn_estimatequantity", "ap360_product", "msdyn_estimateunitamount", "ap360_revisedestimateamount", "msdyn_workorder", "ap360_opportunityid", "ap360_isrevised", "ap360_reviseditemstatus");
                            }
                            else if (entity.LogicalName == "ap360_workordersublet")
                            {
                                if (context.Depth > 1) return;
                                columset = new Microsoft.Xrm.Sdk.Query.ColumnSet("ap360_subletdescription", "ap360_revisedestimatedamount", "ap360_workorderid", "ap360_opportunityid", "ap360_isrevised", "ap360_reviseditemstatus");
                            }
                            // throw new InvalidPluginExecutionException("updated");

                            //if (columset == null) return;
                            tracingService.Trace("column set not null");

                            Entity reterviedEntity = service.Retrieve(entity.LogicalName, entity.Id, columset);
                            bool isReterivedEntityIsRevised = false;
                            if (reterviedEntity != null)
                            {
                                tracingService.Trace("retervied Entity not null");
                                RevisedItem revisedItem = new RevisedItem();
                                if (reterviedEntity.LogicalName == "msdyn_workorderservicetask")
                                {
                                    tracingService.Trace(" inside workorderservicetask");
                                    isReterivedEntityIsRevised = reterviedEntity.GetAttributeValue<Boolean>("ap360_isrevised");
                                    if (isReterivedEntityIsRevised)
                                    {

                                        WorkOrderServiceTask.mapWOServiceTasktoRevisedItem(service, reterviedEntity, revisedItem);
                                    }
                                }
                                else if (reterviedEntity.LogicalName == "msdyn_workorderproduct")
                                {
                                    tracingService.Trace(" inside workorderproduct");

                                    isReterivedEntityIsRevised = reterviedEntity.GetAttributeValue<Boolean>("ap360_isrevised");
                                    if (isReterivedEntityIsRevised)
                                    {

                                        tracingService.Trace("Work ORder Prodcut is revised");

                                        WorkOrderProduct.mapWOProducttoRevisedItem(service, reterviedEntity, revisedItem , tracingService);
                                        tracingService.Trace("Work ORder Prodcut is revised after");
                                    }
                                }
                                else if (reterviedEntity.LogicalName == "ap360_workordersublet")
                                {
                                    tracingService.Trace(" inside workordersublet");
                                    isReterivedEntityIsRevised = reterviedEntity.GetAttributeValue<Boolean>("ap360_isrevised");
                                    if (isReterivedEntityIsRevised)
                                    {
                                        tracingService.Trace("WO Sublet is revised");

                                        WorkOrderSublet.mapWOSubletToRevisedItem(service, reterviedEntity, revisedItem);
                                    }
                                }

                                if (isReterivedEntityIsRevised)
                                {

                                    tracingService.Trace("before Revised Item creation");
                                 //   throw new InvalidPluginExecutionException("before retervied entity nt null");

                                    RevisedItem.CreateRevisedItem(service, tracingService, revisedItem);
                                    tracingService.Trace("After Revised Item Creation");
                                }
                                //  throw new InvalidPluginExecutionException("End Error");
                            }


                        }
                    }
                }

                if (context.MessageName.ToLower() == "delete")
                {
                    tracingService.Trace("delete");
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference)
                    {

                        EntityReference entityref = (EntityReference)context.InputParameters["Target"];
                        RevisedItem reterivedRevisedItem = null;
                        if (entityref.LogicalName == "msdyn_workorderservicetask" || entityref.LogicalName == "msdyn_workorderproduct" || entityref.LogicalName == "ap360_workordersublet")
                        {
                            tracingService.Trace(entityref.LogicalName);

                            Entity reterivedEntity = service.Retrieve(entityref.LogicalName, entityref.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("ap360_opportunityid"));
                            if (reterivedEntity != null)
                            {
                                //if (reterivedEntity.LogicalName == "msdyn_workorderservicetask")
                                //{

                                //    reterivedRevisedItem = RevisedItem.GetRevisedItem(service, tracingService, reterivedEntity);
                                //}
                                //else if (reterivedEntity.LogicalName == "msdyn_workorderproduct")
                                //{
                                //    reterivedRevisedItem = RevisedItem.GetRevisedItem(service, tracingService, reterivedEntity);

                                //}
                                //else if (reterivedEntity.LogicalName == "ap360_workordersublet")
                                //{
                                tracingService.Trace("Before Get Revised Item");
                                reterivedRevisedItem = RevisedItem.GetRevisedItem(service, tracingService, reterivedEntity);
                                tracingService.Trace("After Get Revised Item");
                                // }

                                if (reterivedRevisedItem != null)

                                {

                                    tracingService.Trace("reterived Revised Item is not null " + reterivedRevisedItem.guid);

                                    service.Delete("ap360_reviseditem", reterivedRevisedItem.guid);
                                }

                                //throw new InvalidPluginExecutionException("updated Custom Error");
                            }

                        }

                    }
                }

                if (context.MessageName.ToLower() == "update")
                {

                    tracingService.Trace("Inside Update ");
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        if (context.Depth > 1) return;
                        entity = (Entity)context.InputParameters["Target"];

                        if (entity.LogicalName == "msdyn_workorderservicetask" || entity.LogicalName == "msdyn_workorderproduct" || entity.LogicalName == "ap360_workordersublet")
                        {

                            if (entity.LogicalName == "msdyn_workorderservicetask")
                            {
                                tracingService.Trace("inside msdyn_workorderservicetask");

                                if ((entity.Contains("msdyn_name")) || (entity.Contains("ap360_reviseditemstatus")) || (entity.Contains("ap360_revisedestimatedduration")) || (entity.Contains("ap360_revisedestimatedamount")))
                                {
                                    tracingService.Trace("updated ");

                                    Guid revisedItemGuid = RevisedItem.getWOSTRelatedRevisedItemGuid(service, tracingService, entity.Id);
                                    if (revisedItemGuid != Guid.Empty)
                                    {
                                        tracingService.Trace("Guid is reterived " + revisedItemGuid.ToString());
                                        RevisedItem.updateRevisedItem(service, tracingService, entity, revisedItemGuid);
                                        tracingService.Trace("End");
                                    }
                                    //else
                                    //{
                                    //    tracingService.Trace("before Revised Item creation");
                                    //    RevisedItem.CreateRevisedItem(service, tracingService, revisedItem);
                                    //    tracingService.Trace("After Revised Item Creation");
                                    //}
                                }
                                //  throw new InvalidPluginExecutionException("Error");
                            }
                            else if (entity.LogicalName == "msdyn_workorderproduct")
                            {
                                tracingService.Trace("msdyn_workorderproduct");
                                if ((entity.Contains("ap360_name")) || (entity.Contains("ap360_reviseditemstatus")) || (entity.Contains("msdyn_estimatequantity")) || (entity.Contains("ap360_core") || (entity.Contains("msdyn_estimateunitamount")) || (entity.Contains("ap360_revisedestimateamount"))))
                                {
                                    tracingService.Trace("updated ");

                                    Guid revisedItemGuid = RevisedItem.getWOProductRelatedRevisedItemGuid(service, tracingService, entity.Id);

                                    if (revisedItemGuid != Guid.Empty)
                                    {
                                        tracingService.Trace("Guid is reterived " + revisedItemGuid.ToString());
                                        RevisedItem.updateRevisedItem(service, tracingService, entity, revisedItemGuid);
                                        tracingService.Trace("End");
                                    }

                                }
                            }

                            else if (entity.LogicalName == "ap360_workordersublet")
                            {
                                tracingService.Trace("ap360_workordersublet");

                                if ((entity.Contains("ap360_subletdescription")) || (entity.Contains("ap360_revisedestimatedamount")) || (entity.Contains("ap360_reviseditemstatus")))
                                {
                                    tracingService.Trace("updated ");

                                    Guid revisedItemGuid = RevisedItem.getWOSubletRelatedRevisedItemGuid(service, tracingService, entity.Id);
                                    if (revisedItemGuid != Guid.Empty)
                                    {
                                        tracingService.Trace("Guid is reterived " + revisedItemGuid.ToString());
                                        RevisedItem.updateRevisedItem(service, tracingService, entity, revisedItemGuid);
                                        tracingService.Trace("End");
                                    }

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