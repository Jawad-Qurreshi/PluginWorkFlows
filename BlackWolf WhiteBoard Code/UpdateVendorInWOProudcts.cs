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
    public class UpdateVendorInWOProudcts : IPlugin
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

                if (context.MessageName.ToLower() == "update")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        if (context.Depth == 1)
                        {
                            entity = (Entity)context.InputParameters["Target"];

                            if (entity.LogicalName.ToLower() == "msdyn_purchaseorder")
                            {
                                if (entity.Contains("msdyn_vendor"))
                                {
                                    Entity postImage = (Entity)context.PostEntityImages["Image"];

                                    EntityReference updatedPurchaseOrderVendor = postImage.GetAttributeValue<EntityReference>("msdyn_vendor") != null ? postImage.GetAttributeValue<EntityReference>("msdyn_vendor") : null;

                                    EntityCollection col = PurchaseOrderProduct.RetrievePOPOnBaseOfPO(service, tracingService, entity.Id.ToString());

                                    foreach (Entity pOProduct in col.Entities)
                                    {
                                        Entity updatepoProductVendor = new Entity(pOProduct.LogicalName, pOProduct.Id);

                                        updatepoProductVendor["ap360_vendorid"] = updatedPurchaseOrderVendor;

                                        service.Update(updatepoProductVendor);

                                        EntityReference wOProductRef = pOProduct.GetAttributeValue<EntityReference>("ap360_workorderproductid") ?? null;
                                        if (wOProductRef != null)
                                        {
                                            Entity updateWOProduct = new Entity(wOProductRef.LogicalName, wOProductRef.Id);

                                            updateWOProduct["ap360_vendorid"] = updatedPurchaseOrderVendor;

                                            service.Update(updateWOProduct);
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