// JavaScript source code
// JavaScript source code
function onLoad(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var formType = formContext.ui.getFormType();
    if (formType != 1) {
        formContext.getControl("msdyn_quantity").setDisabled(true);
        formContext.getControl("msdyn_unitcreditamount").setDisabled(true);
    }

}


function receiveAmount(selecteditemids, PrimaryControl, SelectedItemReference) {
    debugger;
    var IsValidated = true;
    var ReturnArray = [];
    IsValidated = ValidateIfPartiallyReceieveProductIsLessThenQuantity(selecteditemids, PrimaryControl, IsValidated);
    if (IsValidated) {
        Xrm.Utility.showProgressIndicator("Please wait while amount is being received...");
        var selectedRTVproductsGUID = "";
        debugger;
        var count = 1;
        for (var i = 0; i < selecteditemids.length; i++) {
            selectedRTVproductsGUID += selecteditemids[i];
            if (count > 1 || count < selecteditemids.length)
                selectedRTVproductsGUID += ",";
            count++;
            //callBack(selecteditemids[i]);
        }
        if (selecteditemids.length > 1)// if only one then no need to remove last character as in case of One record last character is Guid char  
        {
            selectedRTVproductsGUID = selectedRTVproductsGUID.substring(0, selectedRTVproductsGUID.length - 1);

        }
        callAction(selectedRTVproductsGUID);
    }
}



function ValidateIfPartiallyReceieveProductIsLessThenQuantity(selecteditemids, PrimaryControl, IsValidated) {
    debugger;

    var gridContext = PrimaryControl.getControl("RTV_Product"); // get the grid context provinding subgrid name
    var gridRows = gridContext.getGrid().getRows();
    //loop through each row to get values of each column

    gridRows.forEach(function (row, i) {
        selecteditemids.forEach(function (ItemId, j) {
            if (row._entityId.guid == ItemId) {


                var gridColumns = row.getData().getEntity().attributes;
                var ap360_quantityreceived = null;
                var ap360_partialreceivequantity = null;
                var msdyn_quantity = null;
                ap360_quantityreceived = gridColumns.getByName("ap360_quantityreceived").getValue();
                ap360_partialreceivequantity = gridColumns.getByName("ap360_partialreceivequantity").getValue();
                msdyn_quantity = gridColumns.getByName("msdyn_quantity").getValue();
                msdyn_systemstatus = gridColumns.getByName("ap360_systemstatus").getValue();

                if (ap360_quantityreceived == null) {
                    ap360_quantityreceived = 0;
                }

                if (ap360_partialreceivequantity == null) {
                    ap360_partialreceivequantity = 0;
                }

                if (ap360_partialreceivequantity == 0.00) {
                    Xrm.Utility.alertDialog("Quantity cannot be zero");
                    IsValidated = false;
                }
                else if (msdyn_systemstatus == 126300002)//Refund Received
                {
                    Xrm.Utility.alertDialog("Selected RTV(s) is already received");
                    IsValidated = false;
                }
                else if (((msdyn_quantity - ap360_quantityreceived) - ap360_partialreceivequantity) < 0) {

                    var remainingQuantity = msdyn_quantity - ap360_quantityreceived;
                    Xrm.Utility.alertDialog("Only " + remainingQuantity + " item(s) availalbe to receive");
                    IsValidated = false;
                } 

            }
        })


    });


    return IsValidated;


}


function callAction(selectedRTVProducts) {
    debugger;
    try {

        var entity = {};

        entity.selectedRTVProducts = selectedRTVProducts;

        var req = new XMLHttpRequest();
        req.open("POST", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ap360_ReceiveRTVProductAmont", true);
        req.setRequestHeader("OData-MaxVersion", "4.0");
        req.setRequestHeader("OData-Version", "4.0");
        req.setRequestHeader("Accept", "application/json");
        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        req.onreadystatechange = function () {
            if (this.readyState === 4) {
                req.onreadystatechange = null;
                if (this.status === 204) {
                    Xrm.Page.getControl("RTV_Product").refresh();
                    Xrm.Utility.closeProgressIndicator();
                } else if (req.status === 400) {
                    result = JSON.parse(req.response);
                    Xrm.Utility.closeProgressIndicator();
                    Xrm.Utility.alertDialog(result.error.message);
                    return;
                }
            }
        };
        req.send(JSON.stringify(entity));

        //isSuccessful
        //Process.callAction("ap360_ReceiveRTVProductAmont",
        //    [{
        //        key: "selectedRTVProducts",
        //        type: Process.Type.String,
        //        value: selectedRTVProducts

        //    }],
        //    function (params) {

        //       

        //    },
        //    function (e, t) {
        //        alert(e);
        //        Xrm.Utility.closeProgressIndicator();
        //    });
        //   }
    } catch (e) {
        Xrm.Utility.closeProgressIndicator();

        Xrm.Utility.alertDialog(e.message);

    }
}

function updateTotalCreditOnChangeOfRTVReturnFee(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var returnFee = formContext.getAttribute("ap360_returnfee").getValue();
    var quantity = formContext.getAttribute("msdyn_quantity").getValue();
    var unitPrice = formContext.getAttribute("msdyn_unitcreditamount").getValue();
    var totalCreditAmount = (quantity * unitPrice) - returnFee;
    formContext.getAttribute("msdyn_totalcreditamount").setValue(totalCreditAmount);
}

function onRecordSelect(exeContext) {
    //debugger;
    var _formContext = exeContext.getFormContext();
    var disableFields = ["ap360_systemstatus", "msdyn_product", "ap360_partnumber", "msdyn_quantity", "msdyn_totalcreditamount", "msdyn_unitcreditamount", "msdyn_lineorder", "ap360_quantityreceived"];
    lockFields(exeContext, disableFields);
}


function lockFields(exeContext, disableFields) {
    var _formContext = exeContext.getFormContext();
    var currentEntity = _formContext.data.entity;
    currentEntity.attributes.forEach(function (attribute, i) {
        if (disableFields.indexOf(attribute.getName()) > -1) {
            var attributeToDisable = attribute.controls.get(0);
            attributeToDisable.setDisabled(true);
        }
    });
}
