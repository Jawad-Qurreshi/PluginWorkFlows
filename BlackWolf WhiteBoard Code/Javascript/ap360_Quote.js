function OnLoad(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    //formContext.getControl("ap360_productfamily").setDisabled(true);
    FilterPrimaryIncident(executionContext);
    ChangeFieldReqLevel_OnChangeUsePrimaryIncident(executionContext);
}

function ActivateQuote(primaryControl) {
    var formContext = primaryControl;
    Xrm.Utility.showProgressIndicator("Activating Quote");
    var EntityID = Xrm.Page.data.entity.getId();
    var req = new XMLHttpRequest();
    var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.2/ap360_quoteservices?$select=statuscode,ap360_actualworkrequested&$filter=_ap360_quoteid_value%20eq%20(" + EntityID + ")";
    req.open("GET", url, false);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
    req.send();
    if (req.status === 200) {
        var results = JSON.parse(req.response);
        var checkstatus = false;
        for (var i = 0; i < results.value.length; i++) {
            var quoteServiceStatusCode = results.value[i]["statuscode"];

            if (quoteServiceStatusCode != 126300000) // quotes service Status Completed
            {
                checkstatus = true;
            }
        }
        if (checkstatus == true) {

            Xrm.Utility.closeProgressIndicator();
            Xrm.Utility.alertDialog("One of the quote service status is not 'Completed'.Please complete the quote service");
        }
        else {
            formContext.getAttribute("statecode").setValue(1); // Quote / Active
            formContext.getAttribute("statuscode").setValue(3); // Quote / Active-Submitted to Client
            formContext.data.entity.save();
            Xrm.Utility.closeProgressIndicator();
        }
    } else {
        Xrm.Utility.closeProgressIndicator();
        Xrm.Utility.alertDialog(req.statusText);
    }
}

function showQuoteServices(executionContext) {
    var formContext = executionContext.getFormContext();
    var iframeObject = formContext.getControl("IFRAME_QuoteService");

    if (iframeObject != null) {
        //var strURL = "/crmreports/viewer/viewer.aspx?id=14c77f16-6c3d-ec11-8c63-000d3a5b298c&action=run&context=records&recordstype=1084records="
        //    + formContext.data.entity.getId()
        //    + "&helpID=Quote%20QuoteServices.rdl";
        //var strURL = "/crmreports/viewer/viewer.aspx?id=14c77f16-6c3d-ec11-8c63-000d3a5b298c&action=run&helpID=Quote%20QuoteServices.rdl&context=records&recordstype=1084&appid=ab2029aa-0f66-ea11-a811-000d3a33f47e";

        strURL = "/crmreports/viewer/viewer.aspx?id=14c77f16-6c3d-ec11-8c63-000d3a5b298c&action=run&context=records&recordstype=1084&records="
            + formContext.data.entity.getId();
        //+ "&helpID=Quote%20QuoteServices.rdl";

        //Set URL of iframe
        iframeObject.setSrc(strURL);
    }
}
function FilterPrimaryIncident(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();

    formContext.getControl("ap360_incidenttypeid").addPreSearch(addPrimaryIncidentPreSearchFilter);
}
function addPrimaryIncidentPreSearchFilter() {
    var ParentServiceTemplate = Xrm.Page.data.entity.attributes.get("ap360_parentservicetemplatetypeid").getValue();
    ParentServiceTemplate = ParentServiceTemplate[0].id;
    var ServiceTemplate = Xrm.Page.data.entity.attributes.get("ap360_servicetemplateid").getValue();
    ServiceTemplate = ServiceTemplate[0].id;
    var ChildServiceTemplate = Xrm.Page.data.entity.attributes.get("ap360_childservicetemplateid").getValue();
    ChildServiceTemplate = ChildServiceTemplate[0].id;


    var filter = "<filter type=\"and\">" +
        "<condition attribute=\"ap360_parentservicetemplatetypeid\" operator=\"eq\" value=\"" + ParentServiceTemplate + "\"  />" +
        "<condition attribute=\"ap360_servicetemplateid\" operator=\"eq\" value=\"" + ServiceTemplate + "\"  />" +
        "<condition attribute=\"ap360_childservicetemplateid\" operator=\"eq\" value=\"" + ChildServiceTemplate + "\"  />" +
        "</filter>";
    Xrm.Page.getControl("ap360_incidenttypeid").addCustomFilter(filter);
}

