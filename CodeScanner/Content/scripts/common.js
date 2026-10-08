if (typeof toastersetting !== 'function') {
    window.toastersetting = function (message, title, type, colorCode) {
        alert((title ? title + ": " : "") + message);
    };
}

var isOk = true;
var currentScanId = 0;

function saveSetting() {
    var tableValue = $("#settingTr > tr")
    var settingInfoArray = [];
    setSetting();
    $.each(tableValue, function (i, v) {
        settingInfo = {};
        var parm = $("#parm_" + (i + 1)).val();
        var status = $("#isStatus_" + (i + 1)).is(":checked")
        settingInfo.parameters = parm;
        settingInfo.status = status;
        settingInfoArray.push(settingInfo)
    })

    setting.settingInfo = settingInfoArray;
    saveResult()
}
var settingInfo = {
    parameters: "",
    status: false
}
var setting = {
    Id: 0,
    Header: "",
    Footer: "",
    Fields: 0,
    FileId: "",
    CreatedOn: "",
    ModifiedOn: "",
    settingInfo: []
}
function setSetting() {
    var header = $("#input_header").val();
    var footer = $("#input_footer").val();
    var fields = parseInt($("#input_fields").val());
    var fileId = $("#fileId").val();

    setting.Header = header;
    setting.Footer = footer;
    setting.Fields = fields;
    setting.FileId = fileId;
}
function saveResult() {
    $.ajax({
        async: true,
        type: "post",
        url: "/home/setting/",
        data: setting,
        success: function (resp) {
            if (resp == "s") {
                alert("Successfully saved")
            } else {
                alert("Failed to saved")
            }
        },
        error: function (xhr, status) {
            toastersetting(resp.message, resp.title, resp.type, resp.colorCode);
        }
    })
}
function setInfo() {
    var dt = new Date();
    var date = pad(dt.getDate()) + "/"
        + pad((dt.getMonth() + 1), 2) + "/"
        + dt.getFullYear();
    var time = dt.getHours() + ":" + dt.getMinutes() + ":" + dt.getSeconds();

    console.log(date);
    $("#info_date").val(date);
    $("#info_time").val(time);
}
function pad(str, max) {
    str = str.toString();
    return str.length < max ? pad("0" + str, max) : str;
}

function getSetting(id) {
    $.ajax({
        async: true,
        type: "GET",
        url: "/setting/getByFileId?fileId=" + id,
        success: function (resp) {
            $("#input_header").val(resp.Header);
            $("#input_footer").val(resp.Footer);
            $("#input_fields").val(resp.Fields);
            $("#settingTr").empty();
            if (resp.Id > 0) {
                $.each(resp.SettingInfo, function (i, v) {
                    var tr = '<tr><td style="width:10%">' + (i + 1) + ' </td> <td style="width:50%"><input id="parm_' + (i + 1) + '" class="form-control" value="' + v.Parameters + '" /></td> <td class="jsgrid-cell jsgrid-align-center" style="width: 100px;"> <center> <input type="checkbox" id="isStatus_' + (i + 1) + '"> </center></td></tr>';
                    $("#settingTr").append(tr)
                    $('#isStatus_' + (i + 1)).prop('checked', v.Status);
                })
            } else {
                alert("This setting is not defined Please fill the product detail")
            }
        },
        error: function (xhr, status) {
        }
    })
}
function openPort() {
    $.ajax({
        async: true,
        type: "GET",
        url: "/comport/openport",
        success: function (resp) {
        },
        error: function (xhr, status) {
        }
    })
}
var respStatus = false;

function validateMandatoryFields() {
    var allValid = true;
    var firstInvalid = null;

    $(".validate, .isValidate").each(function () {
        var $field = $(this);
        var id = $field.attr("id");

        // Skip Line, Printer Model, and Printer since they have dedicated line-aware validators
        if (id === "ddlLine" || id === "lineSelect" || id === "printerModel" || id === "prnter") {
            return;
        }

        var value = $field.val();
        var isEmpty = value === null || value === undefined || value === "" || value === "0";

        if (isEmpty) {
            $field.addClass("isValidate").removeClass("validate");
            allValid = false;
            if (!firstInvalid) {
                firstInvalid = $field;
            }
        } else {
            $field.removeClass("isValidate").addClass("validate");
        }
    });

    // Move focus to the first missing required field
    if (firstInvalid) {
        firstInvalid.focus();
    }

    return allValid;
}

function validateProductionLine() {
    if (infoValue.productionLine == 2 && infoValue.serialCardNo.length < 10) {
        return false;
    }
    return true;
}

function SendToComPort(isRecurrence) {
    getInfoValue();

    // 1. Mandatory validation check with specific field names
    var missingFields = [];

    var lineValid = validateLine();
    if (!lineValid) {
        missingFields.push("Line");
    }

    var prodLine = $("#productionLine").val();
    if (prodLine == '2') {
        // Assembly line: Printer Model validation
        var printerModelValid = validatePrinterModel();
        if (!printerModelValid) {
            missingFields.push("Printer Model");
        }
    } else {
        // Card line: printer model is not required
        $("#printerModel").removeClass("isValidate").addClass("validate");
        $("#printerStatusBadge").hide();
    }
    // Printer is purely optional: testing is NEVER blocked if no printer is connected/selected
    $("#prnter").removeClass("isValidate");

    var mandatoryValid = validateMandatoryFields();

    $(".isValidate").each(function () {
        var id = $(this).attr("id");
        if (id !== "ddlLine" && id !== "lineSelect" && id !== "printerModel" && id !== "prnter") {
            var label = $(this).closest(".form-group").find("label").first().text().trim();
            if (!label && id) label = id;
            if (label && missingFields.indexOf(label) === -1) {
                missingFields.push(label);
            }
        }
    });

    if (missingFields.length > 0) {
        var msg = "Please fill or select the following required field(s) before scanning:\n• " + missingFields.join("\n• ");
        if (typeof toastersetting === 'function') {
            toastersetting(msg, "Validation Error", "error", "#FF0000");
        } else {
            alert(msg);
        }
        return;
    }

    // 2. Production line validation check
    if (!validateProductionLine()) {
        alert("If production line is assembly, card serial number must have a valid number.");
        return;
    }

    if (!isRecurrence) {
        currentScanId++;
        // Immediately reset Status badge and clear previous output tables
        $("#info_status").val("TESTING...").css({ "background-color": "#ffc107", "color": "#000", "font-weight": "bold" });
        $("#testResponse_0").empty();
        $("#testResponse_1").empty();
        $("#Testresponse").empty();
        $("#printerStatusBadge").hide();
        if (typeof initNewTestRunTelemetry === 'function') {
            initNewTestRunTelemetry(infoValue.barCode || $("#sysNumber").val() || $("#sysNumberDup").val());
        }
    }
    var scanId = currentScanId;
    $("#loadingbtn").show();
    setValueFromLocalStorage();
    checkBarcode(infoValue.barCode, infoValue.port, infoValue.baudRate, infoValue.visualby, infoValue.testedBy, infoValue.productionLine, infoValue.lineInCharge, infoValue.serialCardNo, infoValue.currentDate, infoValue.currentTime, isRecurrence, scanId);
}

