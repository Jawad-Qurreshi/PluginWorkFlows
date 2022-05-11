using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class ClosePostedWorkOrdersRelatedToOpprotunity : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();
            // throw new InvalidPluginExecutionException("CreateStandardWorkOrdersOnCreationofOpportunity");

            try
            {

                //throw new InvalidPluginExecutionException("throw");
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

                        if (entity.LogicalName == "opportunity")
                        {
                            tracingService.Trace("Inside ClosePostedWorkOrdersRelatedToOpprotunity");

                            if (entity.Contains("statuscode"))
                            {

                                int statuCode = entity.GetAttributeValue<OptionSetValue>("statuscode").Value;

                                if (statuCode == 126300010)//Delivered 
                                {

                                    List<Entity> lstWorkOrders = WorkOrder.GetWorkOrderRelatedtoOpportuntiy(service, tracingService, entity.Id);

                                    foreach (Entity workOrder in lstWorkOrders)
                                    {

                                        int msdyn_systemstatus = workOrder.GetAttributeValue<OptionSetValue>("msdyn_systemstatus").Value;

                                        if (msdyn_systemstatus != 690970004)//Closed - Posted
                                        {
                                            string workOrderName = workOrder.GetAttributeValue<string>("msdyn_name");

                                            throw new InvalidPluginExecutionException(workOrderName +" is not Closed Posted");

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