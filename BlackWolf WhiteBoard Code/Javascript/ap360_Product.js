function SetProductIDQuickCreate_onLoad(executionContext) {
    var formContext = executionContext.getFormContext();
    var formtype = formContext.ui.getFormType();
    //debugger;
    if (formtype == 1) {
        var today = new Date();
        var date = today.getFullYear() + '-' + (today.getMonth() + 1) + '-' + today.getDate();
        var time = today.getMinutes() + "" + today.getSeconds();
        var randomNumber = Math.ceil(Math.random() * 100000);
        formContext.getAttribute("productnumber").setValue("BW-PROD-" + randomNumber + "-" + time);
    }
}
function onLoad(executionContext) {

    var formContext = executionContext.getFormContext();
    FilterGGParent(executionContext);

    var GGParentLookup = Xrm.Page.data.entity.attributes.get("ap360_ggparent").getValue();
    if (GGParentLookup != null)
        FilterGParent(executionContext);
    var GParentLookup = Xrm.Page.data.entity.attributes.get("ap360_gparent").getValue();
    if (GParentLookup != null)
        FilterParent(executionContext);
    var ParentLookup = Xrm.Page.data.entity.attributes.get("ap360_parent").getValue();
    if (ParentLookup != null)
        FilterChild(executionContext);

    var formType = formContext.ui.getFormType();
    if (formType != 1) {
        formContext.getControl("ap360_approveproduct").setVisible(true);
    }
    if (formType == 1) {
        setUnitAndDefaultgroup(formContext);
        var BlackWolfPriceListlookupValue = new Array();
        BlackWolfPriceListlookupValue[0] = new Object();
        BlackWolfPriceListlookupValue[0].id = "4fe16dd5-8e55-ea11-a811-000d3a33f3c3";
        BlackWolfPriceListlookupValue[0].name = "Black Wolf Price List";
        BlackWolfPriceListlookupValue[0].entityType = "pricelevel"
        formContext.getAttribute("pricelevelid").setValue(BlackWolfPriceListlookupValue); //Default Price List

        formContext.getAttribute("quantitydecimal").setValue(1);//Decimal Supported
        var formtype = formContext.ui.getFormType();
        //debugger;
        if (formtype == 1) {
            var today = new Date();
            var date = today.getFullYear() + '-' + (today.getMonth() + 1) + '-' + today.getDate();
            var time = today.getMinutes() + "" + today.getSeconds();
            var randomNumber = Math.ceil(Math.random() * 100000);
            formContext.getAttribute("productnumber").setValue("BW-PROD-" + randomNumber + "-" + time);
        }
    }
}

function setSuppliesGGParentONProductFamilyQuickCreate(executionContext) {
    var formContext = executionContext.getFormContext();


    var ggParentProduct = new Array();
    ggParentProduct[0] = new Object();
    ggParentProduct[0].id = "72a07066-e3ed-e911-a811-000d3a579c6d";
    ggParentProduct[0].name = "Supplies.";
    ggParentProduct[0].entityType = "product";
    formContext.getAttribute("ap360_ggparent").setValue(ggParentProduct);

    FilterGParent(executionContext);
    setUnitAndDefaultgroup(formContext);

    SetProductFamily(executionContext);
    mapCustomParentToOOBParent_OnChangeOfCustomParentField(executionContext);
}

function setUnitAndDefaultgroup(formContext) {
    var UnitGrouplookupValue = new Array();
    UnitGrouplookupValue[0] = new Object();
    UnitGrouplookupValue[0].id = "aa8431af-d3f2-431f-acca-9555ec1005ba";
    UnitGrouplookupValue[0].name = "Each";
    UnitGrouplookupValue[0].entityType = "uomschedule"
    formContext.getAttribute("defaultuomscheduleid").setValue(UnitGrouplookupValue);//Unit Group


    var UnitlookupValue = new Array();
    UnitlookupValue[0] = new Object();
    UnitlookupValue[0].id = "361a3eac-749c-4bb3-92a2-d63f692f61ba";
    UnitlookupValue[0].name = "Each";
    UnitlookupValue[0].entityType = "uom"
    formContext.getAttribute("defaultuomid").setValue(UnitlookupValue);//Default Unit

}
function mapCustomParentToOOBParent_OnChangeOfCustomParentField(executionContext) {
    var formContext = executionContext.getFormContext();
    var ap360_parent = formContext.getAttribute("ap360_productfamily").getValue();
    if (ap360_parent != null) {
        formContext.getAttribute("parentproductid").setValue(ap360_parent);

    }
    else {
        formContext.getAttribute("parentproductid").setValue(null);

    }


}
function PreFilter() {
    debugger;
    var fetchXML = "<filter type='or'><condition attribute='ap360_name' operator='eq' value='Repair' /><condition attribute='ap360_name' operator='eq' value='Restoration' /></filter>";
    Xrm.Page.getControl("ap360_parentservicetemplatetypeid").addCustomFilter(fetchXML);
}
function FilterParentServiceTemplateLookup(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    Xrm.Page.getControl("ap360_parentservicetemplatetypeid").addPreSearch(PreFilter);
}
function FilterVendor(executionContext) {
    var formContext = executionContext.getFormContext();

    formContext.getControl("msdyn_defaultvendor").addPreSearch(addAccountsPreSearchFilter);
    // formContext.getControl("ap360_preferredsupplierid").addPreSearch(addAccountsPreSearchFilter);
}

