// JavaScript source code
function onload(executionContext) {
    showReport(executionContext);
    showFollowUPReport(executionContext);
    showProgressReport(executionContext);
    showOpportunityQuotes(executionContext);
    getActivities(executionContext);
}
function showReport(executionContext) {
    var formContext = executionContext.getFormContext();

    var iframeObject = formContext.getControl("IFRAME_workorder");

    if (iframeObject != null) {
        var strURL = "/crmreports/viewer/viewer.aspx?id=0abe425a-1e9e-eb11-b1ac-000d3a35cb5f&action=run&context=records&recordstype=3&records="
            + formContext.data.entity.getId()
            + "&helpID=Post%2010%20Aug%20%20Invoice%20Opportunity.rdl";

        //Set URL of iframe
        iframeObject.setSrc(strURL);
    }
}

function showOpportunityQuotes(executionContext) {
    var formContext = executionContext.getFormContext();

    var iframeObject = formContext.getControl("IFRAME_Quotes");

    if (iframeObject != null) {
        var strURL = "/crmreports/viewer/viewer.aspx?id=47bad51f-1a3c-ec11-b6e5-0022480af5d5&action=run&context=records&recordstype=3&records="
            + formContext.data.entity.getId()
            + "&helpID=NeglectedOpportunity%20Quotes.rdl";

        //Set URL of iframe
        iframeObject.setSrc(strURL);

    }
}

function showFollowUPReport(executionContext) {
    var formContext = executionContext.getFormContext();

    var iframeObject = formContext.getControl("IFRAME_followupitems");

    if (iframeObject != null) {
        var strURL = "/crmreports/viewer/viewer.aspx?id=e13c1a48-bfa6-eb11-b1ac-000d3a317176&action=run&context=records&recordstype=3&records="
            + formContext.data.entity.getId()
            // + "&helpID=Post%2010%20Aug%20%20Invoice%20Opportunity.rdl";
            + "&helpID=FollowUp%20Opportunity%20.rdl"
        //Set URL of iframere
        iframeObject.setSrc(strURL);
    }
}
function showProgressReport(executionContext) {
    var formContext = executionContext.getFormContext();

    var iframeObject = formContext.getControl("IFRAME_progress");

    if (iframeObject != null) {
        var strURL = "/crmreports/viewer/viewer.aspx?id=5431CB61-4480-EB11-A812-000D3A31C0BA&action=run&context=records&recordstype=3&records="
            + formContext.data.entity.getId()
            // + "&helpID=Post%2010%20Aug%20%20Invoice%20Opportunity.rdl";
            + "&helpID=Opportunity%20Progress%20.rdl"

        //Set URL of iframe
        iframeObject.setSrc(strURL);
    }
}

function SetName_onSave(executionContext) {
    var formContext = executionContext.getFormContext();
    // debugger;
    var ap360_vehicleid = formContext.getAttribute("ap360_vehicleid").getValue();
    if (ap360_vehicleid != null) {
        ap360_vehicleid = ap360_vehicleid[0].name;

    }
    var account = formContext.getAttribute("parentaccountid").getValue();
    if (account != null) {
        account = account[0].name;

        var acountLastName = account.split(', ')
        var formtype = formContext.ui.getFormType();
        //debugger;
        if (formtype == 1) {
            var today = new Date();
            var dd = String(today.getDate()).padStart(2, '0');
            var mm = String(today.getMonth() + 1).padStart(2, '0'); //January is 0!
            var yyyy = today.getFullYear();

            today = mm + '/' + dd + '/' + yyyy;
            formContext.getAttribute("name").setValue(ap360_vehicleid + " " + acountLastName[0] + " " + today);
        } else {
            var name = formContext.getAttribute("name").getValue();
            if (name != null) {
                var a = name.split(' ');
                var lastDate = a[a.length - 1];
                formContext.getAttribute("name").setValue(ap360_vehicleid + " " + acountLastName[0] + " " + lastDate);
            }
        }
    }
}
function makeDeliverDateMandatory_onChangeStatusReason(executionContext) {
    var formContext = executionContext.getFormContext();
    // debugger;
    var statusReason = formContext.getAttribute("statuscode").getValue();
    if (statusReason != null && statusReason == 126300010) //Delivered
    {
        formContext.getAttribute("ap360_delivereddate").setRequiredLevel("required");

    }
    else {
        formContext.getAttribute("ap360_delivereddate").setRequiredLevel("none");

    }
}

