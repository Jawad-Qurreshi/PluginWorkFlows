
function OnLoad(executionContext) {
    // debugger;
    var formContext = executionContext.getFormContext();
    var formType = formContext.ui.getFormType();
    if (formType == 1) {
        var lookupData = new Array();
        var lookupItem = new Object();
        lookupItem.id = "361a3eac-749c-4bb3-92a2-d63f692f61ba"; // UOM Each Guid
        lookupItem.name = "Each"; // Entity record name
        lookupItem.entityType = "uom";
        lookupData[0] = lookupItem;
        formContext.getAttribute("msdyn_unit").setValue(lookupData);
    }

}

//test