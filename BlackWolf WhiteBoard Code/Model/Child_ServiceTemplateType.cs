using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code.Model
{
    public class Child_ServiceTemplateType
    {
        public EntityReference Serviceproductmapping { get; set; }

        public EntityReference GGparentproduct { get; set; }
        public EntityReference Gparentproduct { get; set; }

        public Guid guid { get; set; }
        public static void getChildServiceTemplateAndMapProductsOnQuoteSerivice(IOrganizationService service, ITracingService tracing, Entity updateQuoteService, EntityReference childServiceTemplateRef)
        {

            Child_ServiceTemplateType child_ServiceTemplateType = null;
            Entity reterivedChildServiceTemplate = service.Retrieve(childServiceTemplateRef.LogicalName, childServiceTemplateRef.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("ap360_ggparentproductid", "ap360_gparentproductid", "ap360_serviceproductmappingid"));
            if (reterivedChildServiceTemplate != null)
            {
                child_ServiceTemplateType = new Child_ServiceTemplateType();
                child_ServiceTemplateType.guid = reterivedChildServiceTemplate.Id;
                child_ServiceTemplateType.Serviceproductmapping = reterivedChildServiceTemplate.GetAttributeValue<EntityReference>("ap360_serviceproductmappingid") != null ? reterivedChildServiceTemplate.GetAttributeValue<EntityReference>("ap360_serviceproductmappingid") : null;
                child_ServiceTemplateType.GGparentproduct = reterivedChildServiceTemplate.GetAttributeValue<EntityReference>("ap360_ggparentproductid") != null ? reterivedChildServiceTemplate.GetAttributeValue<EntityReference>("ap360_ggparentproductid") : null;
                child_ServiceTemplateType.Gparentproduct = reterivedChildServiceTemplate.GetAttributeValue<EntityReference>("ap360_gparentproductid") != null ? reterivedChildServiceTemplate.GetAttributeValue<EntityReference>("ap360_gparentproductid") : null;


                if (child_ServiceTemplateType.Serviceproductmapping != null)
                {
                    updateQuoteService["ap360_serviceproductmappingid"] = new EntityReference(child_ServiceTemplateType.Serviceproductmapping.LogicalName, child_ServiceTemplateType.Serviceproductmapping.Id);
                }
                else
                {
                    updateQuoteService["ap360_serviceproductmappingid"] = null;

                }
                if (child_ServiceTemplateType.GGparentproduct != null)
                {
                    updateQuoteService["ap360_ggparentproductid"] = new EntityReference(child_ServiceTemplateType.GGparentproduct.LogicalName, child_ServiceTemplateType.GGparentproduct.Id);
                }
                else
                {
                    updateQuoteService["ap360_ggparentproductid"] = null;

                }
                if (child_ServiceTemplateType.Gparentproduct != null)
                {
                    updateQuoteService["ap360_gparentproductid"] = new EntityReference(child_ServiceTemplateType.Gparentproduct.LogicalName, child_ServiceTemplateType.Gparentproduct.Id);
                }
                else
                {
                    updateQuoteService["ap360_gparentproductid"] = null;

                }
            }

        }
    }
}
