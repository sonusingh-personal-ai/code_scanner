function getResponseIds() {
    var responseTr = $("#responseTbl > tbody > tr");
    var ids = [];
    $.each(responseTr, function (i, v) {
        ids.push(parseInt(responseTr[i].cells[0].textContent))
    })
    return ids;
}

function deleteAllResp() {
    var ids = getResponseIds();

    if (!ids || (Array.isArray(ids) && ids.length === 0)) {
        alert('No responses selected.');
        return;
    }

    var idsString = Array.isArray(ids) ? ids.join(',') : ids;

    if (!confirm('Are you sure you want to delete the selected responses?')) {
        return;
    }

    // 1. Show UI indicator
    showLoader();

    // 2. Wrap AJAX call in setTimeout to ensure browser renders UI change before processing request
    setTimeout(function () {
        $.ajax({
            type: "POST",
            url: "/response/deleteAll",
            data: { ids: idsString },
            success: function (resp) {
                var ok = (typeof resp === 'string') ? (resp === 's') : (resp && resp.success === true);

                if (ok) {
                    var deletedCount = resp.deleted || 0;
                    var msg = deletedCount > 0
                        ? 'Selected responses successfully deleted. Total deleted: ' + deletedCount
                        : 'Selected responses successfully deleted.';

                    alert(msg);

                    // Reload the page upon successful deletion
                    location.reload();
                } else {
                    hideLoader();
                    var errorMsg = (resp && resp.message) ? resp.message : 'Selected responses failed to delete.';
                    if (typeof toastersetting === 'function') {
                        toastersetting(errorMsg, 'Error', 'error', '#FF0000');
                    } else {
                        alert(errorMsg);
                    }
                }
            },
            error: function (xhr, status, error) {
                hideLoader();
                console.error("Delete request failed:", status, error);
                if (typeof toastersetting === 'function') {
                    toastersetting('Failed to delete responses.', 'Error', 'error', '#FF0000');
                } else {
                    alert('An error occurred while attempting to delete responses.');
                }
            }
        });
    }, 50);
}

function getSelectedResponse() {
    var responseTr = $("#example > tbody > tr");
    var ids = getResponseIds();
    alert("Are you sure, you want to export? ");
    // 1. Show status message and disable Export button
    showExportLoader();
    // 2. Wrap in setTimeout to allow UI to render status text before executing AJAX
    setTimeout(function () {
        $.ajax({
            async: true,
            type: "POST",
            url: "/excel/download/",
            data: { ids: ids },
            success: function (resp) {
                var ok = (typeof resp === 'string') ? (resp === 's') : (resp && resp.success === true);

                if (ok) {
                    if (typeof toastersetting === 'function') {
                        toastersetting("Excel successfully created", "Success", "success", "#008000");
                    } else {
                        alert("Excel successfully created");
                    }
                } else {
                    var errorMsg = (resp && resp.message) ? resp.message : "Excel failed to create.";
                    if (typeof toastersetting === 'function') {
                        toastersetting(errorMsg, "Error", "error", "#FF0000");
                    } else {
                        alert(errorMsg);
                    }
                }
            },
            error: function (xhr, status, error) {
                console.error("Export request failed:", status, error);
                if (typeof toastersetting === 'function') {
                    toastersetting("An error occurred during export.", "Error", "error", "#FF0000");
                } else {
                    alert("An error occurred while attempting to export.");
                }
            },
            complete: function () {
                // 3. Reset button state and hide status message
                hideExportLoader();
            }
        });
    }, 50);
}

// Store original button HTML markup on initialization or fallback to markup
var originalDeleteBtnHtml = $('#btnDeleteAll').html() || '<i class="fa fa-trash"></i>';
var originalExportBtnHtml = $('#btnExport').html() || '<i class="fa fa-file-excel-o"></i>';

function showLoader() {
    $('#btnDeleteAll').prop('disabled', true).html('<i class="fa fa-spinner fa-spin"></i> Deleting...');
    $('#statusMessage').text('Deleting...').show();
}

function hideLoader() {
    $('#btnDeleteAll').prop('disabled', false).html(originalDeleteBtnHtml);
    $('#statusMessage').text('').hide();
}

function showExportLoader() {
    $('#btnExport').prop('disabled', true).html('<i class="fa fa-spinner fa-spin"></i> Exporting...');
    $('#statusMessage').css('color', '#0275d8').text('Exporting, please wait...').show();
}

function hideExportLoader() {
    $('#btnExport').prop('disabled', false).html(originalExportBtnHtml);
    $('#statusMessage').text('').hide();
}