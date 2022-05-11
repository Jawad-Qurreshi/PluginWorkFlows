using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class MapBookingSrvTaskCompleteTOWoSrvTask : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();
            //  throw new InvalidPluginExecutionException("MapBookingSrvTaskCompleteTOWoSrvTask");
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
                        if (entity.LogicalName == "ap360_bookingservicetask")
                        {
                            if (context.Stage == 20)
                            {
                                entity["ap360_isbookingtimedivided"] = true;// this is used in GetBookingServiceTaskRelatedBRB function to 
                                //update WOServiceTaskTime stamped which are ignored.
                            }
                            if (context.Stage == 40)
                            {
                                Entity postImage = (Entity)context.PostEntityImages["Image"];


                                EntityReference bookingRef = postImage.GetAttributeValue<EntityReference>("ap360_bookableresourcebooking") ?? null;
                                //EntityReference wostRef = postImage.GetAttributeValue<EntityReference>("ap360_workorderservicetask") ?? null;
                                if (bookingRef != null)
                                {
                                    tracingService.Trace("Image of Booking Service Tasks contains Booking");
                                    Entity reterviedBookingEntity = service.Retrieve(bookingRef.LogicalName, bookingRef.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("bookingstatus", "ap360_workorderservicetask"));
                                    EntityReference wostRefOnBRB = reterviedBookingEntity.GetAttributeValue<EntityReference>("ap360_workorderservicetask") ?? null;
                                    if (entity.Contains("ap360_bookingpercenttimespent"))
                                    {
                                        if (postImage.Contains("ap360_bookableresourcebooking"))
                                        {
                                            tracingService.Trace("ap360_bookableresourcebooking Image Exists");
                                            EntityReference bookingstatus = reterviedBookingEntity.GetAttributeValue<EntityReference>("bookingstatus") != null ? reterviedBookingEntity.GetAttributeValue<EntityReference>("bookingstatus") : null;

                                            if (bookingstatus != null)
                                            {
                                                tracingService.Trace("Booking classification exists");
                                                //if (bookingstatus.Id.ToString().ToLower() == "17e92808-7e59-ea11-a811-000d3a33f3c3".ToLower())//Finished
                                                if (bookingstatus.Id.ToString().ToLower() == "c33410b9-1abe-4631-b4e9-6e4a1113af34".ToLower())//closed
                                                {
                                                    tracingService.Trace("Booking classification is Finished");
                                                    Entity updateBooking = new Entity(bookingRef.LogicalName, bookingRef.Id);
                                                    //  updateBooking["ap360_calculateactualamount"] = true;
                                                    // service.Update(updateBooking);
                                                }
                                            }
                                        }
                                    }

                                    if (entity.Contains("ap360_servicetaskcomplete"))
                                    {
                                        tracingService.Trace("Booking Service Task updated");

                                        tracingService.Trace("WO Service Task % Completed , Updated");


                                        if (postImage.Contains("ap360_workorderservicetask"))
                                        {
                                            tracingService.Trace("ap360_workorderservicetask Image Exists");
                                            EntityReference woSrvTaskRef = postImage.GetAttributeValue<EntityReference>("ap360_workorderservicetask") != null ? postImage.GetAttributeValue<EntityReference>("ap360_workorderservicetask") : null;
                                            // throw new InvalidPluginExecutionException(" WO Status "+wostStatusValue.ToString());
                                            tracingService.Trace("workorderservice task ID");
                                            //throw new InvalidPluginExecutionException(woSrvTaskRef.Id.ToString() + "&&" + wostRefOnBRB.Id.ToString());
                                            if (woSrvTaskRef != null)
                                            {
                                                tracingService.Trace("Image of Booking Service Tasks contains WO Service Task");


                                                Entity updateWOSrvTask = new Entity(woSrvTaskRef.LogicalName, woSrvTaskRef.Id);
                                                //updateWOSrvTask["ap360_followedupbyid"] = new EntityReference("systemuser", context.InitiatingUserId);
                                                //updateWOSrvTask["ap360_followupdescription"] = postImage.GetAttributeValue<string>("ap360_followupdescription");

                                                //int wostStatusValue = 0;
                                                //if (entity.Contains("ap360_woststatus"))

                                                //throw new InvalidPluginExecutionException("WOST STATUS");
                                                //if (postImage.GetAttributeValue<OptionSetValue>("ap360_woststatus") != null)
                                                //{
                                                //    wostStatusValue = postImage.GetAttributeValue<OptionSetValue>("ap360_woststatus").Value;

                                                //    updateWOSrvTask["ap360_workorderservicetaskstatus"] = new OptionSetValue(wostStatusValue);
                                                //}


                                                string wostPercentComplete = null;
                                                if (entity.Contains("ap360_servicetaskcomplete"))
                                                {
                                                    if (postImage.GetAttributeValue<OptionSetValue>("ap360_servicetaskcomplete") != null)
                                                    {
                                                        int servicetaskcompletedPercent = postImage.GetAttributeValue<OptionSetValue>("ap360_servicetaskcomplete").Value;
                                                        tracingService.Trace("selected optionset value " + servicetaskcompletedPercent.ToString());                                                        
                                                        if (servicetaskcompletedPercent == 126300007)// 0%
                                                        {
                                                            updateWOSrvTask["msdyn_percentcomplete"] = Convert.ToDouble(1);
                                                            wostPercentComplete = "0";
                                                        }
                                                        //throw new InvalidPluginExecutionException(servicetaskcompletedPercent.ToString());
                                                        if (servicetaskcompletedPercent == 126300000)// 5%
                                                        {
                                                            updateWOSrvTask["msdyn_percentcomplete"] = Convert.ToDouble(5);
                                                            wostPercentComplete = "5";
                                                        }
                                                        else if (servicetaskcompletedPercent == 126300001)// 10%
                                                        {
                                                            updateWOSrvTask["msdyn_percentcomplete"] = Convert.ToDouble(10);
                                                            wostPercentComplete = "10";
                                                        }
                                                        else if (servicetaskcompletedPercent == 126300002)// 20%
                                                        {
                                                            updateWOSrvTask["msdyn_percentcomplete"] = Convert.ToDouble(20);
                                                            wostPercentComplete = "20";
                                                        }
                                                        else if (servicetaskcompletedPercent == 126300008)// 30%
                                                        {
                                                            updateWOSrvTask["msdyn_percentcomplete"] = Convert.ToDouble(30);
                                                            wostPercentComplete = "30";
                                                        }
                                                        else if (servicetaskcompletedPercent == 126300003)// 40%
                                                        {
                                                            updateWOSrvTask["msdyn_percentcomplete"] = Convert.ToDouble(40);
                                                            wostPercentComplete = "40";
                                                        }
                                                        else if (servicetaskcompletedPercent == 126300004)// 60%
                                                        {
                                                            updateWOSrvTask["msdyn_percentcomplete"] = Convert.ToDouble(60);
                                                            wostPercentComplete = "60";
                                                        }
                                                        else if (servicetaskcompletedPercent == 126300009)// 70%
                                                        {
                                                            updateWOSrvTask["msdyn_percentcomplete"] = Convert.ToDouble(70);
                                                            wostPercentComplete = "70";
                                                        }
                                                        else if (servicetaskcompletedPercent == 126300005)// 80%
                                                        {
                                                            updateWOSrvTask["msdyn_percentcomplete"] = Convert.ToDouble(80);
                                                            wostPercentComplete = "80";
                                                        }
                                                        else if (servicetaskcompletedPercent == 126300010)// 90%
                                                        {
                                                            updateWOSrvTask["msdyn_percentcomplete"] = Convert.ToDouble(90);
                                                            wostPercentComplete = "90";
                                                        }
                                                        else if (servicetaskcompletedPercent == 126300006)//100%
                                                        {
                                                            updateWOSrvTask["msdyn_percentcomplete"] = Convert.ToDouble(100);
                                                            wostPercentComplete = "100";
                                                            if (woSrvTaskRef.Id != wostRefOnBRB.Id)
                                                                updateWOSrvTask["ap360_workorderservicetaskstatus"] = new OptionSetValue(126300010);//completed
                                                        }
                                                    }
                                                }

                                                service.Update(updateWOSrvTask);

                                                tracingService.Trace("WOST is updated");


                                                ProjectTask projectTask = ProjectTask.GetProjectTaskRelatedToWOST(service, tracingService, woSrvTaskRef.Id);

                                                if (projectTask != null)
                                                {
                                                    tracingService.Trace("Project TAsk is not null");
                                                    // throw new InvalidPluginExecutionException("Custom Error");
                                                    //string statusName = WorkOrderServiceTask.getWOSTStatusName(wostStatusValue);
                                                    string statusName = null;
                                                    if (postImage.GetAttributeValue<OptionSetValue>("ap360_servicetaskcomplete") != null)
                                                    {
                                                        int servicetaskcompletedPercent = postImage.GetAttributeValue<OptionSetValue>("ap360_servicetaskcomplete").Value;

                                                        if (servicetaskcompletedPercent == 126300010)
                                                        {
                                                            statusName = "Completed";
                                                        }
                                                    }
                                                    Entity updateWOSTProjectTask = new Entity("msdyn_projecttask", projectTask.guid);
                                                    string preDescriptionField = null;
                                                    preDescriptionField = projectTask.Description;

                                                    string predescriptionWOSTstatus = null;
                                                    string predessciptionWOSTPercentComplete = null;

                                                    if (preDescriptionField != null)
                                                    {
                                                        predessciptionWOSTPercentComplete = preDescriptionField.Substring(preDescriptionField.LastIndexOf(':') + 1);
                                                        int index = preDescriptionField.IndexOf(":");
                                                        predescriptionWOSTstatus = (index > 0 ? preDescriptionField.Substring(0, index) : "");
                                                    }
                                                    //  tracingService.Trace(predescriptionWOSTstatus.ToString() + " -- " + predessciptionWOSTPercentComplete.ToString());
                                                    //  throw new InvalidPluginExecutionException(predescriptionWOSTstatus.ToString() + " -- " + predessciptionWOSTPercentComplete.ToString());
                                                    string postDescriptoinField = null;
                                                    if (statusName != null && statusName != "")
                                                        postDescriptoinField = statusName;
                                                    else
                                                        postDescriptoinField = predescriptionWOSTstatus;

                                                    if (wostPercentComplete != null)
                                                        postDescriptoinField = postDescriptoinField + " : " + wostPercentComplete;
                                                    else
                                                        postDescriptoinField = postDescriptoinField + " : " + predessciptionWOSTPercentComplete;


                                                    updateWOSTProjectTask["msdyn_description"] = postDescriptoinField;

                                                    service.Update(updateWOSTProjectTask);
                                                }

                                                tracingService.Trace("WOST is updated");

                                            }
                                            
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