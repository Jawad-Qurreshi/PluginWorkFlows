using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code.Model
{
    public class ProductInventory
    {

        public double quantityAvailable { get; set; }

        public static double getProducutQuantityAvaiable(IOrganizationService service, ITracingService tracingservice, Guid productGuid)
        {


            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='msdyn_productinventory'>
                                    <attribute name='createdon' />
                                    <attribute name='msdyn_warehouse' />
                                    <attribute name='msdyn_unit' />
                                    <attribute name='msdyn_qtyonhand' />
                                    <attribute name='msdyn_product' />
                                    <attribute name='msdyn_reorderpoint' />
                                    <attribute name='msdyn_qtyonorder' />
                                    <attribute name='msdyn_qtyavailable' />
                                    <attribute name='msdyn_qtyallocated' />
                                    <attribute name='msdyn_productinventoryid' />
                                    <order attribute='msdyn_product' descending='false' />
                                    <filter type='and'>
                                      <condition attribute='msdyn_product' operator='eq'  value='" + productGuid + @"' /> 
                                    </filter>
                                  </entity>
                                </fetch>");


            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            WorkOrderServiceTask workOrderServiceTask = new WorkOrderServiceTask();
            tracingservice.Trace("count of productinventory " + col.Entities.Count.ToString());
            double productQuantity = 0.0;
            foreach (Entity entity in col.Entities)
            {

                productQuantity += entity.GetAttributeValue<double>("msdyn_qtyavailable");
                
            }
            tracingservice.Trace("After for loop of productinventory " + col.Entities.Count.ToString());
            return productQuantity;

        }
        public static EntityCollection RetrieveProductInventoryBasedOnProduct(IOrganizationService service, ITracingService tracingservice, Guid productGuid)
        {


            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='msdyn_productinventory'>
                                    <attribute name='msdyn_product' />
                                    <attribute name='msdyn_productinventoryid' />
                                    <attribute name='ap360_partnumber' />
                                    <order attribute='msdyn_product' descending='false' />
                                    <filter type='and'>
                                      <condition attribute='msdyn_product' operator='eq' uiname='' uitype='product' value='" + productGuid + @"' />
                                    </filter>
                                  </entity>
                                </fetch>");

            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (col.Entities.Count < 1) {
                return null;
            }
            return col;
                

        }

    }

}