function ChangeFieldReqLevel_OnChangeUsePrimaryIncident(executionContext) {
    var formContext = executionContext.getFormContext();
    var IsuserprimaryIncident = formContext.getAttribute("ap360_useprimaryincident").getValue();

    if (IsuserprimaryIncident) {
        formContext.getControl("ap360_servicetemplateid").setVisible(true);
        formContext.getControl("ap360_childservicetemplateid").setVisible(true);
        formContext.getControl("ap360_incidenttypeid").setVisible(true);
        formContext.getAttribute("ap360_servicetemplateid").setRequiredLevel("required");
        formContext.getAttribute("ap360_childservicetemplateid").setRequiredLevel("required");
        //  formContext.getAttribute("ap360_incidenttypeid").setRequiredLevel("required");


    }
    else {
        formContext.getControl("ap360_servicetemplateid").setVisible(false);
        formContext.getControl("ap360_childservicetemplateid").setVisible(false);
        formContext.getControl("ap360_incidenttypeid").setVisible(false);
        formContext.getAttribute("ap360_servicetemplateid").setRequiredLevel("none");
        formContext.getAttribute("ap360_childservicetemplateid").setRequiredLevel("none");
        formContext.getAttribute("ap360_incidenttypeid").setRequiredLevel("none");

        formContext.getAttribute("ap360_incidenttypeid").setValue(null);

    }



}


function showPrimaryIncident_OnChangeChildServiceTemplate(executionContext) {
    var formContext = executionContext.getFormContext();

    var ChildServiceTemplate = formContext.getAttribute("ap360_childservicetemplateid").getValue();
    if (ChildServiceTemplate != null) {
        formContext.getControl("ap360_incidenttypeid").setVisible(true);
        formContext.getAttribute("ap360_incidenttypeid").setRequiredLevel("required");
    }
    else {
        formContext.getControl("ap360_incidenttypeid").setVisible(false);
        formContext.getAttribute("ap360_incidenttypeid").setRequiredLevel("none");

        formContext.getAttribute("ap360_incidenttypeid").setValue(null);
    }
}


function populateName(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext(); // get formContext
    var parentservicetemplatetype = null;
    if (formContext.getAttribute("ap360_parentservicetemplatetypeid") != null) {
        parentservicetemplatetype = formContext.getAttribute("ap360_parentservicetemplatetypeid").getValue();

        var vehicle = formContext.getAttribute("ap360_vehicleid").getValue();

        if (parentservicetemplatetype != null) {
            var additionaldescription = formContext.getAttribute("ap360_additionaldescription").getValue()
            if (additionaldescription != null) {
                formContext.getAttribute("name").setValue(parentservicetemplatetype[0].name + " " + additionaldescription + " " + vehicle[0].name);
            }
            else {
                formContext.getAttribute("name").setValue(parentservicetemplatetype[0].name + " " + vehicle[0].name);
            }

        }
        else
            formContext.getAttribute("name").setValue(null);

    }
}
function SetQuoteType_OnchangeParentServiceTemplate(executionContext) {
    var formContext = executionContext.getFormContext();
    //debugger;
    var ap360_parentservicetemplatetypeid = formContext.getAttribute("ap360_parentservicetemplatetypeid").getValue();
    if (ap360_parentservicetemplatetypeid != null) {
        var ap360_parentservicetemplatetypeid = ap360_parentservicetemplatetypeid[0].id.replace('{', '').replace('}', '');
        var diagnostic = "b8392795-407b-ea11-a811-000d3a30fcff";
        var inspection = "ba392795-407b-ea11-a811-000d3a30fcff";

        if (ap360_parentservicetemplatetypeid.toLowerCase() == diagnostic ||
            ap360_parentservicetemplatetypeid.toLowerCase() == inspection
        ) {
            // Assesment: 126300000
            // Production: 126300001
            formContext.getAttribute("ap360_quotetype").setValue(126300000);
        } else {
            formContext.getAttribute("ap360_quotetype").setValue(126300001);
        }
    } else {
        formContext.getAttribute("ap360_quotetype").setValue(null);
    }
}

