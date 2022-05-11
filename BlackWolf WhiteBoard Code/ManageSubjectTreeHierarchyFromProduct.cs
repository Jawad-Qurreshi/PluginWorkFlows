using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class ManageSubjectTreeHierarchyFromProduct : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = factory.CreateOrganizationService(context.UserId);

                var subject = new Subject();
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    if (context.MessageName.ToLower() == "create")
                    {

                        Guid newSubId = Guid.Empty;
                        Entity Target = (Entity)context.InputParameters["Target"];
                        if (Target != null)
                        {
                            if (Target.LogicalName.ToLower() == "product")
                            {
                                tracingService.Trace("Inside Product Create ");

                                if (context.Depth == 1)//Becuase the product we are creating from WOP and Quote Proudcts are already acitvated
                                {
                                    Product.activateProduct(service, tracingService, Target.Id);

                                }
                                Target = service.Retrieve(Target.LogicalName, Target.Id, new ColumnSet(true));
                                var productstructure = Target.GetAttributeValue<OptionSetValue>("productstructure").Value;
                                if (productstructure == 2) //  Product Family.
                                {
                                    var productName = Target.GetAttributeValue<string>("name");
                                    var parentproductid = Target.GetAttributeValue<EntityReference>("parentproductid");

                                    if (parentproductid != null)
                                    {
                                        Entity ParentproductEntity = service.Retrieve(Target.LogicalName, parentproductid.Id, new ColumnSet(true));
                                        if (ParentproductEntity != null)
                                        {
                                            string parentProductSubjecttreeid = ParentproductEntity.GetAttributeValue<string>("ap360_subjecttreeid");

                                            newSubId = subject.CreateSubject(service, tracingService, productName, parentProductSubjecttreeid);
                                            Target["ap360_subjecttreeid"] = newSubId.ToString();

                                            service.Update(Target);
                                        }
                                    }
                                    //else
                                    //{
                                    //    newSubId = subject.CreateSubject(service, productName);
                                    //}
                                    // Update the Created Peoduct with the New Subject Tree ID
                                }
                            }
                        }
                    }


                    if (context.MessageName.ToLower() == "update")
                    {

                        tracingService.Trace("Inside Product Update ");

                        Guid newSubId = Guid.Empty;
                        Entity Target = (Entity)context.InputParameters["Target"];
                        if (Target != null)
                        {
                            if (Target.LogicalName.ToLower() == "product")
                            {
                                // throw new InvalidPluginExecutionException("Custom Error " );
                                if (Target.Contains("name") || Target.Contains("parentproductid"))
                                {
                                    tracingService.Trace("Name updated or Parent Updated ");

                                    Target = service.Retrieve(Target.LogicalName, Target.Id, new ColumnSet(true));
                                    var productstructure = Target.GetAttributeValue<OptionSetValue>("productstructure").Value;
                                    if (productstructure == 2) //  Product Family.
                                    {
                                        var productName = Target.GetAttributeValue<string>("name");
                                        EntityReference parent = Target.GetAttributeValue<EntityReference>("parentproductid") != null ? Target.GetAttributeValue<EntityReference>("parentproductid") : null;
                                        Entity parentProductEntity = null;
                                        string parentSubjectTreeId = null;
                                        if (parent != null)
                                        {
                                            parentProductEntity = service.Retrieve(parent.LogicalName, parent.Id, new ColumnSet("ap360_subjecttreeid"));
                                            if (parentProductEntity != null)
                                            {
                                                parentSubjectTreeId = parentProductEntity.GetAttributeValue<string>("ap360_subjecttreeid");

                                            }
                                        }
                                        string subjecttreeid = Target.GetAttributeValue<string>("ap360_subjecttreeid");

                                        if (subjecttreeid != "" && subjecttreeid != string.Empty)
                                        {
                                            tracingService.Trace("Subject Id Exists on Product " + subjecttreeid);

                                            subject.updateSubject(service, tracingService, productName, parentSubjectTreeId, subjecttreeid);


                                        }

                                    }
                                }
                            }
                        }
                    }
                }
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference)
                {
                    if (context.MessageName.ToLower() == "delete")
                    {
                        tracingService.Trace("Inside Product delete ");


                        EntityReference TargetRef = (EntityReference)context.InputParameters["Target"];

                        if (TargetRef.LogicalName.ToLower() == "product")
                        {
                            var Target = service.Retrieve(TargetRef.LogicalName, TargetRef.Id, new ColumnSet(true));
                            var subjecttreeToDelete = Target.GetAttributeValue<string>("ap360_subjecttreeid");

                            var productstructure = Target.GetAttributeValue<OptionSetValue>("productstructure").Value;
                            if (productstructure == 2) //  Product Family.
                            {
                                if (subjecttreeToDelete != null)
                                {
                                    subject.DeleteSubject(service, tracingService, subjecttreeToDelete);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}