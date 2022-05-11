using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class OnUpdateofWorkOrderHealthGenerateLog : IPlugin
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
                        {
                            if ((entity.Contains("ap360_workorderhealth")))
                            {
                                tracingService.Trace("update of workOrder ");

                                // decimal workOrderHealth = entity.GetAttributeValue<decimal>("ap360_workorderhealth");
                                Entity postImage = (Entity)context.PostEntityImages["Image"];
                                decimal preWorkOrderHealth = 0;
                                decimal postWorkOrderHealth = 0;
                                string WorkOrderName = null;

                                Entity preImage = (Entity)context.PreEntityImages["Image"];
                                if (preImage.Contains("ap360_workorderhealth"))
                                {
                                    preWorkOrderHealth = preImage.GetAttributeValue<decimal>("ap360_workorderhealth");
                                    tracingService.Trace("Pre WORK order health " + preWorkOrderHealth.ToString());
                                }
                                if (postImage.Contains("ap360_workorderhealth"))
                                {
                                    postWorkOrderHealth = postImage.GetAttributeValue<decimal>("ap360_workorderhealth");
                                    WorkOrderName = postImage.GetAttributeValue<string>("msdyn_name");
                                    tracingService.Trace("Post WORK order health " + postWorkOrderHealth.ToString());
                                }

                                if (postWorkOrderHealth != 0 && preWorkOrderHealth != 0)
                                {
                                    //tracingService.Trace("Pre and Post Value is not zero");
                                    //Entity workOrderLog = new Entity("ap360_workorderhealthlog");
                                    //workOrderLog["ap360_name"] = WorkOrderName + " Log";
                                    //workOrderLog["ap360_preworkorderhealth"] = preWorkOrderHealth;
                                    //workOrderLog["ap360_postworkorderhealth"] = postWorkOrderHealth;
                                    //workOrderLog["ap360_workorderid"] = new EntityReference(entity.LogicalName, entity.Id);
                                    //service.Create(workOrderLog);
                                }


                                //throw new InvalidPluginExecutionException("Custom Errro ");


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