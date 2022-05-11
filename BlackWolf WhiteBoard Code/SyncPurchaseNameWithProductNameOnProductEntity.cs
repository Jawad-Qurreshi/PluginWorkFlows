using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace BlackWolf_WhiteBoard_Code
{
    public class SyncPurchaseNameWithProductNameOnProductEntity : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                ITracingService tracingService =
                  (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                IPluginExecutionContext context = (IPluginExecutionContext)
                    serviceProvider.GetService(typeof(IPluginExecutionContext));

                Entity entity = null;

                if (context.MessageName.ToLower() == "update")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        entity = (Entity)context.InputParameters["Target"];

                        if (entity.LogicalName == "product")
                        {
                            if (entity.Contains("name"))
                            {
                                IOrganizationServiceFactory factory =
                                     (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                                IOrganizationService service = factory.CreateOrganizationService(context.UserId);

                                //  throw new InvalidPluginExecutionException("Custom error ");
                                var updatedName = entity.GetAttributeValue<string>("name");

                                tracingService.Trace("updated Name is " + updatedName);

                                if (updatedName != null)
                                {
                                    entity["msdyn_purchasename"] = updatedName;
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