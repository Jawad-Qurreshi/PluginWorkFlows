using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code.Model
{
    public class IncidentTypeServiceTask
    {
        public Guid guid { get; set; }
        public string Name { get; set; }
        public EntityReference TaskType { get; set; }


        // public int EstimatedDuration { get; set; }
        public EntityReference IncidentType { get; set; }
        public int? LineOrder { get; set; }
        public EntityReference Owner { get; set; }

        public String Description { get; set; }


        public EntityReference ParentServiceTask { get; set; }
        public EntityReference ServiceTask { get; set; }
        public string WorkRequested { get; set; }
        public int EstimatedDuration { get; set; }




        public static List<IncidentTypeServiceTask> GetIncidentTypeServiceTask(IOrganizationService service, Guid serviceGuid)
        {


            List<IncidentTypeServiceTask> lstIncidentTypeServiceTasks = new List<IncidentTypeServiceTask>();

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='msdyn_incidenttypeservicetask'>
                                    <attribute name='msdyn_name' />
                                    <attribute name='msdyn_tasktype' />
                                    <attribute name='msdyn_estimatedduration' />
                                    <attribute name='msdyn_incidenttype' />
                                    <attribute name='msdyn_lineorder' />

                                    <attribute name='ap360_parentservicetaskid' />
                                    <attribute name='ap360_servicetaskid' />
                                    <attribute name='ap360_workrequested' />
                                    <attribute name='msdyn_estimatedduration' />

                                    <attribute name='msdyn_description' />

                                    <filter type='and'>
                                      <condition attribute='ap360_incidenttypeserviceid' operator='eq'  value='" + serviceGuid + @"' /> 
                                    </filter>
                                  </entity>
                                </fetch>");

            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            IncidentTypeServiceTask incidentTypeServiceTask;
            foreach (Entity entity in col.Entities)
            {
                incidentTypeServiceTask = new IncidentTypeServiceTask();
                incidentTypeServiceTask.guid = entity.Id;
                incidentTypeServiceTask.Name = entity.GetAttributeValue<string>("msdyn_name") != null ? entity.GetAttributeValue<string>("msdyn_name") : null;
                incidentTypeServiceTask.TaskType = entity.GetAttributeValue<EntityReference>("msdyn_tasktype") != null ? entity.GetAttributeValue<EntityReference>("msdyn_tasktype") : null;
                //IncidentTypeServiceTask.Unit = entity.GetAttributeValue<EntityReference>("msdyn_unit") != null ? entity.GetAttributeValue<EntityReference>("msdyn_unit") : null;
                incidentTypeServiceTask.IncidentType = entity.GetAttributeValue<EntityReference>("msdyn_incidenttype") != null ? entity.GetAttributeValue<EntityReference>("msdyn_incidenttype") : null;

                // Quantity is missing
                incidentTypeServiceTask.ParentServiceTask = entity.GetAttributeValue<EntityReference>("ap360_parentservicetaskid") != null ? entity.GetAttributeValue<EntityReference>("ap360_parentservicetaskid") : null;
                incidentTypeServiceTask.ServiceTask = entity.GetAttributeValue<EntityReference>("ap360_servicetaskid") != null ? entity.GetAttributeValue<EntityReference>("ap360_servicetaskid") : null;
                  incidentTypeServiceTask.EstimatedDuration = entity.GetAttributeValue<int>("msdyn_estimatedduration");
                incidentTypeServiceTask.WorkRequested = entity.GetAttributeValue<string>("ap360_workrequested");



                incidentTypeServiceTask.Owner = entity.GetAttributeValue<EntityReference>("ownerid") != null ? entity.GetAttributeValue<EntityReference>("ownerid") : null;
                incidentTypeServiceTask.Description = entity.GetAttributeValue<string>("msdyn_description") != null ? entity.GetAttributeValue<string>("msdyn_description") : null;

                lstIncidentTypeServiceTasks.Add(incidentTypeServiceTask);
            }
            return lstIncidentTypeServiceTasks;

        }



    }
}
