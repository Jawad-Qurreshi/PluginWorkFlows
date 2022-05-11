using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class UpdateRegardingOnCompleteorCloseOfActivity : IPlugin
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

                        if (entity.LogicalName == "task")
                        {
                            tracingService.Trace("Entity Name " + entity.LogicalName);

                            //Open 0
                            //completed 1
                            //Canceled 2 
                            if (entity.Contains("statecode"))
                            {
                                int activityStatus = entity.GetAttributeValue<OptionSetValue>("statecode").Value;
                                if (activityStatus == 1 || activityStatus == 2)
                                {

                                    Entity reterviedTask = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("regardingobjectid"));

                                    if (reterviedTask != null)
                                    {

                                        EntityReference regardingObjectRef = reterviedTask.GetAttributeValue<EntityReference>("regardingobjectid") != null ? reterviedTask.GetAttributeValue<EntityReference>("regardingobjectid") : null;
                                        if (regardingObjectRef != null)
                                        {
                                            if (regardingObjectRef.LogicalName == "msdyn_workorderproduct")
                                            {

                                                Entity updateWOProduct = new Entity(regardingObjectRef.LogicalName, regardingObjectRef.Id);

                                                if (activityStatus == 1)
                                                    updateWOProduct["ap360_paymentstatus"] = new OptionSetValue(126300002);//Approved By Manager
                                                if(activityStatus ==2)
                                                    updateWOProduct["ap360_paymentstatus"] = new OptionSetValue(126300003);//Rejected By Manager


                                                service.Update(updateWOProduct);
                                            }
                                        }
                                    }

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