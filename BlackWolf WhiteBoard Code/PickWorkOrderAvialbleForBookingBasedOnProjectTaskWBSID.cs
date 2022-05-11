using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class PickWorkOrderAvialbleForBookingBasedOnProjectTaskWBSID : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();
            // throw new InvalidPluginExecutionException("QuoteProductCalculation");

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


                //  tracingService.Trace("MoveQuoteServiceItemstoDifferentQuoteService");
                if (context.MessageName.ToLower() == "update")
                {

                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        entity = (Entity)context.InputParameters["Target"];

                        if (entity.LogicalName == "msdyn_workorder")
                        {
                            if ((entity.Contains("ap360_workorderbwstatustext")))
                            {
                                tracingService.Trace("WorkORder bw status text updated");
                                //if (entity.GetAttributeValue<OptionSetValue>("ap360_workorderbwstatustext") != null)
                                //throw new InvalidPluginExecutionException("testing");
                                //{
                                Entity postImage = (Entity)context.PostEntityImages["Image"];
                                string wobwstatustext = entity.GetAttributeValue<string>("ap360_workorderbwstatustext").ToString();
                                tracingService.Trace("WorkORder bw status text "+ wobwstatustext);

                                if (wobwstatustext.Contains("126300001"))//	
                                {
                                    tracingService.Trace("WorkORder bw status 126300001");
                                    if (postImage.Contains("ap360_wbsid"))
                                    {
                                        int wbsid = postImage.GetAttributeValue<int>("ap360_wbsid");
                                        wbsid++;
                                        tracingService.Trace("Wbs ID " + wbsid.ToString());
                                        Guid opportunityGuid = postImage.GetAttributeValue<EntityReference>("msdyn_opportunityid").Id;
                                        WorkOrder.updateWorkOrderToAvailableForBookingInProgress(service, tracingService, opportunityGuid, wbsid);
                                        // EntityCollection col = WorkOrder.GetNextWorkOrderBasedOnWBSID(service, tracingService, opportunityGuid, wbsid);
                                        //bool isWorkOrderEligibleForInProgress = WorkOrder.isWorkOrderEligibleForInProgress(service, tracingService, opportunityGuid, wbsid);

                                        //if (isWorkOrderEligibleForInProgress)
                                        //{

                                        //    Entity nextWorkOrderEntity = col.Entities[0];

                                        //    Entity updateWorkOrderEntity = new Entity("msdyn_workorder", nextWorkOrderEntity.Id);
                                        //    updateWorkOrderEntity["ap360_wobwstatus"] = new OptionSetValue(126300002);//Available for Booking
                                        //    service.Update(updateWorkOrderEntity);

                                        //}
                                        //if (col.Entities.Count > 0)
                                        //{

                                        //    Entity nextWorkOrderEntity = col.Entities[0];

                                        //    Entity updateWorkOrderEntity = new Entity("msdyn_workorder", nextWorkOrderEntity.Id);
                                        //    updateWorkOrderEntity["ap360_wobwstatus"] = new OptionSetValue(126300002);//Available for Booking
                                        //    service.Update(updateWorkOrderEntity);

                                        //}

                                        // throw new InvalidPluginExecutionException("Eror");
                                    }

                                }

                                // }



                            }
                        }

                    }
                }
               // throw new InvalidPluginExecutionException("Custom error");
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}