function insertIntoDatabase(barcode, port, baudRate, isRepeat, visualby, testedBy, productionLine, lineInCharge, cardSerialNumber, currentDate, currentTime, isRecurrence, scanId) {
    $("#loadingbtn").show();
    if (isRepeat == undefined) {
        isRepeat = false;
        infoValue.isRepeat = false;
    }
    infoValue.isRecurrence = isRecurrence;
    infoValue.isRepeat = isRepeat;

    infoValue.disProgNo = $("#display_pv").val();

    window.currentTestAjaxRequest = $.ajax({
        async: true,
        type: "POST",
        url: "/comport/SendParameter",
        data: infoValue,
        complete: function () {
            window.currentTestAjaxRequest = null;
        },
        success: function (resp) {
            console.log(resp);
            isOk = resp ? resp.isOk : true;
            $("#checkbtn").show();
            $("#loadingbtn").hide();

            if (resp && resp.message) {
                $("#info_status").val("FAIL").css({ "background-color": "#F72F35", "color": "#fff", "font-weight": "bold" });
                if (typeof recordTestTelemetryError === 'function') {
                    recordTestTelemetryError({ status: 500 }, resp.message);
                }
                if (typeof toastersetting === 'function') {
                    toastersetting(resp.message, "Error", "error", "#FF0000");
                } else {
                    alert(resp.message);
                }
                console.log("error : ", resp.message);
                return;
            }

            if (!isOk) {
                $("#info_status").val("TESTING...").css({ "background-color": "#ffc107", "color": "#000", "font-weight": "bold" });
                setTimeout(function () { SendToComPort(true) }, 50);
            }

            $("#control_pv").val(resp.controlPv);
            $("#sysRating").val(resp.sysRating);
            $("#bCode").val(resp.model);

            // Update UI status field (#info_status) matching output table colors
            if (resp.status == 1 || resp.status === 'PASS') {
                if (resp.printerStatus) {
                    $("#info_status").val("PASS (" + resp.printerStatus + ")").css({ "background-color": "#81c57b", "color": "#000", "font-weight": "bold" });
                    showPrinterNotice(resp.printerStatus);
                } else {
                    $("#info_status").val("PASS").css({ "background-color": "#81c57b", "color": "#000", "font-weight": "bold" });
                    $("#printerStatusBadge").hide();
                }
            } else if (resp.status == 2 || resp.status === 'FAIL') {
                $("#info_status").val("FAIL").css({ "background-color": "#F72F35", "color": "#fff", "font-weight": "bold" });
            }

            $("#testResponse_0").empty();
            $("#testResponse_1").empty();

            $.each(resp.interType, function (i, v) {
                var color = "white";
                if (v.status == 'FAIL' || v.status == 'FAULT') {
                    color = "#F72F35"; // Soft light red
                } else if (v.status == 'PASS' || v.status == 'OK') {
                    color = "#81c57b"//light green
                }
                if ((i + 1) % 2 === 0) {
                    var tr = '<tr style="background:' + color + '"><td style="width:10%">' + (i + 1) + '</td> <td style="width:50%;font-weight: 700;"> ' + v.parameter + ' </td> <td style="font-weight: 700;"> ' + v.dispaly + ' </td> <td style="font-weight: 700;"> ' + v.actual + ' </td> <td style="font-weight: 700;"> ' + v.status + ' </td></tr>'
                    $("#testResponse_1").append(tr);
                } else {
                    var tr = '<tr style="background:' + color + '"><td style="width:10%">' + (i + 1) + '</td> <td style="width:50%;font-weight: 700;"> ' + v.parameter + ' </td> <td style="font-weight: 700;"> ' + v.dispaly + ' </td> <td style="font-weight: 700;"> ' + v.actual + ' </td> <td style="font-weight: 700;"> ' + v.status + ' </td></tr>'
                    $("#testResponse_0").append(tr);
                }
            })

            var tbl = '';
            var tr = '';
            $("#Testresponse").empty();
            $.each(resp.totalString, function (indx, val) {
                var sn = 1;
                tbl = '<tr class="cell-' + (indx + 1) + '" data-toggle="collapse" data-target="#demo-' + (indx + 1) + '"><td style="font-weight:bold" colspan="2">' + (indx + 1) + '.</td><td style="font-weight:bold" colspan="3">Param</td><td style="font-weight:bold" colspan="2">Display</td><td style="font-weight:bold" colspan="2">Actual</td><td style="font-weight:bold" colspan="2">Status</td></tr>'
                $.each(val, function (respIndx, result) {
                    if (respIndx > 4 && respIndx != (val.length - 1)) {
                        if (result.indexOf(":") != -1) {
                            tr += '<tr id="demo-' + (indx + 1) + '" class ="collapse cell-' + (indx + 1) + ' row-child"><td colspan="2">#' + sn + '</td><td colspan="3">' + resp.SettingInfoList[sn].Parameters + '</td><td colspan="2">' + result.split(":")[0] + '</td><td colspan="2">' + result.split(":")[1] + '</td><td colspan="2">' + result.split(":")[2] + '</td></tr>'
                        } else {
                            tr += '<tr id="demo-' + (indx + 1) + '" class ="collapse cell-' + (indx + 1) + ' row-child"><td colspan="2">#' + sn + '</td><td colspan="3">' + resp.SettingInfoList[sn].Parameters + '</td><td colspan="2">--</td><td colspan="2">--</td><td colspan="2">' + result + '</td></tr>'
                        }
                        sn++;
                    }
                })
                $("#Testresponse").append(tbl);
                $("#Testresponse").append(tr);
            });

            if (typeof recordTestTelemetryCycle === 'function') {
                recordTestTelemetryCycle(resp);
            }
        },
        error: function (xhr, status) {
            $("#checkbtn").show();
            $("#loadingbtn").hide();
            $("#info_status").val("FAIL").css({ "background-color": "#F72F35", "color": "#fff", "font-weight": "bold" });
            if (typeof recordTestTelemetryError === 'function') {
                recordTestTelemetryError(xhr, status);
            }
            var msg = "Request to the device failed (" + (xhr.status || status) + "). Please rescan.";
            if (typeof toastersetting === 'function') {
                toastersetting(msg, "Error", "error", "#FF0000");
            } else {
                //alert(msg);
                console.log("error :", msg)
            }
        }
    })
}

