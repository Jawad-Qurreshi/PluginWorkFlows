// JavaScript source code
function onLoad(executionContext) {
    var formContext = executionContext.getFormContext();
    var ap360_bookingmistakeadjustment = formContext.getAttribute("ap360_bookingmistakeadjustment").getValue();
    if (ap360_bookingmistakeadjustment) {
        Xrm.Page.ui.tabs.get("TASK_TAB").sections.get("TASK").setVisible(false);
        Xrm.Page.ui.tabs.get("TASK_TAB").sections.get("Description").setVisible(false);
    } else if (ap360_bookingmistakeadjustment == false) {
        Xrm.Page.ui.tabs.get("TASK_TAB").sections.get("BookingMistakeAdjustment").setVisible(false);
    }
}


function setPartTeamAsOwner(executionContext) {

    debugger;
    var formContext = executionContext.getFormContext();

    var formtype = formContext.ui.getFormType();
    if (formtype == 1) {
        var lookupId = "c2f2ae20-96ca-ea11-a812-000d3a33f47e";// Parts Team Guid
        var LookupName = "Parts Team";
        var lookupValue = new Array();
        lookupValue[0] = new Object();
        lookupValue[0].id = lookupId; // GUID of the lookup id
        lookupValue[0].name = LookupName; // Name of the lookup
        lookupValue[0].entityType = "team"; //Entity Type of the lookup entity
        formContext.getAttribute("ownerid").setValue(lookupValue);
        //Xrm.Page.getAttribute("ap360_bookingclassification").setValue(lookupValue);
    }
}

