using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class ManageWBSIDForQuoteEntities : IPlugin
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


                //msdyn_MoveDownWBSTask
                //msdyn_MoveUpWBSTask
                //msdyn_OutdentWBSTask

                //msdyn_IndentWBSTask
                //msdyn_BulkDeletePredecessorsForTask
                //msdyn_BulkCreatePredecessorsForTask

                if (context.MessageName.ToLower() == "update")
                {
                   // if (context.Depth > 1) return;
                    // throw new InvalidPluginExecutionException("Custome Errir");
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {

                        entity = (Entity)context.InputParameters["Target"];

                        if (entity.LogicalName == "msdyn_projecttask")
                        {
                            if (entity.Contains("msdyn_wbsid"))
                            {

                                Entity postImage = (Entity)context.PostEntityImages["Image"];

                                if (postImage.Contains("ap360_quoteservicetaskid"))
                                {
                                    tracingService.Trace("project task updated");

                                    tracingService.Trace("Wbs updated");
                                    tracingService.Trace("ap360_quoteservicetaskid");

                                    EntityReference quoteservicetaskRef = postImage.GetAttributeValue<EntityReference>("ap360_quoteservicetaskid") != null ? postImage.GetAttributeValue<EntityReference>("ap360_quoteservicetaskid") : null;
                                    if (quoteservicetaskRef != null)
                                    {
                                        tracingService.Trace("ap360_quoteservicetask Ref is not null");

                                        string wbsId = postImage.GetAttributeValue<string>("msdyn_wbsid");
                                        tracingService.Trace(wbsId);
                                        Entity updateQuoteServiceTask = new Entity(quoteservicetaskRef.LogicalName, quoteservicetaskRef.Id);
                                        tracingService.Trace(updateQuoteServiceTask.LogicalName);
                                        int lastIndexofDot = wbsId.LastIndexOf(".");
                                        if (lastIndexofDot > 0)
                                        {
                                            wbsId = wbsId.Substring(lastIndexofDot + 1);
                                            tracingService.Trace("Updated WBS ID " + wbsId);
                                        }
                                            updateQuoteServiceTask["ap360_wbsid"] = Convert.ToInt32(wbsId);
                                        service.Update(updateQuoteServiceTask);
                                    }
                                }
                                else if (postImage.Contains("ap360_quoteserviceid"))
                                {
                                    tracingService.Trace("ap360_quoteserviceid");

                                    EntityReference quoteserviceRef = postImage.GetAttributeValue<EntityReference>("ap360_quoteserviceid") != null ? postImage.GetAttributeValue<EntityReference>("ap360_quoteserviceid") : null;
                                    if (quoteserviceRef != null)
                                    {
                                        tracingService.Trace("quoteService Ref is not null");

                                        string wbsId = postImage.GetAttributeValue<string>("msdyn_wbsid");
                                        Entity updateQuoteServiceTask = new Entity(quoteserviceRef.LogicalName, quoteserviceRef.Id);
                                        tracingService.Trace(wbsId);
                                        int firstIndexofDot = wbsId.IndexOf(".");
                                        wbsId = wbsId.Substring(firstIndexofDot + 1);
                                        tracingService.Trace("Update WBS Id : "+ wbsId);
                                        firstIndexofDot = wbsId.IndexOf(".");
                                        if (firstIndexofDot > 0)
                                        {
                                            wbsId = wbsId.Substring(0, firstIndexofDot);
                                            tracingService.Trace("Inside Update WBS Id : " + wbsId);
                                        }
                                        updateQuoteServiceTask["ap360_wbsid"] = Convert.ToInt32(wbsId);
                                        service.Update(updateQuoteServiceTask);
                                    }
                                }

                                else if (postImage.Contains("ap360_quoteid"))
                                {
                                    tracingService.Trace("ap360_quoteid");

                                    EntityReference quoteRef = postImage.GetAttributeValue<EntityReference>("ap360_quoteid") != null ? postImage.GetAttributeValue<EntityReference>("ap360_quoteid") : null;
                                    if (quoteRef != null)
                                    {
                                        tracingService.Trace("quoteRef not null");

                                        string wbsId = postImage.GetAttributeValue<string>("msdyn_wbsid");
                                        Entity updateQuoteServiceTask = new Entity(quoteRef.LogicalName, quoteRef.Id);
                                        // wbsId = wbsId[0].ToString();
                                        tracingService.Trace(wbsId);
                                        int firstIndexofDot = wbsId.IndexOf(".");
                                        if (firstIndexofDot > 0)
                                        {
                                            wbsId = wbsId.Substring(0, firstIndexofDot);
                                            tracingService.Trace("Inside WBSID : " + wbsId);
                                        }
                                        updateQuoteServiceTask["ap360_wbsid"] = Convert.ToInt32(wbsId);
                                        service.Update(updateQuoteServiceTask);
                                    }


                                }
                                else if (postImage.Contains("ap360_workorderid"))
                                {
                                    tracingService.Trace("ap360_workorderid");

                                    EntityReference workorderservicetaskRef = postImage.GetAttributeValue<EntityReference>("ap360_workorderid") != null ? postImage.GetAttributeValue<EntityReference>("ap360_workorderid") : null;
                                    if (workorderservicetaskRef != null)
                                    {


                                        string wbsId = postImage.GetAttributeValue<string>("msdyn_wbsid");
                                        Entity updateWOTask = new Entity(workorderservicetaskRef.LogicalName, workorderservicetaskRef.Id);
                                        tracingService.Trace(wbsId);
                                        int firstIndexofDot = wbsId.IndexOf(".");
                                        if (firstIndexofDot > 0)
                                        {
                                            wbsId = wbsId.Substring(0, firstIndexofDot);
                                            tracingService.Trace("Inside WBSID : " + wbsId);
                                        }
                                        updateWOTask["ap360_wbsid"] = Convert.ToInt32(wbsId);
                                        service.Update(updateWOTask);
                                    }
                                }
                                else if (postImage.Contains("ap360_workorderservicetaskid"))
                                {
                                    tracingService.Trace("ap360_workorderservicetaskid");

                                    EntityReference workorderservicetaskRef = postImage.GetAttributeValue<EntityReference>("ap360_workorderservicetaskid") != null ? postImage.GetAttributeValue<EntityReference>("ap360_workorderservicetaskid") : null;
                                    if (workorderservicetaskRef != null)
                                    {


                                        string wbsId = postImage.GetAttributeValue<string>("msdyn_wbsid");
                                        Entity updateQuoteServiceTask = new Entity(workorderservicetaskRef.LogicalName, workorderservicetaskRef.Id);
                                        tracingService.Trace(wbsId);
                                        int firstIndexofDot = wbsId.IndexOf(".");
                                        wbsId = wbsId.Substring(firstIndexofDot + 1);
                                        tracingService.Trace("Update WBS Id : " + wbsId);
                                        firstIndexofDot = wbsId.IndexOf(".");
                                        if (firstIndexofDot > 0)
                                        {
                                            wbsId = wbsId.Substring(0, firstIndexofDot);
                                            tracingService.Trace("Inside Update WBS Id : " + wbsId);
                                        }
                                        updateQuoteServiceTask["ap360_wbsid"] = Convert.ToInt32(wbsId);
                                        service.Update(updateQuoteServiceTask);
                                    }


                                }
                                else if (postImage.Contains("ap360_childworkorderservicetaskid"))
                                {
                                    tracingService.Trace("project task updated");

                                    tracingService.Trace("Wbs updated");
                                    tracingService.Trace("ap360_childworkorderservicetaskid");

                                    EntityReference childWOSTRef = postImage.GetAttributeValue<EntityReference>("ap360_childworkorderservicetaskid") ?? null;
                                    if (childWOSTRef != null)
                                    {
                                        tracingService.Trace("child WO service task Ref is not null");

                                        string wbsId = postImage.GetAttributeValue<string>("msdyn_wbsid");
                                        tracingService.Trace(wbsId);
                                        Entity updateChildWOST = new Entity(childWOSTRef.LogicalName, childWOSTRef.Id);
                                        tracingService.Trace(updateChildWOST.LogicalName);
                                        int lastIndexofDot = wbsId.LastIndexOf(".");
                                        if (lastIndexofDot > 0)
                                        {
                                            wbsId = wbsId.Substring(lastIndexofDot + 1);
                                            tracingService.Trace("Updated WBS ID " + wbsId);
                                        }
                                        updateChildWOST["ap360_wbsid"] = Convert.ToInt32(wbsId);
                                        service.Update(updateChildWOST);
                                    }
                                }
                            }
                            // When Quote Items SWAP between Quotes
                            if (entity.Contains("msdyn_parenttask"))
                            {
                                tracingService.Trace("Parent Task is updated");
                                tracingService.Trace("Context Depth  is " + context.Depth.ToString());
                                tracingService.Trace("*************************");
                                Entity postImage = (Entity)context.PostEntityImages["Image"];
                                EntityReference quoteChildEntityRef = null;
                                if (postImage.GetAttributeValue<EntityReference>("ap360_quoteserviceid") != null)
                                {
                                    quoteChildEntityRef = postImage.GetAttributeValue<EntityReference>("ap360_quoteserviceid");
                                    tracingService.Trace("quote service is not null");

                                }
                                else if (postImage.GetAttributeValue<EntityReference>("ap360_quoteservicetaskid") != null)
                                {
                                    quoteChildEntityRef = postImage.GetAttributeValue<EntityReference>("ap360_quoteservicetaskid");
                                    tracingService.Trace("quote service task is not null");
                                }

                                if (quoteChildEntityRef != null)
                                {
                                    //throw new InvalidPluginExecutionException("Entity Ref " + quoteChildEntityRef.LogicalName + " context Depth " + context.Depth.ToString());
                                    if (context.Depth == 2)
                                    {

                                        if (quoteChildEntityRef.LogicalName == "ap360_quoteservice")
                                        {
                                            tracingService.Trace("QUOTE SERVICE SWAPPING ");
                                            EntityReference currentTaskparentTask = entity.GetAttributeValue<EntityReference>("msdyn_parenttask") != null ? entity.GetAttributeValue<EntityReference>("msdyn_parenttask") : null;

                                            if (currentTaskparentTask != null)
                                            {
                                                tracingService.Trace("Current Parent Task exists --" + currentTaskparentTask.Id.ToString() + " " + currentTaskparentTask.Name);
                                                Entity currentParentTaskEntity = service.Retrieve(currentTaskparentTask.LogicalName, currentTaskparentTask.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("msdyn_parenttask", "msdyn_wbsid", "msdyn_subject"));

                                                if (currentParentTaskEntity != null)
                                                {
                                                    tracingService.Trace("Current Parent Entity exists ");
                                                    ProjectTask.updateQSLevelCurrentItemIdsinSequence(service, tracingService, currentParentTaskEntity, entity);

                                                }
                                            }

                                            tracingService.Trace("*************************");

                                            Entity preImage = (Entity)context.PreEntityImages["Image"];
                                            EntityReference previousTaskparentTask = null;
                                            if (preImage.Contains("msdyn_parenttask"))
                                            {
                                                tracingService.Trace("Pre Image contains parent task");
                                                previousTaskparentTask = preImage.GetAttributeValue<EntityReference>("msdyn_parenttask") != null ? preImage.GetAttributeValue<EntityReference>("msdyn_parenttask") : null;
                                            }
                                            if (previousTaskparentTask != null)
                                            {
                                                tracingService.Trace("Previous Parent Task exists -- " + previousTaskparentTask.Id.ToString() + " " + previousTaskparentTask.Name);

                                                Entity previousParentTaskEntity = service.Retrieve(previousTaskparentTask.LogicalName, previousTaskparentTask.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("msdyn_parenttask", "msdyn_wbsid", "msdyn_subject"));
                                                if (previousParentTaskEntity != null)
                                                {
                                                    tracingService.Trace("Previous Parent Entity exists ");
                                                    ProjectTask.updateQSLevelPreviousItemIdsinSequence(service, tracingService, previousParentTaskEntity, entity);

                                                }
                                            }


                                        }
                                        else if (quoteChildEntityRef.LogicalName == "ap360_quoteservicetask")
                                        {
                                            //throw new InvalidPluginExecutionException("Dept " + context.Depth.ToString());
                                            tracingService.Trace("QUOTE SERVICE TASK SWAPPING "+quoteChildEntityRef.Name);
                                            EntityReference currentTaskparentTask = entity.GetAttributeValue<EntityReference>("msdyn_parenttask") != null ? entity.GetAttributeValue<EntityReference>("msdyn_parenttask") : null;

                                            if (currentTaskparentTask != null)
                                            {
                                                tracingService.Trace("Current Parent Task exists --" + currentTaskparentTask.Id.ToString() + " "+currentTaskparentTask.Name);
                                                Entity currentParentTaskEntity = service.Retrieve(currentTaskparentTask.LogicalName, currentTaskparentTask.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("msdyn_parenttask", "msdyn_wbsid", "msdyn_subject"));

                                                if (currentParentTaskEntity != null)
                                                {
                                                    tracingService.Trace("Current Parent Entity exists ");
                                                    ProjectTask.updateQSTLevelCurrentItemIdsinSequence(service, tracingService, currentParentTaskEntity, entity);

                                                }
                                            }

                                            tracingService.Trace("*************************");

                                            Entity preImage = (Entity)context.PreEntityImages["Image"];
                                            EntityReference previousTaskparentTask = null;
                                            if (preImage.Contains("msdyn_parenttask"))
                                            {
                                                tracingService.Trace("Pre Image contains parent task");
                                                previousTaskparentTask = preImage.GetAttributeValue<EntityReference>("msdyn_parenttask") != null ? preImage.GetAttributeValue<EntityReference>("msdyn_parenttask") : null;
                                            }
                                            if (previousTaskparentTask != null)
                                            {
                                                tracingService.Trace("Previous Parent Task exists -- " + previousTaskparentTask.Id.ToString() + " " + previousTaskparentTask.Name);

                                                Entity previousParentTaskEntity = service.Retrieve(previousTaskparentTask.LogicalName, previousTaskparentTask.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("msdyn_parenttask", "msdyn_wbsid", "msdyn_subject"));
                                                if (previousParentTaskEntity != null)
                                                {
                                                    tracingService.Trace("Previous Parent Entity exists ");
                                                    ProjectTask.updateQSTLevelPreviousItemIdsinSequence(service, tracingService, previousParentTaskEntity, entity);

                                                }
                                            }

                                         //   throw new InvalidPluginExecutionException("Dept " + context.Depth.ToString());

                                        }
                                    }
                                }

                                tracingService.Trace("Function End");
                            }

                        }
                    }
                }
                //throw new InvalidPluginExecutionException("msdyn_movedownwbstask");

                if (context.MessageName.ToLower() == "msdyn_moveupwbstask")
                {
                    throw new InvalidPluginExecutionException("msdyn_moveupwbstask");
                }
                if (context.MessageName.ToLower() == "msdyn_outdentwbstask")
                {
                    throw new InvalidPluginExecutionException("msdyn_OutdentWBSTask");

                }
                if (context.MessageName.ToLower() == "msdyn_indentwbstask")
                {
                    throw new InvalidPluginExecutionException("msdyn_IndentWBSTask");

                }
                if (context.MessageName.ToLower() == "msdyn_bulkdeletepredecessorsfortask")
                {
                    throw new InvalidPluginExecutionException("msdyn_BulkDeletePredecessorsForTask");

                }
                if (context.MessageName.ToLower() == "msdyn_bulkcreatepredecessorsfortask")
                {
                    throw new InvalidPluginExecutionException("msdyn_BulkCreatePredecessorsForTask");

                }


                if (context.MessageName.ToLower() == "update")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        if (context.Depth == 1)
                        {
                            entity = (Entity)context.InputParameters["Target"];

                            if (entity.LogicalName == "ap360_quoteproduct" || entity.LogicalName == "msdyn_incidenttypeproduct" || entity.LogicalName == "msdyn_workorderproduct")
                            {
                                tracingService.Trace("Entity Name " + entity.LogicalName);

                                if (entity.Contains("ap360_approveproduct"))
                                {
                                    tracingService.Trace("Approve Product Updated");
                                    tracingService.Trace("Plugin Ended Successfully");

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