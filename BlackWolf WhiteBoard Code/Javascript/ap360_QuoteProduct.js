function OnLoad(executionContext) {
    debugger;
    makeFormReadOnlyonQSCompleted(executionContext);
    ToggleCreateCoreFieldOnBasisOfProductField(executionContext);
    var formContext = executionContext.getFormContext();
    FilterVendor(formContext);
    FilterQST(formContext);

    formItem = formContext.ui.formSelector.getCurrentItem();
    var formName = formItem.getLabel();

    if (formName == "BW Estimator") {
        ToggleAprovPrdctDescAndBypasDup_OnFormLoad(executionContext);
        if (formContext.ui.getFormType() == 1) {
            SetProductFamily(executionContext);
        }
    }
    FilterProduct(executionContext);
    var IsVendorIdentified = formContext.getAttribute("ap360_vendoridentified").getValue();
    if (IsVendorIdentified) {
        formContext.getControl("ap360_vendorid").setVisible(true);
    }
    if (formContext.getAttribute("ap360_productfamily")) {
        var ap360_productfamily = formContext.getAttribute("ap360_productfamily").getValue();
        var formtype = formContext.ui.getFormType();
        if (formtype == 1) {
            if (ap360_productfamily != null) {
                ap360_productfamily = ap360_productfamily[0].name;
                formContext.getAttribute("ap360_description").setValue(ap360_productfamily);
                formContext.getAttribute("ap360_name").setValue(ap360_productfamily);
            } else {
                formContext.getAttribute("ap360_description").setValue(null);
                formContext.getAttribute("ap360_name").setValue(null);
            }
        }
    }

}


function FilterQST(formContext) {
    formContext.getControl("ap360_quoteservicetaskid").addPreSearch(addQuoteServicesQSTFilter);

}
function addQuoteServicesQSTFilter() {
    var quoteService = Xrm.Page.getAttribute("ap360_quoteserviceid").getValue();
    if (quoteService == null) return;
    quoteService = quoteService[0].id.replace('{', '').replace('}', '');

    var filter = "<filter type=\"and\">" +
        "<condition attribute=\"ap360_quoteserviceid\" operator=\"eq\"  value=\"" + quoteService + "\" />" +
        "</filter>";
    //"<condition attribute=\"customertypecode\" operator=\"eq\"  value=\"" + productFamilyId + "\" />" +
    Xrm.Page.getControl("ap360_quoteservicetaskid").addCustomFilter(filter);

}

function FilterVendor(formContext) {
    formContext.getControl("ap360_quotevendorid").addPreSearch(addAccountsPreSearchFilter);

    formContext.getControl("ap360_vendorid").addPreSearch(addAccountsPreSearchFilter);
    formContext.getControl("ap360_preferredsupplierid").addPreSearch(addAccountsPreSearchFilter);
}
function addAccountsPreSearchFilter() {
    var filter = "<filter type=\"and\">" +
        "<condition attribute=\"customertypecode\" operator=\"eq\"  value=\"11\" />" +
        "</filter>";
    //"<condition attribute=\"customertypecode\" operator=\"eq\"  value=\"" + productFamilyId + "\" />" +
    Xrm.Page.getControl("ap360_vendorid").addCustomFilter(filter);
    Xrm.Page.getControl("ap360_quotevendorid").addCustomFilter(filter);

    Xrm.Page.getControl("ap360_preferredsupplierid").addCustomFilter(filter);
}
function setUOMtoEach(executionContext) {
    var lookupData = new Array();
    var lookupItem = new Object();
    lookupItem.id = "361a3eac-749c-4bb3-92a2-d63f692f61ba"; // UOM Each Guid
    lookupItem.name = "Each"; // Entity record name
    lookupItem.entityType = "uom";
    lookupData[0] = lookupItem;
    Xrm.Page.getAttribute("ap360_uomid").setValue(lookupData);
}

function roundUp(num, precision) {
    precision = Math.pow(10, precision)
    return Math.ceil(num * precision) / precision
}
roundUp(192.168, 1) //=> 192.2
function getPriceMarkUpMatrix(unitcost) {


    debugger;
    var multiplier = null;
    var purchaseOrderId = null;
    var isPurhcaseOrderExists = false;

    var req = new XMLHttpRequest();
    var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ap360_pricemarkups?$select=ap360_from,ap360_maxprice,ap360_multiplier,ap360_searchable,ap360_to";
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
            var ap360_from = results.value[i]["ap360_from"];
            var ap360_from_formatted = results.value[i]["ap360_from@OData.Community.Display.V1.FormattedValue"];
            var ap360_maxprice = results.value[i]["ap360_maxprice"];
            var ap360_maxprice_formatted = results.value[i]["ap360_maxprice@OData.Community.Display.V1.FormattedValue"];
            var ap360_multiplier = results.value[i]["ap360_multiplier"];
            var ap360_multiplier_formatted = results.value[i]["ap360_multiplier@OData.Community.Display.V1.FormattedValue"];
            var ap360_searchable = results.value[i]["ap360_searchable"];
            var ap360_searchable_formatted = results.value[i]["ap360_searchable@OData.Community.Display.V1.FormattedValue"];
            var ap360_to = results.value[i]["ap360_to"];
            var ap360_to_formatted = results.value[i]["ap360_to@OData.Community.Display.V1.FormattedValue"];


            if ((unitcost >= ap360_from) && unitcost <= ap360_to) {
                multiplier = ap360_multiplier;
                return multiplier;
            }
        }


    } else {
        Xrm.Utility.alertDialog(req.statusText);
    }

    return multiplier;
}
function checkIsvendorIndetified(executionContext) {
    var formContext = executionContext.getFormContext(); // get formContext

    var IsVendorIdentified = false;
    if (formContext.getAttribute("ap360_vendoridentified") != null) {
        IsVendorIdentified = formContext.getAttribute("ap360_vendoridentified").getValue();
        if (IsVendorIdentified) {
            formContext.getControl("ap360_vendorid").setVisible(true);
            // formContext.getControl("ap360_approveproduct").setVisible(true);
        } else {
            formContext.getControl("ap360_vendorid").setVisible(false);
            // formContext.getControl("ap360_approveproduct").setVisible(false);
        }

    }
}

