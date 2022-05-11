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
    public class WOPMainBPFPlugin : IPlugin
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
                            if (Target.LogicalName.ToLower() == "ap360_wopmainbpf")
                            {
                                tracingService.Trace("Inside WOPMainBPF Create ");
                                EntityReference workOrderProductRef = Target.GetAttributeValue<EntityReference>("bpf_msdyn_workorderproductid") ?? null;
                                if (workOrderProductRef != null)
                                {
                                    Entity reterivedWOPEntity = service.Retrieve(workOrderProductRef.LogicalName, workOrderProductRef.Id, new ColumnSet("ap360_isrevised"));
                                    if (reterivedWOPEntity != null)
                                    {
                                        bool isWOPRevised = reterivedWOPEntity.GetAttributeValue<bool>("ap360_isrevised");

                                        if (!isWOPRevised)
                                        {
                                            Target["activestageid"] = new EntityReference("processstage", new Guid("566d5522-d08a-42c2-91bc-bf4256018e1f"));//PurcahseOrder
                                        }

                                    }
                                }
                            }
                        }
                    }


                    if (context.MessageName.ToLower() == "update")
                    {
                        tracingService.Trace(" ");

                        Guid newSubId = Guid.Empty;
                        Entity Target = (Entity)context.InputParameters["Target"];
                        if (Target != null)
                        {
                            if (Target.LogicalName.ToLower() == "")
                            {

                                if (Target.Contains(""))
                                {

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