using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BlackWolf_WhiteBoard_Code
{
    public class MapWorkOrderBWStatusToProjectTask : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();
            //  throw new InvalidPluginExecutionException("MapBookingSrvTaskCompleteTOWoSrvTask");
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
                        if (entity.LogicalName == "msdyn_workorder")
                        {
                            if (entity.Contains("ap360_wobwstatus"))
                            {
                                tracingService.Trace("Work Order WoBWstatus updated");

                                //This plugin is disabled because we have new MultiOptionset field on WorkOrder
                                //Also Workorder status information on project task is not helpfull instead report may
                                //be a better place to dispaly this information.

                                OptionSetValue wobwStatus = entity.GetAttributeValue<OptionSetValue>("ap360_wobwstatus");
                                if (wobwStatus != null)
                                {
                                    ProjectTask projectTaskRelatedToWO = ProjectTask.GetProjectTaskRelatedToWorkOrder(service, tracingService, entity.Id);

                                    if (projectTaskRelatedToWO != null)
                                    {

                                        tracingService.Trace("Project TAsk is not null");
                                        // throw new InvalidPluginExecutionException("Custom Error");
                                        //string statusName = WorkOrder.getWorkOrderBWStatusName(wobwStatus.Value);
                                        //Entity updateWOSTProjectTask = new Entity("msdyn_projecttask", projectTaskRelatedToWO.guid);
                                        //if (statusName != null && statusName != "")
                                        //{
                                        //    updateWOSTProjectTask["msdyn_description"] = statusName;
                                        //    service.Update(updateWOSTProjectTask);
                                        //}
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