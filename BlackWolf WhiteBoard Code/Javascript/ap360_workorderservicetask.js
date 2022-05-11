// Process.js v2.0 - Copyright Paul Nieuwelaar Magnetism 2016
// Download the latest version from https://github.com/PaulNieuwelaar/processjs

/*
    // Call an Action
    Process.callAction("mag_Retrieve",
    [{
        key: "Target",
        type: Process.Type.EntityReference,
        value: new Process.EntityReference("account", Xrm.Page.data.entity.getId())
    },
    {
        key: "ColumnSet",
        type: Process.Type.String,
        value: "name, statuscode"
    }],
    function (params) {
        // Success
        alert("Name = " + params["Entity"].get("name") + "\n" +
                "Status = " + params["Entity"].formattedValues["statuscode"]);
    },
    function (e, t) {
        // Error
        alert(e);
        // Write the trace log to the dev console
        if (window.console && console.error) {
            console.error(e + "\n" + t);
        }
    });
    // Call a Workflow
    Process.callWorkflow("4AB26754-3F2F-4B1D-9EC7-F8932331567A", Xrm.Page.data.entity.getId(),
        function () {
            alert("Workflow executed successfully");
        },
        function () {
            alert("Error executing workflow");
        });
    // Call a Dialog
    Process.callDialog("C50B3473-F346-429F-8AC7-17CCB1CA45BC", "contact", Xrm.Page.data.entity.getId(), 
        function () { 
            Xrm.Page.data.refresh(); 
        });
*/

var Process = Process || {};

// Supported Action input parameter types
Process.Type = {
    Bool: "c:boolean",
    Float: "c:double", // Not a typo
    Decimal: "c:decimal",
    Int: "c:int",
    String: "c:string",
    DateTime: "c:dateTime",
    Guid: "c:guid",
    EntityReference: "a:EntityReference",
    OptionSet: "a:OptionSetValue",
    Money: "a:Money",
    Entity: "a:Entity",
    EntityCollection: "a:EntityCollection"
}

// inputParams: Array of parameters to pass to the Action. Each param object should contain key, value, and type.
// successCallback: Function accepting 1 argument, which is an array of output params. Access values like: params["key"]
// errorCallback: Function accepting 1 argument, which is the string error message. Can be null.
// Unless the Action is global, you must specify a 'Target' input parameter as EntityReference
// actionName is required
Process.callAction = function (actionName, inputParams, successCallback, errorCallback, url) {
    var ns = {
        "": "http://schemas.microsoft.com/xrm/2011/Contracts/Services",
        ":s": "http://schemas.xmlsoap.org/soap/envelope/",
        ":a": "http://schemas.microsoft.com/xrm/2011/Contracts",
        ":i": "http://www.w3.org/2001/XMLSchema-instance",
        ":b": "http://schemas.datacontract.org/2004/07/System.Collections.Generic",
        ":c": "http://www.w3.org/2001/XMLSchema",
        ":d": "http://schemas.microsoft.com/xrm/2011/Contracts/Services",
        ":e": "http://schemas.microsoft.com/2003/10/Serialization/",
        ":f": "http://schemas.microsoft.com/2003/10/Serialization/Arrays",
        ":g": "http://schemas.microsoft.com/crm/2011/Contracts",
        ":h": "http://schemas.microsoft.com/xrm/2011/Metadata",
        ":j": "http://schemas.microsoft.com/xrm/2011/Metadata/Query",
        ":k": "http://schemas.microsoft.com/xrm/2013/Metadata",
        ":l": "http://schemas.microsoft.com/xrm/2012/Contracts",
        //":c": "http://schemas.microsoft.com/2003/10/Serialization/" // Conflicting namespace for guid... hardcoding in the _getXmlValue bit
    };

    var requestXml = "<s:Envelope";

    // Add all the namespaces
    for (var i in ns) {
        requestXml += " xmlns" + i + "='" + ns[i] + "'";
    }

    requestXml += ">" +
        "<s:Body>" +
        "<Execute>" +
        "<request>";

    if (inputParams != null && inputParams.length > 0) {
        requestXml += "<a:Parameters>";

        // Add each input param
        for (var i = 0; i < inputParams.length; i++) {
            var param = inputParams[i];

            var value = Process._getXmlValue(param.key, param.type, param.value);

            requestXml += value;
        }

        requestXml += "</a:Parameters>";
    }
    else {
        requestXml += "<a:Parameters />";
    }

    requestXml += "<a:RequestId i:nil='true' />" +
        "<a:RequestName>" + actionName + "</a:RequestName>" +
        "</request>" +
        "</Execute>" +
        "</s:Body>" +
        "</s:Envelope>";

    Process._callActionBase(requestXml, successCallback, errorCallback, url);
}

// Runs the specified workflow for a particular record
// successCallback and errorCallback accept no arguments
// workflowId, and recordId are required
Process.callWorkflow = function (workflowId, recordId, successCallback, errorCallback, url) {
    if (url == null) {
        url = Xrm.Page.context.getClientUrl();
    }

    var request = "<s:Envelope xmlns:s='http://schemas.xmlsoap.org/soap/envelope/'>" +
        "<s:Body>" +
        "<Execute xmlns='http://schemas.microsoft.com/xrm/2011/Contracts/Services' xmlns:i='http://www.w3.org/2001/XMLSchema-instance'>" +
        "<request i:type='b:ExecuteWorkflowRequest' xmlns:a='http://schemas.microsoft.com/xrm/2011/Contracts' xmlns:b='http://schemas.microsoft.com/crm/2011/Contracts'>" +
        "<a:Parameters xmlns:c='http://schemas.datacontract.org/2004/07/System.Collections.Generic'>" +
        "<a:KeyValuePairOfstringanyType>" +
        "<c:key>EntityId</c:key>" +
        "<c:value i:type='d:guid' xmlns:d='http://schemas.microsoft.com/2003/10/Serialization/'>" + recordId + "</c:value>" +
        "</a:KeyValuePairOfstringanyType>" +
        "<a:KeyValuePairOfstringanyType>" +
        "<c:key>WorkflowId</c:key>" +
        "<c:value i:type='d:guid' xmlns:d='http://schemas.microsoft.com/2003/10/Serialization/'>" + workflowId + "</c:value>" +
        "</a:KeyValuePairOfstringanyType>" +
        "</a:Parameters>" +
        "<a:RequestId i:nil='true' />" +
        "<a:RequestName>ExecuteWorkflow</a:RequestName>" +
        "</request>" +
        "</Execute>" +
        "</s:Body>" +
        "</s:Envelope>";

    var req = new XMLHttpRequest();
    req.open("POST", url + "/XRMServices/2011/Organization.svc/web", true);

    req.setRequestHeader("Accept", "application/xml, text/xml, */*");
    req.setRequestHeader("Content-Type", "text/xml; charset=utf-8");
    req.setRequestHeader("SOAPAction", "http://schemas.microsoft.com/xrm/2011/Contracts/Services/IOrganizationService/Execute");
    req.onreadystatechange = function () {
        if (req.readyState == 4) {
            if (req.status == 200) {
                if (successCallback) {
                    successCallback();
                }
            }
            else {
                if (errorCallback) {
                    errorCallback();
                }
            }
        }
    };

    req.send(request);
}

