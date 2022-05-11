
using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class VehiclePlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();

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



                if (context.MessageName.ToLower() == "create")
                {
                    tracingService.Trace("Vehicle created in 39 Line");
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        tracingService.Trace("context.InputParameters.Contains('Target') && context.InputParameters['Target'] is Entity");
                        entity = (Entity)context.InputParameters["Target"];

                        if (entity.LogicalName == "ap360_vehicle")
                        {
                            tracingService.Trace("entity.LogicalName == 'ap360_vehicle'");
                            if (entity.Contains("ap360_year") && entity.Contains("ap360_manufacturer") && entity.Contains("ap360_model"))
                            {
                                tracingService.Trace("entity.Contains('ap360_workorderservicetaskid')");
                                int year = entity.GetAttributeValue<int>("ap360_year");
                                string manufacturer = entity.GetAttributeValue<string>("ap360_manufacturer") ?? null;
                                string model = entity.GetAttributeValue<string>("ap360_model") ?? null;

                                tracingService.Trace("name "+ year + " " + manufacturer + " " + model);
                                Entity vehicle = new Entity(entity.LogicalName, entity.Id);
                                string name = year + " " + manufacturer + " " + model;
                                vehicle["ap360_name"] = year +" "+manufacturer+" "+model;
                               
                               
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