using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code.Model
{
    public class WorkOrderHealthLog
    {
        public static void CreateWorkOrderLog(IOrganizationService service, Entity entity, EntityReference lastbookingworkedon, EntityReference recentlyWorkedResource, EntityReference workOrderRef, Money workOrderEstimatedLaborAmount, decimal cumulativeSumofPredictedSpend, Entity reterivedWorkOrder)
        {
            Entity workOrderLog = new Entity("ap360_workorderhealthlog");
            workOrderLog["ap360_name"] = reterivedWorkOrder["msdyn_name"] + " Log";
            workOrderLog["ap360_preworkorderhealth"] = reterivedWorkOrder.GetAttributeValue<decimal>("ap360_postworkorderhealth");// pre workOrderHealth
            if (workOrderEstimatedLaborAmount != null && workOrderEstimatedLaborAmount.Value > 0)
            {
                workOrderLog["ap360_postworkorderhealth"] = cumulativeSumofPredictedSpend / workOrderEstimatedLaborAmount.Value; //Post Work Order Health
            }
            workOrderLog["ap360_workorderservicetaskid"] = new EntityReference(entity.LogicalName, entity.Id);

            if (lastbookingworkedon != null)
                workOrderLog["ap360_bookableresourcebookingid"] = lastbookingworkedon;
            if (recentlyWorkedResource != null)
                workOrderLog["ap360_bookableresourceid"] = recentlyWorkedResource;

            workOrderLog["ap360_workorderid"] = new EntityReference(workOrderRef.LogicalName, workOrderRef.Id);
            service.Create(workOrderLog);
        }
    }
}
