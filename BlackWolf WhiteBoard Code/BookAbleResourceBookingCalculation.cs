using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public class BookAbleResourceBookingCalculation : IPlugin
    {
        #region Private Variables

        private IOrganizationService _service;

        #endregion

        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {

             //   throw new InvalidPluginExecutionException("BookAbleResourceBookingCalculation");

                var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                var factory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                _service = factory.CreateOrganizationService(context.UserId);
                ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                var entityFactory = EntityFactory.GetEntity(context.PrimaryEntityName);
                if (entityFactory == null) return;
                entityFactory.Execute(context, _service, tracingService);
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }

    }
}
