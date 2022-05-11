function onLoad(executionContext) {
    makeFormReadOnlyonQuoteWon(executionContext);
    var formContext = executionContext.getFormContext();
    filterQuoteService(executionContext);
    var ap360_servicerole = formContext.getAttribute("ap360_serviceroleid").getValue();
    formContext.getControl("statuscode").setDisabled(false);

    showReport(executionContext);
}
function showReport(executionContext) {
    var formContext = executionContext.getFormContext();

    var iframeObject = formContext.getControl("IFRAME_IFRAME_QuoteServiceCalculation");

    if (iframeObject != null) {
        var strURL = formContext.context.getClientUrl() + "/crmreports/viewer/viewer.aspx?id=8a678f2e-af3d-ec11-8c63-000d3a5b2d10&action=run&context=records&recordstype=10299&records="
            + formContext.data.entity.getId()
            + "&helpID=QuoteService%20Calculation.rdl";

        //Set URL of iframe
        iframeObject.setSrc(strURL);
    }
}

function SetParentServiceTask(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();


    var childservicetemplate = formContext.getAttribute("ap360_childservicetemplateid").getValue();

    if (childservicetemplate != null) {

        setQuoteServiceType(executionContext, childservicetemplate[0].id)
        Xrm.WebApi.retrieveRecord("ap360_servicetemplatetype",
            childservicetemplate[0].id,
            "?$select=_ap360_parentservicetaskid_value")
            .then(function (serviceOperation) {
                debugger;
                var lookupValue = new Array();
                lookupValue[0] = new Object();
                lookupValue[0].id = serviceOperation._ap360_parentservicetaskid_value;
                lookupValue[0].name = serviceOperation["_ap360_parentservicetaskid_value@OData.Community.Display.V1.FormattedValue"];
                lookupValue[0].entityType = "msdyn_servicetasktype";

                formContext.getAttribute("ap360_parentservicetaskid").setValue(lookupValue);
            });
    }
    else {

        formContext.getAttribute("ap360_parentservicetaskid").setValue(null);

    }
}

function setQuoteServiceType(executionContext, parentservicetaskid) {
    debugger;
    var formContext = executionContext.getFormContext();



    if (parentservicetaskid != null) {

        Xrm.WebApi.retrieveRecord("ap360_servicetemplatetype",
            parentservicetaskid,
            "?$select=ap360_workordertype")
            .then(function (result) {
                debugger;
                var ap360_workordertype = result["ap360_workordertype"];
                var ap360_workordertype_formatted = result["ap360_workordertype@OData.Community.Display.V1.FormattedValue"];

                formContext.getAttribute("ap360_quoteservicetype").setValue(ap360_workordertype);
            });
    }
    else {

        formContext.getAttribute("ap360_quoteservicetype").setValue(null);

    }
}
function SetServiceRole(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();


    var childservicetemplate = formContext.getAttribute("ap360_childservicetemplateid").getValue();

    if (childservicetemplate != null) {

        Xrm.WebApi.retrieveRecord("ap360_servicetemplatetype",
            childservicetemplate[0].id,
            "?$select=_ap360_servicerole_value")
            .then(function (serviceOperation) {
                if (serviceOperation != null) {
                    debugger;
                    var lookupValue = new Array();
                    lookupValue[0] = new Object();
                    lookupValue[0].id = serviceOperation._ap360_servicerole_value;
                    lookupValue[0].name = serviceOperation["_ap360_servicerole_value@OData.Community.Display.V1.FormattedValue"];
                    lookupValue[0].entityType = "bookableresourcecategory";
                    setServiceRoleHorlyRate(lookupValue[0].id, formContext);
                    formContext.getAttribute("ap360_serviceroleid").setValue(lookupValue);

                }
                else {
                    Xrm.Utility.alertDialog("Service Role not exists for selected Child Service Template Type ");
                }
            });
    }
    else {

        formContext.getAttribute("ap360_serviceroleid").setValue(null);
        formContext.getAttribute("ap360_hourlyrate").setValue(null);

    }

}

//function setServiceRoleHorlyRate(serviceRoleId, formContext) {

//    var HourlyRate = 0;
//    Xrm.WebApi.retrieveRecord("bookableresourcecategory",
//        serviceRoleId,
//        "?$select=ap360_price")
//        .then(function (result) {
//            if (result != null) {
//                debugger;

//                formContext.getAttribute("ap360_hourlyrate").setValue(result["ap360_price"]);

//            }
//            else {
//                alert("Horly Rate is 0");
//            }
//        });
//    return HourlyRate;

