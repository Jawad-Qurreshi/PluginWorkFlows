using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code.Model
{
    public class RTV
    {

        public static Guid CreateRTVForReturnWOProduct(IOrganizationService service, Entity workOrderProduct)
        {
            Entity newRTV = new Entity("msdyn_rtv");
            if (workOrderProduct.Contains("ap360_vendorid"))
            {
                newRTV["msdyn_vendor"] = workOrderProduct.GetAttributeValue<EntityReference>("ap360_vendorid");
            }
            if (workOrderProduct.Contains("transactioncurrencyid"))
            {
                newRTV["transactioncurrencyid"] = workOrderProduct.GetAttributeValue<EntityReference>("transactioncurrencyid");
            }
            if (workOrderProduct.Contains("ap360_product"))
            {
                newRTV["ap360_productid"] = workOrderProduct.GetAttributeValue<EntityReference>("ap360_product");
            }

            Guid newRTVId = service.Create(newRTV);

            return newRTVId;
        }
        public static Guid getDraftRTVForSpecificVendor(IOrganizationService service, Guid vendorGuid)
        {
            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='msdyn_rtv'>
                                    <attribute name='msdyn_rtvid' />
                                    <order attribute='msdyn_name' descending='true' />
                                    <filter type='and'>
                                      <condition attribute='msdyn_systemstatus' operator='eq' value='690970000' />
                                      <condition attribute='msdyn_vendor' operator='eq'  value='" + vendorGuid + @"' />
                                    </filter>
                                  </entity>
                                </fetch>");

            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));

            Guid RTVGuid = Guid.Empty;

            if (col.Entities.Count > 0)
            {
                RTVGuid = col.Entities[0].Id;                                            
            }
            return RTVGuid;
        }

        public static bool checkRTVProductStatus(IOrganizationService service, ITracingService tracingService, EntityReference RTVEntityRef,ref bool IsAnyProductReceived)
        {
            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='msdyn_rtvproduct'>
                                    <attribute name='msdyn_quantity' />
                                    <attribute name='msdyn_rtvproductid' />
                                    <attribute name='ap360_systemstatus' />
                                    <order attribute='msdyn_quantity' descending='true' />
                                    <filter type='and'>
                                      <condition attribute='msdyn_rtv' operator='eq'  value='"+ RTVEntityRef.Id + @"' />
                                    </filter>
                                  </entity>
                                </fetch>");

            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            bool isAmountReceived=true;
            if (col.Entities.Count > 0)
            {
                foreach (var entity in col.Entities) {

                    if (entity.GetAttributeValue<OptionSetValue>("ap360_systemstatus") != null)
                    {
                        int systemStatus = entity.GetAttributeValue<OptionSetValue>("ap360_systemstatus").Value;
                        //Draft   690970000
                        //Approved    690970001
                        //Shipped 690970002
                        //Received    690970003
                        //Canceled    690970004
                        if (systemStatus != 126300002)//Refund ReceivedReceived  
                        {
                            
                            isAmountReceived = false;
                            //return isAmountReceived;
                        }

                        if (systemStatus == 126300002 || systemStatus == 126300001)//Refund Received || partially received
                        {

                            IsAnyProductReceived = true;
                            //return isAmountReceived;
                        }

                    }
                }
            }
            return isAmountReceived;
        }
    }
}
