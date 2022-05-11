using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code.Model
{
    public class WorkOrderServiceTaskHealthLog
    {

        public static void CreateWOSTHealthLog(IOrganizationService service, ITracingService tracingService, Entity entity, Money ap360_predictedspend,
            Money EstimatedLaborAmount, EntityReference lastbookingworkedon, EntityReference recentlyWorkedResource, EntityReference workOrderRef,
            int WOSTpercentcomplete, decimal postwosthealth, EntityReference WOSTServiceRole, Money hourlyRate, decimal timeSpentOnLastBooking,
            decimal timeSpentOnLastBookingAsJourneyMan, decimal durationpostwosthealth, decimal DurationPredictedspend, int EstimatedDuration)
        {


            Entity createWOSTHealthLog = new Entity("ap360_workorderservicetaskhealthlog");
            createWOSTHealthLog["ap360_prewosthealth"] = postwosthealth;
            if (EstimatedLaborAmount!=null && EstimatedLaborAmount.Value > 0)
            {
                createWOSTHealthLog["ap360_postwosthealth"] = ap360_predictedspend.Value / EstimatedLaborAmount.Value;
            }
            createWOSTHealthLog["ap360_workorderservicetaskid"] = new EntityReference(entity.LogicalName, entity.Id);
            createWOSTHealthLog["ap360_workorderid"] = workOrderRef;
            createWOSTHealthLog["ap360_bookableresourcebookingid"] = lastbookingworkedon;
            createWOSTHealthLog["ap360_resouceid"] = recentlyWorkedResource;

            //throw new InvalidPluginExecutionException(timeSpentOnLastBooking.ToString() + "   " + timeSpentOnLastBookingAsJourneyMan.ToString());

            if (timeSpentOnLastBooking > 0)
            {
                createWOSTHealthLog["ap360_timespentonbooking"] = Convert.ToInt32(timeSpentOnLastBooking);
            }
            if (timeSpentOnLastBookingAsJourneyMan > 0)
            {
                createWOSTHealthLog["ap360_journeymantimespentonbooking"] = Convert.ToInt32(timeSpentOnLastBookingAsJourneyMan);
            }
            createWOSTHealthLog["ap360_durationprewosthealth"] = durationpostwosthealth;
            if (EstimatedDuration > 0)
            {
                createWOSTHealthLog["ap360_durationpostwosthealth"] = DurationPredictedspend / EstimatedDuration;
            }// createWOSTHealthLog["ap360_wostpercentcomplete"] = WOSTpercentcomplete;
            createWOSTHealthLog["ap360_wostpercentcomplete"] = WOSTpercentcomplete;


            service.Create(createWOSTHealthLog);
        }

        public static void DeleteOldWOSTHealthLog(IOrganizationService service, ITracingService tracingService, EntityReference BookingRef, EntityReference WOSTRef) {
            tracingService.Trace("inside DeleteOldWOSTHealthLog");
            List<Entity> lstEntities = new List<Entity>();

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='ap360_workorderservicetaskhealthlog'>
                                    <attribute name='ap360_workorderservicetaskhealthlogid' />
                                    <attribute name='ap360_name' />
                                    <attribute name='createdon' />
                                    <order attribute='createdon' descending='true' />
                                    <filter type='and'>
                                      <condition attribute='ap360_bookableresourcebookingid' operator='eq' value='" + BookingRef.Id + @"'/>
                                      <condition attribute='ap360_workorderservicetaskid' operator='eq' value='" + WOSTRef.Id + @"' />
                                    </filter>
                                  </entity>
                                </fetch>");


            tracingService.Trace("After fetch");
            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            tracingService.Trace("before deletion");
            //throw new InvalidPluginExecutionException(col.Entities.Count.ToString());
            if (col.Entities.Count > 0)
            {
                for (int i = 1; i < col.Entities.Count; i++)
                {
                    service.Delete("ap360_workorderservicetaskhealthlog", col.Entities[i].Id);
                }
            }
            tracingService.Trace("After deletion");
        }
    
    }
}
