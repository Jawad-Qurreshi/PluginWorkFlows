
using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class FlagWorkOrderForAdminHelp : IPlugin
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

                        if (entity.LogicalName == "msdyn_workorderservicetask" || entity.LogicalName == "msdyn_workorderproduct" || entity.LogicalName == "ap360_workordersublet" || entity.LogicalName == "bookableresourcebooking")
                        {
                            tracingService.Trace("Entity Name " + entity.LogicalName);

                            if (entity.Contains("ap360_adminfollowup"))
                            {
                                tracingService.Trace("Follup is triggered");

                                if (entity.GetAttributeValue<Boolean>("ap360_adminfollowup"))
                                {
                                    string workOrderFieldName = WorkOrder.getWorkOrderFieldName(entity.LogicalName);
                                    if (workOrderFieldName != null && workOrderFieldName != "")
                                    {
                                        tracingService.Trace("Work Order field name is  " + workOrderFieldName);
                                        Entity postImage = (Entity)context.PostEntityImages["Image"];
                                        if (postImage.Contains(workOrderFieldName))
                                        {
                                            EntityReference workOrderRef = postImage.GetAttributeValue<EntityReference>(workOrderFieldName);
                                            EntityReference opportunityRef = postImage.GetAttributeValue<EntityReference>("ap360_opportunityid");

                                            if (workOrderRef != null)
                                            {
                                                IDictionary<OptionSetValue, string> lstAlreadyWorkOrderBWStatusValue = null;
                                                  lstAlreadyWorkOrderBWStatusValue= WorkOrder.getWorkOrderBWStatus(service, tracingService, workOrderRef.Id);


                                                List<Entity> lstWOSTs = WorkOrderServiceTask.GetWOSTrelatedToWorkOrder(service, tracingService, workOrderRef.Id);
                                                tracingService.Trace("count of WOSt in flagworkorderforadminhelp " + lstWOSTs.Count.ToString());
                                                //List<OptionSetValue> lstWorkOrderBWStatus = WorkOrderServiceTask.GetWorkOrderBWStatus(lstWOSTs, tracingService);
                                                IDictionary<OptionSetValue, string> lstWorkOrderBWStatusCollection = WorkOrderServiceTask.GetWorkOrderBWStatusCollection(lstWOSTs, tracingService, lstAlreadyWorkOrderBWStatusValue);
                                                tracingService.Trace("After GetWorkOrderBWStatusCollection in flag plugin");
                                                //  lstWorkOrderBWStatusCollection.Add(new OptionSetValue(126300003), "Needs Admin FollowUp");//Needs Admin FollowUp

                                                if (lstWorkOrderBWStatusCollection.Count > 0)
                                                {
                                                    tracingService.Trace("lstWorkOrderBWStatusCollection for flag is greater then 0 : " + lstWorkOrderBWStatusCollection.Count.ToString());
                                                    WorkOrder.updateWOBWStatus(service, tracingService, lstWorkOrderBWStatusCollection, workOrderRef);

                                                }
                                            }
                                        }
                                        else
                                        {
                                            throw new InvalidPluginExecutionException("Work Order is not selected");
                                        }
                                    }
                                }

                            }

                            if (entity.Contains("ap360_workorderservicetaskstatus"))//Only for WOST status 
                            {
                                tracingService.Trace("work order service task status updated");
                                int wostStatus = entity.GetAttributeValue<OptionSetValue>("ap360_workorderservicetaskstatus").Value;


                                if (
                                     wostStatus == 126300000//Need Manger Decision
                                                            //wostStatus == 126300011//awaiting dependency //Don"t add this option While techinican clocking out
                                                            // because same work we are implmenting from WOP to WOST
                                    || wostStatus == 126300002//Time Used Up/ Incomplete ST	
                                    || wostStatus == 126300003//Needs External help/ Expert	
                                    || wostStatus == 126300004//needs lead tech guidance	
                                    || wostStatus == 126300007//Needs Discovery Estimated	
                                    )
                                {
                                    // throw new InvalidPluginExecutionException("Error");
                                    tracingService.Trace("work order image exists");
                                    //EntityReference workOrderRef = postImage.GetAttributeValue<EntityReference>("msdyn_workorder");
                                    //if (workOrderRef != null)
                                    //{
                                    Entity updateWOST = new Entity(entity.LogicalName, entity.Id);
                                    updateWOST["ap360_adminfollowup"] = true;
                                    service.Update(updateWOST);
                                    //WorkOrderServiceTask.updateWorkOrderStatus(service,tracingService,workOrderRef, wostStatus);
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