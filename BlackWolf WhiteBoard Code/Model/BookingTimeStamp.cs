

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code.Model
{
    public class BookingTimeStamp
    {

        public Guid guid { get; set; }
        public DateTime TimeStampTime { get; set; }
        public int TimeStampSystemStatus { get; set; }
        public EntityReference TimeStampBookingStatus { get; set; }

        public EntityReference booking { get; set; }


        public static BookingTimeStamp getFinishedbookingTimeStamp(IOrganizationService service, Guid bookingGuid)
        {

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='msdyn_bookingtimestamp'>
                                    <attribute name='createdon' />
                                    <attribute name='msdyn_timestamptime' />
                                    <attribute name='msdyn_timestampsource' />
                                    <attribute name='msdyn_systemstatus' />
                                    <attribute name='msdyn_bookingstatus' />

                                    <attribute name='msdyn_booking' />
                                    <attribute name='msdyn_bookingtimestampid' />
                                    <order attribute='msdyn_timestamptime' descending='false' />
                                    <filter type='and'>
                                      <condition attribute='msdyn_systemstatus' operator='eq' value='690970001' />
                                      <condition attribute='msdyn_booking' operator= 'eq' value = '" + bookingGuid + @"' />
                                    </filter>
                                  </entity>
                                </fetch>");

            //690,970,003 In progress
            //690,970,004 closed
            //690,970,005 finsished
            //690,970,001 taveling

            //<condition attribute='msdyn_name' operator='eq' value='" + ProdWorkOrderName + @"' /> 

            ///int count = 0;
            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            //  BookableResourceBooking bookableResourceBooking = null;
            BookingTimeStamp returnbookingTimeStamp = new BookingTimeStamp();
            foreach (var bookingTimeStamp in col.Entities)
            {
                returnbookingTimeStamp.guid = bookingTimeStamp.Id;
                returnbookingTimeStamp.TimeStampTime = bookingTimeStamp.GetAttributeValue<DateTime>("msdyn_timestamptime");

            }

            return returnbookingTimeStamp;
        }

        public static BookingTimeStamp getInProgressbookingTimeStamp(IOrganizationService service, Guid bookingGuid)
        {

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='msdyn_bookingtimestamp'>
                                    <attribute name='createdon' />
                                    <attribute name='msdyn_timestamptime' />
                                    <attribute name='msdyn_timestampsource' />
                                    <attribute name='msdyn_systemstatus' />
                                    <attribute name='msdyn_booking' />
                                    <attribute name='msdyn_bookingtimestampid' />
                                    <order attribute='msdyn_timestamptime' descending='false' />
                                    <filter type='and'>
                                      <condition attribute='msdyn_systemstatus' operator='eq' value='690970003' />
                                      <condition attribute='msdyn_booking' operator= 'eq' value = '" + bookingGuid + @"' />
                                    </filter>
                                  </entity>
                                </fetch>");

            //690,970,003 In progress
            //690,970,004 closed
            //690,970,005 finsished

            //<condition attribute='msdyn_name' operator='eq' value='" + ProdWorkOrderName + @"' /> 

            ///int count = 0;
            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            //  BookableResourceBooking bookableResourceBooking = null;
            BookingTimeStamp returnbookingTimeStamp = new BookingTimeStamp();
            foreach (var bookingTimeStamp in col.Entities)
            {
                returnbookingTimeStamp.guid = bookingTimeStamp.Id;
                returnbookingTimeStamp.TimeStampTime = bookingTimeStamp.GetAttributeValue<DateTime>("msdyn_timestamptime");

            }

            return returnbookingTimeStamp;
        }

        public static List<BookingTimeStamp> getInProgressAndFinsihedAndClosedbookingTimeStamp(IOrganizationService service, ITracingService tracing, Guid bookingGuid)
        {

            List<BookingTimeStamp> lstBookingTimeStamps = new List<BookingTimeStamp>();
            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='msdyn_bookingtimestamp'>
                                    <attribute name='createdon' />
                                    <attribute name='msdyn_timestamptime' />
                                    <attribute name='msdyn_timestampsource' />
                                    <attribute name='msdyn_systemstatus' />
                                    <attribute name='msdyn_bookingstatus' />

                                    <attribute name='msdyn_booking' />
                                    <attribute name='msdyn_bookingtimestampid' />
                                    <order attribute='msdyn_timestamptime' descending='false' />
                                    <filter type='and'>
                                     <condition attribute='msdyn_systemstatus' operator='in'>
                                         <value>690970003</value>
                                         <value>690970001</value>
                                         <value>690970004</value>
                                         <value>690970005</value>

                                     </condition>                         
                                    <condition attribute='msdyn_booking' operator= 'eq' value = '" + bookingGuid + @"' />
                                    </filter>
                                  </entity>
                                </fetch>");

            //msdyn_systemstatus
            //690,970,003 In progress
            //690,970,004 closed
            //690970001  Traveling
            //690,970,005 finsished 

            //Replaced Finsihed with Traveling in query, because technican were getting error message on Finish of Booking
            //You have canceled this Bookable Resource Booking; canceling a Bookable Resource Booking does not cancel the related Work Order.
            //The related Work Order has status: Open - Completed.


            //<condition attribute='msdyn_name' operator='eq' value='" + ProdWorkOrderName + @"' /> 

            ///int count = 0;
            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            //  BookableResourceBooking bookableResourceBooking = null;
            BookingTimeStamp bookingTimeStamp;
            foreach (var reterivedbookingTimeStamp in col.Entities)
            {
                bookingTimeStamp = new BookingTimeStamp();
                bookingTimeStamp.guid = reterivedbookingTimeStamp.Id;
                bookingTimeStamp.TimeStampTime = reterivedbookingTimeStamp.GetAttributeValue<DateTime>("msdyn_timestamptime");
                bookingTimeStamp.TimeStampSystemStatus = reterivedbookingTimeStamp.GetAttributeValue<OptionSetValue>("msdyn_systemstatus").Value;
                bookingTimeStamp.TimeStampBookingStatus = reterivedbookingTimeStamp.GetAttributeValue<EntityReference>("msdyn_bookingstatus");

                lstBookingTimeStamps.Add(bookingTimeStamp);
            }

            return lstBookingTimeStamps;
        }



    }
}
