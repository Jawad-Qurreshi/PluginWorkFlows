using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class QuoteServiceCalculation : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();
           // throw new InvalidPluginExecutionException("QuoteServiceCalculation");

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

                decimal roleprice = 0;
                tracingService.Trace("Quote Service Calculation");

                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    entity = (Entity)context.InputParameters["Target"];
                    if (entity.LogicalName == "ap360_quoteservice")
                    {
                        tracingService.Trace("Entity is " + entity.LogicalName);

                        if (context.MessageName.ToLower() == "update")
                        {
                            tracingService.Trace("Update ");


                            if (context.Stage == 20)
                            {
                                tracingService.Trace("Pre Step");
                                if (entity.Contains("ap360_serviceroleid") && context.Depth == 1)
                                {
                                    tracingService.Trace("Inside Update of Quote Service: Service Role");
                                    EntityReference serviceRoleRef = entity.GetAttributeValue<EntityReference>("ap360_serviceroleid");
                                    tracingService.Trace(serviceRoleRef.LogicalName + " reference is not null");
                                    Entity ServiceRole = service.Retrieve(serviceRoleRef.LogicalName, serviceRoleRef.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("ap360_price"));
                                    tracingService.Trace(ServiceRole.LogicalName + " is not null");
                                    roleprice = ServiceRole.GetAttributeValue<Money>("ap360_price").Value;
                                    tracingService.Trace(roleprice.ToString());


                                    entity["ap360_hourlyrate"] = new Money(roleprice);


                                }
                            }
                            if (context.Stage == 40)
                            {
                                tracingService.Trace("Post Step");
                                tracingService.Trace("Context.Depth "+context.Depth.ToString());

                                if ((entity.Contains("ap360_estimatedtime")) && context.Depth <= 3)
                                {
                                    int estimatedTime = 0;
                                    decimal hourlyRate = 0;
                                    
                                    tracingService.Trace("Inside Update of Quote Service:Estimated Time");
                                    Entity quoteService = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("ap360_hourlyrate", "ap360_estimatedtime"));
                                    tracingService.Trace(quoteService.LogicalName + " is not null");

                                    estimatedTime = quoteService.GetAttributeValue<int>("ap360_estimatedtime");
                                    hourlyRate = quoteService.GetAttributeValue<Money>("ap360_hourlyrate").Value;


                                    tracingService.Trace(estimatedTime.ToString() + hourlyRate.ToString());

                                    entity["ap360_estimatedlaborprice"] = new Money((hourlyRate * estimatedTime) / 60);
                                    service.Update(entity);


                                }
                            }



                        }



                

                        if (context.MessageName.ToLower() == "create")
                        {
                            tracingService.Trace("Create ");

                            EntityReference serviceRoleRef = entity.GetAttributeValue<EntityReference>("ap360_serviceroleid");
                            int estimatedtime = entity.GetAttributeValue<int>("ap360_estimatedtime");

                            tracingService.Trace(serviceRoleRef.LogicalName + " reference is not null");
                            Entity ServiceRole = service.Retrieve(serviceRoleRef.LogicalName, serviceRoleRef.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("ap360_price"));
                            tracingService.Trace(ServiceRole.LogicalName + " is not null");
                            roleprice = ServiceRole.GetAttributeValue<Money>("ap360_price").Value;
                            tracingService.Trace(roleprice.ToString());
                            entity["ap360_hourlyrate"] = new Money(roleprice);
                            entity["ap360_estimatedlaborprice"] = new Money((roleprice * estimatedtime)/60);


                         //   throw new exception
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