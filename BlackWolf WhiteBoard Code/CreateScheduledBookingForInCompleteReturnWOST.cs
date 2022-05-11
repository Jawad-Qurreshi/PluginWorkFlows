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
   public class CreateScheduledBookingForInCompleteReturnWOST : IPlugin
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



                if (context.MessageName.ToLower() == "update")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        entity = (Entity)context.InputParameters["Target"];

                        if (entity.LogicalName == "msdyn_workorderservicetask")
                        {
                            tracingService.Trace("CreateScheduledBookingForInCompleteReturnWOST");

                            if (entity.Contains("ap360_journeymanactualduration"))
                            {

                                Entity postImage = (Entity)context.PostEntityImages["Image"];


                                Money ap360_originalestimatedamount = new Money();
                                Money ap360_revisedestimatedamount = new Money();
                                Money EstimatedLaborAmount = new Money();


                              
                                int WOSTpercentcomplete = Convert.ToInt32(postImage["msdyn_percentcomplete"]);

                                int postJourneyManActualDuration = postImage.GetAttributeValue<int>("ap360_journeymanactualduration");



                                tracingService.Trace("Out side if : WOSTpercentcomplete: "+WOSTpercentcomplete.ToString());

                                if (WOSTpercentcomplete < 100)
                                {
                                    if (entity.Contains("ap360_journeymanactualduration"))
                                    {
                                        if (postImage.Contains("ap360_lastbookingworkedonid"))
                                        {
                                            tracingService.Trace("Last Booking Worked On Exist ");
                                            int estimatedDuration = postImage.GetAttributeValue<int>("msdyn_estimatedduration") > 0 ? postImage.GetAttributeValue<int>("msdyn_estimatedduration") : postImage.GetAttributeValue<int>("ap360_revisedestimatedduration");
                                            if (estimatedDuration > postJourneyManActualDuration)
                                            {
                                                tracingService.Trace("estimatedDuration > postJourneyManActualDuration"+ estimatedDuration.ToString()+" > "+ postJourneyManActualDuration.ToString());

                                                EntityReference bookableResouceBookingRef = postImage.GetAttributeValue<EntityReference>("ap360_lastbookingworkedonid") ?? null;
                                                if (bookableResouceBookingRef != null)
                                                {
                                                    tracingService.Trace(bookableResouceBookingRef.Name + " ref");
                                                    OptionSetValue wostStatus = postImage.GetAttributeValue<OptionSetValue>("ap360_workorderservicetaskstatus");
                                                    if (wostStatus != null)
                                                    {
                                                        tracingService.Trace("wost status is not null");
                                                        if (wostStatus.Value == 126300008)//Incompete Return
                                                        {
                                                            Entity reterivedBRB = service.Retrieve(bookableResouceBookingRef.LogicalName, bookableResouceBookingRef.Id, new ColumnSet(true));
                                                            if (reterivedBRB != null)
                                                            {
                                                                tracingService.Trace("woststatus 126300008(incomplete return) CreateNewBRBonODone");
                                                                // throw new InvalidPluginExecutionException("updated Error");


                                                                reterivedBRB.EntityState = null;
                                                                //remove the PrimaryId of the record otherwise will show you error
                                                                reterivedBRB.Attributes.Remove(reterivedBRB.LogicalName + "id");
                                                                reterivedBRB.Attributes["starttime"] = DateTime.Now;
                                                                reterivedBRB.Attributes["msdyn_estimatedarrivaltime"] = DateTime.Now;

                                                              // reterivedBRB.Attributes["msdyn_actualarrivaltime"] = DateTime.Now.AddMinutes(1);

                                                                
                                                                reterivedBRB.Attributes["endtime"] = DateTime.Now.AddMinutes(30);
                                                                reterivedBRB.Attributes["bookingstatus"] = new EntityReference("ap360_bookingclassification", new Guid("f16d80d1-fd07-4237-8b69-187a11eb75f9"));//scheduled

                                                                reterivedBRB.Attributes.Remove("ap360_bookingclassification");
                                                                reterivedBRB.Attributes.Remove("ap360_finishtime");
                                                                reterivedBRB.Attributes.Remove("ap360_prewosthealth");
                                                                reterivedBRB.Attributes.Remove("ap360_postwosthealth");
                                                                reterivedBRB.Attributes.Remove("msdyn_totalcost");
                                                                reterivedBRB.Attributes.Remove("msdyn_totaldurationinprogress");
                                                                reterivedBRB.Attributes.Remove("msdyn_totalbillableduration");
                                                                reterivedBRB.Attributes.Remove("msdyn_totalbreakduration");
                                                                reterivedBRB.Attributes.Remove("ap360_bookingactualamounts");
                                                                reterivedBRB.Attributes.Remove("ap360_workorderactualamount");
                                                                //reterivedBRB.Attributes.Remove("msdyn_lineorder");
                                                                //reterivedBRB.Attributes["ap360_partialreceivedquantity"] = notReceivedPOPQuantity;
                                                                //reterivedBRB.Attributes["msdyn_quantity"] = notReceivedPOPQuantity;
                                                                //reterivedBRB.Attributes["msdyn_itemstatus"] = new OptionSetValue(690970000);//pending
                                                                //reterivedBRB.Attributes["ap360_receivedon"] = null;
                                                                //reterivedBRB.Attributes["ap360_receivedbyid"] = null;
                                                                reterivedBRB.Id = Guid.NewGuid();
                                                                service.Create(reterivedBRB);

                                                                //BookableResourceBooking.CreateNewBRBonODone(service, tracingService, reterivedBRB);
                                                            }
                                                        }
                                                    }

                                                }


                                            }

                                        }

                                    }
                                }


                            }
                            //if (entity.Contains("ap360_workorderservicetaskstatus")) { 

                            //}
                        }
                    }

                }





            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //private static decimal GetWOSTEstimatedDuration(Entity WOSTEntityRef)
        //{

        //    var WOSTrevisedestimatedAmount = EntityBase.GetMoneyAttributeValue(WOSTEntity, "ap360_revisedestimatedamount");
        //    var WOSToriginalEstimatedAmount = EntityBase.GetMoneyAttributeValue(WOSTEntity, "ap360_originalestimatedamount");//orginal Estimated Duration
        //    decimal WOSTEstiamtedAmount = 0;
        //    if (WOSTrevisedestimatedAmount > 0)
        //    {
        //        WOSTEstiamtedAmount = WOSTrevisedestimatedAmount;
        //    }
        //    else
        //    {
        //        WOSTEstiamtedAmount = WOSToriginalEstimatedAmount;
        //    }
        //    return WOSTEstiamtedAmount;
        //}


    }
}