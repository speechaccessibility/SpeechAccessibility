
function sendFollowUpContributor(postUrl, table) {
    $("#sendFollowUp-dialog").dialog({
        title: "Send follow-up message to contributor",
        autoOpen: false,
        resizable: false,
        width: 700,
        show: { effect: 'drop', direction: "up" },
        modal: true,
        draggable: true,
        closeOnEscape: true,
        position: { my: "left top", at: "left+50 top+100", of: window },
        open: function () {
            $('#sendFollowUp-dialog').css('overflow', 'hidden'); //hide the vertial bar on the dialog
        },
        close: function () {
            clearFollowUpDialog();
        },
        buttons: {
            "Send": function () {
                $('.spinner').css('display', 'block');
                var contributorId = $("#hidFollowUpContributorId").val();               
                var emailContent = $("#txtFollowUpMessage").val();
                $.ajax({
                    url: postUrl,
                    type: "POST",
                    data: {
                        "contributorId": contributorId, "message": emailContent, "subRole": $("#SubRole").val()
                    },
                    success: function (response) {
                        $('.spinner').css('display', 'none');
                        if (response.success === true) {
                            clearFollowUpDialog();
                            $('#sendFollowUp-dialog').dialog('close');
                            $("#lblMessage").text("Message was sent to the contributor at " + response.message);
                            table.draw(false);
                            //$.scrollTo($("#top"), { offset: { top: -150, left: -100 } });
                        } else {
                            $("#lblFollowUpMessage").text(response.message);
                            $("#lblFollowUpMessage").addClass("errorMessage");
                        }
                    },
                    error: function () { alert('Error Exclude Contributor! It could be the timeout issue. Please try to reload your browser.'); }
                });
            },
            close: function () {
                clearFollowUpDialog();
                $(this).dialog('close');
            }
        }

    });
}

function getRecordingRating(getUrl) {
    $("#rating-dialog").dialog({
        title: "SIT Speech File Rating",
        autoOpen: false,
        resizable: false,
        width: 1000,
        height: 700,
        show: { effect: 'drop', direction: "up" },
        modal: true,
        draggable: true,
        closeOnEscape: true,
        position: { my: "left top", at: "left+50 top+100", of: window },

        open: function () {
            $(this).load(getUrl);
        },
        close: function () {

        },
        buttons: {

            close: function () {

                $(this).dialog('close');
            }
        }

    });
}



function changeContributor(postUrl, table,action, title) {
    $("#makeChanges-dialog").dialog({
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
            $('#makeChanges-dialog').css('overflow', 'hidden'); //hide the vertial bar on the dialog
        },
        close: function () {
            clearMakeChangesDialog();
        },
        buttons: {
            "Submit": function () {
                $('.spinner').css('display', 'block');
                var contributorId = $("#hidMakeChangesContributorId").val();
                var comment = $("#txtMakeChangesComment").val();
                var passwordChange = $("input[type='radio'][name='ChangePassword']:checked").val();
                $.ajax({
                    url: postUrl,
                    type: "POST",
                    data: {
                        "contributorId": contributorId, "comment": comment, "passwordChange": passwordChange, "subRole": $("#SubRole").val(), "action": action //4 for non-responsive, 3 for deny, 2 for approve
                    },
                    success: function (response) {
                        $('.spinner').css('display', 'none');
                        if (response.success === true) {
                            clearMakeChangesDialog();
                            $('#makeChanges-dialog').dialog('close');
                            if (action == 1)
                                $("#lblMessage").text("Contributor is moved to Waiting for Approval list.");
                            else if(action==2)
                                $("#lblMessage").text("Contributor is approved.");
                            else if (action == 3)
                                $("#lblMessage").text("Contributor is denied.");
                            else
                                $("#lblMessage").text("Contributor is moved to non-responsive list.");
                            table.draw(false);                            
                        } else {
                            $("#lblMakeChangesMessage").text(response.message);
                            $("#lblMakeChangesMessage").addClass("errorMessage");
                        }
                    },
                    error: function () { alert('Error Exclude Contributor! It could be the timeout issue. Please try to reload your browser.'); }
                });


            },
            close: function () {
                clearMakeChangesDialog();
                $(this).dialog('close');
            }
        }

    });
}



