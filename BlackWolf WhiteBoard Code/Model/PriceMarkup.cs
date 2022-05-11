using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code.Model
{
    public class PriceMarkup
    {
        public Guid guid { get; set; }

        public Money From { get; set; }
        public Money To { get; set; }

        public Money Muliplier { get; set; }

        public static decimal GetPriceMarkUpMultiplier(IOrganizationService service, ITracingService tracingservice, decimal unitcost)
        {

            decimal ap360_multiplier = 0;
            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='ap360_pricemarkup'>
                                    <attribute name='ap360_pricemarkupid' />
                                    <attribute name='ap360_name' />
                                    <attribute name='createdon' />
                                    <attribute name='ap360_to' />
                                    <attribute name='ap360_multiplier' />
                                    <attribute name='ap360_from' />
                                    <order attribute='ap360_name' descending='false' />
                                  </entity>
                                </fetch>");


            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            foreach (Entity entity in col.Entities)
            {
                decimal ap360_to = entity.GetAttributeValue<Money>("ap360_to") != null ? entity.GetAttributeValue<Money>("ap360_to").Value : 0;
                decimal ap360_from = entity.GetAttributeValue<Money>("ap360_from") != null ? entity.GetAttributeValue<Money>("ap360_from").Value : 0;

                if ((unitcost >= ap360_from) && unitcost <= ap360_to)
                {
                    ap360_multiplier = entity.GetAttributeValue<Money>("ap360_multiplier") != null ? entity.GetAttributeValue<Money>("ap360_multiplier").Value : 0;


                }
            }
            return ap360_multiplier;

        }
    }
}