//}
function setServiceRoleHorlyRate(serviceRoleId, formContext) {

    var HourlyRate = 0;
    var ap360_price = null;
    var req = new XMLHttpRequest();
    req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/bookableresourcecategories(" + serviceRoleId + ")?$select=ap360_price", true);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
    req.onreadystatechange = function () {
        if (req.readyState === 4) {
            req.onreadystatechange = null;
            if (req.status === 200) {
                var result = JSON.parse(req.response);
                ap360_price = result["ap360_price"];
                var ap360_price_formatted = result["ap360_price@OData.Community.Display.V1.FormattedValue"];
                if (ap360_price != null) {
                    debugger;
                    formContext.getAttribute("ap360_hourlyrate").setValue(ap360_price);
                }
                else {
                    Xrm.Utility.alertDialog("Horly Rate is 0");
                }
            }

        }
        else {
            Xrm.Utility.alertDialog(reqthis.statusText);
        }
    }
    req.send();
    return HourlyRate;

};


function SetGGParentAndGParent_onchangeChildServiceTemplate(executionContext) {
    var formContext = executionContext.getFormContext();
    debugger;
    var ap360_childservicetemplateid = formContext.getAttribute("ap360_childservicetemplateid").getValue();
    if (ap360_childservicetemplateid != null) {
        ap360_childservicetemplateid = ap360_childservicetemplateid[0].id.replace('{', '').replace('}', '');
        var req = new XMLHttpRequest();
        var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ap360_servicetemplatetypes(" + ap360_childservicetemplateid + ")?$select=_ap360_ggparentproductid_value,_ap360_gparentproductid_value,_ap360_serviceproductmappingid_value";
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
                var _ap360_serviceproductmappingid_value = result["_ap360_serviceproductmappingid_value"];
                var _ap360_serviceproductmappingid_value_formatted = result["_ap360_serviceproductmappingid_value@OData.Community.Display.V1.FormattedValue"];
                var _ap360_serviceproductmappingid_value_lookuplogicalname = result["_ap360_serviceproductmappingid_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
                var _ap360_ggparentproductid_value = result["_ap360_ggparentproductid_value"];
                var _ap360_ggparentproductid_value_formatted = result["_ap360_ggparentproductid_value@OData.Community.Display.V1.FormattedValue"];
                var _ap360_ggparentproductid_value_lookuplogicalname = result["_ap360_ggparentproductid_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
                var _ap360_gparentproductid_value = result["_ap360_gparentproductid_value"];
                var _ap360_gparentproductid_value_formatted = result["_ap360_gparentproductid_value@OData.Community.Display.V1.FormattedValue"];
                var _ap360_gparentproductid_value_lookuplogicalname = result["_ap360_gparentproductid_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
                //debugger;
                var lookupValue = new Array();
                lookupValue[0] = new Object();
                lookupValue[0].id = _ap360_serviceproductmappingid_value; // GUID of the lookup id
                lookupValue[0].name = _ap360_serviceproductmappingid_value_formatted; // Name of the lookup
                lookupValue[0].entityType = _ap360_serviceproductmappingid_value_lookuplogicalname; //Entity Type of the lookup entity
                if (lookupValue[0].id != null)
                    formContext.getAttribute("ap360_serviceproductmappingid").setValue(lookupValue);

                var lookupValue = new Array();
                lookupValue[0] = new Object();
                lookupValue[0].id = _ap360_ggparentproductid_value; // GUID of the lookup id
                lookupValue[0].name = _ap360_ggparentproductid_value_formatted; // Name of the lookup
                lookupValue[0].entityType = _ap360_ggparentproductid_value_lookuplogicalname; //Entity Type of the lookup entity
                if (lookupValue[0].id != null)
                    formContext.getAttribute("ap360_ggparentproductid").setValue(lookupValue);

                lookupValue = new Array();
                lookupValue[0] = new Object();
                lookupValue[0].id = _ap360_gparentproductid_value; // GUID of the lookup id
                lookupValue[0].name = _ap360_gparentproductid_value_formatted; // Name of the lookup
                lookupValue[0].entityType = _ap360_gparentproductid_value_lookuplogicalname; //Entity Type of the lookup entity
                if (lookupValue[0].id != null)
                    formContext.getAttribute("ap360_gparentproductid").setValue(lookupValue);


            } else {
                Xrm.Utility.alertDialog(this.statusText);
            }
        }



    } else {
        formContext.getAttribute("ap360_serviceproductmappingid").setValue(null);
        formContext.getAttribute("ap360_ggparentproductid").setValue(null);
        formContext.getAttribute("ap360_gparentproductid").setValue(null);

    }

}
function SetName_onSave(executionContext) {
    var formContext = executionContext.getFormContext();
    //debugger;
    var childservicetemplate = formContext.getAttribute("ap360_childservicetemplateid").getValue();
    if (childservicetemplate != null) {
        childservicetemplate = childservicetemplate[0].name;
    }
    var actualWorkRequested = formContext.getAttribute("ap360_actualworkrequested").getValue();

    if (childservicetemplate != null) {
        formContext.getAttribute("ap360_workrequested").setValue(childservicetemplate + "- " + actualWorkRequested);
    }

}


function FilterChildServieTempalteBasedOnServiceRole(formContext) {

    debugger;
    formContext.getControl("ap360_childservicetemplateid").addPreSearch(addChildServiceTempaltePreSearchFilter);
}

function addChildServiceTempaltePreSearchFilter(formContext) {
    // debugger;
    var ap360_lockedserviceroleforquoteservic = Xrm.Page.data.entity.attributes.get("ap360_lockedserviceroleforquoteservicid").getValue();
    debugger;
    if (ap360_lockedserviceroleforquoteservic != null) {
        var ap360_lockedserviceroleforquoteservicid = ap360_lockedserviceroleforquoteservic[0].id;

        var filter = "<filter type=\"and\">" +
            "<condition attribute=\"ap360_servicerole\" operator=\"eq\"  value=\"" + ap360_lockedserviceroleforquoteservicid + "\" />" +
            "</filter>";

        Xrm.Page.getControl("ap360_childservicetemplateid").addCustomFilter(filter);
    }
}

var opportunityId = null;
function filterQuoteService(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext(); // get formContext
    var ap360_quoteid = formContext.getAttribute("ap360_quoteid").getValue();

    // var recordGuid = formContext.data.entity.getId();
    if (ap360_quoteid == null) return;
    ap360_quoteid = ap360_quoteid[0].id.replace('{', '').replace('}', '');

    var req = new XMLHttpRequest();
    var url = formContext.context.getClientUrl() + "/api/data/v9.1/quotes(" + ap360_quoteid + ")?$select=_opportunityid_value";
    req.open("GET", url, false);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
    req.send();
    if (req.status === 200) {
        var result = JSON.parse(req.response);
        opportunityId = result["_opportunityid_value"];
        FilterQuotes(formContext, opportunityId);
    } else {
        Xrm.Utility.alertDialog(this.statusText);
    }
}

function FilterQuotes(formContext, opportunityId) {

    debugger;
    formContext.getControl("ap360_quoteid").addPreSearch(addquotePreSearchFilter);
}

function addquotePreSearchFilter() {
    // debugger;
    debugger;

    var filter = "<filter type=\"and\">" +
        "<condition attribute=\"opportunityid\" operator=\"eq\"  value=\"" + opportunityId + "\" />" +
        "<condition attribute=\"statecode\" operator=\"not-in\">" +
        "<value>2</value>" +
        "<value>1</value>" +
        "</condition>" +
        "</filter>";
    //Quote status does not equal WON, Active. Closed is already added in Quote Lookup view


    Xrm.Page.getControl("ap360_quoteid").addCustomFilter(filter);

}

function checkChildEntityVendor(executionContext) {
    var formContext = executionContext.getFormContext();
    var status = formContext.getAttribute("statuscode").getValue();
    if (status != 126300000) //126300000 status completed code
        return;

    var quoteServiceId = formContext.data.entity.getId();
    quoteServiceId = quoteServiceId.replace('{', '').replace('}', '');
    var isMessageShown = false;
    var req = new XMLHttpRequest();
    var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ap360_quoteproducts?$select=_ap360_vendorid_value,_ap360_product_value&$filter=_ap360_quoteserviceid_value eq " + quoteServiceId;
    req.open("GET", url, false);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
    req.send();
    var _ap360_vendorid_value = null;

    if (req.readyState === 4) {
        req.onreadystatechange = null;
        if (req.status === 200) {
            var results = JSON.parse(req.response);
            for (var i = 0; i < results.value.length; i++) {
                var ap360_quoteproductid = results.value[i]["ap360_quoteproductid"];
                _ap360_vendorid_value = results.value[i]["_ap360_vendorid_value"];
                var _ap360_product_value = results.value[i]["_ap360_product_value"];
                var _ap360_product_value_formatted = results.value[i]["_ap360_product_value@OData.Community.Display.V1.FormattedValue"];

                var _ap360_vendorid_value_lookuplogicalname = results.value[i]["_ap360_ap360_vendorid_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
                if (_ap360_vendorid_value == null || _ap360_product_value == null) {
                    if (isMessageShown == false) {
                        if (_ap360_product_value == null)
                            Xrm.Utility.alertDialog("Product is not selected in quote product");
                        if (_ap360_vendorid_value == null)
                            Xrm.Utility.alertDialog("Vendor is not selected in quote product " + _ap360_product_value_formatted);
                        isMessageShown = true;
                        var eventArgs = executionContext.getEventArgs();
                        eventArgs.preventDefault();
                    }
                }
            }
            if (isMessageShown != true) {
                checkSubletEntityVendor(executionContext);
            }
        } else {
            Xrm.Utility.alertDialog(this.statusText);
        }
    }

}


function checkSubletEntityVendor(executionContext) {
    var formContext = executionContext.getFormContext();
    var quoteServiceId = formContext.data.entity.getId();
    var isMessageShown = false;
    quoteServiceId = quoteServiceId.replace('{', '').replace('}', '');

    var req = new XMLHttpRequest();
    var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ap360_quotesublets?$select=ap360_subletdescription,_ap360_vendorid_value&$filter=_ap360_quoteserviceid_value eq " + quoteServiceId;
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
            var results = JSON.parse(req.response);
            for (var i = 0; i < results.value.length; i++) {

                var _ap360_vendorid_value = results.value[i]["_ap360_vendorid_value"];
                var _ap360_description_value = results.value[i]["ap360_subletdescription"];
                // var _ap360_product_value_formatted = results.value[i]["_ap360_product_value@OData.Community.Display.V1.FormattedValue"];

                //var _ap360_vendorid_value_lookuplogicalname = results.value[i]["_ap360_ap360_vendorid_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
                if (_ap360_vendorid_value == null) //126300000 status completed code
                {
                    if (isMessageShown == false) {
                        Xrm.Utility.alertDialog("Vendor is not selected in Quote sublet  " + _ap360_description_value);
                        isMessageShown = true;
                        var eventArgs = executionContext.getEventArgs();
                        eventArgs.preventDefault();
                    }

                }
            }
        } else {
            Xrm.Utility.alertDialog(this.statusText);
        }
    }

}

