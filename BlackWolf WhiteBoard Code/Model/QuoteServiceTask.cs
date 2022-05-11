using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code.Model
{
    public class QuoteServiceTask
    {


        public Guid guid { get; set; }
        public string Name { get; set; }
        public int EstimatedTime { get; set; }
        public EntityReference ParentServiceTemplate { get; set; }
        public EntityReference ServiceTemplate { get; set; }

        public EntityReference ChildServiceTemplate { get; set; }
        public EntityReference ServiceRole { get; set; }
        public Money HourlyRate { get; set; }
        public EntityReference QuoteService { get; set; }
        public string CustomNotes { get; set; }
        public EntityReference ParentServiceTask { get; set; }

        public EntityReference ServiceTask { get; set; }

        public bool Removetemplatehierarchy { get; set; }
        //public static void CreateQuoteServiceAndAttachToQuote(IOrganizationService service, List<IncidentTypeServiceTask> lstIncidentTypeServiceTask, Guid quoteServiceGuid)
        //{
        //    foreach (IncidentTypeServiceTask incidentTypeServiceTask in lstIncidentTypeServiceTask)
        //    {
        //        Entity entity = new Entity("ap360_quoteservicetask");
        //        entity["ap360_name"] = "Service Task " + incidentTypeServiceTask.Name != null ? incidentTypeServiceTask.Name : "--";
        //        entity["ap360_quoteserviceid"] = new EntityReference("ap360_quoteservice", quoteServiceGuid);


        //        service.Create(entity);

        //    }

        //}

        public static List<QuoteServiceTask> GetQuoteServiceTasks(IOrganizationService service, Guid serviceGuid)
        {

            List<QuoteServiceTask> lstquoteservicetasks = new List<QuoteServiceTask>();

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='ap360_quoteservicetask'>
                                    <attribute name='ap360_quoteservicetaskid' />
                                    <attribute name='ap360_workrequested' />
                                    <attribute name='createdon' />
                                    <attribute name='ap360_estimatedtime' />
                                    <attribute name='ap360_customnotes' />
                                    <attribute name='ap360_servicetaskid' />


                                    <attribute name='ap360_parentservicetemplateid' />
                                    <attribute name='ap360_servicetemplateid' />
                                    <attribute name='ap360_childservicetemplateid' />
                                    <attribute name='ap360_parentservicetaskid' />

                                    <attribute name='ap360_serviceroleid' />
                                    <attribute name='ap360_hourlyrate' />
                                    <attribute name='ap360_removetemplatehierarchy' />

                                    <attribute name='ap360_wbsid' />

                                    <order attribute='ap360_wbsid' descending='false' />
                                    <filter type='and'>
                                      <condition attribute='ap360_quoteserviceid' operator='eq'  value='" + serviceGuid + @"' /> 
                                    </filter>
                                  </entity>
                                </fetch>");



            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            QuoteServiceTask quoteservicetask;
            foreach (Entity entity in col.Entities)
            {
                quoteservicetask = new QuoteServiceTask();
                quoteservicetask.guid = entity.Id;

                quoteservicetask.Name = entity.GetAttributeValue<string>("ap360_workrequested") != null ? entity.GetAttributeValue<string>("ap360_workrequested") : null;
                quoteservicetask.EstimatedTime = entity.GetAttributeValue<int>("ap360_estimatedtime");

                quoteservicetask.ParentServiceTemplate = entity.GetAttributeValue<EntityReference>("ap360_parentservicetemplateid") != null ? entity.GetAttributeValue<EntityReference>("ap360_parentservicetemplateid") : null;
                quoteservicetask.ServiceTemplate = entity.GetAttributeValue<EntityReference>("ap360_servicetemplateid") != null ? entity.GetAttributeValue<EntityReference>("ap360_servicetemplateid") : null;
                quoteservicetask.ChildServiceTemplate = entity.GetAttributeValue<EntityReference>("ap360_childservicetemplateid") != null ? entity.GetAttributeValue<EntityReference>("ap360_childservicetemplateid") : null;
                quoteservicetask.ParentServiceTask = entity.GetAttributeValue<EntityReference>("ap360_parentservicetaskid") != null ? entity.GetAttributeValue<EntityReference>("ap360_parentservicetaskid") : null;
                quoteservicetask.ServiceTask = entity.GetAttributeValue<EntityReference>("ap360_servicetaskid") != null ? entity.GetAttributeValue<EntityReference>("ap360_servicetaskid") : null;
                quoteservicetask.ServiceRole = entity.GetAttributeValue<EntityReference>("ap360_serviceroleid") != null ? entity.GetAttributeValue<EntityReference>("ap360_serviceroleid") : null;
                quoteservicetask.HourlyRate = entity.GetAttributeValue<Money>("ap360_hourlyrate") != null ? entity.GetAttributeValue<Money>("ap360_hourlyrate") : null;
                quoteservicetask.Removetemplatehierarchy = entity.GetAttributeValue<bool>("ap360_removetemplatehierarchy");

                quoteservicetask.CustomNotes = entity.GetAttributeValue<string>("ap360_customnotes");

                quoteservicetask.EstimatedTime = entity.GetAttributeValue<int>("ap360_estimatedtime");

                lstquoteservicetasks.Add(quoteservicetask);

            }
            return lstquoteservicetasks;

        }
        public static List<QuoteServiceTask> GetQuoteServiceTasksForReviseQuote(IOrganizationService service, Guid serviceGuid)
        {

            List<QuoteServiceTask> lstquoteservicetasks = new List<QuoteServiceTask>();

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='ap360_quoteservicetask'>
                                    <attribute name='ap360_quoteservicetaskid' />
                                    <attribute name='ap360_workrequested' />
                                    <attribute name='createdon' />
                                    <attribute name='ap360_estimatedtime' />
                                    <attribute name='ap360_customnotes' />
                                    <attribute name='ap360_servicetaskid' />


                                    <attribute name='ap360_parentservicetemplateid' />
                                    <attribute name='ap360_servicetemplateid' />
                                    <attribute name='ap360_childservicetemplateid' />
                                    <attribute name='ap360_parentservicetaskid' />

                                    <attribute name='ap360_serviceroleid' />
                                    <attribute name='ap360_hourlyrate' />
                                    <attribute name='ap360_removetemplatehierarchy' />
                                    <attribute name='ap360_wbsid' />


                                    <order attribute='ap360_wbsid' descending='false' />
                                    <filter type='and'>
                                      <condition attribute='ap360_quoteserviceid' operator='eq'  value='" + serviceGuid + @"' /> 
                                    </filter>
                                  </entity>
                                </fetch>");



            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            QuoteServiceTask quoteservicetask;
            foreach (Entity entity in col.Entities)
            {
                quoteservicetask = new QuoteServiceTask();
                quoteservicetask.guid = entity.Id;

                quoteservicetask.Name = entity.GetAttributeValue<string>("ap360_workrequested") != null ? entity.GetAttributeValue<string>("ap360_workrequested") : null;
                quoteservicetask.EstimatedTime = entity.GetAttributeValue<int>("ap360_estimatedtime");

                quoteservicetask.ParentServiceTemplate = entity.GetAttributeValue<EntityReference>("ap360_parentservicetemplateid") != null ? entity.GetAttributeValue<EntityReference>("ap360_parentservicetemplateid") : null;
                quoteservicetask.ServiceTemplate = entity.GetAttributeValue<EntityReference>("ap360_servicetemplateid") != null ? entity.GetAttributeValue<EntityReference>("ap360_servicetemplateid") : null;
                quoteservicetask.ChildServiceTemplate = entity.GetAttributeValue<EntityReference>("ap360_childservicetemplateid") != null ? entity.GetAttributeValue<EntityReference>("ap360_childservicetemplateid") : null;
                quoteservicetask.ParentServiceTask = entity.GetAttributeValue<EntityReference>("ap360_parentservicetaskid") != null ? entity.GetAttributeValue<EntityReference>("ap360_parentservicetaskid") : null;
                quoteservicetask.ServiceTask = entity.GetAttributeValue<EntityReference>("ap360_servicetaskid") != null ? entity.GetAttributeValue<EntityReference>("ap360_servicetaskid") : null;
                quoteservicetask.ServiceRole = entity.GetAttributeValue<EntityReference>("ap360_serviceroleid") != null ? entity.GetAttributeValue<EntityReference>("ap360_serviceroleid") : null;
                quoteservicetask.HourlyRate = entity.GetAttributeValue<Money>("ap360_hourlyrate") != null ? entity.GetAttributeValue<Money>("ap360_hourlyrate") : null;
                quoteservicetask.Removetemplatehierarchy = entity.GetAttributeValue<bool>("ap360_removetemplatehierarchy");

                quoteservicetask.CustomNotes = entity.GetAttributeValue<string>("ap360_customnotes");

                quoteservicetask.EstimatedTime = entity.GetAttributeValue<int>("ap360_estimatedtime");

                lstquoteservicetasks.Add(quoteservicetask);

            }
            return lstquoteservicetasks;

        }

        public static void CreateQuoteServiceTasks(IOrganizationService service, ITracingService tracing, List<IncidentTypeServiceTask> lstIncidentTypeServiceTasks, Guid quoteServiceGuid)
        {

            foreach (IncidentTypeServiceTask incidentTypeServiceTask in lstIncidentTypeServiceTasks)
            {
                Entity newquoteServiceTask = new Entity("ap360_quoteservicetask");

                if (incidentTypeServiceTask.ParentServiceTask != null)
                    newquoteServiceTask["ap360_parentservicetaskid"] = new EntityReference("msdyn_servicetasktype", incidentTypeServiceTask.ParentServiceTask.Id);
                if (incidentTypeServiceTask.ServiceTask != null)
                    newquoteServiceTask["ap360_servicetaskid"] = new EntityReference("msdyn_servicetasktype", incidentTypeServiceTask.ServiceTask.Id);
                newquoteServiceTask["ap360_workrequested"] = incidentTypeServiceTask.Name;
                newquoteServiceTask["ap360_estimatedtime"] = incidentTypeServiceTask.EstimatedDuration;
                newquoteServiceTask["ap360_quoteserviceid"] = new EntityReference("ap360_quoteservice", quoteServiceGuid);




                service.Create(newquoteServiceTask);

            }


        }

        public static void CreateQuoteServiceTasksForReviseQuote(IOrganizationService service, ITracingService tracing, List<QuoteServiceTask> lstQuoteServiceTasks, Guid newlyCreatedQuoteServiceGuid)
        {
            tracing.Trace("Inside creation of Quote Service Task and count is " + lstQuoteServiceTasks.Count.ToString());
            foreach (QuoteServiceTask quoteServiceTask in lstQuoteServiceTasks)
            {
                Entity newquoteServiceTask = new Entity("ap360_quoteservicetask");
                ////////////////////////////////////////////////

                newquoteServiceTask["ap360_workrequested"] = quoteServiceTask.Name;
                newquoteServiceTask["ap360_estimatedtime"] = quoteServiceTask.EstimatedTime;


                if (quoteServiceTask.ParentServiceTemplate != null)
                {
                    newquoteServiceTask["ap360_parentservicetemplateid"] = quoteServiceTask.ParentServiceTemplate;
                }
                if (quoteServiceTask.ServiceTemplate != null)
                {
                    newquoteServiceTask["ap360_servicetemplateid"] = quoteServiceTask.ServiceTemplate;
                }
                if (quoteServiceTask.ChildServiceTemplate != null)
                {
                    newquoteServiceTask["ap360_childservicetemplateid"] = quoteServiceTask.ChildServiceTemplate;
                }
                if (quoteServiceTask.ParentServiceTask != null)
                {
                    newquoteServiceTask["ap360_parentservicetaskid"] = quoteServiceTask.ParentServiceTask;
                }
                newquoteServiceTask["ap360_customnotes"] = quoteServiceTask.CustomNotes;

                newquoteServiceTask["ap360_quoteserviceid"] = new EntityReference("ap360_quoteservice", newlyCreatedQuoteServiceGuid);

                //////////////////////////////////////////////////

                if (quoteServiceTask.ServiceTask != null)
                    newquoteServiceTask["ap360_servicetaskid"] = new EntityReference("msdyn_servicetasktype", quoteServiceTask.ServiceTask.Id);

                if (quoteServiceTask.HourlyRate != null)
                {
                    newquoteServiceTask["ap360_hourlyrate"] = quoteServiceTask.HourlyRate;
                }
                if (quoteServiceTask.ServiceRole != null)
                {
                    newquoteServiceTask["ap360_serviceroleid"] = quoteServiceTask.ServiceRole;
                }
                newquoteServiceTask["ap360_removetemplatehierarchy"] = quoteServiceTask.Removetemplatehierarchy;

                service.Create(newquoteServiceTask);

            }


        }
    }

}
