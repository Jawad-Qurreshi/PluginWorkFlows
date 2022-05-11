using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code.Model
{
    public class CreateProductFromDescriptionAndRelatetoProductFamily : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();


            try
            {

                ITracingService tracingService =
                    (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                IPluginExecutionContext context = (IPluginExecutionContext)
                    serviceProvider.GetService(typeof(IPluginExecutionContext));

                IOrganizationServiceFactory factory =
                     (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = factory.CreateOrganizationService(context.UserId);



                string entityId = (string)context.InputParameters["entityGUID"];
                string entityName = (string)context.InputParameters["entityName"];


                //context.OutputParameters["Message"] = "Hello this is message";
                //context.OutputParameters["IsProductPotentialMatched"] = true;
                if (entityId == "")
                {
                    context.OutputParameters["Message"] = "Not Processed"; //Alert::::This Message sholud not be changed due to conditional check in JS of qoute product and WOP
                    context.OutputParameters["IsProductPotentialMatched"] = false;
                    context.OutputParameters["IsSuccess"] = false;
                }
                else if (entityName == "ap360_quoteproduct" || entityName == "msdyn_incidenttypeproduct" || entityName == "msdyn_workorderproduct")
                {
                    CreateProductAndUpdateEntity(context, service, tracingService, entityName, entityId);
                }


                //commet

                //if (context.MessageName.ToLower() == "create")
                //{
                //    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                //    {
                //        if (context.Depth > 1) return;
                //        tracingService.Trace("Inside creation of product");
                //        entity = (Entity)context.InputParameters["Target"];

                //        if (entity.LogicalName == "ap360_quoteproduct" || entity.LogicalName == "msdyn_incidenttypeproduct" || entity.LogicalName == "msdyn_workorderproduct")
                //        {
                //            CreateProductAndUpdateEntity(service, tracingService, entity);
                //        }
                //    }
                //}

                //if (context.MessageName.ToLower() == "update")
                //{
                //    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                //    {
                //        if (context.Depth == 1)
                //        {
                //            entity = (Entity)context.InputParameters["Target"];

                //            if (entity.LogicalName == "ap360_quoteproduct" || entity.LogicalName == "msdyn_incidenttypeproduct" || entity.LogicalName == "msdyn_workorderproduct")
                //            {
                //                tracingService.Trace("Entity Name " + entity.LogicalName);

                //                if (entity.Contains("ap360_approveproduct"))
                //                {
                //                    tracingService.Trace("Approve Product Updated");
                //                    CreateProductAndUpdateEntity(service, tracingService, entity);

                //                    tracingService.Trace("Plugin Ended Successfully");

                //                }

                //            }
                //        }
                //    }
                //}



            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }
        public static void CreateProductAndUpdateEntity(IPluginExecutionContext context, IOrganizationService service, ITracingService tracingService, string entityName, string entityId)
        {
            Microsoft.Xrm.Sdk.Query.ColumnSet columset = null;

            if (entityName == "msdyn_workorderproduct")
            {
                columset = new Microsoft.Xrm.Sdk.Query.ColumnSet("ap360_bypassduplicatedetection", "ap360_name", "ap360_productfamily", "ap360_approveproduct", "ap360_partnumber", "msdyn_estimateunitcost");//msdyn_estimateunitcost = Part Cost  and other name is Name is Unit Cost
            }
            if (entityName == "ap360_quoteproduct")
            {
                columset = new Microsoft.Xrm.Sdk.Query.ColumnSet("ap360_bypassduplicatedetection", "ap360_name", "ap360_productfamily", "ap360_approveproduct", "ap360_partnumber", "ap360_unitcost");

            }
            if (entityName == "msdyn_incidenttypeproduct")
            {
                columset = new Microsoft.Xrm.Sdk.Query.ColumnSet("ap360_bypassduplicatedetection", "ap360_name", "ap360_productfamily", "ap360_approveproduct", "ap360_partnumber");

            }
            Entity productEntity = service.Retrieve(entityName, new Guid(entityId), columset);
            Guid newlyCreatedProductGuid = Guid.Empty;
            if (productEntity != null)
            {
                tracingService.Trace("product is not null");
                bool isProductApproved = false;
                isProductApproved = productEntity.GetAttributeValue<bool>("ap360_approveproduct");
                if (isProductApproved)
                {

                    tracingService.Trace("Product Approved");
                    string description = productEntity.GetAttributeValue<string>("ap360_name");
                    string partNumber = productEntity.GetAttributeValue<string>("ap360_partnumber");
                    bool bypassduplicatedetection = productEntity.GetAttributeValue<bool>("ap360_bypassduplicatedetection");

                    List<string> lstExistingPartsId = new List<string>();
                    tracingService.Trace(" before checkIfProductIsAlreadyCreatedOrNot isPotentialMatched ");
                    bool isPotentialMatched = false;
                    if (!bypassduplicatedetection)
                        isPotentialMatched = Product.checkIfProductIsAlreadyCreatedOrNot(service, tracingService, description, partNumber, ref lstExistingPartsId);
                    tracingService.Trace(" After checkIfProductIsAlreadyCreatedOrNot isPotentialMatched is " + isPotentialMatched);
                    if (isPotentialMatched == false)
                    {
                        EntityReference productFamily = productEntity.GetAttributeValue<EntityReference>("ap360_productfamily") != null ? productEntity.GetAttributeValue<EntityReference>("ap360_productfamily") : null;

                        Money productCost = null;
                        if (entityName == "msdyn_workorderproduct")
                        {
                            productCost = productEntity.GetAttributeValue<Money>("msdyn_estimateunitcost") != null ? productEntity.GetAttributeValue<Money>("msdyn_estimateunitcost") : null;
                        }
                        if (entityName == "ap360_quoteproduct")
                        {
                            productCost = productEntity.GetAttributeValue<Money>("ap360_unitcost") != null ? productEntity.GetAttributeValue<Money>("ap360_unitcost") : null;

                        }
                        if (entityName == "msdyn_incidenttypeproduct")
                        {
                            productCost = null;

                        }
                        if (productFamily != null)
                        {
                            tracingService.Trace("Before product Creation");
                            newlyCreatedProductGuid = Product.createProductFromDescription(service, tracingService, description, partNumber, productFamily, productCost);
                            tracingService.Trace("After product Creation");

                            //  Thread.Sleep(10000);
                            if (newlyCreatedProductGuid != Guid.Empty)
                            {
                                Entity newEntity = new Entity(entityName);
                                newEntity["ap360_product"] = new EntityReference("product", newlyCreatedProductGuid);
                                if (entityName == "msdyn_workorderproduct")
                                {
                                    newEntity["msdyn_product"] = new EntityReference("product", newlyCreatedProductGuid);
                                }
                                //  newEntity["ap360_productdescription"] = null;
                                //  newEntity["msdyn_name"] = description;
                                //  newEntity["ap360_name"] = description;

                                newEntity["ap360_approveproduct"] = false;
                                newEntity.Id = new Guid(entityId);
                                service.Update(newEntity);//This may be important here, because if we pass parmeter to javascript, it require one more 
                                context.OutputParameters["Message"] = "created#" + newlyCreatedProductGuid.ToString(); //Alert:Passing this to javascript iof WOP in function addProductOn
                                context.OutputParameters["IsSuccess"] = true;
                                //in last else 
                                context.OutputParameters["IsProductPotentialMatched"] = false;

                            }
                        }
                        else
                        {
                            context.OutputParameters["Message"] = "Product Family is not selected";
                            context.OutputParameters["IsProductPotentialMatched"] = false;
                            context.OutputParameters["IsSuccess"] = false;
                            //  throw new InvalidPluginExecutionException("Product Family is not selected");
                        }

                    }
                    else
                    {
                        //StringBuilder lstPotentialMatchedProduct = new StringBuilder();
                        //foreach (string str in lstExistingPartsId) {
                        //    lstPotentialMatchedProduct.Append(str);
                        //    lstPotentialMatchedProduct.Append(",");
                        //}



                        string lstPotentialMatchedProduct = string.Join(",", lstExistingPartsId);
                        context.OutputParameters["Message"] = "Similar Product Already exists! Either select from product field or create new using Bypassing Duplicate Detection";
                        context.OutputParameters["IsProductPotentialMatched"] = true;
                        context.OutputParameters["lstPotentialMatchedProduct"] = lstPotentialMatchedProduct;
                        context.OutputParameters["IsSuccess"] = true;
                        //throw new InvalidPluginExecutionException("Product Already Exist!Please Select From Products");
                    }

                }

            }
        }

    }

}