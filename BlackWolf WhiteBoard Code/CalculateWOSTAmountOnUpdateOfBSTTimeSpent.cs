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
    public class CalculateWOSTAmountOnUpdateOfBSTTimeSpent : IPlugin
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
                        if (entity.LogicalName == "ap360_bookingservicetask")
                        {
                            
                            if (entity.Contains("ap360_bookingpercenttimespent") )

                            {
                                //throw new InvalidPluginExecutionException("I worked");
                                tracingService.Trace("ap360_calculateactualamount updated ");
                                Entity reterivedBST = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("ap360_bookableresourcebooking")); 
                                EntityReference BRBRef = reterivedBST.GetAttributeValue<EntityReference>("ap360_bookableresourcebooking")??null;
                                tracingService.Trace("reterivedCurrentBRB retrieved");
                                if (BRBRef != null) {
                                  Entity reterivedCurrentBRB = service.Retrieve(BRBRef.LogicalName, BRBRef.Id, new ColumnSet(true));

                                  tracingService.Trace("Start of caculation funcitons");
                                  List<BookingServiceTask> lstBookingServiceTask = new List<BookingServiceTask>();
                                  lstBookingServiceTask = BookingServiceTask.GetBookingServiceTaskRelatedBRB(service, tracingService, entity.Id);
                                  WorkOrderServiceTask.caculateWOSTTimeStamps(service, tracingService, lstBookingServiceTask, reterivedCurrentBRB, "adminclosed");
                                  tracingService.Trace("End of caculation functions");
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