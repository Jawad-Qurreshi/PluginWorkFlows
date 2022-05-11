using BlackWolf_WhiteBoard_Code.Model;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class CopyWOServiceTaskInBooking : CodeActivity
    {

        protected override void Execute(CodeActivityContext executionContext)
        {
          // throw new InvalidPluginExecutionException("CopyWOServiceTaskInBooking");

            var context = executionContext.GetExtension<IWorkflowContext>();
            var serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            var organizationService = serviceFactory.CreateOrganizationService(context.UserId);
            ITracingService tracingService = executionContext.GetExtension<ITracingService>();
            CopyServiceTaskInBooking(organizationService, tracingService, context);
        }

        #region Private Methods

        private void CopyServiceTaskInBooking(IOrganizationService organizationService, ITracingService tracingService, IWorkflowContext context)
        {

            //Thread.Sleep(3000);
            //Thread.Sleep(3000);
            var bookableResourceBooking = new BookableResourceBooking();
            bookableResourceBooking.CopyWOSTInBooking(organizationService, tracingService, context);
        }

        #endregion
    }

}
