using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk.Query;


namespace BlackWolf_WhiteBoard_Code
{
    public class Updatepaymentsonopportunities : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();
            //   throw new InvalidPluginExecutionException("CreateProductFromDescriptionAndRelatetoProductFamily");

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

                if (context.MessageName.ToLower() == "update")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        if (context.Depth == 1)
                        {
                            entity = (Entity)context.InputParameters["Target"];

                            if (entity.LogicalName.ToLower() == "msdyn_payment")
                            {
                                if (entity.Contains("ap360_opportunityid"))
                                {
                                    Entity preImage = (Entity)context.PreEntityImages["Image"];
                                    EntityReference preOpportunity = preImage.GetAttributeValue<EntityReference>("ap360_opportunityid") != null ? preImage.GetAttributeValue<EntityReference>("ap360_opportunityid") : null;

                                    EntityReference opportunity = entity.GetAttributeValue<EntityReference>("ap360_opportunityid") != null ? entity.GetAttributeValue<EntityReference>("ap360_opportunityid") : null;


                                    if (preImage.Contains("ap360_opportunityid"))
                                    {

                                        //Money preimageamount = preImage.GetAttributeValue<Money>("msdyn_amount") != null ? preImage.GetAttributeValue<Money>("msdyn_amount") : null;
                                        if (preOpportunity != null)
                                        {
                                            Entity updateOpportunity = new Entity(preOpportunity.LogicalName, preOpportunity.Id);
                                            decimal totalamount = Payment.getPaymentAmountRelatedtoOpportunity(preOpportunity, service);
                                            updateOpportunity["ap360_totalopportunitypayment"] = new Money(totalamount);
                                            service.Update(updateOpportunity);
                                        }
                                        if (opportunity != null)
                                        {

                                            //Money preimageamount = preImage.GetAttributeValue<Money>("msdyn_amount") != null ? preImage.GetAttributeValue<Money>("msdyn_amount") : null;
                                            Entity updateOpportunity = new Entity(opportunity.LogicalName, opportunity.Id);
                                            decimal totalamount = Payment.getPaymentAmountRelatedtoOpportunity(opportunity, service);
                                            updateOpportunity["ap360_totalopportunitypayment"] = new Money(totalamount);
                                            service.Update(updateOpportunity);
                                        }
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


