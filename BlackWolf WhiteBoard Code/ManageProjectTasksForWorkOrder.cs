using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
 public   class ManageProjectTasksForWorkOrder : IPlugin
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

                        if (entity.LogicalName == "msdyn_workorder")
                        {
                         //   if (context.Depth > 1) return;
                            tracingService.Trace("Entity is workOrder for Projects tasks");

                            Entity preImage = (Entity)context.PostEntityImages["Image"];

                            if (preImage.Contains("msdyn_opportunityid"))
                            {
                                tracingService.Trace("Inside PreImage of Project");

                                EntityReference opportunityRef = preImage.GetAttributeValue<EntityReference>("msdyn_opportunityid") != null ? preImage.GetAttributeValue<EntityReference>("msdyn_opportunityid") : null;
                                string woNumber = preImage.GetAttributeValue<string>("msdyn_name");
                                string workordername = preImage.GetAttributeValue<string>("ap360_workordername");

                                if (opportunityRef != null)
                                {
                                    tracingService.Trace("Work Order : Project Ref is not null");
                                    Entity opportunityWorkOrderProject = Project.GetWorkOrderProjectofOpportunity(service, tracingService, opportunityRef.Id);
                                    if (opportunityWorkOrderProject != null)
                                    {
                                        EntityReference opportunityWorkOrderProjectRef = new EntityReference(opportunityWorkOrderProject.LogicalName, opportunityWorkOrderProject.Id);
                                      //  ProjectTask.CreateQuoteProjectTask(service, tracingService, workordername+" : "+woNumber, opportunityWorkOrderProjectRef, entity, opportunityRef);


                                        Entity updateWorkOrderProjectField = new Entity(entity.LogicalName, entity.Id);
                                        updateWorkOrderProjectField["ap360_projecttaskid"] = opportunityWorkOrderProjectRef;
                                        service.Update(updateWorkOrderProjectField);
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

                        if (entity.LogicalName == "msdyn_workorder")
                        {
                               if (context.Depth > 1) return;
                            if (entity.Contains("ap360_projecttaskid"))
                            {
                                tracingService.Trace("Entity is wokrOrder");

                                Entity preImage = (Entity)context.PostEntityImages["Image"];

                                if (preImage.Contains("ap360_projectid"))
                                {
                                    tracingService.Trace("Inside PreImage of Project");

                                    // EntityReference workorderRef = preImage.GetAttributeValue<EntityReference>("ap360_workorderid") != null ? preImage.GetAttributeValue<EntityReference>("ap360_workorderid") : null;
                                    EntityReference projectRef = preImage.GetAttributeValue<EntityReference>("ap360_projectid") != null ? preImage.GetAttributeValue<EntityReference>("ap360_projectid") : null;
                                    EntityReference opportunityRef = preImage.GetAttributeValue<EntityReference>("msdyn_opportunityid") != null ? preImage.GetAttributeValue<EntityReference>("msdyn_opportunityid") : null;
                                    string name = preImage.GetAttributeValue<string>("ap360_workordername");

                                    if (projectRef != null)
                                    {
                                        tracingService.Trace("Quote : Project Ref is not null");

                                       // ProjectTask.CreateQuoteProjectTask(service, tracingService, name, projectRef, entity, opportunityRef);

                                    }
                                }

                            }
                            if (entity.Contains("ap360_workordername"))
                            {
                                if (context.Depth > 1) return;
                                Entity preImage = (Entity)context.PostEntityImages["Image"];

                                if (preImage.Contains("ap360_workordername"))
                                {
                                    ProjectTask quoteProjectTask = new ProjectTask();
                                    //throw new InvalidPluginExecutionException("Custom Erro ");
                                    quoteProjectTask = ProjectTask.GetProjectTaskforWorkOrderEntities(service, tracingService, "msdyn_projecttask", "ap360_workorderid", entity.Id, "msdyn_workorder");
                                    if (quoteProjectTask.Quote != null)
                                    {
                                        tracingService.Trace("Task : " + quoteProjectTask.TaskSubject);
                                        // throw new InvalidPluginExecutionException("custom erro ");
                                        Entity updateProjectTaskName = new Entity("msdyn_projecttask", quoteProjectTask.guid);
                                        updateProjectTaskName["msdyn_subject"] = preImage["ap360_workordername"];
                                        service.Update(updateProjectTaskName);


                                    }

                                }
                            }
                        }
                    }
                }
                if (context.MessageName.ToLower() == "delete")
                {
                    tracingService.Trace("Inside of workorder delete ");

                    EntityReference TargetRef = (EntityReference)context.InputParameters["Target"];
                    if (TargetRef.LogicalName.ToLower() == "msdyn_workorder")
                    {
                        tracingService.Trace("refernce of workorder is not null");
                        ProjectTask projectTask = new ProjectTask();
                        projectTask = ProjectTask.GetProjectTaskRelatedToWorkOrder(service, tracingService, TargetRef.Id);
                        if (projectTask != null)
                        {
                            tracingService.Trace("Project task is not null");
                            service.Delete("msdyn_projecttask", projectTask.guid);
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