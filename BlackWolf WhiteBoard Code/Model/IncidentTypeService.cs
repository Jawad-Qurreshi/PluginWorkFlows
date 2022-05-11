using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code.Model
{
    public class IncidentTypeService
    {
        public Guid guid { get; set; }
        public string Name { get; set; }
        public EntityReference Service { get; set; }
        public EntityReference Unit { get; set; }
        public EntityReference IncidentType { get; set; }
        public EntityReference ParentServiceTemplate { get; set; }
        public EntityReference ServiceRole { get; set; }

        public EntityReference ServiceTemplate { get; set; }
        public EntityReference ChildServiceTemplate { get; set; }
        public EntityReference ParentServiceTask { get; set; }
        public String Description { get; set; }

        public int Duration { get; set; }
        public EntityReference Owner { get; set; }
        public String InternalDescription { get; set; }
        public int? LineOrder { get; set; }

        public List<IncidentTypeServiceTask> lstIncidentTypeServiceTask = new List<IncidentTypeServiceTask>();
        public List<IncidentTypeProduct> lstIncidentTypeProduct = new List<IncidentTypeProduct>();
        public static List<IncidentTypeService> GetIncidentTypeServices(IOrganizationService service, ITracingService tracingService, Guid incidentGuid)
        {

            List<IncidentTypeService> lstIncidentTypeServices = new List<IncidentTypeService>();

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='msdyn_incidenttypeservice'>
                                    <attribute name='msdyn_name' />

                                    <attribute name='ap360_parentservicetemplatetypeid' />
                                    <attribute name='ap360_servicetemplateid' />
                                    <attribute name='ap360_childservicetemplateid' />
                                    <attribute name='ap360_parentservicetaskid' />
                                    <attribute name='ap360_serviceroleid' />

                                    <attribute name='msdyn_unit' />
                                    <attribute name='msdyn_duration' />
                                    <attribute name='msdyn_internaldescription' />
                                    <attribute name='msdyn_description' />
                                    <attribute name='msdyn_lineorder' />
                                    <attribute name='msdyn_incidenttype' />
                                    <attribute name='msdyn_service' />

                                    <filter type='and'>
                                      <condition attribute='msdyn_incidenttype'  operator='eq'  value='" + incidentGuid + @"' /> 
                                    </filter>
                                  </entity>
                                </fetch>");

            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            IncidentTypeService incidentTypeService;
            if (col.Entities.Count > 0)
                tracingService.Trace("Incident Type Service Count " + col.Entities.Count.ToString());
            foreach (Entity entity in col.Entities)
            {
                incidentTypeService = new IncidentTypeService();
                incidentTypeService.guid = entity.Id;
                incidentTypeService.Name = entity.GetAttributeValue<string>("msdyn_name") != null ? entity.GetAttributeValue<string>("msdyn_name") : null;

                incidentTypeService.ParentServiceTemplate = entity.GetAttributeValue<EntityReference>("ap360_parentservicetemplatetypeid") != null ? entity.GetAttributeValue<EntityReference>("ap360_parentservicetemplatetypeid") : null;
                incidentTypeService.ServiceTemplate = entity.GetAttributeValue<EntityReference>("ap360_servicetemplateid") != null ? entity.GetAttributeValue<EntityReference>("ap360_servicetemplateid") : null;
                incidentTypeService.ChildServiceTemplate = entity.GetAttributeValue<EntityReference>("ap360_childservicetemplateid") != null ? entity.GetAttributeValue<EntityReference>("ap360_childservicetemplateid") : null;
                incidentTypeService.ParentServiceTask = entity.GetAttributeValue<EntityReference>("ap360_parentservicetaskid") != null ? entity.GetAttributeValue<EntityReference>("ap360_parentservicetaskid") : null;
                incidentTypeService.ServiceRole = entity.GetAttributeValue<EntityReference>("ap360_serviceroleid") != null ? entity.GetAttributeValue<EntityReference>("ap360_serviceroleid") : null;

                incidentTypeService.Description = entity.GetAttributeValue<string>("msdyn_internaldescription");
                incidentTypeService.Service = entity.GetAttributeValue<EntityReference>("msdyn_service") != null ? entity.GetAttributeValue<EntityReference>("msdyn_service") : null;
                incidentTypeService.Unit = entity.GetAttributeValue<EntityReference>("msdyn_unit") != null ? entity.GetAttributeValue<EntityReference>("msdyn_unit") : null;
                incidentTypeService.IncidentType = entity.GetAttributeValue<EntityReference>("msdyn_incidenttype") != null ? entity.GetAttributeValue<EntityReference>("msdyn_incidenttype") : null;

                // Duration is missing

                incidentTypeService.Owner = entity.GetAttributeValue<EntityReference>("ownerid") != null ? entity.GetAttributeValue<EntityReference>("ownerid") : null;
                incidentTypeService.InternalDescription = entity.GetAttributeValue<string>("msdyn_internaldescription") != null ? entity.GetAttributeValue<string>("msdyn_internaldescription") : null;
                incidentTypeService.LineOrder = entity.GetAttributeValue<int?>("msdyn_lineorder") != null ? entity.GetAttributeValue<int>("msdyn_lineorder") : 0;

                tracingService.Trace("Incident Type Service Reterived Completed");

                incidentTypeService.lstIncidentTypeProduct = IncidentTypeProduct.GetIncidentTypeProducts(service, tracingService, incidentTypeService.guid);
                tracingService.Trace("Incident Type Product Reterived Completed");

                incidentTypeService.lstIncidentTypeServiceTask = IncidentTypeServiceTask.GetIncidentTypeServiceTask(service, incidentTypeService.guid);
                tracingService.Trace("Incident Type Service Task Reterived Completed");



                lstIncidentTypeServices.Add(incidentTypeService);
            }
            return lstIncidentTypeServices;

        }


        public static void CreateQtSrvQtSrvProQtSrvTask(IOrganizationService service, ITracingService tracing, List<IncidentTypeService> lstIncidentTypeService, Guid quoteGuid)
        {
            tracing.Trace("Inside Creation of Quoter Service");

            foreach (IncidentTypeService incidentTypeService in lstIncidentTypeService)
            {
                tracing.Trace("Inside Foreach");

                tracing.Trace("Before Quote Service Creation");
                Guid newlyCreatedQuoteServiceGuid = QuoteService.CreateQuoteService(service, tracing, incidentTypeService, quoteGuid);
                tracing.Trace("After Quote Service Creation");


                tracing.Trace("Before Quote Product Creation");
                QuoteProduct.CreateQuoteProducts(service, tracing, incidentTypeService.lstIncidentTypeProduct, newlyCreatedQuoteServiceGuid);
                tracing.Trace("After  Quote Product Creation");

                tracing.Trace("Before Quote Service Task Creation");
                QuoteServiceTask.CreateQuoteServiceTasks(service, tracing, incidentTypeService.lstIncidentTypeServiceTask, newlyCreatedQuoteServiceGuid);
                tracing.Trace("After Quote Service Task Creation");




            }

        }
    }
}