function editContributorInfo(postUrl, table,  title, status) {
    $("#editInfo-dialog").dialog({
        title: title,
        autoOpen: false,
        resizable: false,
        width: 900,
        show: { effect: 'drop', direction: "up" },
        modal: true,
        draggable: true,
        closeOnEscape: true,
        position: { my: "left top", at: "left+50 top+100", of: window },
        open: function () {
            $('#editInfo-dialog').css('overflow', 'hidden'); //hide the vertial bar on the dialog
        },
        close: function () {
            clearEditInfoDialog();
        },
        buttons: {
            "Submit": function () {
                $('.spinner').css('display', 'block');
                var subStatusId = status == 2 ? $("#dlSubStatus").val() : 0; //Sub Status is only for Approved Contributors
                //if (status != null)

                //var subStatusId = status;
                $.ajax({
                    url: postUrl,
                    type: "POST",
                    data: {
                        "contributorId": $("#hidEditInfoContributorId").val(),
                        "contributorEmail": $("#txtEditContributorEmail").val(),
                        "helperEmail": $("#txtEditHelperEmail").val(),
                        "birthYear": $("#txtEditInfoBirthYear").val(),
                        "subStatusId": subStatusId, 
                        "subRole": $("#SubRole").val(),
                        "comments": $("#txtEditInfoComment").val(),
                        "helperPhone": $("#txtEditHelperPhone").val()
                    },
                    success: function (response) {
                        $('.spinner').css('display', 'none');
                        if (response.success === true) {
                            clearEditInfoDialog();
                            $('#editInfo-dialog').dialog('close');
                            $("#lblMessage").text("Contributor information was updated.");
                            table.draw(false);
                        } else {
                            $("#lblEditInfoMessage").text(response.message);
                            $("#lblEditInfoMessage").addClass("errorMessage");
                        }
                    },
                    error: function () { alert('Error Update Information. It could be the timeout issue. Please try to reload your browser.'); }
                });


            },
            close: function () {
                clearEditInfoDialog();
                $(this).dialog('close');
            }
        }

    });
}

//function editContributorSubStatus(postUrl, table, title) {
//    $("#editSubStatus-dialog").dialog({
//        title: title,
//        autoOpen: false,
//        resizable: false,
//        width: 700,
//        show: { effect: 'drop', direction: "up" },
//        modal: true,
//        draggable: true,
//        closeOnEscape: true,
//        position: { my: "left top", at: "left+50 top+100", of: window },
//        open: function () {
//            $('#editSubStatus-dialog').css('overflow', 'hidden'); //hide the vertial bar on the dialog
//        },
//        close: function () {
//            clearEditSubStatusDialog();
//        },
//        buttons: {
//            "Submit": function () {
//                $('.spinner').css('display', 'block');

//                $.ajax({
//                    url: postUrl,
//                    type: "POST",
//                    data: {
//                        "contributorId": $("#hidEditSubStatusContributorId").val(),
//                        "subStatusId": $("#SubStatus").val(),                       
//                        "subRole": $("#SubRole").val()
//                    },
//                    success: function (response) {
//                        $('.spinner').css('display', 'none');
//                        if (response.success === true) {
//                            clearEditSubStatusDialog();
//                            $('#editSubStatus-dialog').dialog('close');
//                            $("#lblMessage").text("Status was updated.");
//                            table.draw(false);
//                        } else {
//                            $("#lblEditSubStatusMessage").text(response.message);
//                            $("#lblEditSubStatusMessage").addClass("errorMessage");
//                        }
//                    },
//                    error: function () { alert('Error Update Status. It could be the timeout issue. Please try to reload your browser.'); }
//                });


//            },
//            close: function () {
//                clearEditSubStatusDialog();
//                $(this).dialog('close');
//            }
//        }

//    });
//}



function clearFollowUpDialog() {
    $("#hidFollowUpContributorId").val("");
    $("#lblFollowUpFirstName").text("");
    $("#lblFollowUpLastName").text("");
    $("#txtFollowUpMessage").val("");
    $("#lblFollowUpMessage").text("Send follow up message to Contributor");
    $("#lblFollowUpMessage").removeClass("errorMessage");
}


function clearMakeChangesDialog() {
    $("#hidMakeChangesContributorId").val("");
    $("#lblMakeChangesFirstName").text("");
    $("#lblMakeChangesLastName").text("");
    $("#txtMakeChangesComment").val("");
    $("#lblMakeChangesMessage").text("Contributor");
    $("#lblMakeChangesMessage").removeClass("errorMessage");
}

function clearEditInfoDialog() {

    $("#lblEditInfoMessage").text("Change Emails:");
    $("#hidEditInfoContributorId").val("");
    $("#lblEditInfoFirstName").text("");
    $("#lblEditInfoLastName").text("");
    $("#txtEditContributorEmail").val("");
    $("#txtEditHelperEmail").val("");   
    $("#txtEditInfoBirthYear").val("");
    $("#txtEditInfoComment").val("");
    $("#lblEditInfoMessage").removeClass("errorMessage");
}

function clearEditSubStatusDialog() {

    $("#lblEditSubStatusMessage").text("Edit Approved Contributor Status:");
    $("#hidEditSubStatusContributorId").val("");
    $("#lblEditSubStatusFirstName").text("");
    $("#lblEditSubStatusLastName").text("");
    //$("#SubStatus")
   
    $("#lblEditSubStatusMessage").removeClass("errorMessage");
}
