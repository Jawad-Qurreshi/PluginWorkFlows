using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code.Model
{
    public class RTV_Product
    {
        public Guid guid { get; set; }
        public double Quantity { get; set; }
        public double PartialReceiveQuantity { get; set; }
        public double QuantityReceived { get; set; }
        public EntityReference RTV { get; set; }
        public EntityReference product { get; set; }
        public EntityReference WareHouse { get; set; }

        public static List<RTV_Product> getRtVProduct(IOrganizationService service, Guid rtvGuid)
        {
            List<RTV_Product> lstRTV_Products = new List<RTV_Product>();

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='msdyn_rtvproduct'>
                                <attribute name='msdyn_name' />
                                <attribute name='createdon' />
                                <attribute name='msdyn_rtv' />
                                <attribute name='msdyn_quantity' />
                                <attribute name='msdyn_product' />
                                <attribute name='msdyn_warehouse' />

                                <attribute name='msdyn_rtvproductid' />
                                <order attribute='msdyn_rtv' descending='true' />
                                <filter type='and'>
                                  <condition attribute='msdyn_rtv' operator='eq'  value='" + rtvGuid + @"' /> 
                                </filter>
                              </entity>
                            </fetch>");


            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            RTV_Product rTV_Product;

            foreach (Entity entity in col.Entities)
            {
                rTV_Product = new RTV_Product();
                rTV_Product.guid = entity.Id;


                rTV_Product.Quantity = entity.GetAttributeValue<double>("msdyn_quantity");
                rTV_Product.product = entity.GetAttributeValue<EntityReference>("msdyn_product") != null ? entity.GetAttributeValue<EntityReference>("msdyn_product") : null;
                rTV_Product.WareHouse= entity.GetAttributeValue<EntityReference>("msdyn_warehouse") != null ? entity.GetAttributeValue<EntityReference>("msdyn_warehouse") : null;
                lstRTV_Products.Add(rTV_Product);



            }
            return lstRTV_Products;


        }


        public static List<RTV_Product> getRtVProducts(IOrganizationService service, List<string> rtvGuidlst)
        {
            List<RTV_Product> lstRTV_Products = new List<RTV_Product>();

            string filterDescription = "";
            for (int i = 0; i < rtvGuidlst.Count; i++)
            {
                filterDescription += " <condition attribute='msdyn_rtvproductid' operator='eq' value='" + rtvGuidlst[i] + @"' />";
            }

           

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='msdyn_rtvproduct'>
                                    <attribute name='msdyn_rtv' />
                                    <attribute name='msdyn_quantity' />
                                    <attribute name='ap360_quantityreceived' />
                                    <attribute name='ap360_partialreceivequantity' />
                                    <attribute name='msdyn_rtvproductid' />
                                    <order attribute='msdyn_rtv' descending='true' />
                                          
                                              <filter type='or'>
                                      
                                        " +
                          "" + filterDescription +
                                @"</filter>
                                         
                                          </entity>
                                        </fetch>");
            EntityCollection col;
            // throw new InvalidPluginExecutionException(fetchXml.ToString());
            col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            RTV_Product rTV_Product;

            foreach (Entity entity in col.Entities)
            {
                rTV_Product = new RTV_Product();
                rTV_Product.guid = entity.Id;
                rTV_Product.Quantity = entity.GetAttributeValue<double>("msdyn_quantity");
                rTV_Product.PartialReceiveQuantity = entity.GetAttributeValue<double>("ap360_partialreceivequantity");
                rTV_Product.QuantityReceived = entity.GetAttributeValue<double>("ap360_quantityreceived");
                rTV_Product.RTV = entity.GetAttributeValue<EntityReference>("msdyn_rtv");
               
                lstRTV_Products.Add(rTV_Product);

            }
            return lstRTV_Products;

        }


        public static void CreateRTVProductForReturnWOProduct(IOrganizationService service, Entity workOrderProduct,  Guid newRTVId,EntityReference workOrderRef,string selectedWOProductId)
        {
            double quantityReturn = 0;
            double estimateQuantity= 0;
            double quantityremaining = 0;
            Entity newRTVProduct = new Entity("msdyn_rtvproduct");
            if (workOrderProduct.Contains("ap360_product"))
            {
                newRTVProduct["msdyn_product"] = workOrderProduct.GetAttributeValue<EntityReference>("ap360_product");
            }
            if (workOrderProduct.Contains("msdyn_warehouse"))
            {
                newRTVProduct["msdyn_warehouse"] = workOrderProduct.GetAttributeValue<EntityReference>("msdyn_warehouse");
            }
            newRTVProduct["msdyn_workorder"] = workOrderRef;
            newRTVProduct["msdyn_workorderproduct"] = new EntityReference("msdyn_workorderproduct", Guid.Parse(selectedWOProductId));

            if (workOrderProduct.Contains("msdyn_unit"))
            {
                newRTVProduct["msdyn_unit"] = workOrderProduct.GetAttributeValue<EntityReference>("msdyn_unit");
            }
            if (workOrderProduct.Contains("msdyn_estimatequantity"))
            {
                estimateQuantity = workOrderProduct.GetAttributeValue<double>("msdyn_estimatequantity"); 
            }
            if (workOrderProduct.Contains("ap360_quantityreturn"))
            {
                 quantityReturn = workOrderProduct.GetAttributeValue<double>("ap360_quantityreturn");
            }
            quantityremaining = workOrderProduct.GetAttributeValue<double>("ap360_quantityremaining");

            newRTVProduct["msdyn_quantity"] =  quantityReturn;
            if (workOrderProduct.Contains("transactioncurrencyid"))
            {
                newRTVProduct["transactioncurrencyid"] = workOrderProduct.GetAttributeValue<EntityReference>("transactioncurrencyid");
            }
            if (newRTVId != null)
            {
                newRTVProduct["msdyn_rtv"] = new EntityReference("msdyn_rtv", newRTVId);
            }

           // newRTVProduct["msdyn_quantity"] = workOrderProduct.GetAttributeValue<double>("msdyn_estimatequantity");
            newRTVProduct["msdyn_unitcreditamount"] = new Money(workOrderProduct.GetAttributeValue<Money>("msdyn_estimateunitcost").Value);


            Guid newRTVProductId = service.Create(newRTVProduct);
        }
    }
}



 