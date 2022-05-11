using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace BlackWolf_WhiteBoard_Code
{
    public class WOPANDWOSTStatusMatrix : IPlugin
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

                        if (entity.LogicalName == "msdyn_workorderproduct")
                        {
                            Entity postImage = (Entity)context.PostEntityImages["Image"];
                            EntityReference workOrderServiceTaskRef = postImage.GetAttributeValue<EntityReference>("ap360_workorderservicetaskid");
                            tracingService.Trace("msdyn_workorderproduct is udpated");
                            if (entity.Contains("ap360_workorderproductstatus") || entity.Contains("ap360_workorderservicetaskid"))
                            {

                                tracingService.Trace("msdyn_linestatus is updated");

                                if (postImage.Contains("ap360_workorderservicetaskid"))
                                {
                                    tracingService.Trace("ap360_workorderservicetaskid image exists");

                                    if (workOrderServiceTaskRef != null)
                                    {

                                        List<Entity> lstWOPs = WorkOrderProduct.GetWOPRelatedToWOST(service, tracingService, workOrderServiceTaskRef.Id);

                                        tracingService.Trace("Count of WOPS " + lstWOPs.Count.ToString());
                                        int count = 0;
                                        foreach (Entity wop in lstWOPs)
                                        {
                                            OptionSetValue wopStatus = wop.GetAttributeValue<OptionSetValue>("ap360_workorderproductstatus");
                                            if (wopStatus != null && wopStatus.Value == 126300001)//Received
                                            {
                                                count++;

                                            }
                                        }
                                        tracingService.Trace("Received WOPS " + count.ToString());
                                        Entity updateWOST = new Entity(workOrderServiceTaskRef.LogicalName, workOrderServiceTaskRef.Id);
                                        if (count == lstWOPs.Count)
                                        {
                                            tracingService.Trace("Count and LstWOPs is equal");

                                            updateWOST["ap360_workorderservicetaskstatus"] = new OptionSetValue(126300001);//Not Started	
                                                                                                                           // throw new InvalidPluginExecutionException("Not started");
                                        }
                                        else
                                        {
                                            updateWOST["ap360_workorderservicetaskstatus"] = new OptionSetValue(126300011);//awaiting dependency	
                                            //throw new InvalidPluginExecutionException("Awaiting dependency");
                                        }
                                        service.Update(updateWOST);
                                    }
                                }
                            }

                            //if (entity.Contains("ap360_workorderservicetaskid"))
                            //{
                            //    Entity updateWOST = new Entity(workOrderServiceTaskRef.LogicalName, workOrderServiceTaskRef.Id);
                            //    updateWOST["ap360_workorderservicetaskstatus"] = new OptionSetValue(126300011);//awaiting dependency	
                            //    service.Update(updateWOST);

                            //}
                        }
                    }
                    //   throw new InvalidProgramException("Custom error");
                }
                if (context.MessageName.ToLower() == "create")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        entity = (Entity)context.InputParameters["Target"];

                        if (entity.LogicalName == "msdyn_workorderproduct")
                        {
                            Entity postImage = (Entity)context.PostEntityImages["Image"];
                            if (postImage.Contains("ap360_workorderservicetaskid"))
                            {
                                EntityReference workOrderServiceTaskRef = postImage.GetAttributeValue<EntityReference>("ap360_workorderservicetaskid");
                                tracingService.Trace("msdyn_workorderproduct is created");
                                if (workOrderServiceTaskRef != null)
                                {

                                    tracingService.Trace("msdyn_linestatus is updated");

                                    Entity updateWOST = new Entity(workOrderServiceTaskRef.LogicalName, workOrderServiceTaskRef.Id);
                                    updateWOST["ap360_workorderservicetaskstatus"] = new OptionSetValue(126300011);//awaiting dependency	
                                    service.Update(updateWOST);

                                }
                            }
                        }
                    }
                    //   throw new InvalidProgramException("Custom error");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
