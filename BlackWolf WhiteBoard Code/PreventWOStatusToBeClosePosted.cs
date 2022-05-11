using System;
using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class PreventWOStatusToBeClosePosted : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();

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
                    tracingService.Trace("WOST updated in 37 Line");
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        tracingService.Trace("context.InputParameters.Contains('Target') && context.InputParameters['Target'] is Entity");
                        entity = (Entity)context.InputParameters["Target"];
                        if (entity.LogicalName == "msdyn_workorder")
                        {
                            tracingService.Trace("entity.LogicalName == 'msdyn_workorder'");
                            if (entity.Contains("msdyn_systemstatus"))
                            {
                                tracingService.Trace("entity.Contains('msdyn_systemstatus')");
                                int SystemStatus =  entity.GetAttributeValue<OptionSetValue>("msdyn_systemstatus").Value;
                                if (SystemStatus == 690970004)//Closed-Posted
                                {

                                    List<Entity> WOST = WorkOrderServiceTask.GetWOSTrelatedToWorkOrder(service, tracingService, entity.Id);
                                    if(WOST.Count > 0){
                                        foreach (Entity wost in WOST)
                                        {
                                            double percentComplete = wost.GetAttributeValue<double>("msdyn_percentcomplete");
                                              //throw new InvalidPluginExecutionException("hello123 " + percentComplete.ToString());
                                            if (percentComplete < 100)
                                            {
                                                string description = wost.GetAttributeValue<string>("msdyn_description");
                                                throw new InvalidPluginExecutionException("Work Order cannot be Closed-Posted! \n Work Order Service Task: '" + description + "' is "+ percentComplete.ToString()+"% completed. \n It should be 100% completed");
                                            }
                                        }
                                    }

                                    EntityCollection WOPCollection =  WorkOrderProduct.getWorkOrderProductsRelatedToWorkOrder(service, tracingService, entity.Id);
                                    if (WOPCollection.Entities.Count > 0)
                                    {
                                        foreach (Entity WOP in WOPCollection.Entities)
                                        {

                                            int WOPLineStatus = WOP.GetAttributeValue<OptionSetValue>("msdyn_linestatus").Value;
                                            if (WOPLineStatus == 690970000)//Estimated
                                            {
                                                string WOPName = WOP.GetAttributeValue<string>("msdyn_name");
                                                EntityReference WOP_Product = WOP.GetAttributeValue<EntityReference>("ap360_product");
                                                string part_No = WOP.GetAttributeValue<string>("ap360_partnumber");
                                                throw new InvalidPluginExecutionException("Work Order cannot be Closed-Posted! \n Work Order Product: " + WOP_Product.Name + " : " + part_No.ToString() + " is Estimated. \n It should either be Cancelled or Used");
                                            }
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