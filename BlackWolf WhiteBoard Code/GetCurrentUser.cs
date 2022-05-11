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
    public class GetCurrentUser : CodeActivity
    {
        [Output("Current User")]
        [ReferenceTarget("systemuser")]
        public OutArgument<EntityReference> CurrentUser { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
           // throw new InvalidPluginExecutionException("GetCurrentUser");

            try
            {
                context.GetExtension<ITracingService>();
                IWorkflowContext extension = context.GetExtension<IWorkflowContext>();
                this.CurrentUser.Set((ActivityContext)context, new EntityReference("systemuser", ((IExecutionContext)extension).UserId));
            }
            catch (Exception ex)
            {
                throw new InvalidWorkflowException(ex.Message);
            }
        }
    }

}