function CreateWorkOrder_OnClickDisable(primaryControl)//(primaryControl) 
{
    var formContext = primaryControl; // rename as formContext 

    //var approvequotetowoconverstion = formContext.getAttribute("ap360_approvequotetowoconverstion").getValue();

    //if (approvequotetowoconverstion) {
    //    alert("Work Order is already Created");
    //}
    //else {
    //var statusReason = formContext.getAttribute("statuscode").getValue();
    //if (statusReason != 3) //Active submitted to client
    //{
    //    alert("Work Order can only be created for Quotes Status: Active-Submitted to Client ");
    //}
    //else {
    // Xrm.Page.getAttribute("ap360_approvequotetowoconverstion").setSubmitMode("always");

    formContext.getAttribute("ap360_approvequotetowoconverstion").setValue(true);//Trigger for Work Order Creation
    formContext.data.entity.save();

    // }
    // }
}


function CreateWorkOrderProject_OnClick_Disable(primaryControl)//(primaryControl) 
{
    var formContext = primaryControl; // rename as formContext 


    formContext.getAttribute("ap360_createwoprojecttask").setValue(true);//Trigger for Work Order Creation
    formContext.data.entity.save();

    // }
    // }
}


function createReviseQuote(primaryControl) {
    //alert("Work In Progress");
    debugger;
    var formContext = primaryControl;


    try {

        var recordId = formContext.data.entity.getId();
        //var msdyn_workorderId = msdyn_workorder[0].id.replace('{', '').replace('}', '');

        //formContext.ui.setFormNotification("Please wait, Quote Creation is in progress... ", "INFO", "1");
        Xrm.Utility.showProgressIndicator("Please wait, Quote Creation is in progress...");

        Process.callAction("ap360_CreateReviseQuote",
            [
                {
                    key: "OldQuoteId",
                    type: Process.Type.String,
                    value: recordId
                }],
            function (params) {
                debugger;
                // alert("test");
                // alert("Guid " + params[NewQuoteId]);
                //formContext.ui.clearFormNotification("Please wait, Quote Creation is in progress... ", "INFO", "1");
                Xrm.Utility.closeProgressIndicator();

                if (params.NewQuoteId != null) {
                    if (params.NewQuoteId == "notcompleted") {

                        Xrm.Utility.alertDialog("Click Again Create Revise Quote, Some Quote Service are not added in Revised Quote ");
                    }
                    else {
                        //var url = "https://blackwolfwhiteboard.crm.dynamics.com/main.aspx?appid=ab2029aa-0f66-ea11-a811-000d3a33f47e&cmdbar=true&forceUCI=1&pagetype=entityrecord&etn=quote&id="+params.NewQuoteId;
                        window.open("https://blackwolfwhiteboard.crm.dynamics.com/main.aspx?appid=ab2029aa-0f66-ea11-a811-000d3a33f47e&cmdbar=true&forceUCI=1&pagetype=entityrecord&etn=quote&id=" + params.NewQuoteId)
                    }
                }
                //Xrm.Page.getControl("workorderproductgrid").refresh();
                //var lookupOptions = {};
                //lookupOptions.entityType = "msdyn_workorderproduct";
                //Xrm.Utility.refreshParentGrid(lookupOptions);

                // Success
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

    } catch (e) {

        Xrm.Utility.alertDialog(e.message);

    }

}
function CreateWorkOrder_OnClick(primaryControl) {

    try {
        var formContext = primaryControl; // rename as formContext 
        var recordId = formContext.data.entity.getId();
        var IsWOCreated = Xrm.Page.getAttribute("ap360_isworkordercreated").getValue();
        if (IsWOCreated == 0) {

            Xrm.Utility.showProgressIndicator("Creating WorkOrder(s)");

            Process.callAction("ap360_CreateWorkOrder",
                [{
                    key: "quoteGuid",
                    type: Process.Type.String,
                    value: recordId
                }],
                function (params) {
                    debugger;
                    var isAllQuoteServiceCoverted = params["isAllQuoteServicesConverted"];

                    if (isAllQuoteServiceCoverted == "Yes") {
                        Xrm.Utility.closeProgressIndicator();
                        var subgrid = formContext.ui.controls.get("workorder");
                        // subgrid.refresh();
                        var quoteServiceSubgrid = formContext.ui.controls.get("quoteservice");
                        //quoteServiceSubgrid.refresh();
                        formContext.data.refresh(true);



                    }
                    else if (isAllQuoteServiceCoverted == "No") {
                        Xrm.Utility.closeProgressIndicator();
                        var subgrid = formContext.ui.controls.get("workorder");
                        var quoteServiceSubgrid = formContext.ui.controls.get("quoteservice");
                        //quoteServiceSubgrid.refresh();
                        formContext.data.refresh(true);

                        //subgrid.refresh();
                        Xrm.Utility.alertDialog("Click 'Create WO' again, some Quote Service(s) are not converted into WorkOrder(s)");
                    }
                    formContext.data.refresh(true);

                    Xrm.Utility.closeProgressIndicator();
                },
                function (e, t) {
                    Xrm.Utility.closeProgressIndicator();

                    Xrm.Utility.alertDialog(e);
                    if (window.console && console.error) {
                        console.error(e + "\n" + t);
                    }
                });

        } else {
            Xrm.Utility.alertDialog("WorkOrder already created!");
        }
        //   }
    } catch (e) {

        Xrm.Utility.alertDialog(e.message);

    }

}
function CreateWorkOrderProject_OnClick(primaryControl)//(primaryControl) 
{
    //var formContext = primaryControl; // rename as formContext 


    //formContext.getAttribute("ap360_createwoprojecttask").setValue(true);//Trigger for Work Order Creation
    //formContext.data.entity.save();

    try {
        var formContext = primaryControl; // rename as formContext 
        var recordId = formContext.data.entity.getId();
        Xrm.Utility.showProgressIndicator("Creating Work Order Projects(s)");

        Process.callAction("ap360_CreateWorkOrderProjectAndProjectTasksFromQuote",
            [{
                key: "quoteGuid",
                type: Process.Type.String,
                value: recordId
            }],
            function (params) {
                debugger;
                var isAllWorkOrdersAreConverted = params["isAllWorkOrdersAreConverted"];

                if (isAllWorkOrdersAreConverted == "Yes") {
                    Xrm.Utility.closeProgressIndicator();
                    var subgrid = formContext.ui.controls.get("workorder");
                    subgrid.refresh();
                    //var quoteServiceSubgrid = formContext.ui.controls.get("quoteservice");

                    // quoteServiceSubgrid.refresh();


                }
                else if (isAllWorkOrdersAreConverted == "No") {
                    Xrm.Utility.closeProgressIndicator();
                    var subgrid = formContext.ui.controls.get("workorder");
                    subgrid.refresh();
                    //var quoteServiceSubgrid = formContext.ui.controls.get("quoteservice");
                    //quoteServiceSubgrid.refresh();

                    Xrm.Utility.alertDialog("Click 'Create WO Project' again, some Work Order(s) tasks are not created");
                }
                else if (isAllWorkOrdersAreConverted == "AlreadyConverted") {
                    Xrm.Utility.alertDialog("Project Task(s) are already created for WorkOrder(s)");

                }

                Xrm.Utility.closeProgressIndicator();
            },
            function (e, t) {
                Xrm.Utility.closeProgressIndicator();

                Xrm.Utility.alertDialog(e);
                if (window.console && console.error) {
                    console.error(e + "\n" + t);
                }
            });
        //   }
    } catch (e) {

        Xrm.Utility.alertDialog(e.message);

    }
}

function checkQSStatus(executionContext) {
    var formContext = executionContext.getFormContext();
    var stateCode = formContext.getAttribute("statecode").getValue();
    if (stateCode == 1)// Quote / Active
    {
        var EntityID = Xrm.Page.data.entity.getId();
        var req = new XMLHttpRequest();
        var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.2/ap360_quoteservices?$select=statuscode,ap360_actualworkrequested&$filter=_ap360_quoteid_value%20eq%20(" + EntityID + ")";
        req.open("GET", url, false);
        req.setRequestHeader("OData-MaxVersion", "4.0");
        req.setRequestHeader("OData-Version", "4.0");
        req.setRequestHeader("Accept", "application/json");
        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
        req.send();
        if (req.status === 200) {
            var results = JSON.parse(req.response);
            var checkstatus = false;
            for (var i = 0; i < results.value.length; i++) {
                var quoteServiceStatusCode = results.value[i]["statuscode"];

                if (quoteServiceStatusCode != 126300000) // quotes service Status Completed
                {
                    checkstatus = true;
                }
            }
            if (checkstatus == true) {

                Xrm.Utility.closeProgressIndicator();
                Xrm.Utility.alertDialog("One of the quote service status is not 'Completed'.Please complete the quote service");
            }
            else {
                formContext.getAttribute("statecode").setValue(1); // Quote / Active
                formContext.getAttribute("statuscode").setValue(3); // Quote / Active-Submitted to Client
                formContext.data.entity.save();
                Xrm.Utility.closeProgressIndicator();
            }
        } else {
            Xrm.Utility.closeProgressIndicator();
            Xrm.Utility.alertDialog(req.statusText);
        }
    }

}
function addOpportunityNumberOnUpdateOpportunity(executionContext) {
    var formContext = executionContext.getFormContext();
    var opportunityRef = formContext.getAttribute("opportunityid").getValue();
    if (opportunityRef != null) {
        var opportunityId = opportunityRef[0].id.replace('{', '').replace('}', '');
        var req = new XMLHttpRequest();
        req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/opportunities(" + opportunityId + ")?$select=ap360_opportunityautonumber", false);
        req.setRequestHeader("OData-MaxVersion", "4.0");
        req.setRequestHeader("OData-Version", "4.0");
        req.setRequestHeader("Accept", "application/json");
        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
        req.send(); {
            if (req.readyState === 4) {
                req.onreadystatechange = null;
                if (req.status === 200) {
                    var result = JSON.parse(req.response);
                    var ap360_opportunityautonumber = result["ap360_opportunityautonumber"];
                    formContext.getAttribute("ap360_opportunityautonumber").setValue(ap360_opportunityautonumber);
                } else {
                    Xrm.Utility.alertDialog(req.statusText);
                }
            }
        }
    }
    else { formContext.getAttribute("ap360_opportunityautonumber").setValue(null); }
}

//function checkQSStatus(executionContext) {
//    var formContext = executionContext.getFormContext();
//    var quotestatus = formContext.getAttribute('statuscode').getValue();

//    if (quotestatus == 3) {
//        var EntityID = Xrm.Page.data.entity.getId();
//        var req = new XMLHttpRequest();
//        var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.2/ap360_quoteservices?$select=statuscode,ap360_actualworkrequested&$filter=_ap360_quoteid_value%20eq%20(" + EntityID + ")";
//        req.open("GET", url, false);
//        req.setRequestHeader("OData-MaxVersion", "4.0");
//        req.setRequestHeader("OData-Version", "4.0");
//        req.setRequestHeader("Accept", "application/json");
//        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
//        req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
//        req.send();
//        if (req.status === 200) {
//            var results = JSON.parse(req.response);
//            var checkstatus = false;
//            for (var i = 0; i < results.value.length; i++) {
//                var quoteServiceStatusCode = results.value[i]["statuscode"];

//                if (quoteServiceStatusCode != 126300000) // quotes service Status Completed
//                {
//                    checkstatus = true;
//                }
//            }
//            if (checkstatus == true) {
//                Xrm.Utility.alertDialog("Quote service status Not completed yet.");

//                formContext.getAttribute("statecode").setValue(0); // Quote / Draft
//                formContext.getAttribute("statuscode").setValue(1); // Quote / Draft - In progress/ Under construction

//                formContext.data.entity.save();
//                setTimeout(function () { formContext.data.entity.refresh(); }, 2000);

//            }
//        } else {
//            Xrm.Utility.alertDialog(req.statusText);
//        }
//    }
//}



