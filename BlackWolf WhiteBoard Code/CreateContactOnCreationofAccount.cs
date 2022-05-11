using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class CreateContactOnCreationofAccount : IPlugin
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


                            string accountName = null;
                            string email = null;
                            string phoneNumber1 = null;
                            string phoneNumber2 = null;
                            Guid newlyCreatedContactGuid = Guid.Empty;
                            if (entity.Contains("name"))
                                accountName = entity.GetAttributeValue<string>("name");
                            if (entity.Contains("emailaddress1"))
                                email = entity.GetAttributeValue<string>("emailaddress1");

                            if (entity.Contains("ap360_phonenumber1"))
                                phoneNumber1 = entity.GetAttributeValue<string>("ap360_phonenumber1");
                            if (entity.Contains("ap360_phonenumber2"))
                                phoneNumber2 = entity.GetAttributeValue<string>("ap360_phonenumber2");
                            //if (entity.Contains("emailaddress1"))
                            //    email = entity.GetAttributeValue<string>("emailaddress1");


                            if (accountName != null)
                            {
                                tracingService.Trace("Acccount Name is not null");

                                Entity newContact = new Entity("contact");
                                string[] splitedName = accountName.Split(' ');

                                System.Text.StringBuilder lastName = new System.Text.StringBuilder();
                                for (int counter = 1; counter <= splitedName.Length-1; counter++)
                                {
                                    lastName.Append(splitedName[counter]);
                                    lastName.Append(" ");
                                }

                                newContact["lastname"] = splitedName[0];
                                newContact["firstname"] = lastName.ToString();

                                if (email != null)
                                    newContact["emailaddress1"] = email; 
                              
                                if (phoneNumber1 != null)
                                    newContact["telephone1"] = phoneNumber1; 
                                if (phoneNumber2 != null)
                                    newContact["mobilephone"] = phoneNumber2;
                               
                                newContact["parentcustomerid"] = new EntityReference("account", entity.Id);
                                newlyCreatedContactGuid = service.Create(newContact);
                                tracingService.Trace("Contact is created : " + newlyCreatedContactGuid.ToString());


                                if (newlyCreatedContactGuid != Guid.Empty)
                                {
                                    Entity account = new Entity("account");

                                    account["primarycontactid"] = new EntityReference("contact", newlyCreatedContactGuid);
                                    account.Id = entity.Id;
                                    service.Update(account);

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