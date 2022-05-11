using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class CreateWorkOrderProjectAndProjectTasksFromQuote : IPlugin
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




                string quoteId = (string)context.InputParameters["quoteGuid"];
                Entity entity = null;
                entity = new Entity("quote", new Guid(quoteId));

                // throw new InvalidPluginExecutionException(quoteId);
                //  throw new InvalidPluginExecutionException(quoteId);
                //  Entity entity = new Entity("quote", quoteGuid);
                //if (context.InputParameters.Contains("Target") &

                ////  tracingService.Trace("MoveQuoteServiceItemstoDifferentQuoteService");
                //if (context.MessageName.ToLower() == "update")
                //{

                //    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                //    {
                //        entity = (Entity)context.InputParameters["Target"];

                //        if (entity.LogicalName == "quote")
                //        {
                //            if ((entity.Contains("ap360_createwoprojecttask")))
                //            {
                //                if (entity.GetAttributeValue<bool>("ap360_createwoprojecttask"))
                //                {
                Quote quote = new Quote();
                quote = Quote.getQuote(service, tracingService, entity.LogicalName, entity.Id);
                tracingService.Trace("Project Task creation started for " + entity.LogicalName);
                //  Entity postImage = (Entity)context.PostEntityImages["Image"];
                EntityReference opportunityRef = null;
                EntityReference customerRef = null;
                if (quote.Opportunity != null)
                {
                    opportunityRef = quote.Opportunity;
                }
                if (opportunityRef == null)
                {
                    tracingService.Trace("Opportunity Ref for Project Task is null");
                    return;
                }
                if (quote.PotentialCustomer != null)
                {
                    customerRef = quote.PotentialCustomer;

                }

                // Entity reterviedQuote = service.Retrieve(entity.LogicalName,entity.Id, new ColumnSet("customerid", "opportunityid"));

                List<Entity> lstProject = new List<Entity>();
                lstProject = Project.GetWOProjectRelatedToOpportunity(service, tracingService, opportunityRef.Id);
                int projectTasksStartingCount = 0;
                Guid ProjectGuid;
                if (lstProject.Count > 0)
                {
                    tracingService.Trace("Project is already created for WO");
                    projectTasksStartingCount = ProjectTask.GetProjectTaskscountRelatedOpprotunityWorkOrderProject(service, tracingService, lstProject[0].Id);
                    ProjectGuid = lstProject[0].Id;
                }
                else
                {
                    tracingService.Trace("Project is not created WO");
                    ProjectGuid = Project.CreateProject(service, tracingService, customerRef, opportunityRef.Name, opportunityRef.Id, "workorder");
                    projectTasksStartingCount = 0;
                }
                List<Entity> lstWOEntities = new List<Entity>();
                lstWOEntities = WorkOrder.GetWorkOrderRelatedtoQuote(service, tracingService, entity.Id);
                int WOProjectTaskCreatedCount = WorkOrder.GetWOProjectTaskCreatedCount(lstWOEntities);

                tracingService.Trace("Already exist project tasks for WorkOrder count is " + projectTasksStartingCount.ToString());

                if (WOProjectTaskCreatedCount == lstWOEntities.Count)
                {
                    context.OutputParameters["isAllWorkOrdersAreConverted"] = "AlreadyConverted";
                    return;
                }

                int workOrderCount = projectTasksStartingCount;
                int sumofWOSrvTasks = 0;
                List<Entity> lstOfProcessedWorkOrderCount = new List<Entity>();
                int tempcount = 0;
                foreach (Entity workOrderEntity in lstWOEntities)
                {

                    tracingService.Trace("WorkOrder Id "+ workOrderEntity.Id.ToString());
                    bool iswoprojecttaskcreated = workOrderEntity.GetAttributeValue<bool>("ap360_iswoprojecttaskcreated");
                    if (iswoprojecttaskcreated == false)
                    {
                        int wostCount = 0;
                        workOrderCount++;
                        List<Entity> lstWOSTEntities = new List<Entity>();
                        lstWOSTEntities = WorkOrderServiceTask.GetWOSTrelatedToWorkOrder(service, tracingService, workOrderEntity.Id);
                        sumofWOSrvTasks = sumofWOSrvTasks + lstWOSTEntities.Count;
                        if (sumofWOSrvTasks < 45)
                        {
                            tempcount++;
                            lstOfProcessedWorkOrderCount.Add(workOrderEntity);
                            tracingService.Trace("lstOfProcessedWorkOrderCount  " + lstOfProcessedWorkOrderCount.Count.ToString());
                            tracingService.Trace("inside for " + tempcount.ToString());
                            var watch1 = System.Diagnostics.Stopwatch.StartNew();
                            Guid newlyCreatedWorkOrderProject = ProjectTask.createProjectTaskForWorkOrder(service, tracingService, workOrderEntity, ProjectGuid, workOrderCount);
                            tracingService.Trace("AFter createProjectTaskForWorkOrder CreateWorkOrderProjectAndProjectTasksFromQuote ");
                            watch1.Stop();
                            var elapsedMs = watch1.ElapsedMilliseconds;


                            foreach (Entity workOrderServiceTask in lstWOSTEntities)
                            {
                                var watch2 = System.Diagnostics.Stopwatch.StartNew();
                                wostCount++;
                                ProjectTask.CreateProjectTaskForWOST(service, tracingService, workOrderEntity, workOrderServiceTask, newlyCreatedWorkOrderProject, ProjectGuid, workOrderCount, wostCount);
                                watch2.Stop();
                                var elapsedTime = watch1.ElapsedMilliseconds;
                                tracingService.Trace("WOST Time  " + elapsedTime.ToString());
                            }
                            Entity updateWorkOrder = new Entity("msdyn_workorder", workOrderEntity.Id);
                            updateWorkOrder["ap360_iswoprojecttaskcreated"] = true;
                            service.Update(updateWorkOrder);

                            Entity updateQuote = new Entity("quote", entity.Id);
                            updateQuote["ap360_createwoprojecttask"] = false;
                            service.Update(updateQuote);


                        }



                    }
                }
                WOProjectTaskCreatedCount = WOProjectTaskCreatedCount + lstOfProcessedWorkOrderCount.Count;
                if (WOProjectTaskCreatedCount == lstWOEntities.Count)
                {
                    context.OutputParameters["isAllWorkOrdersAreConverted"] = "Yes";

                }
                else
                {

                    context.OutputParameters["isAllWorkOrdersAreConverted"] = "No";

                }
                //throw new InvalidPluginExecutionException(WOProjectTaskCreatedCount+ "---" + lstWOEntities.Count);

                tracingService.Trace("Plugin Ended");
            }

            //                }
            //            }

            //        }
            //    }


            //}

            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}