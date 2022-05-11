using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class removePOPFromPOForWOP : IPlugin
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

                string selelctedWorkOrderProductGuids = (string)context.InputParameters["workOrderProductGUID"];

                List<string> lstWorkOrderProductsIds = selelctedWorkOrderProductGuids.Split(',').ToList<string>();

                if (lstWorkOrderProductsIds.Count > 0)
                {
                    foreach (string workOrderProductId in lstWorkOrderProductsIds)
                    {
                        tracingService.Trace("inside foreach");
                        // throw new InvalidPluginExecutionException(workOrderProductId);
                        Entity purchaseOrderProduct = PurchaseOrderProduct.RetrievePOPOnBaseOfWOP(service,tracingService, workOrderProductId);
                        if (purchaseOrderProduct != null)
                        {
                            tracingService.Trace("purchaseOrderProduct not null");
                            service.Delete("msdyn_purchaseorderproduct", purchaseOrderProduct.Id);
                        }
                        tracingService.Trace("before creating msdyn_workorderproduct ");

                        Entity workOrderProduct = new Entity("msdyn_workorderproduct");
                        workOrderProduct.Id = new Guid(workOrderProductId.ToString());
                        workOrderProduct["ap360_purchaseorderid"] = null;
                        workOrderProduct["ap360_purchaseorderproductid"] = null;
                        service.Update(workOrderProduct);
                        tracingService.Trace("after creating msdyn_workorderproduct ");

                    }

                }
                context.OutputParameters["isSuccessful"] = "true";
                tracingService.Trace("End of file");

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}