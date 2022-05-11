
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
    public class UpdateTimeSpentOnBookingAndBookingAmount : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();
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
                        if (entity.LogicalName == "msdyn_bookingtimestamp")
                        {
                            tracingService.Trace("UpdateTimeSpentOnBookingAndBookingAmount msdyn_bookingtimestamp");
                            if (entity.Contains("msdyn_timestamptime"))
                            {
                                Entity preImage = (Entity)context.PreEntityImages["Image"];
                                if (preImage != null)
                                {
                                    tracingService.Trace("Pre Image not null");
                                    EntityReference bookingRef = preImage.GetAttributeValue<EntityReference>("msdyn_booking") != null ? preImage.GetAttributeValue<EntityReference>("msdyn_booking") : null;

                                    if (bookingRef != null)
                                    {
                                        tracingService.Trace("booking ref not null");

                                        Entity updateBookableResourceBooking = new Entity(bookingRef.LogicalName, bookingRef.Id);

                                        updateBookableResourceBooking["ap360_bookingverified"] = true;
                                        updateBookableResourceBooking["ap360_calculateactualamount"] = true;

                                        service.Update(updateBookableResourceBooking);


                                       // This is important same as calculateActualAmount to No
                                        Entity againupdateBookableResourceBooking = new Entity(bookingRef.LogicalName, bookingRef.Id);
                                        againupdateBookableResourceBooking["ap360_bookingverified"] = false;
                                        service.Update(againupdateBookableResourceBooking);


                                    }
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
