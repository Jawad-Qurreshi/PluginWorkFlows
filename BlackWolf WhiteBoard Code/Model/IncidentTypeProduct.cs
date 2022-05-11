using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code.Model
{
    public class IncidentTypeProduct
    {
        public Guid guid { get; set; }
        public EntityReference IncidentType { get; set; }

        public EntityReference Owner { get; set; }
        public String Description { get; set; }
        //public String InternalDescription { get; set; }
        public int? LineOrder { get; set; }
        public EntityReference ParentServiceTask{ get; set; }

        public EntityReference GGParent { get; set; }
        public EntityReference GParent { get; set; }
        public EntityReference Parent { get; set; }
        public EntityReference Child { get; set; }
        public EntityReference Product { get; set; }
        public string ProductDescription { get; set; }
        public bool ApproveProduct { get; set; }
        public EntityReference ProductFamily { get; set; }
        public string Name { get; set; }
        public string PartNumber { get; set; }
        public EntityReference Unit { get; set; }
        public decimal Quantity { get; set; }





        public static List<IncidentTypeProduct> GetIncidentTypeProducts(IOrganizationService service,ITracingService tracing, Guid serviceGuid)
        {

            List<IncidentTypeProduct> lstIncidentTypeProducts = new List<IncidentTypeProduct>();

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='msdyn_incidenttypeproduct'>
                                    <attribute name='msdyn_name' />
                                    <attribute name='msdyn_product' />
                                    <attribute name='msdyn_unit' />
                                    <attribute name='msdyn_incidenttype' />
                                    <attribute name='msdyn_quantity' />
                                    <attribute name='ownerid' />
                                    <attribute name='msdyn_description' />
                                    <attribute name='msdyn_internaldescription' />
                                    <attribute name='msdyn_lineorder' />

                                    <attribute name='ap360_parentservicetaskid' />
                                    <attribute name='ap360_ggparent' />
                                    <attribute name='ap360_gparent' />
                                    <attribute name='ap360_parent' />
                                    <attribute name='ap360_child' />
                                    <attribute name='ap360_product' />
                                    <attribute name='ap360_productdescription' />
                                    <attribute name='ap360_approveproduct' />
                                    <attribute name='ap360_productfamily' />
                                    <attribute name='ap360_name' />
                                    <attribute name='ap360_partnumber' />
                                    <attribute name='msdyn_unit' />
                                    <attribute name='msdyn_quantity' />
                                   




                                    <filter type='and'>
                                      <condition attribute='ap360_incidenttypeserviceid' operator='eq'  value='" + serviceGuid + @"' /> 
                                    </filter>
                                  </entity>
                                </fetch>");

            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            IncidentTypeProduct incidentTypeProduct;
            foreach (Entity entity in col.Entities)
            {
                incidentTypeProduct = new IncidentTypeProduct();
                incidentTypeProduct.guid = entity.Id;

                incidentTypeProduct.IncidentType = entity.GetAttributeValue<EntityReference>("msdyn_incidenttype") != null ? entity.GetAttributeValue<EntityReference>("msdyn_incidenttype") : null;

                // Quantity is missing

                incidentTypeProduct.ParentServiceTask = entity.GetAttributeValue<EntityReference>("ap360_parentservicetaskid") != null ? entity.GetAttributeValue<EntityReference>("ap360_parentservicetaskid") : null;
                incidentTypeProduct.GGParent = entity.GetAttributeValue<EntityReference>("ap360_ggparent") != null ? entity.GetAttributeValue<EntityReference>("ap360_ggparent") : null;
                incidentTypeProduct.GParent = entity.GetAttributeValue<EntityReference>("ap360_gparent") != null ? entity.GetAttributeValue<EntityReference>("ap360_gparent") : null;
                incidentTypeProduct.Parent = entity.GetAttributeValue<EntityReference>("ap360_parent") != null ? entity.GetAttributeValue<EntityReference>("ap360_parent") : null;
                incidentTypeProduct.Child = entity.GetAttributeValue<EntityReference>("ap360_child") != null ? entity.GetAttributeValue<EntityReference>("ap360_child") : null;
                incidentTypeProduct.Product = entity.GetAttributeValue<EntityReference>("ap360_product") != null ? entity.GetAttributeValue<EntityReference>("msdyn_product") : null;
                incidentTypeProduct.ProductDescription = entity.GetAttributeValue<string>("ap360_productdescription");

                incidentTypeProduct.ApproveProduct = entity.GetAttributeValue<bool>("ap360_approveproduct");
                incidentTypeProduct.ProductFamily = entity.GetAttributeValue<EntityReference>("ap360_productfamily") != null ? entity.GetAttributeValue<EntityReference>("ap360_productfamily") : null;
                incidentTypeProduct.Name = entity.GetAttributeValue<string>("ap360_name") != null ? entity.GetAttributeValue<string>("ap360_name") : null;
                incidentTypeProduct.PartNumber = entity.GetAttributeValue<string>("ap360_partnumber");
                incidentTypeProduct.Unit = entity.GetAttributeValue<EntityReference>("msdyn_unit") != null ? entity.GetAttributeValue<EntityReference>("msdyn_unit") : null;
                

               // incidentTypeProduct.Quantity = entity.GetAttributeValue<decimal>("msdyn_quantity");
                

                incidentTypeProduct.Owner = entity.GetAttributeValue<EntityReference>("ownerid") != null ? entity.GetAttributeValue<EntityReference>("ownerid") : null;
                incidentTypeProduct.Description = entity.GetAttributeValue<string>("msdyn_description") != null ? entity.GetAttributeValue<string>("msdyn_description") : null;
                incidentTypeProduct.LineOrder = entity.GetAttributeValue<int?>("msdyn_lineorder") != null ? entity.GetAttributeValue<int>("msdyn_lineorder") : 0;

                lstIncidentTypeProducts.Add(incidentTypeProduct);

            }
            return lstIncidentTypeProducts;

        }


        public static void AttachIncidentTypeProductToWorkOrder(IOrganizationService service, List<IncidentTypeProduct> lstIncidentTypeProduct, Guid WorKOrderGuid)
        {
            foreach (IncidentTypeProduct incidentTypeProduct in lstIncidentTypeProduct)
            {

                //Entity entity1 = new Entity("msdyn_workorderproduct");
                //entity1["msdyn_product"] = new EntityReference("product", new Guid("EA74BA0D-0521-EA11-A815-000D3A468E3E"));
                //entity1["msdyn_unit"] = new EntityReference("uom", new Guid("1C94F1C7-0321-EA11-A814-000D3A468D62"));//UNIT
                //entity1["msdyn_workorder"] = new EntityReference("msdyn_workorder", WorKOrderGuid);
                //service.Create(entity1);


                //Entity entity = new Entity("msdyn_workorderservicetask");
                //entity["msdyn_workorder"] = new EntityReference("msdyn_workorder", WorKOrderGuid);
                //service.Create(entity);

                Entity entity2 = new Entity("msdyn_workorderservice");
                entity2["msdyn_service"] = new EntityReference("product", new Guid("1b2d9a60-e9c2-e411-80e5-c4346bad2660"));
                entity2["msdyn_unit"] = new EntityReference("uom", new Guid("1C94F1C7-0321-EA11-A814-000D3A468D62"));//UNIT
                entity2["msdyn_workorder"] = new EntityReference("msdyn_workorder", WorKOrderGuid);
                service.Create(entity2);

            }

        }

    }
}
