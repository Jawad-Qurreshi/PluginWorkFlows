using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class ManageBSTOfBookingBasedOnWOST : IPlugin
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
                                tracingService.Trace("Entity Name " + entity.LogicalName);
                                // Entity RetrievedWOST = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("ap360_customnotes"));
                                // String customNote = RetrievedWOST.GetAttributeValue<string>("ap360_customnotes");

                                String updatedWOSTName = entity.GetAttributeValue<string>("msdyn_name");
                                if (updatedWOSTName != null)
                                {
                                    DataCollection<Entity> lstbookingsServiceTask = BookingServiceTask.GetBookingServiceTaskRelatedToWOST(service, entity);
                                    foreach (Entity bookingsServiceTask in lstbookingsServiceTask)
                                    {
                                        //throw new InvalidPluginExecutionException(lstbookingsServiceTask.Count.ToString());
                                        Entity updatebookingsServiceTask = new Entity(bookingsServiceTask.LogicalName, bookingsServiceTask.Id);
                                        updatebookingsServiceTask["ap360_name"] = updatedWOSTName;
                                        service.Update(updatebookingsServiceTask);

                                    }
                                }
                            }
                            if (entity.Contains("ap360_workorderservicetaskstatus"))
                            {
                                tracingService.Trace("update of WOST");
                                Entity preImage = (Entity)context.PreEntityImages["Image"];
                                Entity postImage = (Entity)context.PostEntityImages["Image"];
                                tracingService.Trace("after iamge  of WOST");


                                string wostName = preImage.GetAttributeValue<string>("msdyn_name");
                                // int WOSTPercentComplete = Convert.ToInt32(entity["msdyn_percentcomplete"]);
                                int PreWOSTPercentComplete = Convert.ToInt32(preImage["msdyn_percentcomplete"]);
                                EntityReference workOrderRef = preImage.GetAttributeValue<EntityReference>("msdyn_workorder") != null ? preImage.GetAttributeValue<EntityReference>("msdyn_workorder") : null;
                                int WOSTStatus = 0;
                                if (entity.GetAttributeValue<OptionSetValue>("ap360_workorderservicetaskstatus") != null)
                                {
                                    WOSTStatus = entity.GetAttributeValue<OptionSetValue>("ap360_workorderservicetaskstatus").Value;
                                }
                                double PostWOSTPercentComplete = Convert.ToDouble(postImage["msdyn_percentcomplete"]);
                                // int PostWOSTStatus = postImage.GetAttributeValue<OptionSetValue>("ap360_workorderservicetaskstatus").Value;

                                tracingService.Trace("exception");

                                if (WOSTStatus != 0 && WOSTStatus == 126300009 && workOrderRef != null)//Incomplete
                                {
                                    tracingService.Trace("Work Order ST status is incomplete and WO Ref != null");

                                    List<BookableResourceBooking> lstBookableResourceBooking = new List<BookableResourceBooking>();
                                    //  lstBookableResourceBooking = BookableResourceBooking.getWorkOrdersBooking(service, tracingService, workOrderRef.Id);
                                    lstBookableResourceBooking = BookableResourceBooking.getWorkOrdersScheduledBooking(service, tracingService, workOrderRef.Id);

                                    tracingService.Trace("booking count " + lstBookableResourceBooking.Count.ToString());

                                    foreach (BookableResourceBooking bookableResourceBooking in lstBookableResourceBooking)
                                    {
                                        Entity newBookingSrvTask = new Entity("ap360_bookingservicetask");
                                        newBookingSrvTask["ap360_name"] = wostName;
                                        newBookingSrvTask["ap360_workorderservicetask"] = new EntityReference(entity.LogicalName, entity.Id);
                                        newBookingSrvTask["ap360_bookableresourcebooking"] = new EntityReference("bookableresourcebooking", bookableResourceBooking.BRBGuid);
                                        //newBookingSrvTask["ap360_woststatus"] = WOSTStatus;
                                        //newBookingSrvTask["ap360_completed"] = PostWOSTPercentComplete;
                                        //throw new InvalidPluginExecutionException("hello") ;
                                        service.Create(newBookingSrvTask);
                                    }

                                }
                                //This is disabled because Maxi not abble to Compelete the task he is getting error
                                //The record can't be deleted because its associated with other record
                                //if (WOSTStatus == 126300010)//Completed
                                //{
                                //    DataCollection<Entity> lstbookingsServiceTask = BookingServiceTask.GetBookingServiceTaskRelatedToWOST(service, entity);
                                //    foreach (Entity bookingsServiceTask in lstbookingsServiceTask)
                                //    {
                                //        service.Delete(bookingsServiceTask.LogicalName, bookingsServiceTask.Id);
                                //    }
                                //}
                            }
                        }
                    }
                }
                if (context.MessageName.ToLower() == "delete")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference)
                    {
                        EntityReference TargetRef = (EntityReference)context.InputParameters["Target"];
                        if (TargetRef.LogicalName.ToLower() == "msdyn_workorderservicetask")
                        {
                            DataCollection<Entity> lstbookingsServiceTask = BookingServiceTask.GetBookingServiceTaskRelatedToWOSTOnBaseOfWOSTRef(service, TargetRef);
                            foreach (Entity bookingsServiceTask in lstbookingsServiceTask)
                            {
                                //service.Delete(bookingsServiceTask.LogicalName, bookingsServiceTask.Id);
                                tracingService.Trace("inside for each");
                            }
                            //throw new InvalidPluginExecutionException("test it is "+lstbookingsServiceTask.Count().ToString());

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
