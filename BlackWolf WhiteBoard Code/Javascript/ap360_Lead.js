var globalFormContext;
function onload_Lead(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    globalFormContext = formContext;
    formContext.getAttribute("parentcontactid").addOnChange(onChange_ContactField);

}

function onChange_Customer(executionContext) {
    debugger;
    var formcontext = executionContext.getFormContext();
    var customerid = formcontext.getAttribute("customerid").getValue();
    if (customerid != null) {

        if (customerid[0].entityType == "account") {
            var setLookup = new Array();
            setLookup[0] = new Object();
            setLookup[0].id = customerid[0].id;
            setLookup[0].name = customerid[0].name;
            setLookup[0].entityType = customerid[0].entityType;
            formcontext.getAttribute("parentaccountid").setValue(setLookup);//Map Account field

            onChange_Account(executionContext);

            FilterContact(executionContext);//Inorder to filter contact
            FilterVehicle(executionContext);//Inorder to filter vehicle 

        } else if (customerid[0].entityType == "contact") {
            var setLookup = new Array();
            setLookup[0] = new Object();
            setLookup[0].id = customerid[0].id;
            setLookup[0].name = customerid[0].name;
            setLookup[0].entityType = customerid[0].entityType;
            formcontext.getAttribute("parentcontactid").setValue(setLookup);
            setLookupViewByName(customerid[0].id.replace('{', '').replace('}', ''), formcontext);//retreive related account and map
            FilterVehicle(executionContext);//Inorder to filter vehicle 
            onChange_ContactField(executionContext);
        }

    } else {
        formcontext.getAttribute("parentcontactid").setValue(null);
        formcontext.getAttribute("parentaccountid").setValue(null);
        formcontext.getAttribute("ap360_vehicleid").setValue(null);
    }
}


function setLookupViewByName(contactid, formcontext) {
    debugger;
    var req = new XMLHttpRequest();
    req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v8.2/contacts?$select=_parentcustomerid_value&$filter=contactid eq '" + contactid + "'", true);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
    req.onreadystatechange = function () {
        if (this.readyState === 4) {
            req.onreadystatechange = null;
            if (this.status === 200) {
                var results = JSON.parse(this.response);
                if (results.value.length > 0) {
                    var setLookup = new Array();
                    setLookup[0] = new Object();
                    setLookup[0].id = results.value[0]["_parentcustomerid_value"];
                    setLookup[0].name = results.value[0]["_parentcustomerid_value@OData.Community.Display.V1.FormattedValue"];
                    setLookup[0].entityType = results.value[0]["_parentcustomerid_value@Microsoft.Dynamics.CRM.lookuplogicalname"];


                    formcontext.getAttribute("parentaccountid").setValue(setLookup);

                } else {
                    Xrm.Utility.alertDialog("Related account does not exists.");
                }
            } else {
                Xrm.Utility.alertDialog(this.statusText);
            }
        }
    };
    req.send();
}

// Contact filteration functions
function FilterContact(executionContext) {

    debugger;
    var formContext = executionContext.getFormContext();

    formContext.getControl("parentcontactid").addPreSearch(addContactPreSearchFilter);
}
function addContactPreSearchFilter() {
    debugger;
    var parentaccountid = Xrm.Page.data.entity.attributes.get("parentaccountid").getValue();
    debugger;
    if (parentaccountid != null) {
        var parrentAccountId = parentaccountid[0].id;
        parrentAccountId = parrentAccountId.trim();

        var filter = "<filter type=\"and\">" +
            "<condition attribute=\"parentcustomerid\" operator=\"eq\"  value=\"" + parrentAccountId + "\" />" +
            "</filter>";

        Xrm.Page.getControl("parentcontactid").addCustomFilter(filter);
    }
}

// vehicle filteration functions
function FilterVehicle(executionContext) {

    debugger;
    var formContext = executionContext.getFormContext();

    formContext.getControl("ap360_vehicleid").addPreSearch(addVehiclePreSearchFilter);
}
function addVehiclePreSearchFilter() {
    debugger;
    var parentaccountid = Xrm.Page.data.entity.attributes.get("parentaccountid").getValue();
    
    if (parentaccountid != null) {
        var vehicileId = parentaccountid[0].id;
        vehicileId = vehicileId.trim();

        var filter = "<filter type=\"and\">" +
            "<condition attribute=\"ap360_accountid\" operator=\"eq\"  value=\"" + vehicileId + "\" />" +
            "</filter>";

        Xrm.Page.getControl("ap360_vehicleid").addCustomFilter(filter);
    }
}

