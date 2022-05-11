using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class MapProductPartNumberToProductInventory : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();

            try
            {
                // throw new InvalidPluginExecutionException("MapProductPartNumberToProductInventory");

                //throw new InvalidPluginExecutionException("throw");
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
                        if (entity.LogicalName == "msdyn_productinventory")
                        {

                            if (entity.Contains("msdyn_product"))
                            {
                                tracingService.Trace("Inside Creation of product inventory");
                                EntityReference productRef = entity.GetAttributeValue<EntityReference>("msdyn_product") != null ? entity.GetAttributeValue<EntityReference>("msdyn_product") : null;


                                if (productRef != null)
                                {
                                    Entity productEntity = service.Retrieve("product", productRef.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("ap360_partnumber"));

                                    string PartNumber = null;
                                    PartNumber = productEntity.GetAttributeValue<string>("ap360_partnumber");
                                    if (PartNumber != null && PartNumber != string.Empty)
                                    {
                                        tracingService.Trace("mapping part number to product");

                                        entity["ap360_partnumber"] = PartNumber;

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