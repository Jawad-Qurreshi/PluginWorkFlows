using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class PreventCompletedBookingDeletionforBWTechnicain : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // throw new InvalidPluginExecutionException("PreventCompletedBookingDeletionforBWTechnicain");

            #region Setup
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationService service = ((IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory))).
            CreateOrganizationService(new Guid?(context.UserId));
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            //Guid userid = ((WhoAmIResponse)service.Execute(new WhoAmIRequest())).UserId;


            Guid userid = context.InitiatingUserId;

            // Guid? userid = new Guid?(context.UserId);
            if (context.MessageName.ToLower() == "delete")
            {


                #endregion
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference)
                {

                    EntityReference entityref = (EntityReference)context.InputParameters["Target"];

                    if (entityref.LogicalName.ToLower() != "bookableresourcebooking") return;

                    bool isUserAllowtoDelete = true;
                    try
                    {

                        Entity preImage = (Entity)context.PreEntityImages["Image"];
                        if (preImage.Contains("bookingstatus"))
                        {
                            tracingService.Trace("Pre Delete");
                            tracingService.Trace("Image Contain Booking Status");

                            EntityReference bookingStatusRef = preImage.GetAttributeValue<EntityReference>("bookingstatus") != null ? preImage.GetAttributeValue<EntityReference>("bookingstatus") : null;
                            if (bookingStatusRef != null)
                            {
                                tracingService.Trace(" Booking Status is not null");

                                if (bookingStatusRef.Id.ToString().ToLower() == "f16d80d1-fd07-4237-8b69-187a11eb75f9")//Scheduled
                                {
                                    tracingService.Trace(" Booking Status is scheduled");

                                }
                                else
                                {
                                    tracingService.Trace(" Booking Status is not scheduled");


                                    isUserAllowtoDelete = getUserRoleAuthentication(service, tracingService, userid);
                                    if (!isUserAllowtoDelete)
                                    {
                                        tracingService.Trace("User allowed to delted : " + isUserAllowtoDelete.ToString());
                                        throw new InvalidPluginExecutionException("Contact Admin team to Delete Booking ");
                                    }
                                }
                            }
                        }
                    }

                    catch (Exception ex)
                    {
                        throw new Exception("" + ex.Message);
                    }
                }
            }
        }


        private static bool getUserRoleAuthentication(IOrganizationService service, ITracingService tracing, Guid? userGuid)
        {
            bool isUserAllowtoDelete = true;
            string roleID = null;
            string bwTechnicainRoleId = "7446DBE1-9C8B-EA11-A812-000D3A33F47E".ToLower();
            tracing.Trace("User Guid : " + userGuid.ToString());
            QueryExpression qe = new QueryExpression("systemuserroles");
            qe.ColumnSet.AddColumns("systemuserid");
            qe.Criteria.AddCondition("systemuserid", ConditionOperator.Equal, userGuid);

            LinkEntity link1 = qe.AddLink("systemuser", "systemuserid", "systemuserid", JoinOperator.Inner);
            link1.Columns.AddColumns("fullname", "internalemailaddress");
            LinkEntity link = qe.AddLink("role", "roleid", "roleid", JoinOperator.Inner);
            link.Columns.AddColumns("roleid", "name");
            EntityCollection results = service.RetrieveMultiple(qe);
            if (results.Entities.Count > 0)
                tracing.Trace("Number of Role assigned to user " + results.Entities.Count.ToString());
            int count = 0;
            tracing.Trace("BW Technaican Guid : " + bwTechnicainRoleId.ToString());
            foreach (Entity Userrole in results.Entities)
            {
                if (Userrole.Attributes.Contains("role2.roleid"))
                {
                    roleID = (Userrole.Attributes["role2.roleid"] as AliasedValue).Value.ToString();
                    tracing.Trace(count.ToString() + ": Role Id: " + roleID.ToString());
                    if (roleID.ToLower() == bwTechnicainRoleId)
                    {
                        tracing.Trace("roleID.ToLower() == bwTechnicainRoleId");
                        tracing.Trace("ID matches");
                        //if (results.Entities.Count == 1)
                        //{
                        isUserAllowtoDelete = false;

                        // }
                    }
                }
            }

            return isUserAllowtoDelete;

        }


    }
}