// Pops open the specified dialog process for a particular record
// dialogId, entityName, and recordId are required
// callback fires even if the dialog is closed/cancelled
Process.callDialog = function (dialogId, entityName, recordId, callback, url) {
    tryShowDialog("/cs/dialog/rundialog.aspx?DialogId=%7b" + dialogId + "%7d&EntityName=" + entityName + "&ObjectId=" + recordId, 600, 400, callback, url);

    // Function copied from Alert.js v1.0 https://alertjs.codeplex.com
    function tryShowDialog(url, width, height, callback, baseUrl) {
        width = width || Alert._dialogWidth;
        height = height || Alert._dialogHeight;

        var isOpened = false;

        try {
            // Web (IE, Chrome, FireFox)
            var Mscrm = Mscrm && Mscrm.CrmDialog && Mscrm.CrmUri && Mscrm.CrmUri.create ? Mscrm : parent.Mscrm;
            if (Mscrm && Mscrm.CrmDialog && Mscrm.CrmUri && Mscrm.CrmUri.create) {
                // Use CRM light-box (unsupported)
                var crmUrl = Mscrm.CrmUri.create(url);
                var dialogwindow = new Mscrm.CrmDialog(crmUrl, window, width, height);

                // Allows for opening non-webresources (e.g. dialog processes) - CRM messes up when it's not a web resource (unsupported)
                if (!crmUrl.get_isWebResource()) {
                    crmUrl.get_isWebResource = function () { return true; }
                }

                // Open the lightbox
                dialogwindow.show();
                isOpened = true;

                // Make sure when the dialog is closed, the callback is fired
                // This part's all pretty unsupported, hence the try-catches
                // If you can avoid using a callback, best not to use one
                if (callback) {
                    try {
                        // Get the lightbox iframe (unsupported)
                        var $frame = parent.$("#InlineDialog_Iframe");
                        if ($frame.length == 0) { $frame = parent.parent.$("#InlineDialog_Iframe"); }
                        $frame.load(function () {
                            try {
                                // Override the CRM closeWindow function (unsupported)
                                var frameDoc = $frame[0].contentWindow;
                                var closeEvt = frameDoc.closeWindow; // OOTB close function
                                frameDoc.closeWindow = function () {
                                    // Bypasses onunload event on dialogs to prevent "are you sure..." (unsupported - doesn't work with 2015 SP1)
                                    try { frameDoc.Mscrm.GlobalVars.$B = false; } catch (e) { }

                                    // Fire the callback and close
                                    callback();
                                    try { closeEvt(); } catch (e) { }
                                }
                            } catch (e) { }
                        });
                    } catch (e) { }
                }
            }
        } catch (e) { }

        try {
            // Outlook
            if (!isOpened && window.showModalDialog) {
                // If lightbox fails, use window.showModalDialog
                baseUrl = baseUrl || Xrm.Page.context.getClientUrl();
                var params = "dialogWidth:" + width + "px; dialogHeight:" + height + "px; status:no; scroll:no; help:no; resizable:yes";

                window.showModalDialog(baseUrl + url, window, params);
                if (callback) {
                    callback();
                }

                isOpened = true;
            }
        } catch (e) { }

        return isOpened;
    }
}

Process._emptyGuid = "00000000-0000-0000-0000-000000000000";

// This can be used to execute custom requests if needed - useful for me testing the SOAP :)
Process._callActionBase = function (requestXml, successCallback, errorCallback, url) {
    if (url == null) {
        url = Xrm.Page.context.getClientUrl();
    }

    var req = new XMLHttpRequest();
    req.open("POST", url + "/XRMServices/2011/Organization.svc/web", true);
    req.setRequestHeader("Accept", "application/xml, text/xml, */*");
    req.setRequestHeader("Content-Type", "text/xml; charset=utf-8");
    req.setRequestHeader("SOAPAction", "http://schemas.microsoft.com/xrm/2011/Contracts/Services/IOrganizationService/Execute");

    req.onreadystatechange = function () {
        if (req.readyState == 4) {
            if (req.status == 200) {
                // If there's no successCallback we don't need to check the outputParams
                if (successCallback) {
                    // Yucky but don't want to risk there being multiple 'Results' nodes or something
                    var resultsNode = req.responseXML.childNodes[0].childNodes[0].childNodes[0].childNodes[0].childNodes[1]; // <a:Results>

                    // Action completed successfully - get output params
                    var responseParams = Process._getChildNodes(resultsNode, "a:KeyValuePairOfstringanyType");

                    var outputParams = {};
                    for (i = 0; i < responseParams.length; i++) {
                        var attrNameNode = Process._getChildNode(responseParams[i], "b:key");
                        var attrValueNode = Process._getChildNode(responseParams[i], "b:value");

                        var attributeName = Process._getNodeTextValue(attrNameNode);
                        var attributeValue = Process._getValue(attrValueNode);

                        // v1.0 - Deprecated method using key/value pair and standard array
                        //outputParams.push({ key: attributeName, value: attributeValue.value });

                        // v2.0 - Allows accessing output params directly: outputParams["Target"].attributes["new_fieldname"];
                        outputParams[attributeName] = attributeValue.value;

                        /*
                        RETURN TYPES:
                            DateTime = Users local time (JavaScript date)
                            bool = true or false (JavaScript boolean)
                            OptionSet, int, decimal, float, etc = 1 (JavaScript number)
                            guid = string
                            EntityReference = { id: "guid", name: "name", entityType: "account" }
                            Entity = { logicalName: "account", id: "guid", attributes: {}, formattedValues: {} }
                            EntityCollection = [{ logicalName: "account", id: "guid", attributes: {}, formattedValues: {} }]

                        Attributes for entity accessed like: entity.attributes["new_fieldname"].value
                        For entityreference: entity.attributes["new_fieldname"].value.id
                        Make sure attributes["new_fieldname"] is not null before using .value
                        Or use the extension method entity.get("new_fieldname") to get the .value
                        Also use entity.formattedValues["new_fieldname"] to get the string value of optionsetvalues, bools, moneys, etc
                        */
                    }

                    // Make sure the callback accepts exactly 1 argument - use dynamic function if you want more
                    successCallback(outputParams);
                }
            }
            else {
                // Error has occured, action failed
                if (errorCallback) {
                    var message = null;
                    var traceText = null;
                    try {
                        message = Process._getNodeTextValueNotNull(req.responseXML.getElementsByTagName("Message"));
                        traceText = Process._getNodeTextValueNotNull(req.responseXML.getElementsByTagName("TraceText"));
                    } catch (e) { }
                    if (message == null) { message = "Error executing Action. Check input parameters or contact your CRM Administrator"; }
                    errorCallback(message, traceText);
                }
            }
        }
    };

    req.send(requestXml);
}

// Get only the immediate child nodes for a specific tag, otherwise entitycollections etc mess it up
Process._getChildNodes = function (node, childNodesName) {
    var childNodes = [];

    for (var i = 0; i < node.childNodes.length; i++) {
        if (node.childNodes[i].tagName == childNodesName) {
            childNodes.push(node.childNodes[i]);
        }
    }

    // Chrome uses just 'Results' instead of 'a:Results' etc
    if (childNodes.length == 0 && childNodesName.indexOf(":") !== -1) {
        childNodes = Process._getChildNodes(node, childNodesName.substring(childNodesName.indexOf(":") + 1));
    }

    return childNodes;
}

// Get a single child node for a specific tag
Process._getChildNode = function (node, childNodeName) {
    var nodes = Process._getChildNodes(node, childNodeName);

    if (nodes != null && nodes.length > 0) { return nodes[0]; }
    else { return null; }
}

// Gets the first not null value from a collection of nodes
Process._getNodeTextValueNotNull = function (nodes) {
    var value = "";

    for (var i = 0; i < nodes.length; i++) {
        if (value === "") {
            value = Process._getNodeTextValue(nodes[i]);
        }
    }

    return value;
}

// Gets the string value of the XML node
Process._getNodeTextValue = function (node) {
    if (node != null) {
        var textNode = node.firstChild;
        if (textNode != null) {
            return textNode.textContent || textNode.nodeValue || textNode.data || textNode.text;
        }
    }

    return "";
}

