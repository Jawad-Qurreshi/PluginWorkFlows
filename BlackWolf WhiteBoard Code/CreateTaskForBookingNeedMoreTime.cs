using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class CreateTaskForBookingNeedMoreTime : IPlugin
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

                        if (entity.LogicalName == "bookableresourcebooking")
                        {

                            if (entity.Contains("ap360_needmoretimereason"))
                            {
                                Entity postImage = (Entity)context.PostEntityImages["Image"];

                                tracingService.Trace("After Image");
                                int ExtraTimeRequired = postImage.GetAttributeValue<int>("ap360_extratimerequired");
                                OptionSetValue NeedMoreTimeReason = postImage.GetAttributeValue<OptionSetValue>("ap360_needmoretimereason") != null ? postImage.GetAttributeValue<OptionSetValue>("ap360_needmoretimereason") : null;
                                OptionSetValue NeedMoreTimeSubReason = postImage.GetAttributeValue<OptionSetValue>("ap360_needmoretimesubreason") != null ? postImage.GetAttributeValue<OptionSetValue>("ap360_needmoretimesubreason") : null;
                                DateTime TimeStampNeedMoreTime = postImage.GetAttributeValue<DateTime>("ap360_timestampneedmoretime");

                                //int extratimerequiredreasons = postImage.GetAttributeValue<OptionSetValue>("ap360_extratimerequiredreasons").Value;
                                //string extraTimeRequiredReasonString = BookableResourceBooking.getExtraTimeRequiredReasons(extratimerequiredreasons);
                                //string ap360_extratimerequireddescription = postImage.GetAttributeValue<string>("ap360_extratimerequireddescription");
                                string name = postImage.GetAttributeValue<string>("name");
                                EntityReference WorkOrder = postImage.GetAttributeValue<EntityReference>("msdyn_workorder") != null ? postImage.GetAttributeValue<EntityReference>("msdyn_workorder") : null;
                                EntityReference opportunityRef = postImage.GetAttributeValue<EntityReference>("ap360_opportunityid") != null ? postImage.GetAttributeValue<EntityReference>("ap360_opportunityid") : null;
                                EntityReference opportunityOwner = null;
                                if (opportunityRef != null)
                                {

                                    Entity reterivedOpportunity = service.Retrieve(opportunityRef.LogicalName, opportunityRef.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("ownerid"));
                                    if (reterivedOpportunity != null)
                                    {
                                        opportunityOwner = reterivedOpportunity.GetAttributeValue<EntityReference>("ownerid");
                                    }
                                }

                                EntityReference resource = postImage.GetAttributeValue<EntityReference>("resource") != null ? postImage.GetAttributeValue<EntityReference>("resource") : null;

                                tracingService.Trace("Before Creating Task");

                                Entity createTask = new Entity("task");
                                createTask["subject"] = name + "(" + WorkOrder.Name + ")" + " need more time " + ExtraTimeRequired + " minutes";
                                createTask["description"] = name + "(" + WorkOrder.Name + ")" + " need more time " + ExtraTimeRequired + " minutes reported by " + resource.Name + ". \n " + WorkOrderServiceTask.getupdatedWOSTStatusName(NeedMoreTimeReason.Value) + " : " + WorkOrderServiceTask.getupdatedWOSTSubStatusName(NeedMoreTimeSubReason.Value);
                                createTask["regardingobjectid"] = new EntityReference(entity.LogicalName, entity.Id);
                                createTask["prioritycode"] = new OptionSetValue(1);
                                createTask["ownerid"] = opportunityOwner;
                                tracingService.Trace("Custom error");

                                tracingService.Trace(TimeStampNeedMoreTime.ToString());

                                createTask["ap360_needmoretimeextratimerequired"] = ExtraTimeRequired;
                                if (NeedMoreTimeReason != null)
                                {
                                    createTask["ap360_needmoretimereason"] = NeedMoreTimeReason;
                                }
                                if (NeedMoreTimeSubReason != null)
                                {   

                                    createTask["ap360_needmoretimesubreason"] = NeedMoreTimeSubReason;
                                }
                                createTask["ap360_needmoretimestamp"] = TimeStampNeedMoreTime;


                                //Low 0
                                //Normal 1
                                //Hight 2
                                createTask["scheduledend"] = DateTime.Now; //Due Date
                                createTask["scheduledstart"] = DateTime.Now;//Start Date

                                service.Create(createTask);

                                tracingService.Trace("Plugin Ended Successfully");

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