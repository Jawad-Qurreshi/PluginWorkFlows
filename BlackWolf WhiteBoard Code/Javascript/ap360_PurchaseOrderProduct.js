// JavaScript source code
// JavaScript source code
// JavaScript source code
var selelctedPurchaseOrderProductsGuid;

function onLoad(executionContext) {
    var formContext = executionContext.getFormContext();
    var formType = formContext.ui.getFormType();
    if (formType == 1) //create
    {

        var lookupValue = new Array();
        lookupValue[0] = new Object();
        lookupValue[0].id = "5b743789-c329-41ee-89e5-f81b83570131"
        lookupValue[0].name = "Receiving";
        lookupValue[0].entityType = "msdyn_warehouse";
        formContext.getAttribute("msdyn_associatetowarehouse").setValue(lookupValue);
    }

    if (formType == 2) //Update
    {
        var POPItemStatus = formContext.getAttribute("msdyn_itemstatus").getValue();
        var POPItemSubStatus = formContext.getAttribute("ap360_itemsubstatus").getValue();
        if (POPItemSubStatus == 126300000)//Received
        {
            formContext.ui.controls.forEach(function (control, i) {

                if (control && control.getDisabled && !control.getDisabled()) {

                    control.setDisabled(true);

                }

            });

        }
        else {
            formContext.ui.controls.forEach(function (control, i) {
                if (control._controlName != "ap360_partnumber" && control._controlName != "ap360_vendorid" && control._controlName != "msdyn_quantity" && control._controlName != "msdyn_unitcost" && control._controlName != "msdyn_product" && control._controlName != "msdyn_itemstatus" && control._controlName != "ap360_itemsubstatus") {

                    if (control && control.getDisabled && !control.getDisabled()) {

                        control.setDisabled(true);

                    }
                }

            });
        }


    }
}




function createPurchaseOrderReceiptProducts(selectedItems) {
    // alert("Hello");
    debugger;

    Xrm.Utility.showProgressIndicator("Please wait, while selected parts being received...");
    // checkPurchaseOrderforSelectedWOPs(selectedItems);
    var isPurhcaseOrderExists = false;
    selelctedPurchaseOrderProductsGuid = "";
    var count = 1;

    Xrm.Page.getControl("editablesubgridPOP").refresh();

    //InOrder to save grid before receiving product
    setTimeout(function () {
        for (var i = 0; i < selectedItems.length; i++) {

            //isPurhcaseOrderExists = checkPurchaseOrderforSelectedWOPs(selectedItems[i].Id);

            //if (isPurhcaseOrderExists == true) {
            //    alert("DeSelect row(s) where Purchase Order created and Try Again");
            //    return;
            //}
            selelctedPurchaseOrderProductsGuid += selectedItems[i].Id;
            if (count > 1 || count < selectedItems.length)
                selelctedPurchaseOrderProductsGuid += ",";
            count++;
            console.log(selectedItems[i].Id);
        }

        if (selectedItems.length > 1)// if only one then no need to remove last character as in case of One record last character is Guid char  
            selelctedPurchaseOrderProductsGuid = selelctedPurchaseOrderProductsGuid.substring(0, selelctedPurchaseOrderProductsGuid.length - 1);

        CallAction(selelctedPurchaseOrderProductsGuid);

    }, 5000);


}