// Gets the value of a parameter based on its type, can be recursive for entities
Process._getValue = function (node) {
    var value = null;
    var type = null;

    if (node != null) {
        type = node.getAttribute("i:type") || node.getAttribute("type");

        // If the parameter/attribute is null, there won't be a type either
        if (type != null) {
            // Get the part after the ':' (since Chrome doesn't have the ':')
            var valueType = type.substring(type.indexOf(":") + 1).toLowerCase();

            if (valueType == "entityreference") {
                // Gets the lookup object
                var attrValueIdNode = Process._getChildNode(node, "a:Id");
                var attrValueEntityNode = Process._getChildNode(node, "a:LogicalName");
                var attrValueNameNode = Process._getChildNode(node, "a:Name");

                var lookupId = Process._getNodeTextValue(attrValueIdNode);
                var lookupName = Process._getNodeTextValue(attrValueNameNode);
                var lookupEntity = Process._getNodeTextValue(attrValueEntityNode);

                value = new Process.EntityReference(lookupEntity, lookupId, lookupName);
            }
            else if (valueType == "entity") {
                // Gets the entity data, and all attributes
                value = Process._getEntityData(node);
            }
            else if (valueType == "entitycollection") {
                // Loop through each entity, returns each entity, and all attributes
                var entitiesNode = Process._getChildNode(node, "a:Entities");
                var entityNodes = Process._getChildNodes(entitiesNode, "a:Entity");

                value = [];
                if (entityNodes != null && entityNodes.length > 0) {
                    for (var i = 0; i < entityNodes.length; i++) {
                        value.push(Process._getEntityData(entityNodes[i]));
                    }
                }
            }
            else if (valueType == "aliasedvalue") {
                // Gets the actual data type of the aliased value
                // Key for these is "alias.fieldname"
                var aliasedValue = Process._getValue(Process._getChildNode(node, "a:Value"));
                if (aliasedValue != null) {
                    value = aliasedValue.value;
                    type = aliasedValue.type;
                }
            }
            else {
                // Standard fields like string, int, date, money, optionset, float, bool, decimal
                // Output will be string, even for number fields etc
                var stringValue = Process._getNodeTextValue(node);

                if (stringValue != null) {
                    switch (valueType) {
                        case "datetime":
                            value = new Date(stringValue);
                            break;
                        case "int":
                        case "money":
                        case "optionsetvalue":
                        case "double": // float
                        case "decimal":
                            value = Number(stringValue);
                            break;
                        case "boolean":
                            value = stringValue.toLowerCase() === "true";
                            break;
                        default:
                            value = stringValue;
                    }
                }
            }
        }
    }

    return new Process.Attribute(value, type);
}

Process._getEntityData = function (entityNode) {
    var value = null;

    var entityAttrsNode = Process._getChildNode(entityNode, "a:Attributes");
    var entityIdNode = Process._getChildNode(entityNode, "a:Id");
    var entityLogicalNameNode = Process._getChildNode(entityNode, "a:LogicalName");
    var entityFormattedValuesNode = Process._getChildNode(entityNode, "a:FormattedValues");

    var entityLogicalName = Process._getNodeTextValue(entityLogicalNameNode);
    var entityId = Process._getNodeTextValue(entityIdNode);
    var entityAttrs = Process._getChildNodes(entityAttrsNode, "a:KeyValuePairOfstringanyType");

    value = new Process.Entity(entityLogicalName, entityId);

    // Attribute values accessed via entity.attributes["new_fieldname"]
    if (entityAttrs != null && entityAttrs.length > 0) {
        for (var i = 0; i < entityAttrs.length; i++) {

            var attrNameNode = Process._getChildNode(entityAttrs[i], "b:key")
            var attrValueNode = Process._getChildNode(entityAttrs[i], "b:value");

            var attributeName = Process._getNodeTextValue(attrNameNode);
            var attributeValue = Process._getValue(attrValueNode);

            value.attributes[attributeName] = attributeValue;
        }
    }

    // Formatted values accessed via entity.formattedValues["new_fieldname"]
    for (var j = 0; j < entityFormattedValuesNode.childNodes.length; j++) {
        var foNode = entityFormattedValuesNode.childNodes[j];

        var fNameNode = Process._getChildNode(foNode, "b:key")
        var fValueNode = Process._getChildNode(foNode, "b:value");

        var fName = Process._getNodeTextValue(fNameNode);
        var fValue = Process._getNodeTextValue(fValueNode);

        value.formattedValues[fName] = fValue;
    }

    return value;
}

Process._getXmlValue = function (key, dataType, value) {
    var xml = "";
    var xmlValue = "";

    var extraNamespace = "";

    // Check the param type to determine how the value is formed
    switch (dataType) {
        case Process.Type.String:
            xmlValue = Process._htmlEncode(value) || ""; // Allows fetchXml strings etc
            break;
        case Process.Type.DateTime:
            xmlValue = value.toISOString() || "";
            break;
        case Process.Type.EntityReference:
            xmlValue = "<a:Id>" + (value.id || "") + "</a:Id>" +
                "<a:LogicalName>" + (value.entityType || "") + "</a:LogicalName>" +
                "<a:Name i:nil='true' />";
            break;
        case Process.Type.OptionSet:
        case Process.Type.Money:
            xmlValue = "<a:Value>" + (value || 0) + "</a:Value>";
            break;
        case Process.Type.Entity:
            xmlValue = Process._getXmlEntityData(value);
            break;
        case Process.Type.EntityCollection:
            if (value != null && value.length > 0) {
                var entityCollection = "";
                for (var i = 0; i < value.length; i++) {
                    var entityData = Process._getXmlEntityData(value[i]);
                    if (entityData !== null) {
                        entityCollection += "<a:Entity>" + entityData + "</a:Entity>";
                    }
                }
                if (entityCollection !== null && entityCollection !== "") {
                    xmlValue = "<a:Entities>" + entityCollection + "</a:Entities>" +
                        "<a:EntityName i:nil='true' />" +
                        "<a:MinActiveRowVersion i:nil='true' />" +
                        "<a:MoreRecords>false</a:MoreRecords>" +
                        "<a:PagingCookie i:nil='true' />" +
                        "<a:TotalRecordCount>0</a:TotalRecordCount>" +
                        "<a:TotalRecordCountLimitExceeded>false</a:TotalRecordCountLimitExceeded>";
                }
            }
            break;
        case Process.Type.Guid:
            // I don't think guid fields can even be null?
            xmlValue = value || Process._emptyGuid;

            // This is a hacky fix to get guids working since they have a conflicting namespace :(
            extraNamespace = " xmlns:c='http://schemas.microsoft.com/2003/10/Serialization/'";
            break;
        default: // bool, int, double, decimal
            xmlValue = value != undefined ? value : null;
            break;
    }

    xml = "<a:KeyValuePairOfstringanyType>" +
        "<b:key>" + key + "</b:key>" +
        "<b:value i:type='" + dataType + "'" + extraNamespace;

    // nulls crash if you have a non-self-closing tag
    if (xmlValue === null || xmlValue === "") {
        xml += " i:nil='true' />";
    }
    else {
        xml += ">" + xmlValue + "</b:value>";
    }

    xml += "</a:KeyValuePairOfstringanyType>";

    return xml;
}

Process._getXmlEntityData = function (entity) {
    var xml = null;

    if (entity != null) {
        var attrXml = "";

        for (field in entity.attributes) {
            var a = entity.attributes[field];
            var aXml = Process._getXmlValue(field, a.type, a.value);

            attrXml += aXml;
        }

        if (attrXml !== "") {
            xml = "<a:Attributes>" + attrXml + "</a:Attributes>";
        }
        else {
            xml = "<a:Attributes />";
        }

        xml += "<a:EntityState i:nil='true' />" +
            "<a:FormattedValues />" +
            "<a:Id>" + entity.id + "</a:Id>" +
            "<a:KeyAttributes />" +
            "<a:LogicalName>" + entity.logicalName + "</a:LogicalName>" +
            "<a:RelatedEntities />" +
            "<a:RowVersion i:nil='true' />";
    }

    return xml;
}

Process._htmlEncode = function (s) {
    if (typeof s !== "string") { return s; }

    return s.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
}

Process.Entity = function (logicalName, id, attributes) {
    this.logicalName = logicalName || "";
    this.attributes = attributes || {};
    this.formattedValues = {};
    this.id = id || Process._emptyGuid;
}

// Gets the value of the attribute without having to check null
Process.Entity.prototype.get = function (key) {
    var a = this.attributes[key];
    if (a != null) {
        return a.value;
    }

    return null;
}

Process.EntityReference = function (entityType, id, name) {
    this.id = id || Process._emptyGuid;
    this.name = name || "";
    this.entityType = entityType || "";
}

Process.Attribute = function (value, type) {
    this.value = value != undefined ? value : null;
    this.type = type || "";
}

