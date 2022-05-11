using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Crm.Sdk.Messages;

namespace BlackWolf_WhiteBoard_Code
{
    public class createWOPForProductUsingBarcode : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                //  throw new InvalidPluginExecutionException("WorkOrderServcieTaskCalculation");

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
                        entity = (Entity)context.InputParameters["Target"];

                        if (entity.LogicalName == "msdyn_workorder")
                        { if (context.Depth > 1) return;
                            if (entity.Contains("ap360_barcode"))
                            {
                                tracingService.Trace("update of workOrder");

                                Entity postImage = (Entity)context.PostEntityImages["Image"];

                                string barcodeOnWo = postImage.GetAttributeValue<string>("ap360_barcode");
                                double productQtyOnWo = postImage.GetAttributeValue<double>("ap360_productquantity");
                                EntityReference opportunityRef = postImage.GetAttributeValue<EntityReference>("msdyn_opportunityid") != null ? postImage.GetAttributeValue<EntityReference>("msdyn_opportunityid") : null;
                                //throw new InvalidPluginExecutionException("product quantity on Wo " + productQtyOnWo.ToString());                                    
                                if (barcodeOnWo == "" || productQtyOnWo == 0) return;
                                Entity product = Product.getProductBasedOnBarcode(service, tracingService, barcodeOnWo);
                                Money productcost = product.GetAttributeValue<Money>("currentcost") != null ? product.GetAttributeValue<Money>("currentcost") : null;
                                string productPartNumber = product.GetAttributeValue<string>("ap360_partnumber");
                                tracingService.Trace("product par number is " + productPartNumber.ToString());

                                decimal ap360_multiplier = PriceMarkup.GetPriceMarkUpMultiplier(service, tracingService, productcost.Value);
                                //throw new InvalidPluginExecutionException("multiplier is " + ap360_multiplier.ToString());
                                if (product != null)
                                {
                                    Entity workOrderProduct = new Entity("msdyn_workorderproduct");
                                    workOrderProduct["ap360_product"] = new EntityReference(product.LogicalName, product.Id);
                                    workOrderProduct["ap360_multiplier"] = new Money(ap360_multiplier);
                                    if (productcost != null)
                                        workOrderProduct["ap360_revisedestimateamount"] = new Money(Convert.ToDecimal(productQtyOnWo) * ap360_multiplier * productcost.Value);

                                    if (entity != null)
                                        workOrderProduct["msdyn_workorder"] = new EntityReference(entity.LogicalName, entity.Id);
                                    workOrderProduct["msdyn_estimatequantity"] = productQtyOnWo;
                                    workOrderProduct["ap360_isrevised"] = true;
                                    if (opportunityRef != null)
                                        workOrderProduct["ap360_opportunityid"] = new EntityReference(opportunityRef.LogicalName, opportunityRef.Id);
                                    workOrderProduct["ap360_quantityremaining"] = productQtyOnWo;
                                    workOrderProduct["ap360_partnumber"] = productPartNumber;
                                    workOrderProduct["ap360_reviseditemstatus"] = new OptionSetValue(126300001);// Approved
                                    workOrderProduct["ap360_ggparent"] = new EntityReference("product", new Guid("c20708d8-2ae3-ea11-a813-000d3a33f3c3"));//Vehicle
                                    workOrderProduct["ap360_vendoridentified"] = true;
                                    workOrderProduct["ap360_vendorid"] = new EntityReference("account", new Guid("a89044d7-9b9a-ea11-a811-000d3a33f3c3")); //Blackwolf Parts Account
                                    workOrderProduct["msdyn_estimateunitamount"] = new Money(productcost.Value * ap360_multiplier);

                                    service.Create(workOrderProduct);
                                }

                                Entity updateWo = new Entity(entity.LogicalName, entity.Id);
                                updateWo["ap360_barcode"] = "";
                                updateWo["ap360_productquantity"] = null;
                                service.Update(updateWo);
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
