using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class UpdateWorkOrderProductAmountOnUpdateofPOProductAmount : IPlugin
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
                    if (entity.LogicalName == "msdyn_purchaseorderproduct")
                    {

                        if (context.MessageName.ToLower() == "update")
                        {

                            if (entity.Contains("msdyn_unitcost") || entity.Contains("msdyn_quantity"))
                            {
                                Entity preImage = (Entity)context.PreEntityImages["Image"];

                                if (preImage.Contains("ap360_workorderproductid"))
                                {
                                    Money poProductunitcost = entity.GetAttributeValue<Money>("msdyn_unitcost") != null ? entity.GetAttributeValue<Money>("msdyn_unitcost") : null;

                                    EntityReference workorderproductRef = preImage.GetAttributeValue<EntityReference>("ap360_workorderproductid") != null ? preImage.GetAttributeValue<EntityReference>("ap360_workorderproductid") : null;
                                    if (workorderproductRef != null)
                                    {
                                        Entity workOrderProductEntity = service.Retrieve(workorderproductRef.LogicalName, workorderproductRef.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("msdyn_estimatequantity", "ap360_isrevised"));



                                        if (workOrderProductEntity != null)
                                        {
                                            Entity udpateWorkOrderProdcut = new Entity(workOrderProductEntity.LogicalName, workOrderProductEntity.Id);
                                            double quantity = workOrderProductEntity.GetAttributeValue<double>("msdyn_estimatequantity");
                                            //throw new InvalidPluginExecutionException("2");
                                            udpateWorkOrderProdcut.Id = workorderproductRef.Id;
                                            decimal ap360_multiplier = PriceMarkup.GetPriceMarkUpMultiplier(service, tracingService, poProductunitcost.Value);
                                            udpateWorkOrderProdcut["msdyn_estimateunitcost"] = entity.GetAttributeValue<Money>("msdyn_unitcost").Value;
                                            decimal msdyn_unitcost = entity.GetAttributeValue<Money>("msdyn_unitcost").Value;
                                            bool isrevised = false;

                                            isrevised = workOrderProductEntity.GetAttributeValue<bool>("ap360_isrevised");



                                            udpateWorkOrderProdcut["ap360_actualamount"] = Convert.ToDecimal(quantity) * ap360_multiplier * msdyn_unitcost;


                                            // throw new InvalidPluginExecutionException("WO product Quantity "+ quantity.ToString()+" PO Cost "+ msdyn_unitcost.ToString()+" Mulitpler "+ ap360_multiplier.ToString());
                                            udpateWorkOrderProdcut["msdyn_estimatesubtotal"] = Convert.ToDecimal(quantity) * ap360_multiplier * msdyn_unitcost;
                                            udpateWorkOrderProdcut["msdyn_estimateunitamount"] = ap360_multiplier * msdyn_unitcost;



                                            udpateWorkOrderProdcut["ap360_multiplier"] = ap360_multiplier;

                                            service.Update(udpateWorkOrderProdcut);

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