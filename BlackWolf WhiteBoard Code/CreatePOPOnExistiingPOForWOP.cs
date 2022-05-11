using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class CreatePOPOnExistiingPOForWOP : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();
            //  throw new InvalidPluginExecutionException("CreatePurchaseOrderforPreferredSupplier");

            try
            {

                ITracingService tracingService =
                  (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                IPluginExecutionContext context = (IPluginExecutionContext)
                   serviceProvider.GetService(typeof(IPluginExecutionContext));

                IOrganizationServiceFactory factory =
                     (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = factory.CreateOrganizationService(context.UserId);

                string purchaseOrderGUID = (string)context.InputParameters["purchaseOrderGUID"];
                string selelctedWorkOrderProductGuids = (string)context.InputParameters["workOrderProductGuid"];
                List<string> lstWorkOrderProductsIds = selelctedWorkOrderProductGuids.Split(',').ToList<string>();
                tracingService.Trace("Count of workorder products " + lstWorkOrderProductsIds.Count.ToString());
                if (lstWorkOrderProductsIds.Count > 0)
                {
                    int count = 0;
                    foreach (string workOrderProductId in lstWorkOrderProductsIds)
                    {
                        //  throw new InvalidPluginExecutionException("-"+workOrderProductId+"-");
                        if (workOrderProductId == "") return;
                        count++;
                        tracingService.Trace("Work Order 0 number " + count.ToString() + " is processing. Guid :" + workOrderProductId);
                        PurchaseOrderProduct.CreatePOPOnExistingPOforWOP(service, tracingService, new Guid(workOrderProductId), new Guid(purchaseOrderGUID));
                    }
                }

                //context.OutputParameters["accountName"] = selelctedWorkOrderProductGuids.ToString();



            }
            catch (Exception ex)
            {
                throw ex;
            }


        }
    }
}