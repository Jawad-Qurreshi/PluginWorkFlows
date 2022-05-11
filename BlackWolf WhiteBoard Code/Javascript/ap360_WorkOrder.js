function onLoad(executionContextObj) {
    try {
        checkOpportunityStatus(executionContextObj);
        //ShowHideAuthorized();
        //var formContext = executionContextObj.getFormContext();
        //formContext.getControl("msdyn_substatus").addPreSearch(ApplyStatusFilter);
        //FilterSubStatus(executionContextObj)
        // LoadActivitiesGrid();

    }
    catch (e) {
        console.log("onLoad: " + e);;
    }
}
function checkOpportunityStatus(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var opportunityGuid = formContext.getAttribute("msdyn_opportunityid").getValue();
    //var recordId = Xrm.Page.data.entity.getId();

    //debugger;
    if (opportunityGuid != null) {
        opportunityGuid = opportunityGuid[0].id.replace('{', '').replace('}', '');
        var req = new XMLHttpRequest();
        var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/opportunities(" + opportunityGuid + ")?$select=statuscode";
        req.open("GET", url, false);
        req.setRequestHeader("OData-MaxVersion", "4.0");
        req.setRequestHeader("OData-Version", "4.0");
        req.setRequestHeader("Accept", "application/json");
        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
        req.send();

        if (req.readyState === 4) {
            req.onreadystatechange = null;
            if (req.status === 200) {
                var result = JSON.parse(req.response);
                //debugger;
                var statuscode = result["statuscode"];
                if (statuscode == 126300010)//Delivered
                {
                    var controls = Xrm.Page.ui.controls.get();

                    for (var i in controls) {

                        var control = controls[i];

                        if (!control.getDisabled()) {

                            control.setDisabled(true);

                        }

                    }
                    // alert("opportuntiy is delivered");
                }
            } else {
                Xrm.Utility.alertDialog(this.statusText);
            }
        }


    }
}

function FilterSubStatus(executionContext) {
    var formContext = executionContext.getFormContext(); // get formContext
    formContext.getControl("ap360_customwosubstatusid").addPreSearch(addSystemStatusPreSearchFilter);
}

function addSystemStatusPreSearchFilter() {
    debugger;
    //var msdyn_systemstatus = formContext.getAttribute("msdyn_systemstatus").getValue();
    //var msdyn_systemstatus = Xrm.Page.getAttribute("msdyn_systemstatus").getValue();
    var filter = "<filter type=\"and\">" +
        "<condition attribute=\"msdyn_systemstatus\" operator=\"ne\"  value=\"690970000\" />" +
        "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +

        "</filter>";

    Xrm.Page.getControl("ap360_customwosubstatusid").addCustomFilter(filter);
}

function ApplyStatusFilter(executionContextObj) {
    debugger;
    var statusId = Xrm.Page.getAttribute("msdyn_systemstatus").getValue();

    var filter = "<filter type='and'>";
    filter += "<condition attribute='msdyn_systemstatus' operator='eq' value='" + statusId + "' />";
    filter += "</filter>";

    var formContext = executionContextObj.getFormContext();
    formContext.getControl("msdyn_substatus").addCustomFilter(filter);
}

function ShowHideAuthorized() {
    try {
        var FieldName = Xrm.Page.getAttribute("msdyn_workordertype").getValue()[0].name;
        console.log("IN Try");
        if (FieldName == "Assesment") {
            console.log("IN If");
            Xrm.Page.ui.tabs.get("tab_5").sections.get("WO_Service_Tasks").setVisible(true);
            Xrm.Page.ui.tabs.get("tab_5").sections.get("WO_SERVICE_TASK").setVisible(false);
        }
        else {
            console.log("IN else");
            Xrm.Page.ui.tabs.get("tab_5").sections.get("WO_SERVICE_TASK").setVisible(true);
            Xrm.Page.ui.tabs.get("tab_5").sections.get("WO_Service_Tasks").setVisible(false);
        }
    }

    catch (e) {
        console.log("ShowHideAuthorized: " + e);
    }
}


