// JavaScript source code

function onload(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    Lockform(executionContext);
}

function Lockform(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext(); {
        formContext.control.forEach(function (control, index) {
            var controlType = control.getControlType();
            if (controlType != 'iframe' && controlType != 'webresource' && controlType != 'subgrid')
                control.setDisabled(true);
        });
    }
}
