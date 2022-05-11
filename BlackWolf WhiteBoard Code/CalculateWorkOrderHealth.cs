using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class CalculateWorkOrderHealth : IPlugin
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

                        if (entity.LogicalName == "msdyn_workorder")
                        {
                            if (entity.Contains("ap360_totalrevisedestimatedlaboramount"))
                            {
                                tracingService.Trace("update of workOrder");

                                Entity postImage = (Entity)context.PostEntityImages["Image"];
                                Money ap360_predictedspend = new Money();
                                Money ap360_totaloriginalestimatedlaboramount = new Money();
                                Money ap360_totalrevisedestimatedlaboramount = new Money();
                                Money EstimatedLaborAmount = new Money();
                                if (postImage.Contains("ap360_predictedspend"))
                                {
                                    ap360_predictedspend = postImage.GetAttributeValue<Money>("ap360_predictedspend") != null ? postImage.GetAttributeValue<Money>("ap360_predictedspend") : null;
                                    if (ap360_predictedspend != null)
                                    {
                                        if (ap360_predictedspend.Value == 0)
                                            return;
                                    }
                                    tracingService.Trace("ap360_predictedspend " + ap360_predictedspend.Value.ToString());
                                }
                                //if (postImage.Contains("ap360_totaloriginalestimatedlaboramount"))
                                //{
                                //    ap360_totaloriginalestimatedlaboramount = postImage.GetAttributeValue<Money>("ap360_totaloriginalestimatedlaboramount") != null ? postImage.GetAttributeValue<Money>("ap360_totaloriginalestimatedlaboramount") : null;
                                //    EstimatedLaborAmount.Value += ap360_totaloriginalestimatedlaboramount.Value;
                                //    tracingService.Trace("ap360_totaloriginalestimatedlaboramount " + ap360_totaloriginalestimatedlaboramount.Value.ToString());
                                //}
                                EstimatedLaborAmount = WorkOrder.getWorkOrderEstimatedValue(tracingService, postImage, EstimatedLaborAmount);


                                Entity updateWorkOrder = new Entity(entity.LogicalName, entity.Id);
                                if ((EstimatedLaborAmount != null && EstimatedLaborAmount.Value > 0) && ap360_predictedspend.Value > 0)
                                {
                                    tracingService.Trace("Estimated Labor is  " + EstimatedLaborAmount.Value.ToString());
                                    updateWorkOrder["ap360_preworkorderhealth"] = ap360_predictedspend.Value / EstimatedLaborAmount.Value;
                                }
                                service.Update(updateWorkOrder);
                                //throw new InvalidPluginExecutionException("Error ");
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