function ToggleAprovPrdctDescAndBypasDup_OnChngOfPrdctPrtNum(executionContext) {
    var formContext = executionContext.getFormContext(); // get formContext
    var IsProductSelelcted = null;
    if (formContext.getAttribute("ap360_product") != null) {
        IsProductSelelcted = formContext.getAttribute("ap360_product").getValue();
        if (IsProductSelelcted != null) {
            formContext.getControl("ap360_approveproduct").setVisible(false);
            formContext.getAttribute("ap360_approveproduct").setValue(false);
            formContext.getControl("ap360_bypassduplicatedetection").setVisible(false);
            formContext.getAttribute("ap360_bypassduplicatedetection").setValue(false);
            formContext.getControl("ap360_productdescription").setVisible(false);
            formContext.getAttribute("ap360_productdescription").setValue(null);
        }
        else {
            formContext.getControl("ap360_approveproduct").setVisible(true);
            formContext.getControl("ap360_bypassduplicatedetection").setVisible(true);
            formContext.getControl("ap360_productdescription").setVisible(true);
            formContext.getAttribute("ap360_approveproduct").setValue(true);
        }

    }
}
function ToggleAprovPrdctDescAndBypasDup_OnFormLoad(executionContext) {
    var formContext = executionContext.getFormContext(); // get formContext
    var IsProductSelelcted = null;
    if (formContext.getAttribute("ap360_product") != null) {
        IsProductSelelcted = formContext.getAttribute("ap360_product").getValue();
        if (IsProductSelelcted != null) {
            formContext.getControl("ap360_approveproduct").setVisible(false);
            formContext.getAttribute("ap360_approveproduct").setValue(false);
            formContext.getControl("ap360_bypassduplicatedetection").setVisible(false);
            formContext.getAttribute("ap360_bypassduplicatedetection").setValue(false);
            formContext.getControl("ap360_productdescription").setVisible(false);
            formContext.getAttribute("ap360_productdescription").setValue(null);
        }
        else {
            formContext.getControl("ap360_approveproduct").setVisible(true);
            formContext.getControl("ap360_bypassduplicatedetection").setVisible(true);
            formContext.getControl("ap360_productdescription").setVisible(true);
        }

    }
}
////////////////////////////////////////////////////////////////////////////////////////////

////ap360_parentservicetaskid

function FilterProductFamily(formContext) {

    formContext.getControl("ap360_productfamily").addPreSearch(addProductFamilyPreSearchFilter);

}

function addProductFamilyPreSearchFilter() {
    // debugger;

    var productFamilyId = null;

    var GGParentLookup = Xrm.Page.data.entity.attributes.get("ap360_ggparent").getValue();
    var GParentLookup = Xrm.Page.data.entity.attributes.get("ap360_gparent").getValue();
    var ParentLookup = Xrm.Page.data.entity.attributes.get("ap360_parent").getValue();

    if (GGParentLookup != null)
        productFamilyId = GGParentLookup[0].id;
    if (GParentLookup != null)
        productFamilyId = GParentLookup[0].id;
    if (ParentLookup != null)
        productFamilyId = ParentLookup[0].id;

    var filter = "<filter type=\"and\">" +
        "<condition attribute=\"parentproductid\" operator=\"eq\"  value=\"" + productFamilyId + "\" />" +
        "</filter>";

    Xrm.Page.getControl("ap360_productfamily").addCustomFilter(filter);
}

function FilterProductGGParent(formContext) {

    //debugger;
    formContext.getControl("ap360_ggparent").addPreSearch(addParentProductPreSearchFilter);
}

function addParentProductPreSearchFilter() {
    //  debugger;
    var parentServiceOperation = Xrm.Page.data.entity.attributes.get("ap360_parentservicetaskid").getValue();
    // debugger;
    if (parentServiceOperation != null) {
        serviceOperationName = parentServiceOperation[0].name;

        serviceOperationName = serviceOperationName.trim();

        //serviceOperationName = serviceOperationName + "%";

        var filter = "<filter type=\"and\">" +
            "<condition attribute=\"parentproductidname\" operator=\"eq\"  value=\"" + serviceOperationName + "\" />" +
            "</filter>";

        Xrm.Page.getControl("ap360_ggparent").addCustomFilter(filter);
    }
}

function SetProductFamily(executionContext) {
    // debugger;

    var formContext = executionContext.getFormContext();

    var GGParentLookup = formContext.getAttribute("ap360_ggparent").getValue();
    var GParentLookup = formContext.getAttribute("ap360_gparent").getValue();
    var ParentLookup = formContext.getAttribute("ap360_parent").getValue();
    var ChildLookup = formContext.getAttribute("ap360_child").getValue();

    var productFamilyControl = formContext.getAttribute("ap360_productfamily");
    //debugger;
    if (GGParentLookup != null) {
        //debugger;
        productFamilyControl.setValue(GGParentLookup);
    }
    if (GParentLookup != null) {
        //debugger;
        productFamilyControl.setValue(GParentLookup);
    }
    if (ParentLookup != null) {
        //debugger;
        productFamilyControl.setValue(ParentLookup);
    }
    if (ChildLookup != null) {
        //debugger;
        productFamilyControl.setValue(ChildLookup);
    }
    //debugger;
    PrepareSoTemplateName(executionContext);
}

