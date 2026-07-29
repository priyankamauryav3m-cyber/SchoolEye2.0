window.showDeleteConfirm = function () {
    return Swal.fire({
        title: "Are you sure delete this record?",
        icon: "warning",
        timer: 10000,
        timerProgressBar: true,
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then(result => result.isConfirmed);
};
window.showActiveConfirm = function () {
    return Swal.fire({
        title: "Are you sure active this record?",
        icon: "warning",
        timer: 10000,
        timerProgressBar: true,
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes,activeted it!"
    }).then(result => result.isConfirmed);
};
window.showUpdateConfirm = function () {
    return Swal.fire({
        title: "Are you sure you want to  update this record?",
        icon: "warning",
        timer: 10000,
        timerProgressBar: true,
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes,updated it!"
    }).then(result => result.isConfirmed);
};

window.showSelectRecordAlert = function () {
    Swal.fire({
        icon: 'warning',
        title: 'Please select a record !',
        timer: 10000,
        timerProgressBar: true,
        confirmButtonText: 'OK',
        allowOutsideClick: true,
        allowEscapeKey: true
    });
};

window.showPrintConfirm = function () {
    return Swal.fire({
        title: "Choose Print Option",
        text: "Select how you want to export the record.",
        icon: "info",
        timer: 10000,
        timerProgressBar: true,
        showDenyButton: true,
        showCancelButton: false,

        confirmButtonText: "Print PDF",
        denyButtonText: "Print Excel",

        confirmButtonColor: "#3085d6",
        denyButtonColor: "#28a745",

        allowOutsideClick: true,
        allowEscapeKey: true,
        allowEnterKey: false,
        backdrop: true
    });
};

window.downloadPdf = function (base64, fileName) {
    const link = document.createElement("a");
    link.href = "data:application/pdf;base64," + base64;
    link.download = fileName;
    link.click();
};
function downloadExcel(base64, fileName) {
    const link = document.createElement('a');
    link.href = "data:application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;base64," + base64;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}
window.viewPdf = function (base64Pdf) {

    const newTab = window.open("about:blank", "_blank");
    newTab.document.body.innerHTML =
        "<iframe width='100%' height='100%' src='data:application/pdf;base64,"
        + base64Pdf + "'></iframe>";
    newTab.document.close();

}

window.openDatePickerFromWrapper = (wrapper) => {
    if (!wrapper) return;

    const input = wrapper.querySelector("input[type='date']");
    if (input?.showPicker) {
        input.showPicker();
    } else if (input) {
        input.focus();
    }
};
window.openDatePicker = (el) => {
    el.showPicker();
};

window.showRegistrationSuccess = async function (message) {
    const result = await Swal.fire({
        html: message,
        icon: "success",
        timer: 20000,
        timerProgressBar: true,
        showCancelButton: true,
        confirmButtonText: "Yes",
        cancelButtonText: "No"
    });
    if (result.dismiss === Swal.DismissReason.timer) {
        return false;
    }

    return result.isConfirmed;
}
window.scrollToElement = (id) => {
    var element = document.getElementById(id);
    if (element) {
        element.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
};
window.showProcessingAlert = () => {

    Swal.fire({
        title: 'Processing...',
        html: 'Please wait...',
        allowOutsideClick: false,
        allowEscapeKey: false,

        didOpen: () => {
            Swal.showLoading();
        }
    });

};

window.closeSwal = () => {
    Swal.close();
};