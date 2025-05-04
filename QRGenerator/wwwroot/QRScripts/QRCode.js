$(document).ready(function () {
    function onQRCodeTypeChange() {
        const qrcodeType = $("#QRCodeType").val();

        // Hide all type-specific divs
        $(".hideDiv").slideUp("fast");

        // Show the selected one
        $("#DIV" + qrcodeType).slideDown("fast");
    }
    onQRCodeTypeChange();
    $("#QRCodeType").on("change", onQRCodeTypeChange);
});

// Reset button reloads the page
function resetButton() {
    window.location.reload();
}

