using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using BlackWolf_WhiteBoard_Code.Model;


namespace BlackWolf_WhiteBoard_Code
{
   public class ManageWOSTHealthLog : IPlugin
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


                if (context.MessageName.ToLower() == "create")
                {
                   
                    tracingService.Trace("WOST created in 37 Line");
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        tracingService.Trace("context.InputParameters.Contains('Target') && context.InputParameters['Target'] is Entity");
                        entity = (Entity)context.InputParameters["Target"];

                        if (entity.LogicalName == "ap360_workorderservicetaskhealthlog")
                        {
                            tracingService.Trace("entity.LogicalName == 'ap360_workorderservicetaskhealthlog'");
                           
                           
                             EntityReference WOSTRef = entity.GetAttributeValue<EntityReference>("ap360_workorderservicetaskid") ?? null;
                             EntityReference BRBRef = entity.GetAttributeValue<EntityReference>("ap360_bookableresourcebookingid") ?? null;
                            
                            tracingService.Trace("WOST " + WOSTRef.LogicalName.ToString());
                            tracingService.Trace("BRBRef " + BRBRef.LogicalName.ToString());
                           // throw new InvalidPluginExecutionException(BRBRef.LogicalName.ToString());

                            if (WOSTRef != null && BRBRef != null) {
                                     
                                //    WorkOrderServiceTaskHealthLog.DeleteOldWOSTHealthLog(service, tracingService,  BRBRef , WOSTRef);

                                    tracingService.Trace("After deletion");
                              }
                          
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message.ToString());
            }
        }
    }
}