function makeFormReadOnlyonQuoteWon(executionContext) {
    var formContext = executionContext.getFormContext();
    var Quote = formContext.getAttribute("ap360_quoteid").getValue();
    if (Quote != null) {
        Quote = Quote[0].id.replace('{', '').replace('}', '');

        var req = new XMLHttpRequest();
        var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/quotes(" + Quote + ")?$select=statuscode";
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
                var quotestatus = result["statuscode"];
                //var msdyn_systemstatus_formatted = result["msdyn_systemstatus@OData.Community.Display.V1.FormattedValue"];

                if (quotestatus == 3)//Active Submitted to Client
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

function preventAutoSave(executionContext) {
    var eventArgs = executionContext.getEventArgs();
    if (eventArgs.getSaveMode() == 70 ||
        eventArgs.getSaveMode() == 2) {
        eventArgs.preventDefault();
    }
}
function rejectQuoteService_OnClickReject(primaryControl, selectedContorlSelectedItemIds) {
    debugger
    try {
        var formContext = primaryControl; // rename as formContext 
        //var selectedContorlSelectedItemIds = formContext.data.entity.getId();
        Xrm.Utility.showProgressIndicator("Rejecting Quote Service(s)");

        Process.callAction("ap360_RejectQuoteServices",
            [{
                key: "SelectedQuoteServicesGUIDs",
                type: Process.Type.String,
                value: selectedContorlSelectedItemIds
            }],
            function (params) {
                debugger;
                //var isAllWorkOrdersAreConverted = params["isAllWorkOrdersAreConverted"];

                Xrm.Utility.closeProgressIndicator();
                //var subgrid = formContext.ui.controls.get("ap360_quoteservice");
                //subgrid.refresh();
                formContext.data.refresh(true);
                //var quoteServiceSubgrid = formContext.ui.controls.get("quoteservice");
                // quoteServiceSubgrid.refresh();


                Xrm.Utility.closeProgressIndicator();
            },
            function (e, t) {
                Xrm.Utility.closeProgressIndicator();

                Xrm.Utility.alertDialog(e);
                if (window.console && console.error) {
                    console.error(e + "\n" + t);
                }
            });
        //   }
    } catch (e) {

        Xrm.Utility.alertDialog(e.message);

    }
}