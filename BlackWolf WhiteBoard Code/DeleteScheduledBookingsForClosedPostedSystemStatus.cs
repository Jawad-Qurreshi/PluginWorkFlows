using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class DeleteScheduledBookingsForClosedPostedSystemStatus : IPlugin
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
                        if (context.Depth == 1)
                        {
                            entity = (Entity)context.InputParameters["Target"];

                            if (entity.LogicalName.ToLower() == "msdyn_workorder")
                            {
                                tracingService.Trace("Inside DeleteScheduledBookingsForClosedPostedSystemStatus");

                                if (entity.Contains("msdyn_systemstatus"))
                                {

                                    int systemStatus = entity.GetAttributeValue<OptionSetValue>("msdyn_systemstatus").Value;

                                    if (systemStatus == 690970004)//Closed - Posted
                                    {



                                        List<BookableResourceBooking> lstBookableResourceBookings = BookableResourceBooking.getSecheduledBookingforWorkOrder(service, tracingService, entity.Id);
                                        //(service, tracingService, entity.Id);

                                        foreach (BookableResourceBooking booking in lstBookableResourceBookings)
                                        {
                                            throw new InvalidPluginExecutionException("Work Order can't be to Closed Posted with Scheduled Booking(s) ");
                                            //  service.Delete("bookableresourcebooking", booking.BRBGuid);

                                        }

                                        //EntityCollection woProductCollection = WorkOrderProduct.getUnUsedWorkOrderProductsRelatedToWorkOrder(service, tracingService, entity.Id);
                                        //if (woProductCollection.Entities.Count > 0)
                                        //{
                                        //    throw new InvalidPluginExecutionException("Work Order can't be to Closed Posted with UnUsed WorkOrder Product(s)");
                                        //}

                                        EntityCollection pendingPurchaseOrderProducts = PurchaseOrderProduct.getPendingPurchaseOrderProductRelatedToWorkOrder(service, tracingService, entity.Id);
                                        if(pendingPurchaseOrderProducts.Entities.Count>0)
                                        {
                                            throw new InvalidPluginExecutionException("Work Order can't be to Closed Posted with Pending Purhase Order Product(s)");


                                        }
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



