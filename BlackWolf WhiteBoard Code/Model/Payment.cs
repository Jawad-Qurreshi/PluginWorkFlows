using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk.Query;

namespace BlackWolf_WhiteBoard_Code.Model
{
   public class Payment
    {

        public static decimal getPaymentAmountRelatedtoOpportunity(EntityReference opportunity, IOrganizationService service)
        {
            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='msdyn_payment'>
                                    <attribute name='msdyn_amount' />
                                    <order attribute='msdyn_date' descending='true' />
                                    <filter type='and'>
                                      <condition attribute='msdyn_amount' operator='not-null' />
                                      <condition attribute='ap360_opportunityid'  operator='eq'  value='" + opportunity.Id + @"' /> 
                                    </filter>
                                  </entity>
                                </fetch>");

            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            decimal totalamount = 0.0M;
            foreach (Entity payment in col.Entities)
            {
                Money Paymentamount = payment.GetAttributeValue<Money>("msdyn_amount");
                totalamount = totalamount + Convert.ToDecimal(Paymentamount.Value);
            }
            return totalamount;
        }
        public static void updateWOSubletOnPayment (IOrganizationService service,Entity entity, Guid newlycreatedWOSubIncidentalGuid)
        {
        Entity updatePayment = new Entity(entity.LogicalName, entity.Id);

        updatePayment["ap360_workordersubletid"] = new EntityReference("ap360_workordersublet", newlycreatedWOSubIncidentalGuid);

        service.Update(updatePayment);

        }
}
}