///////////////////////////////////////////////////////////////////////////////////////////////////////
var previousStatus;

function SetName_onSave(executionContext) {
    var descptionArray = null;
    var formContext = executionContext.getFormContext();
    var selectedNameFromCharArray = "";
    //debugger;
    var description = formContext.getAttribute("msdyn_description").getValue();
    if (description != null) {

        var descriptionCharLength = description.length;

        //var descptionArray = description.trim().split(" ");
        //var length = descptionArray.length;

        if (descriptionCharLength > 80) {
            for (var i = 0; i < 80; i++) {
                selectedNameFromCharArray += description[i];
                //selectedNameFromArray += " ";

            }
        }
        else {
            selectedNameFromCharArray = description;
        }

        formContext.getAttribute("msdyn_name").setValue(selectedNameFromCharArray);

    }


    //var servicetask = formContext.getAttribute("ap360_servicetaskid").getValue();

    //var servicetask = formContext.getAttribute("ap360_servicetaskid").getValue();
    //if (servicetask != null) {
    //    servicetask = servicetask[0].name;
    //}
    //var workrequested = formContext.getAttribute("ap360_workrequested").getValue();
    //if (servicetask == null) {
    //    formContext.getAttribute("msdyn_name").setValue(workrequested);
    //} else if (workrequested == null) {
    //    formContext.getAttribute("msdyn_name").setValue(servicetask);
    //} else if (workrequested != null && servicetask != null) {
    //    formContext.getAttribute("msdyn_name").setValue(servicetask + " " + workrequested);
    //}
}


function ChangeUnMandatoryRevisedEstiamtedDurtion_OnChangeIsRevised(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var isrevised = formContext.getAttribute("ap360_isrevised").getValue();

    if (!isrevised) {

        formContext.getAttribute("ap360_revisedestimatedduration").setRequiredLevel("none");

    }
    else {
        formContext.getAttribute("ap360_revisedestimatedduration").setRequiredLevel("required");
    }


}

function onLoad_Main() {
    debugger;
    try {
        Xrm.Page.data.process.addOnStageSelected(hideShowAssessmentTask);
        hideShowAssessmentTask();
        FilterServiceOperation();

        formContext.getControl("ap360_workorderservicetasksubstatus").setVisible(false);
        formContext.getAttribute("ap360_workorderservicetasksubstatus").setRequiredLevel("none");
    }
    catch (e) {
        console.log("onLoad_Main:" + e);
    }
}


function ManageWOSTSubStatusOnChangeOfWOSTStatus(executionContext) {
    var formContext = executionContext.getFormContext();
    var WOSTStatus = formContext.getAttribute("ap360_workorderservicetaskstatus").getValue();
    if (WOSTStatus == 126300008)//Incomplete- Return
    {
        formContext.getControl("ap360_workorderservicetasksubstatus").setVisible(true);
        formContext.getAttribute("ap360_workorderservicetasksubstatus").setRequiredLevel("required");

        formContext.getControl("ap360_workorderservicetasksubstatus").clearOptions();

        formContext.getControl("ap360_workorderservicetasksubstatus").addOption({ text: "Inc-Needs Lead Tech Guidance", value: 126300006 });
        formContext.getControl("ap360_workorderservicetasksubstatus").addOption({ text: "Inc- Needs MGR Decision", value: 126300007 });
        formContext.getControl("ap360_workorderservicetasksubstatus").addOption({ text: "Waiting on Part", value: 126300005 });
        formContext.getControl("ap360_workorderservicetasksubstatus").addOption({ text: "Efficiency Improvement Opportunity", value: 126300004 });
        formContext.getControl("ap360_workorderservicetasksubstatus").addOption({ text: "Assist CoWorker", value: 126300003 });
        formContext.getControl("ap360_workorderservicetasksubstatus").addOption({ text: "Personal", value: 126300000 });
        formContext.getControl("ap360_workorderservicetasksubstatus").addOption({ text: "End of Day", value: 126300002 });
        formContext.getControl("ap360_workorderservicetasksubstatus").addOption({ text: "Lunch", value: 126300001 });

    }
    else if (WOSTStatus == 126300005) //inc-needs billed child Service Task
    {

        formContext.getControl("ap360_workorderservicetasksubstatus").clearOptions();

        formContext.getControl("ap360_workorderservicetasksubstatus").setVisible(true);
        formContext.getAttribute("ap360_workorderservicetasksubstatus").setRequiredLevel("required");


        formContext.getControl("ap360_workorderservicetasksubstatus").addOption({ text: "Incorrect Part Buried", value: 126300011 });
        formContext.getControl("ap360_workorderservicetasksubstatus").addOption({ text: "Defective New Part Provided", value: 126300010 });
        formContext.getControl("ap360_workorderservicetasksubstatus").addOption({ text: "Poor OOB part fitment", value: 126300009 });
        formContext.getControl("ap360_workorderservicetasksubstatus").addOption({ text: "Planned Time Insufficient", value: 126300008 });

    }
    else if (WOSTStatus == 126300006)//inc-needs unbilled child Service Task
    {


        formContext.getControl("ap360_workorderservicetasksubstatus").clearOptions();
        formContext.getControl("ap360_workorderservicetasksubstatus").setVisible(true);
        formContext.getAttribute("ap360_workorderservicetasksubstatus").setRequiredLevel("required");


        formContext.getControl("ap360_workorderservicetasksubstatus").addOption({ text: "For Clean up", value: 126300016 });
        formContext.getControl("ap360_workorderservicetasksubstatus").addOption({ text: "We damaged", value: 126300015 });
        formContext.getControl("ap360_workorderservicetasksubstatus").addOption({ text: "Should have asked for help sooner", value: 126300014 });
        formContext.getControl("ap360_workorderservicetasksubstatus").addOption({ text: "Overestimated Capability", value: 126300013 });
        formContext.getControl("ap360_workorderservicetasksubstatus").addOption({ text: "Misused Allotted Time", value: 126300012 });


    }
    else {

        formContext.getControl("ap360_workorderservicetasksubstatus").setVisible(false);
        formContext.getAttribute("ap360_workorderservicetasksubstatus").setRequiredLevel("none");
    }

}


function onload_AdjustOptionalOrMadatoryField(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    formContext.getControl("ap360_workorderservicetasksubstatus").setVisible(false);
    formContext.getAttribute("ap360_workorderservicetasksubstatus").setRequiredLevel("none");
    var ap360_isrevised = formContext.getAttribute("ap360_isrevised").getValue();
    if (ap360_isrevised == false) {
        formContext.getAttribute("ap360_servicetaskid").setRequiredLevel("none"); // optional
    }
    else {
        formContext.getAttribute("ap360_servicetaskid").setRequiredLevel("required"); //required

    }
}
function lockField(executionContext) {
    var formContext = executionContext.getFormContext();

    var formType = formContext.ui.getFormType();
    if (formType != 1)//Create
    {
        //formContext.getControl("ap360_revisedestimatedduration").setVisible(false);

        formContext.getControl("ap360_revisedestimatedduration").setDisabled(true);

    }


}
function OnChange_RevisedEstimatedDuration(executionContext) {
    var formContext = executionContext.getFormContext();
    var ap360_revisedestimatedduration = formContext.getAttribute("ap360_revisedestimatedduration").getValue();
    if (ap360_revisedestimatedduration != null) {
        var ap360_hourlyrate = formContext.getAttribute("ap360_hourlyrate").getValue();
        if (ap360_hourlyrate != null) {
            formContext.getAttribute("ap360_revisedestimatedamount").setValue((ap360_hourlyrate * ap360_revisedestimatedduration) / 60);


            var ap360_predictedspend = formContext.getAttribute("ap360_predictedspend").getValue();
            if (ap360_predictedspend == null) {

                formContext.getAttribute("ap360_predictedspend").setValue((ap360_hourlyrate * ap360_revisedestimatedduration) / 60);

            }

            formContext.getAttribute("ap360_estimatedlaborprice").setValue((ap360_hourlyrate * ap360_revisedestimatedduration) / 60);


        }



    }
    else {
        formContext.getAttribute("ap360_revisedestimatedamount").setValue(0);
        formContext.getAttribute("ap360_predictedspend").setValue(0);
        formContext.getAttribute("ap360_estimatedlaborprice").setValue(0);


    }
}

