
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code.Model
{
    public class ResourceRequirement
    {

        public static Guid CreateResourceRequirment(IOrganizationService service, ITracingService tracing, Entity wostEntity)
        {

            Entity resourcerequirement = new Entity("msdyn_resourcerequirement");
            if (wostEntity.GetAttributeValue<EntityReference>("msdyn_workorder")!=null)
            {
                resourcerequirement["name"] = wostEntity.GetAttributeValue<EntityReference>("msdyn_workorder").Name;
                resourcerequirement["msdyn_workorder"] = wostEntity.GetAttributeValue<EntityReference>("msdyn_workorder");
            }
            resourcerequirement["msdyn_workorderservicetask"] = new EntityReference(wostEntity.LogicalName, wostEntity.Id);
            resourcerequirement["statusreason"] = new EntityReference("msdyn_requirmentstatus", new Guid("f1f20cae-4a76-44eb-be6d-db346ba57013"));//Active
            resourcerequirement["duration"] = 30;

            Guid newlyCreateResourceRequirement = service.Create(resourcerequirement);

            return newlyCreateResourceRequirement;

        }
    }
}
