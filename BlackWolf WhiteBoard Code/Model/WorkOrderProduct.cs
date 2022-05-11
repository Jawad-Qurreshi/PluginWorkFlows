using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code.Model
{
    public class WorkOrderProduct
    {
        public Guid guid { get; set; }
        public Money Originalestimateamount { get; set; }
        public Money ActualAmount { get; set; }
        public Money Revisedestimateamount { get; set; }

        public Money TotalOriginalestimateamount { get; set; }
        public Money TotalActualAmount { get; set; }
        public Money TotalRevisedestimateamount { get; set; }
        public string Manufacturer { get; set; }
        public int SKU { get; set; }
        public string BarCode { get; set; }
        public EntityReference PreferredSupplier { get; set; }
        public EntityReference UOM { get; set; }

        public bool IsRevised { get; set; }
        public Guid PurchaseOrderProductGuid { get; set; }
        public EntityReference ProductField { get; set; }
        public string PartNumber { get; set; }
        public double Quantity { get; set; }
        public EntityReference Vendor { get; set; }
        public EntityReference WorkOrder { get; set; }
        public EntityReference Opportuntiy { get; set; }
        public Money PartCost { get; set; }
        public static decimal CreateWorkOrderProducts(IOrganizationService service, ITracingService tracingservice, List<QuoteProduct> lstQuoteProduct, Guid workOrderGuid, Quote quote, IPluginExecutionContext context, Guid firstQuoteServiceTaskGuid, List<WOSTandQSTObject> lstWOSTandQSTObject)
        {
            decimal originalestimateamount = 0;
            if (lstQuoteProduct.Count > 0)
            {
                tracingservice.Trace(lstQuoteProduct.Count.ToString() + " Quote Product");
                var groupedQuoteProducts = lstQuoteProduct.GroupBy(x => x.Vendor);

                //foreach (var eachquoteProductGroup in groupedQuoteProducts)
                //{

                //  foreach (var eachquoteProductGroup in lstQuoteProduct)
                // {
                tracingservice.Trace("Start Foreach");
                // List<QuoteProduct> groupedlstQuoteProduct = new List<QuoteProduct>();
                // groupedlstQuoteProduct = eachquoteProductGroup.ToList();

                //////////////////////////////////////////////////

                // EntityReference vendorRef = eachquoteProductGroup.Key;

                Entity accountEntity = null;
                int vendorBillingType = 0;
                //if (vendorRef != null)
                //{
                //    accountEntity = service.Retrieve(vendorRef.LogicalName, vendorRef.Id, new ColumnSet("ap360_vendorbillingtype"));
                //    if (accountEntity != null)
                //    {
                //        vendorBillingType = accountEntity.GetAttributeValue<OptionSetValue>("ap360_vendorbillingtype") != null ? accountEntity.GetAttributeValue<OptionSetValue>("ap360_vendorbillingtype").Value : 0;
                //    }
                //}
                tracingservice.Trace("before createPOAndPOPAndWOP ");
                /////////////////////////////////////////////
                //if (accountEntity != null)
                //{
                originalestimateamount += PurchaseOrder.createPOAndPOPAndWOP(service, tracingservice, lstQuoteProduct, workOrderGuid, quote, vendorBillingType, context.UserId, firstQuoteServiceTaskGuid, lstWOSTandQSTObject);
                //}
                // else
                // {
                // context.OutputParameters["isAllQuoteServicesConverted"] = "error";
                //throw new InvalidPluginExecutionException("Vendor need to identified in One of Quote Product");
                // }
                // throw new InvalidPluginExecutionException("Custom Error");
                tracingservice.Trace("End Foreach");

                //}
                tracingservice.Trace("foreach function completed");

            }
            return originalestimateamount;

        }

        /*public static decimal CreateWorkOrderProducts(IOrganizationService service, ITracingService tracingservice, List<QuoteProduct> lstQuoteProduct, Guid workOrderGuid)
        {
            tracingservice.Trace(lstQuoteProduct.Count.ToString());

            tracingservice.Trace("Inside Creation of WorkOrder Product");

            decimal originalestimateamount = 0;
            int i = 1;
            foreach (QuoteProduct quoteProduct in lstQuoteProduct)
            {
                tracingservice.Trace(i.ToString() + " quoteproduct is in foreach");
                Entity entity = new Entity("msdyn_workorderproduct");

                entity["msdyn_product"] = new EntityReference("product", Product.createProduct(service, tracingservice, quoteProduct));
                entity["msdyn_description"] = quoteProduct.Name;
                entity["ap360_isrevised"] = false;

                //  entity["ap360_partno"] = quoteProduct.PartNo;
                //  entity["msdyn_estimatequantity"] = quoteProduct.Quantity;
                //  entity["msdyn_estimatetotalcost"] = quoteProduct.UnitCost;
                tracingservice.Trace("Parts Sale Price " + quoteProduct.PartsSalePrice.Value.ToString());
                entity["ap360_originalestimateamount"] = quoteProduct.PartsSalePrice;
                originalestimateamount = originalestimateamount + quoteProduct.PartsSalePrice.Value;
                tracingservice.Trace("Orignal Estimated Amount " + originalestimateamount.ToString());
                //   entity["ap360_refinv"] = quoteProduct.RefInv;
                //  entity["ap360_core"] = quoteProduct.Core;
                if (quoteProduct.Vendor != null) { }
                // entity["ap360_vendorid"] = new EntityReference("vendor", quoteProduct.Vendor.Id);
                entity["msdyn_workorder"] = new EntityReference("msdyn_workorder", workOrderGuid);
                // entity["ap360_workorderserviceid"] = new EntityReference("msdyn_workorderservice", workOrderServiceGuid);




                entity["msdyn_unit"] = new EntityReference("uom", new Guid("361a3eac-749c-4bb3-92a2-d63f692f61ba"));//UNIT

                Guid guid = service.Create(entity);
                tracingservice.Trace("Newly Created Guid " + guid);
                i++;

            }
            return originalestimateamount;


        }*/

        public static EntityCollection getWorkOrderProductsRelatedToWorkOrder(IOrganizationService service, ITracingService tracingService, Guid workOrderGuid)
        {

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='msdyn_workorderproduct'>
                                <attribute name='createdon' />
                                <attribute name='msdyn_unit' />
                                <attribute name='msdyn_name' />
                                <attribute name='msdyn_linestatus' />
                                <attribute name='ap360_product' />
                                <attribute name='ap360_partnumber' />
                                <attribute name='msdyn_description' />
                                <attribute name='msdyn_workorderproductid' />
                                <order attribute='msdyn_name' descending='false' />
                                <filter type='and'>
                                  <condition attribute='msdyn_workorder' operator='eq'   value='" + workOrderGuid + @"' />
                                </filter>
                              </entity>
                            </fetch>");



            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            return col;
        }

        public static WorkOrderProduct getWorkOrderProduct(IOrganizationService service, ITracingService tracingService, Guid workOrderProductGuid)
        {
            Entity reterivedWorkOrderProduct = service.Retrieve("msdyn_workorderproduct", workOrderProductGuid, new ColumnSet(
                "ap360_product", "ap360_partnumber", "ap360_vendorid",
                "ap360_quantityremaining", "msdyn_workorder", "ap360_opportunityid", "msdyn_estimateunitcost"));
            WorkOrderProduct workOrderProduct = null;
            if (reterivedWorkOrderProduct != null)
            {
                workOrderProduct = new WorkOrderProduct();
                workOrderProduct.ProductField = reterivedWorkOrderProduct.GetAttributeValue<EntityReference>("ap360_product") != null ? reterivedWorkOrderProduct.GetAttributeValue<EntityReference>("ap360_product") : null;
                workOrderProduct.PartNumber = reterivedWorkOrderProduct.GetAttributeValue<string>("ap360_partnumber");
                workOrderProduct.Vendor = reterivedWorkOrderProduct.GetAttributeValue<EntityReference>("ap360_vendorid") != null ? reterivedWorkOrderProduct.GetAttributeValue<EntityReference>("ap360_vendorid") : null;
                // workOrderProduct.Originalestimateamount = reterivedWorkOrderProduct.GetAttributeValue<Money>("ap360_originalestimateamount") != null ? reterivedWorkOrderProduct.GetAttributeValue<Money>("ap360_originalestimateamount") : null;
                // workOrderProduct.Revisedestimateamount = reterivedWorkOrderProduct.GetAttributeValue<Money>("ap360_revisedestimateamount") != null ? reterivedWorkOrderProduct.GetAttributeValue<Money>("ap360_revisedestimateamount") : null;
                workOrderProduct.Quantity = reterivedWorkOrderProduct.GetAttributeValue<double>("ap360_quantityremaining");
                workOrderProduct.WorkOrder = reterivedWorkOrderProduct.GetAttributeValue<EntityReference>("msdyn_workorder") != null ? reterivedWorkOrderProduct.GetAttributeValue<EntityReference>("msdyn_workorder") : null;
                workOrderProduct.PartCost = reterivedWorkOrderProduct.GetAttributeValue<Money>("msdyn_estimateunitcost") != null ? reterivedWorkOrderProduct.GetAttributeValue<Money>("msdyn_estimateunitcost") : null;
                workOrderProduct.Opportuntiy = reterivedWorkOrderProduct.GetAttributeValue<EntityReference>("ap360_opportunityid") != null ? reterivedWorkOrderProduct.GetAttributeValue<EntityReference>("ap360_opportunityid") : null;
                workOrderProduct.guid = reterivedWorkOrderProduct.Id;

            }
            return workOrderProduct;
        }
        public static EntityCollection getWorkOrderProductsRelatedToProduct(IOrganizationService service, ITracingService tracingService, Guid productGuid)
        {

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='msdyn_workorderproduct'>
                                    <attribute name='createdon' />
                                    <attribute name='msdyn_unit' />
                                    <attribute name='msdyn_name' />
                                    <attribute name='msdyn_linestatus' />
                                    <attribute name='msdyn_description' />
                                    <attribute name='ap360_row' />
                                    <attribute name='ap360_bin' />

                                    <attribute name='msdyn_workorderproductid' />
                                    <order attribute='msdyn_name' descending='false' />
                                    <filter type='and'>
                                      <condition attribute='ap360_product' operator='eq'  value='" + productGuid + @"' /> 
                                    </filter>
                                  </entity>
                                </fetch>");



            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));


            return col;
        }
        public static List<Entity> GetWOPRelatedToWOST(IOrganizationService service, ITracingService tracingService, Guid wostGuid)
        {



            List<Entity> lstEntities = new List<Entity>();

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='msdyn_workorderproduct'>
                                    <attribute name='createdon' />
                                    <attribute name='msdyn_unit' />
                                    <attribute name='msdyn_name' />
                                    <attribute name='msdyn_linestatus' />
                                    <attribute name='msdyn_description' />
                                    <attribute name='msdyn_workorderproductid' />
                                    <attribute name='ap360_workorderproductstatus' />
                                    <order attribute='msdyn_name' descending='false' />
                                    <filter type='and'>
                                      <condition attribute='ap360_workorderservicetaskid' operator='eq'  value='" + wostGuid + @"' /> 
                                    </filter>
                                  </entity>
                                </fetch>");




            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));

            foreach (Entity ent in col.Entities)
            {

                lstEntities.Add(ent);

            }
            return lstEntities;
        }

        public static void updateWOProductForReturn(IOrganizationService service, ITracingService tracing, Entity workOrderProduct)
        {
            double quantityReturn = 0;
            double estimateQuantity = 0;
            double quantityremaining = 0;
            Entity updateWOProductForNotUsed = new Entity("msdyn_workorderproduct");
            updateWOProductForNotUsed.Id = workOrderProduct.Id;
            // updateWOProductForNotUsed["msdyn_linestatus"] = new OptionSetValue(126300000);//Not Used
            updateWOProductForNotUsed["msdyn_allocated"] = false;

            if (workOrderProduct.Contains("msdyn_estimatequantity"))
            {
                estimateQuantity = workOrderProduct.GetAttributeValue<double>("msdyn_estimatequantity");
            }
            if (workOrderProduct.Contains("ap360_quantityreturn"))
            {
                quantityReturn = workOrderProduct.GetAttributeValue<double>("ap360_quantityreturn");
            }
            quantityremaining = workOrderProduct.GetAttributeValue<double>("ap360_quantityremaining");

            updateWOProductForNotUsed["ap360_quantityremaining"] = quantityremaining - quantityReturn;
            decimal ap360_actualamount = workOrderProduct.GetAttributeValue<Money>("ap360_actualamount") != null ? workOrderProduct.GetAttributeValue<Money>("ap360_actualamount").Value : 0;
            decimal eachPartPrice = (ap360_actualamount / Convert.ToDecimal(quantityremaining));
            // throw new InvalidPluginExecutionException("Actual Amount "+ ap360_actualamount.ToString()+" Each Price"+ eachPartPrice.ToString()+ " Acutal Quantity  "+ estimateQuantity.ToString() +" return qty "+ quantityReturn);
            tracing.Trace("Actual Amount " + ap360_actualamount.ToString());
            //eachPartPrice * Convert.ToDecimal((estimateQuantity - quantityReturn));//
            updateWOProductForNotUsed["ap360_actualamount"] = new Money(eachPartPrice * Convert.ToDecimal((quantityremaining - quantityReturn)));

            //decimal msdyn_estimateunitamount = workOrderProduct.GetAttributeValue<Money>("msdyn_estimateunitamount") != null ? workOrderProduct.GetAttributeValue<Money>("msdyn_estimateunitamount").Value : 0;

            //if (workOrderProduct.GetAttributeValue<Money>("ap360_originalestimateamount") != null)
            //{
            //    updateWOProductForNotUsed["ap360_originalestimateamount"] = new Money(msdyn_estimateunitamount * Convert.ToDecimal((quantityremaining - quantityReturn)));
            //}
            //else if (workOrderProduct.GetAttributeValue<Money>("ap360_revisedestimateamount") != null)
            //{
            //    updateWOProductForNotUsed["ap360_revisedestimateamount"] = new Money(msdyn_estimateunitamount * Convert.ToDecimal((quantityremaining - quantityReturn)));
            //}


            // updateWOProductForNotUsed["ap360_quantityreturn"] = 0.0;
            // tracing.Trace("Divided Actual Amount " + new Money(eachPartPrice * Convert.ToDecimal((estimateQuantity - quantityReturn))).Value.ToString());
            //  decimal ap360_revisedestimateamount = workOrderProduct.GetAttributeValue<Money>("ap360_revisedestimateamount") != null ? workOrderProduct.GetAttributeValue<Money>("ap360_revisedestimateamount").Value : 0;

            //updateWOProductForNotUsed["ap360_actualamount"] = new Money(ap360_actualamount);
            //updateWOProductForNotUsed["ap360_revisedestimateamount"] = new Money(ap360_revisedestimateamount);

            service.Update(updateWOProductForNotUsed);
            tracing.Trace("before reatewoProductReturnLog");
            WOProductReturnLog.createwoProductReturnLog(service, tracing, workOrderProduct, quantityReturn);
            tracing.Trace("after reatewoProductReturnLog");
        }
        public static WorkOrderProduct GetWorkOrderProductAmount(IOrganizationService service, ITracingService tracingservice, Guid workOrderGuid)
        {

            tracingservice.Trace("Inside GetWorkOrderProductAmount " + workOrderGuid.ToString());
            List<WorkOrderProduct> lstWorkOrderProduct = new List<WorkOrderProduct>();

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='msdyn_workorderproduct'>
                                <attribute name='msdyn_workorderproductid' />
                                <attribute name='ap360_revisedestimateamount' />

                                <attribute name='ap360_actualamount' />

                                <attribute name='ap360_originalestimateamount' />
                                <attribute name='ap360_isrevised' />


                                <attribute name='createdon' />
                                <order attribute='createdon' descending='false' />
                                    <filter type='and'>
                                      <condition attribute='msdyn_workorder' operator='eq'  value='" + workOrderGuid + @"' /> 
                                      <condition attribute='msdyn_linestatus' operator='ne' value='126300000' />
                                    </filter>
                                  </entity>
                                </fetch>");

            //operator='ne' value='126300000'  NOt Used

            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            WorkOrderProduct workOrderProduct = new WorkOrderProduct();

            decimal revisedestimatedAmount = 0;
            tracingservice.Trace("Total " + col.Entities.Count.ToString());
            tracingservice.Trace("before");

            int i = 1;
            decimal sumOriginalestimateamount = 0;
            decimal sumactualamount = 0;
            decimal sumRevisedestimateamount = 0;
            foreach (Entity entity in col.Entities)
            {
                // workOrderServiceTask.WOSTGuid.Id = entity.Id;
                tracingservice.Trace(i.ToString() + " is in progress");

                bool isRevised = false;
                isRevised = entity.GetAttributeValue<bool>("ap360_isrevised");
                Money moneyOriginalestimateamount = entity.GetAttributeValue<Money>("ap360_originalestimateamount") != null ? entity.GetAttributeValue<Money>("ap360_originalestimateamount") : null;
                Money moneyactualamount = entity.GetAttributeValue<Money>("ap360_actualamount") != null ? entity.GetAttributeValue<Money>("ap360_actualamount") : null;
                Money moneyRevisedestimateamount = entity.GetAttributeValue<Money>("ap360_revisedestimateamount") != null ? entity.GetAttributeValue<Money>("ap360_revisedestimateamount") : null;

                if (moneyOriginalestimateamount != null)
                {
                    sumOriginalestimateamount += moneyOriginalestimateamount.Value;
                    tracingservice.Trace("Original " + moneyOriginalestimateamount.Value.ToString());
                }
                if (moneyactualamount != null)
                {
                    sumactualamount += moneyactualamount.Value;
                    tracingservice.Trace("actual " + moneyactualamount.Value.ToString());
                }
                if (moneyRevisedestimateamount != null)
                {
                    sumRevisedestimateamount += moneyRevisedestimateamount.Value;
                    tracingservice.Trace("Revised " + moneyRevisedestimateamount.Value.ToString());

                }

                workOrderProduct.TotalOriginalestimateamount = new Money(sumOriginalestimateamount);
                workOrderProduct.TotalActualAmount = new Money(sumactualamount);
                workOrderProduct.TotalRevisedestimateamount = new Money(sumRevisedestimateamount);


                //  tracingservice.Trace("Sum " + revisedestimatedAmount.ToString());
                i++;


            }
            tracingservice.Trace("Error");
            //tracingservice.Trace("Sum of Total Orginal Estimated parts Amount " + workOrderProduct.TotalOriginalestimateamount.Value.ToString());
            //tracingservice.Trace("Sum of Total  Actual parst amount " + workOrderProduct.TotalActualAmount.Value.ToString());
            //tracingservice.Trace("Sum of Total Revised Estimated parts amount " + workOrderProduct.TotalRevisedestimateamount.Value.ToString());
            return workOrderProduct;

        }

        public static decimal CreateWorkOrderProductAndPurchaseOrderProduct(IOrganizationService service, ITracingService tracing, QuoteProduct quoteproduct, Guid newlyCreatedpurchaseOrderGuid, Guid workOrderGuid, Quote quote, string isblackWolfInventory, ref decimal originalestimateamount, Guid firstQuoteServiceTaskGuid, List<WOSTandQSTObject> lstWOSTandQSTObject)
        {

            ////////////////////////////////////////////////////////////////////
            //workOrderProduct["msdyn_estimatequantity"] = quoteproduct.Quantity;
            //workOrderProduct["msdyn_quantity"] = quoteproduct.Quantity;
            //tracing.Trace("Quantity " + quoteproduct.Quantity.ToString());
            //if (quoteproduct.UnitCost != null)
            //{
            //    workOrderProduct["msdyn_unitcost"] = new Money(quoteproduct.UnitCost.Value);
            //    tracing.Trace("Unit Cost " + quoteproduct.UnitCost.Value.ToString());

            //}
            //if (quoteproduct.UnitCost != null)
            //    workOrderProduct["msdyn_estimateunitcost"] = new Money(quoteproduct.UnitCost.Value);
            //if (quoteproduct.PartsSalePrice != null)
            //{
            //    workOrderProduct["msdyn_unitamount"] = quoteproduct.PartsSalePrice.Value;
            //    tracing.Trace("Parts Sale Price " + quoteproduct.PartsSalePrice.Value.ToString());
            //    workOrderProduct["ap360_originalestimateamount"] = quoteproduct.PartsSalePrice.Value;
            //    originalestimateamount = originalestimateamount + quoteproduct.PartsSalePrice.Value;
            //}
            //if (quoteproduct.UnitPrice != null)
            //{
            //    workOrderProduct["msdyn_estimateunitamount"] = new Money(quoteproduct.UnitPrice.Value);
            //    workOrderProduct["msdyn_totalamount"] = new Money(quoteproduct.UnitPrice.Value);
            //    tracing.Trace("Unit Amount " + quoteproduct.UnitPrice.Value.ToString());



            //}

            ///////////////////////////////////////////////////////////////////////////////
            //"
            // tracing.Trace("Inside fuction "+quoteproduct.QST.Id.ToString() +"  sdfsd");
            WOSTandQSTObject wOSTandQSTObject = null;
            if (quoteproduct.QST != null && lstWOSTandQSTObject.Count > 0)
            {
                wOSTandQSTObject = new WOSTandQSTObject();
                wOSTandQSTObject = lstWOSTandQSTObject.First(x => x.QSTGuid == quoteproduct.QST.Id);
            }
            //  decimal originalestimateamount = 0;
            Entity workOrderProduct = new Entity("msdyn_workorderproduct");

            workOrderProduct["msdyn_name"] = quoteproduct.Name;

            if (wOSTandQSTObject != null)
            {
                tracing.Trace("WOST Guid " + wOSTandQSTObject.WOSTGuid.ToString() + " -- QST Guid  " + quoteproduct.QST.Id.ToString() + " " + quoteproduct.QST.Name);
                workOrderProduct["ap360_workorderservicetaskid"] = new EntityReference("msdyn_workorderservicetask", wOSTandQSTObject.WOSTGuid);

            }
            else
            {
                if (firstQuoteServiceTaskGuid != Guid.Empty)
                {
                    workOrderProduct["ap360_workorderservicetaskid"] = new EntityReference("msdyn_workorderservicetask", firstQuoteServiceTaskGuid);

                }
            }
            //-workOrderProduct["msdyn_description"] = quoteproduct.Name;

            // This is not in use because we are not creating purchase ORder from 7/28/2020. 
            //if (newlyCreatedpurchaseOrderGuid != Guid.Empty)//this will be empty in case of Black Wolf White Board Inventory
            //{
            //    workOrderProduct["ap360_purchaseorderid"] = new EntityReference("msdyn_purchaseorder", newlyCreatedpurchaseOrderGuid);
            //}
            workOrderProduct["ap360_isrevised"] = false;
            if (quoteproduct.Manufacturer != null)
                workOrderProduct["ap360_manufacturer"] = quoteproduct.Manufacturer;
            if (quoteproduct.BarCode != null)
                workOrderProduct["ap360_barcode"] = quoteproduct.BarCode;
            if (quoteproduct.Vendor != null)
                workOrderProduct["ap360_vendorid"] = new EntityReference("account", quoteproduct.Vendor.Id);
            if (quoteproduct.UOM != null)
                workOrderProduct["msdyn_unit"] = new EntityReference("uom", quoteproduct.UOM.Id);
            else
                workOrderProduct["msdyn_unit"] = new EntityReference("uom", new Guid("361a3eac-749c-4bb3-92a2-d63f692f61ba"));//UNIT


            if (quote.Opportunity != null)
            {
                workOrderProduct["ap360_opportunityid"] = new EntityReference("opportunity", quote.Opportunity.Id);
            }
            workOrderProduct["ap360_vendoridentified"] = true;
            workOrderProduct["ap360_reviseditemstatus"] = new OptionSetValue(126300001);//Approved

            workOrderProduct["ap360_iscustomerpart"] = quoteproduct.CustomerSupplied;
            workOrderProduct["msdyn_allocated"] = true;
            workOrderProduct["ap360_tyrebatterymarkup"] = quoteproduct.tyreBatteryMarkup;


            /////////////////////////////////////////////////////////////////////////////////////////////////
            // workOrderProduct["msdyn_allocated"] = true;
            if (isblackWolfInventory == "notblackwolfinventory")
            {
                workOrderProduct["ap360_workorderproductstatus"] = new OptionSetValue(126300000);//Authorized To Order
            }
            else if (isblackWolfInventory == "blackwolfinventoryorCustomerSupplied")
            {

                workOrderProduct["ap360_workorderproductstatus"] = new OptionSetValue(126300005);//Needs Release From Inventory
                if (quoteproduct.UnitPrice != null)
                {
                    if (quoteproduct.QuoteProductType != null && quoteproduct.QuoteProductType.Value == 126300001)//BlackWolf Inventory
                        workOrderProduct["ap360_actualamount"] = new Money(quoteproduct.UnitPrice.Value);
                }
            }
            if (quoteproduct.QuoteProductType != null)
                workOrderProduct["ap360_workorderproducttype"] = quoteproduct.QuoteProductType;

            workOrderProduct["msdyn_estimatequantity"] = quoteproduct.Quantity;
            workOrderProduct["ap360_quantityremaining"] = quoteproduct.Quantity;
            if (quoteproduct.UnitCost != null)
                workOrderProduct["msdyn_estimateunitcost"] = new Money(quoteproduct.UnitCost.Value);
            if (quoteproduct.Multipliler != null)
                workOrderProduct["ap360_multiplier"] = new Money(quoteproduct.Multipliler.Value);
            if (quoteproduct.PartsSalePrice != null)
            {
                workOrderProduct["msdyn_estimateunitamount"] = new Money(quoteproduct.PartsSalePrice.Value);
                tracing.Trace("Parts Sale Price" + quoteproduct.PartsSalePrice.Value.ToString());

            }
            if (quoteproduct.UnitPrice != null)
            {
                workOrderProduct["ap360_originalestimateamount"] = new Money(quoteproduct.UnitPrice.Value);
                workOrderProduct["msdyn_estimatesubtotal"] = new Money(quoteproduct.UnitPrice.Value);

                tracing.Trace("Unit Price" + quoteproduct.UnitPrice.Value.ToString());
            }
            if (quoteproduct.QuoteProductType != null)
            {
                if (quoteproduct.QuoteProductType.Value == 126300000)//Customer Supplied
                {
                    workOrderProduct["ap360_originalestimateamount"] = null;

                }

            }
            ///////////////////////////////////////////////////////////////////////////////////////////////////
            if (quoteproduct.PartsSalePrice != null)
            {
                originalestimateamount = originalestimateamount + quoteproduct.PartsSalePrice.Value;
            }
            if (quoteproduct.PartNo != null)
            {
                workOrderProduct["ap360_partnumber"] = quoteproduct.PartNo;

            }
            if (quoteproduct.ParentServiceTask != null)
                workOrderProduct["ap360_parentservicetaskid"] = new EntityReference("msdyn_servicetasktype", quoteproduct.ParentServiceTask.Id);
            if (quoteproduct.GGParent != null)
                workOrderProduct["ap360_ggparent"] = new EntityReference("product", quoteproduct.GGParent.Id);
            if (quoteproduct.GParent != null)
                workOrderProduct["ap360_gparent"] = new EntityReference("product", quoteproduct.GParent.Id);
            if (quoteproduct.Parent != null)
                workOrderProduct["ap360_parent"] = new EntityReference("product", quoteproduct.Parent.Id);
            if (quoteproduct.Child != null)
                workOrderProduct["ap360_child"] = new EntityReference("product", quoteproduct.Child.Id);
            if (quoteproduct.ProductRef != null)
                workOrderProduct["ap360_product"] = new EntityReference("product", quoteproduct.ProductRef.Id);
            if (quoteproduct.ProductFamily != null)
                workOrderProduct["ap360_productfamily"] = new EntityReference("product", quoteproduct.ProductFamily.Id);
            workOrderProduct["ap360_name"] = quoteproduct.App360Name;
            //////////////////////////////////////////////////////////
            workOrderProduct["msdyn_quantity"] = quoteproduct.Quantity;


            workOrderProduct["ap360_sku"] = quoteproduct.SKU;
            if (quoteproduct.Vendor != null) { }
            // entity["ap360_vendorid"] = new EntityReference("vendor", quoteProduct.Vendor.Id);
            workOrderProduct["msdyn_workorder"] = new EntityReference("msdyn_workorder", workOrderGuid);
            // entity["ap360_workorderserviceid"] = new EntityReference("msdyn_workorderservice", workOrderServiceGuid);
            if (quoteproduct.ProductRef != null)
            {
                workOrderProduct["msdyn_product"] = new EntityReference("product", quoteproduct.ProductRef.Id);
            }
            else
            {
                throw new InvalidPluginExecutionException("Product is not selected in Quote Product: " + quoteproduct.Name);
            }

            if (quoteproduct.Warehouse != null)
            {
                workOrderProduct["msdyn_warehouse"] = new EntityReference("msdyn_warehouse", quoteproduct.Warehouse.Id);
            }
            else
            {
                throw new InvalidPluginExecutionException("Ware House is not selected in Quote Product: " + quoteproduct.Name);
            }
            Guid newlycreatedWorkOrderProductGuid = service.Create(workOrderProduct);

            //if (newlyCreatedpurchaseOrderGuid != Guid.Empty)//In case of  Black Wolf Vendor, no need to create Purchase Order and PO products
            //{
            //    PurchaseOrderProduct.CreatePurchaseOrderProduct(service, tracing, quoteproduct, newlyCreatedpurchaseOrderGuid, workOrderGuid, newlycreatedWorkOrderProductGuid);
            //    tracing.Trace("Purchase OrderProduct Created");
            //}

            return originalestimateamount;
        }


        public static void mapWOProducttoRevisedItem(IOrganizationService service, Entity reterviedEntity, RevisedItem revisedItem, ITracingService tracingService)
        {
            // revisedItem.Name = reterviedEntity.GetAttributeValue<string>("ap360_name");
            revisedItem.Name = reterviedEntity.GetAttributeValue<EntityReference>("ap360_product") != null ? reterviedEntity.GetAttributeValue<EntityReference>("ap360_product").Name : reterviedEntity.GetAttributeValue<string>("ap360_name");

            revisedItem.Quantity = reterviedEntity.GetAttributeValue<double>("msdyn_estimatequantity");
            revisedItem.UnitPrice = reterviedEntity.GetAttributeValue<Money>("msdyn_estimateunitamount") != null ? reterviedEntity.GetAttributeValue<Money>("msdyn_estimateunitamount") : null;
            revisedItem.ExtendedPrice = reterviedEntity.GetAttributeValue<Money>("ap360_revisedestimateamount") != null ? reterviedEntity.GetAttributeValue<Money>("ap360_revisedestimateamount") : null;
            revisedItem.WorkOrder = reterviedEntity.GetAttributeValue<EntityReference>("msdyn_workorder") != null ? reterviedEntity.GetAttributeValue<EntityReference>("msdyn_workorder") : null;
            revisedItem.Opportunity = reterviedEntity.GetAttributeValue<EntityReference>("ap360_opportunityid") != null ? reterviedEntity.GetAttributeValue<EntityReference>("ap360_opportunityid") : null;
            revisedItem.WOProduct = reterviedEntity.ToEntityReference();
            tracingService.Trace("Inside function which is not working");
            revisedItem.RevisedItemStatus = reterviedEntity.GetAttributeValue<OptionSetValue>("ap360_reviseditemstatus").Value;
            tracingService.Trace("Inside function which is not working after");
        }

        public static void CreateWOPforCore(IOrganizationService service, ITracingService tracingservice, Entity reterivedWOP, Entity coreproduct)
        {
            tracingservice.Trace("inside CreateWOPforCore");

            Guid productGuid;
            bool Familyhierarchy = reterivedWOP.GetAttributeValue<bool>("ap360_removeproductfamilyhierarchy");
            string partnumber = reterivedWOP.GetAttributeValue<string>("ap360_partnumber");
            double Estimatequantity = reterivedWOP.GetAttributeValue<double>("msdyn_estimatequantity");
            bool vendoridentified = reterivedWOP.GetAttributeValue<bool>("ap360_vendoridentified");
            bool customerpart = reterivedWOP.GetAttributeValue<bool>("ap360_iscustomerpart");
            EntityReference vendor = reterivedWOP.GetAttributeValue<EntityReference>("ap360_vendorid");
            Money estimatedunitcost = reterivedWOP.GetAttributeValue<Money>("msdyn_estimateunitcost");
            EntityReference product = reterivedWOP.GetAttributeValue<EntityReference>("ap360_product");
            EntityReference OOBProduct = reterivedWOP.GetAttributeValue<EntityReference>("msdyn_product");

            EntityReference ProductFamily = new EntityReference("product", new Guid("bbb4903e-75d7-eb11-bacb-000d3a31c760"));//Core Products

            EntityReference WorkOrder = reterivedWOP.GetAttributeValue<EntityReference>("msdyn_workorder");
            EntityReference Opportunity = reterivedWOP.GetAttributeValue<EntityReference>("ap360_opportunityid");
            string OOBName = reterivedWOP.GetAttributeValue<string>("msdyn_name");
            Money multiplier = reterivedWOP.GetAttributeValue<Money>("ap360_multiplier");
            Money estimateunitamount = reterivedWOP.GetAttributeValue<Money>("msdyn_estimateunitamount");
            Money estimatesubtotal = reterivedWOP.GetAttributeValue<Money>("msdyn_estimatesubtotal");
            EntityReference warehouse = reterivedWOP.GetAttributeValue<EntityReference>("msdyn_warehouse");
            EntityReference unit = reterivedWOP.GetAttributeValue<EntityReference>("msdyn_unit");
            //Money originalestimateamount = reterivedWOP.GetAttributeValue<Money>("ap360_originalestimateamount");
            OptionSetValue linestatus = reterivedWOP.GetAttributeValue<OptionSetValue>("msdyn_linestatus");
            //  throw new InvalidPluginExecutionException("update Error");
            // Money revisedestimateamount = reterivedWOP.GetAttributeValue<Money>("ap360_revisedestimateamount");
            double quantityremaining = reterivedWOP.GetAttributeValue<double>("ap360_quantityremaining");
            bool isrevised = reterivedWOP.GetAttributeValue<bool>("ap360_isrevised");
            Money coreamount = reterivedWOP.GetAttributeValue<Money>("ap360_amount");
            EntityReference opportunityRef = reterivedWOP.GetAttributeValue<EntityReference>("ap360_opportunityid");


            Entity WOPForCore = new Entity("msdyn_workorderproduct");
            WOPForCore["ap360_productfamily"] = ProductFamily;///Core Products
            WOPForCore["ap360_partnumber"] = partnumber + "-Core";
            WOPForCore["msdyn_estimatequantity"] = Estimatequantity;
            WOPForCore["ap360_vendoridentified"] = vendoridentified;
            WOPForCore["ap360_removeproductfamilyhierarchy"] = Familyhierarchy;
            if (opportunityRef != null)
                WOPForCore["ap360_opportunityid"] = opportunityRef;
            WOPForCore["ap360_iscustomerpart"] = customerpart;
            if (vendor != null)
                WOPForCore["ap360_vendorid"] = vendor;
            WOPForCore["msdyn_estimateunitcost"] = coreamount;
            if (product == null)
            {
                throw new InvalidPluginExecutionException("Core can't be created, Product is not selected in WorkOrder Product");
            }
            tracingservice.Trace("Product exists for core product");
            if (coreproduct == null)
            {
                productGuid = Product.createProductForWorkOrderProduct(service, tracingservice, OOBName + "-Core", ProductFamily, partnumber + "-Core", estimateunitamount);
                tracingservice.Trace("after createProductForWorkOrderProduct");

                Product.activateProduct(service, tracingservice, productGuid);

            }
            else
            {
                productGuid = coreproduct.Id;
            }
            WOPForCore["msdyn_product"] = new EntityReference("product", productGuid);
            WOPForCore["ap360_product"] = new EntityReference("product", productGuid);

            if (WorkOrder != null)
                WOPForCore["msdyn_workorder"] = new EntityReference(WorkOrder.LogicalName, WorkOrder.Id);
            if (Opportunity != null)
                WOPForCore["ap360_opportunityid"] = new EntityReference(Opportunity.LogicalName, Opportunity.Id);
            WOPForCore["msdyn_name"] = OOBName + "-Core";
            WOPForCore["ap360_name"] = OOBName + "-Core";

            //  WOPForCore["ap360_multiplier"] = multiplier;
            WOPForCore["msdyn_estimateunitamount"] = coreamount;
            WOPForCore["msdyn_estimatesubtotal"] = coreamount;
            if (warehouse != null)
                WOPForCore["msdyn_warehouse"] = new EntityReference(warehouse.LogicalName, warehouse.Id);
            if (unit != null)
                WOPForCore["msdyn_unit"] = new EntityReference(unit.LogicalName, unit.Id);
            // WOPForCore["ap360_originalestimateamount"] = originalestimateamount;
            WOPForCore["msdyn_linestatus"] = linestatus;
            WOPForCore["ap360_revisedestimateamount"] = coreamount;
            WOPForCore["ap360_quantityremaining"] = quantityremaining;
            WOPForCore["ap360_isrevised"] = isrevised;

            service.Create(WOPForCore);
        }



        public static void DeleteCoreWorkOrderProduct(IOrganizationService service, ITracingService tracingService, Guid workOrderGuid, string partNumber)
        {

            // string partnumber = "CNC-325";
            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                      <entity name='msdyn_workorderproduct'>
                                        <attribute name='msdyn_workorderproductid' />
                                        <attribute name='ap360_partnumber' />
                                        <order attribute='msdyn_name' descending='false' />
                                        <filter type='and'>
                                          <condition attribute='msdyn_workorder' operator= 'eq'  value='" + workOrderGuid + @"' />
                                          <condition attribute='ap360_partnumber' operator='like' value='%" + partNumber + @"-Core%' />
                                        </filter>
                                      </entity>
                                    </fetch>");

            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));


            if (col.Entities.Count > 0)
            {
                service.Delete(col.Entities[0].LogicalName, col.Entities[0].Id);

            }
        }

        public static EntityCollection getUnUsedWorkOrderProductsRelatedToWorkOrder(IOrganizationService service, ITracingService tracingService, Guid workOrderGuid)
        {

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='msdyn_workorderproduct'>
                                 
                                    <attribute name='msdyn_name' />
                                    <attribute name='msdyn_linestatus' />

                                 
                                    <attribute name='msdyn_workorderproductid' />
                                    <order attribute='msdyn_name' descending='false' />
                                    <filter type='and'>
                                       <condition attribute='msdyn_linestatus' operator='ne' value='690970001' />
                                      <condition attribute='msdyn_workorder' operator='eq'  value='" + workOrderGuid + @"' /> 
                                    </filter>
                                  </entity>
                                </fetch>");



            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));


            return col;
        }


    }

}


