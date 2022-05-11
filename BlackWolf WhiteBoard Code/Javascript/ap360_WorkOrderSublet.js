

// JavaScript source code
// JavaScript source code



function onload(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    FilterVendor(formContext);
    //makeFormReadOnlyonWOClosedPosted(executionContext);
}

function setWOSubletName_OnChangeOfSubletDescription(executionContext) {
    var formContext = executionContext.getFormContext();
    var subletdescription = formContext.getAttribute("ap360_subletdescription").getValue();
    if (subletdescription != null) {
        formContext.getAttribute("ap360_name").setValue(subletdescription);
    }
    else {
        formContext.getAttribute("ap360_name").setValue(null);
    }

}
function FilterVendor(formContext) {
    formContext.getControl("ap360_vendorid").addPreSearch(addAccountsPreSearchFilter);
}
function addAccountsPreSearchFilter() {
    var filter = "<filter type=\"and\">" +
        "<condition attribute=\"customertypecode\" operator=\"eq\"  value=\"11\" />" +
        "</filter>";
    Xrm.Page.getControl("ap360_vendorid").addCustomFilter(filter);

}
// JavaScript source code


var ap360_vendorid = null;
var selelctedWorkOrderSubletsGuid;


function onload(executionContext) {
    var formContext = executionContext.getFormContext();
    FilterVendor(formContext);

}

