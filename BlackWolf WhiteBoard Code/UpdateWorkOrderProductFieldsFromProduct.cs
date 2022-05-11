using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
  public  class UpdateWorkOrderProductFieldsFromProduct : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();
            // throw new InvalidPluginExecutionException("PublishAndSetFamilyofProduct");

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


                tracingService.Trace("Inside of Create Product");



                if (context.MessageName.ToLower() == "update")
                {
                    tracingService.Trace("Inside  UpdateWorkOrderProductFieldsFromProduct");
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        if (context.Depth == 1)
                        {
                            entity = (Entity)context.InputParameters["Target"];

                            if (entity.LogicalName == "product")
                            {
                                tracingService.Trace("Entity Name " + entity.LogicalName);

                                if (entity.Contains("name") || entity.Contains("ap360_partnumber"))
                                {
                                    string productName = null;
                                    string productPartNumber = null;
                                    if (entity.Contains("name"))
                                    {
                                        productName = entity.GetAttributeValue<string>("name");
                                    }
                                    if (entity.Contains("ap360_partnumber"))
                                    {
                                        productPartNumber = entity.GetAttributeValue<string>("ap360_partnumber");
                                    }

                                    EntityCollection workOrderProductCollection = WorkOrderProduct.getWorkOrderProductsRelatedToProduct(service, tracingService, entity.Id);

                                    foreach (Entity workOrderProduct in workOrderProductCollection.Entities)
                                    {

                                        Entity updateWorkOrderProduct = new Entity(workOrderProduct.LogicalName, workOrderProduct.Id);
                                        if (entity.Contains("name"))
                                        {
                                            updateWorkOrderProduct["ap360_productdescription"] = productName;
                                            updateWorkOrderProduct["msdyn_name"] = productName;
                                            updateWorkOrderProduct["ap360_name"] = productName;
                                        }
                                        if (entity.Contains("ap360_partnumber"))
                                        {
                                            updateWorkOrderProduct["ap360_partnumber"] = productPartNumber;
                                        }
                                        if (entity.Contains("name") || entity.Contains("ap360_partnumber"))
                                        {
                                            service.Update(updateWorkOrderProduct);
                                        }

                                    }
                                }

                            }
                        }
                        //  throw new InvalidPluginExecutionException();
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