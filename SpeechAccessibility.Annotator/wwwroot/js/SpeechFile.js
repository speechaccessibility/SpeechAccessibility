function UpdateRecordingStatus(dialog, title,  action, postUrl, datatable, returnMessageElem) {
    dialog.dialog({
        title: title,
        autoOpen: false,
        resizable: false,
        width: 700,
        show: { effect: 'drop', direction: "up" },
        modal: true,
        draggable: true,
        closeOnEscape: true,
        position: { my: "left top", at: "left+50 top+100", of: window },
        open: function () {
            dialog.css('overflow', 'hidden'); //hide the vertial bar on the dialog
        },
        close: function () {
            clearUpdateStatusInputFields();
        },
        buttons: {
            "Submit": function () {
                var recordingId = $("#hidUpdateStatusRecordingId").val();
                var comment = $("#txtUpdateStatusComment").val();

                $.ajax({
                    url: postUrl,
                    type: "POST",
                    data: {
                        "recordingId": recordingId, "comment": comment, "action": action
                    },
                    success: function (response) {
                        //alert(response.message);
                        if (response.message != "") {
                            returnMessageElem.text(response.message);
                        }
                        if (response.success === true) {
                            if (action == "editComments") {
                                datatable.draw(false);
                                clearUpdateStatusInputFields();
                               
                            }
                            else {
                                datatable.row('.selected').remove().draw(false);
                            }

                            dialog.dialog('close');
                        }
                    },
                    error: function () { alert('Error updating Recording status! It could be the timeout issue. Please try to reload your browser.'); }
                });


            },
            close: function () {
                clearUpdateStatusInputFields();
                dialog.dialog('close');
            }
        }

    });
}


function listenEditTanscriptRecording(postUrl, audio, dialog, datatable, returnMessageElem) {
    dialog.dialog({
        title: "Listen Speech File",
        autoOpen: false,
        resizable: false,
        width: 700,
        show: { effect: 'drop', direction: "up" },
        modal: true,
        draggable: true,
        closeOnEscape: true,
        position: { my: "left top", at: "left+50 top+100", of: window },
        open: function () {
            dialog.css('overflow', 'hidden'); //hide the vertial bar on the dialog
        },
        close: function () {
            audio.setAttribute("src", "");
            clearUpdateTranscriptInputFields() 
            dialog.dialog('close');
        },
        buttons: {
            Update: function () {
                var recordingId = $("#hidUpdateTranscriptRecordingId").val();
                var transcript = $("#txtTranscript").val();
                var startTime = $("#txtStartTime").val();
                var endTime = $("#txtEndTime").val();
                $.ajax({
                    url: postUrl,
                    type: "POST",
                    data: {
                        "recordingId": recordingId, "transcript": transcript, "startTime": startTime, "endTime": endTime
                    },
                    success: function (response) {
                        if (response.message != "") {
                            returnMessageElem.text(response.message);
                        }
                        if (response.success === true) {
                            datatable.draw(false);
                            audio.setAttribute("src", "");
                            clearUpdateTranscriptInputFields() 
                            dialog.dialog('close');
                          

                        }
                    },
                    error: function () { alert('Error Updating Transcript! It could be the timeout issue. Please try to reload your browser.'); }
                });
            }

            , close: function () {
                audio.setAttribute("src", "");
                clearUpdateTranscriptInputFields() 
                dialog.dialog('close');
            }
        }

    });
}

function listenRecording(audio, dialog) {
    dialog.dialog({
        title: "Listen Speech File",
        autoOpen: false,
        resizable: false,
        width: 700,
        show: { effect: 'drop', direction: "up" },
        modal: true,
        draggable: true,
        closeOnEscape: true,
        position: { my: "left top", at: "left+50 top+100", of: window },
        open: function () {
            dialog.css('overflow', 'hidden'); //hide the vertial bar on the dialog
        },
        close: function () {
            audio.setAttribute("src", "");
        },
        buttons: {

            close: function () {
                audio.setAttribute("src", "");
                dialog.dialog('close');
            }
        }

    });
}


//upload new recording
function uploadNewRecording(postUrl, dialog, returnMessageElem) {
    $("#upload-dialog").dialog({
        title: "Upload New Speech File",
        autoOpen: false,
        resizable: false,
        width: 700,
        show: { effect: 'drop', direction: "up" },
        modal: true,
        draggable: true,
        closeOnEscape: true,
        position: { my: "left top", at: "left+50 top+100", of: window },
        open: function () {
            $('#upload-dialog').css('overflow', 'hidden'); //hide the vertial bar on the dialog
        },
        close: function () {
            clearUploadRecordingInputFields();
            dialog.dialog('close');
        },
        buttons: {
            "Upload": function () {
                if ($("#recordingFile").val() != "") {
                    var contributorId = $("#hidUploadContributorId").val();
                    var recordingId = $("#hidUploadRecordingId").val();
                    var recordingVM = new FormData();
                    recordingVM.append("Id", recordingId);
                    recordingVM.append("ContributorId", contributorId);
                    recordingVM.append('file', $('#recordingFile')[0].files[0]);


                    $.ajax({
                        url: postUrl,
                        type: "POST",
                        dataType: "json",
                        contentType: false,
                        processData: false,
                        data: recordingVM,
                        success: function (response) {
                            clearUploadRecordingInputFields();
                            if (response.message != "") {
                                returnMessageElem.text(response.message);
                            }
                            if (response.success === true) {

                              /*  $("#lblMessage").text(response.message);*/
                                dialog.dialog('close');
                            }
                            else {
                                $("#lblUploadTranscriptMessage").text(response.message);
                            }

                        },
                        error: function () { alert('Error Uploading Recording! It could be the timeout issue. Please try to reload your browser.'); }
                    });

                }
                else {
                    $("#lblUploadTranscriptMessage").text("File Upload field cannot be empty.");
                }


            },
            close: function () {
                clearUploadRecordingInputFields();
                dialog.dialog('close');
            }
        }

    });
}



function clearUpdateTranscriptInputFields() {
    $("#hidUpdateTranscriptRecordingId").val("");
    $("#lblContributorId").text("");
    $("#txtTranscript").val("");
    $("#txtStartTime").val("");
    $("#txtEndTime").val("");
}

function clearUpdateStatusInputFields() {
    $("#hidUpdateStatusRecordingId").val("");
    $("#lblUpdateStatusMessage").text("");
    $("#lblUpdateStatusContributorId").text("");
    $("#lblUpdateStatusTranscript").text("");
    $("#txtUpdateStatusComment").val("")
}


function clearUploadRecordingInputFields() {
    $("#hidUploadRecordingId").val("");
    $("#lblUploadTranscriptMessage").text("");
    $("#lblUploadTranscript").text("");
    $("#hidUploadContributorId").val("")
    $("#lblUploadContributorId").text("")
    $("#recordingFile").val("");
}