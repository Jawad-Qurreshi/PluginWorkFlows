using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class UpdateParentMinutesOnCreationofChild : CodeActivity
    {

        #region Input Properties
        [Input("ChildFieldToRetrieve")]
        public InArgument<string> ChildFieldToRetrieve { get; set; }
        [Input("ParentEntityfieldtoUpdate")]
        public InArgument<string> ParentEntityfieldtoUpdate { get; set; }
        [Input("ParentLookupFieldonChildEntity")]
        public InArgument<string> ParentLookupFieldonChildEntity { get; set; }

        #endregion

        protected override void Execute(CodeActivityContext executionContext)
        {
           //throw new InvalidPluginExecutionException("UpdateParentMinutesOnCreationofChild");

            var context = executionContext.GetExtension<IWorkflowContext>();
            var serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            var service = serviceFactory.CreateOrganizationService(context.UserId);
            ITracingService tracingService = executionContext.GetExtension<ITracingService>();

                Guid ChildEntityGuid = context.PrimaryEntityId;
                string ChildEntityName = context.PrimaryEntityName;


            string ChildFieldToRetrieve = this.ChildFieldToRetrieve.Get<string>(executionContext);
            string ParentEntityfieldtoUpdate = this.ParentEntityfieldtoUpdate.Get<string>(executionContext);
            string ParentLookupFieldonChildEntity = this.ParentLookupFieldonChildEntity.Get<string>(executionContext);


            EntityReference parentEntityReference;
            Entity ChildEntity = service.Retrieve(ChildEntityName, ChildEntityGuid, new ColumnSet(ParentLookupFieldonChildEntity));

            int sumofChildEntityDuration = 0;
            if (ChildEntity != null)
            {
                parentEntityReference = ChildEntity.GetAttributeValue<EntityReference>(ParentLookupFieldonChildEntity) != null ? ChildEntity.GetAttributeValue<EntityReference>(ParentLookupFieldonChildEntity) : null;
                if (parentEntityReference != null)
                {
                    DataCollection<Entity> lstChildEntity = Methods.GetChildRecords(service, parentEntityReference.Id, ParentLookupFieldonChildEntity, ChildEntityName, ChildFieldToRetrieve);
                    foreach (Entity childEntity in lstChildEntity)
                    {
                        sumofChildEntityDuration += childEntity.GetAttributeValue<int>(ChildFieldToRetrieve);

                    }
                }

                Entity parentEntity = new Entity(parentEntityReference.LogicalName);
                parentEntity.Id = parentEntityReference.Id;
                parentEntity[ParentEntityfieldtoUpdate] = sumofChildEntityDuration;
                service.Update(parentEntity);
            }




        }
    }

}