function hideShowAssessmentTask() {

    var _SelectedStage = Xrm.Page.data.process.getSelectedStage();
    console.log("Stage: " + _SelectedStage);
    if (_SelectedStage.getName() == "Assessment Result") {
        Xrm.Page.ui.tabs.get("Assessment Task").setVisible(true);
    }

    else {
        Xrm.Page.ui.tabs.get("Assessment Task").setVisible(false);

    }

}

function toggleCompleteasNotOK() {
    var isAssessment = Xrm.Page.getAttribute("ap360_isassessment").getValue();
    if (isAssessment == true)
        return true;
    else
        return false;
}

function FilterServiceOperation() {
    try {
        console.log("in FilterServiceOperation");
        var WOType = Xrm.Page.getAttribute("ap360_workordertype").getValue()[0].name;
        if (WOType == "Admin") {
            var viewUserId = "{8c0b9d3f-bc33-4ccd-a6e0-fb69bd03307c}";
            var userEntityName = "msdyn_servicetasktype";
            var viewUserDisplayName = "Filtered Service Operation";
            var fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                "<entity name='msdyn_servicetasktype'>" +
                "<attribute name='msdyn_name' />" +
                "<attribute name='createdon' />" +
                "<attribute name='msdyn_estimatedduration' />" +
                "<attribute name='msdyn_description' />" +
                "<attribute name='msdyn_servicetasktypeid' />" +
                "<order attribute='msdyn_name' descending='false' />" +
                "<filter type='and'>" +
                "<condition attribute='ap360_parentservicetasktype' operator='eq' uiname='Admin' uitype='msdyn_servicetasktype' value='{72C739E2-2D21-E811-A96A-000D3A1A7FA7}'/>" +
                "</filter>" +
                "</entity>" +
                "</fetch>";
            console.log("fetchXml : " + fetchXml);
            var userLayoutXml = "<grid name=\"resultset\" object=\"8\" jump=\"name\" select=\"1\" preview=\"1\" icon=\"1\">" + "<row name=\"result\" id=\"msdyn_servicetasktypeid\">" + "<cell name=\"msdyn_name\" width=\"150\"/><cell name=\"msdyn_description\" width=\"150\"/></row></grid>";
            Xrm.Page.getControl("msdyn_tasktype").addCustomView(viewUserId, userEntityName, viewUserDisplayName, fetchXml, userLayoutXml, true);
        }
        else {
            var viewUserId = "{8c0b9d3f-bc33-4ccd-a6e0-fb69bd03307c}";
            var userEntityName = "msdyn_servicetasktype";
            var viewUserDisplayName = "Filtered Service Operation";
            var fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                "<entity name='msdyn_servicetasktype'>" +
                "<attribute name='msdyn_name' />" +
                "<attribute name='createdon' />" +
                "<attribute name='msdyn_estimatedduration' />" +
                "<attribute name='msdyn_description' />" +
                "<attribute name='msdyn_servicetasktypeid' />" +
                "<order attribute='msdyn_name' descending='false' />" +
                "<filter type='and'>" +
                "<condition attribute='ap360_parentservicetasktype' operator='ne' uiname='Admin' uitype='msdyn_servicetasktype' value='{72C739E2-2D21-E811-A96A-000D3A1A7FA7}'/>" +
                "</filter>" +
                "</entity>" +
                "</fetch>";
            console.log("fetchXml : " + fetchXml);
            var userLayoutXml = "<grid name=\"resultset\" object=\"8\" jump=\"name\" select=\"1\" preview=\"1\" icon=\"1\">" + "<row name=\"result\" id=\"msdyn_servicetasktypeid\">" + "<cell name=\"msdyn_name\" width=\"150\"/><cell name=\"msdyn_description\" width=\"150\"/></row></grid>";
            Xrm.Page.getControl("msdyn_tasktype").addCustomView(viewUserId, userEntityName, viewUserDisplayName, fetchXml, userLayoutXml, true);

        }

    }
    catch (e) {
        console.log("FilterServiceOperation: " + e);
    }
}
function isValidForBookingCreation(formContext) {

    debugger;
    var isValidtoCreateBooking = false;

    var msdyn_workorder = formContext.getAttribute("msdyn_workorder").getValue();
    if (msdyn_workorder != null) {
        msdyn_workorder = msdyn_workorder[0].id.replace('{', '').replace('}', '');

        var req = new XMLHttpRequest();
        var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/msdyn_workorders(" + msdyn_workorder + ")?$select=msdyn_systemstatus";
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
                var msdyn_systemstatus = result["msdyn_systemstatus"];
                var msdyn_systemstatus_formatted = result["msdyn_systemstatus@OData.Community.Display.V1.FormattedValue"];

                if (msdyn_systemstatus == 690970004 || msdyn_systemstatus == 690970005)//Closed - Posted(690970004)   Closed - Canceled(690970005)
                {
                    isValidtoCreateBooking = false;
                    if (msdyn_systemstatus == 690970004)//Closed - Posted(690970004)
                    {
                        alert("Booking can't be Created for 'Closed-Posted' Work Order ");
                        formContext.ui.clearFormNotification("1");

                        // return;
                    }
                    else if (msdyn_systemstatus == 690970005)//Closed - Canceled(690970005)
                    {
                        alert("Booking can't be Created for 'Closed-Canceled' Work Order ");
                        formContext.ui.clearFormNotification("1");

                        //  return;

                    }

                }
                else {

                    isValidtoCreateBooking = true;

                }

            } else {
                Xrm.Utility.alertDialog(req.statusText);
            }
        }

    }


    return isValidtoCreateBooking;

}


function isValidforBookingCreationFunction(formContext) {
    var isValidtoCreateBooking = isValidForBookingCreation(formContext);
    if (isValidtoCreateBooking) {
        try {
            var url = Xrm.Page.context.getClientUrl();
            var recordId = Xrm.Page.data.entity.getId();
            var workflowId = "aa63bf7e-2cd5-44a6-a7cd-5c83b63a9f21";//WF: WOST: Book Work Order Service Task to Current User

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
            // Xrm.Page.ui.close();

        }
        catch (e) {
            alert("Error Occured. Contact CRM Administrator");
            formContext.ui.clearFormNotification("1");

            console.log("RunGenerateWelcomeLetterWorkflow: " + e);
        }
    }
}
function assignResponse(req) {
    if (req.readyState == 4) {
        if (req.status == 200) {
            alert("Booking Created");
            Xrm.Page.ui.clearFormNotification("1");

            console.log('successfully executed the workflow');
        }
    }
}

function SetParentServiceTask(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();


    var childservicetemplate = formContext.getAttribute("ap360_childservicetemplateid").getValue();

    if (childservicetemplate != null) {

        Xrm.WebApi.retrieveRecord("ap360_servicetemplatetype",
            childservicetemplate[0].id,
            "?$select=_ap360_parentservicetaskid_value")
            .then(function (serviceOperation) {
                debugger;
                var lookupValue = new Array();
                lookupValue[0] = new Object();
                lookupValue[0].id = serviceOperation._ap360_parentservicetaskid_value;
                lookupValue[0].name = serviceOperation["_ap360_parentservicetaskid_value@OData.Community.Display.V1.FormattedValue"];
                lookupValue[0].entityType = "msdyn_servicetasktype";

                formContext.getAttribute("ap360_parentservicetaskid").setValue(lookupValue);
            });
    }
    else {

        formContext.getAttribute("ap360_parentservicetaskid").setValue(null);

    }
}
function SetServiceRoleAndHourlyRate(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();


    var childservicetemplate = formContext.getAttribute("ap360_childservicetemplateid").getValue();

    if (childservicetemplate != null) {

        Xrm.WebApi.retrieveRecord("ap360_servicetemplatetype",
            childservicetemplate[0].id,
            "?$select=_ap360_servicerole_value")
            .then(function (serviceOperation) {
                if (serviceOperation != null) {
                    debugger;
                    var lookupValue = new Array();
                    lookupValue[0] = new Object();
                    lookupValue[0].id = serviceOperation._ap360_servicerole_value;
                    lookupValue[0].name = serviceOperation["_ap360_servicerole_value@OData.Community.Display.V1.FormattedValue"];
                    lookupValue[0].entityType = "bookableresourcecategory";
                    setServiceRoleHorlyRate(lookupValue[0].id, formContext);
                    formContext.getAttribute("ap360_serviceroleid").setValue(lookupValue);

                }
                else {
                    alert("Service Role not exists for selected Child Service Template Type ");
                }
            });
    }
    else {

        formContext.getAttribute("ap360_serviceroleid").setValue(null);
        formContext.getAttribute("ap360_hourlyrate").setValue(null);

    }

}