function GGParentOnChange_SetProductFamily(executionContext) {
    // debugger;

    var formContext = executionContext.getFormContext();
    var GGParentLookup = formContext.getAttribute("ap360_ggparent").getValue();

    var productFamilyControl = formContext.getAttribute("ap360_productfamily");
    //debugger;
    if (GGParentLookup != null) {
        //debugger;
        productFamilyControl.setValue(GGParentLookup);
    } else {
        formContext.getAttribute("ap360_gparent").setValue(null);
        formContext.getAttribute("ap360_parent").setValue(null);
        formContext.getAttribute("ap360_child").setValue(null);
    }
    PrepareSoTemplateName(executionContext);
}
function GParentOnChange_SetProductFamily(executionContext) {
    // debugger;

    var formContext = executionContext.getFormContext();

    var GParentLookup = formContext.getAttribute("ap360_gparent").getValue();

    var productFamilyControl = formContext.getAttribute("ap360_productfamily");

    if (GParentLookup != null) {
        //debugger;
        productFamilyControl.setValue(GParentLookup);
    } else {
        formContext.getAttribute("ap360_parent").setValue(null);
        formContext.getAttribute("ap360_child").setValue(null);
    }
    PrepareSoTemplateName(executionContext);
}
function ParentOnChange_SetProductFamily(executionContext) {
    // debugger;

    var formContext = executionContext.getFormContext();


    var ParentLookup = formContext.getAttribute("ap360_parent").getValue();

    var productFamilyControl = formContext.getAttribute("ap360_productfamily");

    if (ParentLookup != null) {
        //debugger;
        productFamilyControl.setValue(ParentLookup);
    } else {
        formContext.getAttribute("ap360_child").setValue(null);
    }

    PrepareSoTemplateName(executionContext);
}
function ChildOnChange_SetProductFamily(executionContext) {
    // debugger;

    var formContext = executionContext.getFormContext();


    var ChildLookup = formContext.getAttribute("ap360_child").getValue();

    var productFamilyControl = formContext.getAttribute("ap360_productfamily");
    if (ChildLookup != null) {
        //debugger;
        productFamilyControl.setValue(ChildLookup);
    }
    //debugger;
    PrepareSoTemplateName(executionContext);
}

