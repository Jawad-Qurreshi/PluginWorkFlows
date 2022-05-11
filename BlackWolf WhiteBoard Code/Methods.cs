using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class Methods
    {
        public static void MapIncidentTypeSrvSrvTasksSrvProds(IOrganizationService service, ITracingService tracingService, EntityReference incidentType, Guid quoteGuid)
        {
            tracingService.Trace("Before Reterving Records");
            List<IncidentTypeService> lstIncidentTypeService = new List<IncidentTypeService>();
            lstIncidentTypeService = IncidentTypeService.GetIncidentTypeServices(service, tracingService, incidentType.Id);
            tracingService.Trace("After Reterving Records");
            tracingService.Trace("Before Creating Records");

            IncidentTypeService.CreateQtSrvQtSrvProQtSrvTask(service, tracingService, lstIncidentTypeService, quoteGuid);
            tracingService.Trace("After Creating Records");


        }
        public static void GetPreHealth(ITracingService tracingService, List<WorkOrderServiceTask> lstWorkOrderServiceTask, EntityReference currentwostRef
            , string message, Entity WOservicetasktimestamp)
        {

            tracingService.Trace("Inside GetPreHealth");
            WorkOrderServiceTask currentwost = lstWorkOrderServiceTask.First(x => x.WOSTGuid == currentwostRef.Id);
            tracingService.Trace("Current task guid is " + currentwost.WOSTGuid.ToString());
            if (currentwost == null) return;
            int TimeStampRow = 0;

            if (currentwost.WOSTTimeStamps == null)
                TimeStampRow = 1;
            else TimeStampRow = 0;

            List<WorkOrderServiceTask> lstOpporunityWOSTTimeStamp = new List<WorkOrderServiceTask>();
            List<WorkOrderServiceTask> lstWorkOrderWOSTTimeStamp = new List<WorkOrderServiceTask>();
            List<WorkOrderServiceTask> lstWorkOrderServiceTaskWOSTTimeStamp = new List<WorkOrderServiceTask>();
            //lstWorkOrderServiceTask = lstWorkOrderServiceTask.Where(x => x.WOSTTimeStamps != null && x.WOSTTimeStamps.CreatedON <= currentwost.WOSTTimeStamps.CreatedON).ToList();

            lstOpporunityWOSTTimeStamp = lstWorkOrderServiceTask.Where(x => x.WOSTTimeStamps != null).ToList();
            lstWorkOrderWOSTTimeStamp = lstWorkOrderServiceTask.Where(x => x.WOSTTimeStamps != null && x.WorkOrder.Id == currentwost.WorkOrder.Id).ToList();
            lstWorkOrderServiceTaskWOSTTimeStamp = lstWorkOrderServiceTask.Where(x => x.WOSTTimeStamps != null && x.WOSTGuid == currentwost.WOSTGuid).ToList();

            tracingService.Trace("list of timestamps count is" + lstWorkOrderServiceTask.Count.ToString());
            tracingService.Trace("list of timestamps group by WO count is" + lstWorkOrderWOSTTimeStamp.Count.ToString());
            tracingService.Trace("list of timestamps group by WOST count is" + lstWorkOrderServiceTaskWOSTTimeStamp.Count.ToString());


            WorkOrderServiceTaskTimeStamp workOrderServiceTaskTimeStamp = new WorkOrderServiceTaskTimeStamp();
            if (lstOpporunityWOSTTimeStamp.Count > 0)
            {
                if (lstOpporunityWOSTTimeStamp[0].WOSTTimeStamps != null)
                {
                    workOrderServiceTaskTimeStamp.OpportunityPreDurationHealth = lstOpporunityWOSTTimeStamp[0].WOSTTimeStamps.opportunityDurationHealth;
                    workOrderServiceTaskTimeStamp.OpportunityPreHealth = lstOpporunityWOSTTimeStamp[0].WOSTTimeStamps.opportunityPOSTHealth;
                }
            }
            if (lstWorkOrderWOSTTimeStamp.Count > 0)
            {
                if (lstWorkOrderWOSTTimeStamp[0].WOSTTimeStamps != null)
                {
                    workOrderServiceTaskTimeStamp.WorkOrderPreDurationHealth = lstWorkOrderWOSTTimeStamp[0].WOSTTimeStamps.workOrderDurationHealth;
                    workOrderServiceTaskTimeStamp.WorkOrderPreHealth = lstWorkOrderWOSTTimeStamp[0].WOSTTimeStamps.workOrderPOSTHealth;
                    //tracingService.Trace("middle GetPreHealth "+ (lstWorkOrderServiceTask.Count - exclude).ToString()+"--- exclude "+ exclude.ToString());
                }
            }

            if (lstWorkOrderServiceTaskWOSTTimeStamp.Count > 0)
            {
                if (lstWorkOrderServiceTaskWOSTTimeStamp[0].WOSTTimeStamps != null)
                {
                    workOrderServiceTaskTimeStamp.WOSTPreDurationHealth = lstWorkOrderServiceTaskWOSTTimeStamp[0].WOSTTimeStamps.wostDurationHealth;
                    workOrderServiceTaskTimeStamp.WOSTPreHealth = lstWorkOrderServiceTaskWOSTTimeStamp[0].WOSTTimeStamps.wostPOSTHealth;
                }
            }

            if (currentwost.WOSTTimeStamps != null)
            {
                //  Entity WOservicetasktimestamp = new Entity("ap360_woservicetasktimestamp", currentwost.WOSTTimeStamps.guid);
                WOservicetasktimestamp["ap360_wostpredurationhealth"] = workOrderServiceTaskTimeStamp.WOSTPreDurationHealth;
                WOservicetasktimestamp["ap360_wostprehealth"] = workOrderServiceTaskTimeStamp.WOSTPreHealth;

                WOservicetasktimestamp["ap360_workorderpredurationhealth"] = workOrderServiceTaskTimeStamp.WorkOrderPreDurationHealth;
                WOservicetasktimestamp["ap360_workorderprehealth"] = workOrderServiceTaskTimeStamp.WorkOrderPreHealth;

                WOservicetasktimestamp["ap360_opportunitypredurationhealth"] = workOrderServiceTaskTimeStamp.OpportunityPreDurationHealth;
                WOservicetasktimestamp["ap360_opportunityprehealth"] = workOrderServiceTaskTimeStamp.OpportunityPreHealth;
                // service.Update(WOservicetasktimestamp);
            }

            tracingService.Trace("end GetPreHealth");

            //return workOrderServiceTaskTimeStamp;


        }
        public static DataCollection<Entity> GetChildRecords(IOrganizationService organizationService, Guid parentEntiytId, string ParentEntityLookupFieldName, string childEntityName, string childEntityField)
        {
            var fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>"
               + "<entity name='" + childEntityName + "'>"
                + "<attribute name='" + childEntityName + "id' />"
               + "<attribute name='" + childEntityField + "' />"
               + "<filter type ='and' >"
               + "<condition attribute = '" + ParentEntityLookupFieldName + "' operator= 'eq'  value = '" + parentEntiytId + "' />"
               + "</filter >"
               + "</entity >"
                + "</fetch >";


            //+"<condition attribute = 'statecode' operator= 'eq' value = '0' />" // Active

            // tracingService.Trace("XML:" + fetchXml);

            var fetchExpression = new FetchExpression(fetchXml);
            EntityCollection fetchResult = organizationService.RetrieveMultiple(fetchExpression);
            return fetchResult.Entities;
        }

        public static void MainFunction(IOrganizationService service, ITracingService tracing, string ChildEntityName, Guid ChildEntityGuid, string ParentLookupFieldonChildEntity, string ChildEntityFieldToRetrieve, string ParentEntityfieldtoUpdate, string message, string fieldtype)
        {
            EntityReference parentEntityReference;
            tracing.Trace(ChildEntityName + "   " + ChildEntityGuid + "       " + ParentLookupFieldonChildEntity + "    " + ChildEntityFieldToRetrieve + "  " + message + "         " + fieldtype);
            Entity ChildEntity = service.Retrieve(ChildEntityName, ChildEntityGuid, new ColumnSet(ParentLookupFieldonChildEntity, ChildEntityFieldToRetrieve));

            tracing.Trace(ChildEntity.LogicalName + " After reterival ");
            int sumofChildEntityDuration = 0;
            int deleteChildEntityValue = 0;

            //if (fieldtype == "duration")
            //    tracing.Trace("Current duration of  " + ChildEntityFieldToRetrieve + " is " + ChildEntity.GetAttributeValue<int>(ChildEntityFieldToRetrieve).ToString());
            //else if (fieldtype == "money")
            //    tracing.Trace("Current amount of  " + ChildEntityFieldToRetrieve + " is " + ChildEntity.GetAttributeValue<Money>(ChildEntityFieldToRetrieve).Value.ToString());


            decimal sumofChildEntityAmount = 0;
            decimal deleteChildEntityAmount = 0;
            if (ChildEntity != null)
            {
                tracing.Trace(ChildEntity.LogicalName + " is not null");
                parentEntityReference = ChildEntity.GetAttributeValue<EntityReference>(ParentLookupFieldonChildEntity) != null ? ChildEntity.GetAttributeValue<EntityReference>(ParentLookupFieldonChildEntity) : null;
                if (message == "delete" && fieldtype == "duration")
                {
                    tracing.Trace(message + " Message and Field Type is " + fieldtype);
                    deleteChildEntityValue = ChildEntity.GetAttributeValue<int>(ChildEntityFieldToRetrieve);
                }
                else if (message == "delete" && fieldtype == "money")
                {
                    tracing.Trace(message + " Message and Field Type is " + fieldtype);
                    deleteChildEntityAmount = ChildEntity.GetAttributeValue<Money>(ChildEntityFieldToRetrieve) != null ? ChildEntity.GetAttributeValue<Money>(ChildEntityFieldToRetrieve).Value : 0;

                }
                tracing.Trace("Before Parent Entity Reference Null check");

                if (parentEntityReference == null)
                {
                    tracing.Trace("Parent Entity Reference is null");
                    return;
                }

                tracing.Trace("Parent Entity: " + parentEntityReference.LogicalName + " is not null");
                DataCollection<Entity> lstChildEntity = Methods.GetChildRecords(service, parentEntityReference.Id, ParentLookupFieldonChildEntity, ChildEntityName, ChildEntityFieldToRetrieve);
                tracing.Trace("Count of child Entity is " + lstChildEntity.Count.ToString());
                foreach (Entity childEntity in lstChildEntity)
                {
                    if (fieldtype == "duration")
                    {
                        sumofChildEntityDuration += childEntity.GetAttributeValue<int>(ChildEntityFieldToRetrieve);

                    }
                    else if (fieldtype == "money")
                        sumofChildEntityAmount += childEntity.GetAttributeValue<Money>(ChildEntityFieldToRetrieve) != null ? childEntity.GetAttributeValue<Money>(ChildEntityFieldToRetrieve).Value : 0;


                }




                Entity parentEntity = new Entity(parentEntityReference.LogicalName);
                parentEntity.Id = parentEntityReference.Id;
                if (message == "delete" && fieldtype == "duration")
                {

                    sumofChildEntityDuration = sumofChildEntityDuration - deleteChildEntityValue;
                    tracing.Trace("Sum of duration, In case of delete Child Entity " + sumofChildEntityDuration);
                }
                else if (message == "delete" && fieldtype == "money")
                {
                    sumofChildEntityAmount = sumofChildEntityAmount - deleteChildEntityAmount;
                    tracing.Trace("Sum of Amount, In case of delete Child Entity " + sumofChildEntityAmount);


                }

                if (fieldtype == "money")
                {
                    tracing.Trace("Sum of  amount is  " + sumofChildEntityAmount.ToString());

                    parentEntity[ParentEntityfieldtoUpdate] = new Money(sumofChildEntityAmount);
                    tracing.Trace(parentEntityReference.LogicalName + " field to update is " + ParentEntityfieldtoUpdate + " Value for field is " + sumofChildEntityAmount + " Parent Entity Guid :" + parentEntityReference.Id);
                }
                else if (fieldtype == "duration")
                {
                    tracing.Trace("Sum of Duration is" + sumofChildEntityDuration.ToString());

                    parentEntity[ParentEntityfieldtoUpdate] = sumofChildEntityDuration;
                    tracing.Trace(parentEntityReference.LogicalName + " field to update is " + ParentEntityfieldtoUpdate + " Value for field is " + sumofChildEntityDuration + " Parent Entity Guid :" + parentEntityReference.Id);
                }
                tracing.Trace("Before Last Step to update Entity");

                service.Update(parentEntity);
                tracing.Trace("After Last Step to update Entity");
            }

        }
        public static decimal getBookAbleResourceCategoryStandardPrice(IOrganizationService service, string resourcecatergoryName)
        {

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='bookableresourcecategory'>
                                    <attribute name='name' />
                                    <attribute name='createdon' />
                                    <attribute name='description' />
                                    <attribute name='bookableresourcecategoryid' />
                                    <attribute name='ap360_price' />
                                    <order attribute='name' descending='false' />
                                    <filter type='and'>
                                      <condition attribute='name' operator='eq' value='" + resourcecatergoryName + @"' /> 
                                    </filter>
                                  </entity>
                                </fetch>");


            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            Money priceinMoney = null;
            decimal price = 0;
            foreach (Entity entity in col.Entities)
            {
                priceinMoney = entity.GetAttributeValue<Money>("ap360_price") != null ? entity.GetAttributeValue<Money>("ap360_price") : null;

            }
            if (priceinMoney != null)
            {
                price = priceinMoney.Value;

            }
            return price;
        }

        public static decimal GetResourcePriceBasedOnBRC(IOrganizationService service, ITracingService tracing, Guid resourceGuid, string resourcecatergoryName)
        {
            tracing.Trace("Resource Guid " + resourceGuid.ToString());
            tracing.Trace("resourcecatergoryName  " + resourcecatergoryName.ToString());
            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='bookableresourcecategoryassn'>
                                    <attribute name='createdon' />
                                    <attribute name='resourcecategory' />
                                    <attribute name='resource' />
                                    <attribute name='bookableresourcecategoryassnid' />
                                    <attribute name='resourcecategory' />

                                    <order attribute='resource' descending='false' />
                                    <order attribute='resourcecategory' descending='false' />
                                    <filter type='and'>
                                      <condition attribute='resource' operator='eq'  value='" + resourceGuid + @"' /> 
                                    </filter>
                                  </entity>
                                </fetch>");


            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            decimal price = 0;
            foreach (Entity entity in col.Entities)
            {

                EntityReference resourceCategory = entity.GetAttributeValue<EntityReference>("resourcecategory") != null ? entity.GetAttributeValue<EntityReference>("resourcecategory") : null;
                if (resourceCategory.Name.Contains(resourcecatergoryName))
                {

                    Entity bookableresourceCategory = service.Retrieve(resourceCategory.LogicalName, resourceCategory.Id, new ColumnSet("ap360_price"));
                    if (bookableresourceCategory != null)
                    {
                        Money priceinMoney = bookableresourceCategory.GetAttributeValue<Money>("ap360_price") != null ? bookableresourceCategory.GetAttributeValue<Money>("ap360_price") : null;

                        if (priceinMoney != null)
                        {
                            price = priceinMoney.Value;

                        }

                    }
                }

            }
            return price;
        }
        public static void getResourseAndServiceRoleRates(IOrganizationService service, ITracingService tracingService, EntityReference ServiceRole, EntityReference Resource, ref decimal ServiceRoleHourlyRate, ref decimal ResourceHourlyRate, Entity brbEntity)
        {

            //   decimal ResourceHourlyRate = 0.0M;
            //  decimal ResourceMinuteRate = 0.0M;

            //  decimal ServiceRoleHourlyRate = 0.0M;
            // decimal ServiceRolePerMinuteRate = 0;


            if (ServiceRole == null)
            {
                tracingService.Trace("WOST Service Role is Null");

                ServiceRole = new EntityReference("bookableresourcecategory", new Guid("108c5d2f-245e-ea11-a812-000d3a30f257"));
                ResourceHourlyRate = Methods.GetResourcePriceBasedOnBRC(service, tracingService, Resource.Id, "Mechanical Technician");
                tracingService.Trace("After GetResourcePriceBasedOnBRC ");
                ServiceRoleHourlyRate = Methods.getBookAbleResourceCategoryStandardPrice(service, "Mechanical Technician");

            }
            else
            {

                tracingService.Trace("WOST Service Role Name " + ServiceRole.Name);
                ResourceHourlyRate = brbEntity.GetAttributeValue<Money>("ap360_resourcehourlyrate") != null ? brbEntity.GetAttributeValue<Money>("ap360_resourcehourlyrate").Value : 0;
                if (ResourceHourlyRate == 0)
                {
                    tracingService.Trace("Resource hourly rate is null on booking");
                    ResourceHourlyRate = Methods.GetResourcePriceBasedOnBRC(service, tracingService, Resource.Id, ServiceRole.Name);
                }
                tracingService.Trace("After GetResourcePriceBasedOnBRC ");


                ServiceRoleHourlyRate = brbEntity.GetAttributeValue<Money>("ap360_servicerolehourlyrate") != null ? brbEntity.GetAttributeValue<Money>("ap360_servicerolehourlyrate").Value : 0;
                if (ServiceRoleHourlyRate == 0)
                {
                    tracingService.Trace("Resource hourly rate is null on booking");
                    ServiceRoleHourlyRate = Methods.getBookAbleResourceCategoryStandardPrice(service, ServiceRole.Name);
                }

            }

            //  ServiceRolePerMinuteRate = ServiceRoleHourlyRate / 60;
            // ResourceMinuteRate = ResourceHourlyRate / 60;
        }
        public static int? RetrieveCurrentUsersTimeZoneSettings(IOrganizationService service)
        {

            var currentUserSettings = service.RetrieveMultiple(

            new QueryExpression("usersettings")

            {

                ColumnSet = new ColumnSet("localeid", "timezonecode"),

                Criteria = new FilterExpression

                {

                    Conditions =

               {

            new ConditionExpression("systemuserid", ConditionOperator.EqualUserId)

               }

                }

            }).Entities[0].ToEntity<Entity>();

            return (int?)currentUserSettings.Attributes["timezonecode"];

        }

        public static DateTime? RetrieveLocalTimeFromUTCTime(IOrganizationService _serviceProxy, DateTime utcTime, int? _timeZoneCode)

        {

            if (!_timeZoneCode.HasValue)

                return null;



            var request = new LocalTimeFromUtcTimeRequest

            {

                TimeZoneCode = _timeZoneCode.Value,

                UtcTime = utcTime.ToUniversalTime()

            };



            var response = (LocalTimeFromUtcTimeResponse)_serviceProxy.Execute(request);

            return response.LocalTime;

        }
        public static DataCollection<Entity> GetChildRecordsForProjectsTaskCount(IOrganizationService organizationService, Guid parentEntiytId, string ParentEntityLookupFieldName, string childEntityName)
        {
            var fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>"
               + "<entity name='" + childEntityName + "'>"
                + "<attribute name='" + childEntityName + "id' />"
               + "<filter type ='and' >"
               + "<condition attribute = '" + ParentEntityLookupFieldName + "' operator= 'eq'  value = '" + parentEntiytId + "' />"
               + "</filter >"
               + "</entity >"
                + "</fetch >";

            var fetchExpression = new FetchExpression(fetchXml);
            EntityCollection fetchResult = organizationService.RetrieveMultiple(fetchExpression);
            return fetchResult.Entities;
        }
        public static int GetChildEntityCount(IOrganizationService organizationService, Guid parentEntiytId, string ParentEntityLookupFieldName, string childEntityName)
        {
            var fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>"
               + "<entity name='" + childEntityName + "'>"
                + "<attribute name='" + childEntityName + "id' />"
               + "<filter type ='and' >"
               + "<condition attribute = '" + ParentEntityLookupFieldName + "' operator= 'eq'  value = '" + parentEntiytId + "' />"
               + "</filter >"
               + "</entity >"
                + "</fetch >";

            var fetchExpression = new FetchExpression(fetchXml);
            EntityCollection fetchResult = organizationService.RetrieveMultiple(fetchExpression);
            int siblingsCount = 0;
            siblingsCount = fetchResult.Entities.Count;
            return siblingsCount;
        }
        public static List<int> getWBSIds(IOrganizationService service, string childEntityName, string ParentEntityLookupFieldName, Guid parentEntiytId)
        {
            List<int> lstWBSIds = new List<int>();

            var fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>"
              + "<entity name='" + childEntityName + "'>"
               + "<attribute name='" + childEntityName + "id' />"
              + "<attribute name='ap360_wbsid' />"
              + "<order attribute = 'ap360_wbsid' descending = 'false' />"

                 + "<filter type ='and' >"
              + "<condition attribute = '" + ParentEntityLookupFieldName + "' operator= 'eq'  value = '" + parentEntiytId + "' />"
              + "</filter >"
              + "</entity >"
               + "</fetch >";



            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            int count = 0;
            foreach (Entity entity in col.Entities)
            {
                count++;


                int wbsid = entity.GetAttributeValue<int>("ap360_wbsid");
                lstWBSIds.Add(wbsid);

            }
            return lstWBSIds;

        }


    }
}