function CallAction(selelctedPurchaseOrderProductsGuid) {

    try {
        debugger;
        var parameters = {};
        parameters.purchaseOrderGUID = selelctedPurchaseOrderProductsGuid;
        var req = new XMLHttpRequest();
        req.open("POST", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ap360_CreatePurchaseOrderReceiptProduct", true);
        req.setRequestHeader("OData-MaxVersion", "4.0");
        req.setRequestHeader("OData-Version", "4.0");
        req.setRequestHeader("Accept", "application/json");
        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        req.onreadystatechange = function () {
            if (this.readyState === 4) {
                req.onreadystatechange = null;
                if (this.status === 200) {
                    var results = JSON.parse(this.response);
                    Xrm.Utility.closeProgressIndicator();
                    Xrm.Utility.alertDialog(results.resultMessage);
                } else {
                    var results = JSON.parse(this.response);
                    Xrm.Utility.closeProgressIndicator();
                    Xrm.Utility.alertDialog(results.error.message);
                }
            }
        };
        req.send(JSON.stringify(parameters));
    } catch (e) {

        Xrm.Utility.alertDialog(e.message);
        Xrm.Utility.closeProgressIndicator();
    }
}


// function CallAction() {

//     try {
//         Process.callAction("ap360_CreatePurchaseOrderReceiptProduct",
//             [
//                 {
//                     key: "selelctedPurchaseOrderProductsGuid",
//                     type: Process.Type.String,
//                     value: selelctedPurchaseOrderProductsGuid
//                 }],
//             function (params) {
//                 debugger;
//                 Xrm.Utility.closeProgressIndicator();
//                 Xrm.Navigation.openAlertDialog(params["resultMessage"]);
//                 Xrm.Page.getControl("editablesubgridPOP").refresh();

//                 //Xrm.Page.getControl("editablesubgridPOP").refresh();
//                 //Xrm.Navigation.openAlertDialog("Selected Parts Received");
//                 //Xrm.Utility.closeProgressIndicator();
//                 // Success
//                 //alert(params[accountName]);
//                 //alert("Name = " + params["Entity"].get("name") + "\n" +
//                 //      "Status = " + params["Entity"].formattedValues["statuscode"]);
//             },
//             function (e, t) {
//                 // Error
//                 debugger;
//                 Xrm.Navigation.openAlertDialog(e);
//                 Xrm.Utility.closeProgressIndicator();

//                 // Write the trace log to the dev console
//                 if (window.console && console.error) {
//                     console.error(e + "\n" + t);
//                 }
//             });

//     } catch (e) {

//         Xrm.Utility.alertDialog(e.message);

//     }
// }

function updateUpdatedUnitSalePrice(executionContext)//On Change of Quantity
{
    var formContext = executionContext.getFormContext(); // get formContext

    var quantity = null;
    var unitcost = null;
    var multiplier = null;
    quantity = formContext.getAttribute("msdyn_quantity").getValue();
    unitcost = formContext.getAttribute("ap360_updatedunitcost").getValue();
    multiplier = formContext.getAttribute("ap360_multiplier").getValue();


    if (quantity != null && unitcost != null && multiplier != null) {

        formContext.getAttribute("ap360_extendedupdatedunitsaleprice").setValue(multiplier * unitcost * quantity);
        //formContext.getAttribute("ap360_extendedupdatedunitsaleprice").setValue( unitcost * multiplier);
    }
    else {
        formContext.getAttribute("ap360_updatedunitsaleprice").setValue(null);
    }
}

function CalculatePriceMarkup(executionContext)//ON Change of updated  Unit Cost
{

    //msdyn_quantity
    //ap360_updatedunitcost
    //ap360_updatedunitsaleprice
    //ap360_extendedupdatedunitsaleprice
    var formContext = executionContext.getFormContext(); // get formContext

    // use formContext instead of Xrm.Page

    var quantity = null;
    var unitcost = null;
    quantity = formContext.getAttribute("msdyn_quantity").getValue();
    unitcost = formContext.getAttribute("ap360_updatedunitcost").getValue();

    if (quantity != null && unitcost != null) {

        var multiplier = null;
        multiplier = getPriceMarkUpMatrix(unitcost);
        if (multiplier != null) {
            formContext.getAttribute("ap360_multiplier").setValue(multiplier);

            formContext.getAttribute("ap360_updatedunitsaleprice").setValue(multiplier * unitcost);
            formContext.getAttribute("ap360_extendedupdatedunitsaleprice").setValue(unitcost * multiplier * quantity);

        }
        //alert(multiplier);

    }
    else {
        formContext.getAttribute("ap360_updatedunitsaleprice").setValue(null);
        formContext.getAttribute("ap360_multiplier").setValue(null);


        Xrm.Navigation.openAlertDialog("Quantity and UnitCost must be filled")
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

function updateCost_OnChangeOfSaleTax(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext(); // get formContext
    var ap360_saletax = formContext.getAttribute("ap360_saletax").getValue();
    var originalCost = 0;
    if (ap360_saletax != null) {
        var msdyn_quantity = formContext.getAttribute("msdyn_quantity").getValue();

        if (msdyn_quantity > 0 && msdyn_quantity != null) {

            var saleTaxforsingleproduct = ap360_saletax / msdyn_quantity;
            var msdyn_unitcost = formContext.getAttribute("msdyn_unitcost").getValue();
            var ap360_cost = formContext.getAttribute("ap360_cost").getValue();
            if (ap360_cost == null || ap360_cost == 0)//first time map Unit Cost to Cost to preserve original cost
            {
                formContext.getAttribute("ap360_cost").setValue(msdyn_unitcost);
                originalCost = msdyn_unitcost;
            }
            else {
                originalCost = ap360_cost;
            }
            if (msdyn_unitcost > 0 && msdyn_unitcost != null) {
                formContext.getAttribute("msdyn_unitcost").setValue(saleTaxforsingleproduct + originalCost);
            }
            msdyn_unitcost = formContext.getAttribute("msdyn_unitcost").getValue();
            formContext.getAttribute("msdyn_totalcost").setValue(msdyn_quantity * msdyn_unitcost);
        }
        //msdyn_unitcost
    }
    else {
        var ap360_cost = formContext.getAttribute("ap360_cost").getValue();

        if (ap360_cost != null && ap360_cost != 0) {
            formContext.getAttribute("msdyn_unitcost").setValue(ap360_cost);
        }

    }

}


//map change
function mapWOandOppOnChangeOfWOP(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var WOProduct = formContext.getAttribute("ap360_workorderproductid").getValue();
    if (WOProduct != null) {
        WOProduct = WOProduct[0].id.replace('{', '').replace('}', '');

        var req = new XMLHttpRequest();
        req.open("GET", formContext.context.getClientUrl() + "/api/data/v9.1/msdyn_workorderproducts(" + WOProduct + ")?$select=_ap360_opportunityid_value,_msdyn_workorder_value", false);
        req.setRequestHeader("OData-MaxVersion", "4.0");
        req.setRequestHeader("OData-Version", "4.0");
        req.setRequestHeader("Accept", "application/json");
        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
        req.send();

        if (req.readyState === 4) {
            req.onreadystatechange = null;
            if (req.status === 200) {
                var result = JSON.parse(req.response);

                var _ap360_opportunityid_value = result["_ap360_opportunityid_value"];
                var _ap360_opportunityid_value_formatted = result["_ap360_opportunityid_value@OData.Community.Display.V1.FormattedValue"];
                var _ap360_opportunityid_value_lookuplogicalname = result["_ap360_opportunityid_value@Microsoft.Dynamics.CRM.lookuplogicalname"];

                var _msdyn_workorder_value = result["_msdyn_workorder_value"];
                var _msdyn_workorder_value_formatted = result["_msdyn_workorder_value@OData.Community.Display.V1.FormattedValue"];
                var _msdyn_workorder_value_lookuplogicalname = result["_msdyn_workorder_value@Microsoft.Dynamics.CRM.lookuplogicalname"];

                if (_msdyn_workorder_value != null) {
                    var WOlookupValue = new Array();
                    WOlookupValue[0] = new Object();
                    WOlookupValue[0].id = _msdyn_workorder_value;
                    WOlookupValue[0].name = _msdyn_workorder_value_formatted;
                    WOlookupValue[0].entityType = "msdyn_workorder";

                    formContext.getAttribute("msdyn_associatetoworkorder").setValue(WOlookupValue);

                }


                if (_ap360_opportunityid_value != null) {

                    var OpplookupValue = new Array();
                    OpplookupValue[0] = new Object();
                    OpplookupValue[0].id = _ap360_opportunityid_value;
                    OpplookupValue[0].name = _ap360_opportunityid_value_formatted;
                    OpplookupValue[0].entityType = "opportunity";

                    formContext.getAttribute("ap360_opportunityid").setValue(OpplookupValue);
                }
            } else {
                Xrm.Utility.alertDialog(req.statusText);
            }
        }

    }
}

//////////////On Change of Opportunity WO Pre-Filter
function preFilterWOandSetOppNumberonChangeOfOpportunity(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var opportunity = formContext.getAttribute("ap360_opportunityid").getValue();
    if (opportunity != null) {
        opportunity = opportunity[0].id.replace('{', '').replace('}', '');
        var req = new XMLHttpRequest();
        req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/opportunities(" + opportunity + ")?$select=ap360_opportunityautonumber", false);
        req.setRequestHeader("OData-MaxVersion", "4.0");
        req.setRequestHeader("OData-Version", "4.0");
        req.setRequestHeader("Accept", "application/json");
        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
        req.send();

        if (req.readyState === 4) {
            req.onreadystatechange = null;
            if (req.status === 200) {
                var result = JSON.parse(req.response);
                var ap360_opportunityautonumber = result["ap360_opportunityautonumber"];
                if (ap360_opportunityautonumber != null)
                    formContext.getAttribute("ap360_opportunityautonumber").setValue(ap360_opportunityautonumber);
            } else {
                Xrm.Utility.alertDialog(req.statusText);
            }
        }

        FilterWorkOrder(formContext);
    } else {
        formContext.getAttribute("ap360_opportunityautonumber").setValue(null);

    }
}
function FilterWorkOrder(formContext) {
    formContext.getControl("msdyn_associatetoworkorder").addPreSearch(function () {
        addWorkOrderPreSearchFilter(formContext);
    });
}
function addWorkOrderPreSearchFilter(formContext) {
    debugger;
    var opportunity = formContext.getAttribute("ap360_opportunityid").getValue();
    var filter;
    if (opportunity == null) {
        filter = null;
    } else {
        filter = "<filter type=\"and\">" +
            "<condition attribute=\"msdyn_opportunityid\" operator=\"eq\"   value=\"" + opportunity[0].id + "\" />" +
            "</filter>";
        formContext.getControl("msdyn_associatetoworkorder").addCustomFilter(filter);
    }
}
////////////// End On Change of Opportunity WO Pre-Filter

/////////////// on change of WO Pre filter WOP
function preFilterWOPandMapOpportunityonChangeofWO(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var WorkOrder = formContext.getAttribute("msdyn_associatetoworkorder").getValue();
    if (WorkOrder != null) {
        WorkOrder = WorkOrder[0].id.replace('{', '').replace('}', '');

        var req = new XMLHttpRequest();
        req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/msdyn_workorders(" + WorkOrder + ")?$select=_msdyn_opportunityid_value", false);
        req.setRequestHeader("OData-MaxVersion", "4.0");
        req.setRequestHeader("OData-Version", "4.0");
        req.setRequestHeader("Accept", "application/json");
        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
        req.send();

        if (req.readyState === 4) {
            req.onreadystatechange = null;
            if (req.status === 200) {
                var result = JSON.parse(req.response);
                var _msdyn_opportunityid_value = result["_msdyn_opportunityid_value"];
                var _msdyn_opportunityid_value_formatted = result["_msdyn_opportunityid_value@OData.Community.Display.V1.FormattedValue"];
                var _msdyn_opportunityid_value_lookuplogicalname = result["_msdyn_opportunityid_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
                if (_msdyn_opportunityid_value != null) {
                    var OpplookupValue = new Array();
                    OpplookupValue[0] = new Object();
                    OpplookupValue[0].id = _msdyn_opportunityid_value;
                    OpplookupValue[0].name = _msdyn_opportunityid_value_formatted;
                    OpplookupValue[0].entityType = "opportunity";

                    formContext.getAttribute("ap360_opportunityid").setValue(OpplookupValue);
                }

            } else {
                Xrm.Utility.alertDialog(req.statusText);
            }
        }


        preFilterWorkOrderProductonChangeofWO(formContext);
    }
}
function preFilterWorkOrderProductonChangeofWO(formContext) {
    formContext.getControl("ap360_workorderproductid").addPreSearch(function () {
        addWorkOrderProductPreSearchFilteronChangeofWO(formContext);
    });
}
function addWorkOrderProductPreSearchFilteronChangeofWO(formContext) {
    debugger;
    var workorder = formContext.getAttribute("msdyn_associatetoworkorder").getValue();
    var filter;
    if (workorder == null) {
        filter = null;
    } else {
        filter = "<filter type=\"and\">" +
            "<condition attribute=\"msdyn_workorder\" operator=\"eq\"   value=\"" + workorder[0].id + "\" />" +
            "</filter>";
        formContext.getControl("ap360_workorderproductid").addCustomFilter(filter);
    }
}
/////////////// End of on change of WO Pre filter WOP

function createPurchaseOrderReceipt(primaryControl) {
    debugger;
    var purchaseOrderGUID = primaryControl.data.entity.getId();
    var entityName = primaryControl.data.entity.getEntityName();
    purchaseOrderGUID = purchaseOrderGUID.replace('{', '').replace('}', '');
    try {
        Xrm.Utility.showProgressIndicator("Please wait, while receipt is creating...");
        var parameters = {};
        parameters.purchaseOrderGUID = purchaseOrderGUID;

        var req = new XMLHttpRequest();
        req.open("POST", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ap360_CreatePurchaseOrderReceipt", false);
        req.setRequestHeader("OData-MaxVersion", "4.0");
        req.setRequestHeader("OData-Version", "4.0");
        req.setRequestHeader("Accept", "application/json");
        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        req.send(JSON.stringify(parameters));
        //req.onreadystatechange = function () {
        if (req.readyState === 4) {
            debugger;
            req.onreadystatechange = null;
            if (req.status === 200) {
                var ResponseText = JSON.parse(req.responseText);

                Xrm.Utility.closeProgressIndicator();
                Xrm.Utility.alertDialog(ResponseText.resultMessage);
                Xrm.Page.getControl("POReceipts").refresh();

            } else {
                Xrm.Utility.alertDialog(req.statusText);
            }
        }
        //};
    }
    catch (e) {
        Xrm.Utility.closeProgressIndicator();
        Xrm.Utility.alertDialog(e.message);
    }
}


//function createPurchaseOrderReceipt(primaryControl) {
//    debugger;
//    var purchaseOrderGUID = primaryControl.data.entity.getId();
//    var entityName = primaryControl.data.entity.getEntityName();
//    try {
//        Xrm.Utility.showProgressIndicator("Please wait, while receipt is creating...");
//        Process.callAction("ap360_CreatePurchaseOrderReceipt",
//            [
//                {
//                    key: "purchaseOrderGUID",
//                    type: Process.Type.String,
//                    value: purchaseOrderGUID
//                }
//            ],
//            function (params) {
//                debugger;


//                Xrm.Utility.closeProgressIndicator();
//                Xrm.Navigation.openAlertDialog(params["resultMessage"]);
//                Xrm.Page.getControl("POReceipts").refresh();



//            },
//            function (e, t) {
//                // Error
//                debugger;
//                Xrm.Utility.closeProgressIndicator();
//                Xrm.Navigation.openAlertDialog(e);

//                // Write the trace log to the dev console
//                if (window.console && console.error) {
//                    console.error(e + "\n" + t);
//                }
//            });

//    } catch (e) {

//        Xrm.Utility.alertDialog(e.message);

//    }
//}

function LockPurchaseOrderProductSubgridFields(executionContext) {
    debugger;

    LockFieldsOnEditableGrid(executionContext, ["msdyn_purchaseorder", "ap360_partnumber", "msdyn_qtyreceived", "msdyn_totalcost", "msdyn_quantity", "msdyn_unitcost", "msdyn_product", "msdyn_associatetoworkorder", "msdyn_itemstatus", "ap360_itemsubstatus"]);

}

LockFieldsOnEditableGrid = function (context, disabledFields) {

    var currEntity = context.getFormContext().data.entity;

    currEntity.attributes.forEach(function (attribute, i) {

        if (disabledFields.indexOf(attribute.getName()) > -1) {

            var attributeToDisable = attribute.controls.get(0);

            attributeToDisable.setDisabled(true);

        }

    });

}

function mapPartNumber_OnChangeOfProduct(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var Product = formContext.getAttribute("msdyn_product").getValue();
    if (Product != null) {
        Product = Product[0].id.replace('{', '').replace('}', '');

        var req = new XMLHttpRequest();
        var uRL = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/products(" + Product + ")?$select=ap360_partnumber";
        req.open("GET", uRL, false);
        req.setRequestHeader("OData-MaxVersion", "4.0");
        req.setRequestHeader("OData-Version", "4.0");
        req.setRequestHeader("Accept", "application/json");
        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
        req.send();

        if (req.readyState === 4) {
            req.onreadystatechange = null;
            if (req.status === 200) {
                var result = JSON.parse(req.response);
                var ap360_partnumber = result["ap360_partnumber"];
                if (ap360_partnumber != null) {
                    formContext.getAttribute("ap360_partnumber").setValue(ap360_partnumber);
                }
            }

        } else {
            Xrm.Utility.alertDialog(req.statusText);
        }
    } else {
        formContext.getAttribute("ap360_partnumber").setValue(null);
    }


}


function updateItemSubstatusOnUpdateOfItemStatus(executionContext) {
    var formContext = executionContext.getFormContext();
    var POPItemStatus = formContext.getAttribute("msdyn_itemstatus").getValue();
    if (POPItemStatus == 690970002)//Cancelled
    {
        formContext.getAttribute("ap360_itemsubstatus").setValue(126300003);//Cancelled
    } else if (POPItemStatus == 690970000)//Pending
    {
        formContext.getAttribute("ap360_itemsubstatus").setValue(126300002);//Pending
    }
}

