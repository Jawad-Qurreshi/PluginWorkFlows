using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class WOSTAndWorkOrderStatusMatrix : IPlugin
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

                        if (entity.LogicalName == "msdyn_workorderservicetask")
                        {
                            tracingService.Trace("msdyn_workorderservicetask is udpated");
                            if (entity.Contains("ap360_workorderservicetaskstatus"))
                            {

                                tracingService.Trace("ap360_workorderservicetaskstatus is updated");
                                Entity postImage = (Entity)context.PostEntityImages["Image"];

                                if (postImage.Contains("msdyn_workorder"))
                                {
                                    tracingService.Trace("workorder image exists");
                                    EntityReference workOrderRef = postImage.GetAttributeValue<EntityReference>("msdyn_workorder");
                                    EntityReference opportunityRef = postImage.GetAttributeValue<EntityReference>("ap360_opportunityid");

                                    if (workOrderRef != null)
                                    {

                                        IDictionary<OptionSetValue, string> lstAlreadyWorkOrderBWStatusValue = null;
                                        lstAlreadyWorkOrderBWStatusValue = WorkOrder.getWorkOrderBWStatus(service, tracingService, workOrderRef.Id);

                                        List<Entity> lstWOSTs = WorkOrderServiceTask.GetWOSTrelatedToWorkOrder(service, tracingService, workOrderRef.Id);
                                        tracingService.Trace("count of WOSt " + lstWOSTs.Count.ToString());
                                        //List<OptionSetValue> lstWorkOrderBWStatus = WorkOrderServiceTask.GetWorkOrderBWStatus(lstWOSTs, tracingService);
                                        IDictionary<OptionSetValue, string> lstWorkOrderBWStatusCollection = WorkOrderServiceTask.GetWorkOrderBWStatusCollection(lstWOSTs, tracingService, lstAlreadyWorkOrderBWStatusValue);

                                                                               ////////////////////////////////////////////Only when WOST status updated to NotStarted Based on WOP recevied///////////////////
                                        /////////////////////////////////if there is already WO inprogress then don't change next workOrder to Inprogress//////////
                                        int countOFWOInProgress = 0;
                                        OptionSetValue woststatus = entity.GetAttributeValue<OptionSetValue>("ap360_workorderservicetaskstatus");
                                        List<WorkOrder> lstWorkOrders = new List<WorkOrder>();
                                        if (woststatus != null && opportunityRef != null)
                                        {
                                            if (woststatus.Value == 126300001)//Not Started
                                            {
                                                lstWorkOrders = WorkOrder.getListofWorkOrderRelatedToOpportunity(service, tracingService, opportunityRef.Id);
                                                countOFWOInProgress = WorkOrder.getNumberofWorkOrderCountInProgress(lstWorkOrders, tracingService);
                                                if (countOFWOInProgress > 0)
                                                {
                                                    tracingService.Trace(countOFWOInProgress.ToString() + " WorkOrders are in progress under " + opportunityRef.Name);
                                                    //lstWorkOrderBWStatus.RemoveAll(x=>x.Value == 126300008);//Available For Booking-InProgress
                                                    //lstWorkOrderBWStatus.Add(new OptionSetValue(126300006));//Awaiting Predecessor

                                                    // lstWorkOrderBWStatusCollection.Remove(new OptionSetValue(126300008));//Available For Booking-InProgress
                                                    // if (!lstWorkOrderBWStatusCollection.ContainsKey(new OptionSetValue(126300006)))
                                                    if (!lstWorkOrderBWStatusCollection.ContainsKey(new OptionSetValue(126300008)))//Available For Booking-InProgress: this is important if
                                                        //workorder is already in progress then no need to change status back to Awaiting Predecessor
                                                    {
                                                        lstWorkOrderBWStatusCollection.Add(new OptionSetValue(126300006), "Awaiting Predecessor");//Awaiting Predecessor
                                                    }
                                                }
                                            }
                                        }

                                        if (lstWorkOrderBWStatusCollection.Count > 0)
                                        {
                                            tracingService.Trace("lstWorkOrderBWStatusCollection for WO Matrix is greater then 0 : " + lstWorkOrderBWStatusCollection.Count.ToString());
                                          ///  throw new InvalidPluginExecutionException("On flag wost matrix triggerd");

                                            WorkOrder.updateWOBWStatus(service, tracingService, lstWorkOrderBWStatusCollection, workOrderRef);
                                        }

                                    }
                                }
                                // service.Update(postImage);
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