function setCustomSwtich() {
    //var isDispProg_No = localStorage.getItem("isDisplay_pv")
    //if (isDispProg_No == 'true') {
    //    $("#display_pv").removeAttr("disabled").val(localStorage.getItem("display_pv"));
    //    infoValue.isDispProgNo = true;
    //} else {
    //    $("#display_pv").attr("disabled", "disabled").val('')
    //    infoValue.isDispProgNo = false;
    //}
}


$("#fileId").on('change', function () {
    getSetting(this.value)
});

function checkBarcode(barcode, port, baudRate, visualby, testedBy, productionLine, lineInCharge, serialCardNo, currentDate, currentTime, isRecurrense, scanId) {
    if (!isRecurrense) {
        $.ajax({
            async: true,
            type: "GET",
            url: "/comport/checkbarcode?barCode=" + barcode + "&status=" + isRecurrense + "&qcStage=" + infoValue.qcStatus,
            success: function (resp) {
                if (resp) {
                    var r = confirm("Barcode already tested, If you confirmed, previous entry would be delete.");
                    if (r) {
                        insertIntoDatabase(barcode, port, baudRate, true, visualby, testedBy, productionLine, lineInCharge, serialCardNo, currentDate, currentTime, isRecurrense, scanId);
                    } else {
                        $("#loadingbtn").hide();
                        var portVal = $("#com_port").val();
                        if (portVal && portVal.length > 0) {
                            $("#info_status").val("Enable").css({ "background-color": "#81c57b", "color": "#000", "font-weight": "bold" });
                        } else {
                            $("#info_status").val("Ready").css({ "background-color": "#81c57b", "color": "#000", "font-weight": "bold" });
                        }
                    }
                } else {
                    insertIntoDatabase(barcode, port, baudRate, false, visualby, testedBy, productionLine, lineInCharge, serialCardNo, currentDate, currentTime, isRecurrense, scanId);
                }
            },
            error: function (xhr, status) {
                $("#loadingbtn").hide();
                $("#info_status").val("FAIL").css({ "background-color": "#F72F35", "color": "#fff", "font-weight": "bold" });
            }
        })
    } else {
        insertIntoDatabase(barcode, port, baudRate, false, visualby, testedBy, productionLine, lineInCharge, serialCardNo, currentDate, currentTime, isRecurrense, scanId)
    }
}

function setValueFromLocalStorage() {
    localStorage.setItem("qcStatus", infoValue.qcStatus);
    localStorage.setItem("visualBy", infoValue.visualby);
    localStorage.setItem("testedBy", infoValue.testedBy);
    localStorage.setItem("productionLine", infoValue.productionLine);
    localStorage.setItem("lineInCharge", infoValue.lineInCharge);
    localStorage.setItem("baudRate", infoValue.baudRate);
    localStorage.setItem("port", infoValue.port);
    localStorage.setItem("serialCardNo", infoValue.serialCardNo);
    localStorage.setItem("processEngg", infoValue.processEngg);
    localStorage.setItem("display_pv", infoValue.disProgNo);
    localStorage.setItem("isDisplay_pv", infoValue.isDispProgNo);

    // Save Printer Model selection
    localStorage.setItem("printerModel", infoValue.printerModelId);

    // Save Line selection
    localStorage.setItem("selectedLine", infoValue.line);

    // Save Printer Name selection
    localStorage.setItem("selectedPrinterName", infoValue.printerName);
}

function getValueFromLocalStorage() {
    $("#visualBy").val(localStorage.getItem("visualBy"));
    $("#testedBy").val(localStorage.getItem("testedBy"));
    $("#productionLine").val(localStorage.getItem("productionLine"));
    $("#procEngg").val(localStorage.getItem("lineInCharge"));
    $("#baudRate").val(localStorage.getItem("baudRate"));
    $("#com_port").val(localStorage.getItem("port"));
    $("#serialCardNo").val(localStorage.getItem("serialCardNo"));
    $("#QcStatus").val(localStorage.getItem("qcStatus"));
    $("#processEngg").val(localStorage.getItem("processEngg"));
    $("#display_pv").val(localStorage.getItem("display_pv"));

    // Auto-fill Printer Model from LocalStorage
    if (localStorage.getItem("printerModel")) {
        $("#printerModel").val(localStorage.getItem("printerModel")).trigger("change");
    }

    // Auto-fill Line from LocalStorage
    var savedLine = localStorage.getItem("selectedLine");
    if (savedLine) {
        $("#ddlLine, #lineSelect").val(savedLine).trigger("change");
    }

    // Auto-fill Printer Name from LocalStorage
    var savedPrinterName = localStorage.getItem("selectedPrinterName");
    if (savedPrinterName && $("#prnter option[value='" + savedPrinterName + "']").length > 0) {
        $("#prnter").val(savedPrinterName);
    }
    updatePrinterStatus();
}

function getInfoValue() {
    infoValue.currentDate = $("#info_date").val();//Date
    infoValue.currentTime = $("#info_time").val();//Time
    infoValue.qcStatus = $("#QcStatus").val();//Qc Status
    infoValue.testedBy = $("#testedBy").val();//Tested By
    infoValue.visualby = $("#visualBy").val();//Visual By
    infoValue.productionLine = $("#productionLine").val();//Production Line
    infoValue.processEngg = $("#processEngg").val();//Process Engg.
    infoValue.serialCardNo = $("#serialCardNo").val();//Card Serial No.
    infoValue.port = $("#com_port").val();//COM Port
    infoValue.barCode = $("#sysNumber").val();//sys. sr no
    infoValue.disProgNo = $("#display_pv").val();
    infoValue.baudRate = parseInt($("#baudRate").val());//Baud Rate
    infoValue.printerModelId = $("#printerModel").val() ? $("#printerModel option:selected").val() : "";
    infoValue.line = ($("#ddlLine").val() || $("#lineSelect").val() || "");
    infoValue.printerName = $("#prnter").val() || "";
}

function deleteResponse(id) {
    var status = confirm('Are you sure, You want to delete the selected Resonse?');
    if (status) {
        window.location.href = "/response/deleteResponseSummary/" + id;
    }
}

