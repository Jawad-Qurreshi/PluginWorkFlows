using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class PublishAndSetFamilyofProduct : IPlugin
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
                    tracingService.Trace("Inside update of product");
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        if (context.Depth == 1)
                        {
                            entity = (Entity)context.InputParameters["Target"];

                            if (entity.LogicalName == "product")
                            {
                                tracingService.Trace("Entity Name " + entity.LogicalName);

                                if (entity.Contains("ap360_approveproduct"))
                                {
                                    if (entity.GetAttributeValue<bool>("ap360_approveproduct"))
                                    {
                                        tracingService.Trace("Approve Product Updated");
                                        Entity productEntity = service.Retrieve("product", entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("ap360_productfamily"));
                                        if (productEntity != null)
                                        {
                                            EntityReference productFamilyRef = productEntity.GetAttributeValue<EntityReference>("ap360_productfamily") != null ? productEntity.GetAttributeValue<EntityReference>("ap360_productfamily") : null;
                                            if (productFamilyRef != null)
                                            {
                                                tracingService.Trace("Product Family Reference is not null");

                                                Entity product = new Entity("product");
                                                product.Id = entity.Id;
                                                product["parentproductid"] = new EntityReference("product", productFamilyRef.Id);
                                               product["productstructure"] = new OptionSetValue(1);//product
                                                                                                   //product["productstructure"] = new OptionSetValue(2);//family
                                                product["msdyn_fieldserviceproducttype"] = new OptionSetValue(690970000);//Inventory


                                                service.Update(product);
                                                tracingService.Trace("Produrt Parent is updated");
                                                SetStateRequest setStateRequest = new SetStateRequest()
                                                {
                                                    EntityMoniker = new EntityReference
                                                    {
                                                        Id = entity.Id,
                                                        LogicalName = "product",
                                                    },
                                                    State = new OptionSetValue(0),//Status : Active
                                                    Status = new OptionSetValue(1)//Status Reason : Active
                                                };
                                                service.Execute(setStateRequest);
                                                tracingService.Trace("Product if Published");

                                            }
                                        }
                                        tracingService.Trace("Plugin Ended Successfully");
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