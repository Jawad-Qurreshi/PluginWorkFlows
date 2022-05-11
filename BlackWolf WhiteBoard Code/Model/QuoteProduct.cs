using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackWolf_WhiteBoard_Code.Model;


namespace BlackWolf_WhiteBoard_Code.Model
{
    public class QuoteProduct
    {

        public Guid guid { get; set; }
        public string Name { get; set; }

        public string PartNo { get; set; }
        public double Quantity { get; set; }
        public Money UnitCost { get; set; }
        public Money Multipliler { get; set; }
        public Money PartsSalePrice { get; set; }
        public Money UnitPrice { get; set; }
        public bool tyreBatteryMarkup { get; set; }

        public string RefInv { get; set; }
        public bool IsCore { get; set; }
        public Money CoreAmount { get; set; }
        public EntityReference Vendor { get; set; }
        public bool isVendorIdentified { get; set; }
        public string Manufacturer { get; set; }
        public int SKU { get; set; }
        public string BarCode { get; set; }
        public bool CustomerSupplied { get; set; }

        public EntityReference ServiceProductMapping { get; set; }
        public EntityReference ParentServiceTask { get; set; }
        public EntityReference GGParent { get; set; }
        public EntityReference GParent { get; set; }
        public EntityReference Parent { get; set; }
        public EntityReference Child { get; set; }
        public EntityReference ProductRef { get; set; }
        public EntityReference ProductFamily { get; set; }
        public string App360Name { get; set; }

        public EntityReference PreferredSupplier { get; set; }
        public EntityReference Warehouse { get; set; }
        public bool ApproveProduct { get; set; }
        public EntityReference UOM { get; set; }
        public EntityReference QST { get; set; }

        public bool Removeproductfamilyhierarchy { get; set; }
        public OptionSetValue QuoteProductType { get; set; }
        //public static void CreateQuoteProductAndAttachToQuote(IOrganizationService service, List<IncidentTypeProduct> lstIncidentTypeProduct, Guid quoteServiceGuid)
        //{
        //    foreach (IncidentTypeProduct incidentTypeProduct in lstIncidentTypeProduct)
        //    {
        //        Entity entity = new Entity("ap360_quoteproduct");
        //        entity["ap360_name"] = "Product " + incidentTypeProduct.Name != null ? incidentTypeProduct.Name : "--";
        //        entity["ap360_quoteserviceid"] = new EntityReference("ap360_quoteservice", quoteServiceGuid);
        //        service.Create(entity);

        //    }

        //}

