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
    public class QtSrvTaskAndWOSrvTask_SyncQtSrvTaskAndWOSrvTaskToParentEntity : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
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
                Entity reterivedSrvTask = null;
                EntityReference workOrder = null;

                if (context.MessageName.ToLower() == "create")
                {
                    tracingService.Trace("Start");
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {

                        entity = (Entity)context.InputParameters["Target"];
                        if (entity.LogicalName == "ap360_quoteservicetask" || entity.LogicalName == "msdyn_workorderservicetask")
                        {


                            if (entity.LogicalName == "ap360_quoteservicetask")
                                reterivedSrvTask = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("ap360_parentservicetaskid", "ap360_serviceroleid", "ap360_hourlyrate", "ap360_childservicetemplateid", "ap360_quoteserviceid"));
                            else if (entity.LogicalName == "msdyn_workorderservicetask")
                            {  
                                if(context.Depth>1)return;
                                reterivedSrvTask = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("ap360_parentservicetaskid", "ap360_serviceroleid", "ap360_hourlyrate", "ap360_childservicetemplateid", "msdyn_workorder"));
                            }

                            QuoteServiceTask quoteServiceTask = null;
                            if (reterivedSrvTask != null)
                            {
                                tracingService.Trace("reterivedSrvTask is not null");

                                quoteServiceTask = new QuoteServiceTask();
                                quoteServiceTask.guid = reterivedSrvTask.Id;
                                quoteServiceTask.ServiceRole = reterivedSrvTask.GetAttributeValue<EntityReference>("ap360_serviceroleid") != null ? reterivedSrvTask.GetAttributeValue<EntityReference>("ap360_serviceroleid") : null;
                                quoteServiceTask.ChildServiceTemplate = reterivedSrvTask.GetAttributeValue<EntityReference>("ap360_childservicetemplateid") != null ? reterivedSrvTask.GetAttributeValue<EntityReference>("ap360_childservicetemplateid") : null;


                                if (entity.LogicalName == "ap360_quoteservicetask")
                                    quoteServiceTask.QuoteService = reterivedSrvTask.GetAttributeValue<EntityReference>("ap360_quoteserviceid") != null ? reterivedSrvTask.GetAttributeValue<EntityReference>("ap360_quoteserviceid") : null;
                                else if (entity.LogicalName == "msdyn_workorderservicetask")
                                    workOrder = reterivedSrvTask.GetAttributeValue<EntityReference>("msdyn_workorder") != null ? reterivedSrvTask.GetAttributeValue<EntityReference>("msdyn_workorder") : null;

                                quoteServiceTask.HourlyRate = reterivedSrvTask.GetAttributeValue<Money>("ap360_hourlyrate") != null ? reterivedSrvTask.GetAttributeValue<Money>("ap360_hourlyrate") : null;
                                quoteServiceTask.ParentServiceTask = reterivedSrvTask.GetAttributeValue<EntityReference>("ap360_parentservicetaskid") != null ? reterivedSrvTask.GetAttributeValue<EntityReference>("ap360_parentservicetaskid") : null;

                                if (quoteServiceTask.QuoteService != null || workOrder != null)
                                {
                                    tracingService.Trace("Entity is " + entity.LogicalName);

                                    var serviceTaskQuery = new QueryExpression(entity.LogicalName);
                                    serviceTaskQuery.ColumnSet.AllColumns = true;
                                    if (entity.LogicalName == "ap360_quoteservicetask")
                                        serviceTaskQuery.Criteria.AddCondition("ap360_quoteserviceid", ConditionOperator.Equal, quoteServiceTask.QuoteService.Id);
                                    else if (entity.LogicalName == "msdyn_workorderservicetask")
                                        serviceTaskQuery.Criteria.AddCondition("msdyn_workorder", ConditionOperator.Equal, workOrder.Id);


                                    EntityCollection col = service.RetrieveMultiple(serviceTaskQuery);
                                    //If First Quote Service task or WorkOrder service task then lock Service Role and hourly rate on Quote Service or WorkOrder
                                    if (col.Entities.Count == 1)
                                    {
                                        tracingService.Trace("if");

                                        EntityReference serviceRoleTobeLockOnQuoteService = col.Entities[0].GetAttributeValue<EntityReference>("ap360_serviceroleid") != null ? col.Entities[0].GetAttributeValue<EntityReference>("ap360_serviceroleid") : null;

                                        if (serviceRoleTobeLockOnQuoteService != null)
                                        {
                                            Entity updateSrvTaskParentEntity = null;
                                            if (entity.LogicalName == "ap360_quoteservicetask" || entity.LogicalName == "msdyn_workorderservicetask")
                                            {
                                                if (entity.LogicalName == "ap360_quoteservicetask")
                                                {
                                                    updateSrvTaskParentEntity = new Entity(quoteServiceTask.QuoteService.LogicalName, quoteServiceTask.QuoteService.Id);
                                                    updateSrvTaskParentEntity["ap360_serviceroleid"] = new EntityReference(serviceRoleTobeLockOnQuoteService.LogicalName, serviceRoleTobeLockOnQuoteService.Id);

                                                }
                                                else if (entity.LogicalName == "msdyn_workorderservicetask")
                                                {
                                                    updateSrvTaskParentEntity = new Entity(workOrder.LogicalName, workOrder.Id);
                                                    updateSrvTaskParentEntity["ap360_servicerole"] = new EntityReference(serviceRoleTobeLockOnQuoteService.LogicalName, serviceRoleTobeLockOnQuoteService.Id);
                                                }
                                                if (quoteServiceTask.HourlyRate != null)
                                                    updateSrvTaskParentEntity["ap360_hourlyrate"] = new Money(quoteServiceTask.HourlyRate.Value);

                                                if (quoteServiceTask.ChildServiceTemplate != null)
                                                    updateSrvTaskParentEntity["ap360_childservicetemplateid"] = new EntityReference(quoteServiceTask.ChildServiceTemplate.LogicalName, quoteServiceTask.ChildServiceTemplate.Id);
                                                if (quoteServiceTask.ParentServiceTask != null)
                                                    updateSrvTaskParentEntity["ap360_parentservicetaskid"] = new EntityReference(quoteServiceTask.ParentServiceTask.LogicalName, quoteServiceTask.ParentServiceTask.Id);

                                                tracingService.Trace("Before getChildServiceTemplateAndMapProductsOnQuoteSerivice");
                                                if (quoteServiceTask.ChildServiceTemplate != null)

                                                    Child_ServiceTemplateType.getChildServiceTemplateAndMapProductsOnQuoteSerivice(service, tracingService, updateSrvTaskParentEntity, quoteServiceTask.ChildServiceTemplate);


                                                tracingService.Trace("Before End");


                                                service.Update(updateSrvTaskParentEntity);
                                            }


                                        }
                                    }
                                    else if (col.Entities.Count > 1)
                                    {

                                        tracingService.Trace("Last Else");

                                        Entity updateSrvTaskParentEntity = null;
                                        if (entity.LogicalName == "ap360_quoteservicetask")
                                        {
                                            updateSrvTaskParentEntity = new Entity(quoteServiceTask.QuoteService.LogicalName, quoteServiceTask.QuoteService.Id);
                                        }
                                        else if (entity.LogicalName == "msdyn_workorderservicetask")
                                        {
                                            updateSrvTaskParentEntity = new Entity(workOrder.LogicalName, workOrder.Id);
                                        }
                                        tracingService.Trace("before fields ");
                                        if (quoteServiceTask.ChildServiceTemplate != null)
                                            updateSrvTaskParentEntity["ap360_childservicetemplateid"] = new EntityReference(quoteServiceTask.ChildServiceTemplate.LogicalName, quoteServiceTask.ChildServiceTemplate.Id);
                                        tracingService.Trace("inside fields");
                                        if (quoteServiceTask.ParentServiceTask != null)
                                            updateSrvTaskParentEntity["ap360_parentservicetaskid"] = new EntityReference(quoteServiceTask.ParentServiceTask.LogicalName, quoteServiceTask.ParentServiceTask.Id);
                                        tracingService.Trace("Before getChildServiceTemplateAndMapProductsOnQuoteSerivice");
                                        if (quoteServiceTask.ChildServiceTemplate != null)

                                            Child_ServiceTemplateType.getChildServiceTemplateAndMapProductsOnQuoteSerivice(service, tracingService, updateSrvTaskParentEntity, quoteServiceTask.ChildServiceTemplate);
                                        tracingService.Trace("Before End");

                                        service.Update(updateSrvTaskParentEntity);
                                    }






                                }




                            }
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