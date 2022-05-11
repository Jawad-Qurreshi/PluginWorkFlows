using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code.Model
{
    public class PurchaseOrder
    {

        public Guid guid { get; set; }
        public string Name { get; set; }

        public string PartNo { get; set; }
        public double Quantity { get; set; }
        public Money UnitCost { get; set; }
        public Money UnitPrice { get; set; }
        public string RefInv { get; set; }
        public bool Core { get; set; }

        public static decimal createPOAndPOPAndWOP(IOrganizationService service, ITracingService tracingservice, List<QuoteProduct> lstQuoteProduct, Guid workOrderGuid, Quote quote, int vendorBillingType, Guid initiatingUserId,Guid firstQuoteServiceTaskGuid,List<WOSTandQSTObject> lstWOSTandQSTObject)
        {
            tracingservice.Trace("Inside createPOAndPOPAndWOP");
            decimal originalestimateamount = 0;

            Guid newlyCreatedpurchaseOrderGuid = Guid.Empty;
            Guid newlyCreatedPurchaseOrderReceiptGuid = Guid.Empty;
            Guid newlyCreatedPurchaseOrderProductGuid = Guid.Empty;
            if (lstQuoteProduct.Count > 0)
            {
                tracingservice.Trace(lstQuoteProduct.Count.ToString() + " Quote Products");



                //Entity purchaseOrder = new Entity("msdyn_purchaseorder");
                //tracingservice.Trace("Quote Opportunity Id " + quote.Opportunity.Id.ToString());
                //if (lstQuoteProduct[0].Vendor != null)
                //    tracingservice.Trace("Vendor Id " + lstQuoteProduct[0].Vendor.Id.ToString());
                ////else
                ////throw new InvalidPluginExecutionException("In one of Quote Product Vendor not exist ");
                //tracingservice.Trace("workOrder Guid " + workOrderGuid.ToString());
                //if (lstQuoteProduct[0].Vendor != null)
                //    purchaseOrder["msdyn_vendor"] = new EntityReference("account", lstQuoteProduct[0].Vendor.Id);
                //purchaseOrder["msdyn_purchaseorderdate"] = DateTime.Now.Date;
                //purchaseOrder["ap360_opportunityid"] = new EntityReference("opportunity", quote.Opportunity.Id);
                //purchaseOrder["transactioncurrencyid"] = new EntityReference("transactioncurrency", new Guid("d513407b-f54f-ea11-a814-000d3a30fcff"));//US Dollar
                //purchaseOrder["ownerid"] = new EntityReference("systemuser", initiatingUserId);

                ////////////////////////////////////////////////////////////////////
                //purchaseOrder["msdyn_systemstatus"] = new OptionSetValue(690970001);//submitted
                /////////////////////////////////////////////////////////////////////

                // *******************************
                //tracingservice.Trace("Vendor Billing Type = " + vendorBillingType.ToString());
                //if (vendorBillingType == 126300002)//On Accout 
                //{
                //    purchaseOrder["msdyn_substatus"] = new EntityReference("msdynpurchaseordersubstatus", new Guid("e"));//Draft-Billed on Account

                //}
                //else if (vendorBillingType == 126300001)// Draft-COD
                //{

                //}
                //else if (vendorBillingType == 126300000)// Pre-paid
                //{

                //}

                // *******************************

                //// purchaseOrder["msdyn_workorder"] = new EntityReference("msdyn_workorder", workOrderGuid);
                //purchaseOrder["ap360_workorderid"] = new EntityReference("msdyn_workorder", workOrderGuid);//New relationship created, not using defualt
                //                                                                                           //relationship because that is causing problem in product inventory, this relationship is mandatory to poplulate Purchase Order subgrid on
                //                                                                                           //workorder
                //newlyCreatedpurchaseOrderGuid = service.Create(purchaseOrder);
                //Entity purchaseOrderEntity = service.Retrieve("msdyn_purchaseorder", newlyCreatedpurchaseOrderGuid, new Microsoft.Xrm.Sdk.Query.ColumnSet("msdyn_name"));
                // newlyCreatedPurchaseOrderReceiptGuid = PurchaseOrderReceipt.CreatePurchaseOrderReceipt(service, tracingservice, purchaseOrderEntity);
                //if (vendorGuid.ToString().ToLower() == "a89044d7-9b9a-ea11-a811-000d3a33f3c3" || vendorGuid.ToString().ToLower() == "28b07b2b-4928-eb11-a813-000d3a368915")// eq to Black Wolf Inventory or CustomerSupplied
                //{
                //    tracingservice.Trace("Before foreach");

                //    foreach (QuoteProduct quoteproduct in lstQuoteProduct)
                //    {
                //        tracingservice.Trace(quoteproduct.Name);

                //        tracingservice.Trace("Before Work Order Product Creation and Purchase Order ");
                //        originalestimateamount = WorkOrderProduct.CreateWorkOrderProductAndPurchaseOrderProduct(service, tracingservice, quoteproduct, newlyCreatedpurchaseOrderGuid, workOrderGuid, quote, "blackwolfinventoryorCustomerSupplied",ref originalestimateamount, firstQuoteServiceTaskGuid, lstWOSTandQSTObject);
                //        tracingservice.Trace("Work OrderProductt Created");
                //        //    // PurchaseOrderReceiptProduct.CreatePurchaseOrderReceiptProduct(service, tracingservice, newlyCreatedpurchaseOrderGuid, ProductEntity, newlyCreatedPurchaseOrderReceiptGuid, newlyCreatedPurchaseOrderProductGuid, quoteproduct, workOrderGuid);
                //    }
                //}
                //else// if Vendor is Black Wolf then only create work Order not Purchase Order
                //{
                    foreach (QuoteProduct quoteproduct in lstQuoteProduct)
                    {
                        tracingservice.Trace("Before Work Order Product Creation With Out Purchase Order");
                        if (quoteproduct.QST != null)
                        {
                            tracingservice.Trace(quoteproduct.QST.Id.ToString());

                        }
                        tracingservice.Trace("***********************Quote product count "+lstQuoteProduct.Count.ToString());
                        originalestimateamount = WorkOrderProduct.CreateWorkOrderProductAndPurchaseOrderProduct(service, tracingservice, quoteproduct, Guid.Empty, workOrderGuid, quote, "notblackwolfinventory",ref originalestimateamount, firstQuoteServiceTaskGuid, lstWOSTandQSTObject);
                        tracingservice.Trace("Work OrderProductt Created with Out Purcahse Order");
                  //  }
                }

              //  throw new InvalidPluginExecutionException("Error");
            }

            return originalestimateamount;
        }
        public static void CreateSubletUnderOpportunity(IOrganizationService service, ITracingService tracingService, Entity entity, Money shippingFee, EntityReference vendorRef)
        {

            List<PurchaseOrderProduct> lstPurchaseOrderProducts = new List<PurchaseOrderProduct>();
            lstPurchaseOrderProducts = PurchaseOrderProduct.GetPurchaseOrderProductsForSubletCreation(service, tracingService, entity.Id);
            tracingService.Trace("Number of Purchase Order Products " + lstPurchaseOrderProducts.Count.ToString());
            var groupedPOProducts = lstPurchaseOrderProducts.GroupBy(x => x.workOrder_opportunity);

            tracingService.Trace("Number of Groups " + groupedPOProducts.Count().ToString());
            //throw new InvalidPluginExecutionException("Error");
            int count = 0;
            foreach (var eachgroupedPOProduct in groupedPOProducts)
            {
                count++;
                Guid opporutnityGuid = eachgroupedPOProduct.Key;
                tracingService.Trace(count.ToString() + " Opporutnity Guid " + opporutnityGuid.ToString());
                WorkOrderSublet.CreateorUpdateWorkOrderSubletForPurchaseOrderShippingFee(service, tracingService, entity, opporutnityGuid, shippingFee.Value, vendorRef);
            }
        }
        public static decimal createPOAndPOPAndWOSublet(IOrganizationService service, ITracingService tracingservice, List<QuoteSublet> lstQuoteSublet, Guid workOrderGuid, Quote quote, int vendorBillingType)
        {
            tracingservice.Trace("Inside createPOAndPOPAndWOSublet");
            decimal originalestimateSubletamount = 0;

            Guid newlyCreatedpurchaseOrderGuid = Guid.Empty;
            Guid newlyCreatedPurchaseOrderReceiptGuid = Guid.Empty;
            Guid newlyCreatedPurchaseOrderSubletGuid = Guid.Empty;

            if (lstQuoteSublet.Count > 0)
            {
                tracingservice.Trace(lstQuoteSublet.Count.ToString() + "Quote Sublets");

                Entity purchaseOrder = new Entity("msdyn_purchaseorder");
                tracingservice.Trace("Quote Opportunity Id " + quote.Opportunity.Id.ToString());
                if (lstQuoteSublet[0].Vendor != null)
                    tracingservice.Trace("Vendor Id " + lstQuoteSublet[0].Vendor.Id.ToString());
                //else
                //throw new InvalidPluginExecutionException("In one of Quote Product Vendor not exist ");
                tracingservice.Trace("workOrder Guid " + workOrderGuid.ToString());
                if (lstQuoteSublet[0].Vendor != null)
                    purchaseOrder["msdyn_vendor"] = new EntityReference("account", lstQuoteSublet[0].Vendor.Id);
                purchaseOrder["msdyn_purchaseorderdate"] = DateTime.Now.Date;
                purchaseOrder["ap360_opportunityid"] = new EntityReference("opportunity", quote.Opportunity.Id);

                ////////////////////////////////////////////////////////////////////
                //purchaseOrder["msdyn_systemstatus"] = new OptionSetValue(690970001);//submitted
                /////////////////////////////////////////////////////////////////////

                if (vendorBillingType == 126300002)//On Accout 
                {
                   // purchaseOrder["msdyn_substatus"] = new EntityReference("msdyn_purchaseordersubstatus", new Guid("f7b55fb5-4695-ea11-a811-000d3a33f47e"));//Draft-Billed on Account  (Old deleted value)
                  //  purchaseOrder["msdyn_substatus"] = new EntityReference("msdyn_purchaseordersubstatus", new Guid("958b57eb-6aba-eb11-8236-000d3a37fd6b"));//Draft-Billed on Account Sandbox
                    purchaseOrder["msdyn_substatus"] = new EntityReference("msdyn_purchaseordersubstatus", new Guid("10a254fa-6eba-eb11-8236-000d3a37f2ae"));//Draft-Billed on Account Prod


                }
                else if (vendorBillingType == 126300001)// Draft-COD
                {
                    // purchaseOrder["msdyn_substatus"] = new EntityReference("msdyn_purchaseordersubstatus", new Guid("88cb2b97-4695-ea11-a811-000d3a33f47e"));// Draft-COD  (Old deleted value)
                   // purchaseOrder["msdyn_substatus"] = new EntityReference("msdyn_purchaseordersubstatus", new Guid("91b5d303-6bba-eb11-8236-000d3a37fd6b"));// Draft-COD Sandbox
                    purchaseOrder["msdyn_substatus"] = new EntityReference("msdyn_purchaseordersubstatus", new Guid("ce2e9f0c-6fba-eb11-8236-000d3a37f2ae"));// Draft-COD Prod

                }
                else if (vendorBillingType == 126300000)// Pre-paid
                {
                    // purchaseOrder["msdyn_substatus"] = new EntityReference("msdyn_purchaseordersubstatus", new Guid("7e4725a9-4695-ea11-a811-000d3a33f47e"));//Draft-Pre-Paid  (Old deleted value)
                    //purchaseOrder["msdyn_substatus"] = new EntityReference("msdyn_purchaseordersubstatus", new Guid("94ac0f11-6bba-eb11-8236-000d3a37fd6b"));//Draft-Pre-Paid Sandbox
                    purchaseOrder["msdyn_substatus"] = new EntityReference("msdyn_purchaseordersubstatus", new Guid("c224e61a-6fba-eb11-8236-000d3a37f2ae"));//Draft-Pre-Paid prod

                }


                // purchaseOrder["msdyn_workorder"] = new EntityReference("msdyn_workorder", workOrderGuid);
                purchaseOrder["ap360_workorderid"] = new EntityReference("msdyn_workorder", workOrderGuid);//New relationship created, not using defualt
                                                                                                           //relationship because that is causing problem in product inventory, this relationship is mandatory to poplulate Purchase Order subgrid on
                                                                                                           //workorder
                newlyCreatedpurchaseOrderGuid = service.Create(purchaseOrder);

                Entity purchaseOrderEntity = service.Retrieve("msdyn_purchaseorder", newlyCreatedpurchaseOrderGuid, new Microsoft.Xrm.Sdk.Query.ColumnSet("msdyn_name"));
                if (purchaseOrderEntity != null)
                {
                    // newlyCreatedPurchaseOrderReceiptGuid = PurchaseOrderReceipt.CreatePurchaseOrderReceipt(service, tracingservice, purchaseOrderEntity);
                    //tracingservice.Trace("Purchase Order Prodcut Receipt Created");
                }
                tracingservice.Trace("Before foreach");



                foreach (QuoteSublet quoteSublet in lstQuoteSublet)
                {
                    tracingservice.Trace(quoteSublet.Name);

                    // Guid newlyCreatedProductGuid = Product.createProduct(service, tracingservice, quoteproduct);

                    // Entity ProductEntity = service.Retrieve("product", newlyCreatedProductGuid, new Microsoft.Xrm.Sdk.Query.ColumnSet("name"));

                    newlyCreatedPurchaseOrderSubletGuid = PurchaseOrderProduct.CreatePurchaseOrderProductForSublet(service, tracingservice, quoteSublet, newlyCreatedpurchaseOrderGuid, workOrderGuid);
                    tracingservice.Trace("Purchase OrderProduct Created");

                    tracingservice.Trace("Before Work Order Sublet Creation");
                    originalestimateSubletamount += WorkOrderSublet.CreateWorkOrderSubletFromQuoteSublet(service, tracingservice, quoteSublet, newlyCreatedpurchaseOrderGuid, workOrderGuid, quote);
                    tracingservice.Trace("Work Order Sublet Created");


                }

            }

            return originalestimateSubletamount;
        }


    }

}
