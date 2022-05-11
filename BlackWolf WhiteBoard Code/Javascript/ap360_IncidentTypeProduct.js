function OnLoad(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    // FilterVendor(formContext);
    formContext.getControl("ap360_productfamily").setDisabled(true);

    FilterProductGGParent(formContext)
    FilterProductFamily(formContext);


}

function FilterVendor(formContext) {
    formContext.getControl("ap360_vendorid").addPreSearch(addAccountsPreSearchFilter);
    formContext.getControl("ap360_preferredsupplierid").addPreSearch(addAccountsPreSearchFilter);
}

function addAccountsPreSearchFilter() {
    var filter = "<filter type=\"and\">" +
        "<condition attribute=\"customertypecode\" operator=\"eq\"  value=\"11\" />" +
        "</filter>";
    //"<condition attribute=\"customertypecode\" operator=\"eq\"  value=\"" + productFamilyId + "\" />" +
    Xrm.Page.getControl("ap360_vendorid").addCustomFilter(filter);
    Xrm.Page.getControl("ap360_preferredsupplierid").addCustomFilter(filter);
}


function setUOMtoEach(executionContext) {
    var lookupData = new Array();
    var lookupItem = new Object();
    lookupItem.id = "361a3eac-749c-4bb3-92a2-d63f692f61ba"; // UOM Each Guid
    lookupItem.name = "Each"; // Entity record name
    lookupItem.entityType = "uom";
    lookupData[0] = lookupItem;
    Xrm.Page.getAttribute("msdyn_unit").setValue(lookupData);
}


function updateUnitPrice(executionContext)//On Change of Quantity
{
    var formContext = executionContext.getFormContext(); // get formContext

    var quantity = null;
    var unitcost = null;
    var multiplier = null;
    quantity = formContext.getAttribute("ap360_quantity").getValue();
    unitcost = formContext.getAttribute("ap360_unitcost").getValue();
    multiplier = formContext.getAttribute("ap360_multiplier").getValue();


    if (quantity != null && unitcost != null && multiplier != null) {

        formContext.getAttribute("ap360_unitprice").setValue(multiplier * unitcost * quantity);

        //formContext.getAttribute("ap360_partsaleprice").setValue( unitcost * multiplier);



    }
    else {
        formContext.getAttribute("ap360_unitprice").setValue(null);

    }
}

function CalculatePriceMarkup(executionContext)//ON Change of Unit Cost
{

    //ap360_quantity
    //ap360_unitcost
    //ap360_unitprice
    //ap360_partsaleprice
    var formContext = executionContext.getFormContext(); // get formContext

    // use formContext instead of Xrm.Page

    var quantity = null;
    var unitcost = null;
    quantity = formContext.getAttribute("ap360_quantity").getValue();
    unitcost = formContext.getAttribute("ap360_unitcost").getValue();

    if (quantity != null && unitcost != null) {

        var multiplier = null;
        multiplier = getPriceMarkUpMatrix(unitcost);
        if (multiplier != null) {
            formContext.getAttribute("ap360_multiplier").setValue(multiplier);

            formContext.getAttribute("ap360_unitprice").setValue(multiplier * unitcost * quantity);
            formContext.getAttribute("ap360_partsaleprice").setValue(unitcost * multiplier);

        }
        //alert(multiplier);

    }
    else {
        formContext.getAttribute("ap360_unitprice").setValue(null);
        formContext.getAttribute("ap360_multiplier").setValue(null);


        alert("Quantity and UnitCost must be filled")
    }
    if (unitcost == null) {
        formContext.getAttribute("ap360_multiplier").setValue(null);

    }

}