//model
var infoValue = {
    currentDate: "", //9
    currentTime: "", //10
    qcStatus: 0,
    testedBy: 0, //6
    visualby: 0, //5
    productionLine: 0, //7
    processEngg: 0,
    serialCardNo: "",//8
    port: "", //2
    baudRate: 0, //3
    barCode: "", //1
    printerModelId: 0,
    line: "",
    printerName: "",
    isRepeat: false,//4
    isRecurrence: true,
    disProgNo: "",
    isDispProgNo: false
}

var myVar = setInterval(myTimer, 1);
function myTimer() {
    var d = new Date();
    $("#info_time").val(d.toLocaleTimeString())
}

function savePath() {
    var qrCodePath = $("#qrCodePath").val();
    var excelPath = $("#excelPath").val();

    $.ajax({
        async: false,
        type: "GET",
        url: "/model/UpdatePath?qrCodePath=" + qrCodePath + "&excelPath=" + excelPath,
        success: function (resp) {
            if (resp == "s") {
                alert("Path successfully saved")
            } else {
                alert("Failed to saved")
            }
        },
        error: function (xhr, status) {
        }
    })
}

function showInputField(elm) {
    if (elm == 1) {
        $(".qrHide").show();
        $(".qrShow").hide();
    } else if (elm == 2) {
        $(".qrHide").hide();
        $(".qrShow").show();
    }
}

function startTesting() {
    var textVal = $("#sysNumberDup").val()
    $("#sysNumber").val(textVal);
    if (textVal != "") {
        SendToComPort(false);
    }
}

function validateFileds() {
    // Dynamic red border validation toggle on dropdown selection changes
    $(document).on("change input", ".validate, .isValidate", function () {
        var value = $(this).val();
        var isEmpty = value === null || value === undefined || value === "" || value === "0";

        if (isEmpty) {
            $(this).addClass("isValidate").removeClass("validate");
        } else {
            $(this).removeClass("isValidate").addClass("validate");
        }
    });
}

function validatePrinterModel() {
    var prodLine = $("#productionLine").val();
    if (prodLine != '2') {
        $("#printerModel").removeClass("isValidate").addClass("validate");
        return true;
    }

    var $printer = $("#printerModel");
    var printerModelVal = $printer.val();
    if (!printerModelVal || printerModelVal === "" || printerModelVal === "0") {
        $printer.addClass("isValidate").removeClass("validate");
        return false;
    }
    $printer.removeClass("isValidate").addClass("validate");
    return true;
}

function applyPrinterStatusBadge(resp) {
    var $badge = $("#printerStatusBadge");
    $("#prnter").removeClass("isValidate");
    if (!resp) {
        $badge.hide();
        return;
    }
    if (resp.isConnected) {
        $badge.removeClass("badge-danger badge-warning badge-secondary").addClass("badge-success")
              .html('<i class="fas fa-check-circle mr-1"></i> ' + (resp.statusText || "Connected"))
              .attr("title", resp.message || "Printer is ready.")
              .show();
    } else {
        $badge.removeClass("badge-success badge-warning").addClass("badge-secondary")
              .html('<i class="fas fa-print mr-1"></i> ' + (resp.statusText || "Print Disabled"))
              .attr("title", resp.message || "No printer connected. Print command will not be invoked.")
              .show();
    }
}

function verifyPrinterConnection(callback) {
    var prodLine = $("#productionLine").val();
    $("#prnter").removeClass("isValidate");
    if (prodLine != '2') {
        $("#printerStatusBadge").hide();
        window.printerStatusData = { isConnected: false };
        if (callback) callback(false);
        return;
    }

    var selectedPrinter = $("#prnter").val();
    if (!selectedPrinter || selectedPrinter === "" || selectedPrinter === "No printer connected" || selectedPrinter === "Select Printer" || selectedPrinter === "Select") {
        window.printerStatusData = { isConnected: false, statusText: "Print Disabled", message: "No printer selected. Print command will not be invoked." };
        applyPrinterStatusBadge(window.printerStatusData);
        if (callback) callback(false);
        return;
    }

    var $badge = $("#printerStatusBadge");
    $badge.removeClass("badge-success badge-danger badge-secondary").addClass("badge-warning")
          .html('<i class="fas fa-spinner fa-spin mr-1"></i> Checking...')
          .show();
    $("#iconVerifyPrinter").addClass("fa-spin");

    $.ajax({
        type: "GET",
        url: "/Home/CheckPrinterStatus",
        data: { printerName: selectedPrinter },
        cache: false,
        success: function (resp) {
            $("#iconVerifyPrinter").removeClass("fa-spin");
            window.printerStatusData = resp;
            applyPrinterStatusBadge(resp);
            if (callback) callback(resp && resp.isConnected);
        },
        error: function () {
            $("#iconVerifyPrinter").removeClass("fa-spin");
            window.printerStatusData = { isConnected: false, statusText: "Offline", message: "Unable to verify printer. Print command will not be invoked." };
            applyPrinterStatusBadge(window.printerStatusData);
            if (callback) callback(false);
        }
    });
}

function updatePrinterStatus() {
    verifyPrinterConnection();
}

function validateLine() {
    var $lineElem = $("#ddlLine").length > 0 ? $("#ddlLine") : $("#lineSelect");
    var lineVal = $lineElem.val();
    var $select2Selection = $lineElem.next('.select2-container').find('.select2-selection');

    if (!lineVal || lineVal === "" || lineVal === "0" || lineVal === "__ADD_NEW__") {
        $lineElem.addClass("isValidate").removeClass("validate");
        if ($select2Selection.length > 0) {
            $select2Selection.addClass("isValidate");
        }
        return false;
    }
    $lineElem.removeClass("isValidate").addClass("validate");
    if ($select2Selection.length > 0) {
        $select2Selection.removeClass("isValidate");
    }
    return true;
}

function showPrinterNotice(message) {
    var prodLine = $("#productionLine").val();
    if (prodLine != '2') {
        $("#printerStatusBadge").hide();
        return;
    }

    var noticeText = message || "No printer connected";
    $("#printerStatusBadge").text(noticeText).show();

    if (typeof $(document).Toasts === 'function') {
        $(document).Toasts('create', {
            class: 'bg-warning',
            title: 'Printer Notice',
            subtitle: 'Assembly',
            body: '<i class="fas fa-exclamation-triangle mr-1"></i> ' + noticeText + '. Label printing skipped.',
            autohide: true,
            delay: 4000
        });
    } else if (typeof toastersetting === 'function') {
        toastersetting(noticeText + ". Label printing skipped.", "Printer Notice", "warning", "#f0ad4e");
    }
}

// =========================================================================
// Live Test Telemetry, Cycle Recording, Raw Console & Shift KPIs
// =========================================================================
var activeTestSession = {
    runs: [],          // Completed runs in this browser session
    currentRun: null,  // Currently active test run
    totalPackets: 0
};

