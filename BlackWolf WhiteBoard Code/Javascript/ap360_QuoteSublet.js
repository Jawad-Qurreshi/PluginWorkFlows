function OnLoad(executionContext) {
    var formContext = executionContext.getFormContext();
    FilterVendor(formContext);
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
function SalePrice_OnChangeofEstimatedAmount(executionContext) {

    debugger;
    var formContext = executionContext.getFormContext();
    var estimatedamount = formContext.getAttribute("ap360_estimatedamount").getValue();
    if (estimatedamount != null) {
        formContext.getAttribute("ap360_saleprice").setValue(estimatedamount * 1.3);
    }

}

var quoteid = null;
function filterQuoteService(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext(); // get formContext
    var ap360_quoteserviceid = formContext.getAttribute("ap360_quoteserviceid").getValue();

    // var recordGuid = formContext.data.entity.getId();
    if (ap360_quoteserviceid == null) return;
    ap360_quoteserviceid = ap360_quoteserviceid[0].id.replace('{', '').replace('}', '');

    var req = new XMLHttpRequest();
    var url = formContext.context.getClientUrl() + "/api/data/v9.1/ap360_quoteservices(" + ap360_quoteserviceid + ")?$select=_ap360_quoteid_value,ap360_quoteserviceid";
    req.open("GET", url, false);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
    req.send();
    if (req.status === 200) {
        var result = JSON.parse(req.response);
        quoteid = result["_ap360_quoteid_value"];
        var _ap360_quoteid_value_formatted = result["_ap360_quoteid_value@OData.Community.Display.V1.FormattedValue"];
        var _ap360_quoteid_value_lookuplogicalname = result["_ap360_quoteid_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
        var ap360_quoteserviceid = result["ap360_quoteserviceid"];

        FilterChildServieTempalteBasedOnServiceRole(formContext, quoteid);
    } else {
        Xrm.Utility.alertDialog(this.statusText);
    }
}

function FilterChildServieTempalteBasedOnServiceRole(formContext, quoteid) {

    debugger;
    formContext.getControl("ap360_quoteserviceid").addPreSearch(addquotePreSearchFilter);
}

function addquotePreSearchFilter() {
    // debugger;
    debugger;

    var filter = "<filter type=\"and\">" +
        "<condition attribute=\"ap360_quoteid\" operator=\"eq\"  value=\"" + quoteid + "\" />" +
        "</filter>";

    Xrm.Page.getControl("ap360_quoteserviceid").addCustomFilter(filter);

}


function makeFormReadOnlyonQSCompleted(executionContext) {
    var formContext = executionContext.getFormContext();
    var quoteservice = formContext.getAttribute("ap360_quoteserviceid").getValue();
    if (quoteservice != null) {
        quoteservice = quoteservice[0].id.replace('{', '').replace('}', '');

        var req = new XMLHttpRequest();
        var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ap360_quoteservices(" + quoteservice + ")?$select=statuscode";
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
                var quoteservicestatus = result["statuscode"];
                //var msdyn_systemstatus_formatted = result["msdyn_systemstatus@OData.Community.Display.V1.FormattedValue"];

                if (quoteservicestatus == 126300000)//completed
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

