
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code.Model
{
    public class WorkOrderSublet
    {

        public Guid guid { get; set; }
        public Money OriginalestimatedSubletamount { get; set; }
        public Money ActualSubletAmount { get; set; }
        public Money RevisedestimatedSubletAmount { get; set; }

        public Money TotalOriginalestimateSubletAmount { get; set; }
        public Money TotalActualSubletAmount { get; set; }
        public Money TotalRevisedestimatedSubletAmount { get; set; }

        public static decimal CreateWorkOrderSublets(IOrganizationService service, ITracingService tracing, List<QuoteSublet> lstQuoteSublets, Guid workOrderGuid, Quote quote)
        {

            decimal originalestimatedSubletamount = 0;
            if (lstQuoteSublets.Count > 0)
            {
                tracing.Trace(lstQuoteSublets.Count.ToString() + " Quote Sublets");
                var groupedQuoteSublets = lstQuoteSublets.GroupBy(x => x.Vendor);

                foreach (var eachquoteSubletGroup in groupedQuoteSublets)
                {
                    tracing.Trace("Start Foreach");
                    tracing.Trace("Thanks");
                    List<QuoteSublet> groupedlstQuoteSublet = new List<QuoteSublet>();
                    groupedlstQuoteSublet = eachquoteSubletGroup.ToList();

                    //////////////////////////////////////////////////

                    EntityReference vendorRef = eachquoteSubletGroup.Key;


                    Entity accountEntity = null;
                    int vendorBillingType = 0;
                    if (vendorRef != null)
                    {
                        accountEntity = service.Retrieve(vendorRef.LogicalName, vendorRef.Id, new ColumnSet("ap360_vendorbillingtype"));
                        if (accountEntity != null)
                        {
                            vendorBillingType = accountEntity.GetAttributeValue<OptionSetValue>("ap360_vendorbillingtype") != null ? accountEntity.GetAttributeValue<OptionSetValue>("ap360_vendorbillingtype").Value : 0;
                        }
                    }
                    /////////////////////////////////////////////
                    tracing.Trace("Before createPOAndPOPAndWOSublet");
                    originalestimatedSubletamount += PurchaseOrder.createPOAndPOPAndWOSublet(service, tracing, groupedlstQuoteSublet, workOrderGuid, quote, vendorBillingType);
                    tracing.Trace("End Foreach");

                }
                tracing.Trace("foreach function completed");

            }


            return originalestimatedSubletamount;
        }

        public static decimal CreateWorkOrderSubletFromQuoteSublet(IOrganizationService service, ITracingService tracing, QuoteSublet quoteSublet, Guid newlyCreatedpurchaseOrderGuid, Guid workOrderGuid, Quote quote)
        {
            decimal originalestimateSubletamount = 0;
            Entity newWorkOrderSublet = new Entity("ap360_workordersublet");
            newWorkOrderSublet["ap360_name"] = quoteSublet.Name;
            if (quoteSublet.Vendor != null)
                newWorkOrderSublet["ap360_vendorid"] = new EntityReference("account", quoteSublet.Vendor.Id);
            newWorkOrderSublet["ap360_subletdescription"] = quoteSublet.SubletDescription;
            if (quoteSublet.SalePrice != null)
            {
                newWorkOrderSublet["ap360_originalestimatedamount"] = quoteSublet.SalePrice;
                originalestimateSubletamount += originalestimateSubletamount + quoteSublet.SalePrice.Value;
            }
            if (quoteSublet.EstimatedAmount != null)
            {
                newWorkOrderSublet["ap360_subletcost"] = quoteSublet.EstimatedAmount;

            }
            tracing.Trace(quoteSublet.EstimateDeliveryDate.ToString());
            if (quoteSublet.EstimateDeliveryDate.ToString() != "1/1/0001 12:00:00 AM")//Empty Estimated Date
                newWorkOrderSublet["ap360_estimateddeliverydate"] = quoteSublet.EstimateDeliveryDate;
            newWorkOrderSublet["ap360_workorderid"] = new EntityReference("msdyn_workorder", workOrderGuid);
            if (quoteSublet.Product != null)
                newWorkOrderSublet["ap360_productid"] = new EntityReference("product", quoteSublet.Product.Id);
            newWorkOrderSublet["ap360_purchaseorderid"] = new EntityReference("msdyn_purchaseorder", newlyCreatedpurchaseOrderGuid);
            if (quote.Opportunity != null)
                newWorkOrderSublet["ap360_opportunityid"] = new EntityReference("opportunity", quote.Opportunity.Id);

            newWorkOrderSublet["ap360_reviseditemstatus"] = new OptionSetValue(126300001);//Approved

            service.Create(newWorkOrderSublet);
            return originalestimateSubletamount;

        }

        public static void CreateorUpdateWorkOrderSubletForPurchaseOrderShippingFee(IOrganizationService service, ITracingService tracing, Entity purchaseOrderEntity, Guid opporutnityGuid, decimal shippingfee, EntityReference vendorRef)
        {

            tracing.Trace("Inside CreateorUpdateWorkOrderSubletForPurchaseOrderShippingFee");
            WorkOrderSublet workOrderSublet = WorkOrderSublet.GetWorkOrderSubletForPurchaseOrder(service, tracing, purchaseOrderEntity.Id, opporutnityGuid);

            if (workOrderSublet != null)
            {
                tracing.Trace("If");

                tracing.Trace("Sublet is not Null");
                Entity updateWorkOrderSublet = new Entity("ap360_workordersublet");
                decimal markup = 1.3m;

                updateWorkOrderSublet["ap360_incidentalcosts"] = new Money(shippingfee * markup);
                updateWorkOrderSublet.Id = workOrderSublet.guid;
                service.Update(updateWorkOrderSublet);
                tracing.Trace("Record is updated");

            }
            else
            {
                tracing.Trace("else");

                tracing.Trace("Sublet is  Null");

                Entity newWorkOrderSublet = new Entity("ap360_workordersublet");
                newWorkOrderSublet["ap360_name"] = "Shipping Fee";
                newWorkOrderSublet["ap360_subletdescription"] = "Shipping Fee";
                if (vendorRef != null)
                {
                    newWorkOrderSublet["ap360_vendorid"] = new EntityReference("account", vendorRef.Id);

                }
                decimal markup = 1.3m;
                newWorkOrderSublet["ap360_incidentalcosts"] = new Money(shippingfee * markup);
                newWorkOrderSublet["ap360_subletcost"] = new Money(shippingfee);


                //newWorkOrderSublet["ap360_estimateddeliverydate"] = quoteSublet.EstimateDeliveryDate;
                //  if (workOrderGuid != null)
                //    newWorkOrderSublet["ap360_workorderid"] = new EntityReference("msdyn_workorder", workOrderGuid);
                // if (quoteSublet.Product != null)
                //   newWorkOrderSublet["ap360_productid"] = new EntityReference("product", quoteSublet.Product.Id);
                newWorkOrderSublet["ap360_purchaseorderid"] = new EntityReference("msdyn_purchaseorder", purchaseOrderEntity.Id);
                newWorkOrderSublet["ap360_opportunityid"] = new EntityReference("opportunity", opporutnityGuid);

                service.Create(newWorkOrderSublet);
                tracing.Trace("Record is created");

            }
        }

        public static WorkOrderSublet GetWorkOrderSubletForPurchaseOrder(IOrganizationService service, ITracingService tracing, Guid purchaseOrderGuid, Guid opportunityGuid)
        {


            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                                      <entity name='ap360_workordersublet'>
                                                        <attribute name='ap360_workordersubletid' />
                                                        <attribute name='ap360_name' />
                                                        <attribute name='createdon' />
                                                        <order attribute='ap360_name' descending='false' />
                                                        <filter type='and'>
                                                          <condition attribute='ap360_purchaseorderid' operator='eq'  value='" + purchaseOrderGuid + @"' /> 
                                                          <condition attribute='ap360_opportunityid' operator='eq'  value='" + opportunityGuid + @"' /> 

                                                            </filter>
                                                      </entity>
                                                    </fetch>");


            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            WorkOrderSublet workOrderSublet = null;
            if (col.Entities.Count > 0)
            {
                workOrderSublet = new WorkOrderSublet();
                workOrderSublet.guid = col.Entities[0].Id;

            }
            return workOrderSublet;

        }


        public static WorkOrderSublet GetWorkOrderSubletAmount(IOrganizationService service, ITracingService tracingservice, Guid workOrderGuid)
        {

            List<WorkOrderProduct> lstWorkOrderProduct = new List<WorkOrderProduct>();

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='ap360_workordersublet'>
                                <attribute name='ap360_workordersubletid' />
                                <attribute name='ap360_name' />
                                <attribute name='createdon' />
                                <attribute name='ap360_revisedestimatedamount' />
                                <attribute name='ap360_originalestimatedamount' />
                                <attribute name='ap360_actualamount' />
                                <order attribute='ap360_name' descending='false' />
                                <filter type='and'>
                                  <condition attribute='ap360_workorderid'  operator='eq'  value='" + workOrderGuid + @"' /> 
                                </filter>
                              </entity>
                            </fetch>");


            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            WorkOrderSublet workOrderSublet = new WorkOrderSublet();

            decimal revisedestimatedAmount = 0;
            tracingservice.Trace("Total " + col.Entities.Count.ToString());
            int i = 1;
            decimal sumOriginalestimateamount = 0;
            decimal sumactualamount = 0;
            decimal sumRevisedestimateamount = 0;
            foreach (Entity entity in col.Entities)
            {
                // workOrderServiceTask.WOSTGuid.Id = entity.Id;
                tracingservice.Trace(i.ToString() + " is in progress");


                Money moneyOriginalestimateamount = entity.GetAttributeValue<Money>("ap360_originalestimatedamount") != null ? entity.GetAttributeValue<Money>("ap360_originalestimatedamount") : null;
                Money moneyactualamount = entity.GetAttributeValue<Money>("ap360_actualamount") != null ? entity.GetAttributeValue<Money>("ap360_actualamount") : null;
                Money moneyRevisedestimateamount = entity.GetAttributeValue<Money>("ap360_revisedestimatedamount") != null ? entity.GetAttributeValue<Money>("ap360_revisedestimatedamount") : null;

                if (moneyOriginalestimateamount != null)
                    sumOriginalestimateamount += moneyOriginalestimateamount.Value;
                if (moneyactualamount != null)
                    sumactualamount += moneyactualamount.Value;
                if (moneyRevisedestimateamount != null)
                    sumRevisedestimateamount += moneyRevisedestimateamount.Value;

                workOrderSublet.TotalOriginalestimateSubletAmount = new Money(sumOriginalestimateamount);
                workOrderSublet.TotalActualSubletAmount = new Money(sumactualamount);
                workOrderSublet.TotalRevisedestimatedSubletAmount = new Money(sumRevisedestimateamount);


                tracingservice.Trace("Sum " + revisedestimatedAmount.ToString());
                i++;


            }

            tracingservice.Trace("Sum of Total Orginal Estimated Sublet Amount " + workOrderSublet.TotalOriginalestimateSubletAmount.Value.ToString());
            tracingservice.Trace("Sum of Total  Actual  Sublet amount " + workOrderSublet.TotalActualSubletAmount.Value.ToString());
            tracingservice.Trace("Sum of Total Revised Estimated Sublet amount " + workOrderSublet.TotalRevisedestimatedSubletAmount.Value.ToString());
            return workOrderSublet;

        }
        public static void mapWOSubletToRevisedItem(IOrganizationService service, Entity reterviedEntity, RevisedItem revisedItem)
        {

            revisedItem.Name = reterviedEntity.GetAttributeValue<string>("ap360_subletdescription");
            //revisedItem.Quantity = reterviedEntity.GetAttributeValue<double>("msdyn_estimatequantity");
            revisedItem.ExtendedPrice = reterviedEntity.GetAttributeValue<Money>("ap360_revisedestimatedamount") != null ? reterviedEntity.GetAttributeValue<Money>("ap360_revisedestimatedamount") : null;
            revisedItem.WOSublet = reterviedEntity.ToEntityReference();
            revisedItem.WorkOrder = reterviedEntity.GetAttributeValue<EntityReference>("ap360_workorderid") != null ? reterviedEntity.GetAttributeValue<EntityReference>("ap360_workorderid") : null;
            revisedItem.Opportunity = reterviedEntity.GetAttributeValue<EntityReference>("ap360_opportunityid") != null ? reterviedEntity.GetAttributeValue<EntityReference>("ap360_opportunityid") : null;
            revisedItem.RevisedItemStatus = reterviedEntity.GetAttributeValue<OptionSetValue>("ap360_reviseditemstatus").Value;
        }


        public static EntityCollection getWorkOrderSubletsRelatedToWorkOrder(IOrganizationService service, ITracingService tracingService, Guid workOrderGuid)
        {

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='msdyn_workorderproduct'>
                                 
                                    <attribute name='msdyn_name' />
                                    <attribute name='msdyn_linestatus' />

                                 
                                    <attribute name='msdyn_workorderproductid' />
                                    <order attribute='msdyn_name' descending='false' />
                                    <filter type='and'>
                                       <condition attribute='msdyn_linestatus' operator='ne' value='690970001' />
                                      <condition attribute='ap360_product' operator='eq'  value='" + workOrderGuid + @"' /> 
                                    </filter>
                                  </entity>
                                </fetch>");



            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));


            return col;
        }
        public static Guid CreateWOSIncidentalForPayments(IOrganizationService service, ITracingService tracing, Money creditCardCharges, EntityReference opportunityRef)
        {
            Entity newWOSubletForIncidental = new Entity("ap360_workordersublet");
            newWOSubletForIncidental["ap360_name"] = "Credit Card Fee";
            newWOSubletForIncidental["ap360_subletdescription"] = "Credit Card Fee";
            newWOSubletForIncidental["ap360_incidentalcosts"] = creditCardCharges;
            newWOSubletForIncidental["ap360_opportunityid"] = opportunityRef;

            Guid newlycreatedWOSubIncidentalGuid = service.Create(newWOSubletForIncidental);

            return newlycreatedWOSubIncidentalGuid;
        }

    }
}
