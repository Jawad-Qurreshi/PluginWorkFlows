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
    public class MapActivityToOpportunity : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = factory.CreateOrganizationService(context.UserId);

                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    if (context.MessageName.ToLower() == "create")
                    {

                        Guid newSubId = Guid.Empty;
                        Entity Target = (Entity)context.InputParameters["Target"];
                        if (Target != null)
                        {
                            if (Target.LogicalName.ToLower() == "task")
                            {
                                tracingService.Trace("Inside MapActivityToOpportunity Create ");
                                EntityReference regardingObjectRef = Target.GetAttributeValue<EntityReference>("regardingobjectid") ?? null;
                                if (regardingObjectRef != null)
                                {
                                    string entityFieldToRetrieve = null;
                                    string entityFieldToRetrieveSecond = "ownerid";

                                    if (regardingObjectRef.LogicalName == "msdyn_workorderproduct" ||
                                        regardingObjectRef.LogicalName == "bookableresourcebooking" ||
                                        regardingObjectRef.LogicalName == "ap360_quoteproduct" ||
                                        regardingObjectRef.LogicalName == "ap360_workordersublet")
                                        entityFieldToRetrieve = "ap360_opportunityid";
                                    else if (regardingObjectRef.LogicalName == "msdyn_workorder")
                                        entityFieldToRetrieve = "msdyn_opportunityid";
                                    else if (regardingObjectRef.LogicalName == "quote" ||
                                        regardingObjectRef.LogicalName == "opportunity")
                                        entityFieldToRetrieve = "opportunityid";
                                    else if (regardingObjectRef.LogicalName == "msdyn_workorderservicetask")
                                    {
                                        entityFieldToRetrieve = "ap360_opportunityid";
                                        entityFieldToRetrieveSecond = "msdyn_workorder";
                                    }

                                    if (entityFieldToRetrieve == null) return;
                                    tracingService.Trace(regardingObjectRef.LogicalName);
                                    Entity reterivedEntity = service.Retrieve(regardingObjectRef.LogicalName, regardingObjectRef.Id, new ColumnSet(entityFieldToRetrieve, entityFieldToRetrieveSecond));
                                    if (reterivedEntity != null)
                                    {
                                        tracingService.Trace("retrieved entity is not null");

                                        EntityReference opportuntiyRef = null;
                                        if (regardingObjectRef.LogicalName != "opportunity")
                                        {
                                            opportuntiyRef = reterivedEntity.GetAttributeValue<EntityReference>(entityFieldToRetrieve);
                                        }
                                        else
                                        {

                                            opportuntiyRef = regardingObjectRef;

                                        }



                                        if (opportuntiyRef != null)
                                        {
                                            tracingService.Trace("before oppr ref");
                                            Entity reterivedOpportunity = service.Retrieve(opportuntiyRef.LogicalName, opportuntiyRef.Id, new ColumnSet("ownerid"));
                                            tracingService.Trace("retrieved entity is not null ");
                                            EntityReference opportuntiyOwnerRef = reterivedOpportunity.GetAttributeValue<EntityReference>("ownerid");
                                            //
                                            //  throw new InvalidPluginExecutionException("custom error "+ opportuntiyOwnerRef.LogicalName);

                                            Entity updateTask = new Entity("task", Target.Id);

                                            if (regardingObjectRef.LogicalName == "msdyn_workorderservicetask")
                                            {
                                                tracingService.Trace("Yes it was a wost");
                                                updateTask["ap360_workorderid"] = reterivedEntity.GetAttributeValue<EntityReference>("msdyn_workorder");
                                               // bool ap360_mapdescriptiontowost = Target.GetAttributeValue<bool>("ap360_mapdescriptiontowost");
                                                //if (ap360_mapdescriptiontowost)
                                                //{
                                                //    Entity WOSTEntity = service.Retrieve(regardingObjectRef.LogicalName, regardingObjectRef.Id, new ColumnSet("ap360_workorderservicetask"));
                                                 //   if (WOSTEntity != null)
                                                 //   {
                                                   //     EntityReference WOST = WOSTEntity.GetAttributeValue<EntityReference>("ap360_workorderservicetask") ?? null;
                                                    //    if (WOST != null)
                                                       //{
                                                        //    Entity WOSTEntityToUpdate = new Entity(WOST.LogicalName, WOST.Id);
                                                        //    WOSTEntityToUpdate["ap360_customnotes"] = Target.GetAttributeValue<string>("description");
                                                         //   service.Update(WOSTEntityToUpdate);
                                                       // }
                                                  //  }
                                                //}
                                            }
                                            //throw new InvalidPluginExecutionException("custom error " );
                                            bool bookingmistakeadjustment = Target.GetAttributeValue<bool>("ap360_bookingmistakeadjustment");
                                            if (bookingmistakeadjustment)
                                            {
                                                updateTask["ownerid"] = new EntityReference("systemuser", new Guid("e8f3ec91-f696-407e-8481-98660a72dedf")); //Chris user

                                            }
                                            else
                                            {
                                                updateTask["ownerid"] = opportuntiyOwnerRef;
                                            }
                                            updateTask["ap360_opportunityid"] = opportuntiyRef;
                                            updateTask["ap360_originatingfrom"] = TaskActivity.GetRegardingEntityDisplayName(service, tracingService, regardingObjectRef);
                                            service.Update(updateTask);

                                        }

                                    }







                                }
                            }
                        }
                    }


                    if (context.MessageName.ToLower() == "update")
                    {
                        tracingService.Trace("In Update case ");

                        Guid newSubId = Guid.Empty;
                        Entity Target = (Entity)context.InputParameters["Target"];
                        if (Target != null)
                        {
                            if (Target.LogicalName.ToLower() == "task")
                            {

                                if (Target.Contains("ap360_mapdescriptiontowost"))
                                {
                                    tracingService.Trace("Target contain ap360_mapdescriptiontowost ");

                                    bool ap360_mapdescriptiontowost = Target.GetAttributeValue<bool>("ap360_mapdescriptiontowost");
                                    if (ap360_mapdescriptiontowost)
                                    {
                                        Entity postImage = (Entity)context.PostEntityImages["Image"];
                                        if (postImage.Contains("regardingobjectid") && postImage.Contains("description"))
                                        {
                                            tracingService.Trace("After post images");

                                            EntityReference regardingObjectRef = postImage.GetAttributeValue<EntityReference>("regardingobjectid") ?? null;
                                            if (regardingObjectRef != null)
                                            {

                                                if (regardingObjectRef.LogicalName == "msdyn_workorderservicetask")
                                                {
                                                   
                                                        Entity WOSTEntityToUpdate = new Entity(regardingObjectRef.LogicalName, regardingObjectRef.Id);
                                                        WOSTEntityToUpdate["ap360_customnotes"] = postImage.GetAttributeValue<string>("description");
                                                        service.Update(WOSTEntityToUpdate);
                                                  
                                                }
                                            }
                                        }
                                    }
                                    //tracingService.Trace("ap360_mapdescriptiontowost "+ ap360_mapdescriptiontowost);

                                }
                            }
                        }
                    }
                }
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference)
                {
                    if (context.MessageName.ToLower() == "delete")
                    {

                        tracingService.Trace("");

                        EntityReference TargetRef = (EntityReference)context.InputParameters["Target"];
                        if (TargetRef.LogicalName.ToLower() == "")
                        {
                            // Entity reterivedProductInventory = service.Retrieve(TargetRef.LogicalName, TargetRef.Id, new ColumnSet("msdyn_product", "msdyn_warehouse", "msdyn_unit", "msdyn_qtyonorder", "msdyn_qtyonhand", "msdyn_qtyallocated", "msdyn_qtyavailable"));

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