function setServiceRoleHorlyRate(serviceRoleId, formContext) {

    var HourlyRate = 0;
    Xrm.WebApi.retrieveRecord("bookableresourcecategory",
        serviceRoleId,
        "?$select=ap360_price")
        .then(function (result) {
            if (result != null) {
                debugger;

                formContext.getAttribute("ap360_hourlyrate").setValue(result["ap360_price"]);

            }
            else {
                alert("Horly Rate is 0");
            }
        });
    return HourlyRate;

}



function RemoveTemplateHierarchy_OnChangeOfRemoveTemplateHierarchy(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var removetemplatehierarchy = formContext.getAttribute("ap360_removetemplatehierarchy").getValue();
    if (removetemplatehierarchy == 1) {
        //var lookupData = new Array();
        //var lookupItem = new Object();
        //lookupItem.id = "62a07066-e3ed-e911-a811-000d3a579c6d"; // Vehicle Product
        //lookupItem.name = "Vehicle"; // Entity record name
        //lookupItem.entityType = "product";
        //lookupData[0] = lookupItem;
        //formContext.getAttribute("ap360_serviceproductmappingid").setValue(lookupData);
        //formContext.getAttribute("ap360_name").setValue("Vehicle");
        //formContext.getAttribute("ap360_description").setValue("Vehicle");


        //formContext.getControl("ap360_ggparent").setVisible(true);
        //formContext.getControl("ap360_parentservicetemplateid").setVisible(true);
        //formContext.getControl("ap360_servicetemplateid").setVisible(true);
        //formContext.getControl("ap360_childservicetemplateid").setVisible(true);
        //formContext.getControl("ap360_parentservicetaskid").setVisible(true);
        //formContext.getControl("ap360_serviceroleid").setVisible(true);
        //formContext.getControl("ap360_hourlyrate").setVisible(true);


        //formContext.getControl("ap360_ggparent").setVisible(true);
        formContext.getAttribute("ap360_parentservicetemplateid").setValue(null);
        formContext.getAttribute("ap360_servicetemplateid").setValue(null);
        formContext.getAttribute("ap360_childservicetemplateid").setValue(null);
        formContext.getAttribute("ap360_parentservicetaskid").setValue(null);

        //formContext.getAttribute("ap360_serviceroleid").setValue(null);
        // formContext.getAttribute("ap360_lockedserviceroleforquoteservicid").setValue(null);
        //formContext.getAttribute("ap360_hourlyrate").setValue(null);
        formContext.getControl("ap360_removetemplatehierarchy").setDisabled(true);




    }



}



function Timelimitonrevisedestimatedtime(executionContext) {
    var formContext = executionContext.getFormContext();
    var revisedesmatedtime = formContext.getAttribute("ap360_revisedestimatedduration").getValue();
    if (revisedesmatedtime > 240) {
        Xrm.Page.getControl("ap360_revisedestimatedduration").setNotification("Time should not more than 4 hour", "removeestimatedtimenotification")
    }
    else {
        Xrm.Page.getControl("ap360_revisedestimatedduration").clearNotification("removeestimatedtimenotification");
    }
}


function makeFormReadOnlyonWOClosedPosted(executionContext) {
    var formContext = executionContext.getFormContext();
    //Just inorder to use it on form load to get previous Status of WOST
    previousStatus = formContext.getAttribute("ap360_workorderservicetaskstatus").getValue();
    var msdyn_workorder = formContext.getAttribute("msdyn_workorder").getValue();
    if (msdyn_workorder != null) {
        msdyn_workorder = msdyn_workorder[0].id.replace('{', '').replace('}', '');

        var req = new XMLHttpRequest();
        var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/msdyn_workorders(" + msdyn_workorder + ")?$select=msdyn_systemstatus";
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
                var msdyn_systemstatus = result["msdyn_systemstatus"];
                var msdyn_systemstatus_formatted = result["msdyn_systemstatus@OData.Community.Display.V1.FormattedValue"];

                if (msdyn_systemstatus == 690970004)//Closed - Posted(690970004)
                {
                    var cs = formContext.ui.controls.get();
                    for (var i in cs) {
                        var c = cs[i];
                        if (c.getName() != "" && c.getName() != null) {
                            if (!c.getDisabled()) { c.setDisabled(true); }
                        }
                    }
                }
            } else {
                Xrm.Utility.alertDialog(req.statusText);
            }
        }

    }
}
function SaveAndNewForm(primaryControl) {
    //alert("Working");
    var formContext = primaryControl;
    formContext.data.entity.save();

    var formParameters = {};
    var entityOptions = {};
    entityOptions["entityName"] = "msdyn_workorderservicetask";

    var removetemplatehierarchy = formContext.getAttribute("ap360_removetemplatehierarchy").getValue();
    formParameters["ap360_removetemplatehierarchy"] = removetemplatehierarchy;
    var parentservicetemplate = formContext.getAttribute("ap360_parentservicetemplateid").getValue();
    if (parentservicetemplate != null) {
        formParameters["ap360_parentservicetemplateid"] = parentservicetemplate;
    }
    var servicetemplate = formContext.getAttribute("ap360_servicetemplateid").getValue();
    if (servicetemplate != null) {
        formParameters["ap360_servicetemplateid"] = servicetemplate;
    }
    var childservicetemplate = formContext.getAttribute("ap360_childservicetemplateid").getValue();
    if (childservicetemplate != null) {
        formParameters["ap360_childservicetemplateid"] = childservicetemplate;
    }
    var parentservicetask = formContext.getAttribute("ap360_parentservicetaskid").getValue();
    if (parentservicetask != null) {
        formParameters["ap360_parentservicetaskid"] = parentservicetask;
    }
    var servicerole = formContext.getAttribute("ap360_serviceroleid").getValue();
    if (servicerole != null) {
        formParameters["ap360_serviceroleid"] = servicerole;
    }
    var hourlyrate = formContext.getAttribute("ap360_hourlyrate").getValue();
    if (hourlyrate != null) {
        formParameters["ap360_hourlyrate"] = hourlyrate;
    }
    var opportunity = formContext.getAttribute("ap360_opportunityid").getValue();
    if (opportunity != null) {
        formParameters["ap360_opportunityid"] = opportunity;
    }
    var workorder = formContext.getAttribute("msdyn_workorder").getValue();
    if (workorder != null) {
        formParameters["msdyn_workorder"] = workorder;

        Xrm.Navigation.openForm(entityOptions, formParameters).then(
            function (lookup) { console.log("Success"); },
            function (error) { console.log("Error"); });
    }

    else {


        alert("Can not use 'Save & New' because Quote Service is not selected");
    }




}

