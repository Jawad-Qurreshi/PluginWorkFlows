using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class TempPluginCodeFORwoproductreturnlog : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {

            try
            {
                //  throw new InvalidPluginExecutionException("WorkOrderServcieTaskCalculation");

                ITracingService tracingService =
                  (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                IPluginExecutionContext context = (IPluginExecutionContext)
                    serviceProvider.GetService(typeof(IPluginExecutionContext));

                IOrganizationServiceFactory factory =
                     (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = factory.CreateOrganizationService(context.UserId);


                Entity entity = null;

                //  entity = (Entity)context.InputParameters["Target"];

                if (context.MessageName.ToLower() == "retrieve")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        if (entity.LogicalName == "ap360_woproductreturnlog")
                        {
                            throw new InvalidPluginExecutionException("Retervie Error");
                        }

                    }

                }
                else if (context.MessageName.ToLower() == "retrievemultiple")
                {
                    var businessEntityCollection = (EntityCollection)context.OutputParameters["BusinessEntityCollection"];



                    tracingService.Trace("Target is Entity " + businessEntityCollection.Entities.Count.ToString());

                    tracingService.Trace("Context Depth "+context.Depth.ToString());

                    //   throw new InvalidPluginExecutionException("Custom Errro");
                    tracingService.Trace("Record Count:" + businessEntityCollection.Entities.Count);

                    foreach (Entity eachEntity in businessEntityCollection.Entities)
                    {
                        var watch1 = System.Diagnostics.Stopwatch.StartNew();

                      //  Entity reterviedEntity = service.Retrieve(eachEntity.LogicalName,eachEntity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
                        if (eachEntity.Contains("ap360_name"))//$ time remaining
                        {
                            eachEntity.Attributes.Add("ap360_name", "xyz");
                        }
                        else
                        {
                            eachEntity["ap360_name"] = "abc";
                            eachEntity["ap360_returneddate"] = eachEntity["modifiedon"];
                            eachEntity["createdon"] = eachEntity["modifiedon"];


                        }

                        Entity updateEntity = new Entity(eachEntity.LogicalName,eachEntity.Id);
                        updateEntity["ap360_quantityreturned"] = 90;

                      //  service.Update(updateEntity);
                        watch1.Stop();
                        var elapsedMs = watch1.ElapsedMilliseconds;
                        tracingService.Trace("Time " + elapsedMs.ToString());



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