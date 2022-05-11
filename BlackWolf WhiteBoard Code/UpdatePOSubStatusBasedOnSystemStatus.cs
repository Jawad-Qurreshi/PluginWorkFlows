using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class UpdatePOSubStatusBasedOnSystemStatus : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();
            // throw new InvalidPluginExecutionException("QuoteServiceCalculation");



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



                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    entity = (Entity)context.InputParameters["Target"];
                    if (entity.LogicalName == "msdyn_purchaseorder")
                    {



                        if (context.MessageName.ToLower() == "update")
                        {



                            if (entity.Contains("msdyn_systemstatus"))
                            {
                                //Entity preImage = (Entity)context.PreEntityImages["Image"];



                                int systemStatus = entity.GetAttributeValue<OptionSetValue>("msdyn_systemstatus").Value;
                                //Draft    690970000
                                //Submitted   690970001
                                //Canceled    690970002
                                //Products Received   690970003
                                //Billed  690970004
                                if (systemStatus == 690970003)//Products Received
                                {
                                    Entity updatePO = new Entity(entity.LogicalName, entity.Id);
                                    updatePO["msdyn_substatus"] = new EntityReference("msdyn_purchaseordersubstatus", new Guid("765227fd-267d-eb11-a812-0022480299f1"));//Production Received
                                    service.Update(updatePO);
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
