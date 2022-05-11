using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class MapWOSTTimeStampBookingToWOST : IPlugin
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


                if (context.MessageName.ToLower() == "create")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        entity = (Entity)context.InputParameters["Target"];

                        if (entity.LogicalName.ToLower() == "ap360_woservicetasktimestamp")
                        {
                            tracingService.Trace("ap360_woservicetasktimestamp is created");
                            MainFuction(tracingService, service, entity);

                        }
                    }
                }


                if (context.MessageName.ToLower() == "update")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        entity = (Entity)context.InputParameters["Target"];

                        if (entity.LogicalName.ToLower() == "ap360_woservicetasktimestamp")
                        {
                            tracingService.Trace("ap360_woservicetasktimestamp is created");
                            if(entity.Contains("ap360_actualamount"))
                            MainFuction(tracingService, service, entity);

                        }
                    }
                }


                //if (context.MessageName.ToLower() == "update")
                //{
                //    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                //    {
                //        entity = (Entity)context.InputParameters["Target"];

                //        if (entity.LogicalName.ToLower() == "ap360_woservicetasktimestamp")
                //        {
                //            if (entity.Contains("ap360_actualamount"))
                //            {
                //                Entity preImage = (Entity)context.PreEntityImages["Image"];
                //                Entity postImage = (Entity)context.PostEntityImages["Image"];
                //                Money postActualAmount = new Money();
                //                Money preActualAmount = new Money();
                //                postActualAmount = postImage.GetAttributeValue<Money>("ap360_actualamount") != null ? postImage.GetAttributeValue<Money>("ap360_actualamount") : null;
                //                preActualAmount = preImage.GetAttributeValue<Money>("ap360_actualamount") != null ? preImage.GetAttributeValue<Money>("ap360_actualamount") : null;

                //                if (postActualAmount.Value != preActualAmount.Value)
                //                {
                //                    throw new InvalidPluginExecutionException("Updated Pre " + preActualAmount.Value.ToString() + " Post " + postActualAmount.Value.ToString());
                //                    MainFuction(tracingService, service, entity);
                //                }
                //            }

                //        }
                //    }
                //}

            }




            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void MainFuction(ITracingService tracingService, IOrganizationService service, Entity entity)
        {
            Entity reterivedWOSTTimeStamp = service.Retrieve("ap360_woservicetasktimestamp", entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("ap360_bookableresourcebookingid", "ap360_workorderservicetaskid", "ap360_tsformasterbookingservicetask", "ap360_bookableresourceid"));

            EntityReference ap360_workorderservicetask = reterivedWOSTTimeStamp.GetAttributeValue<EntityReference>("ap360_workorderservicetaskid") != null ? reterivedWOSTTimeStamp.GetAttributeValue<EntityReference>("ap360_workorderservicetaskid") : null;

            if (ap360_workorderservicetask != null)
            {
                tracingService.Trace("ap360_workorderservicetask is not null");

                EntityReference ap360_bookableresourcebooking = reterivedWOSTTimeStamp.GetAttributeValue<EntityReference>("ap360_bookableresourcebookingid") != null ? reterivedWOSTTimeStamp.GetAttributeValue<EntityReference>("ap360_bookableresourcebookingid") : null;
                if (ap360_bookableresourcebooking != null)
                {
                    tracingService.Trace("ap360_bookableresourcebooking is not null");
                    Entity updateWorkOrderServiceTask = new Entity(ap360_workorderservicetask.LogicalName, ap360_workorderservicetask.Id);
                    //if (reterivedWOSTTimeStamp.GetAttributeValue<bool>("ap360_tsformasterbookingservicetask"))
                    //{

                    // tracingService.Trace("WOST Time stamp for master booking service task");
                    updateWorkOrderServiceTask["ap360_lastbookingworkedonid"] = ap360_bookableresourcebooking;
                    // }
                    EntityReference ap360_bookableresource = reterivedWOSTTimeStamp.GetAttributeValue<EntityReference>("ap360_bookableresourceid") != null ? reterivedWOSTTimeStamp.GetAttributeValue<EntityReference>("ap360_bookableresourceid") : null;

                    // updateWorkOrderServiceTask["ap360_recentlyworkedbookingid"] = ap360_bookableresourcebooking;
                     updateWorkOrderServiceTask["ap360_recentlyworkedresourceid"] = ap360_bookableresource;
                    TimeZoneInfo targetZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                    DateTime newDT = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, targetZone);
                    updateWorkOrderServiceTask["ap360_wosthealthupdate"] = newDT;
                    service.Update(updateWorkOrderServiceTask);
                    //  throw new InvalidPluginExecutionException();

                }
            }
        }
    }
}