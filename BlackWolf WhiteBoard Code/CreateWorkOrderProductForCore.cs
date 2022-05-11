using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackWolf_WhiteBoard_Code.Model;


namespace BlackWolf_WhiteBoard_Code
{
    public class CreateWorkOrderProductForCore : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();
            //   throw new InvalidPluginExecutionException("CreateProductFromDescriptionAndRelatetoProductFamily");

            try
            {

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

                        if (entity.LogicalName == "msdyn_workorderproduct")
                        {
                            tracingService.Trace("Update of msdyn_workorderproduct");

                            if (entity.Contains("ap360_core"))

                            {
                                Entity reterivedWOP = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));

                                bool core = reterivedWOP.GetAttributeValue<bool>("ap360_core");
                                string partNumber = reterivedWOP.GetAttributeValue<string>("ap360_partnumber");

                                if (core == true)
                                {
                                   Entity coreProduct = Product.getCoreProduct(service, tracingService, partNumber);

                                   WorkOrderProduct.CreateWOPforCore(service, tracingService, reterivedWOP, coreProduct); 
                                }
                                else if (core == false)
                                {

                                    EntityReference WorkOrder = reterivedWOP.GetAttributeValue<EntityReference>("msdyn_workorder");
                                    

                                    if (WorkOrder != null && partNumber != "")
                                    {
                                        WorkOrderProduct.DeleteCoreWorkOrderProduct(service, tracingService, WorkOrder.Id, partNumber);
                                    }


                                }
                            }

                        }
                    }

                }
                if (context.MessageName.ToLower() == "create")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        entity = (Entity)context.InputParameters["Target"];

                        if (entity.LogicalName == "msdyn_workorderproduct")
                        {
                            tracingService.Trace("Create of msdyn_workorderproduct");



                            Entity reterivedWOP = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));

                            bool core = reterivedWOP.GetAttributeValue<bool>("ap360_core");
                            string partNumber = reterivedWOP.GetAttributeValue<string>("ap360_partnumber");
                            if (core == true)
                            {
                                Entity coreProduct = Product.getCoreProduct(service, tracingService, partNumber);
                                WorkOrderProduct.CreateWOPforCore(service, tracingService, reterivedWOP, coreProduct);
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
