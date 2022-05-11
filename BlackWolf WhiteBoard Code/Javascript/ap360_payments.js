// JavaScript source code
function onLoad(executionContext) {
    var formContext = executionContext.getFormContext();
    var formType = formContext.ui.getFormType();
    if (formType == 1) // Create
    {
        var currentDateTime = new Date();
        formContext.getAttribute("msdyn_date").setValue(currentDateTime);
    }
}

function calculateCreditCardFee_onChangeOfPaymentTypeAndAmount(executionContext) {
    var formContext = executionContext.getFormContext();
    var paymentType = formContext.getAttribute("msdyn_paymenttype").getValue();
    var amountBeforeFee = formContext.getAttribute("ap360_amountbeforefee").getValue();

    if (paymentType == 690970002) //Credit Card
    {
        formContext.getControl("msdyn_checknumber").setVisible(false);
        formContext.getAttribute("msdyn_checknumber").setValue(null);

        formContext.getControl("ap360_creditcardfee").setVisible(true);
        formContext.getControl("ap360_creditcardfeepaymentoptions").setVisible(true);
        formContext.getAttribute("ap360_creditcardfeepaymentoptions").setRequiredLevel("required");
        if (amountBeforeFee != null) {
            var creditCardFee = amountBeforeFee * 0.025;
            formContext.getAttribute("ap360_creditcardfee").setValue(creditCardFee);
            var ccFeePaymentOption = formContext.getAttribute("ap360_creditcardfeepaymentoptions").getValue();
            //Add to today's payment	126300000
            //Apply to opportunity account	126300001
            if (ccFeePaymentOption == 126300000) // Add to today's payment	
            {
                var calCreditCardFee = formContext.getAttribute("ap360_creditcardfee").getValue();
                var amountApplied = amountBeforeFee + calCreditCardFee;
                formContext.getAttribute("msdyn_amount").setValue(amountApplied);
            }
            else {
                formContext.getAttribute("msdyn_amount").setValue(amountBeforeFee);
            }
        }

    }
    else if (paymentType == 690970001) {

        formContext.getControl("msdyn_checknumber").setVisible(true);
        formContext.getAttribute("msdyn_amount").setValue(amountBeforeFee);
        formContext.getAttribute("ap360_creditcardfee").setValue(null);
        formContext.getControl("ap360_creditcardfee").setVisible(false);
        formContext.getAttribute("ap360_creditcardfeepaymentoptions").setValue(null);
        formContext.getControl("ap360_creditcardfeepaymentoptions").setVisible(false);
        formContext.getAttribute("ap360_creditcardfeepaymentoptions").setRequiredLevel("none");
    }
    else {

        formContext.getAttribute("msdyn_amount").setValue(amountBeforeFee);
        formContext.getAttribute("ap360_creditcardfee").setValue(null);
        formContext.getControl("ap360_creditcardfee").setVisible(false);
        formContext.getAttribute("ap360_creditcardfeepaymentoptions").setValue(null);
        formContext.getControl("ap360_creditcardfeepaymentoptions").setVisible(false);
        formContext.getAttribute("ap360_creditcardfeepaymentoptions").setRequiredLevel("none");
    }
}

function visibleCreditCardFeeAndCCFeePaymentOptionFields_onloadOfPayment(executionContext) {
    var formContext = executionContext.getFormContext();
    var paymentType = formContext.getAttribute("msdyn_paymenttype").getValue();
    if (paymentType == 690970002) //Credit Card
    {
        formContext.getControl("ap360_creditcardfee").setVisible(true);
        formContext.getControl("ap360_creditcardfeepaymentoptions").setVisible(true);
    }
    else {

        formContext.getControl("ap360_creditcardfee").setVisible(false);
        formContext.getControl("ap360_creditcardfeepaymentoptions").setVisible(false);
    }
}

