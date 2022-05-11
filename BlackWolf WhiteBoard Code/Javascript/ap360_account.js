function SetAccountName_onChangePrimaryContact(executionContext) {
    //debugger;
    var formContext = executionContext.getFormContext();
    var primarycontact = formContext.getAttribute("primarycontactid").getValue();
    if (primarycontact != null) {
        primarycontact = primarycontact[0].name;
        formContext.getAttribute("name").setValue(primarycontact);
    }
}

function OnLoad(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    FilterContact(formContext);
    if (formContext.ui.getFormType() != 1)//Create
        openForm(executionContext);
}

function FilterContact(formContext) {
    formContext.getControl("primarycontactid").addPreSearch(addContactsPreSearchFilter);
}

function addContactsPreSearchFilter() {
    var accountId = Xrm.Page.data.entity.getId();
    var filter = "<filter type=\"and\">" +
        "<condition attribute=\"parentcustomerid\" operator=\"eq\"   value=\"" + accountId + "\" />" +
        "</filter>";
    Xrm.Page.getControl("primarycontactid").addCustomFilter(filter);
}


function SaveAndNew() {
    Xrm.Page.data.save().then(OpenNewForm, function () { });
}

function OpenNewForm() {
    debugger;
    var parameters = {};

    //if (Xrm.Page.getAttribute("opportunityid").getValue() != null) {
    //    parameters.opportunityid = Xrm.Page.getAttribute("opportunityid").getValue()[0].id;
    //    parameters.opportunityidname = Xrm.Page.getAttribute("opportunityid").getValue()[0].name;
    //}

    Xrm.Utility.openEntityForm("account", null, parameters);
}
function openForm(executionContext) {
    var formContext = executionContext.getFormContext();
    var customertypecode = formContext.getAttribute("customertypecode").getValue();
    var formtype = "BW Sales Vendor Form";
    if (customertypecode == 3)//Customer
    {
        formtype = "BW Sales Client Account Form";
    }
    else if (customertypecode == 11)//Vendor
    {
        formtype = "BW Sales Vendor Form";
    }

    if (formContext.ui.formSelector.getCurrentItem().getLabel() != formtype) {
        var items = formContext.ui.formSelector.items.get();
        for (var i in items) {
            var item = items[i];
            var itemId = item.getId();
            var itemLabel = item.getLabel()
            if (itemLabel == formtype) {
                //navigate to the form
                item.navigate();
            }
        }
    }
}