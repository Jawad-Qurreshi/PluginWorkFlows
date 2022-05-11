using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using Microsoft.Xrm.Sdk.Query;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BlackWolf_WhiteBoard_Code
{
    public class ManageCreditCardPayment : IPlugin
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

                                Entity postImage = (Entity)context.PostEntityImages["Image"];

                                if (entity.Contains("msdyn_paymenttype"))
                                //if (entity.Contains("ap360_creditcardfeepaymentoptions"))
                                {
                                    tracingService.Trace("Entity contains paymenttype");
                                    int paymenttype = entity.GetAttributeValue<OptionSetValue>("msdyn_paymenttype").Value;
                                    //int creditCardFeePaymentOption = entity.GetAttributeValue<OptionSetValue>("ap360_creditcardfeepaymentoptions") != null ? entity.GetAttributeValue<OptionSetValue>("ap360_creditcardfeepaymentoptions").Value : 0;
                                    //Money updatedAmount = entity.GetAttributeValue<Money>("msdyn_amount") != null ? entity.GetAttributeValue<Money>("msdyn_amount") : null;
                                    if (paymenttype == 690970002) //Credit Card
                                    {
                                        EntityReference wOSubletIncidentalRef = postImage.GetAttributeValue<EntityReference>("ap360_workordersubletid") != null ? postImage.GetAttributeValue<EntityReference>("ap360_workordersubletid") : null;

                                        if (wOSubletIncidentalRef != null) return;

                                        tracingService.Trace("payment type is credit card on update");
                                        EntityReference opportunityRef = postImage.GetAttributeValue<EntityReference>("ap360_opportunityid") != null ? postImage.GetAttributeValue<EntityReference>("ap360_opportunityid") : null;
                                        if (opportunityRef == null) return;
                                        Money creditCardFee = postImage.GetAttributeValue<Money>("ap360_creditcardfee") != null ? postImage.GetAttributeValue<Money>("ap360_creditcardfee") : null;
                                        if (creditCardFee != null)
                                        {
                                            Guid newlycreatedWOSubIncidentalGuid = WorkOrderSublet.CreateWOSIncidentalForPayments(service, tracingService, creditCardFee, opportunityRef);

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
                                if (entity.Contains("ap360_creditcardfee"))
                                {
                                    tracingService.Trace("credit card charges updated");
                                    Money updatedCreditCardCharges = entity.GetAttributeValue<Money>("ap360_creditcardfee") != null ? entity.GetAttributeValue<Money>("ap360_creditcardfee") : null;

                                    if (updatedCreditCardCharges == null) return;

                                    EntityReference wOSubIncidental = postImage.GetAttributeValue<EntityReference>("ap360_workordersubletid") != null ? postImage.GetAttributeValue<EntityReference>("ap360_workordersubletid") : null;

                                    if (wOSubIncidental == null) return;

                                    Entity updateWOSubletIncidental = new Entity(wOSubIncidental.LogicalName, wOSubIncidental.Id);
                                    updateWOSubletIncidental["ap360_incidentalcosts"] = updatedCreditCardCharges;
                                    service.Update(updateWOSubletIncidental);
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
                                OptionSetValue getPaymentType = entity.GetAttributeValue<OptionSetValue>("msdyn_paymenttype");
                                int paymentType = getPaymentType.Value;
                                //int paymenttype = entity.GetAttributeValue<OptionSetValue>("msdyn_paymenttype").Value;
                                //int creditCardFeePayment = entity.GetAttributeValue<OptionSetValue>("ap360_creditcardfeepaymentoptions").Value;
                                tracingService.Trace("option set value ");
                                if (paymentType == 690970002)  //Credit Card
                                //if (creditCardFeePayment == 126300001) //Apply to opportunity account
                                {
                                    tracingService.Trace("payment type is credit card on create");

                                    EntityReference opportunityRef = entity.GetAttributeValue<EntityReference>("ap360_opportunityid") != null ? entity.GetAttributeValue<EntityReference>("ap360_opportunityid") : null;
                                    if (opportunityRef == null) return;
                                    Money creditCardCharges = entity.GetAttributeValue<Money>("ap360_creditcardfee") != null ? entity.GetAttributeValue<Money>("ap360_creditcardfee") : null;
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
                if (context.MessageName.ToLower() == "delete")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference)
                    {
                        tracingService.Trace("Inside Payment delete ");
                        EntityReference TargetRef = (EntityReference)context.InputParameters["Target"];
                        Entity preImage = (Entity)context.PreEntityImages["Image"];
                        EntityReference WOSubletRef = preImage.GetAttributeValue<EntityReference>("ap360_workordersubletid") != null ? preImage.GetAttributeValue<EntityReference>("ap360_workordersubletid") : null;
                        if (WOSubletRef == null) return;
                        service.Delete(WOSubletRef.LogicalName, WOSubletRef.Id);
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
