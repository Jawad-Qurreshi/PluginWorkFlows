using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code.Model
{
    public class WOPMainBPF
    {

        public static Entity ReteriveWOPMainBPFRelatedtoWOP(IOrganizationService service, ITracingService tracingService, EntityReference WopRef)
        {
            tracingService.Trace("inside ReteriveWOPMainBPFRelatedtoWOP");
            List<Entity> lstEntities = new List<Entity>();

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='ap360_wopmainbpf'>
                                    <attribute name='businessprocessflowinstanceid' />
                                   
                                    <order attribute='bpf_name' descending='false' />
                                    <filter type='and'>
                                      <condition attribute='bpf_msdyn_workorderproductid' operator='eq' value='" + WopRef.Id + @"' />
                                    </filter>
                                  </entity>
                                </fetch>");


            tracingService.Trace("After fetch");
            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            tracingService.Trace("before deletion");
            //throw new InvalidPluginExecutionException(col.Entities.Count.ToString());
            Entity wopMainBPFEntity = null;
            if (col.Entities.Count > 0)
            {
                wopMainBPFEntity = col.Entities[0];
            }
            tracingService.Trace("After ReteriveWOPMainBPFRelatedtoWOP");
            return wopMainBPFEntity;
        }
        public static void UpdateWOPMainBPFStage(IOrganizationService service, ITracingService tracingService, Entity wopMainBPF,Guid stageGuid)
        {
            Entity updateWOPMainBPF = new Entity(wopMainBPF.LogicalName, wopMainBPF.Id);
            updateWOPMainBPF["activestageid"] = new EntityReference("processstage", stageGuid);//used
            service.Update(updateWOPMainBPF);
        }
    }
}