function initTelemetryPanel() {
    $("#btnExportTestCSV, #btnExportTestData").off("click").on("click", function () {
        showExportJsonModal();
    });

    $("#btnCopyJson").off("click").on("click", function () {
        copyJsonExportToClipboard();
    });

    $("#btnDownloadJson").off("click").on("click", function () {
        downloadJsonExportFile();
    });

    $("#btnDownloadCsvFromModal").off("click").on("click", function () {
        exportTestCyclesToCSV();
    });

    $("#btnToggleWrapJson").off("click").on("click", function () {
        var $pre = $("#jsonExportContent");
        if ($pre.css("white-space") === "pre-wrap") {
            $pre.css("white-space", "pre");
        } else {
            $pre.css("white-space", "pre-wrap");
        }
    });

    $("#btnClearTelemetry").off("click").on("click", function () {
        if (confirm("Are you sure you want to clear the telemetry log and active test cycles?")) {
            clearTelemetryUI();
        }
    });

    $("#btnCopyRawConsole").off("click").on("click", function () {
        copyRawConsoleToClipboard();
    });

    $("#ddlCycleSelector").off("change").on("change", function () {
        filterCyclesBySelector($(this).val());
    });

    $("#btnCloseTelemetryPanel").off("click").on("click", function () {
        toggleTelemetryPanel(false);
    });
}

function updateTelemetryToggleBtn(isVisible) {
    if (isVisible) {
        $("#iconToggleTelemetry").removeClass("fa-eye").addClass("fa-eye-slash");
        $("#textToggleTelemetry").text("Hide Info");
        $("#btnToggleTelemetry")
            .removeClass("btn-info")
            .addClass("btn-outline-info")
            .attr("title", "Click to hide Info & Telemetry Panel");
    } else {
        $("#iconToggleTelemetry").removeClass("fa-eye-slash").addClass("fa-eye");
        $("#textToggleTelemetry").text("Show Info");
        $("#btnToggleTelemetry")
            .removeClass("btn-outline-info")
            .addClass("btn-info")
            .attr("title", "Click to unhide Info & Telemetry Panel");
    }
}

function toggleTelemetryPanel(forcedState) {
    var $section = $("#telemetrySection");
    var targetState = (forcedState !== undefined) ? forcedState : !$section.is(":visible");

    if (targetState) {
        $section.slideDown(200, function () {
            updateTelemetryToggleBtn(true);
            localStorage.setItem("telemetryPanelVisible", "true");
        });
        updateTelemetryToggleBtn(true);
        localStorage.setItem("telemetryPanelVisible", "true");
    } else {
        $section.slideUp(200, function () {
            updateTelemetryToggleBtn(false);
            localStorage.setItem("telemetryPanelVisible", "false");
        });
        updateTelemetryToggleBtn(false);
        localStorage.setItem("telemetryPanelVisible", "false");
    }
}

function initTelemetryToggle() {
    var stored = localStorage.getItem("telemetryPanelVisible");
    var isVisible = (stored === null || stored === "true");

    if (!isVisible) {
        $("#telemetrySection").hide();
        updateTelemetryToggleBtn(false);
    } else {
        $("#telemetrySection").show();
        updateTelemetryToggleBtn(true);
    }

    $("#btnToggleTelemetry").off("click").on("click", function (e) {
        e.preventDefault();
        toggleTelemetryPanel();
    });

    $("#btnCloseTelemetryPanel").off("click").on("click", function (e) {
        e.preventDefault();
        toggleTelemetryPanel(false);
    });
}

function clearTelemetryUI() {
    activeTestSession.currentRun = null;
    $("#cycleHistoryList").empty().append(
        '<div class="text-center py-4 text-muted" id="emptyCyclePlaceholder">' +
        '  <i class="fas fa-stream fa-2x mb-2 text-secondary"></i>' +
        '  <p class="mb-0 font-weight-bold">Waiting for device test sequence to start...</p>' +
        '  <small>Each telemetry frame received from start to PASS/FAIL will be recorded here for inspection.</small>' +
        '</div>'
    );
    $("#badgeCycleCount").text("0 Cycles").removeClass("badge-success badge-danger").addClass("badge-info");
    $("#testRunStatusIndicator").text("Standby").css({ "background-color": "#6c757d", "color": "#fff" });
    $("#cycleSummaryText").html('<i class="fas fa-info-circle mr-1"></i> No test run recorded yet. Scan or enter a barcode to begin logging cycles.');
    $("#btnExportTestCSV").prop("disabled", true);
    $("#ddlCycleSelector").empty().append('<option value="ALL">All Cycles (Timeline)</option>');
    $("#rawStreamConsole").html('[SYSTEM] Cleared. Diagnostic serial stream listener ready.\n');
}

function initNewTestRunTelemetry(barcode) {
    var bCode = barcode || $("#sysNumber").val() || $("#sysNumberDup").val() || "UNKNOWN";
    var model = $("#bCode").val() || ($("#printerModel option:selected").val() ? $("#printerModel option:selected").text() : "");
    var port = $("#com_port").val() || "PORT";

    activeTestSession.currentRun = {
        barcode: bCode,
        model: model,
        port: port,
        startTime: new Date(),
        startTimestamp: Date.now(),
        status: "TESTING",
        cycles: []
    };

    $("#badgeCycleCount").text("0 Cycles").removeClass("badge-success badge-danger").addClass("badge-info");
    $("#testRunStatusIndicator").text("TESTING...").css({ "background-color": "#ffc107", "color": "#000" });
    $("#cycleSummaryText").html('<i class="fas fa-spinner fa-spin mr-1 text-primary"></i> Logging active test run for <strong>' + escapeHtmlText(bCode) + '</strong>...');
    $("#btnExportTestCSV").prop("disabled", false);
    $("#emptyCyclePlaceholder").hide();
    $("#cycleHistoryList").empty();
    $("#ddlCycleSelector").empty().append('<option value="ALL">All Cycles (Timeline)</option>');
    $("#streamActivePort").text(port);

    appendRawConsoleLine("[START] Test initialized | Barcode: " + bCode + " | Port: " + port + " | Time: " + new Date().toLocaleTimeString(), "text-info");
}

