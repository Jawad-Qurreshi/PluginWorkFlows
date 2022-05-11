using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code.Model
{
    public class TaskActivity
    {
        public static void CreateTaskForTechnicianClockOut(IOrganizationService service, ITracingService tracingService, Entity BRBEntity, EntityReference wostRef)
        {
            Entity createActivity = new Entity("task");

            createActivity["subject"] = BRBEntity.GetAttributeValue<string>("ap360_activitysubject");
            createActivity["ap360_cause"] = BRBEntity.GetAttributeValue<string>("ap360_activitycause");
            createActivity["ap360_correction"] = BRBEntity.GetAttributeValue<string>("ap360_activitycorrection");
            createActivity["description"] = BRBEntity.GetAttributeValue<string>("ap360_activitydescription");
            createActivity["actualdurationminutes"] = BRBEntity.GetAttributeValue<string>("ap360_actualdurationminutes");
            createActivity["regardingobjectid"] = new EntityReference("msdyn_workorderservicetask", wostRef.Id);
            createActivity["ap360_workorderservicetaskstatus"] = BRBEntity.GetAttributeValue<OptionSetValue>("ap360_workorderservicetaskstatus") ?? null;
            createActivity["ap360_workorderservicetasksubstatus"] = BRBEntity.GetAttributeValue<OptionSetValue>("ap360_workorderservicetasksubstatus") ?? null;
            createActivity["ap360_requiredreason"] = BRBEntity.GetAttributeValue<OptionSetValue>("ap360_requiredreason") ?? null;
            createActivity["ap360_discoveryimpact"] = BRBEntity.GetAttributeValue<OptionSetValue>("ap360_discoveryimpact") ?? null;
            createActivity["ap360_ethicaltobill100"] = BRBEntity.GetAttributeValue<OptionSetValue>("ap360_ethicaltobill100") ?? null;
            createActivity["ap360_not100completedreason"] = BRBEntity.GetAttributeValue<OptionSetValue>("ap360_not100completedreason") ?? null;

            

            createActivity["scheduledend"] = DateTime.Now;


            
            service.Create(createActivity);
        }

        public static void CreateTaskForMistakeAdjustment(IOrganizationService service, ITracingService tracingService, Entity BRBEntity, DateTime mistakeadjustmentstarttime, DateTime mistakeadjustmentendtime, string mistakeadjustmentreason)
        {
            //throw new InvalidPluginExecutionException("Custom error");
            Entity createActivity = new Entity("task");

            
            DateTime TempCheckdateTime = new DateTime(); //This will be default date inorder to check if date is default or correct 

            createActivity["subject"] = "Adjust Booking Time";

            createActivity["ap360_bookingmistakeadjustment"] = true;

            if (mistakeadjustmentstarttime != TempCheckdateTime)//Default date check
             createActivity["ap360_bookingmistakeadjustmentstarttime"] = mistakeadjustmentstarttime;
            if (mistakeadjustmentendtime != TempCheckdateTime)//Default date check
                createActivity["ap360_bookingmistakeadjustmentendtime"] = mistakeadjustmentendtime;
            createActivity["ap360_bookingmistakeadjustmentreason"] = mistakeadjustmentreason;
            createActivity["regardingobjectid"] = new EntityReference("bookableresourcebooking", BRBEntity.Id);
            createActivity["scheduledend"] = DateTime.Now;

            service.Create(createActivity);
        }

        public static string GetRegardingEntityDisplayName(IOrganizationService service, ITracingService tracingService, EntityReference regardingObjectRef)
        {
            string EntityDisplayName="-";
            if (regardingObjectRef.LogicalName == "msdyn_workorderproduct" )
                EntityDisplayName = "Work Order Product";
            else if (regardingObjectRef.LogicalName == "msdyn_workorderservicetask" )
                EntityDisplayName = "Work Order Service Task";
            else if (regardingObjectRef.LogicalName == "opportunity")
                EntityDisplayName = "Opportunity";
            else if (regardingObjectRef.LogicalName == "ap360_quoteproduct" )
                EntityDisplayName = "Quote Product";
            else if (regardingObjectRef.LogicalName == "ap360_workordersublet")
                EntityDisplayName  = "Work Order Sublet";
            else if (regardingObjectRef.LogicalName == "msdyn_workorder")
                EntityDisplayName = "Work Order";
            else if (regardingObjectRef.LogicalName == "quote")
                EntityDisplayName = "Quote";
            else if (regardingObjectRef.LogicalName == "bookableresourcebooking")
                EntityDisplayName = "Bookable Resource Booking";
            return EntityDisplayName;


        }
    }
}
