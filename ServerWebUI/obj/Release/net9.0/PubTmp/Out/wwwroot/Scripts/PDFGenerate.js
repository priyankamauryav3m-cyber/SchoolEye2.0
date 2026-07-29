function DownloadPdf(filename, byteBase64) {
    var link = document.createElement('a');
    link.download = filename;
    link.href = "data:application/pdf;base64," + byteBase64;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

function ViewPdf(iframeId, byteBase64) {
    document.getElementById(iframeId).innerHTML = "";
    var iframe = document.createElement('iframe');
    iframe.setAttribute("src", "data:application/pdf;base64," + byteBase64);
    iframe.style.width = "100%";
    iframe.style.height = "680px";
    document.getElementById(iframeId).appendChild(iframe);
}

function OpenPdfNewTab(filename, byteBase64) {
    var blob = base64ToBlob(byteBase64, "application/pdf");
    var blobURL = URL.createObjectURL(blob);
    window.open(blobURL);
}

function base64ToBlob(base64, contentType) {
    var byteCharacters = atob(base64);
    var byteArrays = [];

    for (var offset = 0; offset < byteCharacters.length; offset += 512) {
        var slice = byteCharacters.slice(offset, offset + 512);
        var byteNumbers = new Array(slice.length);
        for (var i = 0; i < slice.length; i++) {
            byteNumbers[i] = slice.charCodeAt(i);
        }
        var byteArray = new Uint8Array(byteNumbers);
        byteArrays.push(byteArray);
    }

    return new Blob(byteArrays, { type: contentType });
}
