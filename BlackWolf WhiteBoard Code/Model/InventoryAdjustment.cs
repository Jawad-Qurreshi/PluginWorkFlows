using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code.Model
{
    public class InventoryAdjustment
    {

        public static Guid createInventoryAdjustment(IOrganizationService service, EntityReference wareHouse)

        {

            Entity newInventoryAdjust = new Entity("msdyn_inventoryadjustment");
            if (wareHouse != null)
            {
                newInventoryAdjust["msdyn_warehouse"] = new EntityReference(wareHouse.LogicalName,wareHouse.Id);
            }
            newInventoryAdjust["msdyn_adjustmenttime"] = DateTime.Today;
            Guid newInventoryAdjustId = service.Create(newInventoryAdjust);
            return newInventoryAdjustId;
        }

        public static void CreateInventoryAdjustmentForReturnWOProducts(IOrganizationService service, Entity workOrderProduct)
        {

            // Create Inventory Adjustment
            Entity newInventoryAdjust = new Entity("msdyn_inventoryadjustment");
            if (workOrderProduct.Contains("msdyn_warehouse"))
            {
                newInventoryAdjust["msdyn_warehouse"] = workOrderProduct.GetAttributeValue<EntityReference>("msdyn_warehouse");
            }
            newInventoryAdjust["msdyn_adjustmenttime"] = DateTime.Today;
            Guid newInventoryAdjustId = service.Create(newInventoryAdjust);

            // Create Inventory Adjustment Product
            Entity newInventoryAdjustProduct = new Entity("msdyn_inventoryadjustmentproduct");
            if (workOrderProduct.Contains("ap360_product"))
            {
                newInventoryAdjustProduct["msdyn_product"] = workOrderProduct.GetAttributeValue<EntityReference>("ap360_product"); ;
                // String productUrl = "https://blackwolfwhiteboard.crm.dynamics.com/main.aspx?appid=ab2029aa-0f66-ea11-a811-000d3a33f47e&newWindow=true&pagetype=entityrecord&etn=product&id=" + workOrderProduct.GetAttributeValue<EntityReference>("ap360_product").Id.ToString();

            }
            if (workOrderProduct.Contains("msdyn_unit"))
            {
                newInventoryAdjustProduct["msdyn_unit"] = workOrderProduct.GetAttributeValue<EntityReference>("msdyn_unit");
            }
            if (workOrderProduct.Contains("ap360_quantityreturn"))
            {
                double quantity = workOrderProduct.GetAttributeValue<double>("ap360_quantityreturn");

                newInventoryAdjustProduct["msdyn_quantity"] = quantity;
            }
            newInventoryAdjustProduct["msdyn_inventoryadjustment"] = new EntityReference("msdyn_inventoryadjustment", newInventoryAdjustId);
           service.Create(newInventoryAdjustProduct);
        }
    }
}
