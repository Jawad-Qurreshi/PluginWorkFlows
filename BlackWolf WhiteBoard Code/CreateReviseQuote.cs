using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class CreateReviseQuote : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();
            //  throw new InvalidPluginExecutionException("CreatePurchaseOrderforPreferredSupplier");

            try
            {

                ITracingService tracingService =
                  (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                IPluginExecutionContext context = (IPluginExecutionContext)
                    serviceProvider.GetService(typeof(IPluginExecutionContext));

                IOrganizationServiceFactory factory =
                     (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = factory.CreateOrganizationService(context.UserId);


                string OldQuoteId = (string)context.InputParameters["OldQuoteId"];
                // throw new InvalidPluginExecutionException("Custom Error"+ OldQuoteId);

                //(entity.Contains("statuscode") ||

                Quote quote = new Quote();
                QuoteService quoteService = null;
                List<QuoteService> lstquoteServices = new List<QuoteService>();


                quote = Quote.getQuote(service, tracingService, "quote", new Guid(OldQuoteId));
                //if (quote.IsRevisedQuoteCreated == true) throw new InvalidPluginExecutionException("Work Order(s)  already created");
                //if (quote.IsWorkOrderCreated == true) throw new InvalidPluginExecutionException("Work Order is already created");
                ///////////////////////////////////////Temporary code neeed to remove once plugin on quote status//////////////////
                //quote.quotetype = entity.GetAttributeValue<OptionSetValue>("ap360_quotetype") != null ? entity.GetAttributeValue<OptionSetValue>("ap360_quotetype").Value : 0;

                //////////////////////////Move this line into getQuote funciton once pugin updated/////////////////////////

                tracingService.Trace("Quote Reterived");

                lstquoteServices = QuoteService.GetQuoteServices(service, tracingService, new Guid(OldQuoteId));

                tracingService.Trace(lstquoteServices.Count.ToString());
                int IsQSRevisedQSCreatedCount = QuoteService.GetIsQSRevisedQSCreatedCount(lstquoteServices);
                tracingService.Trace("******************lstquotesrv count " + lstquoteServices.Count.ToString());
                tracingService.Trace("******************Processed QS count " + IsQSRevisedQSCreatedCount.ToString());

                List<QuoteService> updatedLstquoteServices = new List<QuoteService>();


                double sumofItems = 0;

                foreach (QuoteService qtSrv in lstquoteServices)
                {
                    if (!qtSrv.IsQSRevisedQuoteServiceCreated)
                    {
                        tracingService.Trace(" \n Start of Foreach");
                        quoteService = new QuoteService();
                        quoteService.EstimatedTime = qtSrv.EstimatedTime;
                        quoteService.guid = qtSrv.guid;
                        quoteService.Name = qtSrv.Name;
                        quoteService.ActualWorkRequested = qtSrv.ActualWorkRequested;
                        quoteService.ServiceRole = qtSrv.ServiceRole;
                        tracingService.Trace("Quote SErvice Role :" + qtSrv.ServiceRole.Name);

                        quoteService.PartsSalePrice = qtSrv.PartsSalePrice;
                        if (qtSrv.HourlyRate != null)
                            quoteService.HourlyRate = qtSrv.HourlyRate;
                        if (qtSrv.ParentServiceTemplate != null)
                            quoteService.ParentServiceTemplate = qtSrv.ParentServiceTemplate;
                        if (qtSrv.ChildServiceTemplate != null)
                            quoteService.ChildServiceTemplate = qtSrv.ChildServiceTemplate;
                        if (qtSrv.ServiceTemplate != null)
                            quoteService.ServiceTemplate = qtSrv.ServiceTemplate;
                        if (qtSrv.ParentServiceTask != null)
                            quoteService.ParentServiceTask = qtSrv.ParentServiceTask;

                        if (qtSrv.ServiceProductMapping != null)
                            quoteService.ServiceProductMapping = qtSrv.ServiceProductMapping;
                        if (qtSrv.GGParentProduct != null)
                            quoteService.GGParentProduct = qtSrv.GGParentProduct;
                        if (qtSrv.GParentProduct != null)
                            quoteService.GParentProduct = qtSrv.GParentProduct;
                        if (qtSrv.QuoteServiceType != null)
                            quoteService.QuoteServiceType = qtSrv.QuoteServiceType;

                        tracingService.Trace("Before Fetching Quote Product and Quote Service task");


                        quoteService.lstQuoteProduct = new List<QuoteProduct>();
                        quoteService.lstQuoteProduct = QuoteProduct.GetQuoteProducts(service, tracingService, qtSrv.guid);
                        tracingService.Trace(" Quote Products reterived");

                        if (quoteService.lstQuoteProduct.Count > 0)
                        {
                            tracingService.Trace("Quote Product Count " + quoteService.lstQuoteProduct.Count.ToString());
                            double productCount = 0;
                            productCount = quoteService.lstQuoteProduct.Count * 1.6;
                            sumofItems = sumofItems + productCount;

                        }
                        quoteService.lstquoteServiceTask = new List<QuoteServiceTask>();
                        quoteService.lstquoteServiceTask = QuoteServiceTask.GetQuoteServiceTasks(service, qtSrv.guid);
                        if (quoteService.lstquoteServiceTask.Count > 0)
                        {
                            tracingService.Trace("Quote Service Task Count " + quoteService.lstquoteServiceTask.Count.ToString());
                            sumofItems = sumofItems + quoteService.lstquoteServiceTask.Count;

                        }
                        quoteService.lstQuoteSublet = new List<QuoteSublet>();
                        quoteService.lstQuoteSublet = QuoteSublet.GetQuoteSublet(service, tracingService, qtSrv.guid);
                        if (quoteService.lstQuoteSublet.Count > 0)
                        {
                            tracingService.Trace("Quote Sublet Count " + quoteService.lstQuoteSublet.Count.ToString());
                            sumofItems = sumofItems + quoteService.lstQuoteSublet.Count;

                        }
                        // throw new InvalidPluginExecutionException("sum of items " + sumofItems.ToString());        
                        if (sumofItems < 60)
                        {
                            updatedLstquoteServices.Add(quoteService);
                        }
                        else
                        {
                            throw new InvalidPluginExecutionException(quote.Name + " has " + sumofItems + " items. Items limit inside quote is 60");
                        }
                    }
                    tracingService.Trace("End of Foreach\n");

                }
                //throw new InvalidPluginExecutionException("Custom Error");
                //throw new InvalidPluginExecutionException("Remaining quote service " + updatedLstquoteServices.Count.ToString());

                if (updatedLstquoteServices.Count < 1) return;
                int count = 0;
                Guid newQuoteGuid = Guid.Empty;
                if (quote.RevisedQuoteGuid == null)
                {
                    newQuoteGuid = Quote.createQuote(service, tracingService, quote);

                }
                else
                {
                    newQuoteGuid = quote.RevisedQuoteGuid.Id;
                }
                foreach (QuoteService qtSrv in updatedLstquoteServices)
                {
                    count++;
                    tracingService.Trace("Record Number " + count.ToString() + " *********************");
                    tracingService.Trace("Quote Srv Name " + qtSrv.Name);

                    tracingService.Trace("Fetching  of Quote Product and Quote Services completed");
                    tracingService.Trace("Before QuoteService Creation");

                    Guid newlyCreatedQuoteServiceGuid = QuoteService.CreateQuoteServiceForReviseQuote(service, tracingService, newQuoteGuid, qtSrv);
                    tracingService.Trace("After QuoteService Created : Guid " + newlyCreatedQuoteServiceGuid.ToString());

                    tracingService.Trace("Before Quote Product Creation");
                    QuoteProduct.CreateQuoteProductsForReviseQuote(service, tracingService, qtSrv.lstQuoteProduct, newlyCreatedQuoteServiceGuid);
                    //tracingService.Trace("Original Estimated Amount " + totaloriginalestimatepartsamount.ToString());


                    tracingService.Trace("Before Quote Service Task Creation");
                    QuoteServiceTask.CreateQuoteServiceTasksForReviseQuote(service, tracingService, qtSrv.lstquoteServiceTask, newlyCreatedQuoteServiceGuid);
                    //tracingService.Trace("Total Original Estimated Duration " + totaloriginalestimatedduration.ToString());

                    tracingService.Trace("Before Quote Sublet  Creation");
                    QuoteSublet.createQuoteSubletsForReviseQuote(service, tracingService, qtSrv.lstQuoteSublet, newlyCreatedQuoteServiceGuid);

                    tracingService.Trace("Plugin Successfully ended");


                    Entity updateQuoteServiceEntity = new Entity("ap360_quoteservice", qtSrv.guid);
                    updateQuoteServiceEntity["ap360_isqsrevisedquoteservicecreated"] = true;
                    service.Update(updateQuoteServiceEntity);
                }
                tracingService.Trace("*************** IsQSRevisedQSCreatedCount = IsQSRevisedQSCreatedCount + updatedLstquoteServices.Count");
                tracingService.Trace("***************IsQSRevisedQSCreatedCount =" + IsQSRevisedQSCreatedCount + updatedLstquoteServices.Count);
                IsQSRevisedQSCreatedCount = IsQSRevisedQSCreatedCount + updatedLstquoteServices.Count;

                Entity updateOldQuote = new Entity("quote", new Guid(OldQuoteId));


                if (IsQSRevisedQSCreatedCount == lstquoteServices.Count)
                {
                    updateOldQuote["ap360_isrevisedquotecreated"] = true;
                    context.OutputParameters["NewQuoteId"] = newQuoteGuid.ToString();
                }
                else
                {
                    updateOldQuote["ap360_revisedquoteid"] = new EntityReference("quote", newQuoteGuid);


                    updateOldQuote["ap360_isrevisedquotecreated"] = false;
                    context.OutputParameters["NewQuoteId"] = "notcompleted";

                }

                service.Update(updateOldQuote);

                ProjectTask quoteProjectTask = new ProjectTask();
                quoteProjectTask = ProjectTask.GetProjectTaskforQuoteEntities(service, tracingService, "msdyn_projecttask", "ap360_quoteid", quote.quoteGuid, "ap360_quoteservice");
                // throw new InvalidPluginExecutionException("Custom Erro ");
                if (quoteProjectTask != null)
                {
                    //  throw new InvalidPluginExecutionException("custom "+quoteProjectTask.guid.ToString());

                    service.Delete("msdyn_projecttask", quoteProjectTask.guid);

                }

                //throw new InvalidPluginExecutionException("custom Error");


                //Entity quoteEntity = new Entity("quote");
                //quoteEntity.Id = quote.quoteGuid;

                //service.Update(quoteEntity);
                //SetStateRequest setStateRequest = new SetStateRequest()
                //{
                //    EntityMoniker = new EntityReference
                //    {
                //        Id = quote.quoteGuid,
                //        LogicalName = "quote",
                //    },
                //    State = new OptionSetValue(2),//Status : Active
                //    Status = new OptionSetValue(4)//Won-Authorized By Client
                //};
                //service.Execute(setStateRequest);











            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}