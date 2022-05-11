// JavaScript source code
// JavaScript source code
// JavaScript source code

var formContext = null;
var executionContext = null;
var previousBookingStatus = null;
var globalFormContext = null;
var globalExecutionContext = null;

function onLoad(executionContext) {
    globalExecutionContext = executionContext;
    formContext = executionContext.getFormContext(); // get the form context
    globalFormContext = formContext;
    previousBookingStatus = formContext.getAttribute("bookingstatus").getValue();
    mistakeadjustment_OnChange(executionContext);

    //EnsureSrvTaskPercentCompleteAndWOSTStatus_OnChangeOfServicetaskcomplete(executionContext);///Inorder to check master percentages

    formContext.getControl("ap360_workorderservicetaskstatus").removeOption(126300002);//Time Used Up/ Incomplete ST
    formContext.getControl("ap360_workorderservicetaskstatus").removeOption(126300003);//Needs External help/ Expert

    formContext.getControl("ap360_workorderservicetaskstatus").removeOption(126300011);//Awaiting dependency
    formContext.getControl("ap360_workorderservicetaskstatus").removeOption(126300001);//Not Started

    IsDiscoveryReason_OnChange(executionContext);
    ManageDiscoveryReasonOnChangeOfDiscovery(executionContext);
    // Manage100PercentageCompleteOnChangeOfEthicalToBill(executionContext);
    ManageWOSTSubStatusOnChangeOfWOSTStatus(executionContext);
   // ToggleWOSTStatus_onchangeServicePercentSpent(executionContext);
}

function ManageDiscoveryReasonOnChangeOfDiscovery(executionContext) {
    var formContext = executionContext.getFormContext();
    var discovry = formContext.getAttribute("ap360_discovery").getValue();
    if (discovry) {
        formContext.getControl("ap360_discoveryimpact").setVisible(true);
        formContext.getAttribute("ap360_discoveryimpact").setRequiredLevel("required");
        formContext.getControl("ap360_isdiscoveryrequired").setVisible(true);
        //formContext.getAttribute("ap360_required").setRequiredLevel("required");
        formContext.getControl("ap360_discoverytimeestimated").setVisible(true);
        formContext.getAttribute("ap360_discoverytimeestimated").setRequiredLevel("required");
        IsDiscoveryReason_OnChange(executionContext);
    } else {
        formContext.getControl("ap360_discoveryimpact").setVisible(false);
        formContext.getAttribute("ap360_discoveryimpact").setRequiredLevel("none");
        formContext.getControl("ap360_isdiscoveryrequired").setVisible(false);
        //formContext.getAttribute("ap360_required").setRequiredLevel("none");
        formContext.getControl("ap360_discoverytimeestimated").setVisible(false);
        formContext.getAttribute("ap360_discoverytimeestimated").setRequiredLevel("none");
        formContext.getControl("ap360_requiredreason").setVisible(false);
        formContext.getAttribute("ap360_requiredreason").setRequiredLevel("none");
    }

}

function Manage100PercentageCompleteOnChangeOfEthicalToBill(executionContext) {
    var formContext = executionContext.getFormContext();
    var EthicalToBill = globalFormContext.getAttribute("ap360_ethicaltobill100").getValue();
    if (EthicalToBill != null && EthicalToBill != 126300010)//100 
    {
        globalFormContext.getControl("ap360_not100completedreason").setVisible(true);
        globalFormContext.getAttribute("ap360_not100completedreason").setRequiredLevel("required");
    } else if (EthicalToBill == null) {
        globalFormContext.getControl("ap360_not100completedreason").setVisible(false);
        globalFormContext.getAttribute("ap360_ethicaltobill100").setRequiredLevel("required");
    }
    else if (EthicalToBill == 126300010) //100
    {
        globalFormContext.getControl("ap360_not100completedreason").setVisible(false);
        globalFormContext.getAttribute("ap360_not100completedreason").setRequiredLevel("none");
    }
    else {
        //globalFormContextt.getControl("ap360_not100completedreason").setVisible(false);
        globalFormContext.getAttribute("ap360_not100completedreason").setRequiredLevel("none");
    }

}

function IsDiscoveryReason_OnChange(executionContext) {
    var formContext = executionContext.getFormContext();
    var Required = formContext.getAttribute("ap360_isdiscoveryrequired").getValue();
    if (Required) {
        formContext.getControl("ap360_requiredreason").setVisible(true);
        formContext.getAttribute("ap360_requiredreason").setRequiredLevel("required");
    } else {
        formContext.getControl("ap360_requiredreason").setVisible(false);
        formContext.getAttribute("ap360_requiredreason").setRequiredLevel("none");
    }

}

function ManageWOSTSubStatusOnChangeOfWOSTStatus(executionContext) {
    var formContext = executionContext.getFormContext();
    var WOSTStatus = formContext.getAttribute("ap360_workorderservicetaskstatus").getValue();
    if (WOSTStatus == 126300008)//Incomplete- Return
    {
        formContext.getControl("ap360_workorderservicetasksubstatus").setVisible(true);
        formContext.getAttribute("ap360_workorderservicetasksubstatus").setRequiredLevel("required");


        formContext.getControl("ap360_activityduration").setVisible(false);
        formContext.getAttribute("ap360_activityduration").setRequiredLevel("none");

        formContext.getControl("ap360_workorderservicetasksubstatus").clearOptions();

        formContext.getControl("ap360_workorderservicetasksubstatus").addOption({ text: "Lunch", value: 126300001 });
        formContext.getControl("ap360_workorderservicetasksubstatus").addOption({ text: "Personal", value: 126300000 });
        formContext.getControl("ap360_workorderservicetasksubstatus").addOption({ text: "End of Day", value: 126300002 });
        formContext.getControl("ap360_workorderservicetasksubstatus").addOption({ text: "Assist CoWorker", value: 126300003 });
        formContext.getControl("ap360_workorderservicetasksubstatus").addOption({ text: "Waiting on Part", value: 126300005 });
        formContext.getControl("ap360_workorderservicetasksubstatus").addOption({ text: "Efficiency Improvement Opportunity", value: 126300004 });
        formContext.getControl("ap360_workorderservicetasksubstatus").addOption({ text: "Inc- Needs MGR Decision", value: 126300007 });
        formContext.getControl("ap360_workorderservicetasksubstatus").addOption({ text: "Inc-Needs Lead Tech Guidance", value: 126300006 });


    }
    else if (WOSTStatus == 126300005) //inc-needs billed child Service Task
    {

        formContext.getControl("ap360_workorderservicetasksubstatus").clearOptions();

        formContext.getControl("ap360_workorderservicetasksubstatus").setVisible(true);
        formContext.getAttribute("ap360_workorderservicetasksubstatus").setRequiredLevel("required");
        formContext.getControl("ap360_activityduration").setVisible(true);
        formContext.getAttribute("ap360_activityduration").setRequiredLevel("required");


        formContext.getControl("ap360_workorderservicetasksubstatus").addOption({ text: "Poor OOB part fitment", value: 126300009 });
        formContext.getControl("ap360_workorderservicetasksubstatus").addOption({ text: "Incorrect Part Buried", value: 126300011 });
        formContext.getControl("ap360_workorderservicetasksubstatus").addOption({ text: "Planned Time Insufficient", value: 126300008 });
        formContext.getControl("ap360_workorderservicetasksubstatus").addOption({ text: "Defective New Part Provided", value: 126300010 });
        formContext.getControl("ap360_workorderservicetasksubstatus").addOption({ text: "Other - Description Required", value: 126300017 });

    }
    else if (WOSTStatus == 126300006)//inc-needs unbilled child Service Task
    {


        formContext.getControl("ap360_workorderservicetasksubstatus").clearOptions();
        formContext.getControl("ap360_workorderservicetasksubstatus").setVisible(true);
        formContext.getAttribute("ap360_workorderservicetasksubstatus").setRequiredLevel("required");
        formContext.getControl("ap360_activityduration").setVisible(true);
        formContext.getAttribute("ap360_activityduration").setRequiredLevel("required");

        formContext.getControl("ap360_workorderservicetasksubstatus").addOption({ text: "We damaged", value: 126300015 });
        formContext.getControl("ap360_workorderservicetasksubstatus").addOption({ text: "For Clean up", value: 126300016 });
        formContext.getControl("ap360_workorderservicetasksubstatus").addOption({ text: "Misused Allotted Time", value: 126300012 });
        formContext.getControl("ap360_workorderservicetasksubstatus").addOption({ text: "Overestimated Capability", value: 126300013 });
        formContext.getControl("ap360_workorderservicetasksubstatus").addOption({ text: "Should have asked for help sooner", value: 126300014 });
        formContext.getControl("ap360_workorderservicetasksubstatus").addOption({ text: "Other - Description Required", value: 126300017 });

    }
    else {
        formContext.getControl("ap360_activityduration").setVisible(false);
        formContext.getAttribute("ap360_activityduration").setRequiredLevel("none");
        formContext.getControl("ap360_workorderservicetasksubstatus").setVisible(false);
        formContext.getAttribute("ap360_workorderservicetasksubstatus").setRequiredLevel("none");
    }

}


function WOSTSubStatus_OnChange(executionContext) {
    var formContext = executionContext.getFormContext();
    var WOSTSubStatus = formContext.getAttribute("ap360_workorderservicetasksubstatus").getValue();
    if (WOSTSubStatus == 126300017)//Other - Description Required
    {
        formContext.getAttribute("ap360_activitydescription").setRequiredLevel("required");
    }
    else {
        formContext.getAttribute("ap360_activitydescription").setRequiredLevel("none");
    }

}

