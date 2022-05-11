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
    public class ApproveProductStructureAsFamily : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();

            try
            {

                // throw new InvalidPluginExecutionException("ApproveProductStructureAsFamily");
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
                        if (context.Depth == 1)
                        {
                            entity = (Entity)context.InputParameters["Target"];

                            if (entity.LogicalName == "ap360_quoteproduct" || entity.LogicalName == "msdyn_workorderproduct")
                            {
                                tracingService.Trace("Inside update of product");

                                tracingService.Trace("Entity Name " + entity.LogicalName);

                                if (entity.Contains("ap360_addtoproductfamily"))
                                {
                                    if (entity.GetAttributeValue<bool>("ap360_addtoproductfamily"))
                                    {
                                        tracingService.Trace("Approve Product as Family");
                                        Entity productEntity = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("ap360_productfamily", "ap360_product"));
                                        if (productEntity != null)
                                        {
                                            EntityReference productFamilyRef = productEntity.GetAttributeValue<EntityReference>("ap360_productfamily") != null ? productEntity.GetAttributeValue<EntityReference>("ap360_productfamily") : null;
                                            EntityReference productRef = productEntity.GetAttributeValue<EntityReference>("ap360_product") != null ? productEntity.GetAttributeValue<EntityReference>("ap360_product") : null;

                                            if (productFamilyRef != null && productRef != null)
                                            {
                                                tracingService.Trace("Product Family Reference is not null");
                                                int productExistCount = 0;
                                                productExistCount = Product.CheckProductFamilyByName(service, tracingService, productRef.Name);
                                                if (productExistCount > 0) // product family with same name already exists
                                                {
                                                    throw new InvalidPluginExecutionException(productRef.Name + " Product Family already exists");

                                                }
                                                Entity productFamily = new Entity("product");
                                                productFamily["name"] = productRef.Name;

                                                // product.Id = productFamilyRef.Id;
                                                productFamily["parentproductid"] = new EntityReference("product", productFamilyRef.Id);
                                                productFamily["productstructure"] = new OptionSetValue(2);//product Family
                                                Guid newlyCreatedProductFamily = service.Create(productFamily);
                                                tracingService.Trace("Produrt Parent is updated");
                                                SetStateRequest setStateRequest = new SetStateRequest()
                                                {
                                                    EntityMoniker = new EntityReference
                                                    {
                                                        Id = newlyCreatedProductFamily,
                                                        LogicalName = "product",
                                                    },
                                                    State = new OptionSetValue(0),//Status : Active
                                                    Status = new OptionSetValue(1)//Status Reason : Active
                                                };
                                                service.Execute(setStateRequest);
                                                tracingService.Trace("Product is aproved as Family");

                                                SetStateRequest setStateRequestproduct = new SetStateRequest()
                                                {
                                                    EntityMoniker = new EntityReference
                                                    {
                                                        Id = productRef.Id,
                                                        LogicalName = "product",
                                                    },
                                                    State = new OptionSetValue(3),//Status : under revision
                                                    Status = new OptionSetValue(3)//Status Reason : under revsion
                                                };
                                                service.Execute(setStateRequestproduct);

                                                Entity product = new Entity("product");
                                                product.Id = productRef.Id;
                                                product["parentproductid"] = new EntityReference("product", newlyCreatedProductFamily);
                                                service.Update(product);

                                                SetStateRequest setStateRequestproduct1 = new SetStateRequest()
                                                {
                                                    EntityMoniker = new EntityReference
                                                    {
                                                        Id = productRef.Id,
                                                        LogicalName = "product",
                                                    },
                                                    State = new OptionSetValue(0),//Status : Active
                                                    Status = new OptionSetValue(1)//Status Reason : Active
                                                };
                                                service.Execute(setStateRequestproduct1);

                                                Entity quoteProduct = new Entity(entity.LogicalName);
                                                quoteProduct["ap360_addtoproductfamily"] = false;
                                                quoteProduct.Id = entity.Id;
                                                service.Update(quoteProduct);

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