function setWOSubletName_OnChangeOfSubletDescription(executionContext) {
    var formContext = executionContext.getFormContext();
    var subletdescription = formContext.getAttribute("ap360_subletdescription").getValue();
    if (subletdescription != null) {
        formContext.getAttribute("ap360_name").setValue(subletdescription);
    }
    else {
        formContext.getAttribute("ap360_name").setValue(null);
    }

}
function FilterVendor(formContext) {
    formContext.getControl("ap360_vendorid").addPreSearch(addAccountsPreSearchFilter);
}
function addAccountsPreSearchFilter() {
    var filter = "<filter type=\"and\">" +
        "<condition attribute=\"customertypecode\" operator=\"eq\"  value=\"11\" />" +
        "</filter>";
    Xrm.Page.getControl("ap360_vendorid").addCustomFilter(filter);

}
function isVendorIdentified(selectedItems) {
    var isVendorExists = true;
    var listOfVendors = [];
    for (var i = 0; i < selectedItems.length; i++) {
        var req = new XMLHttpRequest();
        var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ap360_workordersublets(" + selectedItems[i] + ")?$select=ap360_name,_ap360_vendorid_value";
        req.open("GET", url, false);
        req.setRequestHeader("OData-MaxVersion", "4.0");
        req.setRequestHeader("OData-Version", "4.0");
        req.setRequestHeader("Accept", "application/json");
        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
        req.onreadystatechange = function () {
            if (this.readyState === 4) {
                req.onreadystatechange = null;
                if (this.status === 200) {
                    var result = JSON.parse(this.response);
                    //debugger;
                    var ap360_name = result["ap360_name"];
                    ap360_vendorid = result["_ap360_vendorid_value"];

                    if (ap360_vendorid == null) {
                        isVendorExists = false;
                        Xrm.Utility.alertDialog(" Work Order Sublet (" + ap360_name + ")  vendor is not identified. Please identify the vendor!");
                        return;
                    }
                    else {
                        listOfVendors.push(ap360_vendorid);

                    }

                    for (var i = 0; i < listOfVendors.length; i++) {

                        if (ap360_vendorid != listOfVendors[i]) {
                            Xrm.Utility.alertDialog("One or more selected WO Sublet have different vendor.  Selected WO Sublets should have same Vendor");
                            isVendorExists = false;
                        }

                    }
                } else {
                    Xrm.Utility.alertDialog(this.statusText);
                }
            }
        };
        req.send();
    }

    return isVendorExists;

}
function callWorkOrderSubletPurchaseOrderPlugin(selectedAccount) {
    try {
        //if (selectedAccount.length > 1) {
        //    return;
        //}
        //if (selectedAccount != undefined && selectedAccount != null && selectedAccount != "") {
        //    var selectedAccountGuid = selectedAccount[0].id.replace('{', '').replace('}', '');
        Process.callAction("ap360_CreatePurchaseOrderForWOSublet",
            [{
                key: "preferredSupplierId",
                type: Process.Type.EntityReference,
                value: new Process.EntityReference("account", ap360_vendorid)
            },
            {
                key: "selelctedWorkOrderSubletsGuid",
                type: Process.Type.String,
                value: selelctedWorkOrderSubletsGuid
            }],
            function (params) {
                //debugger;
                var lookupOptions = {};
                lookupOptions.entityType = "ap360_workordersublet";
                Xrm.Utility.refreshParentGrid(lookupOptions);
            },
            function (e, t) {
                debugger;
                // Error
                alert(e);

                // Write the trace log to the dev console
                if (window.console && console.error) {
                    console.error(e + "\n" + t);
                }
            });


        //}
    } catch (e) {

        Xrm.Utility.alertDialog("A:" + e.message);

    }

}
function SalePrice_OnChangeofSubletcost(executionContext) {

    debugger;
    var formContext = executionContext.getFormContext();
    var estimatedamount = formContext.getAttribute("ap360_subletcost").getValue();
    if (estimatedamount != null) {
        formContext.getAttribute("ap360_revisedestimatedamount").setValue(estimatedamount * 1.3);//Sale Price
    }
    formContext.getAttribute("ap360_isrevised").setValue(true);

}
function CreatePurchaseOrderforSublet(selectedItems, formContext) {
    debugger;
    Xrm.Utility.showProgressIndicator("Creating purchase order...");
    var nameOfWOS = checkIfWOSApprovedOrNot(selectedItems);

    if (nameOfWOS != null) {
        Xrm.Utility.closeProgressIndicator();
        Xrm.Utility.alertDialog("Revised item status of sublet '" + nameOfWOS + "' not approved!");
        return;
    }

    var isPurhcaseOrderExists = false;
    selelctedWorkOrderSubletsGuid = "";
    var count = 1;
    var isVendorExists = true;
    isVendorExists = isVendorIdentified(selectedItems);
    if (!isVendorExists) {
        Xrm.Utility.closeProgressIndicator();
        return;
    }

    for (var i = 0; i < selectedItems.length; i++) {

        isPurhcaseOrderExists = checkPurchaseOrderforSelectedWOSublets(selectedItems[i]);
        if (isPurhcaseOrderExists == true) {
            Xrm.Utility.alertDialog("DeSelect row(s) where Purchase Order created and Try Again");
            Xrm.Utility.closeProgressIndicator();
            return;
        }
        selelctedWorkOrderSubletsGuid += selectedItems[i];
        if (count > 1 || count < selectedItems.length)
            selelctedWorkOrderSubletsGuid += ",";
        count++;
        console.log(selectedItems[i]);
    }
    if (selectedItems.length > 1)// if only one then no need to remove last character as in case of One record last character is Guid char  
        selelctedWorkOrderSubletsGuid = selelctedWorkOrderSubletsGuid.substring(0, selelctedWorkOrderSubletsGuid.length - 1);
    // showAccountLookupDialog();
    callWorkOrderSubletPurchaseOrderPlugin(null);

    Xrm.Utility.closeProgressIndicator();
}

