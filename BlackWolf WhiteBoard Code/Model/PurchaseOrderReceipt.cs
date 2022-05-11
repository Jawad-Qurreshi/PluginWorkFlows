using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code.Model
{
    public class PurchaseOrderReceipt
    {
        public static Guid CreatePurchaseOrderReceipt(IOrganizationService service, ITracingService tracing, Entity purchaseOrderEntity,Guid userId)
        {
            Guid newlyCreatedPurchaseOrderReceiptGuid = Guid.Empty;
            if (purchaseOrderEntity != null)
            {
                Entity newPurchaseOrderReceipt = new Entity("msdyn_purchaseorderreceipt");

                newPurchaseOrderReceipt["msdyn_purchaseorder"] = new EntityReference("msdyn_purchaseorder", purchaseOrderEntity.Id);
                newPurchaseOrderReceipt["msdyn_receivedby"] = new EntityReference("systemuser", userId);

                newPurchaseOrderReceipt["msdyn_name"] = purchaseOrderEntity.GetAttributeValue<string>("msdyn_name") + " PO Receipt";
               newlyCreatedPurchaseOrderReceiptGuid =  service.Create(newPurchaseOrderReceipt);
            }
            return newlyCreatedPurchaseOrderReceiptGuid;
        }

        public static void RetrieveAndDeletePurchaseOrderReceipt(IOrganizationService service, ITracingService tracingService, Guid purchaseOrderGuid)
        {

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='msdyn_purchaseorderreceipt'>
                                    <attribute name='msdyn_name' />
                                    <attribute name='createdon' />
                                    <attribute name='msdyn_purchaseorder' />
                                    <attribute name='msdyn_datereceived' />
                                    <attribute name='msdyn_purchaseorderreceiptid' />
                                    <order attribute='msdyn_datereceived' descending='true' />
                                    <filter type='and'>
                                      <condition attribute='msdyn_purchaseorder' operator= 'eq'  value='" + purchaseOrderGuid + @"' /> 
                                    </filter>
                                  </entity>
                                </fetch>");


            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (col.Entities.Count > 0)
            {
                tracingService.Trace("Count " + col.Entities.Count.ToString());
                foreach (Entity receipt in col.Entities)
                {
                    service.Delete(receipt.LogicalName, receipt.Id);
                }
            }


        }


    }
}