function mistakeAdjustmentTime_OnChange(executionContext) {
    var formContext = executionContext.getFormContext();
    var endTime = formContext.getAttribute("ap360_mistakeadjustmentendtime").getValue();
    var startTime = formContext.getAttribute("ap360_mistakeadjustmentstarttime").getValue();

    if (endTime != null && startTime != null) {
        formContext.getAttribute("ap360_mistakeadjustmentreason").setRequiredLevel("required");
    } else if (endTime != null) {
        formContext.getAttribute("ap360_mistakeadjustmentreason").setRequiredLevel("required");
    } else if (startTime != null) {
        formContext.getAttribute("ap360_mistakeadjustmentreason").setRequiredLevel("required");
    } else {
        formContext.getAttribute("ap360_mistakeadjustmentreason").setRequiredLevel("none");
    }
}

function mistakeadjustment_OnChange(executionContext) {
    var formContext = executionContext.getFormContext();
    var formItem = formContext.ui.formSelector.getCurrentItem();
    if (formItem != null) {
        var itemId = formItem.getId();
        var itemLabel = formItem.getLabel();
    }
    if (formContext.getAttribute("ap360_mistakeadjustment")) {

        var ap360_mistakeadjustment = formContext.getAttribute("ap360_mistakeadjustment").getValue();
        if (ap360_mistakeadjustment) {
            formContext.getControl("ap360_mistakeadjustmentstarttime").setVisible(true);
            //formContext.getAttribute("ap360_mistakeadjustmentstarttime").setRequiredLevel("required");
            formContext.getControl("ap360_mistakeadjustmentendtime").setVisible(true);
            //formContext.getAttribute("ap360_mistakeadjustmentendtime").setRequiredLevel("required");
            formContext.getControl("ap360_mistakeadjustmentreason").setVisible(true);
            //formContext.getAttribute("ap360_mistakeadjustmentreason").setRequiredLevel("required");
        } else if (ap360_mistakeadjustment == false) {
            formContext.getControl("ap360_mistakeadjustmentstarttime").setVisible(false);
            //formContext.getAttribute("ap360_mistakeadjustmentstarttime").setRequiredLevel("none");
            formContext.getControl("ap360_mistakeadjustmentendtime").setVisible(false);
            // formContext.getAttribute("ap360_mistakeadjustmentendtime").setRequiredLevel("none");
            formContext.getControl("ap360_mistakeadjustmentreason").setVisible(false);
            // formContext.getAttribute("ap360_mistakeadjustmentreason").setRequiredLevel("none");
        }
    }
}

var inProgressBookingCount = 0;

function StartWork(lookupId, LookupName, primaryControl)//Shop
{

    // Set booking Classification
    debugger;
    var formContext = primaryControl;

    var isBookingAlreadyCreated = checkIfBokingIsAlreadyCreatedOrNot(formContext);
    if (isBookingAlreadyCreated == true) {
        Xrm.Utility.alertDialog("You already have booking In-Progress.");
        return;
    }
    Xrm.Utility.showProgressIndicator("Starting Booking...");
    var lookupValue = new Array();
    lookupValue[0] = new Object();
    lookupValue[0].id = lookupId; // GUID of the lookup id
    lookupValue[0].name = LookupName; // Name of the lookup
    lookupValue[0].entityType = "ap360_bookingclassification"; //Entity Type of the lookup entity
    Xrm.Page.getAttribute("ap360_bookingclassification").setValue(lookupValue);
    var date = new Date();
    formContext.getAttribute("ap360_inprogresstime").setValue(date);
    if (lookupId == "1e174c8a-a858-ea11-a811-000d3a30f257") // Shop
    {

        //  var shoptaskreason = formContext.getAttribute("ap360_shoptaskreason").getValue();
        // if (shoptaskreason == null) {
        formContext.getControl("ap360_shoptaskreason").setVisible(true);
        formContext.getAttribute("ap360_shoptaskreason").setRequiredLevel("required");
        ChangeBookingStatusToInProgress(formContext);
        //  }
    }
    else {
        //   callWorkFlowToStartBooking();

        ChangeBookingStatusToInProgress(formContext);

        //var lookupValue = new Array();
        //lookupValue[0] = new Object();
        //lookupValue[0].id = "20174c8a-a858-ea11-a811-000d3a30f257"; // GUID of the lookup id
        //lookupValue[0].name = "Standard"; // Name of the lookup
        //lookupValue[0].entityType = "ap360_bookingclassification"; //Entity Type of the lookup entity
        //Xrm.Page.getAttribute("ap360_bookingclassification").setValue(lookupValue);
        //Xrm.Page.data.entity.save();

    }

    Xrm.Utility.closeProgressIndicator();
}
function AutoStartWork(primaryControl)//Shop
{

    var shopBookingClassification = "1e174c8a-a858-ea11-a811-000d3a30f257";
    // Set booking Classification
    debugger;
    var formContext = primaryControl;
    var dollarTimeRemaining = formContext.getAttribute("ap360_actualtimeremaining").getValue();
    var BoookingClassification = formContext.getAttribute("ap360_bookingclassification").getValue();
    var msdyn_workorder = formContext.getAttribute("msdyn_workorder").getValue();
    var msdyn_workorderId = msdyn_workorder[0].id.replace('{', '').replace('}', '');

    if (BoookingClassification != null) {
        var BoookingClassificationId = BoookingClassification[0].id.replace('{', '').replace('}', '');
        if (dollarTimeRemaining <= 0 &&
            BoookingClassificationId.toLowerCase() != shopBookingClassification &&
            BoookingClassificationId.toLowerCase() != "1a174c8a-a858-ea11-a811-000d3a30f257")//Shop && Protocol
        {
            var wobwType = getWorkOrderBWType(msdyn_workorderId);
            if (wobwType != null) {
                if (wobwType != 126300008) //Admin , Admin WOST doesn't contain amounts
                {
                    // return;

                    //Admin	126300008
                    Xrm.Utility.alertDialog("Booking can't be started because time exceeded.");

                    var lookupValue = new Array();
                    lookupValue[0] = new Object();
                    lookupValue[0].id = "{0adbf4e6-86cc-4db0-9dbb-51b7d1ed4020}"; // GUID of the lookup id
                    lookupValue[0].name = "Canceled"; // Name of the lookup
                    lookupValue[0].entityType = "bookingstatus"; //Entity Type of the lookup entity

                    //formContext.getAttribute("bookingstatus").setValue(lookupValue);
                    // formContext.save();
                    return;
                }
            }
        }
    }

    var isBookingAlreadyCreated = checkIfBokingIsAlreadyCreatedOrNot(formContext);
    if (isBookingAlreadyCreated == true) {
        Xrm.Utility.alertDialog("You already have booking In-Progress.");
        return;
    }

    Xrm.Utility.showProgressIndicator("Starting Booking...");
    if (msdyn_workorder != null) {
        var ap360_bookingclassification = formContext.getAttribute("ap360_bookingclassification").getValue();
        if (ap360_bookingclassification == null)//If booking classification is not mapped then map booking as per workorder
        {
            var bookingClassification = getWorkOrderType(msdyn_workorderId);
            if (bookingClassification != null) {
                var bookingClassification = bookingClassification.split("/");
                var lookupValue = new Array();
                lookupValue[0] = new Object();
                lookupValue[0].id = bookingClassification[1]; // GUID of the lookup id
                lookupValue[0].name = bookingClassification[0]; // Name of the lookup
                lookupValue[0].entityType = "ap360_bookingclassification"; //Entity Type of the lookup entity
                formContext.getAttribute("ap360_bookingclassification").setValue(lookupValue);

            }
        }

        ap360_bookingclassification = formContext.getAttribute("ap360_bookingclassification").getValue();
        var BoookingClassificationId = ap360_bookingclassification[0].id.replace('{', '').replace('}', '');
        var date = new Date();
        formContext.getAttribute("ap360_inprogresstime").setValue(date);
        if (BoookingClassificationId.toLowerCase() == shopBookingClassification) // Shop
        {

            //  var shoptaskreason = formContext.getAttribute("ap360_shoptaskreason").getValue();
            // if (shoptaskreason == null) {
            formContext.getControl("ap360_shoptaskreason").setVisible(true);
            formContext.getAttribute("ap360_shoptaskreason").setRequiredLevel("required");
            ChangeBookingStatusToInProgress(formContext);
            Xrm.Utility.closeProgressIndicator();
            //  }
        }
        else {
            ChangeBookingStatusToInProgress(formContext);
            Xrm.Utility.closeProgressIndicator();
            // callWorkFlowToStartBooking();
            //var lookupValue = new Array();
            //lookupValue[0] = new Object();
            //lookupValue[0].id = "20174c8a-a858-ea11-a811-000d3a30f257"; // GUID of the lookup id
            //lookupValue[0].name = "Standard"; // Name of the lookup
            //lookupValue[0].entityType = "ap360_bookingclassification"; //Entity Type of the lookup entity
            //Xrm.Page.getAttribute("ap360_bookingclassification").setValue(lookupValue);
            //Xrm.Page.data.entity.save();
        }

    }
    else {
        Xrm.Utility.closeProgressIndicator();
        Xrm.Utility.alertDialog("Booking can't be started, Unknown WorkOrder Type");

    }
}


function ChangeBookingStatusToInProgress(formContext) {
    var lookupValue = new Array();
    lookupValue[0] = new Object();
    lookupValue[0].id = "53f39908-d08a-4d9c-90e8-907fd7bec07d"; // GUID of the lookup id
    lookupValue[0].name = "In Progress"; // Name of the lookup
    lookupValue[0].entityType = "bookingstatus"; //Entity Type of the lookup entity
    formContext.getAttribute("bookingstatus").setValue(lookupValue);
    formContext.data.entity.save();
}

