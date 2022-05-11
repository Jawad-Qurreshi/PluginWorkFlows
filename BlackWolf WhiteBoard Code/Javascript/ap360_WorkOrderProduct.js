var previousWOPStatus = null;
function OnLoad(executionContext) {
    // debugger;
    var formContext = executionContext.getFormContext();
    //  mapvalue(executionContext);
    FilterVendor(formContext);
    FilterProduct(executionContext);
    FilterWOST(formContext);
    if (formContext.ui.getFormType() != 1)//create
    {
        var IsVendorIdentified = formContext.getAttribute("ap360_vendoridentified").getValue();
        if (IsVendorIdentified) {
            formContext.getControl("ap360_vendorid").setVisible(true);
        }

        var IsCore = formContext.getAttribute("ap360_core").getValue();
        if (IsCore) {
            formContext.getControl("ap360_amount").setVisible(true);
        }

        var ap360_isrevised = formContext.getAttribute("ap360_isrevised").getValue();
        if (ap360_isrevised == false) {
            formContext.ui.controls.forEach(function (control, index) {
                var controlType = control.getControlType();
                if (controlType != "iframe" && controlType != "webresource" && controlType != "subgrid") {
                    control.setDisabled(true);
                    // formContext.getControl("msdyn_allocated").setDisabled(false);
                    formContext.getControl("msdyn_linestatus").setDisabled(false);
                    formContext.getControl("msdyn_warehouse").setDisabled(false);
                    formContext.getControl("ap360_workorderproductstatus").setDisabled(false);
                    formContext.getControl("ap360_vendoridentified").setDisabled(false);
                    formContext.getControl("ap360_core").setDisabled(false);
                    formContext.getControl("ap360_amount").setDisabled(false);
                    formContext.getControl("ap360_vendorid").setDisabled(false);
                    formContext.getControl("ap360_row").setDisabled(false);
                    formContext.getControl("ap360_bin").setDisabled(false);
                    formContext.getControl("ap360_adminfollowup").setDisabled(false);
                    formContext.getControl("ap360_followupdescription").setDisabled(false);
                    formContext.getControl("ap360_workorderservicetaskid").setDisabled(false);
                    IsUserHasDispatcherRole(executionContext);

                }
            });
        }

        var msdyn_linestatus = formContext.getAttribute("msdyn_linestatus").getValue();
        var ap360_workorderproducttype = formContext.getAttribute("ap360_workorderproducttype").getValue();
        if (msdyn_linestatus == 690970001 && ap360_workorderproducttype != 126300001)//Used && BlackWolfPart
        {
            var returnpartsectionTab = formContext.ui.tabs.get("returnpartsection"); //Get Tab
            returnpartsectionTab.setVisible(true); //Show Section
            formContext.getControl("ap360_reasontoreturn").setDisabled(false);
            formContext.getControl("ap360_quantityreturn").setDisabled(false);
            formContext.getAttribute("ap360_quantityreturn").setValue(null);
            formContext.getAttribute("ap360_reasontoreturn").setValue(null);
            formContext.getControl("ap360_demagedbyid").setDisabled(false);
            //formContext.getControl("ap360_isreturntoblackwolf").setDisabled(false);
        }

        if (formContext.getAttribute("ap360_productfamily")) {
            var ap360_productfamily = formContext.getAttribute("ap360_productfamily").getValue();
            var formtype = formContext.ui.getFormType();
            if (formtype == 1) {
                if (ap360_productfamily != null) {
                    ap360_productfamily = ap360_productfamily[0].name;
                    formContext.getAttribute("msdyn_name").setValue(ap360_productfamily);
                    formContext.getAttribute("ap360_name").setValue(ap360_productfamily);
                } else {
                    formContext.getAttribute("msdyn_name").setValue(null);
                    formContext.getAttribute("ap360_name").setValue(null);
                }
            }
        }

        var ap360_iscustomerpart = formContext.getAttribute("ap360_iscustomerpart").getValue();
        if (ap360_iscustomerpart == true) {
            formContext.ui.controls.forEach(function (control, index) {
                var controlType = control.getControlType();
                if (controlType != "iframe" && controlType != "webresource" && controlType != "subgrid") {
                    control.setDisabled(true);
                    formContext.getControl("msdyn_linestatus").setDisabled(false);
                    formContext.getControl("ap360_core").setDisabled(false);

                    formContext.getControl("msdyn_allocated").setDisabled(false);
                    formContext.getControl("ap360_workorderproductstatus").setDisabled(false);

                    formContext.getControl("ap360_adminfollowup").setDisabled(false);
                    formContext.getControl("ap360_followupdescription").setDisabled(false);

                    formContext.getControl("ap360_workorderservicetaskid").setDisabled(false);

                }
            });
        }

    }
    if (formContext.ui.getFormType() == 1)//create
    {
        formContext.getControl("ap360_workorderproducttype").setDisabled(false);
        SetProductFamily(executionContext);

        updateWOPStatus_OnChangeOfWOPType(executionContext);
    }

    makeFormReadOnlyForClosedPostedWorkOrder(executionContext);

    //handles changes to the BPF before they actually occur
    // process.addOnPreStageChange(myProcessStateOnPreChange);
    makeStageFieldsReadOnly(executionContext);
    var process = formContext.data.process;
    process.addOnPreStageChange(function (formContext) {
        myProcessStateOnPreChange(formContext);
    });

}

//Process Stage Exceptions
function myProcessStateOnPreChange(executionContext) {

    var formContext = executionContext.getFormContext();
    var process = formContext.data.process;
    var eventArgs = executionContext.getEventArgs();
    var stageName = process.getSelectedStage().getName().toString().toLowerCase();

    if (stageName == "vendor") {
        var vendor = formContext.getAttribute("ap360_vendorid").getValue();
        if (vendor == null) {
            if (eventArgs._direction != 1) //Which means it is moving forward backwards
            {
                eventArgs.preventDefault();
                Xrm.Utility.alertDialog("Please Select vendor or contact Admin team");
                return;
            }

        }
    }
    if (stageName == "approval") {
        var approval = formContext.getAttribute("ap360_reviseditemstatus").getValue();
        if (approval != 126300001)//Approved
        {
            if (eventArgs._direction != 1) //backwards
            {
                eventArgs.preventDefault();
                Xrm.Utility.alertDialog("Work Order Product is not Approved. Contact Admin team");
                return;
            }

        }
    }
    if (stageName == "purchase order") {
        var purchaseorder = formContext.getAttribute("ap360_purchaseorderid").getValue();
        if (purchaseorder == null) {

            if (eventArgs._direction != 1) //backwards
            {
                eventArgs.preventDefault();
                Xrm.Utility.alertDialog("Purchase order not created. Contact Admin team");
                return;
            }
        }
    }
    if (stageName == "product status") {
        var WOPStatus = formContext.getAttribute("ap360_workorderproductstatus").getValue();
        if (WOPStatus != 126300001 && WOPStatus != 126300007)//Received &&& Used
        {
            if (eventArgs._direction != 1) //Which means it is moving forward backwards
            {
                eventArgs.preventDefault();
                Xrm.Utility.alertDialog("Wait until Product is received.");
                return;
            }

        }
    }
}



