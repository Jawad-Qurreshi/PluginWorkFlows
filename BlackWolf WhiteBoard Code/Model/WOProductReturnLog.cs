using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code.Model
{
    public class WOProductReturnLog
    {

        public static void createwoProductReturnLog(IOrganizationService service, ITracingService tracing, Entity workOrderProduct, double quantityReturn)
        {
            Entity createWOProReturnLog = new Entity("ap360_woproductreturnlog");
            createWOProReturnLog["ap360_workorderproductid"] = new EntityReference("msdyn_workorderproduct", workOrderProduct.Id);
            int reasonToReturn = workOrderProduct.GetAttributeValue<OptionSetValue>("ap360_reasontoreturn").Value;

            if (reasonToReturn == 126300000)
                createWOProReturnLog["ap360_reasontoreturn"] = "We Damaged";
            if (reasonToReturn == 126300001)
                createWOProReturnLog["ap360_reasontoreturn"] = "DOA";
            if (reasonToReturn == 126300002)
                createWOProReturnLog["ap360_reasontoreturn"] = "Defective";
            if (reasonToReturn == 126300003)
                createWOProReturnLog["ap360_reasontoreturn"] = "Incorrect";
            if (reasonToReturn == 126300004)
                createWOProReturnLog["ap360_reasontoreturn"] = "Not Needed	";

            createWOProReturnLog["ap360_quantityreturned"] = quantityReturn;
            createWOProReturnLog["ap360_returneddate"] = DateTime.Now;

            service.Create(createWOProReturnLog);
            //  throw new InvalidPluginExecutionException("Error");
        }
    }
}