function recordTestTelemetryCycle(resp) {
    if (!activeTestSession.currentRun) {
        initNewTestRunTelemetry($("#sysNumber").val() || $("#sysNumberDup").val());
    }

    var run = activeTestSession.currentRun;
    var cycleNum = run.cycles.length + 1;
    var elapsedSec = ((Date.now() - run.startTimestamp) / 1000).toFixed(2);
    activeTestSession.totalPackets++;

    var cycleStatus = "TESTING";
    var badgeClass = "badge-warning";
    if (resp && (resp.status == 1 || resp.status === 'PASS')) {
        cycleStatus = "PASS";
        badgeClass = "badge-success";
        run.status = "PASS";
    } else if (resp && (resp.status == 2 || resp.status === 'FAIL')) {
        cycleStatus = "FAIL";
        badgeClass = "badge-danger";
        run.status = "FAIL";
    }

    var cycle = {
        cycleNumber: cycleNum,
        timestamp: new Date().toLocaleTimeString(),
        elapsedSec: elapsedSec,
        status: cycleStatus,
        rawData: (resp && resp.rawData) ? resp.rawData : "",
        parameters: []
    };

    if (resp && resp.interType && resp.interType.length > 0) {
        $.each(resp.interType, function (i, item) {
            cycle.parameters.push({
                sn: i + 1,
                parameter: item.parameter || "",
                display: item.dispaly || "",
                actual: item.actual || "",
                status: item.status || ""
            });
        });
    }

    run.cycles.push(cycle);

    // Update Badges and Cycle Selector
    $("#badgeCycleCount").text(run.cycles.length + " Cycles");
    $("#badgeStreamPackets").text(activeTestSession.totalPackets + " Packets");
    $("#ddlCycleSelector").append('<option value="' + cycleNum + '">Cycle #' + cycleNum + ' (' + cycleStatus + ' - ' + elapsedSec + 's)</option>');

    // Render Cycle Card into DOM
    renderCycleCard(cycle, run);

    // Append to Raw Serial Stream Console
    var rawText = cycle.rawData;
    if (!rawText && cycle.parameters.length > 0) {
        rawText = "@" + cycle.parameters.map(function (p) { return p.parameter + ":" + p.actual + ":" + p.status; }).join(",") + "^";
    }
    var consoleColor = cycleStatus === "PASS" ? "text-success" : (cycleStatus === "FAIL" ? "text-danger" : "text-light");
    appendRawConsoleLine("[" + cycle.timestamp + "] [CYCLE #" + cycleNum + " (+" + elapsedSec + "s)] [" + cycleStatus + "] " + (rawText || "[Telemetry Frame]"), consoleColor);

    // On completion (PASS / FAIL)
    if (cycleStatus === "PASS" || cycleStatus === "FAIL") {
        $("#testRunStatusIndicator").text(cycleStatus).css({
            "background-color": cycleStatus === "PASS" ? "#28a745" : "#dc3545",
            "color": "#fff"
        });
        $("#badgeCycleCount").removeClass("badge-info").addClass(cycleStatus === "PASS" ? "badge-success" : "badge-danger");
        $("#cycleSummaryText").html('<i class="fas fa-check-circle mr-1 ' + (cycleStatus === "PASS" ? "text-success" : "text-danger") + '"></i> Test completed in <strong>' + elapsedSec + 's</strong> across <strong>' + run.cycles.length + ' cycles</strong> with result: <strong class="' + (cycleStatus === "PASS" ? "text-success" : "text-danger") + '">' + cycleStatus + '</strong>');

        activeTestSession.runs.push(run);

        appendRawConsoleLine("[END] Final Result: " + cycleStatus + " | Total Elapsed: " + elapsedSec + "s | Recorded Cycles: " + run.cycles.length, cycleStatus === "PASS" ? "text-success font-weight-bold" : "text-danger font-weight-bold");

        // Refresh Shift KPIs
        if (typeof loadShiftStats === 'function') {
            loadShiftStats();
        }
    }
}

function recordTestTelemetryError(xhr, statusText) {
    if (!activeTestSession.currentRun) return;
    var run = activeTestSession.currentRun;
    var elapsedSec = ((Date.now() - run.startTimestamp) / 1000).toFixed(2);
    run.status = "ERROR";

    $("#testRunStatusIndicator").text("ERROR").css({ "background-color": "#dc3545", "color": "#fff" });
    $("#cycleSummaryText").html('<i class="fas fa-exclamation-circle text-danger mr-1"></i> Communication error at ' + elapsedSec + 's (' + (xhr.status || statusText) + ')');
    appendRawConsoleLine("[" + new Date().toLocaleTimeString() + "] [ERROR] Serial communication failed (" + (xhr.status || statusText) + "). Test stopped.", "text-danger font-weight-bold");
}

function renderCycleCard(cycle, run) {
    var badgeClass = cycle.status === "PASS" ? "badge-success" : (cycle.status === "FAIL" ? "badge-danger" : "badge-warning");
    var collapseId = "cycleCollapse_" + cycle.cycleNumber;

    var html = '<div class="cycle-card" id="cycleCard_' + cycle.cycleNumber + '">' +
        '  <div class="cycle-card-header collapsed" data-toggle="collapse" data-target="#' + collapseId + '" aria-expanded="false" style="cursor: pointer;">' +
        '    <div class="d-flex align-items-center">' +
        '      <span class="badge badge-secondary mr-2" style="font-size: 0.82rem;">#' + cycle.cycleNumber + '</span>' +
        '      <strong class="mr-3" style="font-size: 0.9rem;">Cycle ' + cycle.cycleNumber + '</strong>' +
        '      <span class="text-muted small mr-3"><i class="far fa-clock mr-1"></i>' + cycle.timestamp + ' (+' + cycle.elapsedSec + 's)</span>' +
        '      <span class="text-muted small mr-3"><i class="fas fa-list-ol mr-1"></i>' + cycle.parameters.length + ' Parameters</span>' +
        '    </div>' +
        '    <div class="d-flex align-items-center">' +
        '      <span class="badge ' + badgeClass + ' mr-2 px-2 py-1">' + cycle.status + '</span>' +
        '      <i class="fas fa-chevron-down text-muted small cycle-chevron"></i>' +
        '    </div>' +
        '  </div>' +
        '  <div id="' + collapseId + '" class="collapse cycle-collapse-body">' +
        '    <div class="cycle-card-body">';

    if (cycle.rawData) {
        html += '<div class="mb-2 p-1 bg-light border rounded small font-italic text-break" style="font-family: Consolas, monospace; font-size: 11px;">' +
            '<strong>Raw Packet:</strong> ' + escapeHtmlText(cycle.rawData) +
            '</div>';
    }

    if (cycle.parameters.length > 0) {
        html += '<div class="table-responsive">' +
            '<table class="table table-bordered table-sm cycle-param-table">' +
            '  <thead class="thead-light">' +
            '    <tr><th style="width: 40px">Sn.</th><th>Parameter</th><th style="width: 120px">Display</th><th style="width: 120px">Actual</th><th style="width: 100px">Status</th></tr>' +
            '  </thead>' +
            '  <tbody>';

        $.each(cycle.parameters, function (pIdx, param) {
            var rowBg = "white";
            var statusColor = "#495057";
            if (param.status === 'FAIL' || param.status === 'FAULT') {
                rowBg = "#f8d7da";
                statusColor = "#721c24";
            } else if (param.status === 'PASS' || param.status === 'OK') {
                rowBg = "#d4edda";
                statusColor = "#155724";
            }

            html += '<tr style="background-color: ' + rowBg + ';">' +
                '  <td>' + param.sn + '</td>' +
                '  <td style="text-align: left; font-weight: 600;">' + escapeHtmlText(param.parameter) + '</td>' +
                '  <td>' + escapeHtmlText(param.display) + '</td>' +
                '  <td>' + escapeHtmlText(param.actual) + '</td>' +
                '  <td style="font-weight: 700; color: ' + statusColor + ';">' + escapeHtmlText(param.status) + '</td>' +
                '</tr>';
        });

        html += '  </tbody>' +
            '</table>' +
            '</div>';
    }

    html += '    </div>' +
        '  </div>' +
        '</div>';

    $("#cycleHistoryList").append(html);

    var cycleListElem = document.getElementById("cycleHistoryList");
    if (cycleListElem) {
        cycleListElem.scrollTop = cycleListElem.scrollHeight;
    }
}

