using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class ManageProjectTasksForQuote : IPlugin
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

                        if (entity.LogicalName == "quote")
                        {

                            // throw new InvalidPluginExecutionException("Error "+context.Depth.ToString());
                            if (context.Depth != 2) return;// From associated view this value is 2 for quote only, from Main Form this value is 1

                            tracingService.Trace("Entity is quote");

                            Entity preImage = (Entity)context.PostEntityImages["Image"];
                            if (preImage.Contains("opportunityid"))
                            {
                                tracingService.Trace("Inside PreImage of Project");

                                EntityReference opportunityRef = preImage.GetAttributeValue<EntityReference>("opportunityid") != null ? preImage.GetAttributeValue<EntityReference>("opportunityid") : null;
                                string name = preImage.GetAttributeValue<string>("name");

                                if (opportunityRef != null)
                                {
                                    tracingService.Trace("Quote : Project Ref is not null");
                                    Entity opportunityQuoteProject = Project.GetQuoteProjectofOpportunity(service, tracingService, opportunityRef.Id);
                                    if (opportunityQuoteProject != null)
                                    {
                                        //throw new InvalidPluginExecutionException("Quote Project");
                                        EntityReference opportunityQuoteProjectRef = new EntityReference(opportunityQuoteProject.LogicalName, opportunityQuoteProject.Id);
                                        ProjectTask.CreateQuoteProjectTask(service, tracingService, name, opportunityQuoteProjectRef, entity, opportunityRef);





                                    
                                    }
                                   // throw new InvalidPluginExecutionException("out side");
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

                        if (entity.LogicalName == "quote")
                        {
                            //   if (context.Depth > 1) return;
                            if (entity.Contains("ap360_projectid"))
                            {
                                tracingService.Trace("Entity is quote");

                                Entity preImage = (Entity)context.PostEntityImages["Image"];

                                if (preImage.Contains("ap360_projectid"))
                                {
                                    tracingService.Trace("Inside PreImage of Project");

                                    // EntityReference workorderRef = preImage.GetAttributeValue<EntityReference>("ap360_workorderid") != null ? preImage.GetAttributeValue<EntityReference>("ap360_workorderid") : null;
                                    EntityReference projectRef = preImage.GetAttributeValue<EntityReference>("ap360_projectid") != null ? preImage.GetAttributeValue<EntityReference>("ap360_projectid") : null;
                                    EntityReference opportunityRef = preImage.GetAttributeValue<EntityReference>("opportunityid") != null ? preImage.GetAttributeValue<EntityReference>("opportunityid") : null;
                                    string name = preImage.GetAttributeValue<string>("name");

                                    if (projectRef != null)
                                    {
                                        tracingService.Trace("Quote : Project Ref is not null");

                                        ProjectTask.CreateQuoteProjectTask(service, tracingService, name, projectRef, entity, opportunityRef);

                                    }
                                }

                            }
                            if (entity.Contains("name"))
                            {
                                if (context.Depth > 1) return;
                                Entity preImage = (Entity)context.PostEntityImages["Image"];

                                if (preImage.Contains("name"))
                                {
                                    ProjectTask quoteProjectTask = new ProjectTask();
                                    //throw new InvalidPluginExecutionException("Custom Erro ");
                                    quoteProjectTask = ProjectTask.GetProjectTaskforQuoteEntities(service, tracingService, "msdyn_projecttask", "ap360_quoteid", entity.Id, "quote");
                                    if (quoteProjectTask != null)
                                    {
                                        if (quoteProjectTask.Quote != null)
                                        {
                                            tracingService.Trace("Task : " + quoteProjectTask.TaskSubject);
                                            // throw new InvalidPluginExecutionException("custom erro ");
                                            Entity updateProjectTaskName = new Entity("msdyn_projecttask", quoteProjectTask.guid);
                                            updateProjectTaskName["msdyn_subject"] = preImage["name"];
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
                    tracingService.Trace("Inside of quote delete ");

                    EntityReference TargetRef = (EntityReference)context.InputParameters["Target"];
                    if (TargetRef.LogicalName.ToLower() == "quote")
                    {

                        ProjectTask deletingProjectTask = new ProjectTask();
                        deletingProjectTask = ProjectTask.GetProjectTaskforQuoteEntities(service, tracingService, "msdyn_projecttask", "ap360_quoteid", TargetRef.Id, TargetRef.LogicalName);
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