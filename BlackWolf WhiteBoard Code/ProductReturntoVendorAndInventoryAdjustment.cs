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
    public class ProductReturntoVendorAndInventoryAdjustment : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();
            //  throw new InvalidPluginExecutionException("CreatePurchaseOrderforPreferredSupplier");

            try
            {

                ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = factory.CreateOrganizationService(context.UserId);

                string SeletectedWorkOrderProductIds = null;
                Entity workOrderProduct = null;
                EntityReference workOrderRef = null;
                Entity entity = null;

                if (context.MessageName.ToLower() == "ap360_ReturnToVendorForWorkOrderProduct".ToLower())
                {
                    SeletectedWorkOrderProductIds = (string)context.InputParameters["SeletectedWorkOrderProductIds"];

                }
                else if (context.MessageName.ToLower() == "update")
                {

                    tracingService.Trace("Context depth " + context.Depth.ToString());
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {

                        entity = (Entity)context.InputParameters["Target"];
                        if (entity.Contains("ap360_quantityreturn"))
                        {
                            SeletectedWorkOrderProductIds = entity.Id.ToString();

                        }
                    }

                }
                int workOrderProductLinestatus = 0;
                List<string> seletectedWorkOrderProductIdsList = SeletectedWorkOrderProductIds.Split(',').ToList<string>();
                if (seletectedWorkOrderProductIdsList.Count > 0)
                {
                    tracingService.Trace("Count of WO Products " + seletectedWorkOrderProductIdsList.Count.ToString());

                    bool isWorkOrderProductAlreadyNotUsed = false;
                    foreach (var selectedWOProductId in seletectedWorkOrderProductIdsList)
                    {

                        Entity reterviedWorkOrderProductEntity = service.Retrieve("msdyn_workorderproduct", new Guid(selectedWOProductId), new ColumnSet("msdyn_linestatus"));

                        if (reterviedWorkOrderProductEntity != null)
                        {
                            workOrderProductLinestatus = reterviedWorkOrderProductEntity.GetAttributeValue<OptionSetValue>("msdyn_linestatus").Value;
                            tracingService.Trace("WO Product Line Status " + workOrderProductLinestatus.ToString());
                            if (workOrderProductLinestatus == 126300000)//Not Used
                            {

                                isWorkOrderProductAlreadyNotUsed = true;
                            }

                        }

                    }
                    if (isWorkOrderProductAlreadyNotUsed)
                    {
                        throw new InvalidPluginExecutionException(" Selected WorkOrder Product(s) is already Returned To Vendor");
                    }


                    foreach (var selectedWOProductId in seletectedWorkOrderProductIdsList)
                    {
                        tracingService.Trace("Inside Foreach");
                        var QEmsdyn_workorderproduct = new QueryExpression("msdyn_workorderproduct");
                        QEmsdyn_workorderproduct.ColumnSet.AllColumns = true;
                        QEmsdyn_workorderproduct.Criteria.AddCondition("msdyn_workorderproductid", ConditionOperator.Equal, selectedWOProductId);

                        EntityCollection workOrderProductEntityCol = service.RetrieveMultiple(QEmsdyn_workorderproduct);
                        if (workOrderProductEntityCol.Entities.Any())
                        {
                            int reasonToReturn = 0;
                            bool ap360_isreturntoblackwolf = false;
                            workOrderProduct = workOrderProductEntityCol.Entities.FirstOrDefault();

                            double quantityReturn = workOrderProduct.GetAttributeValue<double>("ap360_quantityreturn");

                            if (quantityReturn > 0)
                            {
                                if (workOrderProduct.Contains("msdyn_workorder"))
                                {
                                    tracingService.Trace("Before updateWOProductForReturn");
                                    WorkOrderProduct.updateWOProductForReturn(service, tracingService, workOrderProduct);


                                    workOrderRef = workOrderProduct.GetAttributeValue<EntityReference>("msdyn_workorder");
                                    ap360_isreturntoblackwolf = workOrderProduct.GetAttributeValue<bool>("ap360_isreturntoblackwolf");

                                    reasonToReturn = workOrderProduct.GetAttributeValue<OptionSetValue>("ap360_reasontoreturn").Value;
                                    if (reasonToReturn != 126300000)// We Damaged
                                    {
                                        if (!ap360_isreturntoblackwolf)

                                        {

                                            EntityReference vendorRef = workOrderProduct.GetAttributeValue<EntityReference>("ap360_vendorid") != null ? workOrderProduct.GetAttributeValue<EntityReference>("ap360_vendorid") : null;
                                            if (vendorRef == null) return;
                                            Guid rtvDraftStatusGuid = RTV.getDraftRTVForSpecificVendor(service, vendorRef.Id);
                                            Guid rtvGuid = Guid.Empty;
                                            if (rtvDraftStatusGuid == Guid.Empty)
                                            {
                                                tracingService.Trace("Before CreateRTVForReturnWOProduct");
                                                rtvGuid = RTV.CreateRTVForReturnWOProduct(service, workOrderProduct);
                                            }
                                            else
                                            {
                                                rtvGuid = rtvDraftStatusGuid;
                                            }
                                            tracingService.Trace("Before CreateRTVProductForReturnWOProduct");
                                            RTV_Product.CreateRTVProductForReturnWOProduct(service, workOrderProduct, rtvGuid, workOrderRef, selectedWOProductId);
                                            //tracingService.Trace("Before CreateRTVForReturnWOProduct");
                                            //Guid newRTVId = RTV.CreateRTVForReturnWOProduct(service, workOrderProduct);
                                            //tracingService.Trace("Before CreateRTVProductForReturnWOProduct");
                                            //RTV_Product.CreateRTVProductForReturnWOProduct(service, workOrderProduct, newRTVId, workOrderRef, selectedWOProductId);
                                        }
                                    }
                                }

                                if (workOrderProductLinestatus == 690970001)//used
                                {
                                    InventoryAdjustment.CreateInventoryAdjustmentForReturnWOProducts(service, workOrderProduct);
                                }
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