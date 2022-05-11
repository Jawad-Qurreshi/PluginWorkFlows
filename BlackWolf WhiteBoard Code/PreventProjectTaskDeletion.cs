using System;
using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class PreventProjectTaskDeletion : IPlugin
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

                Entity entity = null;


                if (context.MessageName.ToLower() == "delete")
                {
                    if (context.Depth == 1)// This is important, we want to prevent Project task deletion from project
                        //but WOST and QST should delete(context.Depth >1) and corresponding Project task should delete
                    {
                       throw new InvalidPluginExecutionException("Unable to delete project task!");
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