function onChange_Account(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var parentAccount = formContext.getAttribute("parentaccountid").getValue();
    if (parentAccount != null) {
        formContext.getAttribute("lastname").setValue(parentAccount[0].name);
    }
    else {
        formContext.getAttribute("lastname").setValue(null);

    }
}


function qualifyLead(primaryControl) {
    debugger;
    var formContext = primaryControl;
   
    var parentaccountid = formContext.getAttribute("parentaccountid").getValue();
    var parentcontactid = formContext.getAttribute("parentcontactid").getValue();
    var vehicleid = formContext.getAttribute("ap360_vehicleid").getValue();

    if (parentaccountid == null || parentcontactid == null || vehicleid == null) {
        Xrm.Utility.alertDialog("Please enter required field");
        if (parentaccountid == null) {
            formContext.getAttribute("parentaccountid").setRequiredLevel("required");
            //formContext.getAttribute("parentaccountid").setRequiredLevel("required");
            //formContext.getControl("parentaccountid").setNotification("Please select or create account", 1);
        } 
        if (parentcontactid == null) {
            formContext.getAttribute("parentcontactid").setRequiredLevel("required");
        } 
        if (vehicleid == null) {
            formContext.getAttribute("ap360_vehicleid").setRequiredLevel("required");
        } 
    }else {
        Mscrm.LeadCommandActions.qualifyLeadQuick();
    }

}



//function onChange_ExistingCustomerField(executionContext) {
//    onChange_ParentAccountField(executionContext);
//    var formContext = executionContext.getFormContext();
//    var isexistingcustomer = formContext.getAttribute("ap360_existingcustomer").getValue();
//    if (isexistingcustomer) {
//        formContext.ui.tabs.get("Summary").sections.get("existingcustomersection").setVisible(true);
//        formContext.ui.tabs.get("Summary").sections.get("contactdetailsection").setVisible(false);
//        formContext.ui.tabs.get("Summary").sections.get("company").setVisible(false);
//        // formContext.ui.tabs.get("Summary").sections.get("vehicle").setVisible(false);
//    }
//    else {

//        formContext.getAttribute("parentaccountid").setValue(null);
//        formContext.getAttribute("parentcontactid").setValue(null);
//        formContext.getAttribute("ap360_vehicleid").setValue(null);
//        formContext.ui.tabs.get("Summary").sections.get("existingcustomersection").setVisible(false);
//        formContext.ui.tabs.get("Summary").sections.get("contactdetailsection").setVisible(true);
//        formContext.ui.tabs.get("Summary").sections.get("company").setVisible(true);
//        // formContext.ui.tabs.get("Summary").sections.get("vehicle").setVisible(true);

//    }

//}
function onChange_ParentAccountField(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var parentaccount = formContext.getAttribute("parentaccountid").getValue();
    if (parentaccount != null) {
        formContext.getAttribute("companyname").setValue(null);
    }
    else {
        formContext.getAttribute("parentcontactid").setValue(null);
        formContext.getAttribute("ap360_vehicleid").setValue(null);
        onChange_ContactField(executionContext);
    }
}
function onChange_ContactField(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var parentcontact = formContext.getAttribute("parentcontactid").getValue();
    if (parentcontact != null) {
        var contactName = parentcontact[0].name;
        formContext.getAttribute("lastname").setValue(contactName);
    }
    else {
        formContext.getAttribute("ap360_vehicleid").setValue(null);
        formContext.getAttribute("firstname").setValue(null);
        formContext.getAttribute("lastname").setValue(null);
        formContext.getAttribute("jobtitle").setValue(null);
        formContext.getAttribute("mobilephone").setValue(null);
        formContext.getAttribute("telephone1").setValue(null);
        formContext.getAttribute("emailaddress1").setValue(null);
    }
}



//function Year_OnChange(executionContext) {
//    var formContext = executionContext.getFormContext();
//    var year = formContext.getAttribute("ap360_year").getValue();
//    if (year != null) {
//        formContext.getControl("ap360_year").clearNotification(2);
//    }
//}



