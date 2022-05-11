// JavaScript source code


function onLoad(executionContext) {
    var formContext = executionContext.getFormContext(); // get formContext
    var formtype = formContext.ui.getFormType();
    if (formtype == 1) {
        formContext.getControl("ap360_shippingfee").setDisabled(true);
    }
    makeStageFieldsReadOnly(executionContext);
    var process = formContext.data.process;
    process.addOnPreStageChange(function (formContext) {
        myProcessStateOnPreChange(formContext);
    });
}

function myProcessStateOnPreChange(executionContext) {

    var formContext = executionContext.getFormContext();
    var process = formContext.data.process;
    var eventArgs = executionContext.getEventArgs();
    var stageName = process.getSelectedStage().getName().toString().toLowerCase();

    if (stageName == "submit purchase order") {
        var systemstatus = formContext.getAttribute("msdyn_systemstatus").getValue();
        var paymentmethod = formContext.getAttribute("ap360_paymentmethod").getValue();
        if (systemstatus != 690970001) // submitted
        {
            if (eventArgs._direction != 1) //Which means it is moving forward backwards
            {
                eventArgs.preventDefault();
                Xrm.Utility.alertDialog("Purchase Order status must be submitted");
                return;
            }

        }
        if (paymentmethod == null) {
            if (eventArgs._direction != 1) //Which means it is moving forward backwards
            {
                eventArgs.preventDefault();
                Xrm.Utility.alertDialog("Please select Payment Method");
                return;
            }
        }
    }
    if (stageName == "purchase order status") {
        var substatus = formContext.getAttribute("msdyn_substatus").getValue();
        if (substatus == null) {
            if (eventArgs._direction != 1) //Which means it is moving forward backwards
            {
                eventArgs.preventDefault();
                Xrm.Utility.alertDialog("Create PO Receipt to receive Purcahse Order Products");
                return;
            }
        }
        else if (substatus != null) {
            var substatusId = substatus[0].id.replace('{', '').replace('}', '');
            if (substatusId.toLowerCase() == "10a254fa-6eba-eb11-8236-000d3a37f2ae" || substatusId.toLowerCase() == "ce2e9f0c-6fba-eb11-8236-000d3a37f2ae" || substatusId.toLowerCase() == "c224e61a-6fba-eb11-8236-000d3a37f2ae") // Draft-Billed on Account || Draft-COD || Draft-Pre-Paid
            {
                if (eventArgs._direction != 1) //Which means it is moving forward backwards
                {
                    eventArgs.preventDefault();
                    Xrm.Utility.alertDialog("Create PO Receipt to receive Purcahse Order Products");
                    return;
                }
            }
        }
    }
    if (stageName == "purchase order received") {
        var substatus = formContext.getAttribute("msdyn_substatus").getValue();
        if (substatus != null)
            var substatusId = substatus[0].id.replace('{', '').replace('}', '');
        if (substatusId.toLowerCase() != "765227fd-267d-eb11-a812-0022480299f1") // Received
        {
            if (eventArgs._direction != 1) //Which means it is moving forward backwards
            {
                eventArgs.preventDefault();
                Xrm.Utility.alertDialog("Cannot move to next stage until all Purchase Order Porducts received. Create new Receipt to receive remaining Purchase Order Products");
                return;
            }
        }
    }
}

function makeStageFieldsReadOnly(executionContext) {
    var formContext = executionContext.getFormContext();

    // formContext.getControl("header_process_msdyn_substatus").setDisabled(true);
    // formContext.getControl("header_process_msdyn_substatus_1").setDisabled(true);
    //formContext.getControl("header_process_msdyn_systemstatus_1").setDisabled(true);
}

function setDateStamped(executionContext) {

    var formContext = executionContext.getFormContext(); // get formContext

    var amountPaid = null;
    if (formContext.getAttribute("ap360_amountpaid") != null) {//PO Status
        amountPaid = formContext.getAttribute("ap360_amountpaid").getValue();
        if (amountPaid != null) {
            var currentDateTime = new Date();


            formContext.getAttribute("ap360_datestamped").setValue(currentDateTime);
        }
    }
    else
        formContext.getAttribute("ap360_datestamped").setValue(null);

}

