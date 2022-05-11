using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code.Model
{
    public class Product
    {

        public static Guid createProduct(IOrganizationService service, ITracingService tracing, QuoteProduct quoteProduct)
        {
            DateTime currentDatetime = DateTime.Now;
            string formatedDate = currentDatetime.ToString("Hmm");
            if (formatedDate.StartsWith("0"))
            {
                formatedDate = formatedDate.TrimStart('0');
            }
            Random random = new Random();
            int randomNumber = random.Next();
            int time = Int32.Parse(formatedDate);


            Entity product = new Entity("product");
            product["name"] = quoteProduct.Name;
            product["productnumber"] = "BW-PROD-" + randomNumber.ToString() + "-" + time.ToString();
            product["defaultuomscheduleid"] = new EntityReference("uomschedule", new Guid("aa8431af-d3f2-431f-acca-9555ec1005ba"));
            product["defaultuomid"] = new EntityReference("uom", new Guid("361a3eac-749c-4bb3-92a2-d63f692f61ba"));
            product["quantitydecimal"] = 1;


            //product["statecode"] = new OptionSetValue(0);//Status : Active
            //product["statuscode"] = new OptionSetValue(1);//Status Reason : Active

            product["msdyn_fieldserviceproducttype"] = new OptionSetValue(690970000);

            Guid guid = service.Create(product);
            SetStateRequest setStateRequest = new SetStateRequest()
            {
                EntityMoniker = new EntityReference
                {
                    Id = guid,
                    LogicalName = "product",
                },
                State = new OptionSetValue(0),//Status : Active
                Status = new OptionSetValue(1)//Status Reason : Active
            };
            service.Execute(setStateRequest);

            tracing.Trace("Product Ceatead : " + guid.ToString());
            return guid;
        }

        public static Guid createProductFromDescription(IOrganizationService service, ITracingService tracing, string description, string partNumber, EntityReference productFamily, Money productCost)
        {
            tracing.Trace("inside createProductFromDescription");
            int isProductExists = 0;
            isProductExists = CheckProductByNameAndPartNumber(service, tracing, description, partNumber);
            if (isProductExists > 0)
            {
                throw new InvalidPluginExecutionException(description + " already exists with Part Number " + partNumber);
            }

            Guid guid = createProductForWorkOrderProduct(service, tracing, description, productFamily, partNumber, productCost);
            activateProduct(service, tracing, guid);

            tracing.Trace("Product Ceatead : " + guid.ToString());
            return guid;
        }

        public static void activateProduct(IOrganizationService service, ITracingService tracing, Guid guid)
        {
            tracing.Trace("Inside activateProduct");
            SetStateRequest setStateRequest = new SetStateRequest()
            {
                EntityMoniker = new EntityReference
                {
                    Id = guid,
                    LogicalName = "product",
                },
                State = new OptionSetValue(0),//Status : Active
                Status = new OptionSetValue(1)//Status Reason : Active
            };
            service.Execute(setStateRequest);
        }
        public static Guid createProductForWorkOrderProduct(IOrganizationService service, ITracingService tracing, string description, EntityReference productFamily, string partNumber, Money productCost)
        {
            // throw new InvalidPluginExecutionException("updated "+ description + "  -- "+ partNumber);
            tracing.Trace("inside createProductForWorkOrderProduct");
            DateTime currentDatetime = DateTime.Now;
            string formatedDate = currentDatetime.ToString("Hmm");
            if (formatedDate.StartsWith("0"))
            {
                formatedDate = formatedDate.TrimStart('0');
            }
            Random random = new Random();
            int randomNumber = random.Next();
            int time = Int32.Parse(formatedDate);

            Entity product = new Entity("product");
            product["name"] = description;
            if (productFamily == null)
            {
                throw new InvalidPluginExecutionException("Product family is not selected in WOP ");
            }

            product["ap360_productfamily"] = new EntityReference("product", productFamily.Id);

            product["parentproductid"] = new EntityReference("product", productFamily.Id);
            product["ap360_partnumber"] = partNumber;
            if (productCost != null)
            {
                product["currentcost"] = productCost;
            }
            product["productnumber"] = "BW-PROD-" + randomNumber.ToString() + "-" + time.ToString();
            product["defaultuomscheduleid"] = new EntityReference("uomschedule", new Guid("aa8431af-d3f2-431f-acca-9555ec1005ba"));
            product["defaultuomid"] = new EntityReference("uom", new Guid("361a3eac-749c-4bb3-92a2-d63f692f61ba"));
            product["quantitydecimal"] = 1;


            //product["statecode"] = new OptionSetValue(0);//Status : Active
            //product["statuscode"] = new OptionSetValue(1);//Status Reason : Active

            product["msdyn_fieldserviceproducttype"] = new OptionSetValue(690970000);//Inventory

            Guid guid = Guid.Empty;
            guid = service.Create(product);
            return guid;
        }
        public static int CheckProductFamilyByName(IOrganizationService service, ITracingService tracing, string productName)
        {

            List<QuoteServiceTask> lstquoteservicetasks = new List<QuoteServiceTask>();

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >
                                  <entity name='product' >
                                    <attribute name='ap360_name' />
                                    <attribute name='name' />
                                    <filter type='and'>
                                      <condition attribute='name' operator= 'like' value = '%" + productName + @"%' />
                                      <condition attribute='productstructure' operator='eq' value='2' />
                                    </filter>
                                  </entity>
                                </fetch>");



            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            tracing.Trace("Entities Count " + col.TotalRecordCount.ToString());
            return col.Entities.Count;

        }

        public static int CheckProductByNameAndPartNumber(IOrganizationService service, ITracingService tracing, string productName, string partNumber)
        {


            string newProductName = System.Security.SecurityElement.Escape(productName);
            string specialChar = @"\";
            foreach (var item in specialChar)
            {
                if (partNumber.Contains(item))
                    newProductName = "@" + newProductName;

                //   tracing.Trace("partNumber");
            }
            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='product'>
                                    <attribute name='name' />
                                    <attribute name='productid' />
                                    <attribute name='productnumber' />
                                    <attribute name='description' />
                                    <attribute name='statecode' />
                                    <attribute name='productstructure' />
                                    <order attribute='productnumber' descending='false' />
                                    <filter type='and'>
                                        <condition attribute='ap360_partnumber' operator='eq' value='" + partNumber + @"' />
                                       <condition attribute='name' operator='eq' value='" + newProductName + @"' />
                                      <condition attribute='productstructure' operator='eq' value='1' />
                                         <condition attribute='statecode' operator='eq' value='0' />

                                    </filter>
                                  </entity>
                                </fetch>");

            //Status (statecode) 0= Active
            //<condition attribute='name' operator= 'like' value = '%" + productName + @"%' />
            //<condition attribute='ap360_partnumber' operator= 'like' value = '%" + partNumber + @"%' />



            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            tracing.Trace(col.Entities.Count.ToString() + " Products exists ");
            //throw new InvalidPluginExecutionException(productName+" "+partNumber+" "+ col.Entities.Count.ToString());
            return col.Entities.Count;

        }
        public static Entity getCoreProduct(IOrganizationService service, ITracingService tracingService, string partNumber)
        {
            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='product'>
                                    <attribute name='name' />
                                    <attribute name='productid' />
                                    <attribute name='productnumber' />
                                    <attribute name='description' />
                                    <order attribute='productnumber' descending='false' />
                                    <filter type='and'>
                                      <condition attribute='ap360_partnumber' operator='eq' value='" + partNumber + @"-Core' />
                                    </filter>
                                  </entity>
                                </fetch>");
            EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            Entity product = null;
            if (col.Entities.Count > 0)
            {
                product = col.Entities[0];
            }
            return product;

        }
        public static bool checkIfProductIsAlreadyCreatedOrNot(IOrganizationService service, ITracingService tracingService, string description, string partNumber, ref List<string> lstExistingPartsId)
        {
            tracingService.Trace("inside checkIfProductIsAlreadyCreatedOrNot");
            //string descriptionInitials = description.Substring(0, 2);
            //string partNumberInitials = partNumber.Substring(0, 2);
            bool isPotentialMatched = false;


            List<string> partNumberList = splitStringIntoList(tracingService, partNumber);
            List<string> descriptionList = splitStringIntoList(tracingService, description);


            tracingService.Trace("After spliting");
            string filterPart = "";
            for (int i = 0; i < partNumberList.Count; i++)
            {
                filterPart += "<condition attribute='ap360_partnumber' operator='like' value='%" + partNumberList[i] + @"%' />";
            }
            tracingService.Trace("After partNumberList");

            string filterDescription = "";
            for (int i = 0; i < descriptionList.Count; i++)
            {
                filterDescription += " <condition attribute='name' operator='like' value='%" + descriptionList[i] + @"%' />";
            }

            tracingService.Trace("After descriptionList");

            string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                          <entity name='product'>
                                            <attribute name='name' />
                                            <attribute name='productid' />
                                            <attribute name='productnumber' />    
                                            <attribute name='ap360_partnumber' />
                                            <attribute name='description' />
                                            <attribute name='productstructure' />
                                            <order attribute='name' descending='false' />
                                          
                                              <filter type='or'>
                                      
                                        " + filterPart + " " +
                          "" + filterDescription +
                                @"</filter>
                                         
                                          </entity>
                                        </fetch>");
            EntityCollection col;
            // throw new InvalidPluginExecutionException(fetchXml.ToString());
            col = service.RetrieveMultiple(new FetchExpression(fetchXml));
            tracingService.Trace("Get total count of entites with pre existing names");
            if (col.Entities.Count > 0)
            {

                StringBuilder threeCharStringOfPartNumber = new StringBuilder();
                StringBuilder threeCharStringOfDescription = new StringBuilder();
                bool partNumberExist = false;
                    bool nameExist = false;
                char[] charArrPartNumer = partNumber.ToCharArray();
                char[] charArrDescription = description.ToCharArray();

                tracingService.Trace("col.Entities.Count is " + col.Entities.Count.ToString());
                for (int j = 0; j < col.Entities.Count; j++)
                {

                    tracingService.Trace(col.Entities[j].GetAttributeValue<string>("name").ToLower() + "-- " + col.Entities[j].GetAttributeValue<string>("productnumber").ToLower());

                    //Loop for partNUmbe                   
                    for (int i = 0; i < charArrPartNumer.Length; i++)
                    {
                        if (i < charArrPartNumer.Length - 2)
                        {

                            if (charArrPartNumer[i] != ' ' && charArrPartNumer[i + 1] != ' ' && charArrPartNumer[i + 2] != ' ')
                            {
                                threeCharStringOfPartNumber.Append(charArrPartNumer[i]);
                                threeCharStringOfPartNumber.Append(charArrPartNumer[i + 1]);
                                threeCharStringOfPartNumber.Append(charArrPartNumer[i + 2]);

                                string productNumber = col.Entities[j].GetAttributeValue<string>("ap360_partnumber") ?? null;
                                if (productNumber != null)
                                {
                                    productNumber = productNumber.ToLower();
                                    if (productNumber.Contains(threeCharStringOfPartNumber.ToString().ToLower()))
                                    {
                                        partNumberExist = true;
                                        //Console.WriteLine("-----------------------partNumberExist " + partNumberExist);
                                    }
                                }

                                threeCharStringOfPartNumber.Clear();
                            }
                           
                        }
                    }

                    //Loop for description
                    for (int i = 0; i < charArrDescription.Length; i++)
                    {
                        if (i < charArrDescription.Length - 3)
                        {
                            if (charArrDescription[i] != ' ' && charArrDescription[i + 1] != ' ' && charArrDescription[i + 2] != ' ' && charArrDescription[i + 3] != ' ')
                            {
                                threeCharStringOfDescription.Append(charArrDescription[i]);
                                threeCharStringOfDescription.Append(charArrDescription[i + 1]);
                                threeCharStringOfDescription.Append(charArrDescription[i + 2]);
                                threeCharStringOfDescription.Append(charArrDescription[i + 3]);

                                // Console.WriteLine("-----N------" + threeCharStringOfDescription);
                                string name = col.Entities[j].GetAttributeValue<string>("name") ?? null;
                                if (name != null)
                                {
                                    name = name.ToLower();
                                    if (name.Contains(threeCharStringOfDescription.ToString().ToLower()))
                                    {
                                        nameExist = true;
                                        // Console.WriteLine("-----------------------partNumberExist " + partNumberExist);
                                    }
                                }
                                threeCharStringOfDescription.Clear();
                            }
                        }
                    }

                    if (nameExist && partNumberExist)
                    {

                        isPotentialMatched = true;
                        lstExistingPartsId.Add(col.Entities[j].GetAttributeValue<Guid>("productid").ToString());
                        tracingService.Trace("-------------////////////////----------isPotentialMatched");
                        // return isPotentialMatched;
                    }

                    nameExist = false;
                    partNumberExist = false;

                }


                tracingService.Trace("After loop");
            }

            return isPotentialMatched;
        }

        private static List<string> splitStringIntoList(ITracingService tracingService, string strRecord)
        {
            StringBuilder threeCharStringOfRecord = new StringBuilder();

            List<string> lstRecord = new List<string>();
            tracingService.Trace("splitStringIntoList ");
            char[] charArrRecord = strRecord.ToCharArray();
            //Loop for partNUmber
            for (int i = 0; i < charArrRecord.Length; i++)
            {
                if (i < charArrRecord.Length - 2)
                {
                        if (charArrRecord[i] != ' ' && charArrRecord[i + 1] != ' ' && charArrRecord[i + 2] != ' ')
                        {
                            threeCharStringOfRecord.Append(charArrRecord[i]);
                            threeCharStringOfRecord.Append(charArrRecord[i + 1]);
                            threeCharStringOfRecord.Append(charArrRecord[i + 2]);

                            lstRecord.Add(threeCharStringOfRecord.ToString());

                            threeCharStringOfRecord.Clear();

                        }

                }
            }
            return lstRecord;

        }

        public static Entity getProductBasedOnBarcode(IOrganizationService service, ITracingService tracingService, string barcode)
        {

        string fetchXml = (@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                                      <entity name='product'>
                                                        <attribute name='name' />
                                                        <attribute name='productid' />
                                                        <attribute name='currentcost' />
                                                        <attribute name='productnumber' />
                                                        <attribute name='ap360_partnumber' />
                                                        <order attribute='productnumber' descending='false' />
                                                        <filter type='and'>
                                                          <condition attribute='ap360_barcode' operator='eq' value='" + barcode + @"' />
                                                        </filter>
                                                      </entity>
                                                    </fetch>");

        EntityCollection col = service.RetrieveMultiple(new FetchExpression(fetchXml));

            if (col.Entities.Count > 0)
            {
                return col.Entities[0];
            }
            return null;
        
        }
    }
}

//foreach (var eachCol in col.Entities)
//{
//    string part = eachCol.GetAttributeValue<string>("ap360_partnumber");
//    if (part != null)
//    {
//        if (part.Contains(partNumber))
//        {
//            return eachCol;
//        }
//    }
//}
//return null;