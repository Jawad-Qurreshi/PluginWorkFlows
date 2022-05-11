using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class CreateSubletForPurchaseOrderShippingFee : IPlugin
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


                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    entity = (Entity)context.InputParameters["Target"];
                    if (entity.LogicalName == "msdyn_purchaseorder")
                    {

                        if (context.MessageName.ToLower() == "update")
                        {

                            if (entity.Contains("ap360_shippingfee"))
                            {
                                Entity preImage = (Entity)context.PreEntityImages["Image"];

                                if (preImage.Contains("msdyn_vendor"))
                                {

                                    EntityReference vendorRef = preImage.GetAttributeValue<EntityReference>("msdyn_vendor") != null ? preImage.GetAttributeValue<EntityReference>("msdyn_vendor") : null;

                                    Money shippingFee = entity.GetAttributeValue<Money>("ap360_shippingfee") != null ? entity.GetAttributeValue<Money>("ap360_shippingfee") : null;

                                    if (shippingFee != null)
                                    {
                                        PurchaseOrder.CreateSubletUnderOpportunity(service, tracingService, entity, shippingFee, vendorRef);
                                    }
                                }
                            }

                            //if (entity.Contains("ap360_tax"))
                            //{
                            //    Entity preImage = (Entity)context.PreEntityImages["Image"];

                            //    if (preImage.Contains("msdyn_vendor"))
                            //    {

                            //        EntityReference vendorRef = preImage.GetAttributeValue<EntityReference>("msdyn_vendor") != null ? preImage.GetAttributeValue<EntityReference>("msdyn_vendor") : null;

                            //        Money tax = entity.GetAttributeValue<Money>("ap360_tax") != null ? entity.GetAttributeValue<Money>("ap360_tax") : null;

                            //        if (tax != null)
                            //        {
                            //            PurchaseOrder.CreateSubletUnderOpportunity(service, tracingService, entity, tax, vendorRef);
                            //        }
                            //    }
                            //}

                            //throw new InvalidPluginExecutionException("Error ");
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