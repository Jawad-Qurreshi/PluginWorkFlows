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
    public class CreateWorkOrder : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();
            //  throw new InvalidPluginExecutionException("CreateWorkOrder");

            try
            {

                //throw new InvalidPluginExecutionException("throw");F:\Upwork\Chris 2019\Black Wolf Server Codes\New Code By Me\BlackWolf WhiteBoard Code\BlackWolf WhiteBoard Code\CreateWorkOrder.cs
                ITracingService tracingService =
                  (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                IPluginExecutionContext context = (IPluginExecutionContext)
                    serviceProvider.GetService(typeof(IPluginExecutionContext));

                IOrganizationServiceFactory factory =
                     (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = factory.CreateOrganizationService(context.UserId);

                //throw new InvalidPluginExecutionException("Error");

                string quoteId = (string)context.InputParameters["quoteGuid"];
                Guid quoteGuid = new Guid(quoteId);
                //  throw new InvalidPluginExecutionException(quoteId);
                Entity entity = new Entity("quote", quoteGuid);
                //if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                //{
                //    entity = (Entity)context.InputParameters["Target"];
                //    if (entity.LogicalName == "quote")
                //    {

                //        if (context.MessageName.ToLower() == "update")
                //        {
                //            tracingService.Trace("Entity is " + entity.LogicalName);
                //            tracingService.Trace("Update ");


                //            if (context.Stage == 40)
                //            {

                //                if (entity.Contains("ap360_approvequotetowoconverstion") && context.Depth == 1)
                //                {


                //                    //(entity.Contains("statuscode") ||
                tracingService.Trace("Quote is approved ");
                //throw new InvalidPluginExecutionException("Error");
                //This code is only to override code
                
                SetStateRequest setStateRequest1 = new SetStateRequest()
                {
                    EntityMoniker = new EntityReference
                    {
                        Id = entity.Id,
                        LogicalName = "quote",
                    },
                    State = new OptionSetValue(0),//Status : Draft
                    Status = new OptionSetValue(126300001)//Draft- Approved For Submittal To Client	
                };
                service.Execute(setStateRequest1);


                tracingService.Trace("Post Step");
                tracingService.Trace("Contect Depth" + context.Depth);



                //  if (entity.GetAttributeValue<bool>("ap360_approvequotetowoconverstion") != true) return;
                Quote quote = new Quote();
                QuoteService quoteService = null;
                List<QuoteService> lstquoteServices = new List<QuoteService>();


                quote = Quote.getQuote(service, tracingService, entity.LogicalName, entity.Id);
                if (quote.IsWorkOrderCreated == true) throw new InvalidPluginExecutionException("Work Order(s)  already created");
                ///////////////////////////////////////Temporary code neeed to remove once plugin on quote status//////////////////
                //quote.quotetype = entity.GetAttributeValue<OptionSetValue>("ap360_quotetype") != null ? entity.GetAttributeValue<OptionSetValue>("ap360_quotetype").Value : 0;

                //////////////////////////Move this line into getQuote funciton once pugin updated/////////////////////////
                tracingService.Trace(" Opportuntiy : " + quote.Opportunity.Name.ToString() + " Quote Name " + quote.Name.ToString());

                lstquoteServices = QuoteService.GetQuoteServices(service, tracingService, entity.Id);

                tracingService.Trace(lstquoteServices.Count.ToString());
                int IsQSWOCreatedQuoteServicesCount = QuoteService.GetIsQSWOCreatedQuoteServicesCount(lstquoteServices);
                tracingService.Trace("******************lstquotesrv count " + lstquoteServices.Count.ToString());
                tracingService.Trace("******************Processed QS count " + IsQSWOCreatedQuoteServicesCount.ToString());


                List<QuoteService> updatedLstquoteServices = new List<QuoteService>();
                double sumofItems = 0;

                foreach (QuoteService qtSrv in lstquoteServices)
                {
                    if (!qtSrv.IsQSWorkOrderCreated)
                    {
                        //   throw new InvalidPluginExecutionException("lst of qt " + lstquoteServices.Count.ToString());
                        quoteService = new QuoteService();
                        quoteService.EstimatedTime = qtSrv.EstimatedTime;
                        quoteService.guid = qtSrv.guid;
                        quoteService.Name = qtSrv.Name;
                        quoteService.ServiceRole = qtSrv.ServiceRole;
                        quoteService.PartsSalePrice = qtSrv.PartsSalePrice;
                        quoteService.QuoteServiceType = qtSrv.QuoteServiceType;
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

                        //  throw new InvalidPluginExecutionException("Sum of items " + sumofItems.ToString());
                        if (sumofItems < 60)// it can be upto 90-100 but becasue of Create revised quote its 60
                            //95 now its reduced becasue of setting workorder status MakeQuoteFirstWorkOrderAvailableForBooking
                        {

                            updatedLstquoteServices.Add(quoteService);
                        }

                    }
                }
                // throw new InvalidPluginExecutionException("product count " + sumofItems.ToString());
                int count = 0;
                var watch1 = System.Diagnostics.Stopwatch.StartNew();
                tracingService.Trace("*************** UPdate Quote SErvice List count" + updatedLstquoteServices.Count.ToString());
               // throw new InvalidPluginExecutionException("Error updated " + updatedLstquoteServices.Count.ToString());
                foreach (QuoteService qtSrv in updatedLstquoteServices)
                {
                    count++;
                    tracingService.Trace("Record Number " + count.ToString());
                    tracingService.Trace("Quote Srv Name " + qtSrv.Name);

                    tracingService.Trace("Fetching  of Quote Product and Quote Services completed");
                    tracingService.Trace("Before WorkOrder Creation");
                    Guid newlyCreatedWorkOrderGuid = WorkOrder.CreateWorkOrder(service, tracingService, quote, qtSrv);
                    tracingService.Trace("After WorkOrder Created : Guid " + newlyCreatedWorkOrderGuid.ToString());


                    Guid firstQuoteServiceTaskGuid = Guid.Empty;

                    List<WOSTandQSTObject> lstWOSTandQSTObject = new List<WOSTandQSTObject>();



                    tracingService.Trace("Before WorOrder Service Task Creation");
                    int totaloriginalestimatedduration = WorkOrderServiceTask.CreateWorkOrderServiceTasks(service, tracingService, qtSrv.lstquoteServiceTask, newlyCreatedWorkOrderGuid, quote, qtSrv,out firstQuoteServiceTaskGuid, lstWOSTandQSTObject);
                    tracingService.Trace("Total Original Estimated Duration " + totaloriginalestimatedduration.ToString());

                    tracingService.Trace("Before WorOrder Product Creation");
                    decimal totaloriginalestimatepartsamount = WorkOrderProduct.CreateWorkOrderProducts(service, tracingService, qtSrv.lstQuoteProduct, newlyCreatedWorkOrderGuid, quote, context, firstQuoteServiceTaskGuid, lstWOSTandQSTObject);
                    tracingService.Trace("Original Estimated Amount " + totaloriginalestimatepartsamount.ToString());


                    tracingService.Trace("Before WorOrder Sublet Task Creation");
                    decimal totaloriginalestimatedSubletamount = WorkOrderSublet.CreateWorkOrderSublets(service, tracingService, qtSrv.lstQuoteSublet, newlyCreatedWorkOrderGuid, quote);
                    tracingService.Trace("Before Updating WorkOrder");

                    //This is for adjusting Orignal and estimated on time of creation of Work Order Entity.
                    WorkOrder.updateWorkOrder(service, tracingService, newlyCreatedWorkOrderGuid, totaloriginalestimatepartsamount, totaloriginalestimatedduration, totaloriginalestimatedSubletamount);


                    tracingService.Trace("Plugin Successfully ended");


                    Entity updateQuoteServiceEntity = new Entity("ap360_quoteservice", qtSrv.guid);
                    updateQuoteServiceEntity["ap360_isqsworkordercreated"] = true;
                    service.Update(updateQuoteServiceEntity);
                }


                tracingService.Trace("*************** IsQSWOCreatedQuoteServicesCount = IsQSWOCreatedQuoteServicesCount + updatedLstquoteServices.Count");
                tracingService.Trace("***************IsQSWOCreatedQuoteServicesCount =" + IsQSWOCreatedQuoteServicesCount + updatedLstquoteServices.Count);
                IsQSWOCreatedQuoteServicesCount = IsQSWOCreatedQuoteServicesCount + updatedLstquoteServices.Count;

                //throw new InvalidPluginExecutionException("Is QS Created count "+ IsQSWOCreatedQuoteServicesCount.ToString()+"  LstofQuote services"+lstquoteServices.Count.ToString());
                if (IsQSWOCreatedQuoteServicesCount == lstquoteServices.Count)
                {

                    Entity quoteEntity = new Entity("quote");
                    quoteEntity.Id = quote.quoteGuid;
                    quoteEntity["ap360_isworkordercreated"] = true;
                    quoteEntity["ap360_quoteconversiontowotimestamped"] = DateTime.Now;
                    quoteEntity["ap360_quoteapprovedbyid"] = new EntityReference("systemuser", context.UserId);
                    quoteEntity["ap360_approvequotetowoconverstion"] = false;//Trigger for Work Order Creation
                                                                             //back to false is mandatory so that admin can create workOrder for remaining items 

                    //   quoteEntity["ap360_createprojecttasks"] = true;

                    service.Update(quoteEntity);


                    Quote.SetQuoteStatus(service, tracingService, quote, 2, 4);//(2)Status : Won ,(4)Won-Authorized By Client
                    //SetStateRequest setStateRequest = new SetStateRequest()
                    //{
                    //    EntityMoniker = new EntityReference
                    //    {
                    //        Id = quote.quoteGuid,
                    //        LogicalName = "quote",
                    //    },
                    //    State = new OptionSetValue(2),//Status : Won
                    //    Status = new OptionSetValue(4)//Won-Authorized By Client
                    //};
                    //service.Execute(setStateRequest);
                    context.OutputParameters["isAllWorkOrdersAreConverted"] = "Yes";
                    context.OutputParameters["isAllQuoteServicesConverted"] = "Yes";
                    //  throw new InvalidPluginExecutionException("Completed");
                }
                else
                {

                    //  throw new InvalidPluginExecutionException("INside no error");
                    Entity quoteEntity = new Entity("quote");
                    quoteEntity.Id = quote.quoteGuid;
                    quoteEntity["ap360_isworkordercreated"] = false;
                    quoteEntity["ap360_approvequotetowoconverstion"] = false;//Trigger for Work Order Creation
                                                                             //back to false is mandatory so that admin can create workOrder for remaining items 
                                                                             //   quoteEntity["ap360_createprojecttasks"] = true;

                  service.Update(quoteEntity);
                    Quote.SetQuoteStatus(service, tracingService, quote, 1, 3);//(1)//Status : Active ,(3)Active-submitted to Client
                    //SetStateRequest setStateRequest = new SetStateRequest()
                    //{
                    //    EntityMoniker = new EntityReference
                    //    {
                    //        Id = quote.quoteGuid,
                    //        LogicalName = "quote",
                    //    },
                    //    State = new OptionSetValue(1),//Status : Active
                    //    Status = new OptionSetValue(3)//Won-Authorized By Client
                    //};
                    //service.Execute(setStateRequest);
                    context.OutputParameters["isAllWorkOrdersAreConverted"] = "No";
                    context.OutputParameters["isAllQuoteServicesConverted"] = "No";
                    //throw new InvalidPluginExecutionException("Not Completed");


                }

                watch1.Stop();
                var elapsedMs = watch1.ElapsedMilliseconds;
                tracingService.Trace("Time  " + elapsedMs.ToString());
                //   throw new InvalidPluginExecutionException("exception");
                //                }
                //            }
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                throw ex;

            }
        }
    }
}