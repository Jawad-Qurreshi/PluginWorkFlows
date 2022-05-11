
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code.Model
{
    public class BookingServiceTask
    {
        public Guid BSTGuid { get; set; }
        public string LogicalName { get; set; }
        public decimal PercentTimeSpent { get; set; }
        public EntityReference WorkOrderServiceTask { get; set; }
        public OptionSetValue WOSTStatus { get; set; }
        public double StartingPercentCompleted { get; set;}
        public OptionSetValue EndingTaskPercentCompleted { get; set; }
        public bool IsMasterBST { get; set; }

        public EntityReference Booking { get; set; }
        // public double PercentComplete { get; set; }

        public static List<BookingServiceTask> GetBookingServiceTaskRelatedBRB(IOrganizationService service, ITracingService tracing, Guid brbGuid)
        {
            List<BookingServiceTask> lstBookingServiceTask = new List<BookingServiceTask>();

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='ap360_bookingservicetask'>
                                <attribute name='ap360_bookingservicetaskid' />
                                <attribute name='ap360_bookableresourcebooking' />
                                <attribute name='ap360_workorderservicetask' />
                                <attribute name='ap360_bookingpercenttimespent' />
                                <attribute name='ap360_completed' />
                                <attribute name='ap360_servicetaskcomplete' />
                                <attribute name='ap360_woststatus' />
                                <attribute name='ap360_ismasterbst' />



                                <attribute name='createdon' />
                                <order attribute='createdon' descending='false' />
                                    <filter type='and'>
                                      <condition attribute='ap360_bookableresourcebooking' operator='eq'  value='" + brbGuid + @"' /> 
                                          <filter type='or'>
                                            <filter type='and'>
                                              <condition attribute='ap360_bookingpercenttimespent' operator='not-null' />
                                            
                                             </filter>
                                              <condition attribute='ap360_isbookingtimedivided' operator='eq' value='1' />
                                               <condition attribute='ap360_ismasterbst' operator='eq' value='1' />

                                          </filter>
                                    </filter>
                                  </entity>
                                </fetch>");
             // < condition attribute = 'ap360_bookingpercenttimespent' operator= 'gt' value = '0' />
                    //<filter type='and'>
                    //  <condition attribute='ap360_bookableresourcebooking' operator='eq'  value='" + brbGuid + @"' /> 
                    //   <condition attribute = 'ap360_isbookingtimedivided' operator= 'eq' value = '1' />
                    //</filter>

                    // <condition attribute='ap360_bookingpercenttimespent' operator='not-null' />
                    // <condition attribute='ap360_bookingpercenttimespent' operator='gt' value='0' />
                    //    < condition attribute = 'ap360_bookingpercenttimespent' operator= 'not-null' />  contain data

                    EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            BookingServiceTask bookingServiceTask = null;
            //if (col.Entities.Count > 0)
            //{
            //    tracingservice.Trace("count of WorkOrderServiceTask " + col.Entities.Count.ToString());
            //}
            foreach (Entity entity in col.Entities)
            {
                bookingServiceTask = new BookingServiceTask();
                bookingServiceTask.BSTGuid = entity.Id;
                bookingServiceTask.LogicalName = entity.LogicalName;
                bookingServiceTask.Booking = entity.GetAttributeValue<EntityReference>("ap360_bookableresourcebooking") != null ? entity.GetAttributeValue<EntityReference>("ap360_bookableresourcebooking") : null;
                bookingServiceTask.WorkOrderServiceTask = entity.GetAttributeValue<EntityReference>("ap360_workorderservicetask") != null ? entity.GetAttributeValue<EntityReference>("ap360_workorderservicetask") : null;
                bookingServiceTask.PercentTimeSpent = entity.GetAttributeValue<decimal>("ap360_bookingpercenttimespent"); 
                tracing.Trace("before Ending percent completed ");
                bookingServiceTask.EndingTaskPercentCompleted = entity.GetAttributeValue<OptionSetValue>("ap360_servicetaskcomplete");
                tracing.Trace("before starting percent completed ");
                bookingServiceTask.StartingPercentCompleted = entity.GetAttributeValue<double>("ap360_completed");
                tracing.Trace("after starting and ending.");
                bookingServiceTask.WOSTStatus = entity.GetAttributeValue<OptionSetValue>("ap360_woststatus");
                bookingServiceTask.IsMasterBST = entity.GetAttributeValue<bool>("ap360_ismasterbst");
                lstBookingServiceTask.Add(bookingServiceTask);

            }
            return lstBookingServiceTask;

        }

        public static void UpdateBST(IOrganizationService service, ITracingService tracing, List<BookingServiceTask> lstBookingServiceTask, Guid newlyCreatedBRBGuid)
        {
            tracing.Trace("Inside UpdateBST");
            foreach (BookingServiceTask bst in lstBookingServiceTask)
            {
                Entity entity = new Entity("ap360_bookingservicetask");
                entity.Id = bst.BSTGuid;
                // double bookingPercentTimeSpent = 0;
                // entity["ap360_bookingpercenttimespent"] = bst.PercentTimeSpent;
                // entity["ap360_bookingpercenttimespent"] = bookingPercentTimeSpent;
                // entity["ap360_bookableresourcebooking"] = new EntityReference("bookableresourcebooking", newlyCreatedBRBGuid);
                service.Update(entity);
            }

        }

        public static DataCollection<Entity> GetBookingServiceTaskRelatedToWOST(IOrganizationService service, Entity WOST)
        {
            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                      <entity name='ap360_bookingservicetask'>
                                        <attribute name='ap360_bookingservicetaskid' />
                                        <attribute name='ap360_name' />
                                        <attribute name='ap360_woststatus' />
                                        <attribute name='ap360_workorderservicetask' />
                                        <order attribute='ap360_name' descending='false' />
                                        <filter type='and'>
                                          <condition attribute='ap360_workorderservicetask' operator='eq'  value='" + WOST.Id + @"' />
                                        </filter>
                                      </entity>
                                    </fetch>");




            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            return col.Entities;
        }

        public static DataCollection<Entity> GetBookingServiceTaskRelatedToWOSTOnBaseOfWOSTRef(IOrganizationService service, EntityReference WOSTRef)
        {
            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='ap360_bookingservicetask'>
                                    <attribute name='ap360_bookingservicetaskid' />
                                    <attribute name='ap360_name' />
                                    <attribute name='createdon' />
                                    <order attribute='ap360_name' descending='false' />
                                    <filter type='and'>
                                      <condition attribute='ap360_workorderservicetask' operator='eq'  value='" + WOSTRef.Id + @"' />
                                    </filter>
                                    <link-entity name='bookableresourcebooking' from='bookableresourcebookingid' to='ap360_bookableresourcebooking' link-type='inner' alias='ad'>
                                      <filter type='and'>
                                        <condition attribute='bookingstatus' operator='in'>
                                          <value uiname='In Progress' uitype='bookingstatus'>{53F39908-D08A-4D9C-90E8-907FD7BEC07D}</value>
                                          <value uiname='Scheduled' uitype='bookingstatus'>{F16D80D1-FD07-4237-8B69-187A11EB75F9}</value>
                                        </condition>
                                      </filter>
                                    </link-entity>
                                  </entity>
                                </fetch>");

            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            return col.Entities;
        }

        public static int getBSTWOSTPercentComplete(int statusOption)
        {
            int serviceTaskPercentComplete = 0;
            if (statusOption == 126300007   )
            {
                serviceTaskPercentComplete = 1;
            }
            else if (statusOption == 126300000)
            {
                serviceTaskPercentComplete = 5;
            }
            else if (statusOption == 126300001)
            {
                serviceTaskPercentComplete = 10;
            }
            else if (statusOption == 126300002)
            {
                serviceTaskPercentComplete = 20;
            }
            else if (statusOption == 126300008)
            {
                serviceTaskPercentComplete = 30;
            }
            else if (statusOption == 126300003)
            {
                serviceTaskPercentComplete = 40;
            }
            else if (statusOption == 126300004)
            {
                serviceTaskPercentComplete = 60;
            }
            else if (statusOption == 126300009)
            {
                serviceTaskPercentComplete = 70;
            }
            else if (statusOption == 126300005)
            {
                serviceTaskPercentComplete = 80;
            }
            else if (statusOption == 126300010)
            {
                serviceTaskPercentComplete = 90;
            }

            else if (statusOption == 126300006)
            {
                serviceTaskPercentComplete = 100;
            }
           
            return serviceTaskPercentComplete;
        }

    }
}
