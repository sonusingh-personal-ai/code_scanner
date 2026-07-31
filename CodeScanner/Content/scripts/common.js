var isOk = true;

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
function SendToComPort(isRecurrence) {
    $("#loadingbtn").show();
    getInfoValue();
    setValueFromLocalStorage();
    console.log(infoValue)
    //Step-1
    checkBarcode(infoValue.barCode, infoValue.port, infoValue.baudRate, infoValue.visualby, infoValue.testedBy, infoValue.productionLine, infoValue.lineInCharge, infoValue.serialCardNo, infoValue.currentDate, infoValue.currentTime, isRecurrence);

    if (!isOk) {
        setTimeout(function () { SendToComPort(true) }, 50);
    }
}

function insertIntoDatabase(barcode, port, baudRate, isRepeat, visualby, testedBy, productionLine, lineInCharge, cardSerialNumber, currentDate, currentTime, isRecurrence) {
    $("#loadingbtn").show();
    if (isRepeat == undefined) {
        isRepeat = false;
        infoValue.isRepeat = false;
    }
    infoValue.isRecurrence = isRecurrence;
    infoValue.isRepeat = isRepeat;
    //infoValue.disProgNo = '';
    //if (!$("#customSwitches").prop('checked')) {
    //    infoValue.disProgNo = $("#display_pv").val();
    //    infoValue.isDispProgNo = true;
    //} else {
    //    infoValue.isDispProgNo = false;
    //}

    infoValue.disProgNo = $("#display_pv").val()

    $.ajax({
        async: false,
        type: "POST",
        url: "/comport/SendParameter",
        data: infoValue,
        success: function (resp) {
            console.log(resp)
            isOk = resp.isOk;
            $("#checkbtn").show();
            $("#loadingbtn").hide();
            $("#control_pv").val(resp.controlPv)
            $("#sysRating").val(resp.sysRating)
            $("#bCode").val(resp.model)
            $("#testResponse_0").empty()
            $("#testResponse_1").empty()

            $.each(resp.interType, function (i, v) {
                var color = "white";
                if (v.status == 'FAIL' || v.status == 'FAULT') {
                    color = "red"
                } else if (v.status == 'PASS' || v.status == 'OK') {
                    color = "green"
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
            })
        },
        error: function (xhr, status) {
            $("#checkbtn").show();
            $("#loadingbtn").hide();
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

function checkBarcode(barcode, port, baudRate, visualby, testedBy, productionLine, lineInCharge, serialCardNo, currentDate, currentTime, isRecurrense) {
    if (!isRecurrense) {
        $.ajax({
            async: false,
            type: "GET",
            url: "/comport/checkbarcode?barCode=" + barcode + "&status=" + isRecurrense + "&qcStage=" + infoValue.qcStatus,
            success: function (resp) {
                if (resp) {
                    var r = confirm("Barcode already tested, If you confirmed, previous entry would be delete.");
                    if (r) {
                        insertIntoDatabase(barcode, port, baudRate, true, visualby, testedBy, productionLine, lineInCharge, serialCardNo, currentDate, currentTime, isRecurrense)
                        printautomatically();
                    }
                } else {
                    insertIntoDatabase(barcode, port, baudRate, false, visualby, testedBy, productionLine, lineInCharge, serialCardNo, currentDate, currentTime, isRecurrense)
                    printautomatically()
                }
            },
            error: function (xhr, status) {
                alert("error")
            }
        })
    } else {
        insertIntoDatabase(barcode, port, baudRate, false, visualby, testedBy, productionLine, lineInCharge, serialCardNo, currentDate, currentTime, isRecurrense)
        printautomatically()
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
}

function getValueFromLocalStorage() {
    $("#visualBy").val(localStorage.getItem("visualBy"))
    $("#testedBy").val(localStorage.getItem("testedBy"))
    $("#productionLine").val(localStorage.getItem("productionLine"))
    $("#procEngg").val(localStorage.getItem("lineInCharge"))
    $("#baudRate").val(localStorage.getItem("baudRate"))
    $("#com_port").val(localStorage.getItem("port"))
    $("#serialCardNo").val(localStorage.getItem("serialCardNo"))
    $("#QcStatus").val(localStorage.getItem("qcStatus"))
    $("#processEngg").val(localStorage.getItem("processEngg"))
    $("#display_pv").val(localStorage.getItem("display_pv"))
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
    isRepeat: false,//4
    isRecurrence: true,
    disProgNo: "",
    isDispProgNo:false
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

function getSelectedResponse() {
    var responseTr = $("#example > tbody > tr");
    var ids = getResponseIds();
    alert("Export will Start....");
    $.ajax({
        async: true,
        type: "post",
        url: "/excel/download/",
        data: { ids: ids },
        success: function (resp) {
            confirm.log(resp)
            if (resp == "s") {
                alert("excel successfully created")
            } else {
                alert("excel failed to created")
            }
        },
        error: function (xhr, status) {
            toastersetting(resp.message, resp.title, resp.type, resp.colorCode);
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

function deleteAllResp() {
    var ids = getResponseIds();
    console.log(ids);
    var status = confirm('Are you sure, You want to delete shown response?');
    if (status) {
        $.ajax({
            async: true,
            type: "post",
            url: "/response/deleteAll?ids=" + ids,
            data: { ids: ids },
            success: function (resp) {
                console.log(resp)
                if (resp == "s") {
                    alert("Selected Response successfully deleted.")
                } else {
                    alert("Selected Response failed to deleted.")
                }
                location.reload();
            },
            error: function (xhr, status) {
                toastersetting(resp.message, resp.title, resp.type, resp.colorCode);
            }
        })

    }
}

function getResponseIds() {
    var responseTr = $("#responseTbl > tbody > tr");
    var ids = [];
    $.each(responseTr, function (i, v) {
        ids.push(parseInt(responseTr[i].cells[0].textContent))
    })
    return ids;
}

//print after testing
function printautomatically() {
    var qrCode = $("#sysNumber").val();
    var printer = $("#prnter").val();
    $.ajax({
        async: false,
        type: "GET",
        url: "/home/printqrcode?qrCode=" + qrCode + "&printerName=" + printer,
        success: function (resp) {
            alert("Print Done")
        },
        error: function (xhr, status) {
            console.log(xhr)
            console.log(status)
        }
    })
}
