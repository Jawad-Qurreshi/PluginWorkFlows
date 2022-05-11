function onload(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext(); // get formContext

    var formType = formContext.ui.getFormType();
    if (formType == 1) {
        var lookupDataWarehouse = new Array();
        var lookupItemWarehouse = new Object();
        lookupItemWarehouse.id = "5b743789-c329-41ee-89e5-f81b83570131"; //
        lookupItemWarehouse.name = "Receiving"; // Entity record name
        lookupItemWarehouse.entityType = "msdyn_warehouse";
        lookupDataWarehouse[0] = lookupItemWarehouse;
        formContext.getAttribute("msdyn_warehouse").setValue(lookupDataWarehouse);

    }
}