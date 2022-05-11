function OnLoad(executionContext) {

    debugger;

    var formContext = executionContext.getFormContext();
    //FilterServiceTask(formContext);


    //var ap360_servicerole = formContext.getAttribute("ap360_serviceroleid").getValue();
    //if (ap360_servicerole != null) {
    //    FilterChildServieTempalteBasedOnServiceRole(executionContext);
    //}

}


function FilterServiceTask(formContext) {

    formContext.getControl("ap360_servicetaskid").addPreSearch(addServiceTaskPreSearchFilter);

}


function addServiceTaskPreSearchFilter() {
    debugger;

    var serviceOperationName = null;


    var parentservicetask = Xrm.Page.data.entity.attributes.get("ap360_parentservicetaskid").getValue();

    if (parentservicetask != null) {

        Xrm.WebApi.retrieveRecord("ap360_templatetype",
            parentservicetask[0].id,
            "?$select=_ap360_parentservicetaskid_value")
            .then(function (parentServiceTask) {
                debugger;
                var filter = "<filter type=\"and\">" +
                    "<condition attribute=\"ap360_parentservicetaskid\" operator=\"eq\"  value=\"" + parentServiceTask._ap360_parentservicetaskid_value + "\" />" +
                    "</filter>";
                Xrm.Page.getControl("ap360_servicetaskid").addCustomFilter(filter);
            });

    }




    //var filter = "<filter type=\"and\">" +
    //    "<condition attribute=\"ap360_parentservicetasktype\" operator=\"eq\"  value=\"52288d7c-29ea-e911-a9a7-000d3a11e605\" />" +
    //    "</filter>";

    //Xrm.Page.getControl("ap360_serviceoperation").addCustomFilter(filter);
}



function SetName_onSave(executionContext) {
    var formContext = executionContext.getFormContext();
    debugger;
    var formtype = formContext.ui.getFormType();
    //debugger;
    if (formtype == 1) {
        var servicetask = formContext.getAttribute("ap360_servicetaskid").getValue();
        if (servicetask != null) {
            servicetask = servicetask[0].name;
            var workrequested = formContext.getAttribute("ap360_workrequested").getValue();
            // workrequested = workrequested.split(" ");

            if (workrequested != null)
                formContext.getAttribute("ap360_workrequested").setValue(servicetask + " " + workrequested);
        }
    }

}

function SetParentServiceTask(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();


    var childservicetemplate = formContext.getAttribute("ap360_childservicetemplateid").getValue();

    if (childservicetemplate != null) {

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
function SetServiceRoleAndHourlyRate(executionContext) {
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
                    alert("Service Role not exists for selected Child Service Template Type ");
                }
            });
    }
    else {

        formContext.getAttribute("ap360_serviceroleid").setValue(null);
        formContext.getAttribute("ap360_hourlyrate").setValue(null);

    }

}

function setServiceRoleHorlyRate(serviceRoleId, formContext) {

    var HourlyRate = 0;
    Xrm.WebApi.retrieveRecord("bookableresourcecategory",
        serviceRoleId,
        "?$select=ap360_price")
        .then(function (result) {
            if (result != null) {
                debugger;

                formContext.getAttribute("ap360_hourlyrate").setValue(result["ap360_price"]);

            }
            else {
                alert("Horly Rate is 0");
            }
        });
    return HourlyRate;

}


