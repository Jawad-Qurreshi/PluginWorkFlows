using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class MapRowAndBinToWorkOrderProduct : IPlugin
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


                //  tracingService.Trace("MoveQuoteServiceItemstoDifferentQuoteService");
                if (context.MessageName.ToLower() == "update")
                {

                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        entity = (Entity)context.InputParameters["Target"];

                        if (entity.LogicalName == "msdyn_productinventory")
                        {
                            Entity postImage = (Entity)context.PostEntityImages["Image"];

                            if ((entity.Contains("msdyn_row") || entity.Contains("msdyn_bin")))
                            {
                                EntityReference product = null;
                                string row = null;
                                string bin = null;
                                if (postImage.Contains("msdyn_product"))
                                {
                                    product = postImage.GetAttributeValue<EntityReference>("msdyn_product");
                                }




                                if (product != null)
                                {
                                    EntityCollection col = WorkOrderProduct.getWorkOrderProductsRelatedToProduct(service, tracingService, product.Id);

                                    foreach (Entity reterviedWOProductEntity in col.Entities)
                                    {
                                        Entity updateWOProductEntity = new Entity("msdyn_workorderproduct", reterviedWOProductEntity.Id);

                                        string retWOProductRow = null;
                                        retWOProductRow = reterviedWOProductEntity.GetAttributeValue<string>("ap360_row");
                                        string retWOProductBin = null;
                                        retWOProductBin = reterviedWOProductEntity.GetAttributeValue<string>("ap360_bin");

                                        if (retWOProductRow == null)
                                        {
                                            if ((entity.Contains("msdyn_row")))
                                            {
                                                row = entity.GetAttributeValue<string>("msdyn_row");
                                                updateWOProductEntity["ap360_row"] = row;
                                            }
                                        }
                                        if (retWOProductBin == null)
                                        {
                                            if ((entity.Contains("msdyn_bin")))
                                            {
                                                bin = entity.GetAttributeValue<string>("msdyn_bin");
                                                updateWOProductEntity["ap360_bin"] = bin;
                                            }
                                        }
                                        service.Update(updateWOProductEntity);
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