using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class CreateReviseQuoteProjectTasks : IPlugin
    {

        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();
            // throw new InvalidPluginExecutionException("QuoteProductCalculation");

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


                //  tracingService.Trace("MoveQuoteServiceItemstoDifferentQuoteService");
                if (context.MessageName.ToLower() == "update")
                {

                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        entity = (Entity)context.InputParameters["Target"];
                        if (entity.LogicalName == "quote")
                        {
                            if ((entity.Contains("ap360_createrevisequoteprojecttask")))
                            {
                                if (entity.GetAttributeValue<bool>("ap360_createrevisequoteprojecttask"))
                                {
                                    tracingService.Trace("Project Task creation started for " + entity.LogicalName);
                                    Entity postImage = (Entity)context.PostEntityImages["Image"];
                                    EntityReference opportunityRef = null;
                                    EntityReference customerRef = null;
                                    string quoteName = null;
                                    if (postImage.Contains("opportunityid"))
                                    {
                                        opportunityRef = postImage.GetAttributeValue<EntityReference>("opportunityid") != null ? postImage.GetAttributeValue<EntityReference>("opportunityid") : null;
                                    }
                                    if (opportunityRef == null)
                                    {
                                        tracingService.Trace("Opportunity Ref for Project Task is null");
                                        return;
                                    }
                                    if (postImage.Contains("customerid"))
                                    {
                                        customerRef = postImage.GetAttributeValue<EntityReference>("customerid") != null ? postImage.GetAttributeValue<EntityReference>("customerid") : null;

                                    }
                                    if (postImage.Contains("name"))
                                    {
                                        quoteName = postImage.GetAttributeValue<string>("name");

                                    }
                                    // Entity reterviedQuote = service.Retrieve(entity.LogicalName,entity.Id, new ColumnSet("customerid", "opportunityid"));

                                    Entity quoteProject = null;
                                    quoteProject = Project.GetQuoteProjectofOpportunity(service, tracingService, opportunityRef.Id);
                                    int projectTasksStartingCount = 0;
                                    Guid ProjectGuid;
                                    if (quoteProject != null)
                                    {
                                        tracingService.Trace("Project is already created for Revise Quote Process");
                                        projectTasksStartingCount = ProjectTask.GetProjectTaskscountRelatedOpprotunityQuoteProject(service, tracingService, quoteProject.Id);
                                        ProjectGuid = quoteProject.Id;
                                    }
                                    else
                                    {
                                        tracingService.Trace("Project is not created WO for Revise Quote Process");
                                        ProjectGuid = Project.CreateProject(service, tracingService, customerRef, opportunityRef.Name, opportunityRef.Id, "quote");
                                        projectTasksStartingCount = 0;
                                    }


                                    //  List<Quote> lstQuoteEntities = new List<Quote>();
                                    //  lstQuoteEntities = Quote.GetQuotesRelatedtoOpportunity(service, tracingService, opportunityRef.Id);


                                    // tracingService.Trace("ount of quotes " + lstQuoteEntities.Count.ToString());
                                    int quoteCount = 0;
                                    // quoteCount = lstQuoteEntities.Count;
                                    quoteCount = projectTasksStartingCount;
                                    quoteCount++;
                                    tracingService.Trace("quote projects quote " + quoteCount.ToString());

                                    //   throw new InvalidPluginExecutionException(lstQuoteEntities.Count.ToString());



                                    Guid newlyCreatedRevisedQuoteProjectTaskGuid = Guid.Empty;
                                    int quoteWBSID = 0;
                                    //if (quoteProject != null)
                                    //{
                                    //  ProjectTask quoteProjectTask = new ProjectTask();
                                    ProjectTask quoteProjectTask;
                                    quoteProjectTask = ProjectTask.GetProjectTaskforQuoteEntities(service, tracingService, "msdyn_projecttask", "ap360_quoteid", entity.Id, "ap360_quoteservice");
                                    // throw new InvalidPluginExecutionException(" project task not null custom error ");
                                    if (quoteProjectTask != null)
                                    {
                                        tracingService.Trace("Project task is already created for Revise Quote Process");
                                        if (quoteProjectTask.guid != null)
                                        {
                                            tracingService.Trace("quote Project task guid is not null " + quoteProjectTask.guid.ToString());

                                            newlyCreatedRevisedQuoteProjectTaskGuid = quoteProjectTask.guid;
                                            quoteWBSID = Convert.ToInt32(quoteProjectTask.WBSId);
                                        }
                                    }
                                    //}
                                    else
                                    {
                                        // throw new InvalidPluginExecutionException(" else custom error ");
                                        tracingService.Trace("Projectv task is not created for Revise Quote Process");
                                        newlyCreatedRevisedQuoteProjectTaskGuid = ProjectTask.createProjectTaskForReviseQuote(service, tracingService, quoteName, entity, ProjectGuid, quoteCount);
                                        quoteWBSID = quoteCount;
                                        // projectTasksStartingCount = 0;
                                    }




                                    int sumofItems = 0;
                                    if (newlyCreatedRevisedQuoteProjectTaskGuid != Guid.Empty)
                                    {
                                        tracingService.Trace("Newly created Quote Project Task Guid " + newlyCreatedRevisedQuoteProjectTaskGuid.ToString());
                                        List<QuoteService> lstQuoteServcieEntities = new List<QuoteService>();
                                        lstQuoteServcieEntities = QuoteService.GetQuoteServices(service, tracingService, entity.Id);
                                        int countofQuoteSericeProjectTaskCreated = 0;
                                        countofQuoteSericeProjectTaskCreated = QuoteService.getCountOfQuoteSericesWhereProjectTaskCreate(lstQuoteServcieEntities);
                                        //throw new InvalidPluginExecutionException("Custom Erro");
                                        tracingService.Trace("code executed this line");
                                        if (lstQuoteServcieEntities.Count > 0)
                                        {
                                            tracingService.Trace("Before Foreach " + lstQuoteServcieEntities.Count.ToString());
                                        }
                                        int quoteServiceCount = 0;
                                        foreach (QuoteService quoteService in lstQuoteServcieEntities)
                                        {
                                            int quoteServiceTaskCount = 0;
                                            if (!quoteService.IsQsProjecttaskCreated)
                                            {

                                                countofQuoteSericeProjectTaskCreated++;
                                                quoteServiceCount++;
                                                List<QuoteServiceTask> lstQuoteServiceTasks = new List<QuoteServiceTask>();
                                                lstQuoteServiceTasks = QuoteServiceTask.GetQuoteServiceTasksForReviseQuote(service, quoteService.guid);
                                                sumofItems = sumofItems + lstQuoteServiceTasks.Count;

                                                if (sumofItems < 35)
                                                {
                                                    Guid newcreatedProjectTaskGuid = ProjectTask.createProjectTaskForReviseQuoteService(service, tracingService, quoteService, ProjectGuid, quoteWBSID, countofQuoteSericeProjectTaskCreated, newlyCreatedRevisedQuoteProjectTaskGuid);

                                                    if (lstQuoteServiceTasks.Count > 0)
                                                    {

                                                        tracingService.Trace("Inside Foreach " + lstQuoteServiceTasks.Count.ToString());
                                                    }
                                                    foreach (QuoteServiceTask quoteServiceTask in lstQuoteServiceTasks)
                                                    {
                                                        tracingService.Trace("Srv Task : " + quoteServiceTask.Name);
                                                        quoteServiceTaskCount++;
                                                        ProjectTask.createProjectTaskForReviseQuoteServiceTask(service, tracingService, quoteService, quoteServiceTask, newcreatedProjectTaskGuid, ProjectGuid, quoteWBSID, countofQuoteSericeProjectTaskCreated, quoteServiceTaskCount);
                                                    }

                                                    Entity updateQuoteService = new Entity("ap360_quoteservice", quoteService.guid);
                                                    updateQuoteService["ap360_isqsprojecttaskcreated"] = true;
                                                    service.Update(updateQuoteService);
                                                }
                                            }
                                        }
                                    }
                                    tracingService.Trace("Plugin Ended");
                                    //   throw new InvalidPluginExecutionException("Error");
                                }
                            }
                        }

                    }
                }

                //throw new InvalidPluginExecutionException("Cutom error");
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}