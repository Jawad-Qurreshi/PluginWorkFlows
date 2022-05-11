using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code.Model
{
    public class QuoteService
    {


        public Guid guid { get; set; }
        public string Name { get; set; }
        public string ActualWorkRequested { get; set; }
        public EntityReference ServiceRole { get; set; }
        public int EstimatedTime { get; set; }

        public Money PartsSalePrice { get; set; }
        public EntityReference ParentServiceTemplate { get; set; }

        public EntityReference ServiceTemplate { get; set; }
        public EntityReference ChildServiceTemplate { get; set; }

        public EntityReference ParentServiceTask { get; set; }

        public EntityReference ServiceProductMapping { get; set; }
        public EntityReference GGParentProduct { get; set; }
        public EntityReference GParentProduct { get; set; }

        public Money HourlyRate { get; set; }
        public int wbsID { get; set; }
        public bool IsQSWorkOrderCreated { get; set; }
        public bool IsQSRevisedQuoteServiceCreated { get; set; }
        public bool IsQsProjecttaskCreated { get; set; }
        public OptionSetValue QuoteServiceType { get; set; }

        public List<QuoteProduct> lstQuoteProduct { get; set; }
        public List<QuoteServiceTask> lstquoteServiceTask { get; set; }
        public List<QuoteSublet> lstQuoteSublet { get; set; }

        //public static QuoteService CreateQuoteServicesAndAttachToQuote(IOrganizationService service, IncidentTypeService incidentTypeService, Guid quoteGuid)
        //{
        //    List<QuoteService> lstQuoteService = new List<QuoteService>();

        //    //foreach (IncidentTypeService incidentTypeService in lstIncidentTypeService)
        //    //{
        //    QuoteService quoteService = new QuoteService();
        //    Entity entity = new Entity("ap360_quoteservice");
        //    entity["ap360_name"] = "Service  " + incidentTypeService.Name != null ? incidentTypeService.Name : "--";
        //    entity["ap360_quoteid"] = new EntityReference("quote", quoteGuid);
        //    quoteService.guid = service.Create(entity);
        //    // lstQuoteService.Add(quoteService);
        //    // }
        //    return quoteService;

        //}
        public static int GetIsQSWOCreatedQuoteServicesCount(List<QuoteService> lstQuoteServices)

        {
            int IsQSWOCreatedQuoteServicesCount = 0;
            foreach (QuoteService quoteSRV in lstQuoteServices)
            {
                if (quoteSRV.IsQSWorkOrderCreated)
                {
                    IsQSWOCreatedQuoteServicesCount++;
                }
            }
            return IsQSWOCreatedQuoteServicesCount;
        }
        public static int GetIsQSRevisedQSCreatedCount(List<QuoteService> lstQuoteServices)

        {
            int IsQSRevisedQSCreatedCount = 0;
            foreach (QuoteService quoteSRV in lstQuoteServices)
            {
                if (quoteSRV.IsQSRevisedQuoteServiceCreated)
                {
                    IsQSRevisedQSCreatedCount++;
                }
            }
            return IsQSRevisedQSCreatedCount;
        }
        public static List<QuoteService> GetQuoteServices(IOrganizationService service, ITracingService tracing, Guid quoteGuid)
        {


            List<QuoteService> lstQuoteServices = new List<QuoteService>();

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='ap360_quoteservice'>
                                <attribute name='ap360_quoteserviceid' />
                                <attribute name='ap360_serviceroleid' />
                                <attribute name='ap360_parentservicetemplateid' />
                                <attribute name='ap360_isqsrevisedquoteservicecreated' />
                                <attribute name='ap360_servicetemplateid' />
                                <attribute name='ap360_childservicetemplateid' />
                                <attribute name='ap360_parentservicetaskid' />
                                <attribute name='ap360_serviceproductmappingid' />
                                <attribute name='ap360_ggparentproductid' />
                                <attribute name='ap360_gparentproductid' />
                                <attribute name='ap360_hourlyrate' />
                                <attribute name='ap360_actualworkrequested' />
                                <attribute name='ap360_isqsworkordercreated' />
                                <attribute name='ap360_isqsprojecttaskcreated' />
                                <attribute name='ap360_quoteservicetype' />


                                <attribute name='ap360_wbsid' />

                                <attribute name='ap360_workrequested' />
                                <attribute name='createdon' />
                                <order attribute='ap360_wbsid' descending='false' />
                              
                                <filter type='and'>
                                  <condition attribute='ap360_quoteid' operator='eq'  value='" + quoteGuid + @"' /> 
                                </filter>
                              </entity>
                            </fetch>");



            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (col.Entities.Count > 0)
            {
                tracing.Trace(col.Entities.Count.ToString() + " Quote Services reterived");

                QuoteService quoteService;
                foreach (Entity entity in col.Entities)
                {
                    quoteService = new QuoteService();
                    quoteService.guid = entity.Id;
                    quoteService.ActualWorkRequested = entity.GetAttributeValue<string>("ap360_actualworkrequested");

                    quoteService.Name = entity.GetAttributeValue<string>("ap360_workrequested") != null ? entity.GetAttributeValue<string>("ap360_workrequested") : null;
                    quoteService.ServiceRole = entity.GetAttributeValue<EntityReference>("ap360_serviceroleid") != null ? entity.GetAttributeValue<EntityReference>("ap360_serviceroleid") : null;
                    quoteService.ParentServiceTemplate = entity.GetAttributeValue<EntityReference>("ap360_parentservicetemplateid") != null ? entity.GetAttributeValue<EntityReference>("ap360_parentservicetemplateid") : null;
                    quoteService.ServiceTemplate = entity.GetAttributeValue<EntityReference>("ap360_servicetemplateid") != null ? entity.GetAttributeValue<EntityReference>("ap360_servicetemplateid") : null;
                    quoteService.ChildServiceTemplate = entity.GetAttributeValue<EntityReference>("ap360_childservicetemplateid") != null ? entity.GetAttributeValue<EntityReference>("ap360_childservicetemplateid") : null;
                    quoteService.ParentServiceTask = entity.GetAttributeValue<EntityReference>("ap360_parentservicetaskid") != null ? entity.GetAttributeValue<EntityReference>("ap360_parentservicetaskid") : null;
                    quoteService.HourlyRate = entity.GetAttributeValue<Money>("ap360_hourlyrate") != null ? entity.GetAttributeValue<Money>("ap360_hourlyrate") : null;
                    quoteService.IsQSWorkOrderCreated = entity.GetAttributeValue<bool>("ap360_isqsworkordercreated");
                    quoteService.IsQSRevisedQuoteServiceCreated = entity.GetAttributeValue<bool>("ap360_isqsrevisedquoteservicecreated");
                    quoteService.IsQsProjecttaskCreated = entity.GetAttributeValue<bool>("ap360_isqsprojecttaskcreated");
                    if (entity.GetAttributeValue<OptionSetValue>("ap360_quoteservicetype") != null)
                        quoteService.QuoteServiceType = entity.GetAttributeValue<OptionSetValue>("ap360_quoteservicetype");


                    quoteService.ServiceProductMapping = entity.GetAttributeValue<EntityReference>("ap360_serviceproductmappingid") != null ? entity.GetAttributeValue<EntityReference>("ap360_serviceproductmappingid") : null;
                    quoteService.GGParentProduct = entity.GetAttributeValue<EntityReference>("ap360_ggparentproductid") != null ? entity.GetAttributeValue<EntityReference>("ap360_ggparentproductid") : null;
                    quoteService.GParentProduct = entity.GetAttributeValue<EntityReference>("ap360_gparentproductid") != null ? entity.GetAttributeValue<EntityReference>("ap360_gparentproductid") : null;
                    quoteService.wbsID = entity.GetAttributeValue<int>("ap360_wbsid");
                    if (quoteService.ChildServiceTemplate != null)
                    {
                        tracing.Trace("Child Service Template " + quoteService.ChildServiceTemplate.Name);
                    }
                    //quoteService.PartsSalePrice = entity.GetAttributeValue<Money>("ap360_partssaleprice");

                    //quoteService.Name = entity.GetAttributeValue<string>("ap360_name") != null ? entity.GetAttributeValue<string>("ap360_name") : null;
                    //quoteService.EstimatedTime = entity.GetAttributeValue<int>("ap360_estimatedtime");

                    lstQuoteServices.Add(quoteService);

                }
            }
            else
            {
                tracing.Trace(col.Entities.Count.ToString() + " Quote Services reterived");

            }

            return lstQuoteServices;

        }


        public static int getCountOfQuoteSericesWhereProjectTaskCreate(List<QuoteService> lstQuoteService)
        {
            int count = 0;
            foreach (QuoteService quoteService in lstQuoteService)
            {

                if (quoteService.IsQsProjecttaskCreated)
                {
                    count++;
                }
            }
            return count;
        }
        public static Guid CreateQuoteService(IOrganizationService service, ITracingService tracing, IncidentTypeService incidentTypeService, Guid quoteGuid)
        {

            Entity quoteService = new Entity("ap360_quoteservice");
            quoteService["ap360_workrequested"] = incidentTypeService.Name;

            if (incidentTypeService.ParentServiceTemplate != null)
                quoteService["ap360_parentservicetemplateid"] = new EntityReference("ap360_parentservicetemplatetype", incidentTypeService.ParentServiceTemplate.Id);
            if (incidentTypeService.ServiceTemplate != null)
                quoteService["ap360_servicetemplateid"] = new EntityReference("ap360_servicetemplatetype", incidentTypeService.ServiceTemplate.Id);
            if (incidentTypeService.ChildServiceTemplate != null)
                quoteService["ap360_childservicetemplateid"] = new EntityReference("ap360_servicetemplatetype", incidentTypeService.ChildServiceTemplate.Id);
            if (incidentTypeService.ParentServiceTask != null)
                quoteService["ap360_parentservicetaskid"] = new EntityReference("ap360_servicetemplatetype", incidentTypeService.ParentServiceTask.Id);

            quoteService["ap360_serviceroleid"] = new EntityReference("bookableresourcecategory", incidentTypeService.ServiceRole.Id);
            quoteService["ap360_quoteid"] = new EntityReference("quote", quoteGuid);
            Guid newlyCreatedQuoteServiceGuid = service.Create(quoteService);
            return newlyCreatedQuoteServiceGuid;
        }


        public static Guid CreateQuoteServiceForReviseQuote(IOrganizationService service, ITracingService tracing, Guid quoteGuid, QuoteService quoteService)
        {

            Entity newQuoteService = new Entity("ap360_quoteservice");
            newQuoteService["ap360_actualworkrequested"] = quoteService.ActualWorkRequested;
            newQuoteService["ap360_workrequested"] = quoteService.Name;

            if (quoteService.ParentServiceTemplate != null)
                newQuoteService["ap360_parentservicetemplateid"] = new EntityReference("ap360_parentservicetemplatetype", quoteService.ParentServiceTemplate.Id);
            if (quoteService.ServiceTemplate != null)
                newQuoteService["ap360_servicetemplateid"] = new EntityReference("ap360_servicetemplatetype", quoteService.ServiceTemplate.Id);

            //  throw new InvalidPluginExecutionException(quoteService.ChildServiceTemplate.Name.ToString());
            if (quoteService.ChildServiceTemplate != null)
                newQuoteService["ap360_childservicetemplateid"] = new EntityReference("ap360_servicetemplatetype", quoteService.ChildServiceTemplate.Id);
            if (quoteService.ParentServiceTask != null)
                newQuoteService["ap360_parentservicetaskid"] = new EntityReference("ap360_servicetemplatetype", quoteService.ParentServiceTask.Id);
            if (quoteService.QuoteServiceType != null)
                newQuoteService["ap360_quoteservicetype"] = quoteService.QuoteServiceType;

            newQuoteService["ap360_serviceroleid"] = new EntityReference("bookableresourcecategory", quoteService.ServiceRole.Id);
            newQuoteService["ap360_quoteid"] = new EntityReference("quote", quoteGuid);
            Guid newlyCreatedQuoteServiceGuid = service.Create(newQuoteService);
            return newlyCreatedQuoteServiceGuid;
        }
    }

}