function RunGenerateBookingWorkflow(primaryControl) {
    debugger;
    var formContext = primaryControl; // rename as formContext
    var revisedItemStatus = formContext.getAttribute("ap360_reviseditemstatus").getValue();
    var wostGuid = null;
    var orgUrl = null;
    wostGuid = formContext.data.entity.getId();
    orgUrl = formContext.context.getClientUrl();
    if (revisedItemStatus == 126300000)//Approved 
    {
        Xrm.Navigation.openAlertDialog("Revised item status not Approved");
        return;
    } else if (revisedItemStatus == 126300002)//Rejected
    {
        Xrm.Navigation.openAlertDialog("Revised item status rejected");
        return;
    }
    var journeymanactualduration = formContext.getAttribute("ap360_journeymanactualduration").getValue();
    var durationEstimated = formContext.getAttribute("msdyn_estimatedduration").getValue();//Origianl Estimated duration
    if (durationEstimated == 0 || durationEstimated == null) {
        durationEstimated = formContext.getAttribute("ap360_revisedestimatedduration").getValue();
    }
    //if ((durationEstimated == 0 || durationEstimated == null) && journeymanactualduration > 60) {
    //    Xrm.Navigation.openAlertDialog("Booking cannot be started as 60 Minutes spent based on Journey man");
    //    return;
    //}
    //else
    //if (!isNaN(parseInt(durationEstimated))) {
    //    if (journeymanactualduration > durationEstimated > 0) {
    //        Xrm.Navigation.openAlertDialog("Booking Cannot be Picked As Journey man actual duration is greater then Estimated Duration");
    //        return;
    //    }
    //}


    var serviceroleid = null;
    serviceroleid = formContext.getAttribute("ap360_serviceroleid").getValue();
    if (serviceroleid == null) {
        Xrm.Utility.alertDialog("Service Role is not mapped. Contact CRM Admin to map Service Role");
    }
    else {
        var msdyn_percentcomplete = formContext.getAttribute("msdyn_percentcomplete");
        if (msdyn_percentcomplete != null) {
            if (msdyn_percentcomplete.getValue() == 100) {
                Xrm.Utility.alertDialog("Work Order Servcie Task is Completed");
                return;
            }
        }
        var delayTime = 30000; // 30 seconds in milliseconds 
        // formContext.ui.setFormNotification("Please wait while Booking is created", "INFO", "infoid");
        Xrm.Utility.showProgressIndicator("Please wait, booking is creating...");
        callActionForPickWOSTAndStartWork(wostGuid, orgUrl, "PickTask");
        //setTimeout(isValidforBookingCreationFunction(formContext), delayTime);
    }

}

function PickWOSTAndStartWork(primaryControl, selectedEntityTypeName, firstSelectedItemId)//startwork in ribbon workbecnh 
{
    debugger;
    var formContext = primaryControl;
    var wostGuid = null;
    var orgUrl = null;

    var RevisedItemapproved = true;
    Xrm.Utility.showProgressIndicator("Please wait, booking is starting...");

    var isBookingAlreadyCreated = checkIfBokingIsAlreadyCreatedOrNot(formContext);
    if (isBookingAlreadyCreated == true) {
        Xrm.Navigation.openAlertDialog("You already have booking In-Progress.");
        Xrm.Utility.closeProgressIndicator();
        return;
    }

    if (firstSelectedItemId != null) {
        wostGuid = firstSelectedItemId

        if (wostGuid == "{EE8099A8-156B-EB11-A812-00224802B3CE}")//Shop task ID
        {
            Xrm.Navigation.openAlertDialog("Cannot start Booking service of Shop Task");
            Xrm.Utility.closeProgressIndicator();
            return;
        }

        if (primaryControl._grid != null)
            orgUrl = primaryControl._grid._applicationContext.DataSource._serverUri; //inCase of Home grid/ Page
        else
            orgUrl = formContext.context.getClientUrl();//Incase of subgrid


        var revisedItemStatus = getRevisedItemStatusOfSelectedItemId(wostGuid);

        if (revisedItemStatus == 126300000)//Approved 
        {
            Xrm.Navigation.openAlertDialog("Revised item status not Approved");
            Xrm.Utility.closeProgressIndicator();
            return;
        } else if (revisedItemStatus == 126300002)//Rejected
        {
            Xrm.Navigation.openAlertDialog("Revised item status rejected");
            Xrm.Utility.closeProgressIndicator();
            return;
        }
        //wostGuid = formContext.data.entity.getId();
        //orgUrl = formContext.context.getClientUrl();
    }
    else {
        wostGuid = formContext.data.entity.getId();

        if (wostGuid == "{EE8099A8-156B-EB11-A812-00224802B3CE}")//Shop task ID
        {
            Xrm.Navigation.openAlertDialog("Cannot start Booking service of Shop Task");
            Xrm.Utility.closeProgressIndicator();
            return;
        }

        orgUrl = formContext.context.getClientUrl(); //Incase of Form


        var revisedItemStatus = formContext.getAttribute("ap360_reviseditemstatus").getValue();

        if (revisedItemStatus == 126300000)//Approved 
        {
            Xrm.Navigation.openAlertDialog("Revised item status not Approved");
            Xrm.Utility.closeProgressIndicator();
            return;
        } else if (revisedItemStatus == 126300002)//Rejected
        {
            Xrm.Navigation.openAlertDialog("Revised item status rejected");
            Xrm.Utility.closeProgressIndicator();
            return;
        }

    }


    wostGuid = wostGuid.replace('{', '').replace('}', '');

    //var timeStamp = getWOSTTimes(wostGuid);
    ////if ((timeStamp.estimatedduration == 0 || timeStamp.estimatedduration == null) && timeStamp.ap360_journeymanactualduration > 60) {
    ////    Xrm.Navigation.openAlertDialog("Booking Cannot be started as 60 Minutes spent based on Journey man");
    ////    return;
    ////}
    ////else
    //if (!isNaN(parseInt(timeStamp.estimatedduration))) {

    //    if (timeStamp.ap360_journeymanactualduration > timeStamp.estimatedduration > 0) {
    //        Xrm.Navigation.openAlertDialog("Booking Cannot be started as Journey man actual duration is greater then Estimated Duration");
    //        return;
    //    }
    //}
    callActionForPickWOSTAndStartWork(wostGuid, orgUrl, "StartWork");

}




