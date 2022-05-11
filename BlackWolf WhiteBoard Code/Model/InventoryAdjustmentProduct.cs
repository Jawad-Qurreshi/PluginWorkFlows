using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code.Model
{
    public class InventoryAdjustmentProduct
    {

        public static void createInventoryAdjustmentProduct(IOrganizationService service, EntityReference product, EntityReference unit, double quantity, Guid inventoryAdjustmentId)
        {



            // Create Inventory Adjustment Product
            Entity newInventoryAdjustProduct = new Entity("msdyn_inventoryadjustmentproduct");
            if (product != null)
            {
                newInventoryAdjustProduct["msdyn_product"] = new EntityReference(product.LogicalName, product.Id);

            }
            if (unit != null)
            {
                newInventoryAdjustProduct["msdyn_unit"] = new EntityReference(unit.LogicalName, unit.Id);
            }

            newInventoryAdjustProduct["msdyn_quantity"] = quantity;

            newInventoryAdjustProduct["msdyn_inventoryadjustment"] = new EntityReference("msdyn_inventoryadjustment", inventoryAdjustmentId);
            Guid newInventoryAdjustProductId = service.Create(newInventoryAdjustProduct);
        }
    }
}
