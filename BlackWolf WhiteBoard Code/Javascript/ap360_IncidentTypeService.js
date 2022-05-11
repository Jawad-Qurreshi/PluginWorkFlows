// JavaScript source code
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

                    formContext.getAttribute("ap360_serviceroleid").setValue(lookupValue);
                }
                else {
                    alert("Service Role not exists for selected Child Service Template Type ");
                }
            });
    }
    else {

        formContext.getAttribute("ap360_serviceroleid").setValue(null);
    }

}

function SetName_onSave(executionContext) {
    var formContext = executionContext.getFormContext();
    //debugger;
    var childservicetemplate = formContext.getAttribute("ap360_childservicetemplateid").getValue();
    if (childservicetemplate != null) {
        childservicetemplate = childservicetemplate[0].name;
    }
    var workRequested = formContext.getAttribute("ap360_workrequested").getValue();

    if (childservicetemplate != null) {
        formContext.getAttribute("msdyn_name").setValue(childservicetemplate + "- " + workRequested);
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

                    formContext.getAttribute("ap360_serviceroleid").setValue(lookupValue);
                }
                else {
                    alert("Service Role not exists for selected Child Service Template Type ");
                }
            });
    }
    else {

        formContext.getAttribute("ap360_serviceroleid").setValue(null);
    }

}
