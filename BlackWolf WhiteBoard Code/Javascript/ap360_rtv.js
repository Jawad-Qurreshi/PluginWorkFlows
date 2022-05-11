// JavaScript source code
function onLoad(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
   
    //formContext.getControl("msdyn_systemstatus").removeOption(126300000);
    //formContext.getControl("msdyn_systemstatus").removeOption(126300001);
    //formContext.getControl("msdyn_systemstatus").removeOption(126300002);

}


function SetShipmentTimeStamp(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var SystemStatus = formContext.getAttribute("msdyn_systemstatus").getValue();
    if (SystemStatus == 690970002)//Shipped
    {
        var currentDateTime = new Date();
        formContext.getAttribute("ap360_shippedtimestamp").setValue(currentDateTime);

    }
   
}