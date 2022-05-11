using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class UpdateParentRecordBasedOnChildEntityActivity : IPlugin
    {


        private readonly string _unsecureString;
        public UpdateParentRecordBasedOnChildEntityActivity (string unsecureString, string secureString)
        {
            if (String.IsNullOrWhiteSpace(unsecureString) ||
                String.IsNullOrWhiteSpace(secureString))
            {
                throw new InvalidPluginExecutionException("Unsecure and secure strings are required for this plugin to execute.");
            }

            _unsecureString = unsecureString;
        }

        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
               //throw new InvalidPluginExecutionException("UpdateParentRecordBasedOnChildEntityActivity");


                ITracingService tracingService =
                  (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                IPluginExecutionContext context = (IPluginExecutionContext)
                    serviceProvider.GetService(typeof(IPluginExecutionContext));

                IOrganizationServiceFactory factory =
                     (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = factory.CreateOrganizationService(context.UserId);


                Entity entity = null;

                string[] values = _unsecureString.Split(',');


                string ChildEntityFieldToRetrieveFirst = values[0].Trim();
                string ParentEntityfieldtoUpdateFirst = values[1].Trim();
                string Firstfieldtype = values[2].Trim();
            //    tracingService.Trace("Frist Field Type is " + Firstfieldtype);


                string ChildEntityFieldToRetrieveSecond = values[3].Trim();
                string ParentEntityfieldtoUpdateSecond = values[4].Trim();
                string Secondfieldtype = values[5].Trim();
            //    tracingService.Trace("Second Field Type is " + Secondfieldtype);

                string ChildEntityFieldToRetrieveThird = values[6].Trim();
                string ParentEntityfieldtoUpdatThird = values[7].Trim();
                string Thirdfieldtype = values[8].Trim();
            //    tracingService.Trace("Third Field Type is "+Thirdfieldtype );


                string ParentLookupFieldonChildEntity = values[9].Trim();
            //    tracingService.Trace("Parent Look up on Child Entity "+ParentLookupFieldonChildEntity);





                //string str =     "ap360_partsaleprice,ap360_partssaleprice,money,   NA,NA,NA,   NA,NA,NA, ap360_quoteserviceid"+

                if (context.MessageName.ToLower() == "create")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        // if (context.Depth > 1) return;
                        entity = (Entity)context.InputParameters["Target"];
                        Guid ChildEntityGuid = entity.Id;
                        string ChildEntityName = entity.LogicalName;

                        Methods.MainFunction(service,tracingService, ChildEntityName, ChildEntityGuid, ParentLookupFieldonChildEntity, ChildEntityFieldToRetrieveFirst, ParentEntityfieldtoUpdateFirst, context.MessageName.ToLower(), Firstfieldtype);
                        if (ChildEntityFieldToRetrieveSecond != "NA")//only if more then one field in secure configuration
                            Methods.MainFunction(service, tracingService, ChildEntityName, ChildEntityGuid, ParentLookupFieldonChildEntity, ChildEntityFieldToRetrieveSecond, ParentEntityfieldtoUpdateSecond, context.MessageName.ToLower(), Secondfieldtype);
                        if (ChildEntityFieldToRetrieveThird != "NA")//only if more then two field in secure configuration
                            Methods.MainFunction(service, tracingService, ChildEntityName, ChildEntityGuid, ParentLookupFieldonChildEntity, ChildEntityFieldToRetrieveThird, ParentEntityfieldtoUpdatThird, context.MessageName.ToLower(), Thirdfieldtype);

                        ////////////////////////////////////////////////////END //////////////////////////////////////////////////

                    }
                }

                if (context.MessageName.ToLower() == "update")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        // throw new InvalidPluginExecutionException(ChildEntityFieldToRetrieve);
                        entity = (Entity)context.InputParameters["Target"];
                        Guid ChildEntityGuid = entity.Id;
                        string ChildEntityName = entity.LogicalName;



                        if (entity.Contains(ChildEntityFieldToRetrieveFirst))
                        {
                            Methods.MainFunction(service, tracingService, ChildEntityName, ChildEntityGuid, ParentLookupFieldonChildEntity, ChildEntityFieldToRetrieveFirst, ParentEntityfieldtoUpdateFirst, context.MessageName.ToLower(), Firstfieldtype);
                        }
                        if (ChildEntityFieldToRetrieveSecond != "NA")
                        {
                            if (entity.Contains(ChildEntityFieldToRetrieveSecond))
                            {
                                if (ChildEntityFieldToRetrieveSecond != "NA")//only if more then one field in secure configuration
                                    Methods.MainFunction(service, tracingService, ChildEntityName, ChildEntityGuid, ParentLookupFieldonChildEntity, ChildEntityFieldToRetrieveSecond, ParentEntityfieldtoUpdateSecond, context.MessageName.ToLower(), Secondfieldtype);

                            }
                        }
                        if (ChildEntityFieldToRetrieveSecond != "NA")
                        {
                            if (entity.Contains(ChildEntityFieldToRetrieveThird))
                            {

                                if (ChildEntityFieldToRetrieveThird != "NA")//only if more then two field in secure configuration
                                    Methods.MainFunction(service, tracingService, ChildEntityName, ChildEntityGuid, ParentLookupFieldonChildEntity, ChildEntityFieldToRetrieveThird, ParentEntityfieldtoUpdatThird, context.MessageName.ToLower(), Thirdfieldtype);
                            }


                        }
                    }

                }
                if (context.MessageName.ToLower() == "delete")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference)
                    {

                        EntityReference entityref = (EntityReference)context.InputParameters["Target"];
                        //entity = (Entity)context.InputParameters["Target"];
                        Guid ChildEntityGuid = entityref.Id;
                        string ChildEntityName = entityref.LogicalName;


                        Methods.MainFunction(service, tracingService, ChildEntityName, ChildEntityGuid, ParentLookupFieldonChildEntity, ChildEntityFieldToRetrieveFirst, ParentEntityfieldtoUpdateFirst, context.MessageName.ToLower(), Firstfieldtype);
                        if (ChildEntityFieldToRetrieveSecond != "NA")//only if more then one field in secure configuration
                            Methods.MainFunction(service, tracingService,ChildEntityName, ChildEntityGuid, ParentLookupFieldonChildEntity, ChildEntityFieldToRetrieveSecond, ParentEntityfieldtoUpdateSecond, context.MessageName.ToLower(), Secondfieldtype);
                        if (ChildEntityFieldToRetrieveThird != "NA")//only if more then two field in secure configuration
                            Methods.MainFunction(service, tracingService, ChildEntityName, ChildEntityGuid, ParentLookupFieldonChildEntity, ChildEntityFieldToRetrieveThird, ParentEntityfieldtoUpdatThird, context.MessageName.ToLower(), Thirdfieldtype);

                    }

                }
            }

            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }

    }
}