        public static List<QuoteProduct> GetQuoteProducts(IOrganizationService service, ITracingService tracing, Guid serviceGuid)
        {

            List<QuoteProduct> lstQuoteProducts = new List<QuoteProduct>();

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='ap360_quoteproduct'>
                                <attribute name='ap360_quoteproductid' />
                                <attribute name='ap360_description' />
                                <attribute name='createdon' />
                                <attribute name='ap360_vendorid' />
                                <attribute name='ap360_vendoridentified' />
                                <attribute name='ap360_tyrebatterymarkup' />

                                <attribute name='ap360_quantity' />
                                <attribute name='ap360_unitcost' />
                                <attribute name='ap360_multiplier' />
                                <attribute name='ap360_partsaleprice' />
                                <attribute name='ap360_unitprice' />
                                <attribute name='ap360_quoteservicetaskid' />

                                <attribute name='ap360_refinv' />
                                <attribute name='ap360_partnumber' />
                                <attribute name='ap360_manufacturer' />
                                <attribute name='ap360_preferredsupplierid' />

                                <attribute name='ap360_warehouseid' />
                                <attribute name='ap360_parentservicetaskid' />
                                <attribute name='ap360_ggparent' />
                                <attribute name='ap360_gparent' />
                                <attribute name='ap360_parent' />
                                <attribute name='ap360_child' />
                                <attribute name='ap360_product' />
                                <attribute name='ap360_productfamily' />
                                <attribute name='ap360_name' />
                                <attribute name='ap360_customersupplied' />
                                <attribute name='ap360_approveproduct' />
                                <attribute name='ap360_serviceproductmappingid' />
                                <attribute name='ap360_removeproductfamilyhierarchy' />

                                <attribute name='ap360_quoteproducttype' />




                                <attribute name='ap360_barcode' />
                                <attribute name='ap360_uomid' />
                                <attribute name='ap360_sku' />
                                <attribute name='ap360_core' />
                                <attribute name='ap360_amount' />

                                <order attribute='createdon' descending='false' />
                                    <filter type='and'>
                                      <condition attribute='ap360_quoteserviceid' operator='eq'  value='" + serviceGuid + @"' /> 
                                    </filter>
                                  </entity>
                                </fetch>");


            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            QuoteProduct quoteProduct;
            if (col.Entities.Count > 0)
            {
                tracing.Trace("Quote Product Count " + col.Entities.Count.ToString());
            }
            int count = 0;
            foreach (Entity entity in col.Entities)
            {
                count++;
                quoteProduct = new QuoteProduct();
                quoteProduct.guid = entity.Id;
                quoteProduct.Name = entity.GetAttributeValue<string>("ap360_description");
                quoteProduct.PartNo = entity.GetAttributeValue<string>("ap360_partnumber");
                ////////////////////////////////////////////////////////////////////////////////////////
                tracing.Trace(count.ToString() + " Started");
                tracing.Trace(quoteProduct.Name);

                quoteProduct.Quantity = entity.GetAttributeValue<int>("ap360_quantity");
                quoteProduct.UnitCost = entity.GetAttributeValue<Money>("ap360_unitcost") != null ? entity.GetAttributeValue<Money>("ap360_unitcost") : null;
                quoteProduct.Multipliler = entity.GetAttributeValue<Money>("ap360_multiplier") != null ? entity.GetAttributeValue<Money>("ap360_multiplier") : null;
                quoteProduct.PartsSalePrice = entity.GetAttributeValue<Money>("ap360_partsaleprice") != null ? entity.GetAttributeValue<Money>("ap360_partsaleprice") : null;
                quoteProduct.UnitPrice = entity.GetAttributeValue<Money>("ap360_unitprice") != null ? entity.GetAttributeValue<Money>("ap360_unitprice") : null;
                quoteProduct.tyreBatteryMarkup = entity.GetAttributeValue<bool>("ap360_tyrebatterymarkup");



                if (quoteProduct.PartsSalePrice != null || quoteProduct.UnitPrice != null)
                {
                    tracing.Trace("Quote Product PartSale Price" + quoteProduct.PartsSalePrice.Value.ToString());
                    tracing.Trace("Quote Product Unit Price" + quoteProduct.UnitPrice.Value.ToString());
                }
                //else {
                //    throw new InvalidPluginExecutionException("Quote Product " + quoteProduct.Name +" is not completed");
                //}
                //////////////////////////////////////////////////////////////////////////////////

                quoteProduct.RefInv = entity.GetAttributeValue<string>("ap360_refinv") != null ? entity.GetAttributeValue<string>("ap360_refinv") : null;
                quoteProduct.IsCore = false;
                quoteProduct.IsCore = entity.GetAttributeValue<bool>("ap360_core");
                quoteProduct.CoreAmount = entity.GetAttributeValue<Money>("ap360_amount") != null ? entity.GetAttributeValue<Money>("ap360_amount") : null;

                quoteProduct.CustomerSupplied = entity.GetAttributeValue<bool>("ap360_customersupplied");
                quoteProduct.ApproveProduct = false;
                quoteProduct.ApproveProduct = entity.GetAttributeValue<bool>("ap360_approveproduct");
                quoteProduct.isVendorIdentified = false;
                quoteProduct.isVendorIdentified = entity.GetAttributeValue<bool>("ap360_vendoridentified");

                quoteProduct.QST = entity.GetAttributeValue<EntityReference>("ap360_quoteservicetaskid") != null ? entity.GetAttributeValue<EntityReference>("ap360_quoteservicetaskid") : null;
                //if (quoteProduct.QST != null)
                //    throw new InvalidPluginExecutionException("qst id " + quoteProduct.QST.Id.ToString());
                //else {
                //    throw new InvalidPluginExecutionException("error");
                //}
                quoteProduct.Vendor = entity.GetAttributeValue<EntityReference>("ap360_vendorid") != null ? entity.GetAttributeValue<EntityReference>("ap360_vendorid") : null;
                quoteProduct.Manufacturer = entity.GetAttributeValue<string>("ap360_manufacturer") != null ? entity.GetAttributeValue<string>("ap360_manufacturer") : null;
                quoteProduct.BarCode = entity.GetAttributeValue<string>("ap360_barcode") != null ? entity.GetAttributeValue<string>("ap360_barcode") : null;
                quoteProduct.PreferredSupplier = entity.GetAttributeValue<EntityReference>("ap360_preferredsupplierid") != null ? entity.GetAttributeValue<EntityReference>("ap360_preferredsupplierid") : null;
                quoteProduct.UOM = entity.GetAttributeValue<EntityReference>("ap360_uomid") != null ? entity.GetAttributeValue<EntityReference>("ap360_uomid") : null;
                quoteProduct.SKU = entity.GetAttributeValue<int>("ap360_sku");
                quoteProduct.Warehouse = entity.GetAttributeValue<EntityReference>("ap360_warehouseid") != null ? entity.GetAttributeValue<EntityReference>("ap360_warehouseid") : null;

                quoteProduct.ServiceProductMapping = entity.GetAttributeValue<EntityReference>("ap360_serviceproductmappingid") != null ? entity.GetAttributeValue<EntityReference>("ap360_serviceproductmappingid") : null;
                quoteProduct.ParentServiceTask = entity.GetAttributeValue<EntityReference>("ap360_parentservicetaskid") != null ? entity.GetAttributeValue<EntityReference>("ap360_parentservicetaskid") : null;
                quoteProduct.GGParent = entity.GetAttributeValue<EntityReference>("ap360_ggparent") != null ? entity.GetAttributeValue<EntityReference>("ap360_ggparent") : null;
                quoteProduct.GParent = entity.GetAttributeValue<EntityReference>("ap360_gparent") != null ? entity.GetAttributeValue<EntityReference>("ap360_gparent") : null;
                quoteProduct.Parent = entity.GetAttributeValue<EntityReference>("ap360_parent") != null ? entity.GetAttributeValue<EntityReference>("ap360_parent") : null;
                quoteProduct.Child = entity.GetAttributeValue<EntityReference>("ap360_child") != null ? entity.GetAttributeValue<EntityReference>("ap360_child") : null;
                quoteProduct.ProductRef = entity.GetAttributeValue<EntityReference>("ap360_product") != null ? entity.GetAttributeValue<EntityReference>("ap360_product") : null;
                quoteProduct.ProductFamily = entity.GetAttributeValue<EntityReference>("ap360_productfamily") != null ? entity.GetAttributeValue<EntityReference>("ap360_productfamily") : null;
                quoteProduct.App360Name = entity.GetAttributeValue<string>("ap360_name");
                quoteProduct.Removeproductfamilyhierarchy = entity.GetAttributeValue<bool>("ap360_removeproductfamilyhierarchy");
                quoteProduct.QuoteProductType = entity.GetAttributeValue<OptionSetValue>("ap360_quoteproducttype");

                lstQuoteProducts.Add(quoteProduct);



            }
            tracing.Trace("GetQuoteProducts Function Ended");
            return lstQuoteProducts;

        }


