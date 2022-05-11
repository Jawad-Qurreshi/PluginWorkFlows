using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class RejectQuoteService : IPlugin
    {

        public void Execute(IServiceProvider serviceProvider)
        {
           

            try
            {
                //throw new InvalidPluginExecutionException("throw");F:\Upwork\Chris 2019\Black Wolf Server Codes\New Code By Me\BlackWolf WhiteBoard Code\BlackWolf WhiteBoard Code\CreateWorkOrder.cs
                ITracingService tracingService =
                  (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                tracingService.Trace("tracek");
            throw new InvalidPluginExecutionException("Error");

                IPluginExecutionContext context = (IPluginExecutionContext)
                    serviceProvider.GetService(typeof(IPluginExecutionContext));

                IOrganizationServiceFactory factory =
                     (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = factory.CreateOrganizationService(context.UserId);

                string quoteServiceIds = (string)context.InputParameters["SelectedQuoteServicesGUIDs"];


                List<string> lstQuoteServiceIds = quoteServiceIds.Split(',').ToList<string>();
                tracingService.Trace(" Count " + lstQuoteServiceIds.Count.ToString());
                if (lstQuoteServiceIds.Count > 0)
                {
                    tracingService.Trace("Inside  list of quote Service Ids : Count " + lstQuoteServiceIds.Count.ToString());
                    Entity quoteService = service.Retrieve("ap360_quoteservice", new Guid(lstQuoteServiceIds[0]), new ColumnSet("ap360_quoteid"));
                    if (quoteService == null) return;
                    EntityReference quote = quoteService.GetAttributeValue<EntityReference>("ap360_quoteid") != null ? quoteService.GetAttributeValue<EntityReference>("ap360_quoteid") : null;

                    if (quote == null) { throw new InvalidPluginExecutionException("Quote is not selelcted in Quote Service"); }
                    Entity initialQuote = service.Retrieve(quote.LogicalName, quote.Id, new ColumnSet(true));

                    Guid newQuoteGUID = Quote.CreateQuoteForRejectQuoteServices(service, tracingService, initialQuote);

                    tracingService.Trace("Before foreach ");

                    foreach (string quoteServiceId in lstQuoteServiceIds)
                    {
                        tracingService.Trace("Inside update new qoute in quotes services");

                        Entity updateQuoteInQuoteService = service.Retrieve("ap360_quoteservice", new Guid(quoteServiceId), new ColumnSet("ap360_quoteid"));
                        updateQuoteInQuoteService["ap360_quoteid"] = new EntityReference("quote", newQuoteGUID);
                        service.Update(updateQuoteInQuoteService);
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
