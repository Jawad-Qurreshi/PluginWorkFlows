using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class ManageProjectTasksForQuoteService : IPlugin
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

                        if (entity.LogicalName == "ap360_quoteservice")
                        {

                            //throw new InvalidPluginExecutionException(context.Depth.ToString());
                           // if (context.Depth > 1) return;


                            Entity postImage = (Entity)context.PostEntityImages["Image"];

                            if (postImage.Contains("ap360_quoteid"))
                            {
                                tracingService.Trace("Inside PreImage of Project");

                                EntityReference quoteRef = postImage.GetAttributeValue<EntityReference>("ap360_quoteid") != null ? postImage.GetAttributeValue<EntityReference>("ap360_quoteid") : null;
                                string ap360_workrequested = postImage.GetAttributeValue<string>("ap360_workrequested");
                                EntityReference projectRef = postImage.GetAttributeValue<EntityReference>("ap360_projectid");

                                if (quoteRef != null)
                                {
                                    tracingService.Trace("Quote Service : Quote Ref is not null");

                                    ProjectTask quoteProjectTask = new ProjectTask();
                                    quoteProjectTask = ProjectTask.GetProjectTaskforQuoteEntities(service, tracingService, "msdyn_projecttask", "ap360_quoteid", quoteRef.Id, "ap360_quoteservice");
                                    // throw new InvalidPluginExecutionException("Custom Erro ");
                                    if (quoteProjectTask != null)
                                    {
                                        tracingService.Trace("Quote Project task " + quoteProjectTask.guid.ToString());
                                        if (projectRef != null)
                                        {


                                            tracingService.Trace("Quote Service: Project Ref is not null");
                                            EntityReference quoteProjectTaskRef = new EntityReference("msdyn_projecttask", quoteProjectTask.guid);
                                            ProjectTask.CreateQuoteServiceProjectTask(service, tracingService, ap360_workrequested, projectRef, quoteProjectTaskRef, entity);


                                        }

                                        //Entity updateQuoteProjectField = new Entity(entity.LogicalName, entity.Id);
                                        //updateQuoteProjectField["ap360_projecttaskid"] = new EntityReference("msdyn_projecttask", quoteProjectTask.guid);
                                        //service.Update(updateQuoteProjectField);
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

                        if (entity.LogicalName == "ap360_quoteservice")
                        {
                          //  if (context.Depth > 1) return;

                            if (entity.Contains("ap360_projecttaskid"))
                            {
                                tracingService.Trace("Entity is quoteservice");

                                Entity postImage = (Entity)context.PostEntityImages["Image"];

                                if (postImage.Contains("ap360_projecttaskid"))
                                {


                                    // EntityReference workorderRef = preImage.GetAttributeValue<EntityReference>("ap360_workorderid") != null ? preImage.GetAttributeValue<EntityReference>("ap360_workorderid") : null;
                                    EntityReference projectTaskRef = postImage.GetAttributeValue<EntityReference>("ap360_projecttaskid") != null ? postImage.GetAttributeValue<EntityReference>("ap360_projecttaskid") : null;
                                    string name = postImage.GetAttributeValue<string>("ap360_workrequested");
                                    EntityReference projectRef = postImage.GetAttributeValue<EntityReference>("ap360_projectid");
                                    EntityReference quoteRef = postImage.GetAttributeValue<EntityReference>("ap360_quoteid") != null ? postImage.GetAttributeValue<EntityReference>("ap360_quoteid") : null;

                                    if (projectRef != null)
                                    {

                                        if (projectTaskRef != null)
                                        {
                                            tracingService.Trace("Quote Service: Project Ref is not null");

                                            ProjectTask.CreateQuoteServiceProjectTask(service, tracingService, name, projectRef, projectTaskRef, entity);

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
                                    EntityReference quoteRef = postImage.GetAttributeValue<EntityReference>("ap360_quoteid") != null ? postImage.GetAttributeValue<EntityReference>("ap360_quoteid") : null;

                                    ProjectTask quoteProjectTask = new ProjectTask();
                                    quoteProjectTask = ProjectTask.GetProjectTaskforQuoteEntities(service, tracingService, "msdyn_projecttask", "ap360_quoteserviceid", entity.Id, "ap360_quoteservice");
                                    //  throw new InvalidPluginExecutionException("custom");

                                    if (quoteProjectTask != null)
                                    {
                                        if (quoteProjectTask.QuoteService != null)
                                        {
                                            tracingService.Trace("Task : " + quoteProjectTask.TaskSubject);
                                            // throw new InvalidPluginExecutionException("custom erro ");
                                            Entity updateProjectTaskName = new Entity("msdyn_projecttask", quoteProjectTask.guid);
                                            updateProjectTaskName["msdyn_subject"] = postImage["ap360_workrequested"];
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
                    tracingService.Trace("Inside of quote service delete ");

                    EntityReference TargetRef = (EntityReference)context.InputParameters["Target"];
                    if (TargetRef.LogicalName.ToLower() == "ap360_quoteservice")
                    {

                        ProjectTask deletingProjectTask = new ProjectTask();
                        deletingProjectTask = ProjectTask.GetProjectTaskforQuoteEntities(service, tracingService, "msdyn_projecttask", "ap360_quoteserviceid", TargetRef.Id, TargetRef.LogicalName);
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