        public static void CreateQuoteProducts(IOrganizationService service, ITracingService tracing, List<IncidentTypeProduct> lstIncidentTypeProducts, Guid quoteServiceGuid)
        {

            foreach (IncidentTypeProduct incidentTypeProduct in lstIncidentTypeProducts)
            {
                Entity newquoteProduct = new Entity("ap360_quoteproduct");
                Guid productGuid = new Guid("7ea07066-e3ed-e911-a811-000d3a579c6d");

                if (incidentTypeProduct.ParentServiceTask != null)
                    newquoteProduct["ap360_parentservicetaskid"] = new EntityReference("msdyn_servicetasktype", incidentTypeProduct.ParentServiceTask.Id);
                if (incidentTypeProduct.GGParent != null)
                    newquoteProduct["ap360_ggparent"] = new EntityReference("product", incidentTypeProduct.GGParent.Id);
                if (incidentTypeProduct.GParent != null)
                    newquoteProduct["ap360_gparent"] = new EntityReference("product", incidentTypeProduct.GParent.Id);
                if (incidentTypeProduct.Parent != null)
                    newquoteProduct["ap360_parent"] = new EntityReference("product", incidentTypeProduct.Parent.Id);
                if (incidentTypeProduct.Child != null)
                    newquoteProduct["ap360_child"] = new EntityReference("product", incidentTypeProduct.Child.Id);
                if (incidentTypeProduct.Product != null)
                    newquoteProduct["ap360_product"] = new EntityReference("product", incidentTypeProduct.Product.Id);
                newquoteProduct["ap360_productdescription"] = incidentTypeProduct.Description;
                newquoteProduct["ap360_approveproduct"] = incidentTypeProduct.ApproveProduct;
                if (incidentTypeProduct.ProductFamily != null)
                    newquoteProduct["ap360_productfamily"] = new EntityReference("product", incidentTypeProduct.ProductFamily.Id);
                newquoteProduct["ap360_name"] = incidentTypeProduct.Name;
                newquoteProduct["ap360_description"] = incidentTypeProduct.Name;


                newquoteProduct["ap360_partnumber"] = incidentTypeProduct.PartNumber;
                newquoteProduct["ap360_quantity"] = 0;
                decimal amount = 0;

                newquoteProduct["ap360_unitcost"] = new Money(amount);
                newquoteProduct["ap360_multiplier"] = new Money(amount);
                newquoteProduct["ap360_partsaleprice"] = new Money(amount);
                newquoteProduct["ap360_unitprice"] = new Money(amount);//Exteneded Price



                //newquoteProduct["ap360_vendoridentified"] = true;
                newquoteProduct["ap360_warehouseid"] = new EntityReference("msdyn_warehouse", new Guid("5b743789-c329-41ee-89e5-f81b83570131"));//Exteneded Price
                //newquoteProduct["ap360_core"] = true;
                newquoteProduct["ap360_quoteserviceid"] = new EntityReference("ap360_quoteservice", quoteServiceGuid);

                //newquoteProduct["ap360_refinv"] = "abc";

                service.Create(newquoteProduct);






            }


        }

