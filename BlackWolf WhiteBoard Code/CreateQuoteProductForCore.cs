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
    public class CreateQuoteProductForCore : IPlugin
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

                if (context.Depth == 1)
                {

                    if (context.MessageName.ToLower() == "update")
                    {
                        if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                        {
                            entity = (Entity)context.InputParameters["Target"];

                            if (entity.LogicalName == "ap360_quoteproduct")
                            {
                                tracingService.Trace("Update of ap360_quoteproduct");

                                if (entity.Contains("ap360_core"))
                                {
                                    bool core = entity.GetAttributeValue<bool>("ap360_core");
                                    if (core == true)
                                    {
                                        Entity reterivedQP = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
                                        string partNumber = reterivedQP.GetAttributeValue<string>("ap360_partnumber");
                                        tracingService.Trace("Part Number "+partNumber);

                                        Entity coreProduct = Product.getCoreProduct(service, tracingService, partNumber);
                                        
                                        QuoteProduct.CreateQPforCore(service, tracingService, reterivedQP, coreProduct);
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

                            if (entity.LogicalName == "ap360_quoteproduct")
                            {
                                tracingService.Trace("Create of ap360_quoteproduct");
                                bool core = entity.GetAttributeValue<bool>("ap360_core");
                                if (core == true)
                                {
                                    Entity reterivedQP = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
                                    string partNumber = reterivedQP.GetAttributeValue<string>("ap360_partnumber");
                                    tracingService.Trace("Create : part number " + partNumber);
                                    Entity coreProduct = Product.getCoreProduct(service, tracingService, partNumber);
                                    QuoteProduct.CreateQPforCore(service, tracingService, reterivedQP, coreProduct);
                                }

                            }

                        }
                    }
                 //   throw new InvalidPluginExecutionException("Error");
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
