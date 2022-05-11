using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code.Model
{
    public class PurchaseOrderProduct
    {
        Guid guid { get; set; }
        EntityReference purchaseOrder { get; set; }
        double Quantity { get; set; }
        double QuantityReceived { get; set; }
        double QuantityRemaining { get; set; }


        Money UnitCost { get; set; }
        public EntityReference WorkOrder { get; set; }
        public Guid workOrder_opportunity { get; set; }

        public static Guid CreatePurchaseOrderProduct(IOrganizationService service, ITracingService tracingservice, QuoteProduct quoteproduct, Guid newlyCreatedpurchaseOrderGuid, Guid workOrderGuid, Guid newlycreatedWorkOrderProductGuid)
        {

            Entity purchaseOrderProduct = new Entity("msdyn_purchaseorderproduct");
            if (quoteproduct.ProductRef != null)
            {
                purchaseOrderProduct["msdyn_product"] = new EntityReference("product", quoteproduct.ProductRef.Id);
            }
            else
            {
                throw new InvalidPluginExecutionException("Product is not selected in Quote Product: " + quoteproduct.Name);

            }
            tracingservice.Trace("Purchase Order Guid " + newlyCreatedpurchaseOrderGuid.ToString());
            purchaseOrderProduct["ap360_workorderproductid"] = new EntityReference("msdyn_workorderproduct", newlycreatedWorkOrderProductGuid);

            purchaseOrderProduct["msdyn_purchaseorder"] = new EntityReference("msdyn_purchaseorder", newlyCreatedpurchaseOrderGuid);
            tracingservice.Trace("Work Order Guid " + workOrderGuid.ToString());
            //purchaseOrderProduct["msdyn_associatetoworkorder"] = new EntityReference("msdyn_workorder", workOrderGuid);
            if (quoteproduct.Warehouse != null)
                purchaseOrderProduct["msdyn_associatetowarehouse"] = new EntityReference("msdyn_warehouse", quoteproduct.Warehouse.Id);

            tracingservice.Trace("Middle");

            if (quoteproduct.UnitCost != null)
                purchaseOrderProduct["msdyn_unitcost"] = new Money(quoteproduct.UnitCost.Value);
            //we don't need to map unit price in Purchase Order Product
            purchaseOrderProduct["msdyn_quantity"] = quoteproduct.Quantity;
            purchaseOrderProduct["msdyn_itemstatus"] = new OptionSetValue(690970000);//pending
            purchaseOrderProduct["ap360_itemsubstatus"] = new OptionSetValue(126300002);//pending


            if (quoteproduct.Manufacturer != null)
                purchaseOrderProduct["ap360_manufacturer"] = quoteproduct.Manufacturer;
            if (quoteproduct.BarCode != null)
                purchaseOrderProduct["ap360_barcode"] = quoteproduct.BarCode;
            if (quoteproduct.Vendor != null)
                purchaseOrderProduct["ap360_vendorid"] = new EntityReference("account", quoteproduct.Vendor.Id);
            if (quoteproduct.UOM != null)
                purchaseOrderProduct["msdyn_unit"] = new EntityReference("uom", quoteproduct.UOM.Id);
            else
                purchaseOrderProduct["msdyn_unit"] = new EntityReference("uom", new Guid("361a3eac-749c-4bb3-92a2-d63f692f61ba"));//UNIT
            purchaseOrderProduct["ap360_sku"] = quoteproduct.SKU;

            tracingservice.Trace("End");



            //purchaseOrderProduct["ap360_opportunityid"] = new EntityReference();
            //purchaseOrderProduct["ap360_workorderproductid"] = new EntityReference();
            tracingservice.Trace("Before Purchase Order Product Creation");
            Guid newlyCreatedPurchaseOrderProductGuid = service.Create(purchaseOrderProduct);
            tracingservice.Trace("PO Created with Guid " + newlyCreatedPurchaseOrderProductGuid.ToString());
            return newlyCreatedPurchaseOrderProductGuid;
        }
        public static Guid CreatePurchaseOrderProductForSublet(IOrganizationService service, ITracingService tracingservice, QuoteSublet quoteSublet, Guid newlyCreatedpurchaseOrderGuid, Guid workOrderGuid)
        {
            tracingservice.Trace("Creation Started for Sublet Purchase Order Product");
            Entity purchaseOrderProduct = new Entity("msdyn_purchaseorderproduct");
            if (quoteSublet.Product != null)
            {
                purchaseOrderProduct["msdyn_product"] = new EntityReference("product", quoteSublet.Product.Id);
            }
            else
            {
                throw new InvalidPluginExecutionException("Product is not selected in Quote Sublet : Select sublet product: " + quoteSublet.Name);

            }
            tracingservice.Trace("Purchase Order Guid " + newlyCreatedpurchaseOrderGuid.ToString());
            purchaseOrderProduct["msdyn_purchaseorder"] = new EntityReference("msdyn_purchaseorder", newlyCreatedpurchaseOrderGuid);
            tracingservice.Trace("Work Order Guid " + workOrderGuid.ToString());
            purchaseOrderProduct["msdyn_associatetoworkorder"] = new EntityReference("msdyn_workorder", workOrderGuid);
            purchaseOrderProduct["msdyn_associatetowarehouse"] = new EntityReference("msdyn_warehouse", new Guid("5b743789-c329-41ee-89e5-f81b83570131"));

            tracingservice.Trace("Middle of Sublet Creation");
            if (quoteSublet.EstimatedAmount != null)
                purchaseOrderProduct["msdyn_unitcost"] = new Money(quoteSublet.EstimatedAmount.Value);
            //we don't need to map unit price in Purchase Order Product
            double quantity = 1;
            purchaseOrderProduct["msdyn_quantity"] = quantity;
           purchaseOrderProduct["msdyn_itemstatus"] = new OptionSetValue(690970000);//pending
            purchaseOrderProduct["ap360_itemsubstatus"] = new OptionSetValue(126300002);//pending
            purchaseOrderProduct["msdyn_unit"] = new EntityReference("uom", new Guid("361a3eac-749c-4bb3-92a2-d63f692f61ba"));//UNIT
            tracingservice.Trace("End");



            //purchaseOrderProduct["ap360_opportunityid"] = new EntityReference();
            //purchaseOrderProduct["ap360_workorderproductid"] = new EntityReference();
            tracingservice.Trace("Before Purchase Order Product Creation");
            Guid newlyCreatedPurchaseOrderProductGuid = service.Create(purchaseOrderProduct);
            tracingservice.Trace("PO Created with Guid " + newlyCreatedPurchaseOrderProductGuid.ToString());
            return newlyCreatedPurchaseOrderProductGuid;
        }



        public static void CreatePOPOnExistingPOforWOP(IOrganizationService service, ITracingService tracingservice, Guid workOrderProductGuid, Guid purchaseOrderGUID)
        {


            tracingservice.Trace("inside CreatePOPOnExistingPOforWOP");

            WorkOrderProduct workOrderProduct = new WorkOrderProduct();
            workOrderProduct = WorkOrderProduct.getWorkOrderProduct(service, tracingservice, workOrderProductGuid);
            if (workOrderProduct == null) return;

            tracingservice.Trace("Creation Started for Sublet Purchase Order Product " + workOrderProductGuid.ToString());
            Entity purchaseOrderProduct = new Entity("msdyn_purchaseorderproduct");
            if (workOrderProduct.ProductField != null)
                purchaseOrderProduct["msdyn_product"] = workOrderProduct.ProductField;

            purchaseOrderProduct["ap360_workorderproductid"] = new EntityReference("msdyn_workorderproduct", workOrderProductGuid);
            if (workOrderProduct.WorkOrder != null)
                purchaseOrderProduct["msdyn_associatetoworkorder"] = workOrderProduct.WorkOrder;
            if (workOrderProduct.Opportuntiy != null)
            {
                purchaseOrderProduct["ap360_opportunityid"] = workOrderProduct.Opportuntiy;
                Entity reterivedOpportunity = service.Retrieve(workOrderProduct.Opportuntiy.LogicalName, workOrderProduct.Opportuntiy.Id, new ColumnSet("ap360_opportunityautonumber"));
                string opportuntityAutoNumber = reterivedOpportunity.GetAttributeValue<string>("ap360_opportunityautonumber");
                purchaseOrderProduct["ap360_opportunityautonumber"] = opportuntityAutoNumber;

            }
            purchaseOrderProduct["msdyn_purchaseorder"] = new EntityReference("msdyn_purchaseorder", purchaseOrderGUID);


            purchaseOrderProduct["msdyn_associatetowarehouse"] = new EntityReference("msdyn_warehouse", new Guid("5b743789-c329-41ee-89e5-f81b83570131"));


            //  throw new InvalidPluginExecutionException("Quantity "+workOrderProduct.Quantity.ToString()+" Price "+ workOrderProduct.PartCost.Value.ToString());
            double quantity = workOrderProduct.Quantity;
            if (quantity <= 0 && workOrderProduct.ProductField != null)
            {
                throw new InvalidPluginExecutionException("Pop can't be created for "+workOrderProduct.ProductField +", quantity is 0" );
            }
            if (workOrderProduct.PartCost != null)
            {
                purchaseOrderProduct["msdyn_unitcost"] = new Money(workOrderProduct.PartCost.Value);
                purchaseOrderProduct["msdyn_totalcost"] = new Money(workOrderProduct.PartCost.Value * Convert.ToDecimal(quantity));//quanitty remaining

            }
            purchaseOrderProduct["ap360_partnumber"] = workOrderProduct.PartNumber;
            purchaseOrderProduct["msdyn_quantity"] = quantity;
            purchaseOrderProduct["msdyn_itemstatus"] = new OptionSetValue(690970000);//pending
            purchaseOrderProduct["ap360_itemsubstatus"] = new OptionSetValue(126300002);//pending

            purchaseOrderProduct["msdyn_unit"] = new EntityReference("uom", new Guid("361a3eac-749c-4bb3-92a2-d63f692f61ba"));//UNIT

            tracingservice.Trace("End");
            tracingservice.Trace("Before Purchase Order Product Creation");
            Guid newlyCreatedPurchaseOrderProductGuid = service.Create(purchaseOrderProduct);

            Entity updateWorkOrderProduct = new Entity("msdyn_workorderproduct",workOrderProduct.guid);
            updateWorkOrderProduct["ap360_purchaseorderproductid"] = new EntityReference("msdyn_workorderproduct", newlyCreatedPurchaseOrderProductGuid);
            service.Update(updateWorkOrderProduct);

            tracingservice.Trace("pop Created with Guid " + newlyCreatedPurchaseOrderProductGuid.ToString());
        }

        public static List<PurchaseOrderProduct> GetPurchaseOrderProducts(IOrganizationService service, ITracingService tracingservice, Guid purchaseOrderGuid)
        {

            List<PurchaseOrderProduct> lstPurchaseOrderProducts = new List<PurchaseOrderProduct>();

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='msdyn_purchaseorderproduct'>
                                    <attribute name='createdon' />
                                    <attribute name='msdyn_quantity' />
                                    <attribute name='msdyn_purchaseorder' />
                                    <attribute name='msdyn_product' />
                                    <attribute name='msdyn_purchaseorderproductid' />
                                    <attribute name='msdyn_unitcost' />
                                    <attribute name='msdyn_associatetoworkorder' />
                                    <order attribute='msdyn_purchaseorder' descending='true' />
                                    <filter type='and'>
                                      <condition attribute='msdyn_purchaseorder' operator='eq'  value='" + purchaseOrderGuid + @"' /> 
                                      <condition attribute='ap360_itemsubstatus' operator='eq' value='126300002' />
                                    </filter>
                                  </entity>
                                </fetch>");

            //                         <condition attribute='msdyn_itemstatus' operator='eq' value='690970000' /> 
            // 690970000 item status pending
            // 126300002 item substatus pending



            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            tracingservice.Trace("Data Reterived successfully");
            PurchaseOrderProduct purchaseOrderProduct;
            foreach (Entity entity in col.Entities)
            {
                tracingservice.Trace("Inside foreach");                

                purchaseOrderProduct = new PurchaseOrderProduct();
                purchaseOrderProduct.guid = entity.Id;

                purchaseOrderProduct.Quantity = entity.GetAttributeValue<double>("msdyn_quantity");
                tracingservice.Trace("first row");

                purchaseOrderProduct.UnitCost = entity.GetAttributeValue<Money>("msdyn_unitcost") != null ? entity.GetAttributeValue<Money>("msdyn_unitcost") : null;
                tracingservice.Trace("second row");

                purchaseOrderProduct.purchaseOrder = entity.GetAttributeValue<EntityReference>("msdyn_purchaseorder") != null ? entity.GetAttributeValue<EntityReference>("msdyn_purchaseorder") : null;
                tracingservice.Trace("third row");

                purchaseOrderProduct.WorkOrder = entity.GetAttributeValue<EntityReference>("msdyn_associatetoworkorder") != null ? entity.GetAttributeValue<EntityReference>("msdyn_associatetoworkorder") : null;


                lstPurchaseOrderProducts.Add(purchaseOrderProduct);
                tracingservice.Trace("end of one foreach object");

            }
            return lstPurchaseOrderProducts;

        }
        public static List<PurchaseOrderProduct> GetNotReceivedAndPartiallyReceivedPOPs(IOrganizationService service, ITracingService tracingservice, Guid purchaseOrderGuid)
        {

            List<PurchaseOrderProduct> lstPurchaseOrderProducts = new List<PurchaseOrderProduct>();

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='msdyn_purchaseorderproduct'>
                                    <attribute name='createdon' />
                                    <attribute name='msdyn_quantity' />
                                    <attribute name='msdyn_qtyreceived' />
                                    <attribute name='msdyn_purchaseorder' />
                                    <attribute name='msdyn_product' />
                                    <attribute name='msdyn_purchaseorderproductid' />
                                    <attribute name='msdyn_unitcost' />
                                    <attribute name='msdyn_associatetoworkorder' />
                                    <order attribute='msdyn_purchaseorder' descending='true' />
                                    <filter type='and'>
                                      <condition attribute='msdyn_purchaseorder' operator='eq'  value='" + purchaseOrderGuid + @"' /> 
                                    </filter>
                                  </entity>
                                </fetch>");

            // 690970000 item status pending


            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            tracingservice.Trace("Data Reterived successfully");
            PurchaseOrderProduct purchaseOrderProduct;
            foreach (Entity entity in col.Entities)
            {
                tracingservice.Trace("Inside foreach");                
                purchaseOrderProduct = new PurchaseOrderProduct();
                purchaseOrderProduct.guid = entity.Id;

                purchaseOrderProduct.Quantity = entity.GetAttributeValue<double>("msdyn_quantity");
                purchaseOrderProduct.QuantityReceived = entity.GetAttributeValue<double>("msdyn_qtyreceived");
                purchaseOrderProduct.QuantityRemaining = purchaseOrderProduct.Quantity - purchaseOrderProduct.QuantityReceived;
                if (purchaseOrderProduct.Quantity != purchaseOrderProduct.QuantityReceived)
                {

                    lstPurchaseOrderProducts.Add(purchaseOrderProduct);
                    tracingservice.Trace("end of one foreach object");

                }
            }
            return lstPurchaseOrderProducts;

        }

        public static int GetPurchaseOrderProductsRelatedTOPOCount(IOrganizationService service, ITracingService tracingservice, Guid purchaseOrderGuid, out bool isQuantitiesSame)
        {

            List<PurchaseOrderProduct> lstPurchaseOrderProducts = new List<PurchaseOrderProduct>();

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='msdyn_purchaseorderproduct'>  
                                    <attribute name='msdyn_quantity' />
                                    <attribute name='msdyn_qtyreceived' />


 
                                
                                    <filter type='and'>
                                      <condition attribute='msdyn_purchaseorder' operator='eq'  value='" + purchaseOrderGuid + @"' /> 
                                    </filter>                                    
                                  </entity>
                                </fetch>");


            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            //col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            tracingservice.Trace("Data Reterived successfully");
            isQuantitiesSame = true;
            foreach (Entity entity in col.Entities)
            {
                tracingservice.Trace("Inside foreach");

                double quantity = entity.GetAttributeValue<double>("msdyn_quantity");
                double quantityReceived = entity.GetAttributeValue<double>("msdyn_qtyreceived");
                if (quantity != quantityReceived)
                {
                    isQuantitiesSame = false;
                }

            }
            return col.Entities.Count;
        }
        public static int CheckAllPurchaseOrderProductsAreRecevied(IOrganizationService service, ITracingService tracingservice, Guid purchaseOrderGuid, out bool isAllItemsSubStatusRecevied)
        {

            List<PurchaseOrderProduct> lstPurchaseOrderProducts = new List<PurchaseOrderProduct>();

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='msdyn_purchaseorderproduct'>  
                                    <attribute name='msdyn_quantity' />
                                    <attribute name='msdyn_qtyreceived' />
                                    <attribute name='ap360_itemsubstatus' />


 
                                
                                    <filter type='and'>
                                      <condition attribute='msdyn_purchaseorder' operator='eq'  value='" + purchaseOrderGuid + @"' /> 
                                    </filter>                                    
                                  </entity>
                                </fetch>");


            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            //col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            tracingservice.Trace("CheckAllPurchaseOrderProductsAreRecevied :POP count " + col.Entities.Count.ToString());
            isAllItemsSubStatusRecevied = true;
            int count = 0;
            foreach (Entity entity in col.Entities)
            {
                count++;
                tracingservice.Trace("Inside foreach "+count.ToString());
                if (entity.GetAttributeValue<OptionSetValue>("ap360_itemsubstatus") != null)
                {
                    int itemSubStatus = entity.GetAttributeValue<OptionSetValue>("ap360_itemsubstatus").Value;
                    tracingservice.Trace(itemSubStatus.ToString());

                    double Quantity = entity.GetAttributeValue<double>("msdyn_quantity");
                    double QuantityReceived = entity.GetAttributeValue<double>("msdyn_qtyreceived");
                    tracingservice.Trace(Quantity.ToString() + " " + QuantityReceived.ToString());
                    //Received    126300000
                    //Partially Received  126300001
                    //Pending 126300002
                    //Canceled    126300003
                    if (itemSubStatus != 126300000 && itemSubStatus!= 126300003 )
                    {
                        tracingservice.Trace("item Substatus is not recevied of " + count.ToString() + " pop");
                        isAllItemsSubStatusRecevied = false;
                    }
                }
                else
                {
                    tracingservice.Trace("item sub status is null");
                    isAllItemsSubStatusRecevied = false;

                }

            }
         //   throw new InvalidPluginExecutionException("stop");
            
            return col.Entities.Count;
        }

        public static List<PurchaseOrderProduct> GetPurchaseOrderProductsForSubletCreation(IOrganizationService service, ITracingService tracingservice, Guid purchaseOrderGuid)
        {

            List<PurchaseOrderProduct> lstPurchaseOrderProducts = new List<PurchaseOrderProduct>();

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='msdyn_purchaseorderproduct'>
                                    <attribute name='createdon' />
                                    <attribute name='msdyn_quantity' />
                                    <attribute name='msdyn_purchaseorder' />
                                    <attribute name='msdyn_product' />
                                    <attribute name='msdyn_purchaseorderproductid' />
                                    <attribute name='msdyn_unitcost' />
                                    <attribute name='msdyn_associatetoworkorder' />
                                    <order attribute='msdyn_purchaseorder' descending='true' />
                                    <filter type='and'>
                                      <condition attribute='msdyn_purchaseorder' operator='eq'  value='" + purchaseOrderGuid + @"' /> 
                                    </filter>
                                     <link-entity name='msdyn_workorder' from='msdyn_workorderid' to='msdyn_associatetoworkorder' visible='false' link-type='inner' alias='workorder'>
                                          <attribute name='msdyn_opportunityid' />
                                        </link-entity>
                                  </entity>
                                </fetch>");


            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            tracingservice.Trace("Data Reterived successfully");
            PurchaseOrderProduct purchaseOrderProduct;
            foreach (Entity entity in col.Entities)
            {
                tracingservice.Trace("Inside foreach");

                purchaseOrderProduct = new PurchaseOrderProduct();
                purchaseOrderProduct.guid = entity.Id;

                purchaseOrderProduct.Quantity = entity.GetAttributeValue<double>("msdyn_quantity");
                tracingservice.Trace("first row");

                purchaseOrderProduct.UnitCost = entity.GetAttributeValue<Money>("msdyn_unitcost") != null ? entity.GetAttributeValue<Money>("msdyn_unitcost") : null;
                tracingservice.Trace("second row");

                purchaseOrderProduct.purchaseOrder = entity.GetAttributeValue<EntityReference>("msdyn_purchaseorder") != null ? entity.GetAttributeValue<EntityReference>("msdyn_purchaseorder") : null;
                tracingservice.Trace("third row");

                purchaseOrderProduct.WorkOrder = entity.GetAttributeValue<EntityReference>("msdyn_associatetoworkorder") != null ? entity.GetAttributeValue<EntityReference>("msdyn_associatetoworkorder") : null;
                if (entity.GetAttributeValue<AliasedValue>("workorder.msdyn_opportunityid") != null)
                {
                    purchaseOrderProduct.workOrder_opportunity = ((Microsoft.Xrm.Sdk.EntityReference)(entity.GetAttributeValue<AliasedValue>("workorder.msdyn_opportunityid")).Value).Id;
                }

                lstPurchaseOrderProducts.Add(purchaseOrderProduct);
                tracingservice.Trace("end of one foreach object");

            }
            // throw new InvalidPluginExecutionException("Error " +lstPurchaseOrderProducts.Count.ToString());
            return lstPurchaseOrderProducts;

        }

        public static void UpdatePurchaseOrderProducts(IOrganizationService service, ITracingService tracingservice, List<PurchaseOrderProduct> lstPurchaseOrderProduct, Guid newlyCreatedPurchaseOrderReceiptGuid)
        {

            foreach (PurchaseOrderProduct purchaseOrderProduct in lstPurchaseOrderProduct)
            {
                Entity updatePurchaseOrderProductEntity = new Entity("msdyn_purchaseorderproduct");
                updatePurchaseOrderProductEntity.Id = purchaseOrderProduct.guid;
                updatePurchaseOrderProductEntity["ap360_purchaseorderreceiptid"] = new EntityReference("msdyn_purchaseorderreceipt", newlyCreatedPurchaseOrderReceiptGuid);
                updatePurchaseOrderProductEntity["ap360_partialreceivedquantity"] = purchaseOrderProduct.QuantityRemaining;
                service.Update(updatePurchaseOrderProductEntity);

            }
        }

        public static void UpdatePurchaseOrderProductPartNumber(IOrganizationService service, ITracingService tracingservice, string partNumber, Guid purchaseOrderProductGuid)
        {

            tracingservice.Trace("inside UpdatePurchaseOrderProductPartNumber");
            Entity updatePurchaseOrderProduct = new Entity("msdyn_purchaseorderproduct", purchaseOrderProductGuid);
            updatePurchaseOrderProduct["ap360_partnumber"] = partNumber;
            service.Update(updatePurchaseOrderProduct);


        }


        public static EntityCollection getPendingPurchaseOrderProductRelatedToWorkOrder(IOrganizationService service, ITracingService tracingService, Guid workOrderGuid)
        {

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='msdyn_purchaseorderproduct'>
                                    <attribute name='createdon' />
                                    <attribute name='msdyn_quantity' />
                                    <attribute name='msdyn_purchaseorder' />
                                    <attribute name='msdyn_product' />
                                    <attribute name='msdyn_purchaseorderproductid' />
                                    <attribute name='ap360_workordersubletid' />
                                    <attribute name='ap360_workorderproductid' />
                                    <attribute name='msdyn_associatetoworkorder' />
                                    <order attribute='msdyn_purchaseorder' descending='true' />
                                    <filter type='and'>
                                      <condition attribute= 'msdyn_associatetoworkorder' operator= 'eq'  value='" + workOrderGuid + @"' /> 
                                      <condition attribute='ap360_itemsubstatus' operator='eq' value='126300002' />
                                    </filter>
                                  </entity>
                                </fetch>");

            //<condition attribute='msdyn_itemstatus' operator='eq' value='690970000' />
            //Item Status  = Pending  690970000
            //Item subStatus  = Pending  126300002

            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));


            return col;
        }

        public static Entity RetrievePOPOnBaseOfWOP(IOrganizationService service, ITracingService tracingService, string workOrderProductGUID)
        {

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='msdyn_purchaseorderproduct'>
                                <attribute name='msdyn_purchaseorder' />
                                <attribute name='msdyn_product' />
                                <attribute name='msdyn_purchaseorderproductid' />
                                <order attribute='msdyn_purchaseorder' descending='true' />
                                <filter type='and'>
                                  <condition attribute='ap360_workorderproductid' operator='eq' uiname='' uitype='msdyn_workorderproduct' value='" + workOrderProductGUID + @"' />
                                </filter>
                              </entity>
                            </fetch>");


            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            Entity pop = null;
            if (col.Entities.Count > 0)
            {
                tracingService.Trace("Count " + col.Entities.Count.ToString());
                return col[0];
            }

            return pop;

        }

        public static EntityCollection RetrivePOPBasedOnProduct(IOrganizationService service, ITracingService tracingService, Guid productGuid)
        {

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                          <entity name='msdyn_purchaseorderproduct'>
                            <attribute name='createdon' />                         
                            <order attribute='msdyn_purchaseorder' descending='true' />
                            <filter type='and'>
                              <condition attribute='msdyn_product' operator='eq' uiname='' uitype='product' value='" + productGuid + @"' />
                            </filter>
                          </entity>
                        </fetch>");


            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (col.Entities.Count < 1)
            {
                tracingService.Trace("Count " + col.Entities.Count.ToString());
                return null;
            }
            return col;
        }

        public static EntityCollection RetrievePOPOnBaseOfPO(IOrganizationService service, ITracingService tracingService, string purchaseOrderGUID)
        {

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='msdyn_purchaseorderproduct'>
                                    <attribute name='createdon' />
                                    <attribute name='ap360_itemsubstatus' />
                                    <attribute name='msdyn_itemstatus' />
                                    <attribute name='ap360_workorderproductid' />
                                    <attribute name='ap360_vendorid' />
                                    <order attribute='msdyn_purchaseorder' descending='true' />
                                    <filter type='and'>
                                      <condition attribute='msdyn_purchaseorder' operator='eq'  uitype='msdyn_purchaseorder' value='" + purchaseOrderGUID + @"' />
                                    </filter>
                                  </entity>
                                </fetch>");


            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            
            return col;

        }       

    }
}
