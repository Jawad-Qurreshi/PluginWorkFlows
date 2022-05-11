
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code.Model
{
    public class BookingJournal
    {

        public Guid guid { get; set; }

        public static void UpdatebookingJournal(IOrganizationService service, ITracingService tracing, Guid bookingGuid, double billableDuration, DateTime startTime, DateTime endTime)
        {

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='msdyn_bookingjournal'>
                                    <attribute name='createdon' />
                                    <attribute name='msdyn_journaltype' />
                                    <attribute name='msdyn_starttime' />
                                    <attribute name='msdyn_endtime' />
                                    <attribute name='msdyn_duration' />
                                    <attribute name='msdyn_bookingjournalid' />
                                    <order attribute='msdyn_starttime' descending='false' />
                                    <filter type='and'>
                                      <condition attribute='msdyn_booking' operator= 'eq' value = '" + bookingGuid + @"' />
                                    </filter>
                                  </entity>
                                </fetch>");



            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));

            if (col.Entities.Count > 0)
            {
                Entity updateBookingJournal = new Entity(col.Entities[0].LogicalName);
                updateBookingJournal.Id = col.Entities[0].Id;

                updateBookingJournal["msdyn_name"] = "Time Spent "+ billableDuration.ToString();
                updateBookingJournal["msdyn_starttime"] = startTime;
                updateBookingJournal["msdyn_endtime"] = endTime;

                updateBookingJournal["msdyn_duration"] = billableDuration;
                service.Update(updateBookingJournal);

            }

        }
        public static EntityCollection RetrievebookingJournal(IOrganizationService service, ITracingService tracing, Guid bookingGuid)
        {

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='msdyn_bookingjournal'>
                                    <attribute name='createdon' />
                                    <attribute name='msdyn_journaltype' />
                                    <attribute name='msdyn_starttime' />
                                    <attribute name='msdyn_endtime' />
                                    <attribute name='msdyn_duration' />
                                    <attribute name='msdyn_bookingjournalid' />
                                    <order attribute='msdyn_starttime' descending='false' />
                                    <filter type='and'>
                                      <condition attribute='msdyn_booking' operator= 'eq' value = '" + bookingGuid + @"' />
                                    </filter>
                                  </entity>
                                </fetch>");



            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            return col;
         
        }

    }
}