function IsUserHasDispatcherRole(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();

    // GUID of role to check
    var dispatcherRoleId = "B9884913-9D8B-EA11-A812-000D3A33F47E".toLowerCase();// BW Dispatcher Role
    var adminRoleId = "6DF69374-E44F-EA11-A814-000D3A30FCFF".toLowerCase()//Administrator
    // Get all the roles of the Logged in User.
    var currentUserRoles = Xrm.Utility.getGlobalContext().userSettings.securityRoles;
    for (var i = 0; i < currentUserRoles.length; i++) {
        var userRoleId = currentUserRoles[i];
        if (userRoleId.toLowerCase() == dispatcherRoleId || userRoleId.toLowerCase() == adminRoleId) {
            // Return true if the Role matches
            formContext.getControl("msdyn_estimateunitamount").setDisabled(false);

            return true;
        }
    }
    return false;
}
function FilterVendor(formContext) {
    formContext.getControl("ap360_quotevendorid").addPreSearch(addAccountsPreSearchFilter);

    formContext.getControl("ap360_vendorid").addPreSearch(addAccountsPreSearchFilter);
    //Xrm.Page.getControl("ap360_quotevendorid").addCustomFilter(filter);

    formContext.getControl("ap360_preferredsupplierid").addPreSearch(addAccountsPreSearchFilter);
}
function FilterWOST(formContext) {
    formContext.getControl("ap360_workorderservicetaskid").addPreSearch(addWorkOrdersWOSTFilter);

}
function addWorkOrdersWOSTFilter() {
    var workOrder = Xrm.Page.getAttribute("msdyn_workorder").getValue();
    if (workOrder == null) return;
    workOrder = workOrder[0].id.replace('{', '').replace('}', '');

    var filter = "<filter type=\"and\">" +
        "<condition attribute=\"msdyn_workorder\" operator=\"eq\"  value=\"" + workOrder + "\" />" +
        "</filter>";
    //"<condition attribute=\"customertypecode\" operator=\"eq\"  value=\"" + productFamilyId + "\" />" +
    Xrm.Page.getControl("ap360_workorderservicetaskid").addCustomFilter(filter);

}
function addAccountsPreSearchFilter() {
    var filter = "<filter type=\"and\">" +
        "<condition attribute=\"customertypecode\" operator=\"eq\"  value=\"11\" />" +
        "</filter>";
    //"<condition attribute=\"customertypecode\" operator=\"eq\"  value=\"" + productFamilyId + "\" />" +
    Xrm.Page.getControl("ap360_vendorid").addCustomFilter(filter);
    Xrm.Page.getControl("ap360_preferredsupplierid").addCustomFilter(filter);
    Xrm.Page.getControl("ap360_quotevendorid").addCustomFilter(filter);

}
async function createProductOnSave(executionContext) {
    debugger;
    console.log("*****************************createProductOnSave");

    var formContext = executionContext.getFormContext();
    Xrm.Utility.showProgressIndicator("Please wait...")

    var timer;
    if (formContext.ui.getFormType() == 1) {
        timer = 7000;
    } else {
        timer = 2000;
    }
    var approveProduct = formContext.getAttribute("ap360_approveproduct").getValue();
    if (approveProduct) {
        setTimeout(function () {
            var results = null;
            var parameters = {};
            var entityGUID = formContext.data.entity.getId();
            parameters.entityGUID = entityGUID.replace('{', '').replace('}', '');
            parameters.entityName = formContext.data.entity.getEntityName();
            var urlPassByPerameter = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ap360_CreateProductFromDescription";
            results = callWebAPIWithParams(urlPassByPerameter, false, parameters)
            if (results != null) {
                console.log("*********************************************Result is not null");
                debugger;
                if (results.IsProductPotentialMatched == true) {
                    console.log("*******************************************Proudct matched");
                    console.log("*********************************" + results.Message);


                    Xrm.Utility.alertDialog((results.Message).toString());
                    Xrm.Utility.closeProgressIndicator();
                    FilterPotentialMatchedProduct(formContext, results.lstPotentialMatchedProduct)
                }
                else if (results.IsProductPotentialMatched == false && results.Message == "Not Processed") {
                    createProductOnSave(executionContext);
                    console.log("*******************************************Not processed");
                    console.log("*********************************" + results.Message);


                }
                else {
                    console.log("*******************************************Normal case");
                    console.log("*********************************" + results.Message);

                    var responseMessage = results.Message;
                    const resonseMassageArr = responseMessage.split("#");
                    var guid = resonseMassageArr[1];
                    formContext.data.refresh();
                    Xrm.Utility.closeProgressIndicator();
                    timer = 2000;
                    adjustInventoryForCustomerPart(executionContext, guid, timer);
                }
            }
        }, timer);
    } else {
        console.log("*****************************************Error occured");

        var guid = null;
        adjustInventoryForCustomerPart(executionContext, guid, timer);
        Xrm.Utility.closeProgressIndicator()
    }
}
function ToggleAprovPrdctDescAndBypasDup_OnLoad(executionContext) {
    var formContext = executionContext.getFormContext(); // get formContext
    var IsProductSelelcted = null;
    if (formContext.getAttribute("ap360_product") != null) {
        IsProductSelelcted = formContext.getAttribute("ap360_product").getValue();
        if (IsProductSelelcted != null) {
            formContext.getControl("ap360_approveproduct").setVisible(false);
            formContext.getAttribute("ap360_approveproduct").setValue(false);
            formContext.getControl("ap360_bypassduplicatedetection").setVisible(false);
            formContext.getAttribute("ap360_bypassduplicatedetection").setValue(false);
            formContext.getControl("ap360_productdescription").setVisible(false);
            formContext.getAttribute("ap360_productdescription").setValue(null);
        }
        else {
            formContext.getControl("ap360_approveproduct").setVisible(true);
            formContext.getControl("ap360_bypassduplicatedetection").setVisible(true);
            formContext.getControl("ap360_productdescription").setVisible(true);
        }
    }
}
async function adjustInventoryForCustomerPart(executionContext, guid, timer) {
    debugger;
    var formContext = executionContext.getFormContext();


    if (guid != null) {

        //   alert(guid + "this was");
        var WOPType = formContext.getAttribute("ap360_workorderproducttype").getValue();
        if (WOPType == 126300000)//Customer supplied
            callInventoryPluginForInventoryAdjustment(executionContext);

    } else {

        if (formContext.ui.getFormType() == 1) //create
        {
            setTimeout(function () {
                var WOPType = formContext.getAttribute("ap360_workorderproducttype").getValue();
                var product = formContext.getAttribute("ap360_product").getValue();
                if (product != null && WOPType == 126300000)//Customer supplied
                    callInventoryPluginForInventoryAdjustment(executionContext);
            }, timer);
        }
        else if (formContext.ui.getFormType() == 2) //Update
        {
            var isProductDirty = formContext.getAttribute("ap360_product").getIsDirty();
            var isWorkOrderProductTypeDirty = formContext.getAttribute("ap360_workorderproducttype").getIsDirty();
            setTimeout(function () {
                if (isProductDirty || isWorkOrderProductTypeDirty) {
                    var product = formContext.getAttribute("ap360_product").getValue();
                    var WOPType = formContext.getAttribute("ap360_workorderproducttype").getValue();
                    if (product != null && WOPType == 126300000)//Customer supplied  
                        callInventoryPluginForInventoryAdjustment(executionContext);
                }
            }, timer);
        }

    }
}
function callInventoryPluginForInventoryAdjustment(executionContext) {

    var result = null;
    var formContext = executionContext.getFormContext();
    var parameters = {};
    var entityGUID = formContext.data.entity.getId();
    if (entityGUID == "") {
        Xrm.Utility.alertDialog("Entity Guid is null");
        return;
    }
    parameters.entityGUID = entityGUID.replace('{', '').replace('}', '');
    parameters.entityName = formContext.data.entity.getEntityName();

    var urlPassByPerameter = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ap360_AdjustInventoryForCustomerSuppliedParts";
    result = callWebAPIWithParams(urlPassByPerameter, false, parameters)
    if (result != null) {
        formContext.data.refresh();
        Xrm.Utility.closeProgressIndicator()
    }
}
var lstPotentialMatchedProductIds = null;
function FilterPotentialMatchedProduct(formContext, lstPotentialMatchedProduct) {

    var lookupData = new Array();
    var lookupItem = new Object();
    lookupItem.id = "62a07066-e3ed-e911-a811-000d3a579c6d" //Vehicle ID;
    lookupItem.name = "Vehicle"; // Entity record name
    lookupItem.entityType = "product";
    lookupData[0] = lookupItem;

    formContext.getAttribute("ap360_ggparent").setValue(lookupData);
    formContext.getAttribute("ap360_productfamily").setValue(lookupData);

    formContext.getAttribute("ap360_gparent").setValue(null);
    formContext.getAttribute("ap360_parent").setValue(null);
    formContext.getAttribute("ap360_child").setValue(null);

    //var ggParent = formContext.getAttribute("ap360_ggparent").getValue();
    //formContext.getAttribute("ap360_productfamily").setValue(ggParent);

    lstPotentialMatchedProductIds = lstPotentialMatchedProduct.split(',');

    var filterPotentailMatchedProductIds = "";
    for (var i = 0; i < lstPotentialMatchedProductIds.length; i++) {
        filterPotentailMatchedProductIds += "<condition attribute=\"productid\" operator=\"eq\"  value=\"" + lstPotentialMatchedProductIds[i] + "\" />";
    }

    var filter = "<filter type =\"or\" >" +
        filterPotentailMatchedProductIds +
        "</filter>"


    formContext.getControl("ap360_product").addPreSearch(addPotentialMatchedProductFilter);
}
function addPotentialMatchedProductFilter() {



    var filterPotentailMatchedProductIds = "";
    for (var i = 0; i < lstPotentialMatchedProductIds.length; i++) {
        filterPotentailMatchedProductIds += "<condition attribute=\"productid\" operator=\"eq\"  value=\"" + lstPotentialMatchedProductIds[i] + "\" />";
    }

    var filter = "<filter type =\"or\" >" +
        filterPotentailMatchedProductIds +
        "</filter>"

    //"<condition attribute=\"customertypecode\" operator=\"eq\"  value=\"" + productFamilyId + "\" />" +
    // "<condition attribute=\"ap360_partnumber\" operator=\"like\" value=\"%" + partNumber + "%\" />" +
    Xrm.Page.getControl("ap360_product").addCustomFilter(filter);

}
function setUOMandPriceList(executionContext) {
    var lookupData = new Array();
    var lookupItem = new Object();
    lookupItem.id = "361a3eac-749c-4bb3-92a2-d63f692f61ba"; // UOM Each Guid
    lookupItem.name = "Each"; // Entity record name
    lookupItem.entityType = "uom";
    lookupData[0] = lookupItem;
    Xrm.Page.getAttribute("msdyn_unit").setValue(lookupData);


    var lookupData = new Array();
    var lookupItem = new Object();
    lookupItem.id = "4fe16dd5-8e55-ea11-a811-000d3a33f3c3"; // Black Wolf Price list
    lookupItem.name = "Black Wolf Price List"; // Entity record name
    lookupItem.entityType = "pricelevel";
    lookupData[0] = lookupItem;
    Xrm.Page.getAttribute("msdyn_pricelist").setValue(lookupData);
}
function checkIsvendorIndetified(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext(); // get formContext

    var IsVendorIdentified = false;
    if (formContext.getAttribute("ap360_vendoridentified") != null) {
        IsVendorIdentified = formContext.getAttribute("ap360_vendoridentified").getValue();
        if (IsVendorIdentified) {
            formContext.getControl("ap360_vendorid").setVisible(true);
            //formContext.getControl("process_header_ap360_vendorid").setVisible(true);
            // formContext.getControl("ap360_approveproduct").setVisible(true);
        } else {
            formContext.getAttribute("ap360_vendorid").setValue(null);
            formContext.getControl("ap360_vendorid").setVisible(false);
            // formContext.getControl("process_header_ap360_vendorid").setVisible(false);
            checkVendor_OnChangeofVendor(executionContext);
            // formContext.getControl("ap360_approveproduct").setVisible(false);
        }

    }
}
var selelctedWorkOrderProductsGuid;
var ap360_vendorid = null;
///////////////////////////////Purchase Order Creation functions
function createPurchaseOrder(selectedItems) {
    debugger;
    selelctedWorkOrderProductsGuid = "";
    var count = 1;
    Xrm.Utility.showProgressIndicator("Please wait, Purchase Order(s) are creating ...");
    var WOPVerified = true;
    WOPVerified = verificationBeforePurchaseOrderCreation(selectedItems);

    if (!WOPVerified) {
        Xrm.Utility.closeProgressIndicator()
        return;
    } else {

        for (var i = 0; i < selectedItems.length; i++) {

            selelctedWorkOrderProductsGuid += selectedItems[i].Id;
            if (count > 1 || count < selectedItems.length)
                selelctedWorkOrderProductsGuid += ",";
            count++;
        }
        if (selectedItems.length > 1)// if only one then no need to remove last character as in case of One record last character is Guid char  
            selelctedWorkOrderProductsGuid = selelctedWorkOrderProductsGuid.substring(0, selelctedWorkOrderProductsGuid.length - 1);


        //DisplaySelectedAccount(null);
        callWebAPIForCreatePO();
    }


}
function verificationBeforePurchaseOrderCreation(selectedItems) {
    debugger;
    var WOPVerified = true;
    var listOfVendors = [];
    for (var i = 0; i < selectedItems.length; i++) {
        var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/msdyn_workorderproducts(" + selectedItems[i].Id + ")?$select=ap360_reviseditemstatus,_ap360_product_value,_ap360_purchaseorderid_value,ap360_workorderproducttype,ap360_name,ap360_vendoridentified,_ap360_vendorid_value";
        var result = callWebAPIWithoutParams(url, false);
        if (result != null) {
            debugger;
            var ap360_name = result["ap360_name"];
            var ap360_vendoridentified = result["ap360_vendoridentified"];
            ap360_vendorid = result["_ap360_vendorid_value"];
            var ap360_product = result["_ap360_product_value"];
            var _ap360_purchaseorderid_value = result["_ap360_purchaseorderid_value"];
            var ap360_workorderproducttype = result["ap360_workorderproducttype"];
            var ap360_reviseditemstatus = result["ap360_reviseditemstatus"];

            if (ap360_workorderproducttype == 126300001) {
                WOPVerified = false;
                Xrm.Utility.alertDialog("Purchase order cannot be created for BlackWolf part.");
            } else if (ap360_reviseditemstatus != 126300001) {
                WOPVerified = false;
                Xrm.Utility.alertDialog("Work Order Product " + ap360_name + " Not Approved.");
            } else if (ap360_product == null) {
                WOPVerified = false;
                Xrm.Utility.alertDialog("Product doesnot exist in work order product " + ap360_name + ". Please identify the Product!");
            } else if (_ap360_purchaseorderid_value != null) {
                WOPVerified = false;
                Xrm.Utility.alertDialog("Purchase order for Work Order Product " + ap360_name + " already exist!");
            } else if (ap360_vendorid == null) {
                WOPVerified = false;
                Xrm.Utility.alertDialog(" Work Order Product (" + ap360_name + ")  vendor is not identified. Please identify the vendor!");
            }
            else {
                listOfVendors.push(ap360_vendorid);
            }
            for (var j = 0; j < listOfVendors.length; j++) {
                if (ap360_vendorid != listOfVendors[j]) {
                    Xrm.Utility.alertDialog("One or more selected WO Products have different vendor.  Selected WO Products should have same Vendor");
                    isVendorExists = false;
                }
            }
        }
    }
    return WOPVerified;
}
function callWebAPIForCreatePO() {
    var selectedAccountGuid = ap360_vendorid;

    var parameters = {};
    var preferredsupplierid = {};
    preferredsupplierid.accountid = selectedAccountGuid; //Delete if creating new record 
    preferredsupplierid["@odata.type"] = "Microsoft.Dynamics.CRM.account";
    parameters.preferredSupplierId = preferredsupplierid;
    parameters.selelctedWorkOrderProductsGuid = selelctedWorkOrderProductsGuid;

    var results = null;
    var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ap360_CreatePurchaseOrderForPrefferedSupplier";
    results = callWebAPIWithParams(url, false, parameters);
    if (results != null) {
        var lookupOptions = {};
        lookupOptions.entityType = "msdyn_workorderproduct";
        Xrm.Utility.refreshParentGrid(lookupOptions);
        Xrm.Utility.closeProgressIndicator();
    } else {
        Xrm.Utility.closeProgressIndicator();
    }

}
////////////////////////////////End of Purchase Order Creation functions
function updateExtendedPartPrice(executionContext)//On Change of Quantity
{
    var formContext = executionContext.getFormContext(); // get formContext
    var workOrderProductType = formContext.getAttribute("ap360_workorderproducttype").getValue();

    //if (isCustomerPart == true) {
    //Customer Supplied  126,300,000
    //BlackWolf Inventory  126,300,001
    //Vendor Supplied  126,300,002
    if (workOrderProductType == 126300000)//  Customer Supplied
    {
        return;// for customer supplied price doesn't matter
    }
    var quantity = null;
    var unitcost = null;
    var multiplier = null;
    quantity = formContext.getAttribute("msdyn_estimatequantity").getValue();
    unitcost = formContext.getAttribute("msdyn_estimateunitcost").getValue();
    multiplier = formContext.getAttribute("ap360_multiplier").getValue();

    var quantityreturn = formContext.getAttribute("ap360_quantityreturn").getValue();
    if (quantityreturn != null) {
        formContext.getAttribute("ap360_quantityremaining").setValue(quantity - quantityreturn);
    }
    else {
        formContext.getAttribute("ap360_quantityremaining").setValue(quantity);
    }

    if (quantity != null && unitcost != null && multiplier != null) {
        formContext.getAttribute("msdyn_estimateunitamount").setValue(multiplier * unitcost);

        formContext.getAttribute("msdyn_estimatesubtotal").setValue(multiplier * unitcost * quantity);

        formContext.getAttribute("ap360_revisedestimateamount").setValue(multiplier * unitcost * quantity);
        checkVendor_OnChangeofVendor(executionContext);
    }
    else {
        formContext.getAttribute("msdyn_estimatesubtotal").setValue(null);
        formContext.getAttribute("msdyn_estimateunitamount").setValue(null);
        formContext.getAttribute("ap360_revisedestimateamount").setValue(null);


    }
}
function getPriceMarkUpMatrix(unitcost) {
    var multiplier = null;
    var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ap360_pricemarkups?$select=ap360_from,ap360_maxprice,ap360_multiplier,ap360_searchable,ap360_to";
    var results = callWebAPIWithoutParams(url, false);
    if (results != null) {
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
    }
    return multiplier;
}
function updateUnitPrice(executionContext)//On Change of Quantity
{
    // debugger;
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
    //debugger;
    var formContext = executionContext.getFormContext(); // get formContext
    unitcost = formContext.getAttribute("msdyn_estimateunitcost").getValue();
    if (unitcost != null) {
        var multiplier = null;
        multiplier = getPriceMarkUpMatrix(unitcost);
        if (multiplier != null)
            formContext.getAttribute("ap360_multiplier").setValue(multiplier);
    }
    else
        formContext.getAttribute("ap360_multiplier").setValue(null);
}
function FilterProductFamily(formContext) {
    formContext.getControl("ap360_productfamily").addPreSearch(addProductFamilyPreSearchFilter);
}
function addProductFamilyPreSearchFilter() {
    // debugger;

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
    formContext.getControl("header_process_ap360_ggparent").addPreSearch(addHeaderParentProductPreSearchFilter);
}
function addParentProductPreSearchFilter() {
    // debugger;
    var parentServiceOperation = Xrm.Page.data.entity.attributes.get("ap360_parentservicetaskid").getValue();
    debugger;
    if (parentServiceOperation != null) {
        serviceOperationName = parentServiceOperation[0].name;
        serviceOperationName = serviceOperationName.trim();
        var filter = "<filter type=\"and\">" +
            "<condition attribute=\"parentproductidname\" operator=\"eq\"  value=\"" + serviceOperationName + "\" />" +
            "</filter>";

        Xrm.Page.getControl("ap360_ggparent").addCustomFilter(filter);
        //Xrm.Page.getControl("header_process_ap360_ggparent").addCustomFilter(filter);
    }
}

