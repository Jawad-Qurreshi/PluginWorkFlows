using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace BlackWolf_WhiteBoard_Code
{
    class CreateIncidentalWOSOnCreditCardPayment : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();
            // throw new InvalidPluginExecutionException("PublishAndSetFamilyofProduct");



            try
            {



                //throw new InvalidPluginExecutionException("throw");
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
                    tracingService.Trace("Inside  payment update");
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        if (context.Depth == 1)
                        {
                            entity = (Entity)context.InputParameters["Target"];



                            if (entity.LogicalName == "msdyn_payment")
                            {
                                tracingService.Trace("Entity Name " + entity.LogicalName);



                                //if (entity.Contains("msdyn_paymenttype"))
                                if (entity.Contains("ap360_creditcardfeepayment"))
                                {
                                    tracingService.Trace("Entity contains creditcardfeepayment");
                                    //int paymenttype = entity.GetAttributeValue<OptionSetValue>("msdyn_paymenttype").Value;
                                    int creditCardFeePayment = entity.GetAttributeValue<OptionSetValue>("ap360_creditcardfeepayment") != null ? entity.GetAttributeValue<OptionSetValue>("ap360_creditcardfeepayment").Value : 0;



                                    Entity postImage = (Entity)context.PostEntityImages["Image"];



                                    //if (paymenttype == 690970002)  //Credit Card
                                    if (creditCardFeePayment == 126300001) //Apply to opportunity account
                                    {
                                        tracingService.Trace("payment type is credit card on update");
                                        EntityReference opportunityRef = postImage.GetAttributeValue<EntityReference>("ap360_opportunityid") != null ? postImage.GetAttributeValue<EntityReference>("ap360_opportunityid") : null;
                                        if (opportunityRef == null) return;
                                        Money creditCardCharges = postImage.GetAttributeValue<Money>("ap360_creditcardcharges") != null ? postImage.GetAttributeValue<Money>("ap360_creditcardcharges") : null;
                                        if (creditCardCharges != null)
                                        {
                                            Guid newlycreatedWOSubIncidentalGuid = WorkOrderSublet.CreateWOSIncidentalForPayments(service, tracingService, creditCardCharges, opportunityRef);



                                            Payment.updateWOSubletOnPayment(service, entity, newlycreatedWOSubIncidentalGuid);
                                        }
                                    }



                                    else
                                    {
                                        tracingService.Trace("payment type is other than credit card");
                                        EntityReference wOSubIncidental = postImage.GetAttributeValue<EntityReference>("ap360_workordersubletid") != null ? postImage.GetAttributeValue<EntityReference>("ap360_workordersubletid") : null;
                                        if (wOSubIncidental == null) return;
                                        service.Delete(wOSubIncidental.LogicalName, wOSubIncidental.Id);
                                    }



                                }



                            }
                        }



                    }
                }



                if (context.MessageName.ToLower() == "create")
                {
                    tracingService.Trace("Inside  Payment Create");
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        if (context.Depth == 1)
                        {
                            entity = (Entity)context.InputParameters["Target"];



                            if (entity.LogicalName == "msdyn_payment")
                            {
                                tracingService.Trace("Entity Name " + entity.LogicalName);



                                //int paymenttype = entity.GetAttributeValue<OptionSetValue>("msdyn_paymenttype").Value;
                                int creditCardFeePayment = entity.GetAttributeValue<OptionSetValue>("ap360_creditcardfeepayment").Value;



                                //if (paymenttype == 690970002)  //Credit Card
                                if (creditCardFeePayment == 126300001) //Apply to opportunity account
                                {
                                    tracingService.Trace("payment type is credit card on create");



                                    EntityReference opportunityRef = entity.GetAttributeValue<EntityReference>("ap360_opportunityid") != null ? entity.GetAttributeValue<EntityReference>("ap360_opportunityid") : null;
                                    if (opportunityRef == null) return;
                                    Money creditCardCharges = entity.GetAttributeValue<Money>("ap360_creditcardcharges") != null ? entity.GetAttributeValue<Money>("ap360_creditcardcharges") : null;
                                    if (creditCardCharges != null)
                                    {
                                        Guid newlycreatedWOSubIncidentalGuid = WorkOrderSublet.CreateWOSIncidentalForPayments(service, tracingService, creditCardCharges, opportunityRef);



                                        Payment.updateWOSubletOnPayment(service, entity, newlycreatedWOSubIncidentalGuid);
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