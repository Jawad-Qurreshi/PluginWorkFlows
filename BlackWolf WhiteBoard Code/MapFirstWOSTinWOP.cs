using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackWolf_WhiteBoard_Code.Model;

namespace BlackWolf_WhiteBoard_Code
{
    public class MapFirstWOSTinWOP : IPlugin
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

                        if (entity.LogicalName == "msdyn_workorderproduct")
                        {
                            tracingService.Trace("Update of msdyn_workorderproduct");

                            if (entity.Contains("ap360_core"))

                            {
                                Entity reterivedWOP = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));

                                bool core = reterivedWOP.GetAttributeValue<bool>("ap360_core");

                                if (core == true)
                                {
                                  //  WorkOrderProduct.CreateWOPforCore(service, tracingService, reterivedWOP);
                                }
                                else if (core == false)
                                {

                                    EntityReference WorkOrder = reterivedWOP.GetAttributeValue<EntityReference>("msdyn_workorder");
                                    string partnumber = reterivedWOP.GetAttributeValue<string>("ap360_partnumber");

                                    if (WorkOrder != null && partnumber != "")
                                    {
                                        WorkOrderProduct.DeleteCoreWorkOrderProduct(service, tracingService, WorkOrder.Id, partnumber);
                                    }


                                }
                            }

                        }
                    }

                }
                if (context.MessageName.ToLower() == "create")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        entity = (Entity)context.InputParameters["Target"];

                        if (entity.LogicalName == "msdyn_workorderproduct")
                        {
                            tracingService.Trace("Create of msdyn_workorderproduct");

                            //Entity reterivedWOP = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));

                            EntityReference WOST = entity.GetAttributeValue<EntityReference>("ap360_workorderservicetaskid");

                            if (WOST == null)
                            {
                                EntityReference workOrder = entity.GetAttributeValue<EntityReference>("msdyn_workorder");
                                if (workOrder == null)
                                {
                                    tracingService.Trace("Workorder was null and returned");
                                    return;
                                }
                               List<WorkOrderServiceTask> lstWOST = WorkOrderServiceTask.GetlstOfWOSTrelatedToWorkOrder(service, tracingService, workOrder.Id);
                                if (lstWOST.Count > 0) {
                                    tracingService.Trace("List of WOST Count"+lstWOST.Count.ToString());
                                    
                                    lstWOST = lstWOST.Where(x => x.WBSID > 0).ToList();
                                    var sortedWOSTsByWBSID = lstWOST.OrderBy(x => x.WBSID);
                                   
                                    tracingService.Trace("LIST of WOST Sorted");
                                    List<WorkOrderServiceTask> lstofSortedWost = sortedWOSTsByWBSID.ToList();
                                   
                                    tracingService.Trace("lstofSortedWorkOrdersCount"+ lstofSortedWost.Count.ToString());
                                    if (lstofSortedWost.Count > 0) { 
                                   
                                       Guid wostGuid = lstofSortedWost[0].WOSTGuid;
                                     
                                        Entity updateWOP = new Entity(entity.LogicalName, entity.Id);
                                        updateWOP["ap360_workorderservicetaskid"] = new EntityReference("msdyn_workorderservicetask", wostGuid);
                                        service.Update(updateWOP);
          
                                    }
                                   // throw new InvalidPluginExecutionException(WBSID.ToString());
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
