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
    public class AdjustRTVProduct : IPlugin
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


                Entity entity = (Entity)context.InputParameters["Target"];

                if (context.MessageName.ToLower() == "update")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        if (entity.LogicalName == "msdyn_rtvproduct")
                        {
                            Entity postImage = (Entity)context.PostEntityImages["Image"];
                           
                            tracingService.Trace("Inside RTV PRODUCT update");
                            if (entity.Contains("ap360_returnfee") || entity.Contains("msdyn_unitcreditamount") || entity.Contains("msdyn_quantity"))
                            {
                                tracingService.Trace("Yes this is ap360_returnfee");
                                if (postImage.Contains("ap360_returnfee") && postImage.Contains("msdyn_unitcreditamount") && postImage.Contains("msdyn_quantity"))
                                {
                                    tracingService.Trace("Yes this is postImage.Contains(ap360_returnfee) || postImage.Contains(msdyn_totalcreditamount)");
                                    
                                    Money unitCreditAmount = postImage.GetAttributeValue<Money>("msdyn_unitcreditamount") ?? null;
                                    Money RTVReturnFee = postImage.GetAttributeValue<Money>("ap360_returnfee") ?? null;
                                    double quantity = postImage.GetAttributeValue<double>("msdyn_quantity");
                                  
                                    tracingService.Trace("All three postimages retrieves befoe cjeck");
                                    if (unitCreditAmount != null && RTVReturnFee != null)
                                    {

                                        tracingService.Trace("All three postimages retrieves after cjeck");
                               
                                        Entity updateRTVProduct = new Entity("msdyn_rtvproduct", entity.Id);
                                        updateRTVProduct["msdyn_totalcreditamount"] = new Money((unitCreditAmount.Value * Convert.ToDecimal(quantity)) - RTVReturnFee.Value);
                                        tracingService.Trace("service.Update(updateRTVProduct);");
                                        service.Update(updateRTVProduct);
                                        tracingService.Trace("service.Updated");
                                        //throw new InvalidPluginExecutionException("test62");
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