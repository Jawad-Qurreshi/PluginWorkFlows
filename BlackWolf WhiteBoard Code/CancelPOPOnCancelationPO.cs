using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;



namespace BlackWolf_WhiteBoard_Code
{
    public class CancelPOPOnCancelationPO : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();

            try
            {

                ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = factory.CreateOrganizationService(context.UserId);

                Entity entity = null;

                if (context.MessageName.ToLower() == "update")
                {
                    tracingService.Trace("Inside Update of PO");
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        entity = (Entity)context.InputParameters["Target"];

                        if (entity.LogicalName == "msdyn_purchaseorder")
                        {
                            tracingService.Trace("entity.LogicalName");
                            if (entity.Contains("msdyn_systemstatus"))
                            {
                                tracingService.Trace("entity.Contains(msdyn_systemstatus)");
                                var systemStatus = entity.GetAttributeValue<OptionSetValue>("msdyn_systemstatus").Value;
                                if (systemStatus == 690970002) //Cancelled
                                {
                                    tracingService.Trace("systemStatus == 690970002");
                                    EntityCollection POPCollection = PurchaseOrderProduct.RetrievePOPOnBaseOfPO(service, tracingService, entity.Id.ToString());
                                    foreach (Entity POP in POPCollection.Entities) {
                                        tracingService.Trace("inside foreach");
                                        //var POPItemSubStatus = POP.GetAttributeValue<OptionSetValue>("ap360_itemsubstatus").Value;
                                        int POPItemSubStatus = POP.GetAttributeValue<OptionSetValue>("ap360_itemsubstatus") != null ? POP.GetAttributeValue<OptionSetValue>("ap360_itemsubstatus").Value : 0;
                                        tracingService.Trace("POPItemSubStatus " + POPItemSubStatus);
                                        //Received    126300000
                                        //Partially Received  126300001
                                        //Pending 126300002
                                        //Canceled    126300003
                                        if (POPItemSubStatus == 126300000 || POPItemSubStatus == 126300001)//Received || partially received
                                        {
                                            throw new InvalidPluginExecutionException("Purchase Order Cannot be cancelled");
                                        }
                                        else {
                                            tracingService.Trace("Updateing status ");
                                            POP["msdyn_itemstatus"] = new OptionSetValue(690970002);//Cancelled
                                            POP["ap360_itemsubstatus"] = new OptionSetValue(126300002);//Cancelled
                                            service.Update(POP);
                                        }

                                    }
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