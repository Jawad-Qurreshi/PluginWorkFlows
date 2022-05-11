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
    public class MoveQuoteServiceItemstoDifferentQuoteService : IPlugin
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

                int quantity = 0;

                string quotesubletpricetoReterive = "ap360_saleprice";
                string quoteServiceQSFieldtoUpdate = "ap360_totalsubletamount";

                string quoteproductpricetoReterive = "ap360_unitprice";
                string quoteServiceQPFieldtoUpdate = "ap360_partssaleprice";

                string quoteSrvTaskmintoReterive = "ap360_estimatedtime";
                string quoteServiceQSrvTaskFieldtoUpdate = "ap360_estimatedlaborprice";

                //  tracingService.Trace("MoveQuoteServiceItemstoDifferentQuoteService");
                if (context.MessageName.ToLower() == "update")
                {

                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        entity = (Entity)context.InputParameters["Target"];

                        if (entity.LogicalName == "ap360_quoteproduct" || entity.LogicalName == "ap360_quotesublet" || entity.LogicalName == "ap360_quoteservicetask")
                        {
                            tracingService.Trace(entity.LogicalName);


                            if ((entity.Contains("ap360_quoteserviceid")))
                            {
                                tracingService.Trace("ap360_quoteserviceid IS UPDATED");

                                Entity preImage = (Entity)context.PreEntityImages["Image"];

                                if (preImage.Contains("ap360_quoteserviceid"))
                                {

                                    EntityReference quoteserviceRefPreImage = preImage.GetAttributeValue<EntityReference>("ap360_quoteserviceid") != null ? preImage.GetAttributeValue<EntityReference>("ap360_quoteserviceid") : null;
                                    EntityReference quoteserviceRef = entity.GetAttributeValue<EntityReference>("ap360_quoteserviceid") != null ? entity.GetAttributeValue<EntityReference>("ap360_quoteserviceid") : null;

                                    if (quoteserviceRefPreImage != null)
                                    {
                                        //update process of unselected Quote Service prices
                                        if (entity.LogicalName == "ap360_quoteproduct")
                                            updateQuoteService(service, tracingService, preImage, quoteproductpricetoReterive, quoteServiceQPFieldtoUpdate);
                                        else if (entity.LogicalName == "ap360_quotesublet")
                                            updateQuoteService(service, tracingService, preImage, quotesubletpricetoReterive, quoteServiceQSFieldtoUpdate);
                                        else if (entity.LogicalName == "ap360_quoteservicetask")
                                            updateQuoteService(service, tracingService, preImage, quoteSrvTaskmintoReterive, quoteServiceQSrvTaskFieldtoUpdate);



                                        tracingService.Trace("***************");
                                        //update process of selected Quote Service prices
                                        if (entity.LogicalName == "ap360_quoteproduct")
                                            updateQuoteService(service, tracingService, entity, quoteproductpricetoReterive, quoteServiceQPFieldtoUpdate);
                                        else if (entity.LogicalName == "ap360_quotesublet")
                                            updateQuoteService(service, tracingService, entity, quotesubletpricetoReterive, quoteServiceQSFieldtoUpdate);
                                        else if (entity.LogicalName == "ap360_quoteservicetask")
                                            updateQuoteService(service, tracingService, entity, quoteSrvTaskmintoReterive, quoteServiceQSrvTaskFieldtoUpdate);

                                        if (entity.LogicalName == "ap360_quoteservicetask")
                                        {

                                            //////////////////////////////////////////////////////// update of Project task Section/////////////////
                                            ProjectTask quoteServiceTaskProjectTask = new ProjectTask();
                                            quoteServiceTaskProjectTask = ProjectTask.GetProjectTaskforQuoteEntities(service, tracingService, "msdyn_projecttask", "ap360_quoteservicetaskid", entity.Id, "ap360_quoteservicetask");
                                            if (quoteServiceTaskProjectTask != null)
                                            {
                                                tracingService.Trace(" ***" + quoteServiceTaskProjectTask.TaskSubject);
                                                tracingService.Trace("quote Service Task Project task is not null");
                                                ProjectTask quoteServiceProjectTask = new ProjectTask();
                                                quoteServiceProjectTask = ProjectTask.GetProjectTaskforQuoteEntities(service, tracingService, "msdyn_projecttask", "ap360_quoteserviceid", quoteserviceRef.Id, "ap360_quoteservice");

                                                if (quoteServiceProjectTask != null)
                                                {
                                                    tracingService.Trace("* " + quoteServiceProjectTask.TaskSubject);
                                                    tracingService.Trace(" quote Service Project task is not null");
                                                    //throw new InvalidPluginExecutionException("Custom Error");
                                                    ProjectTask.updateParentTask(service, tracingService, quoteServiceTaskProjectTask, quoteServiceProjectTask, entity);
                                                    //After this function call ManageWBSIDForQuoteEntities( // Line 115 When Quote Items SWAP between Quotes) through plugins steps

                                                }

                                                else
                                                {

                                                    throw new InvalidPluginExecutionException("Quote Service Project Task is Null ");
                                                }
                                            }
                                        }

                                    }


                                }
                                //   throw new InvalidPluginExecutionException("Custom Error");
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

        public static DataCollection<Entity> GetChildRecords(IOrganizationService organizationService, ITracingService tracing, Guid parentEntiytId, string ParentEntityLookupFieldName, string childEntityName, string childEntityField)
        {
            tracing.Trace("Inside GetChildRecords " + childEntityName);
            var fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>"
            + "<entity name='" + childEntityName + "'>"
                  + "<attribute name='" + childEntityName + "id' />"

                + "<attribute name='" + childEntityField + "' />";

            //+"< attribute name = 'ap360_unitprice' />"
            if (childEntityName == "ap360_quoteservicetask")
            {
                fetchXml += "<attribute name='ap360_hourlyrate' />";
            }
            fetchXml += "<filter type ='and' >"
               + "<condition attribute = '" + ParentEntityLookupFieldName + "' operator= 'eq'  value = '" + parentEntiytId + "' />"
               + "</filter >"
               + "</entity >"
                + "</fetch >";

            tracing.Trace(fetchXml);

            //+"<condition attribute = 'statecode' operator= 'eq' value = '0' />" // Active

            // tracingService.Trace("XML:" + fetchXml);

            var fetchExpression = new FetchExpression(fetchXml);
            EntityCollection fetchResult = organizationService.RetrieveMultiple(fetchExpression);
            tracing.Trace("Child Entity Count " + fetchResult.Entities.Count.ToString());
            return fetchResult.Entities;
        }

        public static void updateQuoteService(IOrganizationService service, ITracingService tracingService, Entity entity, string ChildEntityFieldToReterive, string QSFieldToUpdate)
        {

            decimal totalCost = 0;
            int totalminutes = 0;
            EntityReference quoteserviceRef2 = entity.GetAttributeValue<EntityReference>("ap360_quoteserviceid") != null ? entity.GetAttributeValue<EntityReference>("ap360_quoteserviceid") : null;

            if (quoteserviceRef2 != null)
            {
                DataCollection<Entity> lstChildEntities = GetChildRecords(service, tracingService, quoteserviceRef2.Id, "ap360_quoteserviceid", entity.LogicalName, ChildEntityFieldToReterive);

                tracingService.Trace("Count  of " + entity.LogicalName + " " + lstChildEntities.Count.ToString());

                foreach (Entity chlildEntity in lstChildEntities)
                {
                    tracingService.Trace("name of QS is " + quoteserviceRef2.Name);
                    if (entity.LogicalName == "ap360_quoteservicetask")
                    {
                        decimal hourlyRate = chlildEntity.GetAttributeValue<Money>("ap360_hourlyrate") != null ? chlildEntity.GetAttributeValue<Money>("ap360_hourlyrate").Value : 0;

                        if (hourlyRate == 0) return;
                        int taskMinutes = chlildEntity.GetAttributeValue<int>(ChildEntityFieldToReterive);
                        tracingService.Trace("Current Loop minutes " + taskMinutes.ToString());
                        totalminutes += taskMinutes;

                        totalCost = (hourlyRate * totalminutes) / 60;
                    }
                    else
                    {
                        decimal unitPrice = chlildEntity.GetAttributeValue<Money>(ChildEntityFieldToReterive) != null ? chlildEntity.GetAttributeValue<Money>(ChildEntityFieldToReterive).Value : 0;

                        if (unitPrice != 0)
                        {
                            tracingService.Trace("Current Loop Unit Price " + unitPrice.ToString());
                            totalCost += unitPrice;
                        }
                    }

                }


                Entity updateQuoteService = new Entity(quoteserviceRef2.LogicalName, quoteserviceRef2.Id);
                tracingService.Trace("Total Cost " + totalCost.ToString());
                if (entity.LogicalName == "ap360_quoteservicetask")
                {
                    tracingService.Trace(totalminutes.ToString());
                    updateQuoteService["ap360_estimatedtime"] = totalminutes;

                }
                else
                {
                    updateQuoteService[QSFieldToUpdate] = new Money(totalCost);

                }
                service.Update(updateQuoteService);
                tracingService.Trace("Quote Service is updated");

            }
        }
    }
}