function filterCyclesBySelector(selectedValue) {
    if (selectedValue === "ALL") {
        $(".cycle-card").show();
    } else {
        $(".cycle-card").hide();
        var $targetCard = $("#cycleCard_" + selectedValue);
        $targetCard.show();
        $targetCard.find(".cycle-collapse-body").collapse("show");
    }
}

function appendRawConsoleLine(text, cssClass) {
    var $console = $("#rawStreamConsole");
    if ($console.length === 0) return;
    var lineHtml = '<div class="raw-log-line ' + (cssClass || "text-light") + '">' + escapeHtmlText(text) + '</div>';
    $console.append(lineHtml);

    if ($("#chkAutoScroll").is(":checked")) {
        $console.scrollTop($console[0].scrollHeight);
    }
}

function copyRawConsoleToClipboard() {
    var text = $("#rawStreamConsole").text();
    if (navigator.clipboard && navigator.clipboard.writeText) {
        navigator.clipboard.writeText(text).then(function () {
            alert("Raw console log copied to clipboard.");
        });
    } else {
        var $temp = $("<textarea>");
        $("body").append($temp);
        $temp.val(text).select();
        document.execCommand("copy");
        $temp.remove();
        alert("Raw console log copied to clipboard.");
    }
}

function escapeHtmlText(str) {
    if (!str) return "";
    return String(str)
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}

