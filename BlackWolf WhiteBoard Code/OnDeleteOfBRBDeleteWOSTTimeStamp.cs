using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class OnDeleteOfBRBDeleteWOSTTimeStamp : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();

            try
            {
                // throw new InvalidPluginExecutionException("WorkOrderProductCalculation");


                ITracingService tracingService =
                  (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                IPluginExecutionContext context = (IPluginExecutionContext)
                    serviceProvider.GetService(typeof(IPluginExecutionContext));

                IOrganizationServiceFactory factory =
                     (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = factory.CreateOrganizationService(context.UserId);







                if (context.MessageName.ToLower() == "delete")
                {

                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference)
                    {
                        tracingService.Trace("Inside Delete step");
                        EntityReference entityref = (EntityReference)context.InputParameters["Target"];
                        if (entityref.LogicalName == "bookableresourcebooking")
                        {
                            tracingService.Trace("delete Step of BookableResouceBooking");

                            Entity brbEntity = service.Retrieve(entityref.LogicalName, entityref.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("bookingstatus"));
                           // throw new InvalidPluginExecutionException(entityref.Id.ToString());
                            WorkOrderServiceTaskTimeStamp.DeleteWOSTTimeStampRelatedBooking(service, brbEntity.Id);

                        }
                    }






                }

            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("Error Occured On OnDeleteOfBRBDeleteWOSTTimeStamp, Make Sure related Time Stamp Deleted. Contact CRM Admin for More Information");
            }
        }
    }
}