function getRevisedItemStatusOfSelectedItemId(wostGuid) {
    wostGuid = wostGuid.replace('{', '').replace('}', '');
    var revisedItemStatus;

    var req = new XMLHttpRequest();
    var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/msdyn_workorderservicetasks(" + wostGuid + ")?$select=ap360_reviseditemstatus";
    req.open("GET", url, false);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
    req.send();

    if (req.status === 200) {
        var results = JSON.parse(req.response);

        revisedItemStatus = results["ap360_reviseditemstatus"];
    } else {
        Xrm.Utility.alertDialog(req.statusText);
    }

    return revisedItemStatus;

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

function getWOSTTimes(WOSTGuid) {
    var timeStamps = [];
    //var WOSTGuid = formContext.data.entity.getId();
    //WOSTGuid = WOSTGuid.replace('{', '').replace('}', '');

    var req = new XMLHttpRequest();
    var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/msdyn_workorderservicetasks(" + WOSTGuid + ")?$select=ap360_journeymanactualduration,msdyn_estimatedduration,ap360_revisedestimatedduration";
    req.open("GET", url, false);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
    req.send();

    if (req.status === 200) {
        var results = JSON.parse(req.response);
        timeStamps["ap360_journeymanactualduration"] = results["ap360_journeymanactualduration"];
        results["ap360_revisedestimatedduration"] == null ? timeStamps["estimatedduration"] = results["msdyn_estimatedduration"] : timeStamps["estimatedduration"] = results["ap360_revisedestimatedduration"];
        var ap360_journeymanactualduration_formatted = results["ap360_journeymanactualduration@OData.Community.Display.V1.FormattedValue"];

        var msdyn_estimatedduration_formatted = results["msdyn_estimatedduration@OData.Community.Display.V1.FormattedValue"];
    } else {
        Xrm.Utility.alertDialog(req.statusText);
    }

    return timeStamps;
}

function callActionForPickWOSTAndStartWork(wostGuid, orgUrl, bookingType) {


    var parameters = {};
    parameters.WOSTGuid = wostGuid;
    parameters.BookingType = bookingType;

    var req = new XMLHttpRequest();
    req.open("POST", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ap360_PickAndStartWOSTBooking", false);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.send(JSON.stringify(parameters));
    if (req.readyState === 4) {
        req.onreadystatechange = null;
        if (req.status === 200) {
            var results = JSON.parse(req.response);
            if (bookingType == "StartWork") {
                setTimeout(openBookingInNewWindow(orgUrl, results), 1);
            }
            else {
                Xrm.Utility.closeProgressIndicator();
                Xrm.Utility.alertDialog("Booking scheduled!");
            }
            Xrm.Utility.closeProgressIndicator();
        } else if (req.status == 400) {
            var response = req.response;
            const responseObj = JSON.parse(response);
            Xrm.Utility.alertDialog(responseObj.error.message);
            Xrm.Utility.closeProgressIndicator();

        }
        else {
            Xrm.Utility.alertDialog(req.statusText);
            Xrm.Utility.closeProgressIndicator();

        }
    }
}



function openBookingInNewWindow(orgUrl, params) {

    if (params.NewlyCreatedBookingGuid != null) {
        // reteriveCurrentBooking(orgUrl,params.NewlyCreatedBookingGuid);
        //window.open(formContext.context.getClientUrl() + "/main.aspx?appid=ab2029aa-0f66-ea11-a811-000d3a33f47e&cmdbar=true&forceUCI=1&pagetype=entityrecord&etn=bookableresourcebooking&id=" + params.NewlyCreatedBookingGuid)
        window.open(orgUrl + "/main.aspx?appid=ab2029aa-0f66-ea11-a811-000d3a33f47e&cmdbar=true&forceUCI=1&pagetype=entityrecord&etn=bookableresourcebooking&id=" + params.NewlyCreatedBookingGuid)
        // window.open(Xrm.Page.context.getClientUrl() + "/main.aspx?settingsonly=true#963810963")
        console.log("Newly created booking ID: " + params.NewlyCreatedBookingGuid);
        //Xrm.Page.getControl("workorderproductgrid").refresh();
        Xrm.Utility.closeProgressIndicator();
    }
    else {

        alert("Erro occured, Contact CRM Administrator");
        Xrm.Utility.closeProgressIndicator();

    }
}


function reteriveCurrentBooking(orgUrl, bookingGuid) {
    var formContext = executionContext.getFormContext();

    if (quotestatus == 3) {
        var EntityID = Xrm.Page.data.entity.getId();
        var req = new XMLHttpRequest();
        var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.2/bookableresourcebookings?$select=statuscode&$filter=bookableresourcebookingid(" + bookingGuid + ")";
        req.open("GET", url, false);
        req.setRequestHeader("OData-MaxVersion", "4.0");
        req.setRequestHeader("OData-Version", "4.0");
        req.setRequestHeader("Accept", "application/json");
        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
        req.send();
        if (req.status === 200) {
            var results = JSON.parse(req.response);
            window.open(orgUrl + "/main.aspx?appid=ab2029aa-0f66-ea11-a811-000d3a33f47e&cmdbar=true&forceUCI=1&pagetype=entityrecord&etn=bookableresourcebooking&id=" + bookingGuid)

        } else {
            Xrm.Utility.alertDialog(req.statusText);
        }
    }
}



function markWOSTStatusCompleted_OnClickMarkComplete(primaryControl) {
    debugger;
    var formContext = primaryControl;

    formContext.getAttribute("ap360_workorderservicetaskstatus").setValue(126300010);//Completed
    formContext.getAttribute("msdyn_percentcomplete").setValue(100);
    formContext.data.entity.save();
}


function markWOSTStatusCompletedOnSubgrid_OnClickMarkComplete(SelectedControlSelectedItemIds, primaryControl) {
    debugger;
    // var formContext = primaryControl;
    var selectWOSTIds = SelectedControlSelectedItemIds;

    //Xrm.Page.ui.setFormNotification("WOST Status and Percentage is updating", "INFO", "1");
    // Xrm.Utility.alertDialog("WOST Status and Percentage is updating");

    Xrm.Utility.showProgressIndicator("Please Wait, WOST Status and Percentage are updating");

    for (var i = 0; i < selectWOSTIds.length; i++) {
        updateWOSTStatusAndPercentComplete(selectWOSTIds[i]);
    }
    Xrm.Utility.closeProgressIndicator();
    // Xrm.Page.ui.clearFormNotification("1");
    Xrm.Page.getControl("servicetasks").refresh();

}

function updateWOSTStatusAndPercentComplete(wostId) {
    var entity = {};
    entity.msdyn_percentcomplete = 100;
    entity.ap360_workorderservicetaskstatus = 126300010;

    var req = new XMLHttpRequest();
    req.open("PATCH", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/msdyn_workorderservicetasks( " + wostId + " )", true);
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


function changeStatusToIncomplete_OnChangeOfPercentComplete(executionContext) {
    var formContext = executionContext.getFormContext();
    var percentComplete = formContext.getAttribute("msdyn_percentcomplete").getValue();

    if (percentComplete == 100) {
        formContext.getAttribute("ap360_workorderservicetaskstatus").setValue(126300010);//Complete 
    }


    if (previousStatus == 126300010 && percentComplete < 100)//Complete
    {
        formContext.getAttribute("ap360_workorderservicetaskstatus").setValue(126300009);//Incomplete 
    }
}

function changePercentComplete_OnChangeOfWOSTStatus(executionContext) {
    var formContext = executionContext.getFormContext();
    var wostStatus = formContext.getAttribute("ap360_workorderservicetaskstatus").getValue();

    if (wostStatus == 126300010)//Complete
    {
        formContext.getAttribute("msdyn_percentcomplete").setValue(100);
    }

    if (wostStatus == 126300009)//InComplete
    {
        formContext.getAttribute("msdyn_percentcomplete").setValue(null);
        formContext.getAttribute("msdyn_percentcomplete").setRequiredLevel("required");
    }

}

function CheckFormIsDirty() {
    attributes = Xrm.Page.data.entity.attributes.get();

    if (attributes != null) {
        for (var i in attributes) {
            if (attributes[i].getIsDirty()) {
                // Display the name and value of the attribute that has changed
                alert(attributes[i].getName() + ":" + attributes[i].getValue());
            }
        }
    }
}

function checkIfResourseCanBeBookedOrNotOnBasisOfTimeStamp(primaryControl, firstSelectedItemId, selectedEntityTypeName) {
    debugger;
    var formContext = primaryControl;
    var wostGuid = null; var orgUrl = null;

    if (firstSelectedItemId != null) {
        wostGuid = firstSelectedItemId
    }
    else {
        wostGuid = formContext.data.entity.getId();
    }


    var revisedItemStatus = getRevisedItemStatusOfSelectedItemId(wostGuid);

    if (revisedItemStatus == 126300000)//Approved 
    {
        Xrm.Navigation.openAlertDialog("Revised item status not Approved");
        return;
    } else if (revisedItemStatus == 126300002)//Rejected
    {
        Xrm.Navigation.openAlertDialog("Revised item status rejected");
        return;
    }

    wostGuid = wostGuid.replace('{', '').replace('}', '');

    var timeStamp = getWOSTTimes(wostGuid);

    //if ((timeStamp.estimatedduration == 0 || timeStamp.estimatedduration == null) && timeStamp.ap360_journeymanactualduration > 60) {
    //    Xrm.Navigation.openAlertDialog("Booking cannot be started as 60 Minutes spent based on Journey man");
    //    return;

    //}
    //else 
    if (!isNaN(parseInt(timeStamp.estimatedduration))) {

        if (timeStamp.ap360_journeymanactualduration > timeStamp.estimatedduration > 0) {
            Xrm.Navigation.openAlertDialog("Cannot be Book because 'Journey Men Actual Duration' is Greater then 'Estimated Duration' ");
            return;
        }

    }
    FpsUtils.Form.bookButtonAction(primaryControl, firstSelectedItemId, selectedEntityTypeName);

}


//Xrm.WebApi.online.retrieveRecord("msdyn_workorderservicetask", "CDAE3C77-DB3E-EB11-A813-000D3A33F3C3", "?$select=msdyn_estimatedduration").then(
//    function success(result) {
//        var msdyn_estimatedduration = result["msdyn_estimatedduration"];
//        var msdyn_estimatedduration_formatted = result["msdyn_estimatedduration@OData.Community.Display.V1.FormattedValue"];
//    },
//    function (error) {
//        Xrm.Utility.alertDialog(error.message);
//    }
//);


//EE8099A8-156B-EB11-A812-00224802B3CE