function checkIfBokingIsAlreadyCreatedOrNot(formContext) {


    var isBookingAlreadyCreated = false;
    var globalContext = Xrm.Utility.getGlobalContext();
    resourceId = globalContext.userSettings.userId;
    resourceId = resourceId.replace('{', '').replace('}', '');

    var userId = getUserId(resourceId);


    var req = new XMLHttpRequest();
    var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/bookableresourcebookings?$filter=_bookingstatus_value eq 53f39908-d08a-4d9c-90e8-907fd7bec07d and  _resource_value eq " + userId + "&$count=true";
    req.open("GET", url, false);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
    req.send();

    if (req.status === 200) {
        var results = JSON.parse(req.response);
        //var recordCount = results["@odata.count"];
        if (results.value.length > 0) {
            isBookingAlreadyCreated = true
        }
        //for (var i = 0; i < results.value.length; i++) {
        //    var bookableresourcebookingid = results.value[i]["bookableresourcebookingid"];
        //}
    } else {
        Xrm.Utility.alertDialog(req.statusText);
    }

    return isBookingAlreadyCreated;
}

function getUserId(resourceId) {
    var bookableresourceid;
    var req = new XMLHttpRequest();
    var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/bookableresources?$filter=_userid_value eq " + resourceId + "&$count=true";
    req.open("GET", url, false);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
    req.send();

    if (req.status === 200) {
        var results = JSON.parse(req.response);
        //var recordCount = results["@odata.count"];
        for (var i = 0; i < results.value.length; i++) {
            bookableresourceid = results.value[i]["bookableresourceid"];
        }
        //for (var i = 0; i < results.value.length; i++) {
        //    var bookableresourcebookingid = results.value[i]["bookableresourcebookingid"];
        //}
    } else {
        Xrm.Utility.alertDialog(req.statusText);
    }

    return bookableresourceid;
}

function AdminFollowUpClick(primaryControl)//Shop
{

    // Set booking Classification
    debugger;
    var formContext = primaryControl;
    var generalTab = formContext.ui.tabs.get("generaltab");
    var followup = generalTab.sections.get("followup"); //Get sections
    followup.setVisible(true); //Show Section

    formContext.getControl("ap360_adminfollowup").setVisible(true);
    formContext.getAttribute("ap360_adminfollowup").setValue(true);

    formContext.getControl("ap360_followupdescription").setVisible(true);
    formContext.getAttribute("ap360_followupdescription").setRequiredLevel("required");

    var globalContext = Xrm.Utility.getGlobalContext();
    var currentUser = new Array();
    currentUser[0] = new Object();
    currentUser[0].entityType = "systemuser";
    currentUser[0].id = globalContext.userSettings.userId;
    currentUser[0].name = globalContext.userSettings.userName;

    formContext.getAttribute("ap360_followedupbyid").setValue(currentUser);

}

function AdminUnFollowUpClick(primaryControl)//Shop
{
    debugger;
    var formContext = primaryControl;
    var generalTab = formContext.ui.tabs.get("generaltab");
    var followup = generalTab.sections.get("followup"); //Get sections

    formContext.getControl("ap360_adminfollowup").setVisible(false);
    formContext.getAttribute("ap360_adminfollowup").setValue(false);

    formContext.getControl("ap360_followupdescription").setVisible(false);
    formContext.getAttribute("ap360_followupdescription").setValue(null);
    formContext.getAttribute("ap360_followupdescription").setRequiredLevel("none");

    formContext.getControl("ap360_followedupbyid").setVisible(false);
    formContext.getAttribute("ap360_followedupbyid").setValue(null);

    followup.setVisible(false); //Hide Section
    formContext.data.entity.save();


}

function callWorkFlowToStartBooking() {
    debugger;
    if (Xrm.Page.getAttribute("starttime") != null && Xrm.Page.getAttribute("msdyn_estimatedarrivaltime") != null) {
        var startTime = Xrm.Page.getAttribute("starttime").getValue();
        var estimatedArrivalTime = Xrm.Page.getAttribute("msdyn_estimatedarrivaltime").getValue();

        if (startTime != null && estimatedArrivalTime != null) {
            if (startTime > estimatedArrivalTime) {
                Xrm.Utility.alertDialog("Booking can't be started because Start time cannot be after the estimated arrival time.");

                return;
            }
            else {

                var recordId = Xrm.Page.data.entity.getId();
                var workflowId = "EEAB73D5-2841-4C18-94B5-3C96DF9E6546";
                Process.callWorkflow(workflowId, recordId, callbackFunction, function (e) {
                    Xrm.Utility.alertDialog("An Error occured. Contact CRM Administrator " + e)

                });
            }
        }
    }
}

function starttWork_OnChangeSHOPTaskReason(executionContext) {
    formContext = executionContext.getFormContext(); // get the form context
    debugger;
    var shoptaskreason = formContext.getAttribute("ap360_shoptaskreason").getValue();
    if (shoptaskreason == "126300005")// other
    {
        formContext.getControl("ap360_shoptasknotes").setVisible(true);
        formContext.getAttribute("ap360_shoptasknotes").setRequiredLevel("required");
    }
    else {
        formContext.getControl("ap360_shoptasknotes").setVisible(false);
        formContext.getAttribute("ap360_shoptasknotes").setRequiredLevel("none");
        callWorkFlowToStartBooking();
    }

}
function starttWork_OnChangeSHOPTaskReasonNotes(executionContext) {
    formContext = executionContext.getFormContext(); // get the form context

    debugger;
    callWorkFlowToStartBooking();


}


function callbackFunction() {
    debugger;
    setTimeout(Delay, 500);
}

function Delay() {
    debugger;
    // Xrm.Page.data.entity.save("saveandclose");7
    Xrm.Page.data.entity.save();

}

function NextStep(nextStepAction, primaryControl) {
    debugger;
    var formContext = primaryControl;
    var msdyn_workorder = formContext.getAttribute("msdyn_workorder").getValue();
    if (msdyn_workorder != null) {

        var msdyn_workorderId = msdyn_workorder[0].id.replace('{', '').replace('}', '');

        if (nextStepAction == "needsparts") {
            formContext.getAttribute("ap360_woselectedsubstatus").setValue("needsparts");

        }
        else if (nextStepAction == "needsmoretime") {
            formContext.getAttribute("ap360_woselectedsubstatus").setValue("needsmoretime");
        }
        else if (nextStepAction == "needssublet") {
            formContext.getAttribute("ap360_woselectedsubstatus").setValue("needssublet");

        }
        else if (nextStepAction == "needstechdirection") {
            formContext.getAttribute("ap360_woselectedsubstatus").setValue("needstechdirection");

        }
        else if (nextStepAction == "needsdiscoveryestimated") {
            formContext.getAttribute("ap360_woselectedsubstatus").setValue("needsdiscoveryestimated");

        }
        else if (nextStepAction == "needsmanagerdecision") {
            formContext.getAttribute("ap360_woselectedsubstatus").setValue("needsmanagerdecision");

        }
        FinishWork("nextstep", primaryControl);

    }
}

function Popup() {

    debugger;
    var pageInput = {
        pageType: "webresource",
        webresourceName: "ap360_bookinghtml"
    };
    var navigationOptions = {
        target: 2,
        width: 500, // value specified in pixel
        height: 400, // value specified in pixel
        position: 1
    };
    Xrm.Navigation.navigateTo(pageInput, navigationOptions).then(
        function success() {
            // Run code on success
        },
        function error() {
            // Handle errors
        }
    );

}

function HTMLButtonClick() {
    debugger;
    // window.parent.alert("Hello");

}

function NeedMoreTime(timeInMinutes, primaryControl) {
    var formContext = primaryControl;

    debugger;
    try {
        //Popup();
        // debugger;
        formContext.getAttribute("ap360_extratimerequired").setValue(timeInMinutes);
        formContext.getControl("ap360_extratimerequired").setVisible(true)

        formContext.getAttribute("ap360_needmoretimereason").setValue(null);
        formContext.getControl("ap360_needmoretimereason").setVisible(true)

        formContext.getControl("ap360_needmoretimereason").clearOptions();

        formContext.getControl("ap360_needmoretimereason").addOption({ text: "Incomplete - Needs billed child Service Task", value: 126300005 });
        formContext.getControl("ap360_needmoretimereason").addOption({ text: "Incomplete - Needs Unbilled child Service task", value: 126300006 })

        formContext.getAttribute("ap360_needmoretimesubreason").setValue(null);
        formContext.getControl("ap360_needmoretimesubreason").setVisible(true)


        var date = new Date();
        formContext.getAttribute("ap360_timestampneedmoretime").setValue(date);

        formContext.getAttribute("ap360_needmoretimereason").setRequiredLevel("required");
        //Xrm.Page.data.save().then(ExecuteNeedMoreTimeWorkFlow(), function () { });//
    } catch (e) {
        console.log("NeedMoreTime: " + e);
    }
}