function updateWOBWStatusTextandFormatedValue(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();

    formContext.getAttribute("ap360_workorderbwstatustext").setValue(null);
    formContext.getAttribute("ap360_workorderbwstatusformatedvalue").setValue(null);
    var WOstatusValueArray = formContext.getAttribute("ap360_workorderbwstatus").getValue();
    var WOstatusTextArray = formContext.getAttribute("ap360_workorderbwstatus").getText();

    if (WOstatusTextArray == null && WOstatusValueArray == null) {
        formContext.getAttribute("ap360_workorderbwstatustext").setValue(null);
        formContext.getAttribute("ap360_workorderbwstatusformatedvalue").setValue(null);
    } else {
        formContext.getAttribute("ap360_workorderbwstatustext").setValue(WOstatusValueArray.toString());
        formContext.getAttribute("ap360_workorderbwstatusformatedvalue").setValue(WOstatusTextArray.toString());
    }

}

function RunGenerateBookingWorkflow(primaryControl) {
    debugger;
    var formContext = primaryControl; // rename as formContext 

    try {
        var url = Xrm.Page.context.getClientUrl();
        var recordId = Xrm.Page.data.entity.getId();
        var workflowId = "5000f4b7-fcd1-42a4-87ab-db4e7d4386d9";//WF: WO: Book WorkOrder to Current User
        var OrgServicePath = "/XRMServices/2011/Organization.svc/web";
        url = url + OrgServicePath;
        var request;
        request = "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\">" +
            "<s:Body>" +
            "<Execute xmlns=\"http://schemas.microsoft.com/xrm/2011/Contracts/Services\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">" +
            "<request i:type=\"b:ExecuteWorkflowRequest\" xmlns:a=\"http://schemas.microsoft.com/xrm/2011/Contracts\" xmlns:b=\"http://schemas.microsoft.com/crm/2011/Contracts\">" +
            "<a:Parameters xmlns:c=\"http://schemas.datacontract.org/2004/07/System.Collections.Generic\">" +
            "<a:KeyValuePairOfstringanyType>" +
            "<c:key>EntityId</c:key>" +
            "<c:value i:type=\"d:guid\" xmlns:d=\"http://schemas.microsoft.com/2003/10/Serialization/\">" + recordId + "</c:value>" +
            "</a:KeyValuePairOfstringanyType>" +
            "<a:KeyValuePairOfstringanyType>" +
            "<c:key>WorkflowId</c:key>" +
            "<c:value i:type=\"d:guid\" xmlns:d=\"http://schemas.microsoft.com/2003/10/Serialization/\">" + workflowId + "</c:value>" +
            "</a:KeyValuePairOfstringanyType>" +
            "</a:Parameters>" +
            "<a:RequestId i:nil=\"true\" />" +
            "<a:RequestName>ExecuteWorkflow</a:RequestName>" +
            "</request>" +
            "</Execute>" +
            "</s:Body>" +
            "</s:Envelope>";

        var req = new XMLHttpRequest();
        req.open("POST", url, true)
        // Responses will return XML. It isn't possible to return JSON.
        req.setRequestHeader("Accept", "application/xml, text/xml, */*");
        req.setRequestHeader("Content-Type", "text/xml; charset=utf-8");
        req.setRequestHeader("SOAPAction", "http://schemas.microsoft.com/xrm/2011/Contracts/Services/IOrganizationService/Execute");
        req.onreadystatechange = function () { assignResponse(req); };
        req.send(request);
        //Xrm.Page.ui.close();
    }
    catch (e) {
        console.log("RunGenerateWelcomeLetterWorkflow: " + e);
    }
}
function assignResponse(req) {
    if (req.readyState == 4) {
        if (req.status == 200) {
            alert("Booking Created");
            console.log('successfully executed the workflow');
        }
    }

}

