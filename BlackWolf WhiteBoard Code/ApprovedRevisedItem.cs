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
    public class ApprovedRevisedItem : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();
            //  throw new InvalidPluginExecutionException("ApprovedRevisedItem");

            try
            {

                ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = factory.CreateOrganizationService(context.UserId);

                string SeletectedRevisedItemsIds = (string)context.InputParameters["revsiedItemId"];


                List<string> seletectedRevisedItemIdsList = SeletectedRevisedItemsIds.Split(',').ToList<string>();

                if (seletectedRevisedItemIdsList.Count > 0)
                {


                    bool isRevisedItemAlreadyApproved = false;
                    foreach (var selectedRevisedItemId in seletectedRevisedItemIdsList)
                    {

                          //  throw new InvalidPluginExecutionException("after "+SeletectedRevisedItemsIds + "  " + seletectedRevisedItemIdsList[0].ToString());
                        Entity reterviedRevisedItemEntity = service.Retrieve("ap360_reviseditem", new Guid(selectedRevisedItemId), new ColumnSet(true));
                        //ap360_name
                        //ap360_quantity
                        //ap360_unitprice
                        //ap360_extendedprice
                        //ap360_opportunityid
                        //ap360_workorderservicetaskid  
                        //ap360_workorderproductid
                        //ap360_workordersubletid

                        //*************
                        //ap360_reviseditemstatus
                        // Need Approval  126,300,000
                        //Approved  126,300,001
                        //Rejected 126,300,002

                        RevisedItem revisedItem = new RevisedItem();
                        if (reterviedRevisedItemEntity != null)
                        {
                            revisedItem.RevisedItemStatus = reterviedRevisedItemEntity.GetAttributeValue<OptionSetValue>("ap360_reviseditemstatus").Value;
                            if (revisedItem.RevisedItemStatus == 126300001)//Approved
                            {
                                isRevisedItemAlreadyApproved = true;
                            }
                            if (!isRevisedItemAlreadyApproved)
                            {

                                Entity updateRevisedItem = new Entity("ap360_reviseditem");
                                updateRevisedItem.Id = new Guid(selectedRevisedItemId);
                                updateRevisedItem["ap360_reviseditemstatus"] = new OptionSetValue(126300001);//Approved
                                service.Update(updateRevisedItem);

                                revisedItem.ItemType = reterviedRevisedItemEntity.GetAttributeValue<OptionSetValue>("ap360_itemtype") != null ? reterviedRevisedItemEntity.GetAttributeValue<OptionSetValue>("ap360_itemtype").Value : 0;
                                //throw new InvalidPluginExecutionException("Temporary Error, Try again later "+revisedItem.ItemType.ToString());

                                if (revisedItem.ItemType == 126300000) //WO Service Task
                                {
                                    revisedItem.WOItem = reterviedRevisedItemEntity.GetAttributeValue<EntityReference>("ap360_workorderservicetaskid") != null ? reterviedRevisedItemEntity.GetAttributeValue<EntityReference>("ap360_workorderservicetaskid") : null;
                                }
                                else if (revisedItem.ItemType == 126300001)//WO Product
                                {
                                    revisedItem.WOItem = reterviedRevisedItemEntity.GetAttributeValue<EntityReference>("ap360_workorderproductid") != null ? reterviedRevisedItemEntity.GetAttributeValue<EntityReference>("ap360_workorderproductid") : null;
                                }
                                else if (revisedItem.ItemType == 126300002)  //WO Sublet 
                                {
                                    revisedItem.WOItem = reterviedRevisedItemEntity.GetAttributeValue<EntityReference>("ap360_workordersubletid") != null ? reterviedRevisedItemEntity.GetAttributeValue<EntityReference>("ap360_workordersubletid") : null;
                                }

                                if (revisedItem.WOItem != null)
                                {
                                    Entity updateWOItem = new Entity(revisedItem.WOItem.LogicalName);
                                    updateWOItem.Id = revisedItem.WOItem.Id;
                                    updateWOItem["ap360_reviseditemstatus"] = new OptionSetValue(126300001);//Approved
                                    service.Update(updateWOItem);
                                }

                            }

                        }

                    }
                    if (isRevisedItemAlreadyApproved)
                    {
                        throw new InvalidPluginExecutionException("One of the Selected Revised Item is already Approved");
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