function NeedMoreTimeReason_OnChange(executionContext) {
    var formContext = executionContext.getFormContext();
    var NeedMoreTimeReason = formContext.getAttribute("ap360_needmoretimereason").getValue();


    if (NeedMoreTimeReason == 126300005) //inc-needs billed child Service Task
    {
        formContext.getControl("ap360_needmoretimesubreason").clearOptions();
        formContext.getControl("ap360_needmoretimesubreason").addOption({ text: "Poor OOB part fitment", value: 126300009 });
        formContext.getControl("ap360_needmoretimesubreason").addOption({ text: "Incorrect Part Buried", value: 126300011 });
        formContext.getControl("ap360_needmoretimesubreason").addOption({ text: "Planned Time Insufficient", value: 126300008 });
        formContext.getControl("ap360_needmoretimesubreason").addOption({ text: "Defective New Part Provided", value: 126300010 });
        // formContext.getControl("ap360_needmoretimesubreason").addOption({ text: "Other - Description Required", value: 126300017 });

    }
    else if (NeedMoreTimeReason == 126300006)//inc-needs unbilled child Service Task
    {
        formContext.getControl("ap360_needmoretimesubreason").clearOptions();
        formContext.getControl("ap360_needmoretimesubreason").addOption({ text: "We damaged", value: 126300015 });
        formContext.getControl("ap360_needmoretimesubreason").addOption({ text: "For Clean up", value: 126300016 });
        formContext.getControl("ap360_needmoretimesubreason").addOption({ text: "Misused Allotted Time", value: 126300012 });
        formContext.getControl("ap360_needmoretimesubreason").addOption({ text: "Overestimated Capability", value: 126300013 });
        formContext.getControl("ap360_needmoretimesubreason").addOption({ text: "Should have asked for help sooner", value: 126300014 });
        // formContext.getControl("ap360_needmoretimesubreason").addOption({ text: "Other - Description Required", value: 126300017 });

    }
}

function ExecuteNeedMoreTimeWorkFlow() {
    debugger;
    setTimeout(ExecuteNeedMoreTimeWorkFlow_Fuction, 2000);
}

function ExecuteNeedMoreTimeWorkFlow_Fuction() {
    debugger;
    try {
        // debugger;
        var recordId = Xrm.Page.data.entity.getId();
        var workflowId = "3D21E457-8C60-46D3-8AF1-50CCA598882A";//WF : BRB : NEED MORE TIME
        Process.callWorkflow(workflowId, recordId, function () { Xrm.Page.data.entity.save(); }, function (e) {
            Xrm.Utility.alertDialog("An Error occured. Contact CRM Administrator " + e)
        });

    }
    catch (e) {
        console.log("NeedMoreTime: " + e);
    }
}

function finishBooking(primaryControl) {
    debugger;
    ToggleWOSTStatusMandatoryOnBookingFinish(primaryControl);
    var formContext = primaryControl;
    DoneTimeDivision(primaryControl, "dummy");
}
function UpdateWOToInProgressAndCreateNewBooking(primaryControl)// INC- Return
{
    debugger;
    var formContext = primaryControl;
    DoneTimeDivision(primaryControl, "woinprogressCreateNewBooking");
}

function UpdateWOToInProgress(primaryControl) // WO In Progress
{
    debugger;
    var formContext = primaryControl;
    DoneTimeDivision(primaryControl, "inprogress");
}
function UpdateWOToComplete(primaryControl)//WO Completed
{
    debugger;
    var formContext = primaryControl;
    DoneTimeDivision(primaryControl, "completed");
}




function DoneTimeDivision(primaryControl, NextPrferredAction) {
    debugger;
    var formContext = primaryControl;
    var bookingId = formContext.data.entity.getId();


    //////////////
    var shopBookingClassification = "1e174c8a-a858-ea11-a811-000d3a30f257";
    var isBookingClockOutVerificationRequired = true;
    var BoookingClassification = formContext.getAttribute("ap360_bookingclassification").getValue();
    if (BoookingClassification != null) {
        var BoookingClassificationId = BoookingClassification[0].id.replace('{', '').replace('}', '');
        if (BoookingClassificationId.toLowerCase() == shopBookingClassification)//Shop && Protocol
        {
            isBookingClockOutVerificationRequired = false;
        }
    }

    ////////////////
    var lookupValue = new Array();
    lookupValue[0] = new Object();
    lookupValue[0].id = "{c33410b9-1abe-4631-b4e9-6e4a1113af34}"; // GUID of the lookup id
    lookupValue[0].name = "Closed"; // Name of the lookup
    lookupValue[0].entityType = "bookingstatus"; //Entity Type of the lookup entity
    var currentDateTime = null;

    currentDateTime = new Date();

    if (isBookingClockOutVerificationRequired) {
        var percentageTotal = 0;
        percentageTotal = getBookingTaskPercentComplete(bookingId);
        if (percentageTotal != 100) {
            Xrm.Utility.alertDialog("Please divide time properly. % time spent of all tasks should be equal to 100%")

        }
        else {
            var activitysubject = formContext.getAttribute("ap360_activitysubject").getValue();
            if (activitysubject == null) {
                formContext.getAttribute("ap360_activitysubject").setRequiredLevel("required");
                //  formContext.getControl("ap360_activitysubject").setNotification("Enter Subject", 1);
                //Xrm.Utility.alertDialog("Please enter Subject");
            }
            else {
                //formContext.getControl("ap360_activitysubject").clearNotification(1);

            }

            var EthicalToBill = formContext.getAttribute("ap360_ethicaltobill100").getValue();
            if (EthicalToBill != null && EthicalToBill != 126300010)//100 
            {
                var not100completedreason = formContext.getAttribute("ap360_not100completedreason").getValue();
                if (not100completedreason == null) {
                    formContext.getAttribute("ap360_not100completedreason").setRequiredLevel("required");
                }
            } else if (EthicalToBill == null) {
                formContext.getAttribute("ap360_ethicaltobill100").setRequiredLevel("required");
            }

            var WOSTStatus = formContext.getAttribute("ap360_workorderservicetaskstatus").getValue();
            if (WOSTStatus == null) {
                formContext.getAttribute("ap360_workorderservicetaskstatus").setRequiredLevel("required");
            }

            formContext.getAttribute("bookingstatus").setValue(lookupValue);
            formContext.getAttribute("ap360_finishtime").setValue(currentDateTime);
            if (NextPrferredAction == "woinprogressCreateNewBooking")//Inc-Return
            {
                formContext.getAttribute("ap360_wobwstatus").setValue(126300000);//In Progress
                formContext.getAttribute("ap360_dividetime").setValue(true);//This trigger plugin to create new BRB: CreateNewBRBonODone Function
                formContext.getAttribute("ap360_nextpreferredaction").setValue(126300001);//INC - RETURN

            }
            else if (NextPrferredAction == "inprogress") {
                formContext.getAttribute("ap360_wobwstatus").setValue(126300000);//In Progress

            }
            else if (NextPrferredAction == "completed") {
                formContext.getAttribute("ap360_wobwstatus").setValue(126300001);//Completed

            }

            formContext.data.entity.save();


        }
    }

    else {

        formContext.getAttribute("ap360_workorderservicetaskstatus").setValue(126300008); //Incomplete - Return 
        formContext.getAttribute("ap360_workorderservicetasksubstatus").setValue(126300004); //Efficiency Improvement Opportunity 

        formContext.getAttribute("bookingstatus").setValue(lookupValue);
        formContext.getAttribute("ap360_finishtime").setValue(currentDateTime);
        formContext.data.entity.save();

    }


}

function DivideTime(recordId, workflowId) {

    debugger;

    if (Xrm.Page.getAttribute("ap360_workflowid") != null) {
        Xrm.Page.getAttribute("ap360_workflowid").setValue(workflowId);
        //Xrm.Page.getAttribute("ap360_dividetime").setValue(true);
        Xrm.Page.getAttribute("ap360_isfinished").setValue(true);
        Xrm.Page.data.entity.save();
    }

    //Xrm.Page.ui.tabs.get("tab_divideTime").setVisible(true);
    //Xrm.Page.ui.tabs.get("fstab_general").setVisible(false);
    //var serviceTaskSubGrid = Xrm.Page.ui.controls.get("ap360_subGridBookingTask");
    //if (serviceTaskSubGrid != null)
    //    serviceTaskSubGrid.refresh();
}
function getBookingTaskPercentComplete(bookingId) {
    debugger;
    var percentageTotal = 0;

    var req = new XMLHttpRequest();
    var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ap360_bookingservicetasks?$select=ap360_bookingpercenttimespent,ap360_ismasterbst&$filter=_ap360_bookableresourcebooking_value eq " + bookingId;
    req.open("GET", url, false);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");

    req.send();
    var timespent = 0;

    if (req.status === 200) {

        var results = JSON.parse(req.response);
        for (var i = 0; i < results.value.length; i++) {
            var ap360_bookingpercenttimespent = results.value[i]["ap360_bookingpercenttimespent"];
            var ap360_ismasterbst = results.value[i]["ap360_ismasterbst"];

            if (!ap360_ismasterbst) {

                if (ap360_bookingpercenttimespent == null)
                    ap360_bookingpercenttimespent = 0;
                timespent += ap360_bookingpercenttimespent
            }
            if (ap360_bookingpercenttimespent == null)
                ap360_bookingpercenttimespent = 0;
            percentageTotal = percentageTotal + ap360_bookingpercenttimespent;
        }
    }
    if (timespent == 100) {
        Xrm.Utility.alertDialog("It is mandatory to divide time on master WOST");
        globalExecutionContext.getEventArgs().preventDefault();
    }


    return percentageTotal;
}