function PrepareSoTemplateName(executionContext) {

    //debugger;
    var format = /[`!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?~]/;
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

    //if (descriptionValue != null) {

    //    var isCorrectformat = format.test(descriptionValue);
    //    if (!isCorrectformat) {
    //        formContext.getControl("ap360_productdescription").clearNotification();
    //description = descriptionValue;
    //    }
    //    else {
    //        formContext.getControl("ap360_productdescription").setNotification('Special Characters are not allowed for Product description');
    //        return;
    //    }
    //}

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
    formContext.getAttribute("ap360_description").setValue(name);//Name
}

function filterProductPreSearchFilter(executionContext) {

    var formContext = executionContext.getFormContext();
    var ggparent = formContext.getAttribute("ap360_ggparent").getValue();
    var ggparent = formContext.getAttribute("ap360_gparent").getValue();
    var ggparent = formContext.getAttribute("ap360_parent").getValue();
    var ggparent = formContext.getAttribute("ap360_child").getValue();

    if (ggparent != null) { }

}

function FilterProduct(executionContext) {
    //debugger;
    var formContext = executionContext.getFormContext();

    formContext.getControl("ap360_product").addPreSearch(addProductPreSearchFilter);

}
function addProductPreSearchFilter() {
    //debugger;

    var productFamilyId = null;

    var ProductFamilyLookup = Xrm.Page.data.entity.attributes.get("ap360_productfamily").getValue();
    if (ProductFamilyLookup != null) {
        productFamilyId = ProductFamilyLookup[0].id;
        filter(productFamilyId);
        return;
    }
}

function filter(productFamilyId) {

    //debugger;
    var filter = "<filter type=\"and\">" +
        "<condition attribute=\"productid\" operator=\"under\"  value=\"" + productFamilyId + "\" />" +
        "<condition attribute=\"productstructure\" operator=\"eq\" value=\"1\" />" +
        "</filter>";
    Xrm.Page.getControl("ap360_product").addCustomFilter(filter);
}

function IsProductApproved(executionContext) {
    var formContext = executionContext.getFormContext();
    var isProductApproved = false;
    isProductApproved = formContext.getAttribute("ap360_approveproduct").getValue();
    if (isProductApproved) {
        //Xrm.Page.getControl("ap360_partnumber").setVisible(true);
        //Xrm.Page.getControl("ap360_productdescription").setVisible(true);
        var productdescription = formContext.getAttribute("ap360_productdescription").getValue();
        if (productdescription == null || productdescription == "") {
            var name = formContext.getAttribute("ap360_name").getValue();
            formContext.getAttribute("ap360_productdescription").setValue(name);
        }
        formContext.getAttribute("ap360_partnumber").setRequiredLevel("required");
    }
    else {
        formContext.getAttribute("ap360_partnumber").setRequiredLevel("none");
        formContext.getAttribute("ap360_productdescription").setValue(null);
    }
}

function setQuoteProductName(executionContext) {
    var formContext = executionContext.getFormContext();
    var ap360Name = formContext.getAttribute("ap360_name").getValue();
    if (ap360Name != null) {
        formContext.getAttribute("ap360_description").setValue(ap360Name);
    }
}

function SelectVendor_RibbonButton(formContext, item) {
    debugger;
    var entityName = formContext.data.entity.getEntityName();
    var selectedItemId = item[0];
    if (selectedItemId != null) {
        var req = new XMLHttpRequest();
        req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ap360_vendorquotes(" + selectedItemId + ")?$select=ap360_cost,_ap360_vendor_value", false);
        req.setRequestHeader("OData-MaxVersion", "4.0");
        req.setRequestHeader("OData-Version", "4.0");
        req.setRequestHeader("Accept", "application/json");
        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
        req.onreadystatechange = function () {
            if (this.readyState === 4) {
                req.onreadystatechange = null;
                if (this.status === 200) {
                    var result = JSON.parse(this.response);
                    var ap360_cost = result["ap360_cost"];
                    var ap360_cost_formatted = result["ap360_cost@OData.Community.Display.V1.FormattedValue"];

                    var _ap360_vendor_value = result["_ap360_vendor_value"];
                    var _ap360_vendor_value_formatted = result["_ap360_vendor_value@OData.Community.Display.V1.FormattedValue"];
                    var _ap360_vendor_value_lookuplogicalname = result["_ap360_vendor_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
                    debugger;
                    // Set vendor lookup on Quote product.
                    var lookupValue = new Array();
                    lookupValue[0] = new Object();
                    lookupValue[0].id = _ap360_vendor_value; // GUID of the lookup id
                    lookupValue[0].name = _ap360_vendor_value_formatted; // Name of the lookup
                    lookupValue[0].entityType = _ap360_vendor_value_lookuplogicalname; //Entity Type of the lookup entity
                    formContext.getAttribute("ap360_vendorid").setValue(lookupValue);

                    // Set cost
                    formContext.getAttribute("ap360_unitcost").setValue(ap360_cost);

                    formContext.data.entity.save();

                } else {
                    Xrm.Utility.alertDialog(this.statusText);
                }
            }
        };
        req.send();
    }
}

function RemoveProductFamily_OnChangeOfRemoveProductFamilyHierarchy(executionContext) {
    var formContext = executionContext.getFormContext();
    var removeproductfamilyhierarchy = formContext.getAttribute("ap360_removeproductfamilyhierarchy").getValue();
    if (removeproductfamilyhierarchy == 1) {
        var lookupData = new Array();
        var lookupItem = new Object();
        lookupItem.id = "62a07066-e3ed-e911-a811-000d3a579c6d"; // Vehicle Product
        lookupItem.name = "Vehicle"; // Entity record name
        lookupItem.entityType = "product";
        lookupData[0] = lookupItem;
        formContext.getAttribute("ap360_serviceproductmappingid").setValue(lookupData);
        formContext.getAttribute("ap360_productfamily").setValue(lookupData);

        filter(lookupItem.id);

        var productdescription = formContext.getAttribute("ap360_productdescription").getValue();


        if (productdescription != null) {
            formContext.getAttribute("ap360_name").setValue(productdescription);
            formContext.getAttribute("ap360_description").setValue(productdescription);
        }
        else {
            formContext.getAttribute("ap360_name").setValue("Vehicle");
            formContext.getAttribute("ap360_description").setValue("Vehicle");

        }
        formContext.getAttribute("ap360_parentservicetaskid").setValue(null);
        //formContext.getAttribute("ap360_productfamily").setValue(null);
        formContext.getControl("ap360_ggparent").setVisible(true);
        formContext.getAttribute("ap360_ggparent").setValue(null);
        formContext.getAttribute("ap360_gparent").setValue(null);
        formContext.getAttribute("ap360_parent").setValue(null);
        formContext.getAttribute("ap360_child").setValue(null);
        formContext.getControl("ap360_removeproductfamilyhierarchy").setDisabled(true);
    }
}

function SetProduct_onchangePartNumber(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var ap360_partnumber = formContext.getAttribute("ap360_partnumber").getValue();
    //var tyreBatteryMarkup = formContext.getAttribute("ap360_tyrebatterymarkup").getValue();
    // debugger;
    if (ap360_partnumber != null) {
        var req = new XMLHttpRequest();
        req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/products?$select=currentcost,name&$filter=statecode eq 0 and ap360_partnumber eq '" + ap360_partnumber + "'", false);////Status (statecode ) eq Active(0) not status Reason
        req.setRequestHeader("OData-MaxVersion", "4.0");
        req.setRequestHeader("OData-Version", "4.0");
        req.setRequestHeader("Accept", "application/json");
        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
        req.onreadystatechange = function () {
            if (this.readyState === 4) {
                req.onreadystatechange = null;
                if (this.status === 200) {
                    var results = JSON.parse(this.response);
                    if (results.value.length > 0) {
                        for (var i = 0; i < results.value.length; i++) {
                            var currentcost = results.value[i]["currentcost"];
                            var currentcost_formatted = results.value[i]["currentcostt@OData.Community.Display.V1.FormattedValue"];
                            var name = results.value[i]["name"];
                            var productid = results.value[i]["productid"];
                            //debugger;
                            var lookupValue = new Array();
                            lookupValue[0] = new Object();
                            lookupValue[0].id = productid; // GUID of the lookup id
                            lookupValue[0].name = name; // Name of the lookup
                            lookupValue[0].entityType = "product"; //Entity Type of the lookup entity



                            if (results.value.length == 1) {
                                formContext.getAttribute("ap360_product").setValue(lookupValue);
                            }
                            else {
                                Xrm.Utility.alertDialog("Multiple products exist with Part Number " + ap360_partnumber + ". Try use Product field");
                                return;
                            }
                            if (currentcost != null) {
                                formContext.getAttribute("ap360_unitcost").setValue(currentcost);
                                UpdateSalePriceAndMultiplier(executionContext);
                                //if (tyreBatteryMarkup == false) {
                                //    CalculatePriceMarkup(executionContext);
                                //}
                                //else {
                                //    setTiersBatteryMarkUp(executionContext);
                                //}
                                //updateExtendedPartPrice(executionContext);
                            }
                        }
                    }
                } else {
                    Xrm.Utility.alertDialog(this.statusText);
                }
            }
        };
        req.send();
    } else {
        formContext.getAttribute("ap360_product").setValue(null);
        formContext.getAttribute("ap360_unitcost").setValue(null);
        UpdateSalePriceAndMultiplier(executionContext);
    }
}

function SetPartNumber_onchangeProduct(executionContext) {
    var formContext = executionContext.getFormContext();
    var ap360_product = formContext.getAttribute("ap360_product").getValue();
    debugger;
    if (ap360_product != null) {
        var ap360_name = ap360_product[0].name;
        ap360_product = ap360_product[0].id.replace('{', '').replace('}', '');
        var req = new XMLHttpRequest();
        req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/products(" + ap360_product + ")?$select=ap360_partnumber,currentcost&$filter=statecode eq 0", false);////Status (statecode ) eq Active(0) not status Reason
        req.setRequestHeader("OData-MaxVersion", "4.0");
        req.setRequestHeader("OData-Version", "4.0");
        req.setRequestHeader("Accept", "application/json");
        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
        req.onreadystatechange = function () {
            if (this.readyState === 4) {
                req.onreadystatechange = null;
                if (this.status === 200) {
                    var result = JSON.parse(this.response);
                    debugger;
                    formContext.getAttribute("ap360_description").setValue(ap360_name);
                    formContext.getAttribute("ap360_name").setValue(ap360_name);
                    var ap360_partnumber = result["ap360_partnumber"];
                    var currentcost = result["currentcost"];
                    var currentcost_formatted = result["currentcost@OData.Community.Display.V1.FormattedValue"];
                    if (ap360_partnumber != null) {
                        formContext.getAttribute("ap360_partnumber").setValue(ap360_partnumber);
                        //formContext.data.entity.save();
                    } else {
                        var alertStrings = { confirmButtonLabel: "Close", text: "Product does not have Part Number.", title: "Product" };
                        var alertOptions = { height: 120, width: 260 };
                        Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                            function success(result) {
                                console.log("Alert dialog closed");
                            },
                            function (error) {
                                console.log(error.message);
                            }
                        );
                    }
                    if (currentcost != null) {
                        formContext.getAttribute("ap360_unitcost").setValue(currentcost);
                        UpdateSalePriceAndMultiplier(executionContext);
                    }
                } else {
                    Xrm.Utility.alertDialog(this.statusText);
                }
            }
        };
        req.send();
    } else {
        formContext.getAttribute("ap360_partnumber").setValue(null);
        formContext.getAttribute("ap360_unitcost").setValue(null);
        UpdateSalePriceAndMultiplier(executionContext);
    }
}


var quoteid = null;
function filterQuoteService(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext(); // get formContext
    var ap360_quoteserviceid = formContext.getAttribute("ap360_quoteserviceid").getValue();

    // var recordGuid = formContext.data.entity.getId();
    if (ap360_quoteserviceid == null) return;
    ap360_quoteserviceid = ap360_quoteserviceid[0].id.replace('{', '').replace('}', '');

    var req = new XMLHttpRequest();
    var url = formContext.context.getClientUrl() + "/api/data/v9.1/ap360_quoteservices(" + ap360_quoteserviceid + ")?$select=_ap360_quoteid_value,ap360_quoteserviceid";
    req.open("GET", url, false);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
    req.send();
    if (req.status === 200) {
        var result = JSON.parse(req.response);
        quoteid = result["_ap360_quoteid_value"];
        var _ap360_quoteid_value_formatted = result["_ap360_quoteid_value@OData.Community.Display.V1.FormattedValue"];
        var _ap360_quoteid_value_lookuplogicalname = result["_ap360_quoteid_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
        var ap360_quoteserviceid = result["ap360_quoteserviceid"];

        FilterChildServieTempalteBasedOnServiceRole(formContext, quoteid);
    } else {
        Xrm.Utility.alertDialog(this.statusText);
    }
}

function FilterChildServieTempalteBasedOnServiceRole(formContext, quoteid) {

    debugger;
    formContext.getControl("ap360_quoteserviceid").addPreSearch(addquotePreSearchFilter);
}
function setProductInDescription_OnChangeProduct(executionContext) {
    var formContext = executionContext.getFormContext();
    var product = formContext.getAttribute("ap360_product").getValue();
    if (product != null) {
        formContext.getAttribute("ap360_productdescription").setValue(product[0].name);
    }
    else {
        formContext.getAttribute("ap360_productdescription").setValue(null);
    }
}
function addquotePreSearchFilter() {
    // debugger;
    debugger;

    var filter = "<filter type=\"and\">" +
        "<condition attribute=\"ap360_quoteid\" operator=\"eq\"  value=\"" + quoteid + "\" />" +
        "</filter>";

    Xrm.Page.getControl("ap360_quoteserviceid").addCustomFilter(filter);

}

function UpdateProductFamilyForCustomerSupplied_OnChangeOfQuoteProductType(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var quoteproducttype = formContext.getAttribute("ap360_quoteproducttype").getValue();

    //if (isCustomerPart == true) {
    //Customer Supplied  126,300,000
    //BlackWolf Inventory  126,300,001
    //Vendor Supplied  126,300,002
    if (quoteproducttype == 126300000)//  Customer Supplied
    {
        formContext.getAttribute("ap360_vendoridentified").setValue(true);
        formContext.getControl("ap360_vendorid").setVisible(true);
        var lookupData = new Array();
        var lookupItem = new Object();
        lookupItem.id = "62a07066-e3ed-e911-a811-000d3a579c6d"; // Vehicle Product
        lookupItem.name = "Vehicle"; // Entity record name
        lookupItem.entityType = "product";
        lookupData[0] = lookupItem;
        formContext.getAttribute("ap360_productfamily").setValue(lookupData);
        //formContext.getAttribute("ap360_approveproduct").setValue(true);
        var lookupData = new Array();
        var lookupItem = new Object();
        lookupItem.id = "28b07b2b-4928-eb11-a813-000d3a368915";
        lookupItem.name = "Customer Supplied"; // Entity record name
        lookupItem.entityType = "account";
        lookupData[0] = lookupItem;
        formContext.getAttribute("ap360_vendorid").setValue(lookupData);
        formContext.getAttribute("ap360_vendoridentified").setValue(true);


        //formContext.getAttribute("ap360_multiplier").setValue(0);
        formContext.getAttribute("ap360_partsaleprice").setValue(0);
        formContext.getAttribute("ap360_unitprice").setValue(0);
        UpdateSalePriceAndMultiplier(executionContext);
    }
}
function setBlackWolfVendor_OnChangeOfWorkOrderProductType(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var quoteProductType = formContext.getAttribute("ap360_quoteproducttype").getValue();
    //Customer Supplied  126,300,000
    //BlackWolf Inventory  126,300,001
    //Vendor Supplied  126,300,002
    if (quoteProductType == 126300001)//  BlackWolf Inventory
    {
        formContext.getAttribute("ap360_vendoridentified").setValue(true);
        formContext.getControl("ap360_vendorid").setVisible(true);
        var lookupData = new Array();
        var lookupItem = new Object();
        lookupItem.id = "a89044d7-9b9a-ea11-a811-000d3a33f3c3"; // Blackwolf Part Id
        lookupItem.name = "Black Wolf Parts Account"; // Blackwolf Part
        lookupItem.entityType = "account";
        lookupData[0] = lookupItem;
        formContext.getAttribute("ap360_vendorid").setValue(lookupData);

        UpdateSalePriceAndMultiplier(executionContext);
        SetProductFamily(executionContext);
    }
}
function setFieldsForVendorSupplied_OnChangeOfWorkOrderProductType(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var quoteProductType = formContext.getAttribute("ap360_quoteproducttype").getValue();

    //Customer Supplied  126,300,000
    //BlackWolf Inventory  126,300,001
    //Vendor Supplied  126,300,002
    if (quoteProductType == 126300002)//  Vendor Supplied
    {
        SetProductFamily(executionContext);
        formContext.getAttribute("ap360_vendoridentified").setValue(null);
        formContext.getAttribute("ap360_vendorid").setValue(null);
        formContext.getControl("ap360_vendorid").setVisible(false);
        UpdateSalePriceAndMultiplier(executionContext);
    }
}

function makeFormReadOnlyonQSCompleted(executionContext) {
    var formContext = executionContext.getFormContext();
    var quoteservice = formContext.getAttribute("ap360_quoteserviceid").getValue();
    if (quoteservice != null) {
        quoteservice = quoteservice[0].id.replace('{', '').replace('}', '');

        var req = new XMLHttpRequest();
        var url = Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ap360_quoteservices(" + quoteservice + ")?$select=statuscode";
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
                var quoteservicestatus = result["statuscode"];
                //var msdyn_systemstatus_formatted = result["msdyn_systemstatus@OData.Community.Display.V1.FormattedValue"];

                if (quoteservicestatus == 126300000)//completed
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

function checkProductFamily_OnSave(executionContext) {
    var formContext = executionContext.getFormContext();
    var ap360_productfamily = formContext.getAttribute("ap360_productfamily").getValue();
    if (ap360_productfamily == null) {
        Xrm.Utility.alertDialog("Product Family in not selected");
        var e = executionContext.getEventArgs();
        e.preventDefault();
    }
}

function SaveAndNewForm(primaryControl) {
    //alert("Working");
    var formContext = primaryControl;
    formContext.data.entity.save();

    var formParameters = {};
    var entityOptions = {};
    entityOptions["entityName"] = "ap360_quoteproduct";

    var removeproductfamilyhierarchy = formContext.getAttribute("ap360_removeproductfamilyhierarchy").getValue();
    formParameters["ap360_removeproductfamilyhierarchy"] = removeproductfamilyhierarchy;
    var parentservicetask = formContext.getAttribute("ap360_parentservicetaskid").getValue();
    if (parentservicetask != null) {
        formParameters["ap360_parentservicetaskid"] = parentservicetask;
    }
    var serviceproductmapping = formContext.getAttribute("ap360_serviceproductmappingid").getValue();
    if (serviceproductmapping != null) {
        formParameters["serviceproductmapping"] = serviceproductmapping;
    }
    var productfamily = formContext.getAttribute("ap360_productfamily").getValue();
    if (productfamily != null) {
        formParameters["ap360_productfamily"] = productfamily;
    }
    var ggparent = formContext.getAttribute("ap360_ggparent").getValue();
    if (ggparent != null) {
        formParameters["ap360_ggparent"] = ggparent;
    }
    var warehouse = formContext.getAttribute("ap360_warehouseid").getValue();
    if (warehouse != null) {
        formParameters["ap360_warehouseid"] = warehouse;
    }
    var uom = formContext.getAttribute("ap360_uomid").getValue();
    if (uom != null) {
        formParameters["ap360_uomid"] = uom;
    }
    var quoteservice = formContext.getAttribute("ap360_quoteserviceid").getValue();
    if (quoteservice != null) {
        formParameters["ap360_quoteserviceid"] = quoteservice;


        Xrm.Navigation.openForm(entityOptions, formParameters).then(
            function (lookup) { console.log("Success"); },
            function (error) { console.log("Error"); });
    }

    else {


        Xrm.Utility.alertDialog("Can not use 'Save & New' because Quote Service is not selected");
    }




}
////////////////////////////////////////////////

function UpdateProductFamily_OnChangeOfWorkOrderProductType(executionContext) {

    var formContext = executionContext.getFormContext();
    var quoteproducttype = formContext.getAttribute("ap360_quoteproducttype").getValue();

    //if (isCustomerPart == true) {
    //Customer Supplied  126,300,000
    //BlackWolf Inventory  126,300,001
    //Vendor Supplied  126,300,002
    if (quoteproducttype == 126300000)//  Customer Supplied
    {
        var lookupData = new Array();
        var lookupItem = new Object();
        lookupItem.id = "62a07066-e3ed-e911-a811-000d3a579c6d"; // Vehicle Product
        lookupItem.name = "Vehicle"; // Entity record name
        lookupItem.entityType = "product";
        lookupData[0] = lookupItem;
        //formContext.getAttribute("ap360_serviceproductmappingid").setValue(lookupData);
        formContext.getAttribute("ap360_productfamily").setValue(lookupData);
        var lookupData = new Array();
        var lookupItem = new Object();
        lookupItem.id = "28b07b2b-4928-eb11-a813-000d3a368915";
        lookupItem.name = "Customer Supplied"; // Entity record name
        lookupItem.entityType = "account";
        lookupData[0] = lookupItem;
        formContext.getAttribute("ap360_vendorid").setValue(lookupData);
        formContext.getAttribute("ap360_vendoridentified").setValue(true);
        formContext.getAttribute("ap360_partsaleprice").setValue(0);
        formContext.getAttribute("ap360_unitprice").setValue(0);
        formContext.getControl("ap360_customersupplied").setDisabled(true);
        lockUnLockProductFamilyField(formContext, true);
        UpdateSalePriceAndMultiplier(executionContext);
    }
}
function setBlackWolfVendor_OnChangeOfWorkOrderProductType(executionContext) {
    var formContext = executionContext.getFormContext();
    var quoteproducttype = formContext.getAttribute("ap360_quoteproducttype").getValue();

    //Customer Supplied  126,300,000
    //BlackWolf Inventory  126,300,001
    //Vendor Supplied  126,300,002
    if (quoteproducttype == 126300001)//  BlackWolf Inventory
    {
        formContext.getAttribute("ap360_vendoridentified").setValue(true);
        formContext.getControl("ap360_vendorid").setVisible(true);
        var lookupData = new Array();
        var lookupItem = new Object();
        lookupItem.id = "a89044d7-9b9a-ea11-a811-000d3a33f3c3"; // Blackwolf Part Id
        lookupItem.name = "Black Wolf Parts Account"; // Blackwolf Part
        lookupItem.entityType = "account";
        lookupData[0] = lookupItem;
        formContext.getAttribute("ap360_vendorid").setValue(lookupData);

        lockUnLockProductFamilyField(formContext, false);
        UpdateSalePriceAndMultiplier(executionContext);

        SetProductFamily(executionContext);
    }
}
function setFieldsForVendorSupplied_OnChangeOfWorkOrderProductType(executionContext) {
    var formContext = executionContext.getFormContext();
    var quoteproducttype = formContext.getAttribute("ap360_quoteproducttype").getValue();

    //Customer Supplied  126,300,000
    //BlackWolf Inventory  126,300,001
    //Vendor Supplied  126,300,002
    if (quoteproducttype == 126300002)//  Vendor Supplied
    {
        UpdateSalePriceAndMultiplier(executionContext);
        SetProductFamily(executionContext);
        formContext.getAttribute("ap360_vendoridentified").setValue(false);
        formContext.getAttribute("ap360_vendorid").setValue(null);
        lockUnLockProductFamilyField(formContext, false);
    }
}

function lockUnLockProductFamilyField(formContext, bool) {
    formContext.getControl("ap360_productfamily").setDisabled(bool);
    formContext.getControl("ap360_serviceproductmappingid").setDisabled(bool);
    formContext.getControl("ap360_ggparent").setDisabled(bool);
    formContext.getControl("ap360_gparent").setDisabled(bool);
    formContext.getControl("ap360_parent").setDisabled(bool);
    formContext.getControl("ap360_child").setDisabled(bool);
}

function createProductOnSave(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    Xrm.Utility.showProgressIndicator("Please wait...")

    var timer;
    if (formContext.ui.getFormType() == 1) {
        timer = 5000;
    } else {
        timer = 2000;
    }

    var approveProduct = formContext.getAttribute("ap360_approveproduct").getValue()

    if (approveProduct) {
        setTimeout(function () {
            var parameters = {};
            var entityGUID = formContext.data.entity.getId();
            parameters.entityGUID = entityGUID.replace('{', '').replace('}', '');
            parameters.entityName = formContext.data.entity.getEntityName();

            var req = new XMLHttpRequest();
            req.open("POST", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ap360_CreateProductFromDescription", false);
            req.setRequestHeader("OData-MaxVersion", "4.0");
            req.setRequestHeader("OData-Version", "4.0");
            req.setRequestHeader("Accept", "application/json");
            req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
            req.onreadystatechange = function () {
                debugger;
                if (req.readyState === 4) {
                    req.onreadystatechange = null;
                    if (req.status === 200) {
                        var results = JSON.parse(req.response);


                        if (results.IsProductPotentialMatched == true) {
                            Xrm.Utility.alertDialog((results.Message).toString());
                            Xrm.Utility.closeProgressIndicator();
                            FilterPotentialMatchedProduct(formContext, results.lstPotentialMatchedProduct)
                        }
                        else if (results.IsProductPotentialMatched == false && results.Message == "Not Processed") {
                            createProductOnSave(executionContext);
                            Xrm.Utility.closeProgressIndicator();
                        }
                        else if (results.IsSuccess == false) {
                            Xrm.Utility.alertDialog((results.Message).toString());
                            Xrm.Utility.closeProgressIndicator();
                        }
                        else {
                            setTimeout(function () {
                                formContext.data.refresh();
                                Xrm.Utility.closeProgressIndicator();
                                if (results.Message.contains("Created")) {
                                    // Xrm.Utility.alertDialog((results.Message).toString());
                                }
                            }, 1000)
                        }
                    } else {
                        var results = JSON.parse(req.response);
                        Xrm.Utility.closeProgressIndicator()
                        //Xrm.Utility.alertDialog((results.error.message).toString());
                        Xrm.Utility.alertDialog("request status =" + req.status + ".System Was unable to create product");
                    }
                }
                else {
                    Xrm.Utility.closeProgressIndicator()
                    Xrm.Utility.alertDialog("readyState =" + req.readyState + ".System Was unable to create product");
                }
            };
            req.send(JSON.stringify(parameters));
        }, timer);

    } else {
        Xrm.Utility.closeProgressIndicator()
    }
}
var lstPotentialMatchedProductIds = null;

function FilterPotentialMatchedProduct(formContext, lstPotentialMatchedProduct) {



    var lookupData = new Array();
    var lookupItem = new Object();
    lookupItem.id = "62a07066-e3ed-e911-a811-000d3a579c6d" //Vehicle ID;
    lookupItem.name = "Vehicle"; // Entity record name
    lookupItem.entityType = "product";
    lookupData[0] = lookupItem;

    formContext.getAttribute("ap360_ggparent").setValue(lookupData);
    formContext.getAttribute("ap360_productfamily").setValue(lookupData);

    formContext.getAttribute("ap360_gparent").setValue(null);
    formContext.getAttribute("ap360_parent").setValue(null);
    formContext.getAttribute("ap360_child").setValue(null);

    lstPotentialMatchedProductIds = lstPotentialMatchedProduct.split(',');

    var filterPotentailMatchedProductIds = "";
    for (var i = 0; i < lstPotentialMatchedProductIds.length; i++) {
        filterPotentailMatchedProductIds += "<condition attribute=\"productid\" operator=\"eq\"  value=\"" + lstPotentialMatchedProductIds[i] + "\" />";
    }

    var filter = "<filter type =\"or\" >" +
        filterPotentailMatchedProductIds +
        "</filter>"


    formContext.getControl("ap360_product").addPreSearch(addPotentialMatchedProductFilter);
}

function addPotentialMatchedProductFilter() {



    var filterPotentailMatchedProductIds = "";
    for (var i = 0; i < lstPotentialMatchedProductIds.length; i++) {
        filterPotentailMatchedProductIds += "<condition attribute=\"productid\" operator=\"eq\"  value=\"" + lstPotentialMatchedProductIds[i] + "\" />";
    }

    var filter = "<filter type =\"or\" >" +
        filterPotentailMatchedProductIds +
        "</filter>"

    //"<condition attribute=\"customertypecode\" operator=\"eq\"  value=\"" + productFamilyId + "\" />" +
    // "<condition attribute=\"ap360_partnumber\" operator=\"like\" value=\"%" + partNumber + "%\" />" +
    Xrm.Page.getControl("ap360_product").addCustomFilter(filter);

}

function UpdateSalePriceAndMultiplier(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext(); // get formContext
    var tyrebatteryMarkup = formContext.getAttribute("ap360_tyrebatterymarkup").getValue();
    var quantity = null;
    var unitcost = null;
    var multiplier = null;
    var quoteproducttype = formContext.getAttribute("ap360_quoteproducttype").getValue();
    if (quoteproducttype != 126300000)//  Customer Supplied
    {

        quantity = formContext.getAttribute("ap360_quantity").getValue();
        unitcost = formContext.getAttribute("ap360_unitcost").getValue();
        if (unitcost != null) {
            if (tyrebatteryMarkup == false) {
                multiplier = getPriceMarkUpMatrix(unitcost);
            }
            else {
                multiplier = 1.25;
            }
        }
        if (quantity != null && unitcost != null && multiplier != null) {
            var unitprice = multiplier * unitcost * quantity;
            //unitprice = roundUp(unitprice, 1);

            unitprice = parseFloat(unitprice.toFixed(2));
            formContext.getAttribute("ap360_unitprice").setValue(unitprice);
            formContext.getAttribute("ap360_multiplier").setValue(multiplier);
            formContext.getAttribute("ap360_partsaleprice").setValue(unitcost * multiplier);
        }
        else {
            formContext.getAttribute("ap360_unitprice").setValue(null);
            formContext.getAttribute("ap360_partsaleprice").setValue(null);
        }
    }
    else {
        formContext.getAttribute("ap360_unitprice").setValue(null);
        formContext.getAttribute("ap360_partsaleprice").setValue(null);
        formContext.getAttribute("ap360_multiplier").setValue(null);
    }
}

function ToggleCreateCoreFieldOnBasisOfProductField(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext(); // get formContext
    var product = formContext.getAttribute("ap360_product").getValue();
    if (formContext.ui.getFormType() == 1 && product == null) {
        formContext.getControl("ap360_core").setVisible(false);
    } else {
        formContext.getControl("ap360_core").setVisible(true);
    }


}