        public static void CreateQuoteProductsForReviseQuote(IOrganizationService service, ITracingService tracing, List<QuoteProduct> lstQuoteProducts, Guid newlyCreatedQuoteServiceGuid)
        {
            tracing.Trace("Inside creation of Quote products and count is " + lstQuoteProducts.Count.ToString());

            foreach (QuoteProduct quoteProduct in lstQuoteProducts)
            {
                Entity newquoteProduct = new Entity("ap360_quoteproduct");
                // Guid productGuid = new Guid("7ea07066-e3ed-e911-a811-000d3a579c6d");

                if (quoteProduct.ServiceProductMapping != null)
                {
                    newquoteProduct["ap360_serviceproductmappingid"] = quoteProduct.ServiceProductMapping;
                }
                if (quoteProduct.ParentServiceTask != null)
                    newquoteProduct["ap360_parentservicetaskid"] = new EntityReference("msdyn_servicetasktype", quoteProduct.ParentServiceTask.Id);
                if (quoteProduct.GGParent != null)
                    newquoteProduct["ap360_ggparent"] = new EntityReference("product", quoteProduct.GGParent.Id);
                if (quoteProduct.GParent != null)
                    newquoteProduct["ap360_gparent"] = new EntityReference("product", quoteProduct.GParent.Id);
                if (quoteProduct.Parent != null)
                    newquoteProduct["ap360_parent"] = new EntityReference("product", quoteProduct.Parent.Id);
                if (quoteProduct.Child != null)
                    newquoteProduct["ap360_child"] = new EntityReference("product", quoteProduct.Child.Id);
                if (quoteProduct.ProductRef != null)
                {
                    newquoteProduct["ap360_product"] = new EntityReference("product", quoteProduct.ProductRef.Id);
                }

                // newquoteProduct["ap360_productdescription"] = quoteProduct.Name;
                newquoteProduct["ap360_approveproduct"] = quoteProduct.ApproveProduct;
                if (quoteProduct.ProductFamily != null)
                    newquoteProduct["ap360_productfamily"] = new EntityReference("product", quoteProduct.ProductFamily.Id);
                newquoteProduct["ap360_name"] = quoteProduct.Name;
                newquoteProduct["ap360_description"] = quoteProduct.Name;
                newquoteProduct["ap360_approveproduct"] = quoteProduct.ApproveProduct;
                newquoteProduct["ap360_customersupplied"] = quoteProduct.CustomerSupplied;


                newquoteProduct["ap360_partnumber"] = quoteProduct.PartNo;
                newquoteProduct["ap360_quantity"] = quoteProduct.Quantity;

                if (quoteProduct.UnitCost != null)
                {
                    newquoteProduct["ap360_unitcost"] = quoteProduct.UnitCost;
                }

                if (quoteProduct.Multipliler != null)
                {
                    newquoteProduct["ap360_multiplier"] = quoteProduct.Multipliler;
                }
                if (quoteProduct.PartsSalePrice != null)
                {
                    newquoteProduct["ap360_partsaleprice"] = quoteProduct.PartsSalePrice;
                }
                if (quoteProduct.UnitPrice != null)
                {
                    newquoteProduct["ap360_unitprice"] = quoteProduct.UnitPrice;//Extened Sale price
                }
                // throw new InvalidPluginExecutionException("Updated "+quoteProduct.PartsSalePrice.Value.ToString() + " -- " + quoteProduct.UnitPrice.Value.ToString());
                if (quoteProduct.Vendor != null)
                {
                    newquoteProduct["ap360_vendoridentified"] = quoteProduct.isVendorIdentified;
                    newquoteProduct["ap360_vendorid"] = quoteProduct.Vendor;
                }
                if (quoteProduct.Warehouse != null)
                {
                    newquoteProduct["ap360_warehouseid"] = new EntityReference("msdyn_warehouse", quoteProduct.Warehouse.Id);//Exteneded Price
                }
                if (quoteProduct.CoreAmount != null)
                {
                    newquoteProduct["ap360_core"] = quoteProduct.IsCore;
                    newquoteProduct["ap360_amount"] = quoteProduct.CoreAmount;
                }
                newquoteProduct["ap360_removeproductfamilyhierarchy"] = quoteProduct.Removeproductfamilyhierarchy;
                newquoteProduct["ap360_quoteserviceid"] = new EntityReference("ap360_quoteservice", newlyCreatedQuoteServiceGuid);

                //newquoteProduct["ap360_refinv"] = "abc";

                service.Create(newquoteProduct);






            }


        }