function escapeCsvField(val) {
    if (val === null || val === undefined) return '""';
    var stringVal = String(val).replace(/"/g, '""');
    return '"' + stringVal + '"';
}

function showExportJsonModal() {
    var run = activeTestSession.currentRun;
    if (!run || !run.cycles || run.cycles.length === 0) {
        if (typeof toastersetting === 'function') {
            toastersetting("No test cycle data available to export.", "Notice", "warning", "#f0ad4e");
        } else {
            alert("No test cycle data available to export.");
        }
        return;
    }

    var exportPayload = {
        exportTimestamp: new Date().toISOString(),
        testSession: {
            barcode: run.barcode || "UNKNOWN",
            model: run.model || "",
            port: run.port || "",
            startTime: run.startTime ? new Date(run.startTime).toISOString() : null,
            status: run.status || "UNKNOWN",
            totalCycles: run.cycles.length,
            totalPacketsRecorded: activeTestSession.totalPackets
        },
        cycles: run.cycles.map(function (c) {
            return {
                cycleNumber: c.cycleNumber,
                timestamp: c.timestamp,
                elapsedSec: parseFloat(c.elapsedSec) || 0,
                status: c.status,
                rawData: c.rawData || "",
                parameters: (c.parameters || []).map(function (p) {
                    return {
                        sn: p.sn,
                        parameter: p.parameter,
                        display: p.display,
                        actual: p.actual,
                        status: p.status
                    };
                })
            };
        })
    };

    var jsonString = JSON.stringify(exportPayload, null, 2);

    $("#jsonExportMeta").html(
        '<strong>Barcode:</strong> ' + escapeHtmlText(run.barcode) + ' &nbsp;|&nbsp; ' +
        '<strong>Cycles:</strong> ' + run.cycles.length + ' &nbsp;|&nbsp; ' +
        '<strong>Result:</strong> <span class="' + (run.status === "PASS" ? "text-success font-weight-bold" : "text-danger font-weight-bold") + '">' + run.status + '</span>'
    );

    $("#jsonExportContent").text(jsonString);

    $("#btnCopyJsonText").text("Copy JSON");
    $("#btnCopyJson").removeClass("btn-success").addClass("btn-outline-info");

    $("#modalExportJson").modal("show");
}

function copyJsonExportToClipboard() {
    var text = $("#jsonExportContent").text();
    if (!text) return;

    if (navigator.clipboard && window.isSecureContext) {
        navigator.clipboard.writeText(text).then(function () {
            onJsonCopiedSuccess();
        }).catch(function () {
            fallbackCopyText(text);
        });
    } else {
        fallbackCopyText(text);
    }
}

function fallbackCopyText(text) {
    var $temp = $("<textarea>");
    $("body").append($temp);
    $temp.val(text).select();
    document.execCommand("copy");
    $temp.remove();
    onJsonCopiedSuccess();
}

function onJsonCopiedSuccess() {
    $("#btnCopyJsonText").text("Copied!");
    $("#btnCopyJson").removeClass("btn-outline-info").addClass("btn-success");
    setTimeout(function () {
        $("#btnCopyJsonText").text("Copy JSON");
        $("#btnCopyJson").removeClass("btn-success").addClass("btn-outline-info");
    }, 2000);
}

function downloadJsonExportFile() {
    var run = activeTestSession.currentRun;
    var jsonText = $("#jsonExportContent").text();
    if (!jsonText) return;

    var blob = new Blob([jsonText], { type: "application/json;charset=utf-8;" });
    var url = URL.createObjectURL(blob);
    var downloadLink = document.createElement("a");
    var cleanBarcode = ((run && run.barcode) || "Unit").replace(/[^a-zA-Z0-9_-]/g, "_");
    var timestampStr = new Date().toISOString().replace(/[-:T]/g, "").slice(0, 14);
    downloadLink.href = url;
    downloadLink.download = "TestData_" + cleanBarcode + "_" + timestampStr + "_" + ((run && run.status) || "RUN") + ".json";
    document.body.appendChild(downloadLink);
    downloadLink.click();
    document.body.removeChild(downloadLink);
    URL.revokeObjectURL(url);
}

function exportTestCyclesToCSV() {
    var run = activeTestSession.currentRun;
    if (!run || !run.cycles || run.cycles.length === 0) {
        alert("No test cycle data available to export.");
        return;
    }

    var csvRows = [];
    csvRows.push([
        "Barcode",
        "Model",
        "Cycle #",
        "Timestamp",
        "Elapsed (s)",
        "Cycle Status",
        "Parameter Sn",
        "Parameter Name",
        "Display Value",
        "Actual Value",
        "Parameter Status",
        "Raw Serial Packet"
    ].map(escapeCsvField).join(","));

    $.each(run.cycles, function (cIdx, cycle) {
        if (cycle.parameters && cycle.parameters.length > 0) {
            $.each(cycle.parameters, function (pIdx, param) {
                csvRows.push([
                    run.barcode,
                    run.model,
                    cycle.cycleNumber,
                    cycle.timestamp,
                    cycle.elapsedSec,
                    cycle.status,
                    param.sn,
                    param.parameter,
                    param.display,
                    param.actual,
                    param.status,
                    pIdx === 0 ? cycle.rawData : ""
                ].map(escapeCsvField).join(","));
            });
        } else {
            csvRows.push([
                run.barcode,
                run.model,
                cycle.cycleNumber,
                cycle.timestamp,
                cycle.elapsedSec,
                cycle.status,
                "", "", "", "", "",
                cycle.rawData
            ].map(escapeCsvField).join(","));
        }
    });

    var csvContent = "\uFEFF" + csvRows.join("\r\n");
    var blob = new Blob([csvContent], { type: "text/csv;charset=utf-8;" });
    var url = URL.createObjectURL(blob);
    var downloadLink = document.createElement("a");
    var cleanBarcode = (run.barcode || "Unit").replace(/[^a-zA-Z0-9_-]/g, "_");
    var timestampStr = new Date().toISOString().replace(/[-:T]/g, "").slice(0, 14);
    downloadLink.href = url;
    downloadLink.download = "TestData_" + cleanBarcode + "_" + timestampStr + "_" + run.status + ".csv";
    document.body.appendChild(downloadLink);
    downloadLink.click();
    document.body.removeChild(downloadLink);
    URL.revokeObjectURL(url);
}

function loadShiftStats() {
    var lineVal = $("#ddlLine").length > 0 ? $("#ddlLine").val() : "";
    var url = "/Home/GetShiftStats";
    if (lineVal && lineVal !== "" && lineVal !== "0" && lineVal !== "__ADD_NEW__") {
        url += "?line=" + encodeURIComponent(lineVal);
    }

    $.ajax({
        type: "GET",
        url: url,
        cache: false,
        success: function (resp) {
            if (resp && resp.success) {
                $("#kpiShiftTotal").text(resp.total);
                $("#kpiPassedUnits").text(resp.passed);
                $("#kpiFailedUnits").text(resp.failed);
                $("#kpiYieldRate").text(resp.yieldRate + "%");

                var $bar = $("#kpiYieldBar");
                $bar.css("width", Math.min(100, Math.max(0, resp.yieldRate)) + "%");
                if (resp.yieldRate >= 95) {
                    $bar.removeClass("bg-warning bg-danger").addClass("bg-success");
                } else if (resp.yieldRate >= 85) {
                    $bar.removeClass("bg-success bg-danger").addClass("bg-warning");
                } else {
                    $bar.removeClass("bg-success bg-warning").addClass("bg-danger");
                }
            }
        },
        error: function () {
            // graceful ignore
        }
    });
}

function reloadComPorts(callback) {
    var $icon = $("#iconRefreshPorts");
    $icon.addClass("fa-spin");

    $.ajax({
        type: "GET",
        url: "/Home/GetComPorts",
        cache: false,
        success: function (resp) {
            $icon.removeClass("fa-spin");
            if (resp && resp.success && resp.ports) {
                var $select = $("#com_port");
                var previousVal = $select.val() || localStorage.getItem("port");
                $select.empty();
                $select.append('<option value="">Select</option>');
                var foundPrev = false;
                $.each(resp.ports, function (i, p) {
                    var isSelected = (p.Key === previousVal);
                    if (isSelected) foundPrev = true;
                    $select.append('<option value="' + p.Key + '"' + (isSelected ? ' selected' : '') + '>' + p.Value + '</option>');
                });
                if (!foundPrev && resp.ports.length === 1 && resp.ports[0].Key !== "SIMULATOR") {
                    $select.val(resp.ports[0].Key);
                }
                if (typeof toastersetting === 'function') {
                    toastersetting("COM ports refreshed (" + resp.ports.length + " found)", "Ports Updated", "success", "#28a745");
                }
                if (callback) callback(true);
            }
        },
        error: function () {
            $icon.removeClass("fa-spin");
            if (callback) callback(false);
        }
    });
}

// Expose functions globally on window for interop and debugging
window.activeTestSession = activeTestSession;
window.initTelemetryPanel = initTelemetryPanel;
window.initTelemetryToggle = initTelemetryToggle;
window.toggleTelemetryPanel = toggleTelemetryPanel;
window.initNewTestRunTelemetry = initNewTestRunTelemetry;
window.recordTestTelemetryCycle = recordTestTelemetryCycle;
window.recordTestTelemetryError = recordTestTelemetryError;
window.showExportJsonModal = showExportJsonModal;
window.copyJsonExportToClipboard = copyJsonExportToClipboard;
window.downloadJsonExportFile = downloadJsonExportFile;
window.exportTestCyclesToCSV = exportTestCyclesToCSV;
window.loadShiftStats = loadShiftStats;
window.verifyPrinterConnection = verifyPrinterConnection;
window.applyPrinterStatusBadge = applyPrinterStatusBadge;
window.updatePrinterStatus = updatePrinterStatus;
window.reloadComPorts = reloadComPorts;

$(window).on("beforeunload", function () {
    if (window.currentTestAjaxRequest && typeof window.currentTestAjaxRequest.abort === 'function') {
        window.currentTestAjaxRequest.abort();
    }
});

$(document).ready(function () {
    initTelemetryToggle();
    if ($("#badgeCycleCount").length > 0) {
        initTelemetryPanel();
        loadShiftStats();
    }
    $("#ddlLine").on("change", function () {
        loadShiftStats();
    });
    $(document).on("click", "#btnRefreshPorts", function (e) {
        e.preventDefault();
        reloadComPorts();
    });
});