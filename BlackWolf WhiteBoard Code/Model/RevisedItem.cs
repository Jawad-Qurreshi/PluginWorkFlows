using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code.Model
{
    public class RevisedItem
    {

        public Guid guid { get; set; }
        public string Name { get; set; }
        public double Quantity { get; set; }
        public Money UnitPrice { get; set; }

        public Money ExtendedPrice { get; set; }
        public EntityReference Opportunity { get; set; }
        public EntityReference WorkOrder { get; set; }

        public EntityReference WOServiceTask { get; set; }
        public EntityReference WOProduct { get; set; }
        public EntityReference WOSublet { get; set; }
        public EntityReference WOItem { get; set; }
        public int RevisedItemStatus { get; set; }
        public int ItemType { get; set; }
      

        public static void CreateRevisedItem(IOrganizationService service, ITracingService tracingService, RevisedItem revisedItem)
        {
            tracingService.Trace("Inside Revised Item creation");

            Entity newRevisedItem = new Entity("ap360_reviseditem");
            newRevisedItem["ap360_name"] = revisedItem.Name;
            newRevisedItem["ap360_quantity"] = Convert.ToDecimal(revisedItem.Quantity);
            newRevisedItem["ap360_reviseditemstatus"] = new OptionSetValue(revisedItem.RevisedItemStatus);
            if (revisedItem.UnitPrice != null)
            {
                newRevisedItem["ap360_unitprice"] = new Money(revisedItem.UnitPrice.Value);
            }
            if (revisedItem.ExtendedPrice != null)
            {
                newRevisedItem["ap360_extendedprice"] = new Money(revisedItem.ExtendedPrice.Value);

            }
            if (revisedItem.WorkOrder != null)
            {
                newRevisedItem["ap360_workorderid"] = new EntityReference(revisedItem.WorkOrder.LogicalName, revisedItem.WorkOrder.Id);
                tracingService.Trace("workOrder mapped");

            }
            if (revisedItem.Opportunity != null)
            {
                newRevisedItem["ap360_opportunityid"] = new EntityReference(revisedItem.Opportunity.LogicalName, revisedItem.Opportunity.Id);
                tracingService.Trace("opporutnity mapped");

            }

            if (revisedItem.WOServiceTask != null)
            {
                newRevisedItem["ap360_workorderservicetaskid"] = new EntityReference(revisedItem.WOServiceTask.LogicalName, revisedItem.WOServiceTask.Id);
                newRevisedItem["ap360_itemtype"] = new OptionSetValue(126300000);//Item Type Wo Srv Task Not Revised Item Status
                newRevisedItem["ap360_itemtypenametext"] = "WO Service Task";


            }
            else if (revisedItem.WOProduct != null)
            {
                newRevisedItem["ap360_workorderproductid"] = new EntityReference(revisedItem.WOProduct.LogicalName, revisedItem.WOProduct.Id);
                newRevisedItem["ap360_itemtype"] = new OptionSetValue(126300001);//Item Type Wo Product Not Revised Item Status
                newRevisedItem["ap360_itemtypenametext"] = "WO Product";


            }
            else if (revisedItem.WOSublet != null)
            {
                newRevisedItem["ap360_workordersubletid"] = new EntityReference(revisedItem.WOSublet.LogicalName, revisedItem.WOSublet.Id);
                newRevisedItem["ap360_itemtype"] = new OptionSetValue(126300002);//Item Type Wo Sublet Not Revised Item Status
                newRevisedItem["ap360_itemtypenametext"] = "WO Sublet";


            }
           // throw new InvalidPluginExecutionException("just before creation wop is revised");

            service.Create(newRevisedItem);
            tracingService.Trace("After creation");


        }

        public static RevisedItem GetRevisedItem(IOrganizationService service, ITracingService tracing, Entity deletingEntity)
        {

            RevisedItem reterviedRevisedItem = null;
            EntityReference opportunityRef = null;
            opportunityRef = deletingEntity.GetAttributeValue<EntityReference>("ap360_opportunityid") != null ? deletingEntity.GetAttributeValue<EntityReference>("ap360_opportunityid") : null;

            if (opportunityRef == null) return reterviedRevisedItem;

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='ap360_reviseditem'>
                                    <attribute name='ap360_reviseditemid' />
                                    <attribute name='ap360_name' />
                                    <attribute name='createdon' />
                                    <attribute name='ap360_workordersubletid' />
                                    <attribute name='ap360_workorderservicetaskid' />
                                    <attribute name='ap360_workorderproductid' />
                                    <order attribute='ap360_name' descending='false' />
                                    <filter type='and'>
                                      <condition attribute='ap360_opportunityid' operator='eq' value='" + opportunityRef.Id + @"' /> 
                                    </filter>
                                  </entity>
                                </fetch>");


            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            tracing.Trace("count " + col.Entities.Count.ToString());
            int count = 0;
            foreach (Entity entity in col.Entities)
            {
                tracing.Trace("//////start of loop");
                count++;

                ////////
                tracing.Trace("/////");

                tracing.Trace(entity.GetAttributeValue<EntityReference>("ap360_workorderservicetaskid") != null ? entity.GetAttributeValue<EntityReference>("ap360_workorderservicetaskid").Name : "null");
                tracing.Trace(entity.GetAttributeValue<EntityReference>("ap360_workorderservicetaskid") != null ? entity.GetAttributeValue<EntityReference>("ap360_workorderservicetaskid").Name : "null");
                tracing.Trace(entity.GetAttributeValue<EntityReference>("ap360_workordersubletid") != null ? entity.GetAttributeValue<EntityReference>("ap360_workordersubletid").Name : "null");

                tracing.Trace("/////");

                EntityReference deletingEntityRefInRevisedItem = null;
                if (deletingEntity.LogicalName == "msdyn_workorderservicetask")
                {
                    tracing.Trace("wo service match");
                    tracing.Trace(entity.GetAttributeValue<string>("ap360_name"));
                    tracing.Trace(deletingEntity.LogicalName);
                    deletingEntityRefInRevisedItem = entity.GetAttributeValue<EntityReference>("ap360_workorderservicetaskid") != null ? entity.GetAttributeValue<EntityReference>("ap360_workorderservicetaskid") : null;
                }
                else if (deletingEntity.LogicalName == "msdyn_workorderproduct")
                {
                    tracing.Trace("wo product match");

                    tracing.Trace(entity.GetAttributeValue<string>("ap360_name"));

                    tracing.Trace(deletingEntity.LogicalName);
                    deletingEntityRefInRevisedItem = entity.GetAttributeValue<EntityReference>("ap360_workorderproductid") != null ? entity.GetAttributeValue<EntityReference>("ap360_workorderproductid") : null;
                }
                else if (deletingEntity.LogicalName == "ap360_workordersublet")
                {
                    tracing.Trace("wo sublet match");

                    tracing.Trace(entity.GetAttributeValue<string>("ap360_name"));

                    tracing.Trace(deletingEntity.LogicalName);
                    deletingEntityRefInRevisedItem = entity.GetAttributeValue<EntityReference>("ap360_workordersubletid") != null ? entity.GetAttributeValue<EntityReference>("ap360_workordersubletid") : null;
                }
                if (deletingEntityRefInRevisedItem != null)
                {
                    tracing.Trace("Deleting Entity is " + deletingEntity.LogicalName.ToString());
                    tracing.Trace("Deleting Entity Id " + deletingEntity.Id.ToString());
                    tracing.Trace("Deleting Entity Ref  In REvised Item is " + deletingEntityRefInRevisedItem.LogicalName);
                    tracing.Trace("Del Entity Ref In Revised Item Id " + deletingEntityRefInRevisedItem.Id.ToString());
                    if (deletingEntityRefInRevisedItem.Id.ToString() == deletingEntity.Id.ToString())
                    {
                        reterviedRevisedItem = new RevisedItem();
                        reterviedRevisedItem.guid = entity.Id;

                    }
                }
                tracing.Trace(count.ToString());
                tracing.Trace("//////End of loop");
            }
            return reterviedRevisedItem;

        }

        public static void updateRevisedItem(IOrganizationService service, ITracingService tracingservice, Entity entity, Guid revisedItemGuid)
        {
            bool isRevised = false;
            Microsoft.Xrm.Sdk.Query.ColumnSet columset = null;

            if (entity.LogicalName == "msdyn_workorderservicetask")
            {
                columset = new Microsoft.Xrm.Sdk.Query.ColumnSet("msdyn_description", "ap360_revisedestimatedduration", "ap360_revisedestimatedamount", "ap360_reviseditemstatus", "msdyn_workorder", "ap360_opportunityid", "ap360_isrevised");
                Entity reterviedWOServiceTask = service.Retrieve(entity.LogicalName, entity.Id, columset);
                if (reterviedWOServiceTask != null)
                {
                    isRevised = reterviedWOServiceTask.GetAttributeValue<bool>("ap360_isrevised");
                    if (isRevised)
                    {
                        RevisedItem revisedItem = new RevisedItem();
                        revisedItem.Name = reterviedWOServiceTask.GetAttributeValue<string>("msdyn_description");
                        revisedItem.ExtendedPrice = reterviedWOServiceTask.GetAttributeValue<Money>("ap360_revisedestimatedamount") != null ? reterviedWOServiceTask.GetAttributeValue<Money>("ap360_revisedestimatedamount") : null;
                        revisedItem.WOServiceTask = reterviedWOServiceTask.ToEntityReference();
                        revisedItem.RevisedItemStatus = reterviedWOServiceTask.GetAttributeValue<OptionSetValue>("ap360_reviseditemstatus").Value;
                        Entity updateRevItem = new Entity("ap360_reviseditem");
                        updateRevItem.Id = revisedItemGuid;
                        updateRevItem["ap360_name"] = revisedItem.Name;
                        updateRevItem["ap360_reviseditemstatus"] = new OptionSetValue(revisedItem.RevisedItemStatus);
               // throw new InvalidPluginExecutionException("test");
                        if (revisedItem.ExtendedPrice != null)
                        {
                            updateRevItem["ap360_extendedprice"] = new Money(revisedItem.ExtendedPrice.Value);

                        }
                        service.Update(updateRevItem);
                    }
                }
            }
            else if (entity.LogicalName == "msdyn_workorderproduct")
            {
                columset = new Microsoft.Xrm.Sdk.Query.ColumnSet("ap360_name", "ap360_reviseditemstatus", "msdyn_estimatequantity", "ap360_product", "msdyn_estimateunitamount", "ap360_revisedestimateamount", "msdyn_workorder", "ap360_opportunityid", "ap360_isrevised");
                Entity reterviedWOProduct = service.Retrieve(entity.LogicalName, entity.Id, columset);

                if (reterviedWOProduct != null)
                {
                    isRevised = reterviedWOProduct.GetAttributeValue<bool>("ap360_isrevised");
                    if (isRevised)
                    {
                        // throw new InvalidPluginExecutionException(" insdie ERror");

                        RevisedItem revisedItem = new RevisedItem();
                        revisedItem.Name = reterviedWOProduct.GetAttributeValue<EntityReference>("ap360_product") != null ? reterviedWOProduct.GetAttributeValue<EntityReference>("ap360_product").Name : reterviedWOProduct.GetAttributeValue<string>("ap360_name");
                        revisedItem.Quantity = reterviedWOProduct.GetAttributeValue<double>("msdyn_estimatequantity");
                        revisedItem.UnitPrice = reterviedWOProduct.GetAttributeValue<Money>("msdyn_estimateunitamount") != null ? reterviedWOProduct.GetAttributeValue<Money>("msdyn_estimateunitamount") : null;
                        revisedItem.ExtendedPrice = reterviedWOProduct.GetAttributeValue<Money>("ap360_revisedestimateamount") != null ? reterviedWOProduct.GetAttributeValue<Money>("ap360_revisedestimateamount") : null;
                        revisedItem.WOServiceTask = reterviedWOProduct.ToEntityReference();
                        revisedItem.RevisedItemStatus = reterviedWOProduct.GetAttributeValue<OptionSetValue>("ap360_reviseditemstatus").Value;
                        Entity updateRevItem = new Entity("ap360_reviseditem");
                        updateRevItem.Id = revisedItemGuid;
                        updateRevItem["ap360_name"] = revisedItem.Name;
                        updateRevItem["ap360_quantity"] = Convert.ToDecimal(revisedItem.Quantity);
                        updateRevItem["ap360_reviseditemstatus"] = new OptionSetValue(revisedItem.RevisedItemStatus);

                        if (revisedItem.UnitPrice != null)
                        {
                            updateRevItem["ap360_unitprice"] = new Money(revisedItem.UnitPrice.Value);
                        }
                        if (revisedItem.ExtendedPrice != null)
                        {
                            updateRevItem["ap360_extendedprice"] = new Money(revisedItem.ExtendedPrice.Value);

                        }
                        service.Update(updateRevItem);
                    }

                }
            }
            else if (entity.LogicalName == "ap360_workordersublet")
            {
                columset = new Microsoft.Xrm.Sdk.Query.ColumnSet("ap360_subletdescription", "ap360_reviseditemstatus", "ap360_revisedestimatedamount", "ap360_workorderid", "ap360_opportunityid", "ap360_isrevised");
                Entity reterviedWOSublet = service.Retrieve(entity.LogicalName, entity.Id, columset);


                if (reterviedWOSublet != null)
                {
                    isRevised = reterviedWOSublet.GetAttributeValue<bool>("ap360_isrevised");
                    if (isRevised)
                    {
                        RevisedItem revisedItem = new RevisedItem();
                        revisedItem.Name = reterviedWOSublet.GetAttributeValue<string>("ap360_subletdescription");
                        revisedItem.ExtendedPrice = reterviedWOSublet.GetAttributeValue<Money>("ap360_revisedestimatedamount") != null ? reterviedWOSublet.GetAttributeValue<Money>("ap360_revisedestimatedamount") : null;
                        revisedItem.RevisedItemStatus = reterviedWOSublet.GetAttributeValue<OptionSetValue>("ap360_reviseditemstatus").Value;
                        Entity updateRevItem = new Entity("ap360_reviseditem");
                        updateRevItem.Id = revisedItemGuid;
                        updateRevItem["ap360_name"] = revisedItem.Name;
                        updateRevItem["ap360_reviseditemstatus"] = new OptionSetValue(revisedItem.RevisedItemStatus);

                        if (revisedItem.ExtendedPrice != null)
                        {
                            updateRevItem["ap360_extendedprice"] = new Money(revisedItem.ExtendedPrice.Value);

                        }
                        service.Update(updateRevItem);

                    }
                }

            }











        }


        public static Guid getWOSTRelatedRevisedItemGuid(IOrganizationService service, ITracingService tracingservice, Guid relatedEntityGuid)
        {
            Guid revisedItemGuid = Guid.Empty;
            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='ap360_reviseditem'>
                                <attribute name='ap360_reviseditemid' />
                                <attribute name='ap360_name' />
                                <attribute name='createdon' />
                                <order attribute='ap360_name' descending='false' />
                                <filter type='and'>
                                  <condition attribute='ap360_workorderservicetaskid' operator='eq' value='" + relatedEntityGuid + @"' />
                                </filter>
                              </entity>
                            </fetch>");


            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (col != null && col.Entities.Count > 0)
            {
                revisedItemGuid = col.Entities[0].Id;
            }
            return revisedItemGuid;


        }
        public static Guid getWOProductRelatedRevisedItemGuid(IOrganizationService service, ITracingService tracingservice, Guid relatedEntityGuid)
        {
            Guid revisedItemGuid = Guid.Empty;
            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='ap360_reviseditem'>
                                <attribute name='ap360_reviseditemid' />
                                <attribute name='ap360_name' />
                                <attribute name='createdon' />
                                <order attribute='ap360_name' descending='false' />
                                <filter type='and'>
                                  <condition attribute='ap360_workorderproductid' operator='eq' value='" + relatedEntityGuid + @"' />
                                </filter>
                              </entity>
                            </fetch>");


            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (col != null && col.Entities.Count > 0)
            {
                revisedItemGuid = col.Entities[0].Id;
            }
            return revisedItemGuid;


        }
        public static Guid getWOSubletRelatedRevisedItemGuid(IOrganizationService service, ITracingService tracingservice, Guid relatedEntityGuid)
        {
            Guid revisedItemGuid = Guid.Empty;
            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='ap360_reviseditem'>
                                <attribute name='ap360_reviseditemid' />
                                <attribute name='ap360_name' />
                                <attribute name='createdon' />
                                <order attribute='ap360_name' descending='false' />
                                <filter type='and'>
                                  <condition attribute='ap360_workordersubletid' operator='eq' value='" + relatedEntityGuid + @"' />
                                </filter>
                              </entity>
                            </fetch>");


            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (col != null && col.Entities.Count > 0)
            {
                revisedItemGuid = col.Entities[0].Id;
            }
            return revisedItemGuid;


        }
    }

}
