using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code.Model
{
   public class VendorQuote
    {

        public Guid guid { get; set; }
        public EntityReference Vendor { get; set; }
        public Money Cost { get; set; }
        public DateTime EstimatedDeliveryDate { get; set; }
    }
}
