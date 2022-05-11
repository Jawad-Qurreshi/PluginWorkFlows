using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
public    class Temp_Resouce_Plugin : IPlugin
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

                if (context.MessageName.ToLower() == "update")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        if (entity.LogicalName == "bookableresource")
                        {

                            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='ap360_woproductreturnlog'>
    <attribute name='ap360_woproductreturnlogid' />
    <attribute name='ap360_name' />
    <attribute name='createdon' />
    <attribute name='ap360_returneddate' />
    <attribute name='ap360_quantityreturned' />
    <order attribute='ap360_name' descending='false' />
  </entity>
</fetch>");

                      

                            ///int count = 0;
                            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
                       
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