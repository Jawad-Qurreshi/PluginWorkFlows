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
    public class AdjustProductInventory : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            //Comment by Faisal
            //This whole Plugin disabled and no longer needed because we have the same information on Product Inventory
            try
            {
                ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = factory.CreateOrganizationService(context.UserId);

                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    if (context.MessageName.ToLower() == "create")
                    {

                        Guid newSubId = Guid.Empty;
                        Entity Target = (Entity)context.InputParameters["Target"];
                        if (Target != null)
                        {
                            if (Target.LogicalName.ToLower() == "msdyn_productinventory")
                            {
                                tracingService.Trace("Inside Product Inventory Create ");
                                Entity reterivedProductInventory = service.Retrieve(Target.LogicalName, Target.Id, new ColumnSet("msdyn_product", "msdyn_warehouse", "msdyn_unit", "msdyn_qtyonorder", "msdyn_qtyonhand", "msdyn_qtyallocated", "msdyn_qtyavailable"));
                                if (reterivedProductInventory != null)
                                {
                                    EntityReference product = reterivedProductInventory.GetAttributeValue<EntityReference>("msdyn_product");

                                    if (product != null)
                                    {

                                        double quantityAvailable = reterivedProductInventory.GetAttributeValue<double>("msdyn_qtyavailable");
                                        double preveiousQuanityAvailalbeInInventory = ProductInventory.getProducutQuantityAvaiable(service, tracingService, product.Id);
                                        tracingService.Trace("Quantity Available "+ preveiousQuanityAvailalbeInInventory.ToString());
                                        Entity updateProductEntity = new Entity(product.LogicalName, product.Id);
                                        updateProductEntity["ap360_quantityavailable"] = preveiousQuanityAvailalbeInInventory;
                                       service.Update(updateProductEntity);

                                    }

                                }
                            }
                        }
                    }


                    if (context.MessageName.ToLower() == "update")
                    {

                        tracingService.Trace("Inside Product  InventoryUpdate ");

                        Guid newSubId = Guid.Empty;
                        Entity Target = (Entity)context.InputParameters["Target"];
                        if (Target != null)
                        {
                            if (Target.LogicalName.ToLower() == "msdyn_productinventory")
                            {

                                if (Target.Contains("msdyn_qtyavailable"))
                                {
                                    Entity reterivedProductInventory = service.Retrieve(Target.LogicalName, Target.Id, new ColumnSet("msdyn_product", "msdyn_warehouse", "msdyn_unit", "msdyn_qtyonorder", "msdyn_qtyonhand", "msdyn_qtyallocated", "msdyn_qtyavailable"));
                                    if (reterivedProductInventory != null)
                                    {
                                        EntityReference product = reterivedProductInventory.GetAttributeValue<EntityReference>("msdyn_product");

                                        if (product != null)
                                        {

                                            double quantityAvailable = reterivedProductInventory.GetAttributeValue<double>("msdyn_qtyavailable");
                                            double preveiousQuanityAvailalbeInInventory = ProductInventory.getProducutQuantityAvaiable(service, tracingService, product.Id);
                                            Entity updateProductEntity = new Entity(product.LogicalName, product.Id);
                                            updateProductEntity["ap360_quantityavailable"] = preveiousQuanityAvailalbeInInventory;
                                            service.Update(updateProductEntity);

                                        }


                                    }
                                }
                            }
                        }
                    }
                }
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference)
                {
                    if (context.MessageName.ToLower() == "delete")
                    {

                        tracingService.Trace("Inside Product Inventory delete ");

                        EntityReference TargetRef = (EntityReference)context.InputParameters["Target"];
                        if (TargetRef.LogicalName.ToLower() == "msdyn_productinventory")
                        {
                            Entity reterivedProductInventory = service.Retrieve(TargetRef.LogicalName, TargetRef.Id, new ColumnSet("msdyn_product", "msdyn_warehouse", "msdyn_unit", "msdyn_qtyonorder", "msdyn_qtyonhand", "msdyn_qtyallocated", "msdyn_qtyavailable"));

                            if (reterivedProductInventory != null)
                            {
                                EntityReference product = reterivedProductInventory.GetAttributeValue<EntityReference>("msdyn_product");

                                if (product != null)
                                {

                                    double quantityAvailable = reterivedProductInventory.GetAttributeValue<double>("msdyn_qtyavailable");
                                    double preveiousQuanityAvailalbeInInventory = ProductInventory.getProducutQuantityAvaiable(service, tracingService, product.Id);
                                    Entity updateProductEntity = new Entity(product.LogicalName, product.Id);
                                    updateProductEntity["ap360_quantityavailable"] = preveiousQuanityAvailalbeInInventory - quantityAvailable;
                                    service.Update(updateProductEntity);

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