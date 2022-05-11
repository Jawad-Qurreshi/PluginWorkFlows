
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code.Model
{
    public      class BookableResource
    {
        public static Entity retreiveResoruce(IOrganizationService service, Guid userGuid)
        {
            Entity entity = null;

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='bookableresource'>
                                <attribute name='name' />
                                <attribute name='createdon' />
                                <attribute name='resourcetype' />
                                <attribute name='bookableresourceid' />
                                <order attribute='name' descending='false' />
                                <filter type='and'>
                                  <condition attribute='userid' operator='eq'  value='" + userGuid + @"' /> 
                                </filter>
                              </entity>
                            </fetch>");

            //link - type = 'outer'
            //< attribute name = '' />
            List<WorkOrder> lstWorkOrders = new List<WorkOrder>();
            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));

            if (col.Entities.Count > 0)
            {
                entity = col.Entities[0];

            }
            return entity;
        }

    }
}
