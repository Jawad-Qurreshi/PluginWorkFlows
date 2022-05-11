
using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class RTVProductManagement : IPlugin
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

                entity = (Entity)context.InputParameters["Target"];

                if (context.MessageName.ToLower() == "create")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {

                        double quantity = entity.GetAttributeValue<double>("msdyn_quantity");
                       
                 
                        entity["ap360_quantityreceived"] = 0.00;
                        entity["ap360_partialreceivequantity"] = quantity;
                        

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