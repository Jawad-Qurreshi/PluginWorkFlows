using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace BlackWolf_WhiteBoard_Code
{
    public class PreventWOSTTimeStampDeletion : IPlugin
    {



        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();
            // throw new InvalidPluginExecutionException("QuoteProductCalculation");



            try
            {



                ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));



                IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));



                IOrganizationServiceFactory factory =
                (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = factory.CreateOrganizationService(context.UserId);




                EntityReference entity = null;
                if (context.MessageName.ToLower() == "delete")
                {
                    if (context.Depth == 2) return;
                    // Prevent WOSTTS deletion from delete button(Context.depth=1) but for Booking adjustement\
                    //through Booking Service Task(context.depth =2) , corresponding WOSTTS should delete

                    tracingService.Trace("Depth" + context.Depth.ToString() + " is exist");
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference)
                    {



                        entity = (EntityReference)context.InputParameters["Target"];
                        if (context.MessageName.ToLower() == "delete")
                        {
                            tracingService.Trace("WOST TimeStamps cannot be Deleted");
                            throw new InvalidPluginExecutionException("WOST TimeStamps cannot be Deleted.");
                        }
                        return;
                    }





                }
            }



            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }
    }
}