function getPriceMarkUpMatrix(unitcost) {



    var multiplier = null;
    var purchaseOrderId = null;
    var isPurhcaseOrderExists = false;

    var req = new XMLHttpRequest();
    var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ap360_pricemarkups?$select=ap360_from,ap360_maxprice,ap360_multiplier,ap360_searchable,ap360_to";
    req.open("GET", url, false);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");

    req.send();


    if (req.status === 200) {

        var results = JSON.parse(req.response);
        for (var i = 0; i < results.value.length; i++) {
            var ap360_from = results.value[i]["ap360_from"];
            var ap360_from_formatted = results.value[i]["ap360_from@OData.Community.Display.V1.FormattedValue"];
            var ap360_maxprice = results.value[i]["ap360_maxprice"];
            var ap360_maxprice_formatted = results.value[i]["ap360_maxprice@OData.Community.Display.V1.FormattedValue"];
            var ap360_multiplier = results.value[i]["ap360_multiplier"];
            var ap360_multiplier_formatted = results.value[i]["ap360_multiplier@OData.Community.Display.V1.FormattedValue"];
            var ap360_searchable = results.value[i]["ap360_searchable"];
            var ap360_searchable_formatted = results.value[i]["ap360_searchable@OData.Community.Display.V1.FormattedValue"];
            var ap360_to = results.value[i]["ap360_to"];
            var ap360_to_formatted = results.value[i]["ap360_to@OData.Community.Display.V1.FormattedValue"];


            if ((unitcost >= ap360_from) && unitcost <= ap360_to) {
                multiplier = ap360_multiplier;
                return multiplier;
            }
        }


    } else {
        Xrm.Utility.alertDialog(req.statusText);
    }

    return multiplier;
}

function checkIsvendorIndetified(executionContext) {
    var formContext = executionContext.getFormContext(); // get formContext

    var IsVendorIdentified = false;
    if (formContext.getAttribute("ap360_vendoridentified") != null) {
        IsVendorIdentified = formContext.getAttribute("ap360_vendoridentified").getValue();
        if (IsVendorIdentified)
            formContext.getControl("ap360_vendorid").setVisible(true);
        else
            formContext.getControl("ap360_vendorid").setVisible(false);

    }
}

////////////////////////////////////////////////////////////////////////////////////////////

////ap360_parentservicetaskid

function FilterProductFamily(formContext) {

    formContext.getControl("ap360_productfamily").addPreSearch(addProductFamilyPreSearchFilter);

}

function addProductFamilyPreSearchFilter() {
    debugger;

    var productFamilyId = null;

    var GGParentLookup = Xrm.Page.data.entity.attributes.get("ap360_ggparent").getValue();
    var GParentLookup = Xrm.Page.data.entity.attributes.get("ap360_gparent").getValue();
    var ParentLookup = Xrm.Page.data.entity.attributes.get("ap360_parent").getValue();

    if (GGParentLookup != null)
        productFamilyId = GGParentLookup[0].id;
    if (GParentLookup != null)
        productFamilyId = GParentLookup[0].id;
    if (ParentLookup != null)
        productFamilyId = ParentLookup[0].id;

    var filter = "<filter type=\"and\">" +
        "<condition attribute=\"parentproductid\" operator=\"eq\"  value=\"" + productFamilyId + "\" />" +
        "</filter>";

    Xrm.Page.getControl("ap360_productfamily").addCustomFilter(filter);
}


function FilterProductGGParent(formContext) {

    debugger;
    formContext.getControl("ap360_ggparent").addPreSearch(addParentProductPreSearchFilter);
}

function addParentProductPreSearchFilter() {
    debugger;
    var parentServiceOperation = Xrm.Page.data.entity.attributes.get("ap360_parentservicetaskid").getValue();
    debugger;
    if (parentServiceOperation != null) {
        serviceOperationName = parentServiceOperation[0].name;

        serviceOperationName = serviceOperationName.trim();

        //serviceOperationName = serviceOperationName + "%";

        var filter = "<filter type=\"and\">" +
            "<condition attribute=\"parentproductidname\" operator=\"eq\"  value=\"" + serviceOperationName + "\" />" +
            "</filter>";

        Xrm.Page.getControl("ap360_ggparent").addCustomFilter(filter);
    }
}

function SetProductFamily(executionContext) {
    debugger;

    var formContext = executionContext.getFormContext();

    var GGParentLookup = formContext.getAttribute("ap360_ggparent").getValue();
    var GParentLookup = formContext.getAttribute("ap360_gparent").getValue();
    var ParentLookup = formContext.getAttribute("ap360_parent").getValue();
    var ChildLookup = formContext.getAttribute("ap360_child").getValue();

    var productFamilyControl = formContext.getAttribute("ap360_productfamily");

    if (GGParentLookup != null) {
        productFamilyControl.setValue(GGParentLookup);
    } else {
        formContext.getAttribute("ap360_gparent").setValue(null);
        formContext.getAttribute("ap360_parent").setValue(null);
        formContext.getAttribute("ap360_child").setValue(null);
    }
    if (GParentLookup != null) {
        productFamilyControl.setValue(GParentLookup);
    } else {
        formContext.getAttribute("ap360_parent").setValue(null);
        formContext.getAttribute("ap360_child").setValue(null);
    }
    if (ParentLookup != null) {
        productFamilyControl.setValue(ParentLookup);
    } else {
        formContext.getAttribute("ap360_child").setValue(null);
    }
    if (ChildLookup != null) {
        productFamilyControl.setValue(ChildLookup);
    }
    PrepareSoTemplateName(executionContext);
}

