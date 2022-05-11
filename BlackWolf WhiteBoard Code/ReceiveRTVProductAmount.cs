using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace BlackWolf_WhiteBoard_Code
{
    public class ReceiveRTVProductAmount : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();
            try
            {



                ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));



                IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));



                IOrganizationServiceFactory factory =
                (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = factory.CreateOrganizationService(context.UserId);





                tracingService.Trace("Plugin Started");



                //throw new InvalidPluginExecutionException("RTV is null");
                string selectedRTVProductGuids = (string)context.InputParameters["selectedRTVProducts"];



                List<string> lstRTVProducts = selectedRTVProductGuids.Split(',').ToList<string>();




                List<RTV_Product> lstRetrievedRTVProducts = RTV_Product.getRtVProducts(service, lstRTVProducts);




                foreach (RTV_Product RTVProduct in lstRetrievedRTVProducts)
                {
                    Entity updateRTVProduct = new Entity("msdyn_rtvproduct", RTVProduct.guid);
                    double TotalRTVReceived = RTVProduct.QuantityReceived + RTVProduct.PartialReceiveQuantity;
                    if (TotalRTVReceived == RTVProduct.Quantity)
                    {
                        updateRTVProduct["ap360_systemstatus"] = new OptionSetValue(126300002);//Refund Received
                    }
                    else if (TotalRTVReceived < RTVProduct.Quantity)
                    {
                        updateRTVProduct["ap360_systemstatus"] = new OptionSetValue(126300001);//Refund Partially Received

                    }
                    updateRTVProduct["ap360_quantityreceived"] = TotalRTVReceived;
                    updateRTVProduct["ap360_partialreceivequantity"] = 0.00;
                    service.Update(updateRTVProduct);

                }



                EntityReference RTVEntityRef = lstRetrievedRTVProducts[0].RTV;
                if (RTVEntityRef != null)
                {
                    var RTVEntity = service.Retrieve(RTVEntityRef.LogicalName, RTVEntityRef.Id, new ColumnSet("msdyn_systemstatus"));
                    var systemStatus = RTVEntity.GetAttributeValue<OptionSetValue>("msdyn_systemstatus").Value;
                    if (systemStatus == 690970004)//Cancelled
                    {
                        throw new InvalidPluginExecutionException("RTV Cancelled");
                    }
                    else if (systemStatus == 126300000 || systemStatus == 690970000 || systemStatus == 690970001)//Pending || Draft || Approved
                    {
                        throw new InvalidPluginExecutionException("RTV Product cannot be Received until it is shipped");
                    }
                    bool IsAnyProductReceived = false;
                    bool isAmountReceived = RTV.checkRTVProductStatus(service, tracingService, RTVEntityRef, ref IsAnyProductReceived);
                    Entity updateRTV = new Entity(RTVEntityRef.LogicalName, RTVEntityRef.Id);
                    if (isAmountReceived)
                    {
                        updateRTV["msdyn_systemstatus"] = new OptionSetValue(690970003);//Received
                        service.Update(updateRTV);
                    }
                    else if (IsAnyProductReceived && !isAmountReceived)
                    {



                        updateRTV["msdyn_systemstatus"] = new OptionSetValue(126300001);//Refund partially Received
                        service.Update(updateRTV);
                    }
                }
                else
                {
                    throw new InvalidPluginExecutionException("RTV is null");
                }




            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}