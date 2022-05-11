using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code.Model
{
    public class QuoteSublet
    {

        public Guid guid { get; set; }
        public string Name { get; set; }
        public Money EstimatedAmount { get; set; }
        public Money SalePrice { get; set; }
        public EntityReference Vendor { get; set; }
        public EntityReference Product { get; set; }

        public string SubletDescription { get; set; }
        public DateTime EstimateDeliveryDate { get; set; }
        public EntityReference QuoteService { get; set; }



        public static List<QuoteSublet> GetQuoteSublet(IOrganizationService service, ITracingService tracing, Guid serviceGuid)
        {

            List<QuoteSublet> lstQuoteSublets = new List<QuoteSublet>();

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='ap360_quotesublet'>
                                    <attribute name='ap360_quotesubletid' />
                                    <attribute name='ap360_name' />
                                    <attribute name='ap360_vendorid' />
                                    <attribute name='ap360_productid' />
                                    <attribute name='ap360_saleprice' />


                                    <attribute name='ap360_subletdescription' />
                                    <attribute name='ap360_quoteserviceid' />
                                    <attribute name='ap360_estimateddeliverydate' />
                                    <attribute name='ap360_estimatedamount' />
                                    <order attribute='ap360_name' descending='false' />
                                    <filter type='and'>
                                      <condition attribute='ap360_quoteserviceid' operator='eq'  value='" + serviceGuid + @"' /> 
                                    </filter>
                                  </entity>
                                </fetch>");


            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            QuoteSublet quoteSublet;
            foreach (Entity entity in col.Entities)
            {
                quoteSublet = new QuoteSublet();
                quoteSublet.guid = entity.Id;
                quoteSublet.Name = entity.GetAttributeValue<string>("ap360_name");
                quoteSublet.Vendor = entity.GetAttributeValue<EntityReference>("ap360_vendorid") != null ? entity.GetAttributeValue<EntityReference>("ap360_vendorid") : null;
                quoteSublet.EstimateDeliveryDate = entity.GetAttributeValue<DateTime>("ap360_estimateddeliverydate");
                quoteSublet.SubletDescription = entity.GetAttributeValue<string>("ap360_subletdescription");
                quoteSublet.EstimatedAmount = entity.GetAttributeValue<Money>("ap360_estimatedamount") != null ? entity.GetAttributeValue<Money>("ap360_estimatedamount") : null;
                quoteSublet.SalePrice = entity.GetAttributeValue<Money>("ap360_saleprice") != null ? entity.GetAttributeValue<Money>("ap360_saleprice") : null;
                quoteSublet.Product = entity.GetAttributeValue<EntityReference>("ap360_productid") != null ? entity.GetAttributeValue<EntityReference>("ap360_productid") : null;
                quoteSublet.QuoteService = entity.GetAttributeValue<EntityReference>("ap360_quoteserviceid") != null ? entity.GetAttributeValue<EntityReference>("ap360_quoteserviceid") : null;




                lstQuoteSublets.Add(quoteSublet);

            }
            return lstQuoteSublets;

        }

        public static void createQuoteSubletsForReviseQuote(IOrganizationService service, ITracingService tracing, List<QuoteSublet> lstQuoteSublets, Guid newlyCreatedQuoteServiceGuid)
        {
            tracing.Trace("Inside creation of Quote Sublet and count is " + lstQuoteSublets.Count.ToString());

            foreach (QuoteSublet quoteSublet in lstQuoteSublets)
            {
                Entity newQuoteSublet = new Entity("ap360_quotesublet");
                newQuoteSublet["ap360_name"] = quoteSublet.Name;
                if (quoteSublet.Vendor != null)
                {
                    newQuoteSublet["ap360_vendorid"] = quoteSublet.Vendor;
                }
                if (quoteSublet.EstimateDeliveryDate.ToString() != "1/1/0001 12:00:00 AM")
                {
                    newQuoteSublet["ap360_estimateddeliverydate"] = quoteSublet.EstimateDeliveryDate;
                }
                newQuoteSublet["ap360_subletdescription"] = quoteSublet.SubletDescription;
                if (quoteSublet.EstimatedAmount != null)
                {
                    newQuoteSublet["ap360_estimatedamount"] = quoteSublet.EstimatedAmount;
                }
                if (quoteSublet.SalePrice != null)
                {
                    newQuoteSublet["ap360_saleprice"] = quoteSublet.SalePrice;
                }
                if (quoteSublet.Product != null)
                {
                    newQuoteSublet["ap360_productid"] = quoteSublet.Product;
                }
              
                    newQuoteSublet["ap360_quoteserviceid"] = new EntityReference("ap360_quoteservice", newlyCreatedQuoteServiceGuid);
               
                service.Create(newQuoteSublet);
            }
        }

    }
}
