using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class CreateStandardWorkOrdersOnCreationofOpportunity : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();
            // throw new InvalidPluginExecutionException("CreateStandardWorkOrdersOnCreationofOpportunity");

            try
            {

                //throw new InvalidPluginExecutionException("throw");
                ITracingService tracingService =
                  (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                IPluginExecutionContext context = (IPluginExecutionContext)
                    serviceProvider.GetService(typeof(IPluginExecutionContext));

                IOrganizationServiceFactory factory =
                     (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = factory.CreateOrganizationService(context.UserId);


                Entity entity = null;




                if (context.MessageName.ToLower() == "create")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        entity = (Entity)context.InputParameters["Target"];

                        if (entity.LogicalName == "opportunity")
                        {
                            tracingService.Trace("Inside creation of opportunity to create Standard Workorders");

                            Entity opportunityEntity = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("parentaccountid", "ap360_opportunityautonumber", "ap360_vehicleid", "name", "originatingleadid", "createdon"));
                            if (opportunityEntity != null)
                            {
                                tracingService.Trace("WorkOrderproduct is not null");

                                EntityReference orignatingLeadRef = opportunityEntity.GetAttributeValue<EntityReference>("originatingleadid") != null ? opportunityEntity.GetAttributeValue<EntityReference>("originatingleadid") : null;
                                EntityReference accountRef = opportunityEntity.GetAttributeValue<EntityReference>("parentaccountid") != null ? opportunityEntity.GetAttributeValue<EntityReference>("parentaccountid") : null;
                                EntityReference vehicleRef = opportunityEntity.GetAttributeValue<EntityReference>("ap360_vehicleid") != null ? opportunityEntity.GetAttributeValue<EntityReference>("ap360_vehicleid") : null;
                                decimal opportunityNumber = Convert.ToDecimal(opportunityEntity.GetAttributeValue<string>("ap360_opportunityautonumber"));
                                string opportunityTopic = opportunityEntity.GetAttributeValue<string>("name");
                                DateTime opprCreatedOn= opportunityEntity.GetAttributeValue<DateTime>("createdon");


                                Guid protocolCreatedWorkOrderGuid = Guid.Empty;
                                Guid adminCreatedWorkOrderGuid = Guid.Empty;
                                Guid newlyCreatedWorkOrderGuid3 = Guid.Empty;

                                string checkinVehicle = "Check In Vehicle";
                                string CheckOut = "Check Out";
                                string GRFPU = "GRFPU";

                                string buildEstimate = "Build Estimate";
                                string PartsReseach = "Parts Research";
                                string QualityCheck = "Quality Check";

                                List<string> protocolWOSTs = new List<string>();
                                protocolWOSTs.Add(checkinVehicle);
                                protocolWOSTs.Add(GRFPU);
                                protocolWOSTs.Add(CheckOut);

                                List<string> adminWOSTs = new List<string>();
                                adminWOSTs.Add(buildEstimate);
                                adminWOSTs.Add(PartsReseach);
                                adminWOSTs.Add(QualityCheck);

                                if (accountRef != null)
                                {
                                    tracingService.Trace("Account Ref is not null and Opportuntiy nubmer is " + opportunityNumber.ToString());
                                    if (vehicleRef != null)
                                    {
                                        
                                    }
                                    else if (vehicleRef == null)
                                    {
                                        string vehicleName = null;
                                        Guid VehicleGuid = Vehicle.UpdateVehicleAccount(service, tracingService, orignatingLeadRef,accountRef,ref vehicleName);
                                        Entity updateOpportunity = new Entity(entity.LogicalName, entity.Id);
                                    
                                        updateOpportunity["ap360_vehicleid"] = new EntityReference("ap360_vehicle", VehicleGuid);
                                        updateOpportunity["name"] = vehicleName +" "+ accountRef.Name +" "+ opprCreatedOn.Date.ToString("MM/dd/yyyy");
                                        vehicleRef = new EntityReference("ap360_vehicle", VehicleGuid); 
                                        service.Update(updateOpportunity);

                                    }
                                    
                                    protocolCreatedWorkOrderGuid = WorkOrder.CreateStandardWorkOrderOnCreationOfOpportunity(service, tracingService, accountRef.Id, opportunityEntity.Id, checkinVehicle,vehicleRef,opportunityNumber, "protocol");
                                    tracingService.Trace("standard workorder created");

                                    if (protocolCreatedWorkOrderGuid != Guid.Empty)
                                    {
                                        foreach (string wost in protocolWOSTs)
                                        {
                                            tracingService.Trace("Protocol WOSTs");
                                            WorkOrderServiceTask.CreateStandardWorkOrderServiceTasksOnCreationofOpportunity(service, tracingService, protocolCreatedWorkOrderGuid, wost, opportunityEntity.Id, vehicleRef, opportunityNumber, "protocol");


                                        }
                                    }

                                    adminCreatedWorkOrderGuid = WorkOrder.CreateStandardWorkOrderOnCreationOfOpportunity(service, tracingService, accountRef.Id, opportunityEntity.Id, checkinVehicle, vehicleRef, opportunityNumber,"admin");
                                    if (adminCreatedWorkOrderGuid != Guid.Empty)
                                    {
                                        foreach (string wost in adminWOSTs)
                                        {
                                            tracingService.Trace("Admin WOSTs");
                                            WorkOrderServiceTask.CreateStandardWorkOrderServiceTasksOnCreationofOpportunity(service, tracingService, adminCreatedWorkOrderGuid, wost, opportunityEntity.Id, vehicleRef, opportunityNumber, "admin");


                                        }
                                    }
                                    tracingService.Trace("before Create Project");
                                    Project.CreateProject(service, tracingService, accountRef, opportunityTopic, opportunityEntity.Id, "quote");
                                    tracingService.Trace("after Create Project");
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