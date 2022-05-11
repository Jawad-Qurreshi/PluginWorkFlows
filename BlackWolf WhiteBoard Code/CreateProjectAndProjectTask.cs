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
    public class CreateProjectAndProjectTask : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();
            //   throw new InvalidPluginExecutionException("CreateProductFromDescriptionAndRelatetoProductFamily");

            try
            {

                ITracingService tracingService =
                  (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                IPluginExecutionContext context = (IPluginExecutionContext)
                    serviceProvider.GetService(typeof(IPluginExecutionContext));

                IOrganizationServiceFactory factory =
                     (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = factory.CreateOrganizationService(context.UserId);


                Entity entity = null;



                if (context.MessageName.ToLower() == "update")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {

                        entity = (Entity)context.InputParameters["Target"];

                        if (entity.LogicalName == "quote")
                        {
                            tracingService.Trace("Entity Name " + entity.LogicalName);

                            if (entity.Contains("ap360_createproject"))
                            {



                                if (entity.GetAttributeValue<bool>("ap360_createproject") != true) return;

                                Quote quote = new Quote();
                                QuoteService quoteService = null;
                                List<QuoteService> lstquoteServices = new List<QuoteService>();


                                quote = Quote.getQuote(service, tracingService, entity.LogicalName, entity.Id);
                                //if (quote.IsWorkOrderCreated == true) throw new InvalidPluginExecutionException("Work Order is already created");
                                ///////////////////////////////////////Temporary code neeed to remove once plugin on quote status//////////////////
                                //quote.quotetype = entity.GetAttributeValue<OptionSetValue>("ap360_quotetype") != null ? entity.GetAttributeValue<OptionSetValue>("ap360_quotetype").Value : 0;

                                //////////////////////////Move this line into getQuote funciton once pugin updated/////////////////////////
                                tracingService.Trace("Quote Reterived");

                                lstquoteServices = QuoteService.GetQuoteServices(service, tracingService, entity.Id);
                                List<QuoteService> updatedLstquoteServices = new List<QuoteService>();



                                foreach (QuoteService qtSrv in lstquoteServices)
                                {
                                    quoteService = new QuoteService();
                                    quoteService.EstimatedTime = qtSrv.EstimatedTime;
                                    quoteService.guid = qtSrv.guid;
                                    quoteService.Name = qtSrv.Name;
                                    quoteService.ServiceRole = qtSrv.ServiceRole;
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



                                    tracingService.Trace("Before Fetching Quote Product and Quote Service task");




                                    quoteService.lstquoteServiceTask = new List<QuoteServiceTask>();
                                    quoteService.lstquoteServiceTask = QuoteServiceTask.GetQuoteServiceTasks(service, qtSrv.guid);
                                    if (quoteService.lstquoteServiceTask.Count > 0)
                                        tracingService.Trace("Quote Service Task Count " + quoteService.lstquoteServiceTask.Count.ToString());


                                    updatedLstquoteServices.Add(quoteService);


                                }
                                int count = 0;



                                tracingService.Trace("Before Project Creation");

                                //Guid newlyCreatedProjectGuid = Project.CreateProject(service, tracingService, quote);
                                //tracingService.Trace("After Project Created : Guid " + newlyCreatedProjectGuid.ToString());
                                //foreach (QuoteService qtSrv in updatedLstquoteServices)
                                //{
                                //    count++;
                                //    tracingService.Trace("Record Number " + count.ToString() + " *********************");
                                //    tracingService.Trace("Quote Srv Name " + qtSrv.Name);

                                //    tracingService.Trace("Before Project Task Creation");
                                //    ProjectTask.CreateProjectTasks(service, tracingService, newlyCreatedProjectGuid, qtSrv, qtSrv.lstquoteServiceTask,ref count);
                                //    tracingService.Trace("Plugin Successfully ended");



                                //}

                            }
                        }

                    }

                }




            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }

}