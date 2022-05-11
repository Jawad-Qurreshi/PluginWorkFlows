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
    public class AccountPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            StringBuilder strError = new StringBuilder();
            //  throw new InvalidPluginExecutionException("CreateContactOnCreationofAccount");

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

                if (context.MessageName.ToLower() == "create")
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        entity = (Entity)context.InputParameters["Target"];

                        if (entity.LogicalName == "account")
                        {
                            tracingService.Trace("Entity is account");

                            //Guid newlyCreatedContactGuid = Guid.Empty;
                            //if (entity.Contains("originatingleadid"))
                            EntityReference leadRef = entity.GetAttributeValue<EntityReference>("originatingleadid") ?? null;
                            if (leadRef != null)
                            {
                                Entity reterivedLeadContactNumber = service.Retrieve(leadRef.LogicalName, leadRef.Id, new ColumnSet("mobilephone", "telephone1"));
                                string primaryNumber = reterivedLeadContactNumber.GetAttributeValue<string>("mobilephone");
                                string secondNumber = reterivedLeadContactNumber.GetAttributeValue<string>("telephone1");

                                if (primaryNumber != null)
                                {
                                    entity["ap360_phonenumber1"] = primaryNumber;
                                }
                                if (secondNumber != null)
                                {
                                    entity["ap360_phonenumber2"] = secondNumber;
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