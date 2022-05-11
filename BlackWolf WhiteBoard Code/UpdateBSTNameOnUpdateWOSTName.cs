using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace BlackWolf_WhiteBoard_Code
{



    public class UpdateBSTNameOnUpdateWOSTName : IPlugin
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
                            tracingService.Trace(context.Depth.ToString());
                            entity = (Entity)context.InputParameters["Target"];
                            
                            if (entity.Contains("msdyn_name"))
                            {
                                //tracingService.Trace("Entity Name " + entity.LogicalName);
                                // Entity RetrievedWOST = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("ap360_customnotes"));
                                // String customNote = RetrievedWOST.GetAttributeValue<string>("ap360_customnotes");



                                String updatedWOSTName = entity.GetAttributeValue<string>("msdyn_name");
                                if (updatedWOSTName != null)
                                {
                                    DataCollection<Entity> lstbookingsServiceTask = BookingServiceTask.GetBookingServiceTaskRelatedToWOST(service, entity);
                                    foreach (Entity bookingsServiceTask in lstbookingsServiceTask)
                                    {
                                        Entity updatebookingsServiceTask = new Entity(bookingsServiceTask.LogicalName, bookingsServiceTask.Id);
                                        updatebookingsServiceTask["ap360_name"] = updatedWOSTName;
                                        service.Update(updatebookingsServiceTask);

                                    }
                                }
                            }
                            if (entity.Contains("ap360_workorderservicetaskstatus"))
                            {
                                tracingService.Trace("ap360_workorderservicetaskstatus");
                                Entity preImage = (Entity)context.PreEntityImages["Image"];
                                string wostName = preImage.GetAttributeValue<string>("msdyn_name");
                                // int WOSTPercentComplete = Convert.ToInt32(entity["msdyn_percentcomplete"]);
                                int PreWOSTPercentComplete = Convert.ToInt32(preImage["msdyn_percentcomplete"]);
                                EntityReference workOrderRef = preImage.GetAttributeValue<EntityReference>("msdyn_workorder") != null ? preImage.GetAttributeValue<EntityReference>("msdyn_workorder") : null;
                                int WOSTStatus = entity.GetAttributeValue<OptionSetValue>("ap360_workorderservicetaskstatus").Value;
                                tracingService.Trace("exception");
                                
                             
                                if (WOSTStatus == 126300009 && workOrderRef != null)//Incomplete
                                {
                                    tracingService.Trace("Work Order ST status is incomplete and WO Ref != null");

                                    List<BookableResourceBooking> lstBookableResourceBooking = new List<BookableResourceBooking>();
                                    lstBookableResourceBooking = BookableResourceBooking.getWorkOrdersBooking(service, tracingService, workOrderRef.Id);
                                    tracingService.Trace("booking count " + lstBookableResourceBooking.Count.ToString());

                                    //throw new InvalidPluginExecutionException("hello in incomplete") ;
                                    foreach (BookableResourceBooking bookableResourceBooking in lstBookableResourceBooking)
                                    {
                                        Entity newBookingSrvTask = new Entity("ap360_bookingservicetask");
                                        newBookingSrvTask["ap360_name"] = wostName;
                                        newBookingSrvTask["ap360_workorderservicetask"] = new EntityReference(entity.LogicalName, entity.Id);
                                        newBookingSrvTask["ap360_bookableresourcebooking"] = new EntityReference("bookableresourcebooking", bookableResourceBooking.BRBGuid);
                                        service.Create(newBookingSrvTask);
                                    }

                                }
                                if (WOSTStatus == 126300010)//Completed
                                {
                                    DataCollection<Entity> lstbookingsServiceTask = BookingServiceTask.GetBookingServiceTaskRelatedToWOST(service, entity);
                                    foreach (Entity bookingsServiceTask in lstbookingsServiceTask)
                                    {
                                        service.Delete(bookingsServiceTask.LogicalName, bookingsServiceTask.Id);
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
