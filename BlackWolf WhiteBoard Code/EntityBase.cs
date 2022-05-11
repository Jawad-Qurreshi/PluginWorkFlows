using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
  public  class EntityBase
    {

        #region Varibales and Properties       

        #endregion

        #region Virtual Methods

        public virtual void Execute(IPluginExecutionContext executionContext, IOrganizationService organizationService, ITracingService tracingService)
        {
        }

        public string GetStringAttributeValue(Entity entity, string attributeName)
        {
            var attributeValue = string.Empty;
            if (entity.Contains(attributeName))
                attributeValue = entity[attributeName] as string;
            return attributeValue;
        }

        public static int GetIntAttributeValue(Entity entity, string attributeName)
        {
            var attributeValue = 0;
            if (entity.Contains(attributeName) && entity[attributeName] is int)
                attributeValue = (int)entity[attributeName];
            return attributeValue;
        }
        public double GetDoubleAttributeValue(Entity entity, string attributeName)
        {
            var attributeValue = 0.00;
            if (entity.Contains(attributeName) && entity[attributeName] is double)
                attributeValue = (double)entity[attributeName];
            return attributeValue;
        }

        public static decimal GetDecimalAttributeValue(Entity entity, string attributeName)
        {
            decimal attributeValue = 0;
            if (entity.Contains(attributeName) && entity[attributeName] is decimal)
                attributeValue = (decimal)entity[attributeName];
            return attributeValue;
        }
        public float GetFloatAttributeValue(Entity entity, string attributeName)
        {
            float attributeValue = 0;
            //if (entity.Contains(attributeName) && entity[attributeName] is float)
            //    attributeValue = (float)entity[attributeName];
            if (entity.Contains(attributeName))
                attributeValue = Convert.ToInt32(entity[attributeName]);
            return attributeValue;
        }

        public static decimal GetMoneyAttributeValue(Entity entity, string attributeName)
        {
            decimal attributeValue = 0.0m;
            if (entity.Contains(attributeName) && entity[attributeName] is Money)
                return ((Money)entity[attributeName]).Value;
            return attributeValue;
        }

        public DateTime GetDateTimeAttributeValue(Entity entity, string attributeName)
        {
            var attributeValue = new DateTime();
            if (entity.Contains(attributeName))
                attributeValue = entity[attributeName] is DateTime ? (DateTime)entity[attributeName] : new DateTime();
            return attributeValue;
        }

        public bool GetBoolAttributeValue(Entity entity, string attributeName)
        {
            var attributeValue = false;
            if (entity.Contains(attributeName) && (entity[attributeName] is bool && (bool)entity[attributeName]))
                attributeValue = (bool)entity[attributeName];
            return attributeValue;
        }

        public static EntityReference GetLookupAttributeValue(Entity entity, string attributeName)
        {
            if (entity == null) return null;
            EntityReference attributeValue = null;
            if (entity.Contains(attributeName) && (entity[attributeName] is EntityReference))
                attributeValue = (EntityReference)entity[attributeName];
            return attributeValue;
        }

        public Guid GetPrimaryKeyAttributeValue(Entity entity)
        {
            return entity != null ? entity.Id : new Guid();
        }

        public int GetOptionSetAttributeValue(Entity entity, string attributeName)
        {
            var attributeValue = 0;
            if (!entity.Contains(attributeName) || (!(entity[attributeName] is OptionSetValue))) return attributeValue;
            var optionSetValue = entity[attributeName] as OptionSetValue;
            attributeValue = optionSetValue.Value;
            return attributeValue;
        }

        #endregion

        #region Public Methods   
        public ExecuteMultipleRequest GetExeculteMultipleRequest()
        {
            var multipleExecuteRequest = new ExecuteMultipleRequest
            {
                Settings = new ExecuteMultipleSettings
                {
                    ContinueOnError = true,
                    ReturnResponses = true
                },
                Requests = new OrganizationRequestCollection()
            };

            return multipleExecuteRequest;
        }

        #endregion
    }
}
