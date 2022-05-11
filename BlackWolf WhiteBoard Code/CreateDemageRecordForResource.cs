using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
  public  class CreateDemageRecordForResource : IPlugin
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

                            if (entity.Contains("ap360_demagedbyid"))
                            {
                                tracingService.Trace("demagedby Updated");

                                EntityReference demageby = entity.GetAttributeValue<EntityReference>("ap360_demagedbyid") != null ? entity.GetAttributeValue<EntityReference>("ap360_demagedbyid") : null;

                                if (demageby != null)
                                {

                                    Entity createDemageBy = new Entity("ap360_demage");
                                    createDemageBy["ap360_name"] = "Demage " + demageby.Name;
                                    if (demageby != null)
                                    {
                                        createDemageBy["ap360_resourceid"] = demageby;
                                    }
                                    createDemageBy["ap360_workorderproductid"] = new EntityReference(entity.LogicalName, entity.Id);
                                    service.Create(createDemageBy);
                                }
                                tracingService.Trace("Plugin Ended Successfully");

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
