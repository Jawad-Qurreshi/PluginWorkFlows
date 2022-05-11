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
    public class MoveQuoteServicetoDifferentQuote : IPlugin
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



                //tracingService.Trace("MoveQuoteServicetoDifferentQuote");
                if (context.MessageName.ToLower() == "update")
                {

                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        entity = (Entity)context.InputParameters["Target"];

                        if (entity.LogicalName == "ap360_quoteservice")
                        {
                            tracingService.Trace(entity.LogicalName);


                            if ((entity.Contains("ap360_quoteid")))
                            {

                                tracingService.Trace("ap360_quoteid IS UPDATED");

                                Entity preImage = (Entity)context.PreEntityImages["Image"];

                                if (preImage.Contains("ap360_quoteid"))
                                {
                                    EntityReference quoteRefPreImage = preImage.GetAttributeValue<EntityReference>("ap360_quoteid") != null ? preImage.GetAttributeValue<EntityReference>("ap360_quoteid") : null;
                                    EntityReference quoteRef = entity.GetAttributeValue<EntityReference>("ap360_quoteid") != null ? entity.GetAttributeValue<EntityReference>("ap360_quoteid") : null;

                                    if (quoteRefPreImage != null)
                                    {
                                        //update process of unselected Quote  prices
                                        updateQuote(service, tracingService, preImage);


                                        tracingService.Trace("***************");
                                        //update process of selected Quote  prices
                                        updateQuote(service, tracingService, entity);


                                        //////////////////////////////////////////////////////// update of Project task Section/////////////////
                                        ProjectTask quoteServiceProjectTask = new ProjectTask();
                                        quoteServiceProjectTask = ProjectTask.GetProjectTaskforQuoteEntities(service, tracingService, "msdyn_projecttask", "ap360_quoteserviceid", entity.Id, "ap360_quoteservice");

                                        if (quoteServiceProjectTask != null)
                                        {
                                            tracingService.Trace("quote Service Project task is not null");
                                            ProjectTask quoteProjectTask = new ProjectTask();
                                            quoteProjectTask = ProjectTask.GetProjectTaskforQuoteEntities(service, tracingService, "msdyn_projecttask", "ap360_quoteid", quoteRef.Id, "ap360_quote");

                                            if (quoteProjectTask != null)
                                            {
                                                tracingService.Trace("quote Project task is not null");
                                                ProjectTask.updateParentTask(service, tracingService, quoteServiceProjectTask, quoteProjectTask, entity);
                                                //After this function call ManageWBSIDForQuoteEntities( // Line 115 When Quote Items SWAP between Quotes) through plugins steps
                                            }

                                            else
                                            {

                                               // throw new InvalidPluginExecutionException("Quote Project Task is Null ");
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

        public static DataCollection<Entity> GetChildRecords(IOrganizationService organizationService, ITracingService tracing, Guid quoteId)
        {
            var fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='ap360_quoteservice'>
                                <attribute name='ap360_quoteserviceid' />
                                <attribute name='ap360_workrequested' />
                                <attribute name='createdon' />
                                <attribute name='ap360_totalsubletamount' />
                                <attribute name='ap360_quoteservicetotalamount' />
                                <attribute name='ap360_partssaleprice' />
                                <attribute name='ap360_estimatedtime' />
                                <attribute name='ap360_estimatedlaborprice' />
                                <order attribute='ap360_workrequested' descending='false' />
                                <filter type='and'>
                                  <condition attribute='ap360_quoteid' operator='eq' value='" + quoteId + @"' /> 
                                </filter>
                              </entity>
                            </fetch>";

            tracing.Trace(fetchXml);

            //+"<condition attribute = 'statecode' operator= 'eq' value = '0' />" // Active

            // tracingService.Trace("XML:" + fetchXml);

            var fetchExpression = new FetchExpression(fetchXml);
            EntityCollection fetchResult = organizationService.RetrieveMultiple(fetchExpression);
            tracing.Trace("Child Entity Count " + fetchResult.Entities.Count.ToString());
            return fetchResult.Entities;
        }

        public static void updateQuote(IOrganizationService service, ITracingService tracingService, Entity entity)
        {

            decimal totalCost = 0;
            int totalEstimatedtime = 0;
            decimal totalEstimatedlaborprice = 0;
            decimal totalPartssaleprice = 0;
            decimal totalSubletamount = 0;
            tracingService.Trace("Count ");
            EntityReference quoteRef2 = entity.GetAttributeValue<EntityReference>("ap360_quoteid") != null ? entity.GetAttributeValue<EntityReference>("ap360_quoteid") : null;
            //   throw new InvalidPluginExecutionException("Count ");

            if (quoteRef2 != null)
            {
                DataCollection<Entity> lstChildEntities = GetChildRecords(service, tracingService, quoteRef2.Id);


                foreach (Entity chlildEntity in lstChildEntities)
                {
                    tracingService.Trace("name of Qoute is " + quoteRef2.Name);

                    int estimatedtime = chlildEntity.GetAttributeValue<int>("ap360_estimatedtime");
                    totalEstimatedtime += estimatedtime;
                    decimal estimatedlaborprice = chlildEntity.GetAttributeValue<Money>("ap360_estimatedlaborprice") != null ? chlildEntity.GetAttributeValue<Money>("ap360_estimatedlaborprice").Value : 0;
                    if (estimatedlaborprice != 0)
                    {
                        totalEstimatedlaborprice += estimatedlaborprice;
                    }
                    decimal partssaleprice = chlildEntity.GetAttributeValue<Money>("ap360_partssaleprice") != null ? chlildEntity.GetAttributeValue<Money>("ap360_partssaleprice").Value : 0;
                    if (partssaleprice != 0)
                    {
                        totalPartssaleprice += partssaleprice;
                    }
                    decimal subletamount = chlildEntity.GetAttributeValue<Money>("ap360_totalsubletamount") != null ? chlildEntity.GetAttributeValue<Money>("ap360_totalsubletamount").Value : 0;
                    if (subletamount != 0)
                    {
                        totalSubletamount += subletamount;
                    }



                }

                Entity updateQuote = new Entity(quoteRef2.LogicalName, quoteRef2.Id);

                updateQuote["ap360_totalestimatedtime"] = totalEstimatedtime;
                updateQuote["ap360_totalestimatedlaborprice"] = new Money(totalEstimatedlaborprice);
                updateQuote["ap360_totalpartssaleprice"] = new Money(totalPartssaleprice);
                updateQuote["ap360_totalsubletamount"] = new Money(totalSubletamount);



                service.Update(updateQuote);
                tracingService.Trace("Quote  is updated");

            }
        }
    }
}