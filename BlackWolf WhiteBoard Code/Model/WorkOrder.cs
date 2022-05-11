using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code.Model
{
    public class WorkOrder : EntityBase
    {

        public Guid guid { get; set; }
        public string Name { get; set; }
        public decimal woOriginalLaborAmount { get; set; }
        public decimal woActualLaborAmount { get; set; }
        public decimal woRevisedLaborAmount { get; set; }
        public decimal woEstimatedAmount { get; set; }
        public int WorkOrderType { get; set; }

        public int WBSID { get; set; }
        public int woBWStatus { get; set; }

        public OptionSetValueCollection WorkOrderBWStatus { get; set; }
        public string WorkOrderBWStatusText { get; set; }


        public static void updateWorkOrder(IOrganizationService service, ITracingService tracingservice, Guid workOrderGuid, decimal totaloriginalestimatepartsamount, int totaloriginalestimatedduration, decimal totaloriginalestimatedSubletamount)
        {
            //  Entity retrievedWorkOrder = service.Retrieve("msdyn_workorder", workOrderGuid, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));

            // int retrievedtotaloriginalestimatedduration = retrievedWorkOrder.GetAttributeValue<int>("ap360_totaloriginalestimatedduration");

            Entity entity = new Entity("msdyn_workorder");
            entity.Id = workOrderGuid;
            entity["ap360_totaloriginalestimatedduration"] = totaloriginalestimatedduration; // Setting through WorkOrderServcieTaskCalculation Create
            entity["ap360_totalrevisedestimatedduration"] = totaloriginalestimatedduration;

            entity["ap360_subletoriginalestimatedamount"] = totaloriginalestimatedSubletamount;
            entity["ap360_subletrevisedestimatedamount"] = totaloriginalestimatedSubletamount;

            entity["ap360_totaloriginalestimatepartsamount"] = new Money(totaloriginalestimatepartsamount);//Setting through  WorkOrderProductCalculation Create
            entity["ap360_totalrevisedestimatepartsamount"] = new Money(totaloriginalestimatepartsamount);
            service.Update(entity);
        }

        public static void UpdateWorkOrderForHealth(ITracingService tracingService, IOrganizationService service, EntityReference workOrderRef, Money workOrderEstimatedLaborAmount, decimal cumulativeSumofPredictedSpend, decimal postWorkOrderHealth)
        {
            Entity updateWorkOrder = new Entity(workOrderRef.LogicalName, workOrderRef.Id);
            updateWorkOrder["ap360_predictedspend"] = new Money(cumulativeSumofPredictedSpend);
            if (workOrderEstimatedLaborAmount != null && workOrderEstimatedLaborAmount.Value > 0)
            {

                updateWorkOrder["ap360_preworkorderhealth"] = postWorkOrderHealth;
                updateWorkOrder["ap360_postworkorderhealth"] = cumulativeSumofPredictedSpend / workOrderEstimatedLaborAmount.Value;

                tracingService.Trace("Work Order heaalth  is updating to " + (cumulativeSumofPredictedSpend / workOrderEstimatedLaborAmount.Value).ToString());
                service.Update(updateWorkOrder);
            }
        }

        public static void UpdateWorkOrderWOBWStatus(IOrganizationService service, ITracingService tracingService, EntityReference workOrderRef, int WoBWStatus)
        {
            if (workOrderRef != null)
            {
                Entity updateWorkOrder = new Entity(workOrderRef.LogicalName, workOrderRef.Id);
                if (WoBWStatus != 0)
                {
                    updateWorkOrder["ap360_wobwstatus"] = new OptionSetValue(WoBWStatus);
                    service.Update(updateWorkOrder);
                }
            }
        }
        public static int GetWOProjectTaskCreatedCount(List<Entity> lstWorkOrders)

        {
            int WOProjectTaskCreatedCount = 0;
            foreach (Entity workOrder in lstWorkOrders)
            {
                if (workOrder.GetAttributeValue<bool>("ap360_iswoprojecttaskcreated"))
                {
                    WOProjectTaskCreatedCount++;
                }
            }
            return WOProjectTaskCreatedCount;
        }
        public static Guid CreateStandardWorkOrderOnCreationOfOpportunity(IOrganizationService service, ITracingService tracingservcie, Guid accountGuid, Guid opportunityGuid, string task, EntityReference vehilceRef, decimal opportunityNumber, string standardWOType)
        {
            tracingservcie.Trace("inside CreateStandardWorkOrderOnCreationOfOpportunity ");

            Guid workorderGuid = Guid.Empty;
            try
            {
                Entity entity = new Entity("msdyn_workorder");

                if (standardWOType == "protocol")
                {
                    entity["ap360_workordername"] = "Protocol Work Order";
                    entity["ap360_workorderbwtype"] =new OptionSetValue(126300002);//Protocol

                    
                }
                else if (standardWOType == "admin")
                {
                    entity["ap360_workordername"] = "Admin Work Order";
                    entity["ap360_workorderbwtype"] = new OptionSetValue(126300008);//Admin

                }
                entity["msdyn_pricelist"] = new EntityReference("pricelevel", new Guid("4fe16dd5-8e55-ea11-a811-000d3a33f3c3"));
                entity["msdyn_serviceaccount"] = new EntityReference("account", accountGuid);
                entity["msdyn_opportunityid"] = new EntityReference("opportunity", opportunityGuid);

                if (vehilceRef != null)
                    entity["ap360_vehicleid"] = new EntityReference("ap360_vehicle", vehilceRef.Id);
                else
                    throw new InvalidPluginExecutionException("Vehicle is not selected");
                entity["ap360_opportunitynumber"] = Convert.ToInt32(opportunityNumber);

                entity["ap360_customwosubstatusid"] = new EntityReference("msdyn_workordersubstatus", new Guid("0d6048a8-310b-eb11-a813-000d3a33f3c3"));//Standard

                entity["ap360_servicerole"] = new EntityReference("bookableresourcecategory", new Guid("108c5d2f-245e-ea11-a812-000d3a30f257"));

                workorderGuid = service.Create(entity);


                //Entity reterivedWorkOrder = service.Retrieve("msdyn_workorder", workorderGuid, new Microsoft.Xrm.Sdk.Query.ColumnSet("msdyn_name"));
                //if (reterivedWorkOrder != null)
                //{
                //    Entity newWorkOrder = new Entity("msdyn_workorder");
                //    newWorkOrder["msdyn_name"] = task + " " + reterivedWorkOrder.GetAttributeValue<string>("msdyn_name");
                //    newWorkOrder.Id = reterivedWorkOrder.Id;
                //    service.Update(newWorkOrder);
                //}

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return workorderGuid;


        }
        public static void mapSubstatusToWorkOrder(Entity upDateWorkOrder, int systemStatus, string selectedWorkOrderSubStatus)
        {
            if (systemStatus == 690970003 || systemStatus == 690970000)//Open - Completed
            {


                if (selectedWorkOrderSubStatus == "needsparts")
                {
                    upDateWorkOrder["ap360_customwosubstatusid"] = new EntityReference("msdyn_workordersubstatus", new Guid("e5c1419c-2b5a-ea11-a811-000d3a30f195"));

                }
                else if (selectedWorkOrderSubStatus == "needsmoretime")
                {
                    upDateWorkOrder["ap360_customwosubstatusid"] = new EntityReference("msdyn_workordersubstatus", new Guid("07c2419c-2b5a-ea11-a811-000d3a30f195"));

                }
                else if (selectedWorkOrderSubStatus == "needssublet")
                {
                    upDateWorkOrder["ap360_customwosubstatusid"] = new EntityReference("msdyn_workordersubstatus", new Guid("ffc1419c-2b5a-ea11-a811-000d3a30f195"));

                }
                else if (selectedWorkOrderSubStatus == "needstechdirection")
                {
                    upDateWorkOrder["ap360_customwosubstatusid"] = new EntityReference("msdyn_workordersubstatus", new Guid("edc1419c-2b5a-ea11-a811-000d3a30f195"));

                }
                else if (selectedWorkOrderSubStatus == "needsdiscoveryestimated")
                {
                    upDateWorkOrder["ap360_customwosubstatusid"] = new EntityReference("msdyn_workordersubstatus", new Guid("78e176ba-390b-eb11-a813-000d3a33f3c3"));

                }
                else if (selectedWorkOrderSubStatus == "needsmanagerdecision")
                {
                    upDateWorkOrder["ap360_customwosubstatusid"] = new EntityReference("msdyn_workordersubstatus", new Guid("485bf046-fdea-ea11-a817-000d3a33f3c3"));

                }
                else if (selectedWorkOrderSubStatus == "completed")
                {
                    upDateWorkOrder["ap360_customwosubstatusid"] = new EntityReference("msdyn_workordersubstatus", new Guid("d1c1419c-2b5a-ea11-a811-000d3a30f195"));

                }
                else if (selectedWorkOrderSubStatus == "woinprogress")
                {
                    upDateWorkOrder["ap360_customwosubstatusid"] = new EntityReference("msdyn_workordersubstatus", new Guid("ff20bd3a-a158-ea11-a811-000d3a33f3c3"));//wo inprogress

                }
                else if (selectedWorkOrderSubStatus == "wocomplete")
                {
                    upDateWorkOrder["ap360_customwosubstatusid"] = new EntityReference("msdyn_workordersubstatus", new Guid("d1c1419c-2b5a-ea11-a811-000d3a30f195"));//completed

                }
                //else {
                //    upDateWorkOrder["ap360_customwosubstatusid"] = null;

                //}
            }

            //else if (systemStatus == 690970000)//Open - Unscheduled 
            //{

            //    if (selectedWorkOrderSubStatus == "needsparts")
            //    {
            //        upDateWorkOrder["msdyn_substatus"] = new EntityReference("msdyn_workordersubstatus", new Guid("f3c1419c-2b5a-ea11-a811-000d3a30f195"));
            //    }
            //    else if (selectedWorkOrderSubStatus == "needsmoretime")
            //    {
            //        upDateWorkOrder["msdyn_substatus"] = new EntityReference("msdyn_workordersubstatus", new Guid("f8b4381a-aaf1-ea11-a815-000d3a33f3c3"));

            //    }
            //    else if (selectedWorkOrderSubStatus == "needssublet")
            //    {
            //        upDateWorkOrder["msdyn_substatus"] = new EntityReference("msdyn_workordersubstatus", new Guid("e5f88a2c-aaf1-ea11-a815-000d3a33f3c3"));

            //    }
            //    else if (selectedWorkOrderSubStatus == "needstechdirection")
            //    {
            //        upDateWorkOrder["msdyn_substatus"] = new EntityReference("msdyn_workordersubstatus", new Guid("4834e644-aaf1-ea11-a815-000d3a33f3c3"));

            //    }
            //    else if (selectedWorkOrderSubStatus == "needsdiscoveryauthorization")
            //    {
            //        upDateWorkOrder["msdyn_substatus"] = new EntityReference("msdyn_workordersubstatus", new Guid("39531b57-aaf1-ea11-a815-000d3a33f3c3"));

            //    }
            //    else if (selectedWorkOrderSubStatus == "needsmanagerdecision")
            //    {
            //        upDateWorkOrder["msdyn_substatus"] = new EntityReference("msdyn_workordersubstatus", new Guid("4932a669-aaf1-ea11-a815-000d3a33f3c3"));

            //    }
            //    else if (selectedWorkOrderSubStatus == "completed")
            //    {
            //        upDateWorkOrder["msdyn_substatus"] = new EntityReference("msdyn_workordersubstatus", new Guid("46486907-ab05-eb11-a813-000d3a33f47e"));

            //    }


            //}

        }

        public static void mapWorkOrderFieldsToBooking(IOrganizationService service, ITracingService tracingservice, Entity bookingEntity, Guid workOrderGuid)
        {

            Entity reterviedWorkOrderEntity = service.Retrieve("msdyn_workorder", workOrderGuid, new Microsoft.Xrm.Sdk.Query.ColumnSet("ap360_vehicleid", "ap360_opportunitynumber"));
            if (reterviedWorkOrderEntity != null)
            {

                bookingEntity["ap360_vehicleid"] = reterviedWorkOrderEntity.GetAttributeValue<EntityReference>("ap360_vehicleid") != null ? reterviedWorkOrderEntity.GetAttributeValue<EntityReference>("ap360_vehicleid") : null;
                bookingEntity["ap360_opportuntiynumber"] = reterviedWorkOrderEntity.GetAttributeValue<EntityReference>("ap360_opportunitynumber");
                // bookingEntity[""] = reterviedWorkOrderEntity.GetAttributeValue<EntityReference>("") != null ? reterviedWorkOrderEntity.GetAttributeValue<EntityReference>("") : null;
            }
        }
        public static Guid CreateWorkOrder(IOrganizationService service, ITracingService tracingservcie, Quote quote, QuoteService quoteService)
        {
            tracingservcie.Trace("Inside function");
            // tracingservcie.Trace("bookableresourcecategory " + quoteService.ServiceRole.Id.ToString());
            tracingservcie.Trace("quote type " + quote.quotetype.ToString());
            // tracingservcie.Trace("pricelevel " + quote.priceList.Id.ToString());
            // tracingservcie.Trace("account " + quote.account.Id.ToString());
            if (quote.Opportunity != null)
                tracingservcie.Trace("Opprotunity " + quote.Opportunity.Id.ToString());
            else
                throw new InvalidPluginExecutionException("Opprotunity is not selected");

            Guid workorderGuid;
            try
            {
                Entity entity = new Entity("msdyn_workorder");

                if (quoteService.ServiceRole != null)
                    entity["ap360_servicerole"] = new EntityReference("bookableresourcecategory", quoteService.ServiceRole.Id);
                if (quoteService.Name != null)
                    //entity["msdyn_name"] = quoteService.Name;
                    entity["ap360_workordername"] = quoteService.Name;
                if (quoteService.HourlyRate != null)
                    entity["ap360_hourlyrate"] = new Money(quoteService.HourlyRate.Value);
                entity["msdyn_pricelist"] = new EntityReference("pricelevel", quote.priceList.Id);
                entity["msdyn_serviceaccount"] = new EntityReference("account", quote.account.Id);
                entity["msdyn_opportunityid"] = new EntityReference("opportunity", quote.Opportunity.Id);
                entity["ap360_opportunitynumber"] = Convert.ToInt32(quote.OpportunityAutoNumber);
                //   entity["ap360_wbsid"] = quoteService.wbsID;


                if (quoteService.ParentServiceTemplate != null)
                {
                    entity["ap360_parentservicetempalteid"] = new EntityReference("ap360_parentservicetemplatetype", quoteService.ParentServiceTemplate.Id);
                    tracingservcie.Trace("Parent Service Template " + quoteService.ParentServiceTemplate.Id.ToString());

                }
                if (quoteService.QuoteServiceType != null)
                {

                    entity["ap360_workorderbwtype"] = quoteService.QuoteServiceType;

                    List<OptionSetValue> lstOptionSets = new List<OptionSetValue>();
                    if (quoteService.QuoteServiceType.Value == 126300001 //Production OLD
                        || quoteService.QuoteServiceType.Value == 126300004//Production Mechanical
                        || quoteService.QuoteServiceType.Value == 126300005//Production BodyShop
                        || quoteService.QuoteServiceType.Value == 126300006//Production Electrical
                        || quoteService.QuoteServiceType.Value == 126300007//Production Upholstery
                        || quoteService.QuoteServiceType.Value == 126300008//Admin
                        )
                    {
                        lstOptionSets.Add(new OptionSetValue(126300006));//Awaiting Predecessor
                        entity["ap360_workorderbwstatus"] = new OptionSetValueCollection(lstOptionSets);
                    }
                    else if (quoteService.QuoteServiceType.Value == 126300002 ||//Protocol
                        quoteService.QuoteServiceType.Value == 126300000 || // Assesment- Inspection 
                        quoteService.QuoteServiceType.Value == 126300009)// Assessment- Diagnostic
                    {
                        lstOptionSets.Add(new OptionSetValue(126300008));//Available For Booking-InProgress
                        entity["ap360_workorderbwstatus"] = new OptionSetValueCollection(lstOptionSets);
                    }



                }
                if (quoteService.ServiceTemplate != null)
                {
                    entity["ap360_servicetemplateid"] = new EntityReference("ap360_servicetemplatetype", quoteService.ServiceTemplate.Id);
                    tracingservcie.Trace("Service Template " + quoteService.ServiceTemplate.Id.ToString());

                }
                if (quoteService.ChildServiceTemplate != null)
                {
                    entity["ap360_childservicetemplateid"] = new EntityReference("ap360_servicetemplatetype", quoteService.ChildServiceTemplate.Id);
                    tracingservcie.Trace("Child Service Template " + quoteService.ChildServiceTemplate.Id.ToString());

                }
                if (quoteService.ParentServiceTask != null)
                {
                    entity["ap360_parentservicetaskid"] = new EntityReference("msdyn_servicetasktype", quoteService.ParentServiceTask.Id);
                    tracingservcie.Trace("Parent Service Task " + quoteService.ParentServiceTask.Id.ToString());

                }

                if (quoteService.ServiceProductMapping != null)
                {
                    entity["ap360_serviceproductmappingid"] = new EntityReference("product", quoteService.ServiceProductMapping.Id);
                    tracingservcie.Trace("ServiceProductMapping " + quoteService.ServiceProductMapping.Id.ToString());

                }
                if (quoteService.GGParentProduct != null)
                {
                    entity["ap360_ggparentproductid"] = new EntityReference("product", quoteService.GGParentProduct.Id);
                    tracingservcie.Trace("GGParentProduct " + quoteService.GGParentProduct.Id.ToString());

                }
                if (quoteService.GParentProduct != null)
                {
                    entity["ap360_gparentproductid"] = new EntityReference("product", quoteService.GParentProduct.Id);
                    tracingservcie.Trace("GParentProduct " + quoteService.GParentProduct.Id.ToString());

                }
                entity["msdyn_taxable"] = false;


                entity["ap360_quoteserviceid"] = new EntityReference("ap360_quoteservice", quoteService.guid);
                // entity["msdyn_pricelist"] = new EntityReference("pricelevel", new Guid("4fe16dd5-8e55-ea11-a811-000d3a33f3c3"));//Black Wolf Price List
                entity["msdyn_taxcode"] = new EntityReference("msdyn_taxcode", new Guid("9ff0082c-70b1-ea11-a812-000d3a33f3c3"));//Tax Code




                // entity["ap360_workordertype"] = new OptionSetValue(quote.quotetype);
                entity["ap360_quoteid"] = new EntityReference("quote", quote.quoteGuid);
                if (quote.Vechicle != null)
                {
                    entity["ap360_vehicleid"] = new EntityReference("ap360_vehicle", quote.Vechicle.Id);
                    tracingservcie.Trace("Vechile " + quote.Vechicle.Id.ToString());

                }

                workorderGuid = service.Create(entity);
                //Entity reterivedWorkOrder = service.Retrieve("msdyn_workorder", workorderGuid, new Microsoft.Xrm.Sdk.Query.ColumnSet("msdyn_name"));
                //if (reterivedWorkOrder != null)
                //{
                //    Entity newWorkOrder = new Entity("msdyn_workorder");
                //    newWorkOrder["msdyn_name"] = quoteService.Name + " " + reterivedWorkOrder.GetAttributeValue<string>("msdyn_name");
                //    newWorkOrder.Id = reterivedWorkOrder.Id;
                //    service.Update(newWorkOrder);
                //}


                tracingservcie.Trace("Work order Created : Guid " + workorderGuid.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return workorderGuid;


        }

        public static void WorkOrderRetrieve(IPluginExecutionContext executionContext, IOrganizationService organizationService, ITracingService tracingService)
        {
            if (executionContext.OutputParameters.Contains("BusinessEntity"))
            {
                Entity entity = (Entity)executionContext.OutputParameters["BusinessEntity"];
                CalculateWorkOrderActualLaborAmount(organizationService, tracingService, entity);
            }
            else if (executionContext.OutputParameters.Contains("BusinessEntityCollection"))
            {
                var businessEntityCollection = (EntityCollection)executionContext.OutputParameters["BusinessEntityCollection"];
                tracingService.Trace("Record Count:" + businessEntityCollection.Entities.Count);
                foreach (Entity entity in businessEntityCollection.Entities)
                {
                    CalculateWorkOrderActualLaborAmount(organizationService, tracingService, entity);
                }
            }
        }


        private static void CalculateWorkOrderActualLaborAmount(IOrganizationService organizationService, ITracingService tracingService, Entity entity)
        {

            tracingService.Trace("CalculateWorkOrderActualLaborAmount Inside function ");

            //var workorderEntityRef = GetLookupAttributeValue(entity, "msdyn_workorder");
            var workorderEntityRef = entity;
            tracingService.Trace("Work Order Refereeence");
            if (workorderEntityRef == null) return;

            var ServiceTaskEntityRef = GetLookupAttributeValue(entity, "ap360_workorderservicetask");


            DataCollection<Entity> bookings = null;

            bookings = BookableResourceBooking.GetWorkOrderBookings(organizationService, tracingService, workorderEntityRef.Id);


            decimal totalBillingPrice = 0.0m;
            int totalTimeSpentOnWorkOrder = 0;
            int estimatedTime = 0;
            decimal reportedPercentComplete = 0.0m;
            decimal estimatedAmount = 0.0m;

            foreach (Entity workOrderBooking in bookings)
            {
                var serviceRoleEntityRef = GetLookupAttributeValue(workOrderBooking, "ap360_serviceroles");
                if (serviceRoleEntityRef == null) continue;

                var billingFactor = GetDecimalAttributeValue(workOrderBooking, "ap360_billingfactor");
                var timeSpentonWorkOrder = GetIntAttributeValue(workOrderBooking, "ap360_timespentonbooking");
                Guid blackWolfPriceListGuid = new Guid("4fe16dd5-8e55-ea11-a811-000d3a33f3c3");

                var price = BookableResourceBooking.GetRolePrice(organizationService, tracingService, blackWolfPriceListGuid, serviceRoleEntityRef.Id);
                var billingPrice = ((decimal)timeSpentonWorkOrder / 60) * (billingFactor * price);
                var bookingPercentComplete = GetDecimalAttributeValue(workOrderBooking, "ap360_calculatedbookingpercentcomplete");
                var calculatedEstimatedTime = GetIntAttributeValue(entity, "ap360_estimatedtime");


                tracingService.Trace("EstimatedTime:" + estimatedTime);
                tracingService.Trace("bookingPercentComplete:" + bookingPercentComplete);
                tracingService.Trace("billingFactor:" + billingFactor);
                tracingService.Trace("timeSpentonWorkOrder:" + timeSpentonWorkOrder);
                tracingService.Trace("Role price:" + price);
                tracingService.Trace("Billing price:" + billingPrice);

                totalBillingPrice = totalBillingPrice + billingPrice;
                totalTimeSpentOnWorkOrder = totalTimeSpentOnWorkOrder + timeSpentonWorkOrder;

            }

            tracingService.Trace("Estimated Time:" + estimatedTime);
            tracingService.Trace("Total Time Spent:" + totalTimeSpentOnWorkOrder);
            tracingService.Trace("Total Billing Amount:" + totalBillingPrice);
            tracingService.Trace("reportedPercentComplete:" + reportedPercentComplete);
            tracingService.Trace("Estimated Amount:" + estimatedAmount);

            if (estimatedAmount == 0.0m) return;


            if (entity.Contains("ap360_totalactuallaboramount")) { }
            //  entity["ap360_totalactuallaboramount"] = new Money(totalTimeSpentOnWorkOrder);
            else { }
            // entity.Attributes.Add("ap360_totalactuallaboramount", new Money(totalTimeSpentOnWorkOrder));



        }

        public static WorkOrder getWorkOrderEstimatedAndAcutalAmount(IOrganizationService service, ITracingService tracingService, Guid workOrderGuid)
        {
            WorkOrder workOrder = new WorkOrder();
            List<Entity> lstEntities = WorkOrderServiceTask.GetWOSTrelatedToWorkOrder(service, tracingService, workOrderGuid);

            foreach (Entity entity in lstEntities)
            {
                decimal zero = 0.0m;
                if (entity.GetAttributeValue<Money>("ap360_originalestimatedamount") != null)
                {
                    workOrder.woEstimatedAmount += entity.GetAttributeValue<Money>("ap360_originalestimatedamount") != null ? entity.GetAttributeValue<Money>("ap360_originalestimatedamount").Value : zero;
                }
                if (entity.GetAttributeValue<Money>("ap360_revisedestimatedamount") != null)
                {
                    workOrder.woEstimatedAmount += entity.GetAttributeValue<Money>("ap360_revisedestimatedamount") != null ? entity.GetAttributeValue<Money>("ap360_revisedestimatedamount").Value : zero;
                }
                workOrder.woActualLaborAmount += entity.GetAttributeValue<Money>("ap360_actualamount") != null ? entity.GetAttributeValue<Money>("ap360_actualamount").Value : zero;

            }

            return workOrder;
        }
        public static Money getWorkOrderEstimatedValue(ITracingService tracingService, Entity workOrder, Money EstimatedLaborAmount)
        {

            //if (workOrder.Contains("ap360_totaloriginalestimatedlaboramount"))
            //{

            //    EstimatedLaborAmount.Value += workOrder.GetAttributeValue<Money>("ap360_totaloriginalestimatedlaboramount") != null ? workOrder.GetAttributeValue<Money>("ap360_totaloriginalestimatedlaboramount").Value : 0;
            //    tracingService.Trace("ap360_totaloriginalestimatedlaboramount = EstimatedLaborAmount.Value " + EstimatedLaborAmount.Value.ToString());
            //}
            if (workOrder.Contains("ap360_totalrevisedestimatedlaboramount") && (EstimatedLaborAmount != null && EstimatedLaborAmount.Value <= 0))
            {

                EstimatedLaborAmount.Value += workOrder.GetAttributeValue<Money>("ap360_totalrevisedestimatedlaboramount") != null ? workOrder.GetAttributeValue<Money>("ap360_totalrevisedestimatedlaboramount").Value : 0;
                tracingService.Trace("ap360_totalrevisedestimatedlaboramount = EstimatedLaborAmount.Value " + EstimatedLaborAmount.Value.ToString());
            }

            return EstimatedLaborAmount;

        }
        public static List<Entity> GetWorkOrderRelatedtoQuote(IOrganizationService service, ITracingService tracingService, Guid quoteGuid)
        {

            List<Entity> lstEntities = new List<Entity>();

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                      <entity name='msdyn_workorder'>
                                        <attribute name='msdyn_name' />
                                        <attribute name='createdon' />
                                        <attribute name='ap360_workordername' />
                                        <attribute name='ap360_iswoprojecttaskcreated' />

                                        <attribute name='msdyn_serviceaccount' />
                                        <attribute name='msdyn_workorderid' />
                                        <attribute name='msdyn_functionallocation' />
                                        <attribute name='ap360_wbsid' />

                                         <order attribute='ap360_wbsid' descending='false' />
                                        <filter type='and'>
                                          <condition attribute='ap360_quoteid' operator='eq'  value='" + quoteGuid + @"' /> 
                                        </filter>
                                      </entity>
                                    </fetch>");



            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));

            foreach (Entity ent in col.Entities)
            {

                lstEntities.Add(ent);

            }
            return lstEntities;
        }



        public static List<Entity> GetWorkOrderRelatedtoOpportuntiy(IOrganizationService service, ITracingService tracingService, Guid OpportuntiyGuid)
        {



            List<Entity> lstEntities = new List<Entity>();

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                      <entity name='msdyn_workorder'>
                                        <attribute name='msdyn_name' />
                                        <attribute name='createdon' />
                                        <attribute name='ap360_workordername' />
                                        <attribute name='msdyn_systemstatus' />
                                        <attribute name='ap360_wbsid' />
                                        <attribute name='ap360_wobwstatus' />
                                        <attribute name='ap360_workorderbwstatus' />
                                        <attribute name='ap360_workorderbwstatustext' />


                                         <order attribute='createdon' descending='false' />
                                        <filter type='and'>
                                          <condition attribute='msdyn_opportunityid' operator='eq'  value='" + OpportuntiyGuid + @"' /> 
                                        </filter>
                                      </entity>
                                    </fetch>");



            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));

            foreach (Entity ent in col.Entities)
            {

                lstEntities.Add(ent);

            }
            return lstEntities;
        }

        public static void updateWorkOrderToAvailableForBookingInProgress(IOrganizationService service, ITracingService tracingService, Guid OpportuntiyGuid, int completedWorkOrderwbsId)
        {
            tracingService.Trace("inside isWorkOrderEligibleForInProgress");

            List<WorkOrder> lstWorkOrders = new List<WorkOrder>();
            lstWorkOrders = getListofWorkOrderRelatedToOpportunity(service, tracingService, OpportuntiyGuid);
            lstWorkOrders = lstWorkOrders.Where(x => x.WBSID > 0).ToList();
            var sortedWorkOrdersByWBSID = lstWorkOrders.OrderBy(x => x.WBSID);

            List<WorkOrder> lstofSortedWorkOrders = sortedWorkOrdersByWBSID.ToList();
            int currentItem = 0;
            foreach (WorkOrder sortedWorkOrder in lstofSortedWorkOrders)
            {

                bool isNextWorkOrderISINProgress = false;
                if (currentItem + 1 == lstofSortedWorkOrders.Count) return;// for last workOrder
                if (IsWorkOrderCompleted(sortedWorkOrder))//Completed
                {
                    if (IsAnyWorkOrderInProgressLowerThanCompletedWorkOrder(lstofSortedWorkOrders.Where(x => x.WBSID < completedWorkOrderwbsId).ToList()))
                    {
                        //if (IsAnyWorkOrderInProgress(lstofSortedWorkOrders))
                        //{
                        isNextWorkOrderISINProgress = isNextWorkOrderAvailableForBooking(lstofSortedWorkOrders[currentItem + 1]);

                        if (isNextWorkOrderISINProgress)
                        {
                            WorkOrder.updateWorkOrderBWStatusToInprogress(service, lstofSortedWorkOrders, currentItem);
                            break;
                        }
                        // }
                    }
                }
                currentItem++;

            }
        }
        public static List<WorkOrder> getListofWorkOrderRelatedToOpportunity(IOrganizationService service, ITracingService tracingService, Guid opportunityGuid)
        {
            tracingService.Trace("**********************************getListofWorkOrderRelatedToOpportunity");
            List<WorkOrder> lstWorkOrders = new List<WorkOrder>();

            List<Entity> reterviedLstWorkOrders = WorkOrder.GetWorkOrderRelatedtoOpportuntiy(service, tracingService, opportunityGuid);
            tracingService.Trace("Count of WorkORder " + reterviedLstWorkOrders.Count.ToString());
            WorkOrder wrkOrder = null;
            foreach (Entity reterviedWorkOrder in reterviedLstWorkOrders)
            {
                wrkOrder = new WorkOrder();

                wrkOrder.guid = reterviedWorkOrder.Id;
                //msdyn_name
                wrkOrder.Name = reterviedWorkOrder.GetAttributeValue<string>("msdyn_name");
                wrkOrder.WBSID = reterviedWorkOrder.GetAttributeValue<int>("ap360_wbsid");
                if (reterviedWorkOrder.GetAttributeValue<OptionSetValue>("ap360_wobwstatus") != null)
                    wrkOrder.woBWStatus = reterviedWorkOrder.GetAttributeValue<OptionSetValue>("ap360_wobwstatus").Value;

                wrkOrder.WorkOrderBWStatus = reterviedWorkOrder.GetAttributeValue<OptionSetValueCollection>("ap360_workorderbwstatus");
                wrkOrder.WorkOrderBWStatusText = reterviedWorkOrder.GetAttributeValue<string>("ap360_workorderbwstatustext");

                //if (wrkOrder.WBSID > 0)//only for workorders where id is not assigned
                lstWorkOrders.Add(wrkOrder);

            }
            tracingService.Trace("**********************************End getListofWorkOrderRelatedToOpportunity");

            return lstWorkOrders;
        }
        public static void updateWorkOrderBWStatusToInprogress(IOrganizationService service, List<WorkOrder> lstofSortedWorkOrders, int wbsID)
        {
            Entity updateWorkOrderEntity = new Entity("msdyn_workorder", lstofSortedWorkOrders[wbsID + 1].guid);
            List<OptionSetValue> lstOptionSets = new List<OptionSetValue>();
            lstOptionSets.Add(new OptionSetValue(126300008));
            updateWorkOrderEntity["ap360_workorderbwstatus"] = new OptionSetValueCollection(lstOptionSets);
            updateWorkOrderEntity["ap360_workorderbwstatustext"] = 126300008.ToString();//Available For Booking-InProgress
            updateWorkOrderEntity["ap360_workorderbwstatusformatedvalue"] = "Available For Booking-InProgress";

            service.Update(updateWorkOrderEntity);
        }
        public static int getWorkOrderWBSIDAwaitingPredecessorLowerThanCompletedWorkOrder(List<WorkOrder> lstWorkOrder)
        {
            int WBSIDAwaitingPredecessorLowerThanCompletedWorkOrder = 0;
            int count = 0;
            foreach (WorkOrder workOrder in lstWorkOrder)
            {
                OptionSetValueCollection workOrderBWStatusValueCollection = workOrder.WorkOrderBWStatus;

                foreach (var option in workOrderBWStatusValueCollection)
                {
                    int value = option.Value;// Value of options.
                    if (value == 126300006)//Awaiting Predecessor
                    {
                        if (WBSIDAwaitingPredecessorLowerThanCompletedWorkOrder == 0)
                        {
                            WBSIDAwaitingPredecessorLowerThanCompletedWorkOrder = count;
                            break;
                        }
                    }
                }
                count++;

                ////updateWorkOrderEntity["ap360_wobwstatus"] = new OptionSetValue(126300002);//Available For Booking Sandbox
                ////                if (workOrder.woBWStatus == 126300008)///Available For Booking-InProgress
                //if (workOrder.WorkOrderBWStatusText != null && workOrder.WorkOrderBWStatusText.Contains("126300006"))//Awaiting Predecessor

                //{

                //}
            }


            return WBSIDAwaitingPredecessorLowerThanCompletedWorkOrder;
        }
        public static bool IsAnyWorkOrderInProgress(List<WorkOrder> lstWorkOrder, ITracingService tracing)
        {
            tracing.Trace("inside IsAnyWorkOrderInProgress " + lstWorkOrder.Count);
            bool isAnyWOInProgress = false;
            int count = 0;
            foreach (WorkOrder workOrder in lstWorkOrder)
            {
                count++;
                if (workOrder.WorkOrderBWStatus != null)
                {
                    tracing.Trace("WorkOrder BW status not null " + count.ToString());
                    OptionSetValueCollection workOrderBWStatusValueCollection = workOrder.WorkOrderBWStatus;
                    foreach (var option in workOrderBWStatusValueCollection)
                    {
                        if (option != null)
                        {
                            int value = option.Value;// Value of options.
                            if (value == 126300008)//Available For Booking-InProgress
                            {
                                isAnyWOInProgress = true;

                                break;

                            }
                        }
                    }
                }
                //if (workOrder.WorkOrderBWStatusText != null && workOrder.WorkOrderBWStatusText.Contains("126300008"))///Available For Booking-InProgress
                //{
                //    IsWorkOrderInProgressLowerThanCompletedWorkOrder = false;
                //}
            }


            return isAnyWOInProgress;
        }
        public static int getNumberofWorkOrderCountInProgress(List<WorkOrder> lstWorkOrder, ITracingService tracing)
        {
            tracing.Trace("************inside getNumberofWorkOrderCountInProgress " + lstWorkOrder.Count);
            int countAnyWOInProgress = 0;
            foreach (WorkOrder workOrder in lstWorkOrder)
            {
                if (workOrder.WorkOrderBWStatus != null)
                {
                    tracing.Trace("work order BW status is not null");
                    OptionSetValueCollection workOrderBWStatusValueCollection = workOrder.WorkOrderBWStatus;
                    foreach (var option in workOrderBWStatusValueCollection)
                    {
                        if (option != null)
                        {
                            int value = option.Value;// Value of options.
                            if (value == 126300008)//Available For Booking-InProgress
                            {
                                tracing.Trace("WO is in progress " + value.ToString());
                                countAnyWOInProgress++;


                            }
                        }
                    }
                }
                //if (workOrder.WorkOrderBWStatusText != null && workOrder.WorkOrderBWStatusText.Contains("126300008"))///Available For Booking-InProgress
                //{
                //    IsWorkOrderInProgressLowerThanCompletedWorkOrder = false;
                //}
            }
            tracing.Trace("Nubmer of WorkOrder in progress " + countAnyWOInProgress.ToString());
            tracing.Trace("************inside getNumberofWorkOrderCountInProgress ");

            return countAnyWOInProgress;
        }

        //This plugin check if there is any workorder in progress whoose WBSID is less then the completed work order
        public static bool IsAnyWorkOrderInProgressLowerThanCompletedWorkOrder(List<WorkOrder> lstWorkOrder)
        {
            bool IsWorkOrderInProgressLowerThanCompletedWorkOrder = true;

            foreach (WorkOrder workOrder in lstWorkOrder)
            {
                OptionSetValueCollection workOrderBWStatusValueCollection = workOrder.WorkOrderBWStatus;
                foreach (var option in workOrderBWStatusValueCollection)
                {
                    int value = option.Value;// Value of options.
                    if (value == 126300008)//Available For Booking-InProgress
                    {
                        IsWorkOrderInProgressLowerThanCompletedWorkOrder = false;

                        break;

                    }
                }

                //if (workOrder.WorkOrderBWStatusText != null && workOrder.WorkOrderBWStatusText.Contains("126300008"))///Available For Booking-InProgress
                //{
                //    IsWorkOrderInProgressLowerThanCompletedWorkOrder = false;
                //}
            }


            return IsWorkOrderInProgressLowerThanCompletedWorkOrder;
        }
        public static bool IsWorkOrderCompleted(WorkOrder workOrder)
        {
            bool IsWorkOrderCompleted = false;
            OptionSetValueCollection workOrderBWStatusValueCollection = workOrder.WorkOrderBWStatus;
            foreach (var option in workOrderBWStatusValueCollection)
            {
                int value = option.Value;// Value of options.
                if (value == 126300001)//completed
                {
                    IsWorkOrderCompleted = true;
                    break;
                }
            }
            return IsWorkOrderCompleted;
        }


        public static void updateWOBWStatus(IOrganizationService service, ITracingService tracingService, IDictionary<OptionSetValue, string> lstWorkOrderBWStatusCollection, EntityReference workOrderRef)
        {

            tracingService.Trace("lstWorkOrderBWStatusCollection for matrix is greater then 0 : " + lstWorkOrderBWStatusCollection.Count.ToString());
            List<OptionSetValue> lstWorkOrderBWStatusKeys = new List<OptionSetValue>(lstWorkOrderBWStatusCollection.Keys);
            List<string> lstWorkOrderBWStatusNames = new List<string>(lstWorkOrderBWStatusCollection.Values);

            Entity updateWorkOrder = new Entity(workOrderRef.LogicalName, workOrderRef.Id);
            updateWorkOrder["ap360_workorderbwstatus"] = new OptionSetValueCollection(lstWorkOrderBWStatusKeys);
            string str = WorkOrderServiceTask.getWOSTStatusText(lstWorkOrderBWStatusKeys);
            tracingService.Trace("str " + str);
            updateWorkOrder["ap360_workorderbwstatustext"] = str;
            string formatedNames = WorkOrderServiceTask.getWOSTStatusFormatedValue(lstWorkOrderBWStatusNames);
            updateWorkOrder["ap360_workorderbwstatusformatedvalue"] = formatedNames;
            service.Update(updateWorkOrder);
        }
        public static IDictionary<OptionSetValue, string> getWorkOrderBWStatus(IOrganizationService service, ITracingService tracing, Guid workOrderGuid)
        {
            tracing.Trace("insdie getWorkOrderBWStatus");
            IDictionary<OptionSetValue, string> lstWorkOrderBWStatusOption = new Dictionary<OptionSetValue, string>();

            Entity reterivedWorkOrder = service.Retrieve("msdyn_workorder", workOrderGuid, new ColumnSet("ap360_workorderbwstatus"));
            if (reterivedWorkOrder != null)
            {

                OptionSetValueCollection workOrderBWStatusValueCollection = reterivedWorkOrder.GetAttributeValue<OptionSetValueCollection>("ap360_workorderbwstatus");
                if (workOrderBWStatusValueCollection != null)
                {
                    tracing.Trace("insdie getWorkOrderBWStatus workOrder retervied " + workOrderBWStatusValueCollection.Count.ToString());
                    foreach (var option in workOrderBWStatusValueCollection)
                    {
                        lstWorkOrderBWStatusOption.Add(option, "");
                    }
                }
            }
            tracing.Trace("end getWorkOrderBWStatus");
            return lstWorkOrderBWStatusOption;

        }


        public static bool isNextWorkOrderAvailableForBooking(WorkOrder nextWorkOrder)
        {
            bool isNextWorkOrderISINProgress = true;
            OptionSetValueCollection workOrderBWStatusValueCollection = nextWorkOrder.WorkOrderBWStatus;
            foreach (var option in workOrderBWStatusValueCollection)
            {
                int value = option.Value;// Value of options.
                if (value == 126300001 || value == 126300008)//completed ||Available For Booking-InProgress
                {
                    isNextWorkOrderISINProgress = false;
                    break;

                }
            }
            //if (nextWorkOrder.WorkOrderBWStatusText != null && nextWorkOrder.WorkOrderBWStatusText.Contains("126300001"))//Approved By Production Manager
            //{
            //    isNextWorkOrderISINProgress = false;
            //}

            return isNextWorkOrderISINProgress;

        }
        public static EntityCollection GetNextWorkOrderBasedOnWBSID(IOrganizationService service, ITracingService tracingService, Guid opportunityGuid, int wbsId)
        {

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='msdyn_workorder'>
                                    <attribute name='msdyn_name' />
                                    <attribute name='createdon' />
                                    <attribute name='msdyn_serviceaccount' />
                                    <attribute name='msdyn_workorderid' />
                                    <attribute name='ap360_wbsid' />

                                    <attribute name='msdyn_functionallocation' />
                                    <order attribute='msdyn_name' descending='false' />
                                    <filter type='and'>
                                      <condition attribute='ap360_wbsid' operator='eq' value='" + wbsId + @"' />
                                      <condition attribute='msdyn_opportunityid' operator='eq'  value='" + opportunityGuid + @"' /> 
                                    </filter>
                                  </entity>
                                </fetch>");



            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));


            return col;
        }


        public static void updateWorkOrderFields(IOrganizationService service, Entity entity)
        {
            Entity workOrder = new Entity("msdyn_workorder");
            workOrder.Id = entity.GetAttributeValue<EntityReference>("msdyn_workorder") != null ? entity.GetAttributeValue<EntityReference>("msdyn_workorder").Id : Guid.Empty;
            EntityReference wopercentComplete = entity.GetAttributeValue<EntityReference>("ap360_percentagecomplete") != null ? entity.GetAttributeValue<EntityReference>("ap360_percentagecomplete") : null;
            if (wopercentComplete != null)
                workOrder["ap360_workordercompleteid"] = new EntityReference(wopercentComplete.LogicalName, wopercentComplete.Id);

            //EntityReference startpercentComplete = entity.GetAttributeValue<EntityReference>("ap360_workorderstartingcomplete") != null ? entity.GetAttributeValue<EntityReference>("ap360_workorderstartingcomplete") : null;
            //if (startpercentComplete != null)
            //    workOrder["ap360_workordercompleteid"] = new EntityReference(startpercentComplete.LogicalName, startpercentComplete.Id);


            service.Update(workOrder);

        }

        public static string getWorkOrderBWStatusName(int statusOption)
        {
            string statusName = "";
            if (statusOption == 126300002)
            {
                statusName = "Available For Booking";
            }
            else if (statusOption == 126300003)
            {
                statusName = "Needs Admin FollowUp";
            }
            else if (statusOption == 126300004)
            {
                statusName = "Waiting For Client";
            }
            else if (statusOption == 126300005)
            {
                statusName = "Approved By Production Manager";
            }
            else if (statusOption == 126300006)
            {
                statusName = "Awaiting Predecessor";
            }
            else if (statusOption == 126300007)
            {
                statusName = "Awaiting Dependency";
            }
            else if (statusOption == 126300008)
            {
                statusName = "Available For Booking-InProgress";
            }
            else if (statusOption == 126300000)
            {
                statusName = "In Progress";
            }
            else if (statusOption == 126300001)
            {
                statusName = "Completed";
            }

            return statusName;
        }
        public static string getWorkOrderFieldName(string entityLogicalName)
        {
            string fieldName = "";
            if (entityLogicalName == "msdyn_workorderservicetask")
            {
                fieldName = "msdyn_workorder";
            }
            else if (entityLogicalName == "msdyn_workorderproduct")
            {
                fieldName = "msdyn_workorder";
            }
            else if (entityLogicalName == "ap360_workordersublet")
            {
                fieldName = "ap360_workorderid";
            }
            else if (entityLogicalName == "bookableresourcebooking")
            {
                fieldName = "msdyn_workorder";
            }


            return fieldName;
        }

        public static List<OptionSetValue> getExistingWorkOrderBWStatus(Entity workOrder)
        {

            List<OptionSetValue> lstWorkOrderBWStatus = new List<OptionSetValue>();
            OptionSetValueCollection op = (OptionSetValueCollection)workOrder["ap360_workorderbwstatus"];
            foreach (var options in op)
            {

                lstWorkOrderBWStatus.Add(options);

            }
            return lstWorkOrderBWStatus;
        }


    }



}