function RemoveTemplateHierarchy_OnChangeOfRemoveTemplateHierarchy(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var removetemplatehierarchy = formContext.getAttribute("ap360_removetemplatehierarchy").getValue();
    if (removetemplatehierarchy == 1) {
        //var lookupData = new Array();
        //var lookupItem = new Object();
        //lookupItem.id = "62a07066-e3ed-e911-a811-000d3a579c6d"; // Vehicle Product
        //lookupItem.name = "Vehicle"; // Entity record name
        //lookupItem.entityType = "product";
        //lookupData[0] = lookupItem;
        //formContext.getAttribute("ap360_serviceproductmappingid").setValue(lookupData);
        //formContext.getAttribute("ap360_name").setValue("Vehicle");
        //formContext.getAttribute("ap360_description").setValue("Vehicle");

        //formContext.getControl("ap360_ggparent").setVisible(true);
        formContext.getControl("ap360_parentservicetemplateid").setVisible(true);
        formContext.getControl("ap360_servicetemplateid").setVisible(true);
        formContext.getControl("ap360_childservicetemplateid").setVisible(true);
        formContext.getControl("ap360_parentservicetaskid").setVisible(true);
        formContext.getControl("ap360_serviceroleid").setVisible(true);
        formContext.getControl("ap360_hourlyrate").setVisible(true);

        formContext.getAttribute("ap360_parentservicetemplateid").setValue(null);
        formContext.getAttribute("ap360_servicetemplateid").setValue(null);
        formContext.getAttribute("ap360_childservicetemplateid").setValue(null);
        formContext.getAttribute("ap360_parentservicetaskid").setValue(null);
        formContext.getAttribute("ap360_serviceroleid").setValue(null);
        formContext.getAttribute("ap360_hourlyrate").setValue(null);
        //formContext.getAttribute("ap360_lockedserviceroleforquoteservicid").setValue(null);

        formContext.getControl("ap360_removetemplatehierarchy").setDisabled(true);




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
//function FilterChildServieTempalteBasedOnServiceRole(executionContext) {

//    debugger;
//    var formContext = executionContext.getFormContext();

//    formContext.getControl("ap360_servicetemplateid").addPreSearch(addChildServiceTempaltePreSearchFilter);

//    formContext.getControl("ap360_childservicetemplateid").addPreSearch(addChildServiceTempaltePreSearchFilter);
//}

//function addChildServiceTempaltePreSearchFilter(formContext) {
//    // debugger;
//    var ap360_lockedserviceroleforquoteservic = Xrm.Page.data.entity.attributes.get("ap360_lockedserviceroleforquoteservicid").getValue();
//    debugger;
//    if (ap360_lockedserviceroleforquoteservic != null) {
//        var ap360_lockedserviceroleforquoteservicid = ap360_lockedserviceroleforquoteservic[0].id;

//        var filter = "<filter type=\"and\">" +
//            "<condition attribute=\"ap360_servicerole\" operator=\"eq\"  value=\"" + ap360_lockedserviceroleforquoteservicid + "\" />" +
//            "</filter>";


//        Xrm.Page.getControl("ap360_servicetemplateid").addCustomFilter(filter);

//        Xrm.Page.getControl("ap360_childservicetemplateid").addCustomFilter(filter);
//    }
//}
function checkIfEstimatedHourIsGreaterThenFour(executionContext) {
    var formContext = executionContext.getFormContext();
    var estimatedHour = formContext.getAttribute("ap360_estimatedtime").getValue();

    if (estimatedHour > 240) {
        Xrm.Page.getControl("ap360_estimatedtime").setNotification("Time cannot be greater then 4 hours", "removeNotificationEstimatedTime")

    }
    else {
        Xrm.Page.getControl("ap360_estimatedtime").clearNotification("removeNotificationEstimatedTime")
    }

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
function SaveAndNewForm(primaryControl) {
    //alert("Working");
    var formContext = primaryControl;
    formContext.data.entity.save();

    var formParameters = {};
    var entityOptions = {};
    entityOptions["entityName"] = "ap360_quoteservicetask";

    var removetemplatehierarchy = formContext.getAttribute("ap360_removetemplatehierarchy").getValue();
    formParameters["ap360_removetemplatehierarchy"] = removetemplatehierarchy;
    var parentservicetemplate = formContext.getAttribute("ap360_parentservicetemplateid").getValue();
    if (parentservicetemplate != null) {
        formParameters["ap360_parentservicetemplateid"] = parentservicetemplate;
    }
    var servicetemplate = formContext.getAttribute("ap360_servicetemplateid").getValue();
    if (servicetemplate != null) {
        formParameters["ap360_servicetemplateid"] = servicetemplate;
    }
    var childservicetemplate = formContext.getAttribute("ap360_childservicetemplateid").getValue();
    if (childservicetemplate != null) {
        formParameters["ap360_childservicetemplateid"] = childservicetemplate;
    }
    var parentservicetask = formContext.getAttribute("ap360_parentservicetaskid").getValue();
    if (parentservicetask != null) {
        formParameters["ap360_parentservicetaskid"] = parentservicetask;
    }
    var servicerole = formContext.getAttribute("ap360_serviceroleid").getValue();
    if (servicerole != null) {
        formParameters["ap360_serviceroleid"] = servicerole;
    }
    var quoteservice = formContext.getAttribute("ap360_quoteserviceid").getValue();
    if (quoteservice != null) {
        formParameters["ap360_quoteserviceid"] = quoteservice;


        Xrm.Navigation.openForm(entityOptions, formParameters).then(
            function (lookup) { console.log("Success"); },
            function (error) { console.log("Error"); });
    }

    else {


        alert("Can not use 'Save & New' because Quote Service is not selected");
    }




}
