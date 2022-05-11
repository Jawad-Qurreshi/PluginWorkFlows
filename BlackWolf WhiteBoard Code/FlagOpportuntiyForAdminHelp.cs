using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class FlagOpportuntiyForAdminHelp : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();
            //   throw new InvalidPluginExecutionException("CreateProductFromDescriptionAndRelatetoProductFamily");

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

                        entity = (Entity)context.InputParameters["Target"];

                        if (entity.LogicalName == "msdyn_workorder")
                        {
                            tracingService.Trace("Entity Name " + entity.LogicalName);

                            if (entity.Contains("ap360_workorderbwstatustext"))
                            {
                                tracingService.Trace("WorkOrder Bw status updated");
                                if (entity.GetAttributeValue<string>("ap360_workorderbwstatustext").Contains("126300003"))//Needs Admin FollowUp                                    
                                //This condition is important becasue we can"t have optionsetvaluecollection image so we have to retervie workorder

                                {

                                    //Entity reterviedWorkOrder = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
                                    // OptionSetValueCollection workOrderBWStatusValueCollection = entity.GetAttributeValue<OptionSetValueCollection>("ap360_workorderbwstatus");
                                    string workOrderBWStatus = entity.GetAttributeValue<string>("ap360_workorderbwstatustext");
                                    //foreach (var option in workOrderBWStatusValueCollection)
                                    //{
                                    //    int value = option.Value;// Value of options.
                                    if (workOrderBWStatus.Contains("126300003"))//Needs Admin FollowUp
                                    {
                                        tracingService.Trace("status updated to Needs Admin Followup");

                                        Entity postImage = (Entity)context.PostEntityImages["Image"];
                                        if (postImage.Contains("msdyn_opportunityid"))
                                        {
                                            tracingService.Trace("Opporutntiy exists in image");

                                            EntityReference opportunityRef = postImage.GetAttributeValue<EntityReference>("msdyn_opportunityid");
                                            if (opportunityRef != null)
                                            {
                                                tracingService.Trace("Opportunity ref not null");
                                                Entity updateOpportunity = new Entity(opportunityRef.LogicalName, opportunityRef.Id);
                                                updateOpportunity["statuscode"] = new OptionSetValue(126300013);//InProgress-Admin
                                                service.Update(updateOpportunity);
                                            }
                                        }
                                        else
                                        {
                                            throw new InvalidPluginExecutionException("Opportunity is not selected");
                                        }
                                    }
                                    //}
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