using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code.Model
{
    public class Subject
    {
        public Guid CreateSubject(IOrganizationService service,ITracingService tracing, string productName, string subjectTree = null)
        {
            tracing.Trace("Before Product Create");
            Entity subject = new Entity("subject");
            subject["title"] = productName;
            if (subjectTree != null)
            {
                subject["parentsubject"] = new EntityReference("subject", new Guid(subjectTree));
            }
            Guid subjectGuid = service.Create(subject);
            tracing.Trace("After Product Created "+ subjectGuid.ToString());

            return subjectGuid;
        }

        public void updateSubject(IOrganizationService service,ITracingService tracing, string productName,string parentSubjectTreeId, string subjectTree)
        {
            tracing.Trace("before Subject Update");
            Entity subject = new Entity("subject");

            tracing.Trace("Subject Tree Should contain Data and subject tree id is "+parentSubjectTreeId.ToString());
            subject["parentsubject"] = new EntityReference("subject", new Guid(parentSubjectTreeId));

            subject["title"] = productName;
            subject.Id = new Guid(subjectTree);
          
            service.Update(subject);
            tracing.Trace("After Subject Update");

        }

        public void DeleteSubject(IOrganizationService service,ITracingService tracing, string subjectTreeGuid)
        {
            tracing.Trace("Before Product Delete");
            service.Delete("subject", Guid.Parse(subjectTreeGuid));
            tracing.Trace("After Product Delete");

        }
    }
}
