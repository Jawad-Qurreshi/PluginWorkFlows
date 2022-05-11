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
    public class Quote
    {

        public Guid quoteGuid { get; set; }
        public EntityReference priceList { get; set; }
        public string Name { get; set; }
        public EntityReference account { get; set; }
        public bool IsWorkOrderCreated { get; set; }

        public int OpportunityNumber { get; set; }
        public EntityReference ParentServiceTemplate { get; set; }
        public EntityReference PotentialCustomer { get; set; }

        public EntityReference Vechicle { get; set; }
        public int quotetype { get; set; }
        public string OpportunityAutoNumber { get; set; }
        public bool UsePrimaryIncident { get; set; }
        public int QuoteStatus { get; set; }

        public DateTime QuoteConversionToWOTimeStamped { get; set; }
        public EntityReference QuoteApprovedBy { get; set; }
        public EntityReference Opportunity { get; set; }

        public EntityReference RevisedQuoteGuid { get; set; }
        public static Quote getQuote(IOrganizationService service, ITracingService tracing, string logiclName, Guid quoteGuid)
        {
            Quote quote = new Quote();
            Entity quoteEntity = service.Retrieve(logiclName, quoteGuid, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            if (quoteEntity != null)
            {
                quote.quoteGuid = quoteEntity.Id;
                quote.priceList = quoteEntity.GetAttributeValue<EntityReference>("pricelevelid") != null ? quoteEntity.GetAttributeValue<EntityReference>("pricelevelid") : null;
                quote.Name = quoteEntity.GetAttributeValue<string>("name");
                quote.RevisedQuoteGuid = quoteEntity.GetAttributeValue<EntityReference>("ap360_revisedquoteid") ?? null;
                quote.ParentServiceTemplate = quoteEntity.GetAttributeValue<EntityReference>("ap360_parentservicetemplatetypeid") != null ? quoteEntity.GetAttributeValue<EntityReference>("ap360_parentservicetemplatetypeid") : null;
                quote.account = quoteEntity.GetAttributeValue<EntityReference>("customerid") != null ? quoteEntity.GetAttributeValue<EntityReference>("customerid") : null;
                quote.Vechicle = quoteEntity.GetAttributeValue<EntityReference>("ap360_vehicleid") != null ? quoteEntity.GetAttributeValue<EntityReference>("ap360_vehicleid") : null;
                quote.quotetype = 0;
                quote.quotetype = quoteEntity.GetAttributeValue<OptionSetValue>("ap360_quotetype") != null ? quoteEntity.GetAttributeValue<OptionSetValue>("ap360_quotetype").Value : 0;
                quote.OpportunityAutoNumber = quoteEntity.GetAttributeValue<string>("ap360_opportunityautonumber");
                quote.UsePrimaryIncident = false;
                quote.UsePrimaryIncident = quoteEntity.GetAttributeValue<bool>("ap360_useprimaryincident");
                quote.QuoteStatus = 0;
                quote.QuoteStatus = quoteEntity.GetAttributeValue<OptionSetValue>("statuscode").Value;
                quote.QuoteConversionToWOTimeStamped = quoteEntity.GetAttributeValue<DateTime>("ap360_quoteconversiontowotimestamped");
                quote.QuoteApprovedBy = quoteEntity.GetAttributeValue<EntityReference>("ap360_quoteapprovedbyid") != null ? quoteEntity.GetAttributeValue<EntityReference>("ap360_quoteapprovedbyid") : null;
                quote.IsWorkOrderCreated = false;
                quote.IsWorkOrderCreated = quoteEntity.GetAttributeValue<bool>("ap360_isworkordercreated");
                quote.Opportunity = quoteEntity.GetAttributeValue<EntityReference>("opportunityid") != null ? quoteEntity.GetAttributeValue<EntityReference>("opportunityid") : null;
                //quote.OpportunityNumber = quoteEntity.GetAttributeValue<int>("ap360_opportunitynumber");


                if (quote.priceList != null)
                {
                    tracing.Trace("Price List exists " + quote.priceList.ToString());
                }
                if (quote.account != null)
                {
                    tracing.Trace("Account exists " + quote.account.ToString());
                }
            }
            else
            {
                tracing.Trace("Quote entity is not reterived");
            }
            return quote;
        }

        public static Guid createQuote(IOrganizationService service, ITracingService tracing, Quote quote)
        {
            tracing.Trace("Creation of Quote Started");
            Entity newQuote = new Entity("quote");
            if (quote.ParentServiceTemplate != null)
            {
                newQuote["name"] = quote.ParentServiceTemplate.Name;
                newQuote["ap360_parentservicetemplatetypeid"] = new EntityReference(quote.ParentServiceTemplate.LogicalName, quote.ParentServiceTemplate.Id);
            }
            if (quote.account != null)
            {
                newQuote["customerid"] = new EntityReference(quote.account.LogicalName, quote.account.Id);
            }
            if (quote.Vechicle != null)
            {
                newQuote["ap360_vehicleid"] = new EntityReference(quote.Vechicle.LogicalName, quote.Vechicle.Id);
            }
            if (quote.quotetype != 0)
            {
                newQuote["ap360_quotetype"] = new OptionSetValue(quote.quotetype);
            }
            newQuote["ap360_opportunityautonumber"] = quote.OpportunityAutoNumber;
            //  newQuote["ap360_useprimaryincident"] = quote.UsePrimaryIncident;
            // newQuote["statuscode"] = quote.QuoteStatus;
            //newQuote["ap360_quoteconversiontowotimestamped"] = quote.QuoteConversionToWOTimeStamped;
            //if (quote.QuoteApprovedBy != null)
            //{3w5e6r789
            //    newQuote["ap360_quoteapprovedbyid"] = new EntityReference(quote.QuoteApprovedBy.LogicalName, quote.QuoteApprovedBy.Id);
            //}
            // newQuote["ap360_isworkordercreated"] = quote.IsWorkOrderCreated;

            if (quote.Opportunity != null)
            {
                newQuote["opportunityid"] = new EntityReference(quote.Opportunity.LogicalName, quote.Opportunity.Id);
            }
            Guid newQuoteGuid = service.Create(newQuote);

            return newQuoteGuid;

        }

        public static List<Quote> GetQuotesRelatedtoOpportunity(IOrganizationService service, ITracingService tracing, Guid opportunityGuid)
        {


            List<Quote> lstQuotes = new List<Quote>();

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                      <entity name='quote'>
                                        <attribute name='name' />
                                        <attribute name='customerid' />
                                        <attribute name='statecode' />
                                        <attribute name='totalamount' />
                                        <attribute name='quoteid' />
                                        <attribute name='createdon' />
                                        <order attribute='name' descending='false' />
                                        <filter type='and'>
                                          <condition attribute='opportunityid' operator='eq'  value='" + opportunityGuid + @"' /> 
                                        </filter>
                                      </entity>
                                    </fetch>");



            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (col.Entities.Count > 0)
            {
                tracing.Trace(col.Entities.Count.ToString() + " Quote Services reterived");

                Quote quote;
                foreach (Entity entity in col.Entities)
                {
                    quote = new Quote();
                    quote.quoteGuid = entity.Id;
                    quote.Name = entity.GetAttributeValue<string>("name");

                    lstQuotes.Add(quote);
                }
            }
            else
            {
                tracing.Trace(col.Entities.Count.ToString() + " Quote  reterived");

            }

            return lstQuotes;

        }

        public static void SetQuoteStatus(IOrganizationService service, ITracingService tracing, Quote quote, int state, int status)
        {
            SetStateRequest setStateRequest = new SetStateRequest()
            {
                EntityMoniker = new EntityReference
                {
                    Id = quote.quoteGuid,
                    LogicalName = "quote",
                },
                State = new OptionSetValue(state),
                Status = new OptionSetValue(status)
            };
            service.Execute(setStateRequest);


        }
        public static Guid CreateQuoteForRejectQuoteServices(IOrganizationService service, ITracingService tracing,Entity initialQuote)

        {

            string quoteName = initialQuote.GetAttributeValue<string>("name");
            EntityReference opportunity = initialQuote.GetAttributeValue<EntityReference>("opportunityid") != null ? initialQuote.GetAttributeValue<EntityReference>("opportunityid") : null;
            EntityReference parentServiceTemplate = initialQuote.GetAttributeValue<EntityReference>("ap360_parentservicetemplatetypeid") != null ? initialQuote.GetAttributeValue<EntityReference>("ap360_parentservicetemplatetypeid") : null;
            EntityReference customer = initialQuote.GetAttributeValue<EntityReference>("customerid") != null ? initialQuote.GetAttributeValue<EntityReference>("customerid") : null;
            EntityReference vehicle = initialQuote.GetAttributeValue<EntityReference>("ap360_vehicleid") != null ? initialQuote.GetAttributeValue<EntityReference>("ap360_vehicleid") : null;
            OptionSetValue quoteType = initialQuote.GetAttributeValue<OptionSetValue>("ap360_quotetype") != null ? initialQuote.GetAttributeValue<OptionSetValue>("ap360_quotetype") : null;
            String opportunityAutoNumber = initialQuote.GetAttributeValue<String>("ap360_opportunityautonumber") != null ? initialQuote.GetAttributeValue<String>("ap360_opportunityautonumber") : null;

            Entity newQuote = new Entity("quote");
            newQuote["name"] = quoteName;
            if (opportunity != null)
                newQuote["opportunityid"] = new EntityReference(opportunity.LogicalName, opportunity.Id);
            if (parentServiceTemplate != null)
                newQuote["ap360_parentservicetemplatetypeid"] = new EntityReference(parentServiceTemplate.LogicalName, parentServiceTemplate.Id);
            if (customer != null)
                newQuote["customerid"] = new EntityReference(customer.LogicalName, customer.Id);
            if (vehicle != null)
                newQuote["ap360_vehicleid"] = new EntityReference(vehicle.LogicalName, vehicle.Id);
            if (quoteType != null)
                newQuote["ap360_quotetype"] = new OptionSetValue(quoteType.Value);
            if (opportunityAutoNumber != null)
                newQuote["ap360_opportunityautonumber"] = opportunityAutoNumber;
            Guid newlyCreatedQuoteGUID = service.Create(newQuote);

            return newlyCreatedQuoteGUID;
        }       
    }
}