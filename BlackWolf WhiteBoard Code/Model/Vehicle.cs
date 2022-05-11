using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code.Model
{
    public class Vehicle
    {

        public static Guid UpdateVehicleAccount(IOrganizationService service, ITracingService tracing, EntityReference orignatingLeadRef, EntityReference accountRef, ref string vehicleName)
        {
            tracing.Trace("inside CreateVehicle");
            Guid vehicleId = Guid.Empty;
            Entity reterviedLead = service.Retrieve(orignatingLeadRef.LogicalName, orignatingLeadRef.Id, new ColumnSet("ap360_vehicleid", "parentcontactid"));
            if (reterviedLead != null)
            {
                tracing.Trace("reterviedLead is not null ");
               
                EntityReference vehicleRef = reterviedLead.GetAttributeValue<EntityReference>("ap360_vehicleid") ?? null;
                EntityReference ContactRef = reterviedLead.GetAttributeValue<EntityReference>("parentcontactid") ?? null;
                
                Entity reterviedVehicle = service.Retrieve(vehicleRef.LogicalName, vehicleRef.Id, new ColumnSet("ap360_accountid"));
                Entity reterviedContact = service.Retrieve(ContactRef.LogicalName, ContactRef.Id, new ColumnSet("parentcustomerid"));

                EntityReference AccountRefVehicle = reterviedVehicle.GetAttributeValue<EntityReference>("ap360_accountid") ?? null;
                EntityReference AccountRefContact = reterviedContact.GetAttributeValue<EntityReference>("parentcustomerid") ?? null;

                vehicleId = vehicleRef.Id;
                vehicleName = vehicleRef.Name;
                
                if (AccountRefVehicle == null)
                {
                    Entity updateVehicle = new Entity("ap360_vehicle");
                    updateVehicle.Id = vehicleRef.Id;
                    updateVehicle["ap360_accountid"] = accountRef;
                    service.Update(updateVehicle);
                }

                if (AccountRefContact == null)
                {
                    Entity updateContact = new Entity("contact");
                    updateContact.Id = ContactRef.Id;
                    updateContact["parentcustomerid"] = accountRef;
                    service.Update(updateContact);
                }


            }
            return vehicleId;

        }

    }
}