function addAccountsPreSearchFilter() {
    var filter = "<filter type=\"and\">" +
        "<condition attribute=\"customertypecode\" operator=\"eq\"  value=\"11\" />" +
        "</filter>";
    //"<condition attribute=\"customertypecode\" operator=\"eq\"  value=\"" + productFamilyId + "\" />" +
    Xrm.Page.getControl("msdyn_defaultvendor").addCustomFilter(filter);
    // Xrm.Page.getControl("ap360_preferredsupplierid").addCustomFilter(filter);
}

function SetParentServiceTask(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    //this code is return becuase we are not using Child Servic Template on Form(Which contain PARENT SERVICE TASK), to overcome this problem
    //we are bringing  Parent Service Task through code

    var servicetemplate = formContext.getAttribute("ap360_servicetemplateid").getValue();
    if (servicetemplate != null) {
        var parentservicetask = getParentServiceTaskId(servicetemplate[0].id)
        if (parentservicetask != null) {
            formContext.getAttribute("ap360_parentservicetaskid").setValue(parentservicetask);
        }

    }
    else {

        formContext.getAttribute("ap360_parentservicetaskid").setValue(null);

    }
}

function getParentServiceTaskId(servicetemplate) {
    servicetemplate = servicetemplate.replace(/[{()}]/g, '');
    var _ap360_parentservicetaskid_value = null;
    var req = new XMLHttpRequest();
    var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ap360_servicetemplatetypes?$select=_ap360_parentservicetaskid_value&$filter=_ap360_servicetemplateid_value eq '" + servicetemplate + "'";
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
            var results = JSON.parse(req.response);
            for (var i = 0; i < results.value.length; i++) {
                _ap360_parentservicetaskid_value = results.value[i]["_ap360_parentservicetaskid_value"];
                var _ap360_parentservicetaskid_value_formatted = results.value[i]["_ap360_parentservicetaskid_value@OData.Community.Display.V1.FormattedValue"];
                var _ap360_parentservicetaskid_value_lookuplogicalname = results.value[i]["_ap360_parentservicetaskid_value@Microsoft.Dynamics.CRM.lookuplogicalname"];


                if (_ap360_parentservicetaskid_value != null) {
                    //if any one contain parent then set that parent as Parent Service Task on product.

                    var lookupValue = new Array();
                    lookupValue[0] = new Object();
                    lookupValue[0].id = results.value[i]["_ap360_parentservicetaskid_value"];
                    lookupValue[0].name = results.value[i]["_ap360_parentservicetaskid_value@OData.Community.Display.V1.FormattedValue"];
                    lookupValue[0].entityType = results.value[i]["_ap360_parentservicetaskid_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
                    return lookupValue;


                }
            }
        } else {
            Xrm.Utility.alertDialog(this.statusText);
        }
    }

    return _ap360_parentservicetaskid_value;
}

function FilterGGParent(executionContext) {

    debugger;
    var formContext = executionContext.getFormContext();

    formContext.getControl("ap360_ggparent").addPreSearch(addGGParentPreSearchFilter);
}

function addGGParentPreSearchFilter() {
    debugger;
    var parentServiceOperation = Xrm.Page.data.entity.attributes.get("ap360_parentservicetaskid").getValue();
    debugger;
    if (parentServiceOperation != null) {
        serviceOperationName = parentServiceOperation[0].name;
        serviceOperationName = serviceOperationName.trim();

        var filter = "<filter type=\"and\">" +
            "<condition attribute=\"parentproductidname\" operator=\"eq\"  value=\"" + serviceOperationName + "\" />" +
            "</filter>";

        Xrm.Page.getControl("ap360_ggparent").addCustomFilter(filter);
    }
}

function FilterGParent(executionContext) {

    var formContext = executionContext.getFormContext();
    formContext.getControl("ap360_gparent").addPreSearch(addGParentPreSearchFilter);

}
function addGParentPreSearchFilter() {
    var ggParentId = null;
    var GGParentLookup = Xrm.Page.data.entity.attributes.get("ap360_ggparent").getValue();
    if (GGParentLookup != null)
        ggParentId = GGParentLookup[0].id;
    if (ggParentId != null) {
        var filter = "<filter type=\"and\">" +
            "<condition attribute=\"parentproductid\" operator=\"eq\"  value=\"" + ggParentId + "\" />" +
            "</filter>";
        Xrm.Page.getControl("ap360_gparent").addCustomFilter(filter);
    }
}

