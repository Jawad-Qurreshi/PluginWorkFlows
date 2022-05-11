using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code.Model
{
    public class ProjectTask
    {
        public Guid guid { get; set; }
        public string TaskSubject { get; set; }
        public string WBSId { get; set; }

        public string Description { get; set; }
        public EntityReference Quote { get; set; }
        public EntityReference QuoteService { get; set; }
        public EntityReference QuoteServiceTask { get; set; }
        public EntityReference WorkOrder { get; set; }
        public EntityReference WorkOrderServiceTask { get; set; }
        public static void CreateQuoteProjectTask(IOrganizationService service, ITracingService tracingService, string quoteName, EntityReference project, Entity quoteEntity, EntityReference opportunityRef)
        {

            Entity newProjectTaskQt = new Entity("msdyn_projecttask");
            newProjectTaskQt["msdyn_subject"] = quoteName;
            newProjectTaskQt["msdyn_project"] = new EntityReference("msdyn_project", project.Id);

            if (opportunityRef == null)
            {
                throw new InvalidPluginExecutionException("Opportunity is null for creation of Project Task");
            }
            // DataCollection<Entity> collection = Methods.GetChildRecordsForProjectsTaskCount(service, opportunityRef.Id, "opportunityid", "quote");
            int quoteProjectTasksCount = ProjectTask.GetProjectTaskscountRelatedOpprotunityQuoteProject(service, tracingService, project.Id);

            //  throw new InvalidPluginExecutionException(quoteProjectTasksCount.ToString());
            quoteProjectTasksCount++;
            newProjectTaskQt["msdyn_wbsid"] = quoteProjectTasksCount.ToString();
            // throw new InvalidPluginExecutionException("WBS ID "+ quoteProjectTasksCount.ToString());
            //newProjectTaskQt["msdyn_wbsid"] = "5";


            if (quoteEntity == null)
            {
                throw new InvalidPluginExecutionException("Quote is not mapping");
            }

            newProjectTaskQt["ap360_quoteid"] = new EntityReference(quoteEntity.LogicalName, quoteEntity.Id);

            newProjectTaskQt["msdyn_scheduledstart"] = DateTime.Now;//start date
            newProjectTaskQt["msdyn_scheduledend"] = DateTime.Now.AddDays(3);//due date
                                                                             // int count = 2;
            service.Create(newProjectTaskQt);
            //////////////////////////////Added on 13 August 2021 to instantly populate WBSId in Quote ///////////////////////

            Entity updateQuoteProjectField = new Entity(quoteEntity.LogicalName, quoteEntity.Id);
            updateQuoteProjectField["ap360_projectid"] = project;
            updateQuoteProjectField["ap360_wbsid"] = quoteProjectTasksCount;

            service.Update(updateQuoteProjectField);
        }


        public static void CreateIndividualWOSTProjectTask(IOrganizationService service, ITracingService tracingService, string woName, EntityReference projectRef, EntityReference parentProjectTask,
            Entity workOrderServiceTaskEntity, EntityReference parentEntityRef, int estimatedDuration, Money estimatedAmount)
        
        {

            // throw new InvalidPluginExecutionException("Custom Error");
            tracingService.Trace("Create WorkOrder Task Project Task");
            Entity newProjectTaskQtSrv = new Entity("msdyn_projecttask");
            newProjectTaskQtSrv["msdyn_subject"] = woName;
            newProjectTaskQtSrv["msdyn_project"] = new EntityReference("msdyn_project", projectRef.Id);

            decimal effort = estimatedDuration / 60m;
            newProjectTaskQtSrv["msdyn_effort"] = Convert.ToDouble(effort);
            if (estimatedAmount != null)
            {
                //throw new InvalidPluginExecutionException(estimatedAmount.Value.ToString());
                newProjectTaskQtSrv["msdyn_plannedcost"] = estimatedAmount;
                newProjectTaskQtSrv["msdyn_plannedcost_base"] = new Money(estimatedAmount.Value);
            }






            if (projectRef == null)
            {
                throw new InvalidPluginExecutionException("Project Referecne is null for Work Order Project Task");
            }
            DataCollection<Entity> projectTaskCount = ProjectTask.GetChildRecordsForParentTask(service, parentProjectTask.Id);

            int count = projectTaskCount.Count + 1;
            string parentWbsId = ProjectTask.GetParentWbSId(service, tracingService, parentProjectTask.Id);
            tracingService.Trace(parentWbsId.ToString() + "." + count.ToString());
            newProjectTaskQtSrv["msdyn_wbsid"] = parentWbsId.ToString() + "." + count.ToString();


            if (workOrderServiceTaskEntity == null)
            {
                throw new InvalidPluginExecutionException("Work Order Service Task is not mapping");
            }
            if (parentEntityRef.LogicalName == "msdyn_workorder")
            newProjectTaskQtSrv["ap360_workorderservicetaskid"] = new EntityReference(workOrderServiceTaskEntity.LogicalName, workOrderServiceTaskEntity.Id);
            else if (parentEntityRef.LogicalName == "msdyn_workorderservicetask")
            newProjectTaskQtSrv["ap360_childworkorderservicetaskid"] = new EntityReference(workOrderServiceTaskEntity.LogicalName, workOrderServiceTaskEntity.Id);

            if (parentProjectTask != null)
            {
                newProjectTaskQtSrv["msdyn_parenttask"] = new EntityReference("msdyn_projecttask", parentProjectTask.Id);
            }
            tracingService.Trace("Middle");
            newProjectTaskQtSrv["msdyn_scheduledstart"] = DateTime.Now;//start date
            newProjectTaskQtSrv["msdyn_scheduledend"] = DateTime.Now.AddDays(3);//due date
            Guid newlyCreatedTaskGuid = service.Create(newProjectTaskQtSrv);
            tracingService.Trace("end");

            // throw new InvalidPluginExecutionException("Err");
            Entity updateWorkOrderTaskProjectField = new Entity(workOrderServiceTaskEntity.LogicalName, workOrderServiceTaskEntity.Id);
            updateWorkOrderTaskProjectField["ap360_projecttaskid"] = new EntityReference("msdyn_projecttask", newlyCreatedTaskGuid);
            updateWorkOrderTaskProjectField["ap360_wbsid"] = count;

            service.Update(updateWorkOrderTaskProjectField);
            // int count = 2;
        }

        public static void CreateQuoteServiceProjectTask(IOrganizationService service, ITracingService tracingService, string quoteServiceName, EntityReference projectRef, EntityReference projectTask, Entity quoteServiceEntity)
        {
            tracingService.Trace("Create Quote Service Project Task");
            Entity newProjectTaskQtSrv = new Entity("msdyn_projecttask");
            newProjectTaskQtSrv["msdyn_subject"] = quoteServiceName;
            newProjectTaskQtSrv["msdyn_project"] = new EntityReference("msdyn_project", projectRef.Id);

            if (projectRef == null)
            {
                throw new InvalidPluginExecutionException("Project Referecne is null for Quote Service Project Task");
            }
            DataCollection<Entity> projectTaskCount = ProjectTask.GetChildRecordsForParentTask(service, projectTask.Id);

            int count = projectTaskCount.Count + 1;
            string parentWbsId = ProjectTask.GetParentWbSId(service, tracingService, projectTask.Id);
            tracingService.Trace(parentWbsId.ToString() + "." + count.ToString());
            newProjectTaskQtSrv["msdyn_wbsid"] = parentWbsId.ToString() + "." + count.ToString();

            if (quoteServiceEntity == null)
            {
                throw new InvalidPluginExecutionException("Quote Service is not mapping");
            }

            newProjectTaskQtSrv["ap360_quoteserviceid"] = new EntityReference(quoteServiceEntity.LogicalName, quoteServiceEntity.Id);
            if (projectTask != null)
            {
                newProjectTaskQtSrv["msdyn_parenttask"] = new EntityReference("msdyn_projecttask", projectTask.Id);
            }
            tracingService.Trace("Middle");
            newProjectTaskQtSrv["msdyn_scheduledstart"] = DateTime.Now;//start date
            newProjectTaskQtSrv["msdyn_scheduledend"] = DateTime.Now.AddDays(3);//due date
            service.Create(newProjectTaskQtSrv);
            tracingService.Trace("end");                                                                    // int count = 2;


            //////////////////////////////Added on 13 August 2021 to instantly populate WBSId in Quote service///////////////////////
            Entity updateQuoteProjectField = new Entity(quoteServiceEntity.LogicalName, quoteServiceEntity.Id);
            updateQuoteProjectField["ap360_wbsid"] = count;
            updateQuoteProjectField["ap360_projecttaskid"] = new EntityReference("msdyn_projecttask", projectTask.Id);
            service.Update(updateQuoteProjectField);

            /////////////////////////////////////////////////
        }

        public static void CreateQuoteServiceTaskProjectTask(IOrganizationService service, ITracingService tracingService, string quoteServiceName, EntityReference projectRef, EntityReference projectTask, Entity quoteServiceTaskEntity, EntityReference QuoteServiceRef)
        {
            tracingService.Trace("Create Quote Service Task Project Task");
            Entity newProjectTaskQtSrv = new Entity("msdyn_projecttask");
            newProjectTaskQtSrv["msdyn_subject"] = quoteServiceName;
            newProjectTaskQtSrv["msdyn_project"] = new EntityReference("msdyn_project", projectRef.Id);



            if (projectRef == null)
            {
                throw new InvalidPluginExecutionException("Project Referecne is null for Quote Service Task Project Task");
            }
            DataCollection<Entity> projectTaskCount = ProjectTask.GetChildRecordsForParentTask(service, projectTask.Id);

            int count = projectTaskCount.Count + 1;
            string parentWbsId = ProjectTask.GetParentWbSId(service, tracingService, projectTask.Id);
            tracingService.Trace(parentWbsId.ToString() + "." + count.ToString());
            newProjectTaskQtSrv["msdyn_wbsid"] = parentWbsId.ToString() + "." + count.ToString();
            //   newProjectTaskQtSrv["msdyn_scheduleddurationminutes"] = 5760;
            //  double value = 5;
            // newProjectTaskQtSrv["msdyn_effort"] = value;


            if (quoteServiceTaskEntity == null)
            {
                throw new InvalidPluginExecutionException("Quote Service Task is not mapping");
            }

            newProjectTaskQtSrv["ap360_quoteservicetaskid"] = new EntityReference(quoteServiceTaskEntity.LogicalName, quoteServiceTaskEntity.Id);

            if (projectTask != null)
            {
                newProjectTaskQtSrv["msdyn_parenttask"] = new EntityReference("msdyn_projecttask", projectTask.Id);
            }
            tracingService.Trace("Middle");
            newProjectTaskQtSrv["msdyn_scheduledstart"] = DateTime.Now;//start date
            newProjectTaskQtSrv["msdyn_scheduledend"] = DateTime.Now.AddDays(3);//due date
            service.Create(newProjectTaskQtSrv);

            //////////////////////////////Added on 13 August 2021 to instantly populate WBSId in Quote service task ///////////////////////

            Entity updateQuoteServiceTaskProjectField = new Entity(quoteServiceTaskEntity.LogicalName, quoteServiceTaskEntity.Id);
            updateQuoteServiceTaskProjectField["ap360_projecttaskid"] = new EntityReference("msdyn_projecttask", projectTask.Id);
            updateQuoteServiceTaskProjectField["ap360_wbsid"] = count;

            service.Update(updateQuoteServiceTaskProjectField);

            tracingService.Trace("end");                                                                    // int count = 2;
        }


        public static void CreateProjectTasks(IOrganizationService service, ITracingService tracingService, Guid newlyCreatedProjectGuid, QuoteService qtrSrv, List<QuoteServiceTask> lstQuoteServiceTasks, ref int count)
        {


            Entity newProjectTaskQtSrv = new Entity("msdyn_projecttask");
            newProjectTaskQtSrv["msdyn_subject"] = qtrSrv.Name;
            newProjectTaskQtSrv["msdyn_project"] = new EntityReference("msdyn_project", newlyCreatedProjectGuid);
            newProjectTaskQtSrv["msdyn_wbsid"] = count.ToString();

            newProjectTaskQtSrv["msdyn_scheduledstart"] = DateTime.Now;//start date
            newProjectTaskQtSrv["msdyn_scheduledend"] = DateTime.Now.AddDays(3);//due date
                                                                                // int count = 2;
            Guid newlyCreatedQtSrvProjectGuid = service.Create(newProjectTaskQtSrv);
            count++;
            foreach (QuoteServiceTask quoteServiceTask in lstQuoteServiceTasks)
            {

                Entity newProjectTask = new Entity("msdyn_projecttask");
                newProjectTask["msdyn_subject"] = quoteServiceTask.Name;
                newProjectTask["msdyn_project"] = new EntityReference("msdyn_project", newlyCreatedProjectGuid);
                newProjectTask["msdyn_wbsid"] = count.ToString();
                newProjectTask["msdyn_parenttask"] = new EntityReference("msdyn_projecttask", newlyCreatedQtSrvProjectGuid);

                newProjectTask["msdyn_scheduledstart"] = DateTime.Now;//start date
                newProjectTask["msdyn_scheduledend"] = DateTime.Now.AddDays(3);//due date

                service.Create(newProjectTask);
                count++;

            }
        }

        public static Guid createProjectTaskForReviseQuote(IOrganizationService service, ITracingService tracingService, string quoteName, Entity quoteEntity, Guid projectGuid, int count)
        {
            tracingService.Trace("Create Work Order Project Task");

            Entity newProjectTaskWorkOrder = new Entity("msdyn_projecttask");
            newProjectTaskWorkOrder["msdyn_subject"] = quoteName;
            newProjectTaskWorkOrder["msdyn_project"] = new EntityReference("msdyn_project", projectGuid);
            newProjectTaskWorkOrder["msdyn_wbsid"] = count.ToString();
            newProjectTaskWorkOrder["ap360_quoteid"] = new EntityReference("quote", quoteEntity.Id);

            newProjectTaskWorkOrder["msdyn_scheduledstart"] = DateTime.Now;//start date
            newProjectTaskWorkOrder["msdyn_scheduledend"] = DateTime.Now.AddDays(3);//due date
                                                                                    // int count = 2;
            Guid newcreatedProjectTaskGuid = service.Create(newProjectTaskWorkOrder);
            tracingService.Trace("Work Order Project Task Created");

            Entity updateWorkOrderWBSID = new Entity("quote", quoteEntity.Id);
            updateWorkOrderWBSID["ap360_wbsid"] = count;
            updateWorkOrderWBSID["ap360_projectid"] = new EntityReference("msdyn_project", projectGuid);

            service.Update(updateWorkOrderWBSID);

            return newcreatedProjectTaskGuid;

        }

        public static Guid createProjectTaskForReviseQuoteService(IOrganizationService service, ITracingService tracingService, QuoteService quoteServiceEntity, Guid projectGuid, int quoteCount, int quoteServiceCount, Guid newlyCreatedRevisedQuoteProjectTaskGuid)
        {
            tracingService.Trace("Create Work Order Project Task");

            Entity newProjectTaskQuoteService = new Entity("msdyn_projecttask");
            newProjectTaskQuoteService["msdyn_subject"] = quoteServiceEntity.Name;
            newProjectTaskQuoteService["msdyn_project"] = new EntityReference("msdyn_project", projectGuid);
            newProjectTaskQuoteService["msdyn_wbsid"] = quoteCount.ToString() + "." + quoteServiceCount.ToString();
            newProjectTaskQuoteService["ap360_quoteserviceid"] = new EntityReference("ap360_quoteservice", quoteServiceEntity.guid);
            newProjectTaskQuoteService["msdyn_parenttask"] = new EntityReference("msdyn_projecttask", newlyCreatedRevisedQuoteProjectTaskGuid);

            newProjectTaskQuoteService["msdyn_scheduledstart"] = DateTime.Now;//start date
            newProjectTaskQuoteService["msdyn_scheduledend"] = DateTime.Now.AddDays(3);//due date
                                                                                       // int count = 2;
            Guid newcreatedProjectTaskGuid = service.Create(newProjectTaskQuoteService);
            tracingService.Trace("Work Order Project Task Created");

            Entity updateWorkOrderWBSID = new Entity("ap360_quoteservice", quoteServiceEntity.guid);
            updateWorkOrderWBSID["ap360_wbsid"] = quoteServiceCount;
            updateWorkOrderWBSID["ap360_projectid"] = new EntityReference("msdyn_project", projectGuid);

            service.Update(updateWorkOrderWBSID);

            return newcreatedProjectTaskGuid;

        }
        public static void createProjectTaskForReviseQuoteServiceTask(IOrganizationService service, ITracingService tracingService, QuoteService quoteServiceEntity, QuoteServiceTask quoteServiceTask, Guid quoteServiceTaskGuid, Guid projectGuid, int quoteCount, int QuoteServiceCount, int QuoteServiceTaskCount)
        {

            tracingService.Trace("Create WOST Project Task");
            Entity newProjectTaskWOST = new Entity("msdyn_projecttask");
            newProjectTaskWOST["msdyn_subject"] = quoteServiceTask.Name;
            newProjectTaskWOST["msdyn_project"] = new EntityReference("msdyn_project", projectGuid);
            newProjectTaskWOST["msdyn_wbsid"] = quoteCount.ToString() + "." + QuoteServiceCount.ToString() + "." + QuoteServiceTaskCount.ToString();

            newProjectTaskWOST["ap360_quoteservicetaskid"] = new EntityReference("ap360_quoteservicetask", quoteServiceTask.guid);


            newProjectTaskWOST["msdyn_parenttask"] = new EntityReference("msdyn_projecttask", quoteServiceTaskGuid);

            newProjectTaskWOST["msdyn_scheduledstart"] = DateTime.Now;//start date
            newProjectTaskWOST["msdyn_scheduledend"] = DateTime.Now.AddDays(3);//due date
            service.Create(newProjectTaskWOST);
            tracingService.Trace("WOST Project Task Created");
            Entity updateWOSTWBSID = new Entity("ap360_quoteservicetask", quoteServiceTask.guid);
            updateWOSTWBSID["ap360_wbsid"] = QuoteServiceTaskCount;
            updateWOSTWBSID["ap360_projectid"] = new EntityReference("msdyn_project", projectGuid);

            service.Update(updateWOSTWBSID);
        }

        public static Guid createProjectTaskForWorkOrder(IOrganizationService service, ITracingService tracingService, Entity workOrderEntity, Guid projectGuid, int count)
        {
            tracingService.Trace("Create Work Order Project Task "+ count.ToString());

            Entity newProjectTaskWorkOrder = new Entity("msdyn_projecttask");
            newProjectTaskWorkOrder["msdyn_subject"] = workOrderEntity["ap360_workordername"] + " : " + workOrderEntity["msdyn_name"];
            newProjectTaskWorkOrder["msdyn_project"] = new EntityReference("msdyn_project", projectGuid);
            newProjectTaskWorkOrder["msdyn_wbsid"] = count.ToString();
            newProjectTaskWorkOrder["ap360_workorderid"] = new EntityReference("msdyn_workorder", workOrderEntity.Id);

            newProjectTaskWorkOrder["msdyn_scheduledstart"] = DateTime.Now;//start date
            newProjectTaskWorkOrder["msdyn_scheduledend"] = DateTime.Now.AddDays(3);//due date
                                                                                    // int count = 2;
            Guid newcreatedProjectTaskGuid = service.Create(newProjectTaskWorkOrder);
            tracingService.Trace("Work Order Project Task Created");

            Entity updateWorkOrderWBSID = new Entity("msdyn_workorder", workOrderEntity.Id);
            updateWorkOrderWBSID["ap360_wbsid"] = count;
            updateWorkOrderWBSID["ap360_projectid"] = new EntityReference("msdyn_project", projectGuid);

            service.Update(updateWorkOrderWBSID);

            return newcreatedProjectTaskGuid;

        }

        public static void CreateProjectTaskForWOST(IOrganizationService service, ITracingService tracingService, Entity workOrderEntity, Entity wostEntity, Guid workOrderTaskGuid, Guid projectGuid, int WorkOrderCount, int WOSTCount)
        {

            tracingService.Trace("Create WOST Project Task");
            Entity newProjectTaskWOST = new Entity("msdyn_projecttask");
            newProjectTaskWOST["msdyn_subject"] = wostEntity["msdyn_description"];
            newProjectTaskWOST["msdyn_project"] = new EntityReference("msdyn_project", projectGuid);
            newProjectTaskWOST["msdyn_wbsid"] = WorkOrderCount.ToString() + "." + WOSTCount.ToString();

            newProjectTaskWOST["ap360_workorderservicetaskid"] = new EntityReference("msdyn_workorderservicetask", wostEntity.Id);


            newProjectTaskWOST["msdyn_parenttask"] = new EntityReference("msdyn_projecttask", workOrderTaskGuid);

            newProjectTaskWOST["msdyn_scheduledstart"] = DateTime.Now;//start date
            newProjectTaskWOST["msdyn_scheduledend"] = DateTime.Now.AddDays(3);//due date
            service.Create(newProjectTaskWOST);
            tracingService.Trace("WOST Project Task Created");
            Entity updateWOSTWBSID = new Entity("msdyn_workorderservicetask", wostEntity.Id);
            updateWOSTWBSID["ap360_wbsid"] = WOSTCount;
            updateWOSTWBSID["ap360_projectid"] = new EntityReference("msdyn_project", projectGuid);

            service.Update(updateWOSTWBSID);
        }
        public static string GetParentWbSId(IOrganizationService service, ITracingService tracingService, Guid projectTaskGuid)
        {
            string wbsId = null;
            Entity projectEntity = service.Retrieve("msdyn_projecttask", projectTaskGuid, new Microsoft.Xrm.Sdk.Query.ColumnSet("msdyn_wbsid"));
            if (projectEntity != null)
            {
                wbsId = projectEntity.GetAttributeValue<string>("msdyn_wbsid");

            }

            return wbsId;
        }

        public static DataCollection<Entity> GetChildRecordsForParentTask(IOrganizationService organizationService, Guid projectTaskId)
        {
            var fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='msdyn_projecttask'>
                                <attribute name='msdyn_subject' />
                                <attribute name='createdon' />
                                <attribute name='msdyn_projecttaskid' />
                                <order attribute='msdyn_subject' descending='false' />
                                <filter type='and'>
                                  <condition attribute='msdyn_parenttask' operator='eq'  value='" + projectTaskId + @"' /> 
                                </filter>
                              </entity>
                            </fetch>");

            var fetchExpression = new FetchExpression(fetchXml);
            EntityCollection fetchResult = organizationService.RetrieveMultiple(fetchExpression);
            return fetchResult.Entities;
        }

        public static int GetProjectTaskscountRelatedOpprotunityWorkOrderProject(IOrganizationService service, ITracingService tracingService, Guid woProjectGuid)
        {





            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='msdyn_projecttask'>
                                    <attribute name='msdyn_subject' />
                                    <attribute name='createdon' />
                                    <attribute name='msdyn_projecttaskid' />
                                    <order attribute='msdyn_subject' descending='false' />
                                    <filter type='and'>
                                      <condition attribute='ap360_workorderid' operator='not-null' />
                                      <condition attribute='msdyn_project' operator='eq'  value='" + woProjectGuid + @"' /> 
                                    </filter>
                                  </entity>
                                </fetch>");




            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            int i = 0;
            foreach (Entity ent in col.Entities)
            {
                i++;

            }
            return i;
        }

        public static int GetProjectTaskscountRelatedOpprotunityQuoteProject(IOrganizationService service, ITracingService tracingService, Guid quoteProjectGuid)
        {





            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='msdyn_projecttask'>
                                    <attribute name='msdyn_subject' />
                                    <attribute name='createdon' />
                                    <attribute name='msdyn_projecttaskid' />
                                    <attribute name='msdyn_wbsid' />

                                    <order attribute='msdyn_wbsid' descending='false' />
                                    <filter type='and'>
                                      <condition attribute='ap360_quoteid' operator='not-null' />
                                      <condition attribute='msdyn_project' operator='eq'  value='" + quoteProjectGuid + @"' /> 
                                    </filter>
                                  </entity>
                                </fetch>");




            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            int i = 0;
            string lastProjectTaskWBSID = null;
            int biggestValue = 0;
            foreach (Entity ent in col.Entities)
            {

                lastProjectTaskWBSID = ent.GetAttributeValue<string>("msdyn_wbsid");

                int currentValue = Convert.ToInt32(lastProjectTaskWBSID);
                if (currentValue > biggestValue)
                {
                    biggestValue = currentValue;
                }
                i++;

            }


            return biggestValue;

            //  return i;
        }

        public static List<ProjectTask> GetChildRecordsForCurrentProject(IOrganizationService organizationService, ITracingService tracingService, Guid projectId)
        {
            List<ProjectTask> lstProjectTasks = new List<ProjectTask>();
            var fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='msdyn_projecttask'>
                                <attribute name='msdyn_subject' />
                                <attribute name='createdon' />
                                <attribute name='ap360_quoteid' />
                                <attribute name='ap360_quoteserviceid' />
                                <attribute name='ap360_quoteservicetaskid' />
                                <attribute name='msdyn_projecttaskid' />
                                <attribute name='msdyn_wbsid' />
                                <order attribute='msdyn_wbsid' descending='false' />
                                <filter type='and'>
                                  <condition attribute='msdyn_project' operator='eq'  value='" + projectId + @"' /> 
                                </filter>
                              </entity>
                            </fetch>");

            var fetchExpression = new FetchExpression(fetchXml);
            EntityCollection fetchResult = organizationService.RetrieveMultiple(fetchExpression);
            ProjectTask projectTask = null;
            tracingService.Trace("Project Tasks Count " + fetchResult.Entities.Count.ToString());
            foreach (Entity entity in fetchResult.Entities)
            {

                projectTask = new ProjectTask();
                projectTask.guid = entity.Id;
                projectTask.WBSId = entity.GetAttributeValue<string>("msdyn_wbsid");
                projectTask.TaskSubject = entity.GetAttributeValue<string>("msdyn_subject");

                projectTask.Quote = entity.GetAttributeValue<EntityReference>("ap360_quoteid") != null ? entity.GetAttributeValue<EntityReference>("ap360_quoteid") : null;
                projectTask.QuoteService = entity.GetAttributeValue<EntityReference>("ap360_quoteserviceid") != null ? entity.GetAttributeValue<EntityReference>("ap360_quoteserviceid") : null;
                projectTask.QuoteServiceTask = entity.GetAttributeValue<EntityReference>("ap360_quoteservicetaskid") != null ? entity.GetAttributeValue<EntityReference>("ap360_quoteservicetaskid") : null;
                lstProjectTasks.Add(projectTask);
            }
            tracingService.Trace("List Count " + lstProjectTasks.Count.ToString());
            return lstProjectTasks;
        }
        public static void updateParentTask(IOrganizationService service, ITracingService tracingService, ProjectTask childProjectTask, ProjectTask parentProjectTask, Entity entity)
        {
            tracingService.Trace("Inside updateParentTask");
            if (childProjectTask != null)
            {
                Entity childProjectTaskToUpdate = new Entity("msdyn_projecttask", childProjectTask.guid);
                tracingService.Trace("Parent task :" + parentProjectTask.guid.ToString());
                childProjectTaskToUpdate["msdyn_parenttask"] = new EntityReference("msdyn_projecttask", parentProjectTask.guid);
                if (entity.LogicalName == "ap360_quoteservice")
                {
                    tracingService.Trace("Quote Service is updating : QuoteService Id : " + entity.Id.ToString());
                    childProjectTaskToUpdate["ap360_quoteserviceid"] = new EntityReference(entity.LogicalName, entity.Id);
                }
                else if (entity.LogicalName == "ap360_quoteservicetask")
                {
                    tracingService.Trace("Quote Service Task is updating : QuoteService Id : " + entity.Id.ToString());
                    childProjectTaskToUpdate["ap360_quoteservicetaskid"] = new EntityReference(entity.LogicalName, entity.Id);
                }
                service.Update(childProjectTaskToUpdate);

            }
            else
            {
                throw new InvalidPluginExecutionException("Project Task is not updating in Project Managemnet ");
            }
        }

        public static ProjectTask GetProjectTaskforQuoteEntities(IOrganizationService organizationService, ITracingService tracingService, string childEntityName, string ParentEntityLookupFieldName, Guid parentEntiytId, string parentEntityName)
        {



            var fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>"
               + "<entity name='" + childEntityName + "'>"
                + "<attribute name='" + childEntityName + "id' />"
                + "<attribute name = 'ap360_quoteid' />"
                + "<attribute name = 'msdyn_subject' />"
                + "<attribute name = 'ap360_quoteserviceid' />"
                + "<attribute name = 'ap360_quoteservicetaskid' />"
                + "<attribute name = 'msdyn_wbsid' />"
                + "<attribute name = 'msdyn_projecttaskid' />"
                 + "<filter type ='and' >"
               + "<condition attribute = '" + ParentEntityLookupFieldName + "' operator= 'eq'  value = '" + parentEntiytId + "' />"
               + "</filter >"
               + "</entity >"
                + "</fetch >";





            var fetchExpression = new FetchExpression(fetchXml);
            EntityCollection fetchResult = organizationService.RetrieveMultiple(fetchExpression);
            ProjectTask projectTask = null;
            tracingService.Trace(fetchXml.ToString());
            tracingService.Trace("Project Tasks Count " + fetchResult.Entities.Count.ToString());
            foreach (Entity entity in fetchResult.Entities)
            {

                projectTask = new ProjectTask();
                projectTask.guid = entity.Id;
                projectTask.WBSId = entity.GetAttributeValue<string>("msdyn_wbsid");
                projectTask.TaskSubject = entity.GetAttributeValue<string>("msdyn_subject");

                if (parentEntityName == "quote")
                {
                    tracingService.Trace("quote");
                    projectTask.Quote = entity.GetAttributeValue<EntityReference>("ap360_quoteid") != null ? entity.GetAttributeValue<EntityReference>("ap360_quoteid") : null;
                }
                else if (parentEntityName == "ap360_quoteservice")
                {
                    tracingService.Trace("quote service");
                    projectTask.QuoteService = entity.GetAttributeValue<EntityReference>("ap360_quoteserviceid") != null ? entity.GetAttributeValue<EntityReference>("ap360_quoteserviceid") : null;

                }
                else if (parentEntityName == "ap360_quoteservicetask")
                {
                    tracingService.Trace("quote service task");
                    projectTask.QuoteServiceTask = entity.GetAttributeValue<EntityReference>("ap360_quoteservicetaskid") != null ? entity.GetAttributeValue<EntityReference>("ap360_quoteservicetaskid") : null;
                }
            }
            return projectTask;
        }
        public static ProjectTask GetProjectTaskforWorkOrderEntities(IOrganizationService organizationService, ITracingService tracingService, string childEntityName, string ParentEntityLookupFieldName, Guid parentEntiytId, string parentEntityName)
        {

            var fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>"
               + "<entity name='" + childEntityName + "'>"
                + "<attribute name='" + childEntityName + "id' />"
                + "<attribute name = 'msdyn_subject' />"
                + "<attribute name = 'ap360_workorderid' />"
                + "<attribute name = 'ap360_workorderservicetaskid' />"
                + "<attribute name = 'msdyn_wbsid' />"
                + "<attribute name = 'msdyn_projecttaskid' />"
                 + "<filter type ='and' >"
               + "<condition attribute = '" + ParentEntityLookupFieldName + "' operator= 'eq'  value = '" + parentEntiytId + "' />"
               + "</filter >"
               + "</entity >"
                + "</fetch >";





            var fetchExpression = new FetchExpression(fetchXml);
            EntityCollection fetchResult = organizationService.RetrieveMultiple(fetchExpression);
            ProjectTask projectTask = null;
            tracingService.Trace(fetchXml.ToString());
            tracingService.Trace("WO Project Tasks Count " + fetchResult.Entities.Count.ToString());
            foreach (Entity entity in fetchResult.Entities)
            {

                projectTask = new ProjectTask();
                projectTask.guid = entity.Id;
                projectTask.WBSId = entity.GetAttributeValue<string>("msdyn_wbsid");
                projectTask.TaskSubject = entity.GetAttributeValue<string>("msdyn_subject");

                if (parentEntityName == "msdyn_workorder")
                {
                    projectTask.WorkOrder = entity.GetAttributeValue<EntityReference>("ap360_workorderid") != null ? entity.GetAttributeValue<EntityReference>("ap360_workorderid") : null;
                }
                else if (parentEntityName == "msdyn_workorderservicetask")
                {
                    projectTask.WorkOrderServiceTask = entity.GetAttributeValue<EntityReference>("ap360_workorderservicetaskid") != null ? entity.GetAttributeValue<EntityReference>("ap360_workorderservicetaskid") : null;

                }
            }
            return projectTask;
        }

        public static void updateQSLevelCurrentItemIdsinSequence(IOrganizationService service, ITracingService tracingService, Entity parentTaskEntity, Entity entity)
        {

            // throw new InvalidPluginExecutionException("Custom Error");
            string parentTaskEntityWBSId = parentTaskEntity.GetAttributeValue<string>("msdyn_wbsid");
            string subject = parentTaskEntity.GetAttributeValue<string>("msdyn_subject");

            tracingService.Trace("WBS ID " + parentTaskEntityWBSId + " Subject :" + subject);
            DataCollection<Entity> projectTaskSiblingCount = ProjectTask.GetChildRecordsForParentTask(service, parentTaskEntity.Id);
            tracingService.Trace(subject + " child Count " + projectTaskSiblingCount.Count.ToString());

            Entity updateProjectServiceTask = new Entity(entity.LogicalName, entity.Id);
            updateProjectServiceTask["msdyn_wbsid"] = parentTaskEntityWBSId + "." + projectTaskSiblingCount.Count.ToString();


            // throw new InvalidPluginExecutionException();
            service.Update(updateProjectServiceTask);

            DataCollection<Entity> lstProjectTasks;
            lstProjectTasks = ProjectTask.GetChildRecordsForParentTask(service, entity.Id);
            tracingService.Trace("lst of Child ProjectTasks " + lstProjectTasks.Count.ToString());
            // throw new InvalidPluginExecutionException("child entity count "+lstProjectTasks.Count.ToString());
            int childCount = 1;
            foreach (Entity childProjectTask in lstProjectTasks)
            {
                tracingService.Trace("Child Task " + childCount.ToString() + " : Subject " + childProjectTask.GetAttributeValue<string>("msdyn_subject"));
                Entity updateChildProjectTask = new Entity(childProjectTask.LogicalName, childProjectTask.Id);
                updateChildProjectTask["msdyn_wbsid"] = parentTaskEntityWBSId + "." + projectTaskSiblingCount.Count.ToString() + "." + childCount.ToString();
                service.Update(updateChildProjectTask);
                childCount++;
            }
        }

        public static void updateQSLevelPreviousItemIdsinSequence(IOrganizationService service, ITracingService tracingService, Entity parentTaskEntity, Entity entity)
        {


            string parentTaskEntityWBSId = parentTaskEntity.GetAttributeValue<string>("msdyn_wbsid");
            tracingService.Trace("WBS ID " + parentTaskEntityWBSId);
            DataCollection<Entity> lstprojectTaskSiblings = ProjectTask.GetChildRecordsForParentTask(service, parentTaskEntity.Id);
            tracingService.Trace("Parent Task Count " + lstprojectTaskSiblings.Count.ToString());

            int siblingCount = 1;
            foreach (Entity projectTask in lstprojectTaskSiblings)
            {
                Entity updateSiblingProjectTask = new Entity(projectTask.LogicalName, projectTask.Id);
                updateSiblingProjectTask["msdyn_wbsid"] = parentTaskEntityWBSId + "." + siblingCount.ToString();
                service.Update(updateSiblingProjectTask);




                DataCollection<Entity> lstProjectTasks;
                lstProjectTasks = ProjectTask.GetChildRecordsForParentTask(service, projectTask.Id);
                tracingService.Trace("lstProjectTasks " + lstProjectTasks.Count.ToString());
                // throw new InvalidPluginExecutionException("child entity count "+lstProjectTasks.Count.ToString());
                int childCount = 1;
                foreach (Entity childProjectTask in lstProjectTasks)
                {
                    Entity updateChildProjectTask = new Entity(childProjectTask.LogicalName, childProjectTask.Id);
                    updateChildProjectTask["msdyn_wbsid"] = parentTaskEntityWBSId + "." + siblingCount.ToString() + "." + childCount.ToString();
                    service.Update(updateChildProjectTask);
                    childCount++;
                }
                siblingCount++;
            }

            //Entity updateProjectServiceTask = new Entity(entity.LogicalName, entity.Id);
            //updateProjectServiceTask["msdyn_wbsid"] = parentTaskEntityWBSId + "." + projectTaskSiblingCount.Count.ToString();


            //// throw new InvalidPluginExecutionException();
            //service.Update(updateProjectServiceTask);

            //DataCollection<Entity> lstProjectTasks;
            //lstProjectTasks = ProjectTask.GetChildRecordsForParentTask(service, entity.Id);
            //tracingService.Trace("lstProjectTasks " + lstProjectTasks.Count.ToString());
            //// throw new InvalidPluginExecutionException("child entity count "+lstProjectTasks.Count.ToString());
            //int childCount = 1;
            //foreach (Entity childProjectTask in lstProjectTasks)
            //{
            //    Entity updateChildProjectTask = new Entity(childProjectTask.LogicalName, childProjectTask.Id);
            //    updateChildProjectTask["msdyn_wbsid"] = parentTaskEntityWBSId + "." + lstprojectTaskSiblings.Count.ToString() + "." + childCount.ToString();
            //    service.Update(updateChildProjectTask);
            //    childCount++;
            //}
        }
        public static void updateQSTLevelCurrentItemIdsinSequence(IOrganizationService service, ITracingService tracingService, Entity parentTaskEntity, Entity entity)
        {

            // throw new InvalidPluginExecutionException("Custom Error");
            string parentTaskEntityWBSId = parentTaskEntity.GetAttributeValue<string>("msdyn_wbsid");
            string subject = parentTaskEntity.GetAttributeValue<string>("msdyn_subject");

            tracingService.Trace("WBS ID " + parentTaskEntityWBSId + " Subject :" + subject);
            DataCollection<Entity> projectTaskSiblingCount = ProjectTask.GetChildRecordsForParentTask(service, parentTaskEntity.Id);
            tracingService.Trace(subject + " child Count " + projectTaskSiblingCount.Count.ToString());

            Entity updateProjectServiceTask = new Entity(entity.LogicalName, entity.Id);
            updateProjectServiceTask["msdyn_wbsid"] = parentTaskEntityWBSId + "." + projectTaskSiblingCount.Count.ToString();


            // throw new InvalidPluginExecutionException();
            service.Update(updateProjectServiceTask);

            DataCollection<Entity> lstProjectTasks;
            lstProjectTasks = ProjectTask.GetChildRecordsForParentTask(service, entity.Id);
            tracingService.Trace("lst of Child ProjectTasks " + lstProjectTasks.Count.ToString());
            // throw new InvalidPluginExecutionException("child entity count "+lstProjectTasks.Count.ToString());
            int childCount = 1;
            foreach (Entity childProjectTask in lstProjectTasks)
            {
                tracingService.Trace("Child Task " + childCount.ToString() + " : Subject " + childProjectTask.GetAttributeValue<string>("msdyn_subject"));
                Entity updateChildProjectTask = new Entity(childProjectTask.LogicalName, childProjectTask.Id);
                updateChildProjectTask["msdyn_wbsid"] = parentTaskEntityWBSId + "." + projectTaskSiblingCount.Count.ToString();
                service.Update(updateChildProjectTask);
                childCount++;
            }
        }

        public static void updateQSTLevelPreviousItemIdsinSequence(IOrganizationService service, ITracingService tracingService, Entity parentTaskEntity, Entity entity)
        {


            string parentTaskEntityWBSId = parentTaskEntity.GetAttributeValue<string>("msdyn_wbsid");
            tracingService.Trace("WBS ID " + parentTaskEntityWBSId);
            DataCollection<Entity> lstprojectTaskSiblings = ProjectTask.GetChildRecordsForParentTask(service, parentTaskEntity.Id);
            tracingService.Trace("Parent Task Count " + lstprojectTaskSiblings.Count.ToString());

            int siblingCount = 1;
            foreach (Entity projectTask in lstprojectTaskSiblings)
            {
                Entity updateSiblingProjectTask = new Entity(projectTask.LogicalName, projectTask.Id);
                updateSiblingProjectTask["msdyn_wbsid"] = parentTaskEntityWBSId + "." + siblingCount.ToString();
                service.Update(updateSiblingProjectTask);




                DataCollection<Entity> lstProjectTasks;
                lstProjectTasks = ProjectTask.GetChildRecordsForParentTask(service, projectTask.Id);
                tracingService.Trace("lstProjectTasks " + lstProjectTasks.Count.ToString());
                // throw new InvalidPluginExecutionException("child entity count "+lstProjectTasks.Count.ToString());
                int childCount = 1;
                foreach (Entity childProjectTask in lstProjectTasks)
                {
                    Entity updateChildProjectTask = new Entity(childProjectTask.LogicalName, childProjectTask.Id);
                    updateChildProjectTask["msdyn_wbsid"] = parentTaskEntityWBSId + "." + siblingCount.ToString();
                    service.Update(updateChildProjectTask);
                    childCount++;
                }
                siblingCount++;
            }
        }
        public static ProjectTask GetProjectTaskRelatedToWOST(IOrganizationService organizationService, ITracingService tracingService, Guid wostGuid)
        {
            List<ProjectTask> lstProjectTasks = new List<ProjectTask>();
            var fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='msdyn_projecttask'>
                                    <attribute name='msdyn_description' />

                                  <filter type='and'>
                                  <condition attribute='ap360_workorderservicetaskid' operator='eq' value='" + wostGuid + @"' /> 
                                </filter>
                              </entity>
                            </fetch>");


            var fetchExpression = new FetchExpression(fetchXml);
            EntityCollection fetchResult = organizationService.RetrieveMultiple(fetchExpression);
            ProjectTask projectTask = null;
            tracingService.Trace("Project Tasks Count " + fetchResult.Entities.Count.ToString());
            foreach (Entity entity in fetchResult.Entities)
            {

                projectTask = new ProjectTask();
                projectTask.guid = entity.Id;
                projectTask.Description = entity.GetAttributeValue<string>("msdyn_description");


            }
            //  tracingService.Trace("List Count " + lstProjectTasks.Count.ToString());
            return projectTask;
        }
        public static ProjectTask GetProjectTaskRelatedToWorkOrder(IOrganizationService organizationService, ITracingService tracingService, Guid workOrderGuid)
        {
            tracingService.Trace("GetProjectTaskRelatedToWorkOrder");
            List<ProjectTask> lstProjectTasks = new List<ProjectTask>();
            var fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='msdyn_projecttask'>
                                    <attribute name='msdyn_description' />
                                  <filter type='and'>
                                  <condition attribute='ap360_workorderid' operator='eq' value='" + workOrderGuid + @"' /> 
                                </filter>
                              </entity>
                            </fetch>");


            var fetchExpression = new FetchExpression(fetchXml);
            EntityCollection fetchResult = organizationService.RetrieveMultiple(fetchExpression);
            ProjectTask projectTask = null;
            tracingService.Trace("Project Tasks Count " + fetchResult.Entities.Count.ToString());
            foreach (Entity entity in fetchResult.Entities)
            {

                projectTask = new ProjectTask();
                projectTask.guid = entity.Id;
                projectTask.Description = entity.GetAttributeValue<string>("msdyn_description");

            }
            //  tracingService.Trace("List Count " + lstProjectTasks.Count.ToString());
            return projectTask;
        }

        public static ProjectTask GetProjectTaskRelatedToQuote(IOrganizationService organizationService, ITracingService tracingService, Guid quoteGuid)
        {
            List<ProjectTask> lstProjectTasks = new List<ProjectTask>();
            var fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='msdyn_projecttask'>
                                    <attribute name='msdyn_description' />
                                  <filter type='and'>
                                  <condition attribute='ap360_quoteid' operator='eq' value='" + quoteGuid + @"' /> 
                                </filter>
                              </entity>
                            </fetch>");


            var fetchExpression = new FetchExpression(fetchXml);
            EntityCollection fetchResult = organizationService.RetrieveMultiple(fetchExpression);
            ProjectTask projectTask = null;
            tracingService.Trace("Project Tasks Count " + fetchResult.Entities.Count.ToString());
            foreach (Entity entity in fetchResult.Entities)
            {

                projectTask = new ProjectTask();
                projectTask.guid = entity.Id;
                projectTask.Description = entity.GetAttributeValue<string>("msdyn_description");

            }
            //  tracingService.Trace("List Count " + lstProjectTasks.Count.ToString());
            return projectTask;
        }

        public static ProjectTask GetProjectTaskRelatedToQuoteService(IOrganizationService organizationService, ITracingService tracingService, Guid quoteServiceGuid)
        {
            List<ProjectTask> lstProjectTasks = new List<ProjectTask>();
            var fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='msdyn_projecttask'>
                                    <attribute name='msdyn_description' />
                                  <filter type='and'>
                                  <condition attribute='ap360_quoteserviceid' operator='eq' value='" + quoteServiceGuid + @"' /> 
                                </filter>
                              </entity>
                            </fetch>");


            var fetchExpression = new FetchExpression(fetchXml);
            EntityCollection fetchResult = organizationService.RetrieveMultiple(fetchExpression);
            ProjectTask projectTask = null;
            tracingService.Trace("Project Tasks Count " + fetchResult.Entities.Count.ToString());
            foreach (Entity entity in fetchResult.Entities)
            {

                projectTask = new ProjectTask();
                projectTask.guid = entity.Id;
                projectTask.Description = entity.GetAttributeValue<string>("msdyn_description");

            }
            //  tracingService.Trace("List Count " + lstProjectTasks.Count.ToString());
            return projectTask;
        }

        //Entity updateProjectServiceTask = new Entity(entity.LogicalName, entity.Id);
        //updateProjectServiceTask["msdyn_wbsid"] = parentTaskEntityWBSId + "." + projectTaskSiblingCount.Count.ToString();


        //// throw new InvalidPluginExecutionException();
        //service.Update(updateProjectServiceTask);

        //DataCollection<Entity> lstProjectTasks;
        //lstProjectTasks = ProjectTask.GetChildRecordsForParentTask(service, entity.Id);
        //tracingService.Trace("lstProjectTasks " + lstProjectTasks.Count.ToString());
        //// throw new InvalidPluginExecutionException("child entity count "+lstProjectTasks.Count.ToString());
        //int childCount = 1;
        //foreach (Entity childProjectTask in lstProjectTasks)
        //{
        //    Entity updateChildProjectTask = new Entity(childProjectTask.LogicalName, childProjectTask.Id);
        //    updateChildProjectTask["msdyn_wbsid"] = parentTaskEntityWBSId + "." + lstprojectTaskSiblings.Count.ToString() + "." + childCount.ToString();
        //    service.Update(updateChildProjectTask);
        //    childCount++;
        //}


    }
}
