using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code.Model
{
    public class Project
    {

        public Guid guid { get; set; }
        public EntityReference Opportunity { get; set; }


        public static Guid CreateProject(IOrganizationService service, ITracingService tracingService, EntityReference accountRef, string opportunityTopic, Guid opportunityGuid, string isWorkOrderQuote)
        {
            Guid newlyCreatedProjectGuid = Guid.Empty;

            Entity createProject = new Entity("msdyn_project");

            if (accountRef != null)
            {
                createProject["msdyn_customer"] = accountRef;
            }
            createProject["ap360_opportunityid"] = new EntityReference("opportunity", opportunityGuid);
            if (isWorkOrderQuote == "quote")
            {
                createProject["ap360_projectrelatedto"] = new OptionSetValue(126300000);//Quote
                createProject["msdyn_subject"] = opportunityTopic + " Quote Project";
            }
            else if (isWorkOrderQuote == "workorder")
            {

                createProject["ap360_projectrelatedto"] = new OptionSetValue(126300001);//Work ORder
                createProject["msdyn_subject"] = opportunityTopic + " Work Order Project";
            }
            //createProject["ap360_currentwbsid"] = 0;

            newlyCreatedProjectGuid = service.Create(createProject);



            return newlyCreatedProjectGuid;


        }
        public static string GetProjectCurrentWbSId(IOrganizationService service, ITracingService tracingService, Guid projectGuid)
        {
            string currentWbSId = null;
            Entity projectEntity = service.Retrieve("msdyn_project", projectGuid, new Microsoft.Xrm.Sdk.Query.ColumnSet("ap360_currentwbsid"));
            if (projectEntity != null)
            {
                currentWbSId = projectEntity.GetAttributeValue<string>("ap360_currentwbsid");

            }
            return currentWbSId;
        }

        public static void updateProjectWBSId(IOrganizationService service, ITracingService tracingService, Guid ProjectGuid, string updatedWBSId)
        {

            Entity updateProject = new Entity("msdyn_project");
            updateProject.Id = ProjectGuid;
            updateProject["ap360_currentwbsid"] = updatedWBSId;
            service.Update(updateProject);
        }

        public static Entity GetQuoteProjectofOpportunity(IOrganizationService service, ITracingService tracingService, Guid opportunityGuid)
        {
            List<BookableResourceBooking> lstBookableResourceBooking = new List<BookableResourceBooking>();
            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='msdyn_project'>
                                    <attribute name='msdyn_projectid' />
                                    <attribute name='msdyn_subject' />
                                    <attribute name='createdon' />
                                    <order attribute='msdyn_subject' descending='false' />
                                    <filter type='and'>
                                      <condition attribute='ap360_opportunityid' operator='eq'  value='" + opportunityGuid + @"' /> 
                                      <condition attribute='ap360_projectrelatedto' operator='eq' value='126300000' />
                                    </filter>
                                  </entity>
                                </fetch>");

            //Project Related To 126300000 Quote

            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            Entity quoteProject = null;
            if (col.Entities.Count > 0)
            {
                quoteProject = col.Entities[0];
            }

            return quoteProject;


        }
        public static Entity GetWorkOrderProjectofOpportunity(IOrganizationService service, ITracingService tracingService, Guid opportunityGuid)
        {
            List<BookableResourceBooking> lstBookableResourceBooking = new List<BookableResourceBooking>();
            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='msdyn_project'>
                                    <attribute name='msdyn_projectid' />
                                    <attribute name='msdyn_subject' />
                                    <attribute name='createdon' />
                                    <order attribute='msdyn_subject' descending='false' />
                                    <filter type='and'>
                                      <condition attribute='ap360_opportunityid' operator='eq'  value='" + opportunityGuid + @"' /> 
                                      <condition attribute='ap360_projectrelatedto' operator='eq' value='126300001' />
                                    </filter>
                                  </entity>
                                </fetch>");

            //Project Related To 126300001 WorkORder

            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            Entity workOrderProject = null;
            if (col.Entities.Count > 0)
            {
                workOrderProject = col.Entities[0];
            }

            return workOrderProject;


        }

        public static List<Entity> GetWOProjectRelatedToOpportunity(IOrganizationService service, ITracingService tracingService, Guid opportunityGuid)
        {



            List<Entity> lstEntities = new List<Entity>();

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='msdyn_project'>
                                    <attribute name='msdyn_projectid' />
                                    <attribute name='msdyn_subject' />
                                    <attribute name='createdon' />
                                    <order attribute='msdyn_subject' descending='false' />
                                    <filter type='and'>
                                      <condition attribute='ap360_projectrelatedto' operator='eq' value='126300001' />
                                      <condition attribute='ap360_opportunityid' operator='eq'  value='" + opportunityGuid + @"' /> 
                                    </filter>
                                  </entity>
                                </fetch>");

            // WorkORder   126300001



            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));

            foreach (Entity ent in col.Entities)
            {

                lstEntities.Add(ent);

            }
            return lstEntities;
        }

        public static string getLastModifiedOnBookingSelectedSubstatusForWorkOrder(IOrganizationService service, ITracingService tracingService, Guid workOrderId)
        {
            string ap360_woselectedsubstatus = null;
            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='bookableresourcebooking'>
                                    <attribute name='createdon' />
                                    <attribute name='starttime' />
                                    <attribute name='resource' />
                                    <attribute name='endtime' />
                                    <attribute name='duration' />
                                    <attribute name='bookingtype' />
                                    <attribute name='bookingstatus' />
                                    <attribute name='bookableresourcebookingid' />
                                    <attribute name='ap360_woselectedsubstatus' />
                                    <attribute name='modifiedon' />
                                    <order attribute='modifiedon' descending='false' />
                                    <filter type='and'>
                                      <condition attribute='msdyn_workorder' operator='eq'  value='" + workOrderId + @"' /> 
                                      <condition attribute='ap360_woselectedsubstatus' operator='not-null' />
                                    </filter>
                                  </entity>
                                </fetch>");



            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            // tracingservice.Trace("count of WorkOrderServiceTask " + col.Entities.Count.ToString());

            if (col.Entities.Count > 0)
            {
                int count = col.Entities.Count;

                ap360_woselectedsubstatus = col.Entities[count - 1].GetAttributeValue<string>("ap360_woselectedsubstatus");
            }

            return ap360_woselectedsubstatus;

        }
    }
}