function checkIfWOSApprovedOrNot(selectedItems) {
    debugger;
    var WOSName = null;
    for (var i = 0; i < selectedItems.length; i++) {
        var req = new XMLHttpRequest();
        var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ap360_workordersublets(" + selectedItems[i] + ")?$select=ap360_reviseditemstatus,ap360_name";
        req.open("GET", url, false);
        req.setRequestHeader("OData-MaxVersion", "4.0");
        req.setRequestHeader("OData-Version", "4.0");
        req.setRequestHeader("Accept", "application/json");
        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
        req.onreadystatechange = function () {
            if (this.readyState === 4) {
                req.onreadystatechange = null;
                if (this.status === 200) {
                    var result = JSON.parse(this.response);
                    //debugger;
                    var ap360_name = result["ap360_name"];
                    var ap360_reviseditemstatus = result["ap360_reviseditemstatus"];
                    if (ap360_reviseditemstatus != 126300001)//Approved
                    {

                        WOSName = ap360_name;

                    }

                } else {
                    Xrm.Utility.alertDialog(this.statusText);
                }
            }
        };
        req.send();
    }
    return WOSName;
}
function showAccountLookupDialog() {
    try {

        var fetch = "<filter type='and'><condition attribute='name' operator='like' value='%%' /></filter>";

        var encodedURL = encodeURIComponent(fetch).replace(/"/g, '%22');

        var lookupParameters = {};

        //specify the list of entity types to be displayed in lookup dialog

        lookupParameters.entityTypes = ['account'];

        //Sepecify the default entityType need to be displayed

        lookupParameters.defaultEntityType = 'account';

        //Default view need to be displayed

        lookupParameters.defaultViewId = '{00000000-0000-0000-00aa-000010001002}';

        //allow multiple selection or not

        lookupParameters.allowMultiSelect = false;

        //list multiple views available on lookup dialog

        // lookupParameters.viewIds = ["{0D5D377B-5E7C-47B5-BAB1-A5CB8B4AC105}�, "{A2D479C5-53E3-4C69-ADDD-802327E67A0D}�];

        lookupParameters.customFilters = [encodedURL];

        lookupParameters.customFilterTypes = ['account'];

        Xrm.Utility.lookupObjects(lookupParameters).then(DisplaySelectedAccount, null);

    } catch (e) {

        Xrm.Utility.alertDialog(e.message);

    }

}
function checkPurchaseOrderforSelectedWOSublets(workOrderSubletId) {

    var purchaseOrderId = null;
    var isPurhcaseOrderExists = false;
    //debugger;
    var req = new XMLHttpRequest();
    req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ap360_workordersublets?$select=_ap360_purchaseorderid_value&$filter=ap360_workordersubletid eq " + workOrderSubletId, false);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
    req.send();
    if (req.readyState === 4) {
        req.onreadystatechange = null;
        if (req.status === 200) {
            var results = JSON.parse(req.response);
            for (var i = 0; i < results.value.length; i++) {
                var _ap360_purchaseorderid_value = results.value[i]["_ap360_purchaseorderid_value"];
                var _ap360_purchaseorderid_value_formatted = results.value[i]["_ap360_purchaseorderid_value@OData.Community.Display.V1.FormattedValue"];
                var _ap360_purchaseorderid_value_lookuplogicalname = results.value[i]["_ap360_purchaseorderid_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
                if (_ap360_purchaseorderid_value != null) {
                    isPurhcaseOrderExists = true;
                }
            }
        } else {
            Xrm.Utility.alertDialog(req.statusText);
        }
    }
    return isPurhcaseOrderExists;
}
function Hide_NewWOSublet_Ribbon(formContext, primaryentitytypename) {
    //debugger;
    if (primaryentitytypename == "opportunity") {
        return false;
    } else {
        return true;
    }
}
function AdminFollowUpClick(primaryControl)//Shop
{

    // Set booking Classification
    debugger;
    var formContext = primaryControl;
    var generalTab = formContext.ui.tabs.get("generaltab");
    var followup = generalTab.sections.get("followup"); //Get sections
    followup.setVisible(true); //Show Section

    formContext.getControl("ap360_adminfollowup").setVisible(true);
    formContext.getAttribute("ap360_adminfollowup").setValue(true);

    formContext.getControl("ap360_followupdescription").setVisible(true);
    formContext.getAttribute("ap360_followupdescription").setRequiredLevel("required");

    formContext.getControl("ap360_followedupbyid").setVisible(true);
    var globalContext = Xrm.Utility.getGlobalContext();
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
function makeFormReadOnlyonWOClosedPosted(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var msdyn_workorder = formContext.getAttribute("ap360_workorderid").getValue();
    if (msdyn_workorder != null) {
        msdyn_workorder = msdyn_workorder[0].id.replace('{', '').replace('}', '');

        var req = new XMLHttpRequest();
        var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/msdyn_workorders(" + msdyn_workorder + ")?$select=msdyn_systemstatus";
        req.open("GET", url, false);
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
            } else {
                Xrm.Utility.alertDialog(req.statusText);
            }
        }

    }
}
