using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class ManageProjectTasksForworkorderservicetaskServiceTask : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
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


                if (context.MessageName.ToLower() == "create")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        entity = (Entity)context.InputParameters["Target"];

                        if (entity.LogicalName.ToLower() == "msdyn_workorderservicetask")
                        {
                            //  if (context.Depth > 1) return;
                            tracingService.Trace("Entity is workorderservicetask for Projects tasks");

                            Entity postImage = (Entity)context.PostEntityImages["Image"];

                            if (postImage.Contains("msdyn_workorder"))
                            {
                                tracingService.Trace("Inside PreImage of Project");

                                EntityReference workorderRef = postImage.GetAttributeValue<EntityReference>("msdyn_workorder") != null ? postImage.GetAttributeValue<EntityReference>("msdyn_workorder") : null;
                                EntityReference parentWOSTRef = postImage.GetAttributeValue<EntityReference>("ap360_workorderservicetaskid") != null ? postImage.GetAttributeValue<EntityReference>("ap360_workorderservicetaskid") : null;
                                string name = postImage.GetAttributeValue<string>("msdyn_description");
                                EntityReference projectRef = postImage.GetAttributeValue<EntityReference>("ap360_projectid");


                                int estimatedDuration = 0;
                                Money estimatedAmount = null;

                                bool isrevised = postImage.GetAttributeValue<bool>("ap360_isrevised");

                                if (!isrevised)
                                {

                                    estimatedDuration = postImage.GetAttributeValue<int>("msdyn_estimatedduration");
                                    estimatedAmount  = postImage.GetAttributeValue<Money>("ap360_originalestimatedamount")!=null? postImage.GetAttributeValue<Money>("ap360_originalestimatedamount"):null;
                                }
                                else
                                {
                                    estimatedDuration = postImage.GetAttributeValue<int>("ap360_revisedestimatedduration");
                                    estimatedAmount = postImage.GetAttributeValue<Money>("ap360_revisedestimatedamount") != null ? postImage.GetAttributeValue<Money>("ap360_revisedestimatedamount") : null;
                                }

                                //                                msdyn_plannedcost_base Estimated cost
                                //msdyn_plannedcost       Planned cost
                                //msdyn_effort Estimated Effort
                                //msdyn_actualcost                Actual Cost
                                //msdyn_actualeffort Actual Hours

                                //msdyn_plannedlaborcost

                                //    throw new InvalidPluginExecutionException("Amoun "+ estimatedAmount.Value.ToString() +"Est Cost "+ estimatedDuration.ToString());
                                if (parentWOSTRef == null)
                                {
                                    //throw new InvalidPluginExecutionException("Inside if");
                                    if (workorderRef != null)
                                    {
                                        tracingService.Trace("Work ORder : WO Ref is not null");

                                        ProjectTask workorderProjectTask = new ProjectTask();
                                        workorderProjectTask = ProjectTask.GetProjectTaskforWorkOrderEntities(service, tracingService, "msdyn_projecttask", "ap360_workorderid", workorderRef.Id, "msdyn_workorderservicetask");
                                        if (workorderProjectTask != null)
                                        {
                                            tracingService.Trace("Work ORder Project task " + workorderProjectTask.guid.ToString());
                                            if (projectRef != null)
                                            {


                                                tracingService.Trace("Quote Service: Project Ref is not null");
                                                EntityReference workorderProjectTaskRef = new EntityReference("msdyn_projecttask", workorderProjectTask.guid);
                                                ProjectTask.CreateIndividualWOSTProjectTask(service, tracingService, name, projectRef, workorderProjectTaskRef, entity, workorderRef, estimatedDuration, estimatedAmount);

                                            }
                                        }
                                    }
                                }

                                else if (parentWOSTRef != null)
                                {
                                    tracingService.Trace("Work ORder : workorderservicetask Ref is not null");
                                    
                                    ProjectTask wOSTProjectTask = new ProjectTask();
                                    wOSTProjectTask = ProjectTask.GetProjectTaskforWorkOrderEntities(service, tracingService, "msdyn_projecttask", "ap360_workorderservicetaskid", parentWOSTRef.Id, "msdyn_workorderservicetask");
                                    if (wOSTProjectTask != null)
                                    {
                                        tracingService.Trace("Work ORder Project task " + wOSTProjectTask.guid.ToString());
                                        if (projectRef != null)
                                        {


                                            tracingService.Trace("Quote Service: Project Ref is not null");
                                            EntityReference wostProjectRef = new EntityReference("msdyn_projecttask", wOSTProjectTask.guid);
                                    //throw new InvalidPluginExecutionException("work order service task " + wostProjectRef.LogicalName.ToString());
                                            ProjectTask.CreateIndividualWOSTProjectTask(service, tracingService, name, projectRef, wostProjectRef, entity, parentWOSTRef, estimatedDuration, estimatedAmount);
                                        }
                                    }
                                }
                            }
                        }
                    }

                }

                if (context.MessageName.ToLower() == "update")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        entity = (Entity)context.InputParameters["Target"];

                        if (entity.LogicalName == "msdyn_workorderservicetask")
                        {
                            if (context.Depth > 1) return;
                            if (entity.Contains("ap360_projecttaskid"))
                            {
                                tracingService.Trace("Entity is wokrOrder srv task");

                                Entity preImage = (Entity)context.PostEntityImages["Image"];

                                if (preImage.Contains("ap360_projectid"))
                                {
                                    tracingService.Trace("Inside PreImage of Project");

                                    // EntityReference workorderservicetaskRef = preImage.GetAttributeValue<EntityReference>("ap360_workorderservicetaskid") != null ? preImage.GetAttributeValue<EntityReference>("ap360_workorderservicetaskid") : null;
                                    EntityReference projectRef = preImage.GetAttributeValue<EntityReference>("ap360_projectid") != null ? preImage.GetAttributeValue<EntityReference>("ap360_projectid") : null;
                                    EntityReference opportunityRef = preImage.GetAttributeValue<EntityReference>("ap360_opportunityid") != null ? preImage.GetAttributeValue<EntityReference>("ap360_opportunityid") : null;
                                    string name = preImage.GetAttributeValue<string>("ap360_workorderservicetaskname");

                                    if (projectRef != null)
                                    {
                                        tracingService.Trace("Quote : Project Ref is not null");

                                        //   ProjectTask.CreateQuoteProjectTask(service, tracingService, name, projectRef, entity, opportunityRef);

                                    }
                                }

                            }
                            //msdyn_actualduration
                            //ap360_actualamount
                            if (entity.Contains("msdyn_description"))
                            {
                                // if (context.Depth > 1) return;
                                Entity postImage = (Entity)context.PostEntityImages["Image"];

                                if (postImage.Contains("msdyn_description"))
                                {
                                    ProjectTask workOrderServiceTask = new ProjectTask();
                                    //throw new InvalidPluginExecutionException("Custom Erro ");
                                    workOrderServiceTask = ProjectTask.GetProjectTaskforWorkOrderEntities(service, tracingService, "msdyn_projecttask", "ap360_workorderservicetaskid", entity.Id, "msdyn_workorderservicetask");
                                    if (workOrderServiceTask != null)
                                    {
                                        if (workOrderServiceTask.WorkOrderServiceTask != null)
                                        {
                                            tracingService.Trace("Task : " + workOrderServiceTask.TaskSubject);
                                            // throw new InvalidPluginExecutionException("custom erro ");
                                            Entity updateProjectTaskName = new Entity("msdyn_projecttask", workOrderServiceTask.guid);
                                            updateProjectTaskName["msdyn_subject"] = postImage["msdyn_description"];
                                            service.Update(updateProjectTaskName);


                                        }
                                    }

                                }
                            }
                        }
                    }
                }
                if (context.MessageName.ToLower() == "delete")
                {
                    tracingService.Trace("Inside of work Order service task delete ");

                    EntityReference TargetRef = (EntityReference)context.InputParameters["Target"];
                    if (TargetRef.LogicalName.ToLower() == "msdyn_workorderservicetask")
                    {

                        ProjectTask deletingProjectTask = new ProjectTask();
                        ProjectTask deletingChildWOSTProjectTask = new ProjectTask();
                        deletingProjectTask = ProjectTask.GetProjectTaskforWorkOrderEntities(service, tracingService, "msdyn_projecttask", "ap360_workorderservicetaskid", TargetRef.Id, TargetRef.LogicalName);
                        deletingChildWOSTProjectTask = ProjectTask.GetProjectTaskforWorkOrderEntities(service, tracingService, "msdyn_projecttask", "ap360_childworkorderservicetaskid", TargetRef.Id, TargetRef.LogicalName);
                        if (deletingProjectTask != null)
                        {
                            service.Delete("msdyn_projecttask", deletingProjectTask.guid);
                            if (deletingChildWOSTProjectTask != null)
                                service.Delete("msdyn_projecttask", deletingChildWOSTProjectTask.guid);
                        }
                        else if (deletingChildWOSTProjectTask != null)
                        {
                            service.Delete("msdyn_projecttask", deletingChildWOSTProjectTask.guid);
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