using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
   public class TempCalculatePredictedSpendOnchangeOFPercentComplete : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();

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

                        if (entity.LogicalName == "msdyn_workorderservicetask")
                        {

                            if (entity.Contains("msdyn_percentcomplete"))
                            {
                                if (context.Depth != 1) return;

                                tracingService.Trace("actual amount of wost updated");
                                Entity postImage = (Entity)context.PostEntityImages["Image"];
                                Entity preImage = (Entity)context.PreEntityImages["Image"];

                                Money ap360_predictedspend = new Money();
                                Money ap360_originalestimatedamount = new Money();
                                Money ap360_revisedestimatedamount = new Money();
                                Money EstimatedLaborAmount = new Money();
                                string WOSTName = null;
                                if (postImage.Contains("msdyn_name"))
                                {
                                    WOSTName = postImage.GetAttributeValue<string>("msdyn_name");
                                    tracingService.Trace(WOSTName);
                                }

                            //    EntityReference recentlyWorkedBooking = postImage.GetAttributeValue<EntityReference>("ap360_recentlyworkedbookingid") != null ? postImage.GetAttributeValue<EntityReference>("ap360_recentlyworkedbookingid") : null;
                                EntityReference recentlyWorkedResource = postImage.GetAttributeValue<EntityReference>("ap360_recentlyworkedresourceid") != null ? postImage.GetAttributeValue<EntityReference>("ap360_recentlyworkedresourceid") : null;
                                EntityReference workOrderRef = postImage.GetAttributeValue<EntityReference>("msdyn_workorder");
                                int WOSTpercentcomplete = Convert.ToInt32(postImage["msdyn_percentcomplete"]);
                                Money actualAmount = postImage.GetAttributeValue<Money>("ap360_actualamount") != null ? postImage.GetAttributeValue<Money>("ap360_actualamount") : null;
                                tracingService.Trace("getting  values");
                                Money preActualAmount = preImage.GetAttributeValue<Money>("ap360_actualamount") != null ? preImage.GetAttributeValue<Money>("ap360_actualamount") : null;
                                tracingService.Trace("After getting all values");


                                //if (preActualAmount != null)
                                //{
                                //    if (actualAmount.Value == preActualAmount.Value)
                                //    {
                                //        return;
                                //    }
                                //}

                                decimal postwosthealth = 0;
                                if (postImage.Contains("ap360_postwosthealth"))
                                {
                                    postwosthealth = postImage.GetAttributeValue<decimal>("ap360_postwosthealth");
                                    tracingService.Trace("Post Wost Helath " + postwosthealth.ToString());

                                }

                              //  throw new InvalidPluginExecutionException("before Error "+ actualAmount.Value.ToString());
                                tracingService.Trace("wost percent complete is " + WOSTpercentcomplete.ToString());
                                if (actualAmount != null)
                                {
                                    tracingService.Trace("Actual Amount " + actualAmount.Value.ToString());
                                    EstimatedLaborAmount = WorkOrderServiceTask.getWOSTEstimatedDollarValue(tracingService, postImage, ap360_originalestimatedamount, ap360_revisedestimatedamount, EstimatedLaborAmount);
                                    tracingService.Trace("Estimated Labor Amount " + EstimatedLaborAmount.Value.ToString());
                                    ap360_predictedspend = WorkOrderServiceTask.UpdateWorkOrderServiceTaskHealthByAmount(tracingService, service, entity.Id, ap360_predictedspend, EstimatedLaborAmount, WOSTpercentcomplete, actualAmount, postwosthealth);

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