function FilterParent(executionContext) {

    var formContext = executionContext.getFormContext();
    formContext.getControl("ap360_parent").addPreSearch(addParentPreSearchFilter);

}
function addParentPreSearchFilter() {
    var gParentId = null;
    var GParentLookup = Xrm.Page.data.entity.attributes.get("ap360_gparent").getValue();
    if (GParentLookup != null)
        gParentId = GParentLookup[0].id;
    if (gParentId != null) {
        var filter = "<filter type=\"and\">" +
            "<condition attribute=\"parentproductid\" operator=\"eq\"  value=\"" + gParentId + "\" />" +
            "</filter>";
        Xrm.Page.getControl("ap360_parent").addCustomFilter(filter);
    }
}

function FilterChild(executionContext) {

    var formContext = executionContext.getFormContext();
    formContext.getControl("ap360_child").addPreSearch(addChildPreSearchFilter);

}
function addChildPreSearchFilter() {
    var ParentId = null;
    var ParentLookup = Xrm.Page.data.entity.attributes.get("ap360_parent").getValue();
    if (ParentLookup != null)
        ParentId = ParentLookup[0].id;
    if (ParentId != null) {
        var filter = "<filter type=\"and\">" +
            "<condition attribute=\"parentproductid\" operator=\"eq\"  value=\"" + ParentId + "\" />" +
            "</filter>";
        Xrm.Page.getControl("ap360_child").addCustomFilter(filter);
    }
}

function SetProductFamily(executionContext) {
    //debugger;

    var formContext = executionContext.getFormContext();

    var GGParentLookup = formContext.getAttribute("ap360_ggparent").getValue();
    var GParentLookup = formContext.getAttribute("ap360_gparent").getValue();
    var ParentLookup = formContext.getAttribute("ap360_parent").getValue();
    var ChildLookup = formContext.getAttribute("ap360_child").getValue();

    var productFamilyControl = formContext.getAttribute("ap360_productfamily");
    productFamilyControl.setValue(null);

    if (GGParentLookup != null) {
        productFamilyControl.setValue(GGParentLookup);
    } else {
        formContext.getAttribute("ap360_gparent").setValue(null);
        formContext.getAttribute("ap360_parent").setValue(null);
        formContext.getAttribute("ap360_child").setValue(null);
    }
    if (GParentLookup != null) {
        productFamilyControl.setValue(GParentLookup);
    } else {
        formContext.getAttribute("ap360_parent").setValue(null);
        formContext.getAttribute("ap360_child").setValue(null);
    }
    if (ParentLookup != null) {
        productFamilyControl.setValue(ParentLookup);
    } else {
        formContext.getAttribute("ap360_child").setValue(null);
    }
    if (ChildLookup != null) {
        productFamilyControl.setValue(ChildLookup);
    }
    PrepareProductName(executionContext);
}

function PrepareProductName(executionContext) {

    //debugger;

    var serviceOperation = "";
    var description = null;
    var productFamily = "";

    var formContext = executionContext.getFormContext();
    var serviceOperationLookup = formContext.getAttribute("ap360_parentservicetaskid").getValue();
    var productFamilyLookup = formContext.getAttribute("ap360_productfamily").getValue();


    if (serviceOperationLookup != null)
        serviceOperation = serviceOperationLookup[0].name;

    if (productFamilyLookup != null)
        productFamily = productFamilyLookup[0].name;


    var descriptionValue = formContext.getAttribute("ap360_productdescription").getValue();//Description

    if (descriptionValue != null)
        description = descriptionValue;
    var name = null;
    if (description == null) {
        name = productFamily;
    } else {
        name = description;
    }

    if (name.length > 100)
        name = name.substring(0, 100);

    formContext.getAttribute("ap360_name").setValue(name);//Name
    formContext.getAttribute("name").setValue(name);//Name


}

function PrepareSoTemplateName(executionContext) {

    //debugger;

    var serviceOperation = "";
    var description = null;
    var productFamily = "";

    var formContext = executionContext.getFormContext();
    var serviceOperationLookup = formContext.getAttribute("ap360_parentservicetaskid").getValue();
    var productFamilyLookup = formContext.getAttribute("ap360_productfamily").getValue();


    if (serviceOperationLookup != null)
        serviceOperation = serviceOperationLookup[0].name;

    if (productFamilyLookup != null)
        productFamily = productFamilyLookup[0].name;


    var descriptionValue = formContext.getAttribute("ap360_productdescription").getValue();//Description

    if (descriptionValue != null)
        description = descriptionValue;
    var name = null;
    if (description == null) {
        name = productFamily;
    } else {
        name = description;
    }

    if (name.length > 100)
        name = name.substring(0, 100);

    formContext.getAttribute("ap360_name").setValue(name);//Name
    formContext.getAttribute("name").setValue(name);//Name

}
