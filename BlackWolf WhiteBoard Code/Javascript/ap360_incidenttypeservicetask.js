function SetName_onSave(executionContext) {
    var formContext = executionContext.getFormContext();
    //debugger;
    var servicetask = formContext.getAttribute("ap360_servicetaskid").getValue();
    if (servicetask != null) {
        servicetask = servicetask[0].name;

    }
    var workrequested = formContext.getAttribute("ap360_workrequested").getValue();
    if (workrequested == null) {
        formContext.getAttribute("msdyn_name").setValue(servicetask);
    } else {
        formContext.getAttribute("msdyn_name").setValue(servicetask + " " + workrequested);
    }
}