        public static void CreateQPforCore(IOrganizationService service, ITracingService tracingservice, Entity reterivedQP, Entity coreProduct)
        {
            tracingservice.Trace("inside CreateWOPforCore");
            Guid productGuid;

            bool Familyhierarchy = reterivedQP.GetAttributeValue<bool>("ap360_removeproductfamilyhierarchy");
            string description = reterivedQP.GetAttributeValue<string>("ap360_description");
            EntityReference ParentServiceTask = reterivedQP.GetAttributeValue<EntityReference>("ap360_parentservicetaskid");

            EntityReference ProductFamily = new EntityReference("product", new Guid("bbb4903e-75d7-eb11-bacb-000d3a31c760"));//Core Products

            EntityReference ServiceProductMapping = reterivedQP.GetAttributeValue<EntityReference>("ap360_serviceproductmappingid");
            EntityReference GGParent = reterivedQP.GetAttributeValue<EntityReference>("ap360_ggparent");
            EntityReference GParent = reterivedQP.GetAttributeValue<EntityReference>("ap360_gparent");
            EntityReference Parent = reterivedQP.GetAttributeValue<EntityReference>("ap360_parent");
            EntityReference Child = reterivedQP.GetAttributeValue<EntityReference>("ap360_child");
            string Productdescription = reterivedQP.GetAttributeValue<string>("ap360_productdescription");
            string AdditionalNotes = reterivedQP.GetAttributeValue<string>("ap360_additionalnotes");
            EntityReference ProductRef = reterivedQP.GetAttributeValue<EntityReference>("ap360_product");
            tracingservice.Trace("After retrieving fields");
            string partnumber = reterivedQP.GetAttributeValue<string>("ap360_partnumber");
            int Quantity = reterivedQP.GetAttributeValue<int>("ap360_quantity");
            Money UnitCost = reterivedQP.GetAttributeValue<Money>("ap360_unitcost");
            Money CoreAmount = reterivedQP.GetAttributeValue<Money>("ap360_amount");
            bool vendoridentified = reterivedQP.GetAttributeValue<bool>("ap360_vendoridentified");
            bool core = reterivedQP.GetAttributeValue<bool>("ap360_core");
            EntityReference vendor = reterivedQP.GetAttributeValue<EntityReference>("ap360_vendorid");
            bool ApproveProduct = reterivedQP.GetAttributeValue<bool>("ap360_approveproduct");
            EntityReference QuoteService = reterivedQP.GetAttributeValue<EntityReference>("ap360_quoteserviceid");
            bool AddtoProductFamily = reterivedQP.GetAttributeValue<bool>("ap360_addtoproductfamily");
            string Name = reterivedQP.GetAttributeValue<string>("ap360_name");
            EntityReference opportunityRef = reterivedQP.GetAttributeValue<EntityReference>("ap360_opportunityid");



            Money multiplier = reterivedQP.GetAttributeValue<Money>("ap360_multiplier");
            string refin = reterivedQP.GetAttributeValue<string>("ap360_refinv");
            Money partSalePrice = reterivedQP.GetAttributeValue<Money>("ap360_partsaleprice");
            Money unitSalePrice = reterivedQP.GetAttributeValue<Money>("ap360_unitprice");
            EntityReference wareHouse = reterivedQP.GetAttributeValue<EntityReference>("ap360_warehouseid");

            EntityReference UOM = reterivedQP.GetAttributeValue<EntityReference>("ap360_uomid");
            string manufacturer = reterivedQP.GetAttributeValue<string>("ap360_manufacturer");
            string barcode = reterivedQP.GetAttributeValue<string>("ap360_barcode");
            EntityReference prefferedSupplier = reterivedQP.GetAttributeValue<EntityReference>("ap360_preferredsupplierid");
            bool customerSupplied = reterivedQP.GetAttributeValue<bool>("ap360_customersupplied");
            //bool tyrebatterymarkup = reterivedQP.GetAttributeValue<bool>("ap360_tyrebatterymarkup");


            Entity QPForCore = new Entity("ap360_quoteproduct");
            QPForCore["ap360_removeproductfamilyhierarchy"] = Familyhierarchy;
            QPForCore["ap360_description"] = description + "-Core";
            if (ParentServiceTask != null)
                QPForCore["ap360_parentservicetaskid"] = new EntityReference(ParentServiceTask.LogicalName, ParentServiceTask.Id); ;
            if (ProductFamily != null)
                QPForCore["ap360_productfamily"] = ProductFamily;///Core Products
            if (ServiceProductMapping != null)
                QPForCore["ap360_serviceproductmappingid"] = new EntityReference(ServiceProductMapping.LogicalName, ServiceProductMapping.Id); ;
            if (GGParent != null)
                QPForCore["ap360_ggparent"] = new EntityReference(GGParent.LogicalName, GGParent.Id); ;
            if (GParent != null)
                QPForCore["ap360_gparent"] = new EntityReference(GParent.LogicalName, GParent.Id); ;
            if (Parent != null)
                QPForCore["ap360_parent"] = new EntityReference(Parent.LogicalName, Parent.Id); ;
            if (Child != null)
                QPForCore["ap360_child"] = new EntityReference(Child.LogicalName, Child.Id); ;
            if (opportunityRef != null)
                QPForCore["ap360_opportunityid"] = opportunityRef;
            QPForCore["ap360_productdescription"] = Productdescription;
            QPForCore["ap360_additionalnotes"] = AdditionalNotes;
            if (coreProduct == null)
            {

                productGuid = Product.createProductForWorkOrderProduct(service, tracingservice, Name + "-Core", ProductFamily, partnumber + "-Core", unitSalePrice);
                tracingservice.Trace("if createProductForWorkOrderProduct");
                Product.activateProduct(service, tracingservice, productGuid);
                tracingservice.Trace("after active product");
                QPForCore["ap360_name"] = ProductRef.Name + "- Core";

            }
            else// this is because we had issue of Product family as a quote product name,to be on safe end we are using proudct name for Quote product Core name.
            {
                tracingservice.Trace("else ");

                productGuid = coreProduct.Id;
                QPForCore["ap360_name"] = Name + "- Core";

            }
            QPForCore["ap360_product"] = new EntityReference(ProductRef.LogicalName, productGuid);
            QPForCore["ap360_name"] = Name + "- Core";

            QPForCore["ap360_partnumber"] = partnumber + "-Core";
            QPForCore["ap360_core"] = core;
            QPForCore["ap360_quantity"] = Quantity;
            if (UnitCost != null)
                if (CoreAmount != null)
                {
                    QPForCore["ap360_unitcost"] = CoreAmount;
                    QPForCore["ap360_amount"] = CoreAmount;
                    QPForCore["ap360_partsaleprice"] = CoreAmount;
                    QPForCore["ap360_unitprice"] = new Money(CoreAmount.Value * Quantity);
                }
            if (partSalePrice != null)
                if (unitSalePrice != null)

                    QPForCore["ap360_vendoridentified"] = vendoridentified;
            if (vendor != null)
                QPForCore["ap360_vendorid"] = vendor;

            tracingservice.Trace("after ap360_vendorid");

            QPForCore["ap360_approveproduct"] = ApproveProduct;
            if (QuoteService != null)
                QPForCore["ap360_quoteserviceid"] = new EntityReference(QuoteService.LogicalName, QuoteService.Id);
            QPForCore["ap360_addtoproductfamily"] = AddtoProductFamily;
            //  if (multiplier != null)
            //   QPForCore["ap360_multiplier"] = multiplier;
            QPForCore["ap360_refinv"] = refin;
            if (wareHouse != null)
                QPForCore["ap360_warehouseid"] = new EntityReference(wareHouse.LogicalName, wareHouse.Id);
            if (UOM != null)
                QPForCore["ap360_uomid"] = new EntityReference(UOM.LogicalName, UOM.Id);
            QPForCore["ap360_manufacturer"] = manufacturer;
            QPForCore["ap360_barcode"] = barcode;
            if (prefferedSupplier != null)
                QPForCore["ap360_preferredsupplierid"] = new EntityReference(prefferedSupplier.LogicalName, prefferedSupplier.Id);
            QPForCore["ap360_customersupplied"] = customerSupplied;
            // QPForCore["ap360_tyrebatterymarkup"] = tyrebatterymarkup;

            tracingservice.Trace("end before creation");

            service.Create(QPForCore);
        }
        public static EntityCollection RetriveQuoteProductBasedOnProduct(IOrganizationService service, ITracingService tracingService, Guid productGuid)
        {

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                      <entity name='ap360_quoteproduct'>
                                        <attribute name='ap360_quoteproductid' />
                                        <attribute name='ap360_description' />
                                        <attribute name='createdon' />
                                        <order attribute='ap360_description' descending='false' />
                                        <filter type='and'>
                                          <condition attribute='ap360_product' operator='eq' uiname='' uitype='product' value='" + productGuid + @"' />
                                        </filter>
                                      </entity>
                                    </fetch>");



            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (col.Entities.Count < 1)
            {
                tracingService.Trace("Count " + col.Entities.Count.ToString());
                return null;
            }
            return col;
        }
    }

}
