using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class ManageProjectTasksForQuoteServiceTask : IPlugin
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

                        if (entity.LogicalName == "ap360_quoteservicetask")
                        {

                        //    if (context.Depth > 1) return;

                            Entity postImage = (Entity)context.PostEntityImages["Image"];

                            if (postImage.Contains("ap360_quoteserviceid"))
                            {
                                tracingService.Trace("Inside PreImage of Project");

                                EntityReference quoteServiceRef = postImage.GetAttributeValue<EntityReference>("ap360_quoteserviceid") != null ? postImage.GetAttributeValue<EntityReference>("ap360_quoteserviceid") : null;
                                string ap360_workrequested = postImage.GetAttributeValue<string>("ap360_workrequested");
                                EntityReference projectRef = postImage.GetAttributeValue<EntityReference>("ap360_projectid");

                                if (quoteServiceRef != null)
                                {
                                    tracingService.Trace("Quote Service Task : Quote Ref is not null");

                                    ProjectTask quoteServiceProjectTask = new ProjectTask();
                                    quoteServiceProjectTask = ProjectTask.GetProjectTaskforQuoteEntities(service, tracingService, "msdyn_projecttask", "ap360_quoteserviceid", quoteServiceRef.Id, "ap360_quoteservicetask");
                                    if (quoteServiceProjectTask != null)
                                    {
                                        tracingService.Trace("Quote Service Project task " + quoteServiceProjectTask.guid.ToString());
                                        if (projectRef != null)
                                        {


                                            tracingService.Trace("Quote Service: Project Ref is not null");
                                            EntityReference quoteServiceProjectTaskRef = new EntityReference("msdyn_projecttask", quoteServiceProjectTask.guid);
                                            ProjectTask.CreateQuoteServiceTaskProjectTask(service, tracingService, ap360_workrequested, projectRef, quoteServiceProjectTaskRef, entity, quoteServiceRef);


                                        }
                                        // throw new InvalidPluginExecutionException("Err");
                                        //Entity updateQuoteServiceTaskProjectField = new Entity(entity.LogicalName, entity.Id);
                                        //updateQuoteServiceTaskProjectField["ap360_projecttaskid"] = new EntityReference("msdyn_projecttask", quoteServiceProjectTask.guid);
                                        //service.Update(updateQuoteServiceTaskProjectField);
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

                        if (entity.LogicalName == "ap360_quoteservicetask")
                        {
                          //  if (context.Depth > 1) return;

                            if (entity.Contains("ap360_projecttaskid"))
                            {
                                tracingService.Trace("Entity is quoteservicetask");

                                Entity postImage = (Entity)context.PostEntityImages["Image"];

                                if (postImage.Contains("ap360_projecttaskid"))
                                {


                                    // EntityReference workorderRef = preImage.GetAttributeValue<EntityReference>("ap360_workorderid") != null ? preImage.GetAttributeValue<EntityReference>("ap360_workorderid") : null;
                                    EntityReference projectTaskRef = postImage.GetAttributeValue<EntityReference>("ap360_projecttaskid") != null ? postImage.GetAttributeValue<EntityReference>("ap360_projecttaskid") : null;
                                    EntityReference QuoteServiceRef = postImage.GetAttributeValue<EntityReference>("ap360_quoteserviceid") != null ? postImage.GetAttributeValue<EntityReference>("ap360_quoteserviceid") : null;

                                    string name = postImage.GetAttributeValue<string>("ap360_workrequested");
                                    EntityReference projectRef = postImage.GetAttributeValue<EntityReference>("ap360_projectid");

                                    if (projectRef != null)
                                    {

                                        if (projectTaskRef != null)
                                        {
                                            tracingService.Trace("Project Ref is not null");

                                            ProjectTask.CreateQuoteServiceTaskProjectTask(service, tracingService, name, projectRef, projectTaskRef, entity, QuoteServiceRef);

                                        }
                                    }
                                }


                            }

                            if (entity.Contains("ap360_workrequested"))
                            {
                                if (context.Depth > 1) return;
                                Entity postImage = (Entity)context.PostEntityImages["Image"];

                                if (postImage.Contains("ap360_workrequested"))
                                {
                                    EntityReference quoteServiceRef = postImage.GetAttributeValue<EntityReference>("ap360_quoteserviceid") != null ? postImage.GetAttributeValue<EntityReference>("ap360_quoteserviceid") : null;

                                    ProjectTask quoteServiceProjectTask = new ProjectTask();
                                    quoteServiceProjectTask = ProjectTask.GetProjectTaskforQuoteEntities(service, tracingService, "msdyn_projecttask", "ap360_quoteservicetaskid", entity.Id, "ap360_quoteservicetask");
                                    //   throw new InvalidPluginExecutionException("custom" + quoteServiceProjectTask.TaskSubject);
                                    if (quoteServiceProjectTask != null)
                                    {
                                    tracingService.Trace("Task : " + quoteServiceProjectTask.TaskSubject);
                                        //throw new InvalidPluginExecutionException("custom erro ");
                                        Entity updateProjectTaskName = new Entity("msdyn_projecttask", quoteServiceProjectTask.guid);
                                        updateProjectTaskName["msdyn_subject"] = postImage["ap360_workrequested"];
                                        service.Update(updateProjectTaskName);


                                    }

                                }
                            }
                        }
                    }
                }
                if (context.MessageName.ToLower() == "delete")
                {
                    tracingService.Trace("Inside of quote service delete ");

                    EntityReference TargetRef = (EntityReference)context.InputParameters["Target"];
                    if (TargetRef.LogicalName.ToLower() == "ap360_quoteservicetask")
                    {

                        ProjectTask deletingProjectTask = new ProjectTask();
                        deletingProjectTask = ProjectTask.GetProjectTaskforQuoteEntities(service, tracingService, "msdyn_projecttask", "ap360_quoteservicetaskid", TargetRef.Id, TargetRef.LogicalName);
                        if (deletingProjectTask != null)
                        {
                            service.Delete("msdyn_projecttask", deletingProjectTask.guid);
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