function UpdateWorkOrderStatus(woselectedsubstatus) {
    debugger;
    var BookingStatus = null;
    var msdyn_workorder = Xrm.Page.getAttribute("msdyn_workorder").getValue();
    if (msdyn_workorder != null) {
        var msdyn_workorderId = msdyn_workorder[0].id.replace('{', '').replace('}', '');
        var count = 0;


        do {
            count++;
            var workOrderSystemStatus = getWorkOrderSubstatus(msdyn_workorderId);
            if (workOrderSystemStatus != null) {

                if (workOrderSystemStatus == 690970003 || workOrderSystemStatus == 690970000)// Open completed || Open UnScheduled
                {
                    updateWorkOrderEntitySubStatus(workOrderSystemStatus, woselectedsubstatus, msdyn_workorderId);
                    return;


                }
            }
            else if (count == 125) {
                BookingStatus = CheckIsbookingStatusInProgressORScheduled(msdyn_workorderId);
                if (BookingStatus == "53f39908-d08a-4d9c-90e8-907fd7bec07d")//In Progress
                {
                    Xrm.Utility.alertDialog("Work Order SubStatus can't be updated becasue Work Order contain booking(s) with status 'In Progress'  ");

                    return;
                }
                else if (BookingStatus == "f16d80d1-fd07-4237-8b69-187a11eb75f9") //Scheduled
                {
                    return;
                }
            }
            //else if (count == 26) {
            //    alert("Work Order SubStatus can't be updated becasue Work Order is 'Closed Posted' ");
            //    return;

            //}
        }
        while (workOrderSystemStatus != 690970003 || workOrderSystemStatus != 690970000);// Open completed || Open UnScheduled


    }
}
function CheckIsbookingStatusInProgress(msdyn_workorderId) {
    debugger;
    var BookingStatus = null;
    var req = new XMLHttpRequest();
    var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/bookableresourcebookings?$select=_bookingstatus_value&$filter=_bookingstatus_value eq 53f39908-d08a-4d9c-90e8-907fd7bec07d and _msdyn_workorder_value eq " + msdyn_workorderId;
    req.open("GET", url, false);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
    req.send();

    if (req.status === 200) {
        var results = JSON.parse(req.response);
        inProgressBookingCount = results.value.length;
        for (var i = 0; i < results.value.length; i++) {
            var _bookingstatus_value = results.value[i]["_bookingstatus_value"];
            var _bookingstatus_value_formatted = results.value[i]["_bookingstatus_value@OData.Community.Display.V1.FormattedValue"];
            var _bookingstatus_value_lookuplogicalname = results.value[i]["_bookingstatus_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
            if (_bookingstatus_value == "53f39908-d08a-4d9c-90e8-907fd7bec07d" || _bookingstatus_value == "f16d80d1-fd07-4237-8b69-187a11eb75f9")// In Progress || Scheduled 
            {
                BookingStatus = _bookingstatus_value;

            }
        }
    } else {
        Xrm.Utility.alertDialog(this.statusText);
    }

    return BookingStatus;

}

function CheckIsbookingStatusInProgressORScheduled(msdyn_workorderId) {
    debugger;
    var BookingStatus = null;
    var req = new XMLHttpRequest();
    var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/bookableresourcebookings?$select=_bookingstatus_value&$filter=_msdyn_workorder_value eq " + msdyn_workorderId;
    req.open("GET", url, false);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
    req.send();

    if (req.status === 200) {
        var results = JSON.parse(req.response);
        for (var i = 0; i < results.value.length; i++) {
            var _bookingstatus_value = results.value[i]["_bookingstatus_value"];
            var _bookingstatus_value_formatted = results.value[i]["_bookingstatus_value@OData.Community.Display.V1.FormattedValue"];
            var _bookingstatus_value_lookuplogicalname = results.value[i]["_bookingstatus_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
            if (_bookingstatus_value == "53f39908-d08a-4d9c-90e8-907fd7bec07d" || _bookingstatus_value == "f16d80d1-fd07-4237-8b69-187a11eb75f9")// In Progress || Scheduled 
            {
                BookingStatus = _bookingstatus_value;

            }
        }
    } else {
        Xrm.Utility.alertDialog(this.statusText);
    }

    return BookingStatus;

}
function updateWorkOrderEntitySubStatustoOpenCompleted(msdyn_workorderId) {
    debugger;
    var entity = {};

    entity.msdyn_systemstatus = 690970000;//Unscheduled
    entity["msdyn_substatus@odata.bind"] = "/msdyn_workordersubstatuses(46486907-ab05-eb11-a813-000d3a33f47e)";// Completed  - OU

    var req = new XMLHttpRequest();
    var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/msdyn_workorders(" + msdyn_workorderId + ")";
    req.open("PATCH", url, true);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.onreadystatechange = function () {
        if (this.readyState === 4) {
            req.onreadystatechange = null;
            if (this.status === 204) {
                //Success - No Return Data - Do Something
            } else {
                Xrm.Utility.alertDialog(this.statusText);
            }
        }
    };
    req.send(JSON.stringify(entity));

}

function updateWorkOrderEntitySubStatus(workOrderSystemStatus, woselectedsubStatus, msdyn_workorderId) {
    debugger;
    var entity = {};

    if (workOrderSystemStatus == 690970003)//Open completed
    {
        if (woselectedsubStatus == "needsparts") {
            entity["msdyn_substatus@odata.bind"] = "/msdyn_workordersubstatuses(e5c1419c-2b5a-ea11-a811-000d3a30f195)";
        }
        else if (woselectedsubStatus == "needsmoretime") {
            entity["msdyn_substatus@odata.bind"] = "/msdyn_workordersubstatuses(07c2419c-2b5a-ea11-a811-000d3a30f195)";
        }
        else if (woselectedsubStatus == "needssublet") {
            entity["msdyn_substatus@odata.bind"] = "/msdyn_workordersubstatuses(ffc1419c-2b5a-ea11-a811-000d3a30f195)";
        }
        else if (woselectedsubStatus == "needstechdirection") {
            entity["msdyn_substatus@odata.bind"] = "/msdyn_workordersubstatuses(edc1419c-2b5a-ea11-a811-000d3a30f195)";
        }
        else if (woselectedsubStatus == "needsdiscoveryestimated") {
            entity["msdyn_substatus@odata.bind"] = "/msdyn_workordersubstatuses(e3c1419c-2b5a-ea11-a811-000d3a30f195)";
        }
        else if (woselectedsubStatus == "needsmanagerdecision") {
            entity["msdyn_substatus@odata.bind"] = "/msdyn_workordersubstatuses(efc1419c-2b5a-ea11-a811-000d3a30f195)";
        }

    }
    else if (workOrderSystemStatus == 690970000)//Open Unscheduled
    {

        if (woselectedsubStatus == "needsparts") {
            entity["msdyn_substatus@odata.bind"] = "/msdyn_workordersubstatuses(f3c1419c-2b5a-ea11-a811-000d3a30f195)";
        }
        else if (woselectedsubStatus == "needsmoretime") {
            entity["msdyn_substatus@odata.bind"] = "/msdyn_workordersubstatuses(f8b4381a-aaf1-ea11-a815-000d3a33f3c3)";
        }
        else if (woselectedsubStatus == "needssublet") {
            entity["msdyn_substatus@odata.bind"] = "/msdyn_workordersubstatuses(e5f88a2c-aaf1-ea11-a815-000d3a33f3c3)";
        }
        else if (woselectedsubStatus == "needstechdirection") {
            entity["msdyn_substatus@odata.bind"] = "/msdyn_workordersubstatuses(4834e644-aaf1-ea11-a815-000d3a33f3c3)";
        }
        else if (woselectedsubStatus == "needsdiscoveryestimated") {
            entity["msdyn_substatus@odata.bind"] = "/msdyn_workordersubstatuses(39531b57-aaf1-ea11-a815-000d3a33f3c3)";
        }
        else if (woselectedsubStatus == "needsmanagerdecision") {
            entity["msdyn_substatus@odata.bind"] = "/msdyn_workordersubstatuses(4932a669-aaf1-ea11-a815-000d3a33f3c3)";
        }
        //Needs  part(s) - OU                     f3c1419c-2b5a-ea11-a811-000d3a30f195
        //needs more time -OU                     f8b4381a-aaf1-ea11-a815-000d3a33f3c3
        //needs sublet - OU			e5f88a2c-aaf1-ea11-a815-000d3a33f3c3
        //needs lead tech direction - OU		4834e644-aaf1-ea11-a815-000d3a33f3c3
        //needs discorvery authorization - OU	39531b57-aaf1-ea11-a815-000d3a33f3c3
        //needs manager decison -OU		4932a669-aaf1-ea11-a815-000d3a33f3c3
    }
    var req = new XMLHttpRequest();
    var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/msdyn_workorders(" + msdyn_workorderId + ")";
    req.open("PATCH", url, true);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.onreadystatechange = function () {
        if (this.readyState === 4) {
            req.onreadystatechange = null;
            if (this.status === 204) {
                //Success - No Return Data - Do Something
            } else {
                Xrm.Utility.alertDialog(this.statusText);
            }
        }
    };
    req.send(JSON.stringify(entity));

}

function getWorkOrderType(msdyn_workorderId) {
    debugger;
    var bookingClassification = null;
    var req = new XMLHttpRequest();
    var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/msdyn_workorders(" + msdyn_workorderId + ")?$select=ap360_workorderbwtype";
    req.open("GET", url, false);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
    req.send();

    if (req.status === 200) {
        var result = JSON.parse(req.response);
        var ap360_workordertype = result["ap360_workorderbwtype"];
        var ap360_workordertypeFormatedValue = result["ap360_workorderbwtype@OData.Community.Display.V1.FormattedValue"];
        if (ap360_workordertype != null) {
            if (ap360_workordertype == "126300004" ||//Production Mechanical
                ap360_workordertype == "126300005" ||//Production BodyShop
                ap360_workordertype == "126300006" ||//Production Electrical
                ap360_workordertype == "126300000" ||//Assesment- Inspection
                ap360_workordertype == "126300009" ||//Assessment- Diagnostic
                ap360_workordertype == "126300007" ||//Production Upholstery
                ap360_workordertype == "126300008")//Admin
            {
                bookingClassification = "Standard/20174c8a-a858-ea11-a811-000d3a30f257";
            }
            else if (ap360_workordertype == "126300002") {//Protocol	
                bookingClassification = "Protocol/1a174c8a-a858-ea11-a811-000d3a30f257"
            }
            else if (ap360_workordertype == "126300003") {//Shop
                bookingClassification = "Shop/1e174c8a-a858-ea11-a811-000d3a30f257"
            }
            else if (ap360_workordertype == "126300001") {//Development
                bookingClassification = "Development/d261769b-56c2-eb11-bacc-000d3a3345ab"
            }
        }



        //Protocol	126300002 --
        //Assesment- Inspection	126300000 --
        //Assessment- Diagnostic	126300009--
        //Production OLD	126300001 --
        //Production Mechanical	126300004 --
        //Production BodyShop	126300005 --
        //Production Electrical	126300006 --
        //Production Upholstery	126300007 --
        //Shop	126300003 --
        //Admin	126300008
    } else {
        Xrm.Utility.alertDialog(this.statusText);
    }

    return bookingClassification;
}
function getWorkOrderBWType(msdyn_workorderId) {
    debugger;
    var bookingClassification = null;
    var req = new XMLHttpRequest();
    var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/msdyn_workorders(" + msdyn_workorderId + ")?$select=ap360_workorderbwtype";
    req.open("GET", url, false);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
    req.send();

    if (req.status === 200) {
        var result = JSON.parse(req.response);
        var ap360_workordertype = result["ap360_workorderbwtype"];

    } else {
        Xrm.Utility.alertDialog(this.statusText);
    }

    return ap360_workordertype;
}

function getWorkOrderSubstatus(msdyn_workorderId) {
    debugger;
    var workOrderSystemStatus = null;
    var req = new XMLHttpRequest();
    var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/msdyn_workorders(" + msdyn_workorderId + ")?$select=msdyn_systemstatus";
    req.open("GET", url, false);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
    req.send();

    if (req.status === 200) {
        var result = JSON.parse(req.response);
        var msdyn_systemstatus = result["msdyn_systemstatus"];
        var msdyn_systemstatusFormatedValue = result["msdyn_systemstatus@OData.Community.Display.V1.FormattedValue"];
        if (msdyn_systemstatus != null) {
            if (msdyn_systemstatus == "690970003" || msdyn_systemstatus == "690970000") {
                workOrderSystemStatus = msdyn_systemstatus;

            }
        }

    } else {
        Xrm.Utility.alertDialog(this.statusText);
    }

    return workOrderSystemStatus;
}

function updateBookingServiceTask(GUID) {
    debugger;
    var entity = {};
    entity.ap360_createwoservicetasktimestamp = true;

    Xrm.WebApi.online.updateRecord("ap360_bookingservicetask", GUID, entity).then(
        function success(result) {
            var updatedEntityId = result.id;
        },
        function (error) {
            Xrm.Utility.alertDialog(error.message);
        }
    );
}
//entity.ap360_createwoservicetasktimestamp = true;

//var req = new XMLHttpRequest();
//var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ap360_bookingservicetasks(" + bookingseriviceTaskId + ")"
//req.open("PATCH", url, true);
//req.setRequestHeader("OData-MaxVersion", "4.0");
//req.setRequestHeader("OData-Version", "4.0");
//req.setRequestHeader("Accept", "application/json");
//req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
////req.onreadystatechange = function () {
//req.send(JSON.stringify(entity));

//if (req.readyState === 4) {
//    req.onreadystatechange = null;
//    if (req.status === 204) {
//        //Success - No Return Data - Do Something
//        alert("Updated");
//    } else {
//        Xrm.Utility.alertDialog(this.statusText);

//    }
//}
// };


function RunFinishBookingWorkflow(recordId, workflowId) {
    debugger;
    try {

        Process.callWorkflow(workflowId, recordId, function () {
            Xrm.Page.data.entity.save(); Xrm.Page.data.refresh(true);
        }, function (e) { alert("An Error occured. Contact CRM Administrator " + e) });

    }
    catch (e) {
        console.log("RunGenerateWelcomeLetterWorkflow: " + e);
    }
}

function ToggleWOSTStatus_onchangeServicePercentSpent(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var bookingServiceTaskGridRows = Xrm.Page.getControl("bookingservicetask").getGrid().getRows();
    var rowCount = bookingServiceTaskGridRows.getLength();
    var IsMaster = false;
    var WOSTStatus = formContext.getAttribute("ap360_workorderservicetaskstatus").getValue();
    for (var i = 0; i < rowCount; i++) {
        var eachRowBST = bookingServiceTaskGridRows.get(i).getData().getEntity();
        IsMaster = eachRowBST.attributes.get("ap360_ismasterbst").getValue();
        if (IsMaster) {
            var serviceTaskComplete = eachRowBST.attributes.get("ap360_servicetaskcomplete").getValue();
            if (serviceTaskComplete != 126300006)//100% on grid
            {
                var WOSTStatus = formContext.getAttribute("ap360_workorderservicetaskstatus").getValue();
                if (WOSTStatus == 126300010)//Completed
                {
                    formContext.getAttribute("ap360_workorderservicetaskstatus").setValue(null);
                }
            }
        }
    }
    if (WOSTStatus == 126300007)//Incomplete - Needs Discovery Estimated
    {
        formContext.getAttribute("ap360_discovery").setValue(true);
        ManageDiscoveryReasonOnChangeOfDiscovery(executionContext);
    }

}

function ToggleWOSTStatusMandatoryOnBookingFinish(primayControl) {
    debugger;
    var formContext = primayControl;
    var bookingServiceTaskGridRows = Xrm.Page.getControl("bookingservicetask").getGrid().getRows();
    var rowCount = bookingServiceTaskGridRows.getLength();
    var IsMaster = false;
    var WOSTStatus = formContext.getAttribute("ap360_workorderservicetaskstatus").getValue();
    if (WOSTStatus != null) {
        for (var i = 0; i < rowCount; i++) {
            var eachRowBST = bookingServiceTaskGridRows.get(i).getData().getEntity();
            IsMaster = eachRowBST.attributes.get("ap360_ismasterbst").getValue();
            if (IsMaster) {
                var serviceTaskComplete = eachRowBST.attributes.get("ap360_servicetaskcomplete").getValue();
                if (serviceTaskComplete != 126300006)//100% on grid
                {
                    var WOSTStatus = formContext.getAttribute("ap360_workorderservicetaskstatus").getValue();
                    if (WOSTStatus == 126300010)//Completed
                    {
                        formContext.getAttribute("ap360_workorderservicetaskstatus").setValue(null);
                        formContext.getAttribute("ap360_workorderservicetaskstatus").setRequiredLevel("required");
                    }
                }
            }
        }
    }
    else {
        formContext.getAttribute("ap360_workorderservicetaskstatus").setRequiredLevel("required");
    }
    if (WOSTStatus == 126300007)//Incomplete - Needs Discovery Estimated
    {
        formContext.getAttribute("ap360_discovery").setValue(true);
        //ManageDiscoveryReasonOnChangeOfDiscovery(executionContext);
    }

}

function DivideTime_Subgrid_onchangePercentTimeStamp(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var eventArgs = globalExecutionContext.getEventArgs();

    var packageRows = Xrm.Page.getControl("bookingservicetask").getGrid().getRows();
    var rowCount = packageRows.getLength();
    var timespent = 0;
    var totalTimeSpent = 0;
    for (var i = 0; i < rowCount; i++) {
        var rowEntity = packageRows.get(i).getData().getEntity();
        isMaster = rowEntity.attributes.get("ap360_ismasterbst").getValue();
        if (!isMaster) {
            timespent += rowEntity.attributes.get("ap360_bookingpercenttimespent").getValue();
        }
        totalTimeSpent += rowEntity.attributes.get("ap360_bookingpercenttimespent").getValue();
    }
    if (timespent == 100) {
        Xrm.Utility.alertDialog("It is mandatory to divide time on master WOST");

    }
    if (totalTimeSpent > 100) {

        Xrm.Utility.alertDialog("Please divide time properly. % time spent of all tasks should be equal to 100%");

    }
}



function preventInProgressBookingCancelation(executionContext) {
    var formContext = executionContext.getFormContext();
    debugger;
    var bookingstatus = formContext.getAttribute("bookingstatus").getValue();
    if (bookingstatus != null) {
        bookingstatusID = bookingstatus[0].id.replace('{', '').replace('}', '');
        if (bookingstatusID.toLowerCase() == "0adbf4e6-86cc-4db0-9dbb-51b7d1ed4020")//Cancelled
        {
            var previousBookingStatusID = previousBookingStatus[0].id.replace('{', '').replace('}', '');
            if (previousBookingStatusID.toLowerCase() == "53f39908-d08a-4d9c-90e8-907fd7bec07d")//In-Progress
            {
                Xrm.Utility.alertDialog("In-Progress booking cannot be Cancelled");
                formContext.getAttribute("bookingstatus").setValue(previousBookingStatus);
            } else if (previousBookingStatusID.toLowerCase() == "c33410b9-1abe-4631-b4e9-6e4a1113af34")// Closed
            {
                Xrm.Utility.alertDialog("Closed booking cannot be Cancelled");
                formContext.getAttribute("bookingstatus").setValue(previousBookingStatus);
            } else {
                previousBookingStatus = bookingstatus;
            }
        } else {
            previousBookingStatus = bookingstatus;
        }
    }
}

/////////////////////////////////////////////////////////////////////////////////////////////
function SaveSubgrid(primaryControl) {
    primaryControl.data.save();
}
function AdjustTime_EnableRule(primaryControl) {
    debugger;
    var bookingstatus = primaryControl.getAttribute("bookingstatus").getValue();
    if (bookingstatus != null) {
        bookingstatus = bookingstatus[0].id.replace('{', '').replace('}', '');
        // var scheduled = "f16d80d1-fd07-4237-8b69-187a11eb75f9";
        var inProgress = "53f39908-d08a-4d9c-90e8-907fd7bec07d";
        if (bookingstatus.toLowerCase() == inProgress.toLowerCase()) {
            return true;
            primaryControl.data.save();

        } else {
            return false;
            primaryControl.data.save();

        }
    } else {
        return false;
    }
}
function AdjustTimeButton_OnClick(primaryControl) {
    debugger;
    //alert("Functionality Not Implemented, Work In progress");
    var formContext = primaryControl;

    var bookingstatus = Xrm.Page.getAttribute("bookingstatus").getValue();
    if (bookingstatus != null) {
        bookingstatus = bookingstatus[0].id.replace('{', '').replace('}', '');
        //var scheduled = "f16d80d1-fd07-4237-8b69-187a11eb75f9";
        var inProgress = "53f39908-d08a-4d9c-90e8-907fd7bec07d";

        if (bookingstatus.toLowerCase() == inProgress.toLowerCase()) {
            //var starttime = Xrm.Page.getAttribute("starttime").getValue();
            //var msdyn_estimatedarrivaltime = Xrm.Page.getAttribute("msdyn_estimatedarrivaltime").getValue();
            //if (starttime > msdyn_estimatedarrivaltime) {
            //    Xrm.Page.getAttribute("msdyn_estimatedarrivaltime").setValue(starttime);
            //    Xrm.Page.data.entity.save();

            //}     
            var endTime = Xrm.Page.getAttribute("endtime").getValue();
            var msdyn_actualarrivaltime = Xrm.Page.getAttribute("msdyn_actualarrivaltime").getValue();
            if (endTime <= msdyn_actualarrivaltime) {

                var add_minutes = function (dt, minutes) {
                    return new Date(dt.getTime() + minutes * 60000);
                }

                Xrm.Page.getAttribute("endtime").setValue(add_minutes(new Date(endTime), 2));
                Xrm.Page.data.entity.save();

            }


            //endtime

            //   msdyn_actualarrivaltime

        }


    }
}


function DoneButton_EnableRule(primaryControl) {
    debugger;
    var bookingstatus = primaryControl.getAttribute("bookingstatus").getValue();
    if (bookingstatus != null) {
        bookingstatus = bookingstatus[0].id.replace('{', '').replace('}', '');
        var inProgress = "53f39908-d08a-4d9c-90e8-907fd7bec07d";
        if (bookingstatus.toLowerCase() == inProgress.toLowerCase()) {
            return true;
        } else {
            return false;
        }
    } else {
        return false;
    }
}
function FinishWorkButton_EnableRule(primaryControl) {
    debugger;
    var bookingstatus = primaryControl.getAttribute("bookingstatus").getValue();
    if (bookingstatus != null) {
        bookingstatus = bookingstatus[0].id.replace('{', '').replace('}', '');
        var inProgress = "53f39908-d08a-4d9c-90e8-907fd7bec07d";
        if (bookingstatus.toLowerCase() == inProgress.toLowerCase()) {
            return true;
        } else {
            return false;
        }
    } else {
        return false;
    }
}
function RecalulateBookingButton_EnableRule(primaryControl) {
    debugger;
    var bookingstatus = primaryControl.getAttribute("bookingstatus").getValue();
    if (bookingstatus != null) {
        bookingstatus = bookingstatus[0].id.replace('{', '').replace('}', '');
        var closed = "c33410b9-1abe-4631-b4e9-6e4a1113af34";
        if (bookingstatus.toLowerCase() == closed.toLowerCase()) {
            return true;
        } else {
            return false;
        }
    } else {
        return false;
    }
}
function refreshRibbon(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    formContext.ui.refreshRibbon();
}
function StartWorkButton_EnableRule(formContext) {
    debugger;
    var bookingstatus = formContext.getAttribute("bookingstatus").getValue();
    if (bookingstatus != null) {
        bookingstatus = bookingstatus[0].id.replace('{', '').replace('}', '');
        var scheduled = "f16d80d1-fd07-4237-8b69-187a11eb75f9";
        if (bookingstatus.toLowerCase() == scheduled.toLowerCase()) {
            return true;
        } else {
            return false;
        }
    } else {
        return false;
    }
}

function NeedMoreTimeButton_EnableRule(formContext) {
    debugger;
    var bookingstatus = formContext.getAttribute("bookingstatus").getValue();
    if (bookingstatus != null) {
        bookingstatus = bookingstatus[0].id.replace('{', '').replace('}', '');
        var inProgress = "53f39908-d08a-4d9c-90e8-907fd7bec07d";
        var scheduled = "f16d80d1-fd07-4237-8b69-187a11eb75f9";
        if (bookingstatus.toLowerCase() == inProgress.toLowerCase() || bookingstatus.toLowerCase() == scheduled.toLowerCase()) {
            return true;
        } else {
            return false;
        }
    } else {
        return false;
    }
}
function LockBookingServiceTaskSubgridFields(executioncontext) {
    debugger;
    var BRBObject = executioncontext.getFormContext().data.entity;
    var servicetaskcomplete = BRBObject.attributes.getByName("ap360_servicetaskcomplete");
    var val = servicetaskcomplete.getValue();
    if (val == 126300006)//service task comppleted 100%
    {
        LockFieldsOnEditableGrid(executioncontext, ["ap360_completed", "ap360_name", "ap360_ismasterbst"]);
    } else {
        LockFieldsOnEditableGrid(executioncontext, ["ap360_completed", "ap360_name", "ap360_ismasterbst"]);
    }
    //var formContext = executioncontext.getFormContext();

    //var optionSet = BRBObject.getControl("ap360_woststatus");
    //var optionSetValues = optionSet.getAttribute().getOptions(); 
    //var woststatus = BRBObject.attributes.getByName("ap360_woststatus");
    //var wostStatusVal = woststatus.getValue();
    //wostStatusVal.removeOption(126300011);//avaiting dependency

    //// get the attribute which fired the onchange.
    //var nameAttr = executioncontext.getEventSource();
    //// get the container for the attribute.
    //// var attrParent = nameAttr.getParent();
    ///// get the value of the name.
    //// var name = nameAttr.getValue();
    //// var field1 Attribute
    //var field1 = nameAttr.attributes.get("ap360_servicetaskcomplete");
    ////field1.setValue("<default value for field 1>");
    //// set field as mandatory.
    //field1.setRequiredLevel("required");
    //ap360_name
    //ap360_name
}
function EnsureSrvTaskPercentComplete_OnSaveBookingServiceTaskGrid() {
    debugger;
    // var nameAttr = executionContext.getEventSource();
    //var IsCore = formContext.getAttribute("ap360_core").getValue();

    // var formContext = executionContext.getFormContext(); // get the form context
    //formContext.getControl("ap360_percentagecomplete").setVisible(false);
    var gridContext = formContext.getControl("bookingservicetask"); // get the grid context provinding subgrid name
    var gridRows = gridContext.getGrid().getRows();

    //loop through each row to get values of each column
    gridRows.forEach(function (row, i) {
        var gridColumns = row.getData().getEntity().attributes;

        var bookingpercenttimespent = null;
        bookingpercenttimespent = gridColumns.getByName("ap360_bookingpercenttimespent").getValue();
        name = gridColumns.getByName("ap360_name").getValue();

        if (bookingpercenttimespent != null) {
            var servicetaskcomplete = null;
            servicetaskcomplete = gridColumns.getByName("ap360_servicetaskcomplete").getValue();
            if (servicetaskcomplete == null) {
                Xrm.Utility.alertDialog(name + ": 'Service Task % Complete' can't be null. ");

                var saveEvent = gridContext.getEventArgs();
                saveEvent.preventDefault();



            }
        }
        var gridColumns = row.getData().getEntity().attributes;
        //loop through each column in row
        //gridColumns.forEach(function (column, j) {
        //    var atrName = column.getName();
        //    var atrValue = column.getValue();
        //    if(atrName )
        //});
    });

}

function EnsureSrvTaskPercentCompleteAndWOSTStatus_OnChangeOfbookingpercenttimespent(executionContext) {
    debugger;
    var BRBObject = executionContext.getFormContext().data.entity;
    var bookingpercenttimespent = BRBObject.attributes.getByName("ap360_bookingpercenttimespent");
    //var woststatus = BRBObject.attributes.getByName("ap360_woststatus");
    var servicetaskcomplete = BRBObject.attributes.getByName("ap360_servicetaskcomplete");

    if (bookingpercenttimespent != null && bookingpercenttimespent.getValue() != null && bookingpercenttimespent.getValue() != 0) {

        //woststatus.setRequiredLevel("required");
        servicetaskcomplete.setRequiredLevel("required");

    }
    else {
        // woststatus.setRequiredLevel("none");
        servicetaskcomplete.setRequiredLevel("none");
        // woststatus.setValue(null);
        servicetaskcomplete.setValue(null);
        bookingpercenttimespent.setValue(0);
    }
}
function EnsureSrvTaskPercentCompleteAndWOSTStatus_OnChangeOfwostStatus(executionContext) {
    debugger;
    //var BRBObject = executionContext.getFormContext().data.entity;
    //var formContext = executionContext.getFormContext();
    //var bookingpercenttimespent = BRBObject.attributes.getByName("ap360_bookingpercenttimespent");
    // var woststatus = BRBObject.attributes.getByName("ap360_woststatus");
    // var servicetaskcomplete = BRBObject.attributes.getByName("ap360_servicetaskcomplete");
    // var followupDescription = BRBObject.attributes.getByName("ap360_followupdescription");
    // var wostStatusVal = woststatus.getValue();
    //    if (wostStatusVal == 126300011)//avaiting dependency)
    //    {
    //        var alertStrings = { confirmButtonLabel: "OK", text: "Awaiting dependency cannot be selected", title: "WOST Status Alert" };
    //        var alertOptions = { height: 120, width: 260 };
    //        Xrm.Navigation.alertDialog(alertStrings, alertOptions).then(
    //            function (success) {
    //                console.log("Alert dialog closed");
    //            },
    //            function (error) {
    //                console.log(error.message);
    //            }
    //        );
    //       // woststatus.setValue(null);
    //        if (bookingpercenttimespent != null && bookingpercenttimespent.getValue() != null && bookingpercenttimespent.getValue() != 0) {
    //            //woststatus.setRequiredLevel("required");
    //        }
    //        //woststatus.clearOption();
    //    }
    //    else {
    //        if (bookingpercenttimespent != null && bookingpercenttimespent.getValue() != null) {
    //            servicetaskcomplete.setRequiredLevel("required");
    //            //  woststatus.setRequiredLevel("required");
    //            if (wostStatusVal == 126300000 ||//need manager decision
    //                wostStatusVal == 126300002 ||//time used up
    //                wostStatusVal == 126300003 ||//need external help
    //                wostStatusVal == 126300004 ||// lead tech guidenace
    //                wostStatusVal == 126300007)//need discovery estimated
    //            {
    //                followupDescription.setRequiredLevel("required");
    //            } else {
    //                followupDescription.setRequiredLevel("none");
    //            }

    //        }
    //        else {
    //            //   woststatus.setRequiredLevel("required");
    //            servicetaskcomplete.setRequiredLevel("required");
    //            if (wostStatusVal == 126300000 ||//need manager decision
    //                wostStatusVal == 126300002 ||//time used up
    //                wostStatusVal == 126300003 ||//need external help
    //                wostStatusVal == 126300004 ||// lead tech guidenace
    //                wostStatusVal == 126300007)//need discovery estimated
    //            {
    //                followupDescription.setRequiredLevel("required");
    //            } else {
    //                followupDescription.setRequiredLevel("none");
    //            }
    //        }
    //    }

}
function EnsureSrvTaskPercentCompleteAndWOSTStatus_OnChangeOfServicetaskcomplete(executionContext) {
    debugger;
    Manage100PercentageCompleteOnChangeOfEthicalToBill(executionContext);
    var BRBObject = executionContext.getFormContext().data.entity;
    var servicetaskcomplete = BRBObject.attributes.getByName("ap360_servicetaskcomplete");
    var isMasterBST = BRBObject.attributes.getByName("ap360_ismasterbst");
    var workorderservicetaskstatus = BRBObject.attributes.getByName("ap360_workorderservicetaskstatus");
    var ServiceTaskCompleteVal = servicetaskcomplete.getValue();
    isMasterBST = isMasterBST.getValue();
    if (isMasterBST) {
        globalFormContext.getControl("ap360_workorderservicetaskstatus").removeOption(126300010);
        if (ServiceTaskCompleteVal == 126300006)//service task 100 % from grid
        {
            globalFormContext.getControl("ap360_workorderservicetaskstatus").addOption({ text: "Completed", value: 126300010 });
            globalFormContext.getAttribute("ap360_workorderservicetaskstatus").setValue(126300010);//completed
            globalFormContext.getControl("ap360_workorderservicetaskstatus").setDisabled(true);
        } else {
            globalFormContext.getControl("ap360_workorderservicetaskstatus").removeOption(126300010);//completed
            globalFormContext.getAttribute("ap360_workorderservicetaskstatus").setValue(null);
            globalFormContext.getAttribute("ap360_workorderservicetaskstatus").setRequiredLevel("required");
            globalFormContext.getControl("ap360_workorderservicetaskstatus").setDisabled(false);
        }
    }
}

function recalculateBooking(primaryControl) {
    debugger;
    var formContext = primaryControl;

    var formContext = primaryControl;
    var bookingId = formContext.data.entity.getId();

    var percentageTotal = 0;
    percentageTotal = getBookingTaskPercentComplete(bookingId);
    if (percentageTotal != 100) {
        Xrm.Utility.alertDialog("Please divide time properly. % time spent of all tasks should be equal to 100%");
        //var alertStrings = { confirmButtonLabel: "OK", text: "Please divide time properly. % time spent of all tasks should be equal to 100%" };
        //var alertOptions = { height: 200, width: 350 };
        //Xrm.Navigation.alertDialog(alertStrings, alertOptions);
    } else {

        formContext.getAttribute("ap360_calculateactualamount").setValue(true);

        formContext.data.save();
    }

}

LockFieldsOnEditableGrid = function (context, disabledFields) {
    //test
    var currEntity = context.getFormContext().data.entity;

    currEntity.attributes.forEach(function (attribute, i) {

        if (disabledFields.indexOf(attribute.getName()) > -1) {

            var attributeToDisable = attribute.controls.get(0);

            attributeToDisable.setDisabled(true);

        }

    });

}

UnLockFieldsOnEditableGrid = function (context, disabledFields) {

    var currEntity = context.getFormContext().data.entity;

    currEntity.attributes.forEach(function (attribute, i) {

        if (disabledFields.indexOf(attribute.getName()) > -1) {

            var attributeToDisable = attribute.controls.get(0);

            attributeToDisable.setDisabled(false);

        }

    });

}

function openBookingImageCaptureCanvasApp(primaryControl) {
    var formContext = primaryControl;
    var bookingId = formContext.data.entity.getId();
    var Opportunity = formContext.getAttribute("ap360_opportunityid").getValue();
    if (Opportunity == null) {
        Xrm.Utility.alertDialog("Opportunity not mapped!");
        return;
    }
    var WorkOrder = formContext.getAttribute("msdyn_workorder").getValue();
    if (WorkOrder == null) {
        Xrm.Utility.alertDialog("WorkOrder not mapped!");
        return;
    }
    WorkOrder[0].name = WorkOrder[0].name.replace('&', ' and ');
    var WOST = formContext.getAttribute("ap360_workorderservicetask").getValue();
    if (WOST == null) {
        Xrm.Utility.alertDialog("WOST not mapped!");
        return;
    }
    WOST[0].name = WOST[0].name.replace('&', ' and ');
    var Booking = formContext.getAttribute("name").getValue();
    if (Booking == null) {
        Xrm.Utility.alertDialog("Booking not mapped!");
        return;
    }
    Booking = Booking.replace('&', ' and ');

    var bookingId = bookingId.replace('{', '').replace('}', '');
    var OppId = Opportunity[0].id.replace('{', '').replace('}', '');

    var encodedURL = encodeURI("https://apps.powerapps.com/play/c7d0e5e9-d6d7-49c3-880c-f86a081753fb?tenantId=96a809ae-9f08-444f-b13e-15cd056b5672&bookingId=" + bookingId + "&opportunitytopic=" + Opportunity[0].name + "&workorder=" + WorkOrder[0].name + "&booking=" + Booking + "&opportunityid=" + OppId + "&WOST=" + WOST[0].name)


    encodedURL = encodedURL.replace('#', ' ');

    window.open(encodedURL);

}

function uploadBookingImageCaptureCanvasApp(primaryControl) {
    var formContext = primaryControl;

    var bookingId = formContext.data.entity.getId();
    var Opportunity = formContext.getAttribute("ap360_opportunityid").getValue();
    if (Opportunity == null) {
        Xrm.Utility.alertDialog("Opportunity not mapped!");
        return;
    }
    var WorkOrder = formContext.getAttribute("msdyn_workorder").getValue();
    if (WorkOrder == null) {
        Xrm.Utility.alertDialog("WorkOrder not mapped!");
        return;
    }
    WorkOrder[0].name = WorkOrder[0].name.replace('&', ' and ');

    var WOST = formContext.getAttribute("ap360_workorderservicetask").getValue();
    if (WOST == null) {
        Xrm.Utility.alertDialog("WOST not mapped!");
        return;
    }
    WOST[0].name = WOST[0].name.replace('&', ' and ');

    var Booking = formContext.getAttribute("name").getValue();
    if (Booking == null) {
        Xrm.Utility.alertDialog("Booking not mapped!");
        return;
    }
    Booking = Booking.replace('&', ' and ');

    var bookingId = bookingId.replace('{', '').replace('}', '');
    var OppId = Opportunity[0].id.replace('{', '').replace('}', '');
    var encodedURL = encodeURI("https://apps.powerapps.com/play/e8ba3db7-99ab-4e04-8791-f654f5b17630?tenantId=96a809ae-9f08-444f-b13e-15cd056b5672&bookingId=" + bookingId + "&opportunitytopic=" + Opportunity[0].name + "&workorder=" + WorkOrder[0].name + "&booking=" + Booking + "&opportunityid=" + OppId + "&WOST=" + WOST[0].name)
    encodedURL = encodedURL.replace('#', ' ');

    window.open(encodedURL);


}




