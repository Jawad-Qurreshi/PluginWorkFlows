using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class PickWOSTAndStartBooking : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();
            //  throw new InvalidPluginExecutionException("CreatePurchaseOrderforPreferredSupplier");

            try
            {

                ITracingService tracingService =
                  (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                IPluginExecutionContext context = (IPluginExecutionContext)
                    serviceProvider.GetService(typeof(IPluginExecutionContext));

                IOrganizationServiceFactory factory =
                     (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = factory.CreateOrganizationService(context.UserId);


                string selectedWOSTGuid = (string)context.InputParameters["WOSTGuid"];
                //  selectedWOSTGuid.
                // tracingService.Trace("WOSt Guid" + selectedWOSTGuid);
                if (selectedWOSTGuid == null)
                {
                    throw new InvalidPluginExecutionException("WOST is nulll");

                }
                string bookingType = (string)context.InputParameters["BookingType"];

                

                Entity reterivedWOSTEntity = service.Retrieve("msdyn_workorderservicetask", new Guid(selectedWOSTGuid), new Microsoft.Xrm.Sdk.Query.ColumnSet(true));

                if (reterivedWOSTEntity != null)
                {

                    tracingService.Trace("before createWostBooking");
                    Guid newlyCreatedBookingGuid = BookableResourceBooking.CreateBookAbleResourceBooking(service, tracingService, reterivedWOSTEntity, context,bookingType,"yes");
                    //  ResourceRequirement.CreateResourceRequirment(service, tracingService, reterivedWOSTEntity);
                    tracingService.Trace("after createWostBooking");


                    Entity updateWOST = new Entity(reterivedWOSTEntity.LogicalName, reterivedWOSTEntity.Id);
                    updateWOST["ap360_workorderservicetaskstatus"] = new OptionSetValue(126300001);//In Progress/ Underway
                    service.Update(updateWOST);

                    context.OutputParameters["NewlyCreatedBookingGuid"] = newlyCreatedBookingGuid.ToString();


                }



            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}