function PrepareSoTemplateName(executionContext) {

    debugger;

    var serviceOperation = "";
    var description = null;
    var productFamily = "";

    var formContext = executionContext.getFormContext();
    var serviceOperationLookup = formContext.getAttribute("ap360_parentservicetaskid").getValue();
    var productFamilyLookup = formContext.getAttribute("ap360_productfamily").getValue();


    if (serviceOperationLookup != null)
        serviceOperation = serviceOperationLookup[0].name;

    if (productFamilyLookup != null)
        productFamily = productFamilyLookup[0].name;


    var descriptionValue = formContext.getAttribute("ap360_productdescription").getValue();//Description

    if (descriptionValue != null)
        description = descriptionValue;
    var name = null;
    if (description == null) {
        name = productFamily;
    } else {
        name = description;
    }

    if (name.length > 100)
        name = name.substring(0, 100);

    formContext.getAttribute("ap360_name").setValue(name);//Name
}

function filterProductPreSearchFilter(executionContext) {

    var formContext = executionContext.getFormContext();
    var ggparent = formContext.getAttribute("ap360_ggparent").getValue();
    var ggparent = formContext.getAttribute("ap360_gparent").getValue();
    var ggparent = formContext.getAttribute("ap360_parent").getValue();
    var ggparent = formContext.getAttribute("ap360_child").getValue();

    if (ggparent != null) { }

}

function FilterProduct(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();

    formContext.getControl("msdyn_product").addPreSearch(addProductPreSearchFilter);

}

function addProductPreSearchFilter() {
    debugger;

    var productFamilyId = null;

    var ChildLookup = Xrm.Page.data.entity.attributes.get("ap360_child").getValue();
    if (ChildLookup != null) {
        productFamilyId = ChildLookup[0].id;
        filter(productFamilyId);
        return;
    }
    var ParentLookup = Xrm.Page.data.entity.attributes.get("ap360_parent").getValue();
    if (ParentLookup != null) {
        productFamilyId = ParentLookup[0].id;
        filter(productFamilyId);
        return;
    }
    var GParentLookup = Xrm.Page.data.entity.attributes.get("ap360_gparent").getValue();
    if (GParentLookup != null) {
        productFamilyId = GParentLookup[0].id;
        filter(productFamilyId);
        return;
    }
    var GGParentLookup = Xrm.Page.data.entity.attributes.get("ap360_ggparent").getValue();
    if (GGParentLookup != null) {
        productFamilyId = GGParentLookup[0].id;
        filter(productFamilyId);
        return;
    }
    filter(null);

}

function filter(productFamilyId) {
    var filter = "<filter type=\"and\">" +
        "<condition attribute=\"parentproductid\" operator=\"eq\"  value=\"" + productFamilyId + "\" />" +
        "</filter>";
    Xrm.Page.getControl("msdyn_product").addCustomFilter(filter);
}


function IsProductApproved(executionContext) {
    var formContext = executionContext.getFormContext();
    var isProductApproved = false;
    isProductApproved = formContext.getAttribute("ap360_approveproduct").getValue();
    if (isProductApproved) {
        //Xrm.Page.getControl("ap360_partno").setVisible(true);
        //Xrm.Page.getControl("ap360_productdescription").setVisible(true);


        formContext.getAttribute("ap360_partnumber").setRequiredLevel("required");
        formContext.getAttribute("ap360_productdescription").setRequiredLevel("required");

    }
    else {
        //Xrm.Page.getControl("ap360_partno").setVisible(false);
        //Xrm.Page.getControl("ap360_productdescription").setVisible(false);
        formContext.getAttribute("ap360_partnumber").setRequiredLevel("none");
        formContext.getAttribute("ap360_productdescription").setRequiredLevel("none");


    }
}
