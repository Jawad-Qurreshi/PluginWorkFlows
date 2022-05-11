function setName_OnChangeofYear_Manufacturer_Model(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();

    var vehicleName = " ";

    var year = formContext.getAttribute("ap360_year").getValue().toString();
    var manufacturer = formContext.getAttribute("ap360_manufacturer").getValue();
    var model = formContext.getAttribute("ap360_model").getValue();

    if (year != null) {
        vehicleName += year.replace(",","");
    }
    if (manufacturer != null) {
        vehicleName += " ";
        vehicleName += manufacturer;
    }
    if (model != null) {
        vehicleName += " ";
        vehicleName += model;
    }

    formContext.getAttribute("ap360_name").setValue(vehicleName);



}