function setOpportunityNumbertoOldfield(executionContext) {
    var formContext = executionContext.getFormContext();

    var ap360_opportunityautonumber = formContext.getAttribute("ap360_opportunityautonumber").getValue();

    if (ap360_opportunityautonumber != null) {

        formContext.getAttribute("ap360_opportunitynumber").setValue(ap360_opportunityautonumber);

    }

}
function getActivities(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    //get current contact id;
    var recordId = formContext.data.entity.getId();
    var ActivitiesSubGridControl = formContext.getControl("Activities");
    //recordId = recordId.replace("{", "").replace("}", "");
    recordId = recordId.slice(1, -1);
    var orConditionsList = [];
    var orConditionsFetchXml = "";
    var fetchXml = "";

    Xrm.WebApi.online.retrieveMultipleRecords("activityparty", "?$select=_activityid_value&$filter=_partyid_value eq " + recordId).then(
        function success(result) {
            for (var i = 0; i < result.entities.length; i++) {
                orConditionsList.push(result.entities[i]["_activityid_value"]);
            }

            for (var i = 0; i < orConditionsList.length; i++) {
                orConditionsFetchXml += '<value>' + orConditionsList[i] + '</value>'
                //console.log(orConditionsList[i]); 
            }

            if (orConditionsList.length != 0) {
                fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                    "  <entity name='activitypointer'>" +
                    "    <attribute name='subject' />" +
                    "    <attribute name='ownerid' />" +
                    "    <attribute name='prioritycode' />" +
                    "    <attribute name='regardingobjectid' />" +
                    "    <attribute name='activitytypecode' />" +
                    "    <attribute name='statecode' />" +
                    "    <attribute name='scheduledstart' />" +
                    "    <attribute name='scheduledend' />" +
                    "    <attribute name='activityid' />" +
                    "    <attribute name='instancetypecode' />" +
                    "    <attribute name='community' />" +
                    "    <attribute name='senton' />" +
                    "    <attribute name='statuscode' />" +
                    "    <order attribute='scheduledend' descending='false' />" +
                    "    <filter type='and'>" +
                    "      <condition attribute='isregularactivity' operator='eq' value='1' />" +
                    "      <condition attribute='activityid' operator='in'>" +
                    orConditionsFetchXml +
                    "      </condition>" +
                    "    </filter>" +
                    "  </entity>" +
                    "</fetch>"
                console.log(fetchXml);




                ActivitiesSubGridControl.setFilterXml(fetchXml);
                ActivitiesSubGridControl.refresh();



            } else {
                fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                    "  <entity name='activitypointer'>" +
                    "    <filter type='and'>" +
                    "      <condition attribute='statecode' operator='null'/>" +
                    "    </filter>" +
                    "  </entity>" +
                    "</fetch>";
                ActivitiesSubGridControl.setFilterXml(fetchXml);
                ActivitiesSubGridControl.refresh();

            }

        },
        function (error) {
            console.log(error.message);
            // handle error conditions
        }
    );
}

function getImagesOnCanvasAppIFrame(executionContext) {

    var formContext = executionContext.getFormContext();
    var iframeObject = formContext.getControl("IFRAME_Opportunities");
    var OPPId = formContext.data.entity.getId();
    OPPId = OPPId.replace("{", "").replace("}", "");
    if (iframeObject != null) {
        //var strURL = "https://apps.powerapps.com/play/be05551c-9d9d-4bc6-b5a7-3ec8cf6bce78?tenantId=96a809ae-9f08-444f-b13e-15cd056b5672&OpportunityId=" + OPPId; //For SandBox
        var strURL = "https://apps.powerapps.com/play/33104edf-2dcf-4a14-83a2-3a7f1055618c?tenantId=96a809ae-9f08-444f-b13e-15cd056b5672&source=iframe&OpportunityId=" + OPPId;//For Prod
        //Set URL of iframe
        iframeObject.setSrc(strURL);
    }
}

