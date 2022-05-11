using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk.Query;

namespace BlackWolf_WhiteBoard_Code
{
    public class UpdateFSentitynameonupdateofprojecttask : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();
            //   throw new InvalidPluginExecutionException("CreateProductFromDescriptionAndRelatetoProductFamily");

            try
            {

                ITracingService tracingService =
                  (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                IPluginExecutionContext context = (IPluginExecutionContext)
                    serviceProvider.GetService(typeof(IPluginExecutionContext));

                IOrganizationServiceFactory factory =
                     (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = factory.CreateOrganizationService(context.UserId);


                Entity entity = null;


                //if (context.MessageName.ToLower() == "update")
                //{
                //    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                //    {
                //        if (context.Depth > 1) return;
                //        tracingService.Trace("Inside creation of product");
                //        entity = (Entity)context.InputParameters["Target"];

                //        if (entity.LogicalName == "ap360_quoteproduct" || entity.LogicalName == "msdyn_incidenttypeproduct" || entity.LogicalName == "msdyn_workorderproduct")
                //        {
                //           // CreateProductAndUpdateEntity(service, tracingService, entity);
                //        }
                //    }
                //}

                if (context.MessageName.ToLower() == "update")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        if (context.Depth == 1)
                        {
                            entity = (Entity)context.InputParameters["Target"];

                            if (entity.Contains("msdyn_subject"))
                            {
                                //tracingService.Trace("Entity Name " + entity.LogicalName);

                                EntityReference QStasklookuponPT = null;
                                String updatePTName = entity.GetAttributeValue<string>("msdyn_subject");
                                Entity postImage = (Entity)context.PostEntityImages["Image"];
                                if (postImage.Contains("ap360_quoteservicetaskid"))
                                {
                                    QStasklookuponPT = postImage.GetAttributeValue<EntityReference>("ap360_quoteservicetaskid") != null ? postImage.GetAttributeValue<EntityReference>("ap360_quoteservicetaskid") : null;
                                    if (QStasklookuponPT != null)
                                    {
                                        // throw new InvalidPluginExecutionException( QStasklookuponPT.LogicalName +" "+QStasklookuponPT.Id.ToString());
                                        Entity updateName = new Entity(QStasklookuponPT.LogicalName, QStasklookuponPT.Id);

                                        updateName["ap360_workrequested"] = updatePTName;
                                        service.Update(updateName);

                                    
                                    }
                                }
                                else if (postImage.Contains("ap360_quoteserviceid"))

                                {

                                    QStasklookuponPT = postImage.GetAttributeValue<EntityReference>("ap360_quoteserviceid") != null ? postImage.GetAttributeValue<EntityReference>("ap360_quoteserviceid") : null;
                                    if (QStasklookuponPT != null)
                                    {
                                        // throw new InvalidPluginExecutionException( QStasklookuponPT.LogicalName +" "+QStasklookuponPT.Id.ToString());
                                        Entity updateName = new Entity(QStasklookuponPT.LogicalName, QStasklookuponPT.Id);

                                        updateName["ap360_workrequested"] = updatePTName;
                                        service.Update(updateName);

                                    }
                                }
                                else if (postImage.Contains("ap360_quoteid"))

                                {

                                    QStasklookuponPT = postImage.GetAttributeValue<EntityReference>("ap360_quoteid") != null ? postImage.GetAttributeValue<EntityReference>("ap360_quoteid") : null;
                                    if (QStasklookuponPT != null)
                                    {
                                        // throw new InvalidPluginExecutionException( QStasklookuponPT.LogicalName +" "+QStasklookuponPT.Id.ToString());
                                        Entity updateName = new Entity(QStasklookuponPT.LogicalName, QStasklookuponPT.Id);

                                        updateName["name"] = updatePTName;
                                        service.Update(updateName);

                                    }
                                }

                                ///// work order number is attached in lookup
                                else if (postImage.Contains("ap360_workorderid"))

                                {

                                    QStasklookuponPT = postImage.GetAttributeValue<EntityReference>("ap360_workorderid") != null ? postImage.GetAttributeValue<EntityReference>("ap360_workorderid") : null;
                                    if (QStasklookuponPT != null)
                                    {
                                        // throw new InvalidPluginExecutionException( QStasklookuponPT.LogicalName +" "+QStasklookuponPT.Id.ToString());
                                        Entity updateName = new Entity(QStasklookuponPT.LogicalName, QStasklookuponPT.Id);

                                        updateName["ap360_workordername"] = updatePTName;
                                        service.Update(updateName);

                                    }
                                }

                                else if (postImage.Contains("ap360_workorderservicetaskid"))

                                {

                                    QStasklookuponPT = postImage.GetAttributeValue<EntityReference>("ap360_workorderservicetaskid") != null ? postImage.GetAttributeValue<EntityReference>("ap360_workorderservicetaskid") : null;
                                    if (QStasklookuponPT != null)
                                    {
                                        //throw new InvalidPluginExecutionException( QStasklookuponPT.LogicalName +" "+QStasklookuponPT.Id.ToString());
                                        Entity updateName = new Entity(QStasklookuponPT.LogicalName, QStasklookuponPT.Id);

                                        updateName["msdyn_description"] = updatePTName;
                                        service.Update(updateName);

                                    }
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