function LoadActivitiesGrid() {
    debugger;
    var activityGrid = document.getElementById("ap360_GridWhiteBorardItems");
    if (activityGrid == null)
        activityGrid = parent.document.getElementById("ap360_GridWhiteBorardItems");

    if (activityGrid == null || activityGrid.control == null) {
        setTimeout('LoadActivitiesGrid()', 1000);
        return;
    }
    debugger;
    var id = Xrm.Page.data.entity.getId();
    if (id == null) return;

    var fetchXml = "<fetch distinct='true' mapping='logical' output-format='xml-platform' version='1.0'>";

    fetchXml += "<entity name='ap360_whiteboarditem'>";
    fetchXml += "<attribute name='subject' />";
    fetchXml += "<attribute name='createdon' />";
    fetchXml += "<attribute name='statuscode' />";
    fetchXml += "<attribute name='ownerid' />";
    fetchXml += "<attribute name='scheduledend' />";
    fetchXml += "<attribute name='activityid' />";
    fetchXml += "<attribute name='regardingobjectid' />";
    fetchXml += "<order descending='false' attribute='createdon' />";
    fetchXml += "<filter type='and'>";
    fetchXml += " <condition attribute='statecode' operator='not-null' />";
    fetchXml += " <condition attribute='ap360_workorder' value='" + id + "' operator='eq' />";
    fetchXml += " </filter>";
    fetchXml += " </entity>";
    fetchXml += "  </fetch>";

    activityGrid.control.SetParameter("fetchXml", fetchXml);
    activityGrid.control.refresh();

}
//function ReturnToVendor_RibbonButton(formContext, selectedWoProductsGuids) {

//    var selectedWoProductsIds = "";
//    var count = 1;

//    for (var i = 0; i < selectedWoProductsGuids.length; i++) {
//        selectedWoProductsIds += selectedWoProductsGuids[i];
//        if (count > 1 || count < selectedWoProductsGuids.length)
//            selectedWoProductsIds += ",";
//        count++;
//        console.log(selectedWoProductsGuids[i]);
//    }
//    if (selectedWoProductsGuids.length > 1) {
//        selectedWoProductsIds = selectedWoProductsIds.substring(0, selectedWoProductsIds.length - 1);
//    }
//    debugger;
//    // Call Action 
//    var parameters = {};
//    parameters.SeletectedWorkOrderProductIds = selectedWoProductsIds;

//    var req = new XMLHttpRequest();
//    req.open("POST", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ap360_ReturnToVendorForWorkOrderProduct", false);
//    req.setRequestHeader("OData-MaxVersion", "4.0");
//    req.setRequestHeader("OData-Version", "4.0");
//    req.setRequestHeader("Accept", "application/json");
//    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
//    req.onreadystatechange = function () {
//        if (this.readyState === 4) {
//            req.onreadystatechange = null;
//            if (this.status === 204) {
//                debugger;
//                //Success - No Return Data - Do Something
//            } else {
//                debugger;
//                Xrm.Utility.alertDialog(this.statusText);
//            }
//        }
//    };
//    req.send(JSON.stringify(parameters));

//}

function ReturnToVendor_RibbonButton(formContext, selectedWoProductsGuids) {

    try {
        var selectedWoProductsIds = "";
        var count = 1;

        for (var i = 0; i < selectedWoProductsGuids.length; i++) {
            selectedWoProductsIds += selectedWoProductsGuids[i];
            if (count > 1 || count < selectedWoProductsGuids.length)
                selectedWoProductsIds += ",";
            count++;
            console.log(selectedWoProductsGuids[i]);
        }
        if (selectedWoProductsGuids.length > 1) {
            selectedWoProductsIds = selectedWoProductsIds.substring(0, selectedWoProductsIds.length - 1);
        }
        debugger;
        // Call Action 
        var parameters = {};
        parameters.SeletectedWorkOrderProductIds = selectedWoProductsIds;


        Process.callAction("ap360_ReturnToVendorForWorkOrderProduct",
            [
                {
                    key: "SeletectedWorkOrderProductIds",
                    type: Process.Type.String,
                    value: selectedWoProductsIds
                }],
            function (params) {

                //  Xrm.Page.getControl("workorderproductgrid").refresh();

                // Success
                alert(" WorkOrder Product Line Status updated to 'Not Used' and Selected Product is Returned to Vendor ");
                //alert(params[accountName]);
                //alert("Name = " + params["Entity"].get("name") + "\n" +
                //      "Status = " + params["Entity"].formattedValues["statuscode"]);
            },
            function (e, t) {
                // Error
                alert(e);

                // Write the trace log to the dev console
                if (window.console && console.error) {
                    console.error(e + "\n" + t);
                }
            });
    }
    catch (e) {

        Xrm.Utility.alertDialog(e.message);

    }

}