function addHeaderParentProductPreSearchFilter() {
    // debugger;
    var parentServiceOperation = Xrm.Page.data.entity.attributes.get("ap360_parentservicetaskid").getValue();
    debugger;
    if (parentServiceOperation != null) {
        serviceOperationName = parentServiceOperation[0].name;
        serviceOperationName = serviceOperationName.trim();
        var filter = "<filter type=\"and\">" +
            "<condition attribute=\"parentproductidname\" operator=\"eq\"  value=\"" + serviceOperationName + "\" />" +
            "</filter>";

        Xrm.Page.getControl("ap360_ggparent").addCustomFilter(filter);
        Xrm.Page.getControl("header_process_ap360_ggparent").addCustomFilter(filter);
    }
}
function ToggleAprovPrdctDescAndBypasDup_OnChngOfPrdctPrtNum(executionContext) {
    var formContext = executionContext.getFormContext(); // get formContext
    var IsProductSelelcted = null;
    if (formContext.getAttribute("ap360_product") != null) {
        IsProductSelelcted = formContext.getAttribute("ap360_product").getValue();
        if (IsProductSelelcted != null) {
            formContext.getControl("ap360_approveproduct").setVisible(false);
            formContext.getAttribute("ap360_approveproduct").setValue(false);
            formContext.getControl("ap360_bypassduplicatedetection").setVisible(false);
            formContext.getAttribute("ap360_bypassduplicatedetection").setValue(false);
            formContext.getControl("ap360_productdescription").setVisible(false);
            formContext.getAttribute("ap360_productdescription").setValue(null);
        }
        else {
            formContext.getControl("ap360_approveproduct").setVisible(true);
            formContext.getControl("ap360_bypassduplicatedetection").setVisible(true);
            formContext.getControl("ap360_productdescription").setVisible(true);
            formContext.getAttribute("ap360_approveproduct").setValue(true);
        }

    }
}
function SetProductFamily(executionContext) {
    //debugger;

    var formContext = executionContext.getFormContext();

    var GGParentLookup = formContext.getAttribute("ap360_ggparent").getValue();
    var GParentLookup = formContext.getAttribute("ap360_gparent").getValue();
    var ParentLookup = formContext.getAttribute("ap360_parent").getValue();
    var ChildLookup = formContext.getAttribute("ap360_child").getValue();

    var productFamilyControl = formContext.getAttribute("ap360_productfamily");
    productFamilyControl.setValue(null);

    if (GGParentLookup != null) {
        productFamilyControl.setValue(GGParentLookup);
    }
    if (GParentLookup != null) {
        productFamilyControl.setValue(GParentLookup);
    }
    if (ParentLookup != null) {
        productFamilyControl.setValue(ParentLookup);
    }
    if (ChildLookup != null) {
        productFamilyControl.setValue(ChildLookup);
    }
    PrepareSoTemplateName(executionContext);
}
function GGParentOnChange_SetProductFamily(executionContext) {
    // debugger;

    var formContext = executionContext.getFormContext();
    var GGParentLookup = formContext.getAttribute("ap360_ggparent").getValue();

    var productFamilyControl = formContext.getAttribute("ap360_productfamily");
    //debugger;
    if (GGParentLookup != null) {
        //debugger;
        productFamilyControl.setValue(GGParentLookup);
    } else {
        formContext.getAttribute("ap360_gparent").setValue(null);
        formContext.getAttribute("ap360_parent").setValue(null);
        formContext.getAttribute("ap360_child").setValue(null);
    }
    PrepareSoTemplateName(executionContext);
}
function GParentOnChange_SetProductFamily(executionContext) {
    // debugger;

    var formContext = executionContext.getFormContext();

    var GParentLookup = formContext.getAttribute("ap360_gparent").getValue();

    var productFamilyControl = formContext.getAttribute("ap360_productfamily");

    if (GParentLookup != null) {
        //debugger;
        productFamilyControl.setValue(GParentLookup);
    } else {
        formContext.getAttribute("ap360_parent").setValue(null);
        formContext.getAttribute("ap360_child").setValue(null);
    }
    PrepareSoTemplateName(executionContext);
}
function ParentOnChange_SetProductFamily(executionContext) {
    // debugger;

    var formContext = executionContext.getFormContext();


    var ParentLookup = formContext.getAttribute("ap360_parent").getValue();

    var productFamilyControl = formContext.getAttribute("ap360_productfamily");

    if (ParentLookup != null) {
        //debugger;
        productFamilyControl.setValue(ParentLookup);
    } else {
        formContext.getAttribute("ap360_child").setValue(null);
    }

    PrepareSoTemplateName(executionContext);
}
function ChildOnChange_SetProductFamily(executionContext) {
    debugger;

    var formContext = executionContext.getFormContext();


    var ChildLookup = formContext.getAttribute("ap360_child").getValue();

    var productFamilyControl = formContext.getAttribute("ap360_productfamily");
    if (ChildLookup != null) {
        //debugger;
        productFamilyControl.setValue(ChildLookup);
    }
    //debugger;
    PrepareSoTemplateName(executionContext);
}
function PrepareSoTemplateName(executionContext) {

    debugger;
    var format = /[`!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?~]/;

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


    //if (descriptionValue != null) {

    //    var isCorrectformat = format.test(descriptionValue);
    //    if (!isCorrectformat) {
    //        formContext.getControl("ap360_productdescription").clearNotification();
    description = descriptionValue;
    //    }
    //    else {
    //        formContext.getControl("ap360_productdescription").setNotification('Special Characters are not allowed for Product description');
    //        return;
    //    }
    //}
    var name = null;
    if (description == null) {
        name = productFamily;
    } else {
        name = description;
    }

    if (name.length > 100)
        name = name.substring(0, 100);

    formContext.getAttribute("ap360_name").setValue(name);//Name
    formContext.getAttribute("msdyn_name").setValue(name);
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

    formContext.getControl("ap360_product").addPreSearch(addProductPreSearchFilter);

}
function addProductPreSearchFilter() {
    // debugger;
    var productFamilyId = null;
    var ProductFamilyLookup = Xrm.Page.data.entity.attributes.get("ap360_productfamily").getValue();
    if (ProductFamilyLookup != null) {
        productFamilyId = ProductFamilyLookup[0].id;
        filter(productFamilyId);
        return;
    }
}
function filter(productFamilyId) {
    //debugger;
    var filter = "<filter type=\"and\">" +
        "<condition attribute=\"productid\" operator=\"under\"  value=\"" + productFamilyId + "\" />" +
        "<condition attribute=\"productstructure\" operator=\"eq\" value=\"1\" />" +
        "</filter>";
    Xrm.Page.getControl("ap360_product").addCustomFilter(filter);
}
function IsProductApproved(executionContext) {
    var formContext = executionContext.getFormContext();
    var isProductApproved = false;
    isProductApproved = formContext.getAttribute("ap360_approveproduct").getValue();
    if (isProductApproved) {
        //Xrm.Page.getControl("ap360_partnumber").setVisible(true);
        //Xrm.Page.getControl("ap360_productdescription").setVisible(true);
        var productdescription = formContext.getAttribute("ap360_productdescription").getValue();
        if (productdescription == null || productdescription == "") {
            var name = formContext.getAttribute("ap360_name").getValue();
            formContext.getAttribute("ap360_productdescription").setValue(name);
        }



        formContext.getAttribute("ap360_partnumber").setRequiredLevel("required");
        // formContext.getAttribute("ap360_productdescription").setRequiredLevel("required");

    }
    else {
        //Xrm.Page.getControl("ap360_partnumber").setVisible(false);
        //Xrm.Page.getControl("ap360_productdescription").setVisible(false);
        formContext.getAttribute("ap360_partnumber").setRequiredLevel("none");
        formContext.getAttribute("ap360_productdescription").setValue(null);

        // formContext.getAttribute("ap360_productdescription").setRequiredLevel("none");


    }
}
function SetPartNumber_onchangeProduct(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var ap360_product = formContext.getAttribute("ap360_product").getValue();
    var tyreBatteryMarkup = formContext.getAttribute("ap360_tyrebatterymarkup").getValue();
    //debugger;
    if (ap360_product != null) {
        var ap360_name = ap360_product[0].name;
        ap360_product = ap360_product[0].id.replace('{', '').replace('}', '');
        var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/products(" + ap360_product + ")?$select=ap360_partnumber,currentcost,_ap360_productfamily_value&$filter=statecode eq 0";//Status (statecode ) eq Active(0) not status Reason
        var result = callWebAPIWithoutParams(url, false);
        if (result != null) {
            //debugger;
            formContext.getAttribute("msdyn_name").setValue(ap360_name);
            formContext.getAttribute("ap360_name").setValue(ap360_name);



            if (result["_ap360_productfamily_value"] != null) {
                var productFamilyId = result["_ap360_productfamily_value"];
                var productFamilyName = result["_ap360_productfamily_value@OData.Community.Display.V1.FormattedValue"];
                var lookupValue = new Array();
                lookupValue[0] = new Object();
                lookupValue[0].id = productFamilyId; // GUID of the lookup id
                lookupValue[0].name = productFamilyName; // Name of the lookup
                lookupValue[0].entityType = "product"; //Entity Type of the lookup entity
                formContext.getAttribute("ap360_child").setValue(lookupValue);
            }
        }
        if (result["ap360_partnumber"] != null) {
            var ap360_partnumber = result["ap360_partnumber"];
            formContext.getAttribute("ap360_partnumber").setValue(ap360_partnumber);
        }
        else {
            var alertStrings = { confirmButtonLabel: "Close", text: "Product does not have Part Number.", title: "Product" };
            var alertOptions = { height: 120, width: 260 };
            Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                function success(result) {
                    console.log("Alert dialog closed");
                },
                function (error) {
                    console.log(error.message);
                }
            );
        }
        if (result["currentcost"] != null) {
            var currentcost = result["currentcost"];
            formContext.getAttribute("msdyn_estimateunitcost").setValue(currentcost);
            if (tyreBatteryMarkup == false) {
                CalculatePriceMarkup(executionContext);
            }
            else {
                setTiersBatteryMarkUp(executionContext);
            }
            updateExtendedPartPrice(executionContext);
        }
        else {
            Xrm.Utility.alertDialog(this.statusText);
        }
    }
    else {
        formContext.getAttribute("ap360_partnumber").setValue(null);
        formContext.getAttribute("msdyn_estimateunitcost").setValue(null);
        formContext.getAttribute("ap360_child").setValue(null);
        updateExtendedPartPrice(executionContext);
    }
}
function SetProduct_onchangePartNumber(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var ap360_partnumber = formContext.getAttribute("ap360_partnumber").getValue();
    var tyreBatteryMarkup = formContext.getAttribute("ap360_tyrebatterymarkup").getValue();
    debugger;
    if (ap360_partnumber != null) {
        var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/products?$select=currentcost,name,_ap360_productfamily_value&$filter=statecode eq 0 and ap360_partnumber eq '" + ap360_partnumber + "'";//Status (statecode ) eq Active(0) not status Reason
        var results = callWebAPIWithoutParams(url, false);
        if (results != null) {
            if (results.value.length > 0) {
                for (var i = 0; i < results.value.length; i++) {
                    var currentcost = results.value[i]["currentcost"];
                    var productFamilyId = results.value[i]["_ap360_productfamily_value"];
                    var productFamilyName = results.value[i]["_ap360_productfamily_value@OData.Community.Display.V1.FormattedValue"];
                    var currentcost_formatted = results.value[i]["currentcostt@OData.Community.Display.V1.FormattedValue"];
                    var name = results.value[i]["name"];
                    var productid = results.value[i]["productid"];
                    var lookupValue = new Array();
                    lookupValue[0] = new Object();
                    lookupValue[0].id = productid; // GUID of the lookup id
                    lookupValue[0].name = name; // Name of the lookup
                    lookupValue[0].entityType = "product"; //Entity Type of the lookup entity
                    if (results.value.length == 1) {
                        formContext.getAttribute("ap360_product").setValue(lookupValue);
                    }
                    else {
                        Xrm.Utility.alertDialog("Multiple products exist with Part Number " + ap360_partnumber + ". Try use Product field");
                        return;
                    }
                    if (currentcost != null) {
                        formContext.getAttribute("msdyn_estimateunitcost").setValue(currentcost);
                        if (tyreBatteryMarkup == false) {
                            CalculatePriceMarkup(executionContext);
                        }
                        else {
                            setTiersBatteryMarkUp(executionContext);
                        }
                        updateExtendedPartPrice(executionContext);
                    }
                    if (productFamilyId != null) {
                        var lookupValue = new Array();
                        lookupValue[0] = new Object();
                        lookupValue[0].id = productFamilyId; // GUID of the lookup id
                        lookupValue[0].name = productFamilyName; // Name of the lookup
                        lookupValue[0].entityType = "product"; //Entity Type of the lookup entity
                        formContext.getAttribute("ap360_child").setValue(lookupValue);
                    } else {
                        formContext.getAttribute("ap360_child").setValue(null);
                    }
                }
            }
        }
    } else {
        formContext.getAttribute("ap360_product").setValue(null);
        formContext.getAttribute("msdyn_estimateunitcost").setValue(null);
        formContext.getAttribute("ap360_child").setValue(null);
        updateExtendedPartPrice(executionContext);
    }
}
function checkVendor_OnChangeofVendor(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var isrevised = false;
    isrevised = formContext.getAttribute("ap360_isrevised").getValue();

    if (isrevised) {

        var ap360_vendorid = formContext.getAttribute("ap360_vendorid").getValue();
        if (ap360_vendorid != null) {

            ap360_vendorid = ap360_vendorid[0].id.replace('{', '').replace('}', '');

            if (ap360_vendorid.toLowerCase() == "a89044d7-9b9a-ea11-a811-000d3a33f3c3")//Black Wolf Inventory
            {

                var revisedestimateamount = formContext.getAttribute("ap360_revisedestimateamount").getValue();
                if (revisedestimateamount != null) {
                    formContext.getAttribute("ap360_actualamount").setValue(revisedestimateamount);

                }

            }
            else {
                //formContext.getAttribute("ap360_actualamount").setValue(null);
            }


        }
        else {
            // formContext.getAttribute("ap360_actualamount").setValue(null);
        }

    }
}
function checkPurchaseOrderStatus_onSave(executionContext) {
    console.log("*************************************checkPurchaseOrderStatus_onSave");
    var formContext = executionContext.getFormContext();
    debugger;
    var isLineStatusdirty = formContext.getAttribute("msdyn_linestatus").getIsDirty();
    if (isLineStatusdirty == true) {
        // a89044d7-9b9a-ea11-a811-000d3a33f3c3
        var workOrderProductType = formContext.getAttribute("ap360_workorderproducttype").getValue();
        //Customer Supplied  126,300,000
        //BlackWolf Inventory  126,300,001
        //Vendor Supplied  126,300,002
        if (workOrderProductType != null) {
            if (workOrderProductType == 126300000 || workOrderProductType == 126300001) {
                formContext.getAttribute("ap360_workorderproductstatus").setValue(126300007);//Used

                return;
            }
        }
        var ap360_vendorid = formContext.getAttribute("ap360_vendorid").getValue();
        if (ap360_vendorid != null) {
            ap360_vendorid = ap360_vendorid[0].id.replace('{', '').replace('}', '');
            var ap360_iscustomerpart = formContext.getAttribute("ap360_iscustomerpart").getValue();

            if (ap360_vendorid.toLowerCase() == "a89044d7-9b9a-ea11-a811-000d3a33f3c3")//Black Wolf Inventory
            {
                formContext.getAttribute("ap360_workorderproductstatus").setValue(126300007);//Used
                return;
            }
            if (ap360_iscustomerpart) {
                formContext.getAttribute("ap360_workorderproductstatus").setValue(126300007);//Used
                return;
            }

            //msdyn_linestatus
            //Not Used	126300000
            //Estimated	690970000
            //Used	690970001
            var linestatus = formContext.getAttribute("msdyn_linestatus").getValue();
            if (linestatus == 690970001)//Used	690970001
            {
                var purchaseorder = formContext.getAttribute("ap360_purchaseorderid").getValue();
                var purchaseorderProduct = formContext.getAttribute("ap360_purchaseorderproductid").getValue();
                if (purchaseorderProduct == null) {
                    var ap360_product = formContext.getAttribute("ap360_product").getValue();
                    if (ap360_product != null) {
                        Xrm.Utility.alertDialog(ap360_product[0].name + " is not Ordered");
                        formContext.getAttribute("msdyn_linestatus").setValue(690970000);    //Estimated	
                        formContext.getAttribute("ap360_cancelledon").setValue(null);
                        formContext.getAttribute("ap360_cancelledbyid").setValue(null);
                        formContext.getAttribute("ap360_releasedon").setValue(null);
                        formContext.getAttribute("ap360_releasedtoid").setValue(null);
                        formContext.getAttribute("ap360_releasedtoid").setRequiredLevel("none");
                        formContext.data.entity.save();
                    }
                }
                else {

                    var woProductId = formContext.data.entity.getId();
                    POProductId = purchaseorderProduct[0].id.replace('{', '').replace('}', '');
                    var purchaseOrderProductItemSubStatus = getPurchaseOrderProductStatus(POProductId);
                    debugger;
                    if (purchaseOrderProductItemSubStatus != 126300000)//Received
                    {
                        Xrm.Utility.alertDialog("Work Order Product can't  be used, corresponding Purchase Order Part is not Received");
                        var saveEvent = executionContext.getEventArgs();
                        saveEvent.preventDefault();
                    }
                    else if (purchaseOrderProductItemSubStatus == 126300000)//Received
                    {
                        formContext.getAttribute("ap360_workorderproductstatus").setValue(126300007);//Used
                    }
                }
            }
        }
    }
}
function setTiersBatteryMarkUp(executionContext) {

    var tyreBatteryMultiplier = 1.25;
    var formContext = executionContext.getFormContext();
    var tyrebatterymarkup = formContext.getAttribute("ap360_tyrebatterymarkup").getValue();
    if (tyrebatterymarkup == true) {
        var msdyn_estimateunitcost = formContext.getAttribute("msdyn_estimateunitcost").getValue();
        if (msdyn_estimateunitcost != null) {
            formContext.getAttribute("ap360_multiplier").setValue(tyreBatteryMultiplier);
            formContext.getAttribute("msdyn_estimateunitamount").setValue(msdyn_estimateunitcost * tyreBatteryMultiplier);
            var ap360_quantityremaining = formContext.getAttribute("ap360_quantityremaining").getValue();
            if (ap360_quantityremaining != null) {
                formContext.getAttribute("msdyn_estimatesubtotal").setValue(ap360_quantityremaining * msdyn_estimateunitcost * tyreBatteryMultiplier);
                formContext.getAttribute("ap360_revisedestimateamount").setValue(ap360_quantityremaining * msdyn_estimateunitcost * tyreBatteryMultiplier);
            }
        }
    }
    else {
        CalculatePriceMarkup(executionContext);
        updateExtendedPartPrice(executionContext);
    }
}
function getPurchaseOrderProductStatus(POProductId) {
    debugger;
    var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/msdyn_purchaseorderproducts?$select=ap360_itemsubstatus&$filter=msdyn_purchaseorderproductid eq " + POProductId;
    var results = callWebAPIWithoutParams(url, false);
    if (results != null) {
        for (var i = 0; i < results.value.length; i++) {
            var ap360_itemsubstatus = results.value[i]["ap360_itemsubstatus"];
            var ap360_itemsubstatus_formated = results.value[i]["ap360_itemsubstatus@OData.Community.Display.V1.FormattedValue"];

            if (ap360_itemsubstatus != null) {
                purchaseorderproductItemStatus = ap360_itemsubstatus;
            }
        }
    }
    return purchaseorderproductItemStatus;
}
function RemoveProductFamily_OnChangeOfRemoveProductFamilyHierarchy(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var removeproductfamilyhierarchy = formContext.getAttribute("ap360_removeproductfamilyhierarchy").getValue();
    if (removeproductfamilyhierarchy == 1) {
        var lookupData = new Array();
        var lookupItem = new Object();
        lookupItem.id = "62a07066-e3ed-e911-a811-000d3a579c6d"; // Vehicle Product
        lookupItem.name = "Vehicle"; // Entity record name
        lookupItem.entityType = "product";
        lookupData[0] = lookupItem;
        // formContext.getAttribute("ap360_serviceproductmappingid").setValue(lookupData);
        // formContext.getAttribute("ap360_productfamily").setValue(lookupData);
        formContext.getAttribute("ap360_parentservicetaskid").setValue(null);
        formContext.getAttribute("ap360_productfamily").setValue(null);
        formContext.getAttribute("ap360_serviceproductmappingid").setValue(null);


        var productdescription = formContext.getAttribute("ap360_productdescription").getValue();
        if (productdescription != null) {
            formContext.getAttribute("ap360_name").setValue(productdescription);
            formContext.getAttribute("msdyn_name").setValue(productdescription);
        }
        else {
            // formContext.getAttribute("ap360_name").setValue("Vehicle");
            //  formContext.getAttribute("msdyn_name").setValue("Vehicle");

        }


        //formContext.getAttribute("ap360_name").setValue("Vehicle");
        //formContext.getAttribute("msdyn_name").setValue("Vehicle");



        filter(lookupItem.id);
        formContext.getAttribute("ap360_ggparent").setRequiredLevel("required");
        formContext.getControl("ap360_ggparent").setVisible(true);
        formContext.getAttribute("ap360_ggparent").setValue(null);
        formContext.getAttribute("ap360_gparent").setValue(null);
        formContext.getAttribute("ap360_parent").setValue(null);
        formContext.getAttribute("ap360_child").setValue(null);
        formContext.getControl("ap360_removeproductfamilyhierarchy").setDisabled(true);




    }



}
function Hide_NewWOProduct_Ribbon(formContext, primaryentitytypename) {
    //debugger;
    if (primaryentitytypename == "opportunity") {
        return false;
    } else {
        return true;
    }
}
function getAccountPayment_OnSave(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var ap360_paymentstatus = formContext.getAttribute("ap360_paymentstatus").getValue();

    var woProductId = formContext.data.entity.getId();

    woProductId = woProductId.replace('{', '').replace('}', '');
    var ap360_product = formContext.getAttribute("ap360_product").getValue();
    if (ap360_product != null) {
        var workOrder = new Object();

        workOrder = getPayment(executionContext, "");

        if (workOrder.paymentUnappliedAmount != null) {

            var revisedestimateamount = formContext.getAttribute("ap360_revisedestimateamount").getValue();

            if (workOrder.paymentUnappliedAmount < revisedestimateamount) {

                if (ap360_paymentstatus == 126300001) {//paid
                    alert("Amount is not enough to Purchase " + ap360_product[0].name);
                    formContext.getAttribute("ap360_paymentstatus").setValue(126300000);//ready to paid
                    var isTaskexists = false;
                    isTaskexists = getRelatedTask(woProductId);
                    if (!isTaskexists) {
                        createTaskActivity(executionContext, woProductId, ap360_product[0].name, workOrder, revisedestimateamount)
                    }
                    executionContext.getEventArgs().preventDefault();

                }
            }
        }
    }



}
function adjustActualAmount_OnChangeofPartsReturn(executionContext) {
    debugger;
    var actualamount = 0;
    var quantityRemaining = 0;
    var formContext = executionContext.getFormContext();
    var quantityreturn = formContext.getAttribute("ap360_quantityreturn").getValue();
    if (quantityreturn != null) {

        if (quantityreturn > 0) {

            quantityRemaining = formContext.getAttribute("ap360_quantityremaining").getValue();

            //if (quantityreturn == estimatequantity) {
            //    alert("All parts can't be return using Quantity Return field, Use Return to Vendor Button on WorkOrder");
            //    return;
            //}
            //else
            if (quantityreturn > quantityRemaining) {
                Xrm.Utility.alertDialog("Quantity return can't be greater then  Quantity Remaining");
                return;
            }
            //else {
            //    actualamount = formContext.getAttribute("ap360_actualamount").getValue();

            //    if (actualamount > 0) {

            //        actualamount = actualamount / (estimatequantity - quantityreturn);

            //        formContext.getAttribute("ap360_actualamount").setValue(actualamount);

            //    }
            //}
        }
    }


}
function setProductInDescription_OnChangeProduct(executionContext) {
    var formContext = executionContext.getFormContext();
    var product = formContext.getAttribute("ap360_product").getValue();
    if (product != null) {
        formContext.getAttribute("ap360_productdescription").setValue(product[0].name);
    }
    else {
        formContext.getAttribute("ap360_productdescription").setValue(null);
    }
}
function onChange_QuantityReturn(executionContext) {
    var formContext = executionContext.getFormContext();
    var quantityreturn = formContext.getAttribute("ap360_quantityreturn").getValue();
    if (quantityreturn != null) {

        if (quantityreturn > 0) {
            formContext.getAttribute("ap360_reasontoreturn").setRequiredLevel("required");


        }
    }
    else {
        formContext.getAttribute("ap360_reasontoreturn").setRequiredLevel("none");

    }
}
function onChange_ReturnReason(executionContext) {
    var formContext = executionContext.getFormContext();
    var quantityreturn = formContext.getAttribute("ap360_reasontoreturn").getValue();
    if (quantityreturn != null) {

        if (quantityreturn == "126300000") {//Demaged By
            formContext.getAttribute("ap360_demagedbyid").setRequiredLevel("required");
        }
        else {
            formContext.getAttribute("ap360_demagedbyid").setValue(null);
            formContext.getAttribute("ap360_demagedbyid").setRequiredLevel("none");

        }
        if (quantityreturn == "126300003" || quantityreturn == "126300004")//Incorrect or Not Needed
        {
            formContext.getControl("ap360_isreturntoblackwolf").setDisabled(false);
            formContext.getControl("ap360_isreturntoblackwolf").setVisible(true);

        }
        else {
            formContext.getControl("ap360_isreturntoblackwolf").setDisabled(true);
            formContext.getControl("ap360_isreturntoblackwolf").setVisible(false);
            formContext.getAttribute("ap360_isreturntoblackwolf").setValue(false);

        }
    }

}
function onChange_DemagedBy(executionContext) {
    var formContext = executionContext.getFormContext();

}
function getPayment(executionContext, workOrderId) {
    var workOrder = new Object();
    var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/msdyn_workorders(b9702999-a7dc-ea11-a813-000d3a33f47e)?$select=_msdyn_billingaccount_value,_msdyn_serviceaccount_value&$expand=msdyn_billingaccount($select=ap360_paymentamount,ap360_paymentunappliedamount)";
    var result = callWebAPIWithoutParams(url, false); {
        var _msdyn_billingaccount_value = result["_msdyn_billingaccount_value"];
        workOrder.accountName = result["_msdyn_billingaccount_value@OData.Community.Display.V1.FormattedValue"];
        var _msdyn_billingaccount_value_lookuplogicalname = result["_msdyn_billingaccount_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
        var _msdyn_serviceaccount_value = result["_msdyn_serviceaccount_value"];
        var _msdyn_serviceaccount_value_formatted = result["_msdyn_serviceaccount_value@OData.Community.Display.V1.FormattedValue"];
        var _msdyn_serviceaccount_value_lookuplogicalname = result["_msdyn_serviceaccount_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
        if (result["msdyn_billingaccount"] != null) {
            var paymentAmount = result["msdyn_billingaccount"].ap360_paymentamount;
            workOrder.paymentUnappliedAmount = result["msdyn_billingaccount"].ap360_paymentunappliedamount;
        }
    }
    return workOrder;
}
function createTaskActivity(executionContext, woProductId, productName, workOrder, revisedestimateamount) {

    var entity = {};
    entity.subject = productName + " need approval";

    entity["regardingobjectid_msdyn_workorderproduct@odata.bind"] = "/msdyn_workorderproducts(" + woProductId + ")";
    entity.prioritycode = 2;

    entity.description = "Amount of " + productName + " is " + revisedestimateamount + ". " + workOrder.accountName + " contains only" + workOrder.paymentUnappliedAmount + " un applied";

    var req = new XMLHttpRequest();
    req.open("POST", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/tasks", true);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.onreadystatechange = function () {
        if (this.readyState === 4) {
            req.onreadystatechange = null;
            if (this.status === 204) {
                var uri = this.getResponseHeader("OData-EntityId");
                var regExp = /\(([^)]+)\)/;
                var matches = regExp.exec(uri);
                var newEntityId = matches[1];
            } else {
                Xrm.Utility.alertDialog(this.statusText);
            }
        }
    };
    req.send(JSON.stringify(entity));
}
function getPaymentStatus(executionContext, workOrderId) {
    var ap360_paymentstatus = null;
    var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/msdyn_workorderproducts(" + workOrderId + ")?$select=ap360_paymentstatus";
    var result = callWebAPIWithoutParams(url, false)
    if (result != null) {
        ap360_paymentstatus = result["ap360_paymentstatus"];
        var ap360_paymentstatus_formatted = result["ap360_paymentstatus@OData.Community.Display.V1.FormattedValue"];
    }
    return ap360_paymentstatus;
}
function getRelatedTask(woProductId) {
    var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/tasks?$select=_regardingobjectid_value&$filter=_regardingobjectid_value eq " + woProductId + "";
    var results = callWebAPIWithoutParams(url, false);
    if (results != null) {
        for (var i = 0; i < results.value.length; i++) {
            var _regardingobjectid_value = results.value[i]["_regardingobjectid_value"];
            var _regardingobjectid_value_formatted = results.value[i]["_regardingobjectid_value@OData.Community.Display.V1.FormattedValue"];
            var _regardingobjectid_value_lookuplogicalname = results.value[i]["_regardingobjectid_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
            isTaskExists = true;
        }
    }
    return isTaskExists;
}
function setBinAndRowOfProduct(executionContext) {
    var formContext = executionContext.getFormContext();
    var ap360_vendorid = formContext.getAttribute("ap360_vendorid").getValue();
    if (ap360_vendorid != null) {
        ap360_vendorid = ap360_vendorid[0].id.replace('{', '').replace('}', '');

        if (ap360_vendorid.toLowerCase() == "a89044d7-9b9a-ea11-a811-000d3a33f3c3")//Black Wolf Inventory
        {
            var ap360_product = formContext.getAttribute("ap360_product").getValue();
            if (ap360_product != null) {
                ap360_product = ap360_product[0].id.replace('{', '').replace('}', '');
                var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/msdyn_productinventories?$select=msdyn_bin,msdyn_row&$filter=_msdyn_product_value eq " + ap360_product;
                //  var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/msdyn_productinventories?$select=msdyn_bin,msdyn_row&$filter=_msdyn_product_value eq " + ap360_product + " and  _msdyn_unit_value ne null and  _msdyn_warehouse_value ne null";
                var result = callWebAPIWithoutParams(url, false)
                if (result != null) {
                    for (var i = 0; i < results.value.length; i++) {
                        var msdyn_bin = results.value[i]["msdyn_bin"];
                        var msdyn_row = results.value[i]["msdyn_row"];
                        formContext.getAttribute("ap360_bin").setValue(msdyn_bin);
                        formContext.getAttribute("ap360_row").setValue(msdyn_row);
                    }
                }
            }
            else {
                formContext.getAttribute("ap360_bin").setValue(null);
                formContext.getAttribute("ap360_row").setValue(null);
            }

        }
        else {
            formContext.getAttribute("ap360_bin").setValue(null);
            formContext.getAttribute("ap360_row").setValue(null);
        }
    }
    else {
        formContext.getAttribute("ap360_bin").setValue(null);
        formContext.getAttribute("ap360_row").setValue(null);
    }
}
function lockUnLockProductFamilyField(formContext, bool) {
    formContext.getControl("ap360_productfamily").setDisabled(bool);
    formContext.getControl("ap360_serviceproductmappingid").setDisabled(bool);
    formContext.getControl("ap360_ggparent").setDisabled(bool);
    formContext.getControl("ap360_gparent").setDisabled(bool);
    formContext.getControl("ap360_parent").setDisabled(bool);
    formContext.getControl("ap360_child").setDisabled(bool);
}
function UpdateProductFamily_OnChangeOfWorkOrderProductType(executionContext) {

    var formContext = executionContext.getFormContext();
    //    var isCustomerPart = formContext.getAttribute("ap360_iscustomerpart").getValue();
    var workOrderProductType = formContext.getAttribute("ap360_workorderproducttype").getValue();

    //if (isCustomerPart == true) {
    //Customer Supplied  126,300,000
    //BlackWolf Inventory  126,300,001
    //Vendor Supplied  126,300,002
    if (workOrderProductType == 126300000)//  Customer Supplied
    {
        var lookupData = new Array();
        var lookupItem = new Object();
        lookupItem.id = "62a07066-e3ed-e911-a811-000d3a579c6d"; // Vehicle Product
        lookupItem.name = "Vehicle"; // Entity record name
        lookupItem.entityType = "product";
        lookupData[0] = lookupItem;
        //formContext.getAttribute("ap360_serviceproductmappingid").setValue(lookupData);
        formContext.getAttribute("ap360_productfamily").setValue(lookupData);
        var lookupData = new Array();
        var lookupItem = new Object();
        lookupItem.id = "28b07b2b-4928-eb11-a813-000d3a368915";
        lookupItem.name = "Customer Supplied"; // Entity record name
        lookupItem.entityType = "account";
        lookupData[0] = lookupItem;
        formContext.getAttribute("ap360_vendorid").setValue(lookupData);
        formContext.getAttribute("ap360_vendoridentified").setValue(true);

        formContext.getAttribute("ap360_originalestimateamount").setValue(0);
        formContext.getAttribute("ap360_actualamount").setValue(0);
        formContext.getAttribute("ap360_revisedestimateamount").setValue(0);
        formContext.getControl("ap360_iscustomerpart").setDisabled(true);

        //formContext.getAttribute("ap360_vendorid").setValue(null);

        lockUnLockProductFamilyField(formContext, true);


        updateExtendedPartPrice(executionContext);

    }
}
function setBlackWolfVendor_OnChangeOfWorkOrderProductType(executionContext) {
    var formContext = executionContext.getFormContext();
    var workOrderProductType = formContext.getAttribute("ap360_workorderproducttype").getValue();

    //Customer Supplied  126,300,000
    if (workOrderProductType == 126300001)//  BlackWolf Inventory
    {
        formContext.getAttribute("ap360_vendoridentified").setValue(true);
        formContext.getControl("ap360_vendorid").setVisible(true);
        var lookupData = new Array();
        var lookupItem = new Object();
        lookupItem.id = "a89044d7-9b9a-ea11-a811-000d3a33f3c3"; // Blackwolf Part Id
        lookupItem.name = "Black Wolf Parts Account"; // Blackwolf Part
        lookupItem.entityType = "account";
        lookupData[0] = lookupItem;
        formContext.getAttribute("ap360_vendorid").setValue(lookupData);
        //Customer Supplied  126,300,000
        //BlackWolf Inventory  126,300,001
        //Vendor Supplied  126,300,002
        formContext.getAttribute("ap360_workorderproducttype").setValue(126300001);

        lockUnLockProductFamilyField(formContext, false);

        updateExtendedPartPrice(executionContext);
        SetProductFamily(executionContext);
    }
    updateWOPStatus_OnChangeOfWOPType(executionContext);
}
function setFieldsForVendorSupplied_OnChangeOfWorkOrderProductType(executionContext) {
    var formContext = executionContext.getFormContext();
    var workOrderProductType = formContext.getAttribute("ap360_workorderproducttype").getValue();

    //Customer Supplied  126,300,000
    //BlackWolf Inventory  126,300,001
    //Vendor Supplied  126,300,002
    if (workOrderProductType == 126300002)//  Vendor Supplied
    {
        updateExtendedPartPrice(executionContext);
        SetProductFamily(executionContext);
        formContext.getAttribute("ap360_vendoridentified").setValue(false);
        formContext.getAttribute("ap360_vendorid").setValue(null);

        lockUnLockProductFamilyField(formContext, false);

    }
}
function makeFormReadOnlyForClosedPostedWorkOrder(executionContext) {
    var formContext = executionContext.getFormContext();
    var msdyn_workorder = formContext.getAttribute("msdyn_workorder").getValue();
    if (msdyn_workorder != null) {
        msdyn_workorder = msdyn_workorder[0].id.replace('{', '').replace('}', '');
        var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/msdyn_workorders(" + msdyn_workorder + ")?$select=msdyn_systemstatus";
        var result = callWebAPIWithoutParams(url, false)
        if (result != null) {
            var msdyn_systemstatus = result["msdyn_systemstatus"];
            var msdyn_systemstatus_formatted = result["msdyn_systemstatus@OData.Community.Display.V1.FormattedValue"];

            if (msdyn_systemstatus == 690970004)//Closed - Posted(690970004)
            {
                var cs = formContext.ui.controls.get();
                for (var i in cs) {
                    var c = cs[i];
                    if (c.getName() != "" && c.getName() != null) {
                        if (!c.getDisabled()) { c.setDisabled(true); }
                    }
                }
            }
        }
    }
}
function SaveAndNewForm(primaryControl) {
    var formContext = primaryControl;
    formContext.data.entity.save();
    var entityOptions = {};
    entityOptions["entityName"] = "msdyn_workorderproduct";
    var formParameters = {};

    var removeproductfamilyhierarchy = formContext.getAttribute("ap360_removeproductfamilyhierarchy").getValue();
    formParameters["ap360_removeproductfamilyhierarchy"] = removeproductfamilyhierarchy;
    var serviceproductmapping = formContext.getAttribute("ap360_serviceproductmappingid").getValue();
    if (serviceproductmapping != null) {
        formParameters["ap360_serviceproductmappingid"] = serviceproductmapping;
    }
    var productfamily = formContext.getAttribute("ap360_productfamily").getValue();
    if (productfamily != null) {
        formParameters["ap360_productfamily"] = productfamily;
    }
    var ggparent = formContext.getAttribute("ap360_ggparent").getValue();
    if (ggparent != null) {
        formParameters["ap360_ggparent"] = ggparent;
    }
    var opportunity = formContext.getAttribute("ap360_opportunityid").getValue();
    if (opportunity != null) {
        formParameters["ap360_opportunityid"] = opportunity;
    }
    var warehouse = formContext.getAttribute("msdyn_warehouse").getValue();
    if (warehouse != null) {
        formParameters["msdyn_warehouse"] = warehouse;
    }
    var unit = formContext.getAttribute("msdyn_unit").getValue();
    if (unit != null) {
        formParameters["msdyn_unit"] = unit;
    }
    var pricelist = formContext.getAttribute("msdyn_pricelist").getValue();
    if (pricelist != null) {
        formParameters["msdyn_pricelist"] = pricelist;
    }
    var transactioncurrency = formContext.getAttribute("transactioncurrencyid").getValue();
    if (transactioncurrency != null) {
        formParameters["transactioncurrencyid"] = transactioncurrency;
    }

    var workorder = formContext.getAttribute("msdyn_workorder").getValue();
    if (workorder != null) {
        formParameters["msdyn_workorder"] = workorder;
        Xrm.Navigation.openForm(entityOptions, formParameters).then(
            function (lookup) { console.log("Success"); },
            function (error) { console.log("Error"); });
    }
    else {
        alert("Can not use 'Save & New' because Work Order is not selected");
    }



}
function getlookup(field) {

    var lookupData = new Array();
    var lookupItem = new Object();
    lookupItem.id = formContext.getAttribute(field).getValue()[0].id; // Vehicle Product
    lookupItem.name = formContext.getAttribute(field).getValue()[0].name;  // Entity record name
    lookupItem.entityType = "product";
    lookupData[0] = lookupItem;
    return lookupData;
}
function setBlackWolfVendor_OnChange_Product(executionContext) {
    var formContext = executionContext.getFormContext();
    var ap360_product = formContext.getAttribute("ap360_product").getValue();
    if (ap360_product != null) {
        ap360_product = ap360_product[0].id.replace('{', '').replace('}', '');
        var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/products(" + ap360_product + ")?$select=ap360_isblackwolfpart";
        var result = callWebAPIWithoutParams(url, false);
        if (result != null) {
            var ap360_isblackwolfpart = result["ap360_isblackwolfpart"];
            var ap360_isblackwolfpart_formatted = result["ap360_isblackwolfpart@OData.Community.Display.V1.FormattedValue"];
            if (ap360_isblackwolfpart == true) {
                formContext.getAttribute("ap360_vendoridentified").setValue(true);
                formContext.getControl("ap360_vendorid").setVisible(true);
                var lookupData = new Array();
                var lookupItem = new Object();
                lookupItem.id = "a89044d7-9b9a-ea11-a811-000d3a33f3c3"; // Blackwolf Part Id
                lookupItem.name = "Black Wolf Parts Account"; // Blackwolf Part
                lookupItem.entityType = "account";
                lookupData[0] = lookupItem;
                formContext.getAttribute("ap360_vendorid").setValue(lookupData);
            }
        }
    }
}
function AdminFollowUpClick(primaryControl)//Shop
{

    // Set booking Classification
    debugger;
    var formContext = primaryControl;
    var generalTab = formContext.ui.tabs.get("generaltab");
    var followup = generalTab.sections.get("followup"); //Get sections
    var globalContext = Xrm.Utility.getGlobalContext();
    followup.setVisible(true); //Show Section

    formContext.getControl("ap360_adminfollowup").setVisible(true);
    formContext.getAttribute("ap360_adminfollowup").setValue(true);

    formContext.getControl("ap360_followupdescription").setVisible(true);
    formContext.getAttribute("ap360_followupdescription").setRequiredLevel("required");

    formContext.getControl("ap360_followedupbyid").setVisible(true);
    var currentUser = new Array();
    currentUser[0] = new Object();
    currentUser[0].entityType = "systemuser";
    currentUser[0].id = globalContext.userSettings.userId;
    currentUser[0].name = globalContext.userSettings.userName;

    formContext.getAttribute("ap360_followedupbyid").setValue(currentUser);

}
function AdminUnFollowUpClick(primaryControl)//Shop
{
    debugger;
    var formContext = primaryControl;
    var generalTab = formContext.ui.tabs.get("generaltab");
    var followup = generalTab.sections.get("followup"); //Get sections

    formContext.getControl("ap360_adminfollowup").setVisible(false);
    formContext.getAttribute("ap360_adminfollowup").setValue(false);

    formContext.getControl("ap360_followupdescription").setVisible(false);
    formContext.getAttribute("ap360_followupdescription").setValue(null);
    formContext.getAttribute("ap360_followupdescription").setRequiredLevel("none");

    formContext.getControl("ap360_followedupbyid").setVisible(false);
    formContext.getAttribute("ap360_followedupbyid").setValue(null);

    followup.setVisible(false); //Hide Section
    formContext.data.entity.save();


}
function checkBlackWolfProductQuantityOnHand_OnChange_Quantity(executionContext) {
    var formContext = executionContext.getFormContext();
    var Vendor = formContext.getAttribute("ap360_vendorid").getValue();
    if (Vendor != null) {
        Vendor = Vendor[0].id.replace('{', '').replace('}', '');
        if (Vendor.toLowerCase() == "a89044d7-9b9a-ea11-a811-000d3a33f3c3")// Blackwolf Part Id
        {
            var ap360_product = formContext.getAttribute("ap360_product").getValue();
            if (ap360_product != null) {
                ap360_product = ap360_product[0].id.replace('{', '').replace('}', '');
                var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/msdyn_productinventories?$select=msdyn_qtyonhand&$filter=_msdyn_product_value eq " + ap360_product;
                var result = callWebAPIWithoutParams(url, false);
                if (result != null) {
                    var msdyn_qtyonhand = result.value[0]["msdyn_qtyonhand"];
                    var msdyn_qtyonhand_formatted = result.value[0]["msdyn_qtyonhand@OData.Community.Display.V1.FormattedValue"];
                    var partQuantity = formContext.getAttribute("msdyn_estimatequantity").getValue();
                    if (partQuantity > msdyn_qtyonhand) {
                        alert(" Quantity on Hand is less than the quantity ordered");
                    }
                }
            }
        }
    }
}
function setFollowedByUserLookupFieldToCurrentUser(executionContext) {
    var formContext = executionContext.getFormContext();
    var globalContext = Xrm.Utility.getGlobalContext();
    var currentUser = new Array();
    currentUser[0] = new Object();
    currentUser[0].entityType = "systemuser";
    currentUser[0].id = globalContext.userSettings.userId;
    currentUser[0].name = globalContext.userSettings.userName;

    formContext.getAttribute("ap360_FollowedUpById").setValue(currentUser);
    var adminFollowup = formContext.getAttribute("ap360_adminfollowup").getValue();
    if (adminFollowup == true) {
    }

}
////////////////////////////////////This section is for adding and removing WorkOrder Products to existing Purchase Order as POPs
function AddWOPTOExistingPurchaseOrder_OnClick_AddToExistingPO(primarycontrol, selectedItems) {
    debugger;
    try {
        var listOfWOPsAndPO = null;
        var workOrderProducts = "";


        var confirmStrings = { text: "Are you sure you want to Create PurchaseOrder Product(s).", title: "PurhaseOrder Product Creation" };
        var confirmOptions = { height: 200, width: 450 };
        Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
            function (success) {
                if (success.confirmed) {
                    listOfWOPsAndPO = getPurchaseOrderAndWorkOrderForWorkOrderAssociation(selectedItems);
                    if (listOfWOPsAndPO.length > 0) {

                        var wopWithPOMapped = listOfWOPsAndPO.filter(getWOPwithPurchaseOrderMapped);
                        var wopWithPONotMapped = listOfWOPsAndPO.filter(getWOPwithOUtPurchaseOrderMapped);

                        if (wopWithPOMapped.length == 1) {
                            var isPODraft = isPurchaseOrderDraft(wopWithPOMapped[0].POId);
                            if (!isPODraft) {
                                Xrm.Utility.alertDialog("PurchaseOrder Product can only be added in Draft Purchase Order");
                                return;
                            }
                        }
                        else {
                            Xrm.Utility.alertDialog("Select a WorkOrder Product where Purchase Order is mapped");
                            return;
                        }
                        if (wopWithPONotMapped.length <= 0) return;
                        for (var i = 0; i < wopWithPONotMapped.length; i++) {
                            workOrderProducts += wopWithPONotMapped[i].WOPId + ",";
                        }
                        CreatePOPOnExistingPOforWOP(wopWithPOMapped[0].POId, workOrderProducts);

                    } return;
                }
                else {
                    console.log("Dialog closed using Cancel button or X.");
                    return;
                }
            });



    }
    catch (exception) {
        throw exception;
    }


}
function CreatePOPOnExistingPOforWOP(purchaseOrderGuid, workOrderProductsID) {
    Xrm.Utility.showProgressIndicator("Please wait, Purchase Order Product(s) are creating ...");
    var results = null;
    var parameters = {};
    parameters.purchaseOrderGUID = purchaseOrderGuid;
    parameters.workOrderProductGuid = workOrderProductsID;
    var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ap360_CreatePOPOnExistingPOForWOP";
    results = callWebAPIWithParams(url, false, parameters);

    if (results != 200) {

        var lookupOptions = {};
        lookupOptions.entityType = "msdyn_workorderproduct";
        Xrm.Utility.refreshParentGrid(lookupOptions);
        Xrm.Utility.closeProgressIndicator();
    } else {
        Xrm.Utility.closeProgressIndicator();
    }

}
function getWOPwithPurchaseOrderMapped(listOfPurchaseOrders) {
    return listOfPurchaseOrders.POId != null;
}
function getWOPwithOUtPurchaseOrderMapped(listOfPurchaseOrders) {
    return listOfPurchaseOrders.POId == null;

}
function getPurchaseOrderAndWorkOrderForWorkOrderAssociation(selectedItems) {
    var purchaseOrderId = null;
    var listOfPurchaseOrders = [];
    var listVendorsInWorkOrderProducts = [];
    for (var i = 0; i < selectedItems.length; i++) {
        var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/msdyn_workorderproducts(" + selectedItems[i] + ")?$select=msdyn_name,_ap360_purchaseorderid_value,_ap360_vendorid_value,msdyn_workorderproductid";
        var result = callWebAPIWithoutParams(url, false);
        if (result != null) {
            var _ap360_purchaseorderid_value = result["_ap360_purchaseorderid_value"];
            var _ap360_vendorid_value = result["_ap360_vendorid_value"];
            var msdyn_name = result["msdyn_name"];
            var _ap360_purchaseorderid_value_formatted = result["_ap360_purchaseorderid_value@OData.Community.Display.V1.FormattedValue"];
            var _ap360_purchaseorderid_value_lookuplogicalname = result["_ap360_purchaseorderid_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
            var msdyn_workorderproductid = result["msdyn_workorderproductid"];
            var data = null;
            data = {
                POId: _ap360_purchaseorderid_value,
                WOPId: msdyn_workorderproductid
            }
            for (var i = 0; i < listOfPurchaseOrders.length; i++) {
                if (_ap360_purchaseorderid_value != null && listOfPurchaseOrders[i]["POId"] != null && _ap360_purchaseorderid_value != listOfPurchaseOrders[i]["POId"]) {
                    Xrm.Utility.alertDialog("Selected  WorkOrder Products have different Purhcase Orders, select WorkOrders with Unique Purchase Order");
                    purchaseOrderId = null;
                    return purchaseOrderId
                }
            }
            if (_ap360_vendorid_value == null) {
                Xrm.Utility.alertDialog("Select Vendor  in " + msdyn_name);
                return;
            }
            listVendorsInWorkOrderProducts.push(_ap360_vendorid_value);
            listOfPurchaseOrders.push(data);
        }
    }
    var isVendorUnique = isVendorUniqueInSelectedWOPs(listVendorsInWorkOrderProducts);
    if (!isVendorUnique) {
        Xrm.Utility.alertDialog("Vendor should be unique in selected Work Order Products");
        return;
    }
    return listOfPurchaseOrders;
}
function isVendorUniqueInSelectedWOPs(listVendorsInWorkOrderProducts) {

    var isVendorUnique = true;
    for (var j = 0; j < listVendorsInWorkOrderProducts.length; j++) {
        var selectedVendor = listVendorsInWorkOrderProducts[j];
        for (var i = 0; i < listVendorsInWorkOrderProducts.length; i++) {
            if (selectedVendor != listVendorsInWorkOrderProducts[i]) {
                isVendorUnique = false;
            }
        }
    }
    return isVendorUnique;
}
function isPurchaseOrderDraft(purchaseOrderId) {
    var isPODraft = false;
    var req = new XMLHttpRequest();
    var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/msdyn_purchaseorders(" + purchaseOrderId + ")?$select=msdyn_systemstatus";
    var result = callWebAPIWithoutParams(url, false);
    if (result != null) {
        var msdyn_systemstatus = result["msdyn_systemstatus"];
        if (msdyn_systemstatus == 690970000)//Draft
        {
            isPODraft = true;
        }
    }
    return isPODraft;
}
function createPurhaseOrderProduct(wopWithPONotMapped, wopWithPOMapped) {

    var entity = {};
    entity.msdyn_itemstatus = 690970000;
    entity.ap360_itemsubstatus = 126300002;
    entity["msdyn_product@odata.bind"] = "/products(5f8009f0-b70f-eb11-a813-000d3a33f3c3)";
    entity["msdyn_purchaseorder@odata.bind"] = "/msdyn_purchaseorders(" + wopWithPOMapped[0].POId + ")";
    entity.msdyn_quantity = 1;
    entity["msdyn_unit@odata.bind"] = "/uoms(361a3eac-749c-4bb3-92a2-d63f692f61ba)";
    entity["msdyn_associatetowarehouse@odata.bind"] = "/msdyn_warehouses(5b743789-c329-41ee-89e5-f81b83570131)";
    entity["ap360_workorderproductid@odata.bind"] = "/msdyn_workorderproducts(" + wopWithPONotMapped.WOPId + ")";

    var req = new XMLHttpRequest();
    req.open("POST", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/msdyn_purchaseorderproducts", true);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.send(JSON.stringify(entity));
    if (req.readyState === 4) {
        req.onreadystatechange = null;
        if (req.status === 204) {
            var uri = this.getResponseHeader("OData-EntityId");
            var regExp = /\(([^)]+)\)/;
            var matches = regExp.exec(uri);
            var newEntityId = matches[1];
        } else {
            Xrm.Utility.alertDialog(this.statusText);
        }
    }

}
function updateWorkOrderProduct(wopGuid, poGuid) {
    try {
        var data =
        {
            "ap360_purchaseorderid@odata.bind": "/msdyn_purchaseorders(" + poGuid + ")",
        }
        Xrm.WebApi.updateRecord("msdyn_workorderproduct", wopGuid, data).then(
            function success(result) {
                console.log("wop updated");
                // perform operations on record update
            },
            function (error) {
                console.log(error.message);
                // handle error conditions
            });
    }
    catch (exception) {
        throw exception;
    }

}
function RemoveWOPFromPurchaseOrder_OnClick_RemoveFromPO(primarycontrol, selectedItems) {
    debugger;
    var confirmStrings = { text: "Are you sure you want to remove WorkOrder Products.", title: "Remove PurchaseOrder Product" };
    var confirmOptions = { height: 200, width: 450 };
    Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
        function (success) {
            if (success.confirmed) {
                listOfWOPsAndPO = getPurchaseOrderAndWorkOrderForWorkOrderDisAssociation(selectedItems);
                if (listOfWOPsAndPO.length > 0) {

                    var wopWithPOMapped = listOfWOPsAndPO.filter(getWOPwithPurchaseOrderMapped);
                    var wopWithPONotMapped = listOfWOPsAndPO.filter(getWOPwithOUtPurchaseOrderMapped);

                    if (wopWithPOMapped.length == 1) {
                        var isPODraft = isPurchaseOrderDraft(wopWithPOMapped[0].POId);
                        if (!isPODraft) {
                            Xrm.Utility.alertDialog("PurchaseOrder Product can only be removed from Draft Purchase Order");
                            return;
                        }
                    }
                    for (var i = 0; i < selectedItems.length; i++) {
                        removeWOPFromPOP(selectedItems[i]);
                    }
                }
            }
            else
                console.log("Dialog closed using Cancel button or X.");
        });

}
function getPurchaseOrderAndWorkOrderForWorkOrderDisAssociation(selectedItems) {
    var purchaseOrderId = null;
    var listOfPurchaseOrders = [];
    var listVendorsInWorkOrderProducts = [];
    for (var i = 0; i < selectedItems.length; i++) {
        var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/msdyn_workorderproducts(" + selectedItems[i] + ")?$select=msdyn_name,_ap360_purchaseorderid_value,_ap360_vendorid_value,msdyn_workorderproductid";
        var result = callWebAPIWithoutParams(url, false);
        if (result != null) {
            var _ap360_purchaseorderid_value = result["_ap360_purchaseorderid_value"];
            var _ap360_vendorid_value = result["_ap360_vendorid_value"];
            var msdyn_name = result["msdyn_name"];
            var _ap360_purchaseorderid_value_formatted = result["_ap360_purchaseorderid_value@OData.Community.Display.V1.FormattedValue"];
            var _ap360_purchaseorderid_value_lookuplogicalname = result["_ap360_purchaseorderid_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
            var msdyn_workorderproductid = result["msdyn_workorderproductid"];
            var data = null;
            data = {
                POId: _ap360_purchaseorderid_value,
                WOPId: msdyn_workorderproductid
            }
            if (_ap360_vendorid_value == null) {
                Xrm.Utility.alertDialog("Select Vendor  in " + msdyn_name);
                return;
            }
            listVendorsInWorkOrderProducts.push(_ap360_vendorid_value);
            listOfPurchaseOrders.push(data);
        }
    }
    var isVendorUnique = isVendorUniqueInSelectedWOPs(listVendorsInWorkOrderProducts);
    if (!isVendorUnique) {
        Xrm.Utility.alertDialog("Vendor should be unique in selected Work Order Products");
        return;
    }
    return listOfPurchaseOrders;
}
function removeWOPFromPOP(selectedWOPGuid) {
    Xrm.Utility.showProgressIndicator("Please wait, WorkOrder Product(s) are removing...");
    var parameters = {};
    parameters.workOrderProductGUID = selectedWOPGuid;
    var results = null;
    var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ap360_removePOPFromPOForWOP";
    results = callWebAPIWithParams(url, true, parameters);

    if (results != null) {
        var lookupOptions = {};
        lookupOptions.entityType = "msdyn_workorderproduct";
        Xrm.Utility.refreshParentGrid(lookupOptions);
        Xrm.Utility.closeProgressIndicator();
    } else {
        Xrm.Utility.closeProgressIndicator();
    }
}
function updateWOPStatus_OnChangeOfWOPType(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var WOPType = formContext.getAttribute('ap360_workorderproducttype').getValue();
    //Customer Supplied	126300000
    //BlackWolf Inventory	126300001
    //Vendor Supplied	126300002
    if (WOPType == 126300002)//126300002 Vendor Supplied
    {
        formContext.getAttribute('ap360_workorderproductstatus').setValue(126300000); //Authorized To Order
    }

    else {
        formContext.getAttribute('ap360_workorderproductstatus').setValue(126300005); //Needs Release From Inventory
    }

}
function SetActualAmount_Or_RevisedAmount_onchangeLineStatus(executionContext) {
    var formContext = executionContext.getFormContext();
    var msdyn_linestatus = formContext.getAttribute("msdyn_linestatus").getValue();
    //debugger;
    if (msdyn_linestatus == 690970001) // Used
    {
        var ap360_vendorid = formContext.getAttribute("ap360_vendorid").getValue();
        if (ap360_vendorid != null) {

            ap360_vendorid = ap360_vendorid[0].id.replace('{', '').replace('}', '');

            if (ap360_vendorid.toLowerCase() == "a89044d7-9b9a-ea11-a811-000d3a33f3c3")//Black Wolf Inventory
            {
                var isrevised = formContext.getAttribute("ap360_isrevised").getValue();
                if (isrevised) {

                    var ap360_revisedestimateamount = formContext.getAttribute("ap360_revisedestimateamount").getValue();
                    if (ap360_revisedestimateamount != null) {
                        formContext.getAttribute("ap360_actualamount").setValue(ap360_revisedestimateamount);
                        //alert("Is revised Yes triggerd");
                    }
                }
                else {
                    var ap360_originalestimateamount = formContext.getAttribute("ap360_originalestimateamount").getValue();
                    if (ap360_originalestimateamount != null) {
                        formContext.getAttribute("ap360_actualamount").setValue(ap360_originalestimateamount);
                        //alert("Not revised Yes triggerd");
                    }

                }

            }
        }

    } else {
        // formContext.getAttribute("ap360_actualamount").setValue(null);
    }
}
function ReleasedWOP_OnchangeofLineStatus(executionContext) {
    debugger;
    var globalContext = Xrm.Utility.getGlobalContext();
    var formContext = executionContext.getFormContext();

    var WOPLineStatus = formContext.getAttribute("msdyn_linestatus").getValue();
    var WOPStatus = formContext.getAttribute("ap360_workorderproductstatus").getValue();
    var purchaseOrder = formContext.getAttribute("ap360_purchaseorderid").getValue();
    //msdyn_linestatus
    //Not Used	126300000
    //Estimated	690970000
    //Used	690970001
    if (WOPLineStatus == 690970001)//Used
    {

        formContext.getAttribute("ap360_releasedtoid").setRequiredLevel("required");
        formContext.getControl("ap360_releasedtoid").setDisabled(false);


        var date = new Date();

        formContext.getAttribute("ap360_releasedon").setValue(date);

    } else {
        formContext.getAttribute("ap360_releasedtoid").setRequiredLevel("none");
        formContext.getControl("ap360_releasedtoid").setDisabled(true);
        formContext.getAttribute("ap360_releasedon").setValue(null);
    }

}
function CancelledWOP_OnChangeOfLineStatus(executionContext) {
    //This  fucntion is important to cancel a workorder product, with out populating below mentioned field 
    //system won't allow to cancel a workorderproduct
    var formContext = executionContext.getFormContext();

    //msdyn_unitcost
    formContext.getAttribute("msdyn_unitamount").setValue(0);
    formContext.getAttribute("msdyn_unitcost").setValue(0);


}
function CancelledWOP_OnSave(executionContext) {
    console.log("***********************************CancelledWOP_OnSave");
    debugger;
    var globalContext = Xrm.Utility.getGlobalContext();
    var formContext = executionContext.getFormContext();

    var WOPLineStatus = formContext.getAttribute("msdyn_linestatus").getValue();
    var WOPStatus = formContext.getAttribute("ap360_workorderproductstatus").getValue();
    var purchaseOrder = formContext.getAttribute("ap360_purchaseorderid").getValue();
    //msdyn_linestatus
    //Not Used	126300000
    //Estimated	690970000
    //Used	690970001
    if (WOPLineStatus == 126300000)// Cancelled
    {
        if (purchaseOrder == null) {
            formContext.getControl("ap360_cancelledbyid").setVisible(true);

            //msdyn_unitcost
            formContext.getAttribute("msdyn_unitamount").setValue(0);
            formContext.getAttribute("msdyn_unitcost").setValue(0);

            var currentUser = new Array();
            currentUser[0] = new Object();
            currentUser[0].entityType = "systemuser";
            currentUser[0].id = globalContext.userSettings.userId;
            currentUser[0].name = globalContext.userSettings.userName;

            formContext.getAttribute("ap360_cancelledbyid").setValue(currentUser);

            formContext.getControl("ap360_cancelledon").setVisible(true);
            var date = new Date();
            formContext.getAttribute("ap360_cancelledon").setValue(date);
            formContext.getAttribute('ap360_workorderproductstatus').setValue(126300008); //Cancelled
            formContext.getAttribute("ap360_releasedon").setValue(null);
            formContext.getAttribute("ap360_releasedtoid").setValue(null);
            formContext.getAttribute("ap360_releasedtoid").setRequiredLevel("none");
        }

        else {
            Xrm.Utility.alertDialog("Work Order Product can't  be Cancelled if a Purchase Order Created");
            var saveEvent = executionContext.getEventArgs();
            saveEvent.preventDefault();
        }

    }
    else if (WOPLineStatus == 690970000)//Estimated
    {
        var workorderproducttype = formContext.getAttribute("ap360_workorderproducttype").getValue();
        //Customer Supplied	126300000
        //BlackWolf Inventory	126300001
        //Vendor Supplied	126300002
        if (formContext.ui.getFormType() == 1)//create
        {
            if (workorderproducttype == 126300002) {
                formContext.getAttribute('ap360_workorderproductstatus').setValue(126300000); //Authorized To Order
            }
            else {
                formContext.getAttribute('ap360_workorderproductstatus').setValue(126300005); //Needs Release From Inventory
            }
        }


        formContext.getAttribute("ap360_cancelledon").setValue(null);
        formContext.getAttribute("ap360_cancelledbyid").setValue(null);
        formContext.getAttribute("ap360_releasedon").setValue(null);
        formContext.getAttribute("ap360_releasedtoid").setValue(null);
        formContext.getAttribute("ap360_releasedtoid").setRequiredLevel("none");


    }
}

////////////////////Business Process Flow Function This function will callied on Onload Fuction
function makeStageFieldsReadOnly(executionContext) {
    var formContext = executionContext.getFormContext();
    if (formContext.getControl("header_process_ap360_ggparent") != null) {
        formContext.getControl("header_process_ap360_ggparent").setDisabled(true);
        formContext.getControl("header_process_ap360_product").setDisabled(true);
        formContext.getControl("header_process_ap360_productdescription").setDisabled(true);
        formContext.getControl("header_process_ap360_workorderproducttype").setDisabled(true);
        formContext.getControl("header_process_ap360_workorderproducttype_1").setVisible(false);
        formContext.getControl("header_process_ap360_partnumber").setDisabled(true);
        formContext.getControl("header_process_msdyn_estimatequantity").setDisabled(true);
        formContext.getControl("header_process_ap360_reviseditemstatus").setDisabled(true);
        formContext.getControl("header_process_ap360_vendoridentified").setDisabled(true);
        formContext.getControl("header_process_ap360_vendorid").setDisabled(true);
        formContext.getControl("header_process_ap360_purchaseorderid").setDisabled(true);
        formContext.getControl("header_process_ap360_workorderproductstatus").setDisabled(true);
        formContext.getControl("header_process_ap360_releasedon").setDisabled(true);
        if (formContext.getAttribute("ap360_product").getValue() != null) {
            formContext.getControl("header_process_ap360_productdescription").setVisible(false);
        } else {
            formContext.getControl("header_process_ap360_productdescription").setVisible(true);
        }
    }
}

/////////////////Global function for all web api process calls
function callWebAPIWithoutParams(urlPassByPerameter, isSync) {
    var req = new XMLHttpRequest();
    var url = urlPassByPerameter
    req.open("GET", url, isSync);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
    req.send();
    var result = null
    if (req.readyState === 4) {
        if (req.status === 200) {
            result = JSON.parse(req.response);
        }
        else if (req.status === 400) {
            result = JSON.parse(req.response);
            Xrm.Utility.alertDialog(result.error.message);
            return;
        }
    }
    else {
        result = JSON.parse(req.response);
        Xrm.Utility.alertDialog(result.error.message);
        return;
    }
    return result;
}
function callWebAPIWithParams(urlPassByPerameter, isSync, Record) {
    var result = null
    var req = new XMLHttpRequest();
    var url = urlPassByPerameter
    req.open("POST", url, isSync);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
    req.send(JSON.stringify(Record));
    if (req.readyState === 4) {
        req.onreadystatechange = null;
        if (req.status === 200) {
            result = JSON.parse(req.response);
        } else if (req.status === 400) {
            result = JSON.parse(req.response);
            Xrm.Utility.alertDialog(result.error.message);
            return null;
        }
    } else {
        result = JSON.parse(req.response);
        Xrm.Utility.alertDialog(result.error.message);
        return null;
    }

    return result;
}