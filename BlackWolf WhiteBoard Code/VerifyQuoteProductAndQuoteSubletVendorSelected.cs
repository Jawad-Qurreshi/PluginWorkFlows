using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class VerifyQuoteProductAndQuoteSubletVendorSelected : IPlugin
    {

        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();
            // throw new InvalidPluginExecutionException("QuoteProductCalculation");

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


                //  tracingService.Trace("MoveQuoteServiceItemstoDifferentQuoteService");
                if (context.MessageName.ToLower() == "update")
                {
                    tracingService.Trace("ERROR");
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        entity = (Entity)context.InputParameters["Target"];
                        if (entity.LogicalName == "ap360_quoteservice")
                        {
                            if ((entity.Contains("statuscode")))
                            {
                                if (entity.GetAttributeValue<OptionSetValue>("statuscode").Value == 126300000)//Completed
                                {

                                  //  Entity postImage = (Entity)context.PostEntityImages["Image"];
                                  //  string quoteServiceName = postImage.GetAttributeValue<string>("ap360_workrequested");

                                    List<QuoteProduct> lstQuoteProducts = new List<QuoteProduct>();
                                    lstQuoteProducts = QuoteProduct.GetQuoteProducts(service, tracingService, entity.Id);
                                    foreach (QuoteProduct quoteProduct in lstQuoteProducts)
                                    {

                                        if (quoteProduct.Vendor == null)
                                        {
                                            throw new InvalidPluginExecutionException("Quote Proudct vendor is not selected in " + quoteProduct.ProductRef.Name);
                                        }
                                    }
                                    List<QuoteSublet> lstQuoteSublets = new List<QuoteSublet>();
                                    lstQuoteSublets = QuoteSublet.GetQuoteSublet(service, tracingService, entity.Id);

                                    foreach (QuoteSublet quoteSublet in lstQuoteSublets)
                                    {

                                        if (quoteSublet.Vendor == null)
                                        {
                                            throw new InvalidPluginExecutionException("Vendor is not selected in Quote Sublet");
                                        }
                                    }
                                    tracingService.Trace("Plugin Ended");
                                }
                            }
                        }

                    }
                }

                //throw new InvalidPluginExecutionException("Cutom error");
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}