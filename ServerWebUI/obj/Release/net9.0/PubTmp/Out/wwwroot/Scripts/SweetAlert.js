window.showDeleteConfirm = function () {
    return Swal.fire({
        title: "Are you sure delete this record?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then(result => result.isConfirmed);
};
window.showActiveConfirm = function () {
    return Swal.fire({
        title: "Are you sure active this record?",
        text: "You won't be able to active this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes,activeted it!"
    }).then(result => result.isConfirmed);
};
window.showPrintConfirm = function () {
    return Swal.fire({
        title: "Choose Print Option",
        text: "Select how you want to export the record.",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#28a745",
        confirmButtonText: "Print PDF",
        cancelButtonText: "Print Excel"
    }).then(result => result);
};
function downloadPdf(base64Data, fileName) {
    const link = document.createElement('a');
    link.href = "data:application/pdf;base64," + base64Data;
    link.download = fileName;
    link.click();
}
function downloadExcel(base64, fileName) {
    const link = document.createElement('a');
    link.href = "data:application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;base64," + base64;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}