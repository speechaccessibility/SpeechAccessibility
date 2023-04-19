function openDialogForRecording(dialog, title, recordingIdElem, commentElem, action, postUrl, datatable, returnMessageElem) {
   
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
           
        },
        buttons: {
            "Submit": function () {               
                $.ajax({
                    url: postUrl,
                    type: "POST",
                    data: {
                        "recordingId": recordingIdElem.val(), "comment": commentElem.val(), "action": action
                    },
                    success: function (response) {
                        //alert(response.message);
                        if (response.message != "") {
                            returnMessageElem.text(response.message);
                        }
                        if (response.success === true) {
                            if (action == "editComments") {                               
                                datatable.draw(false);                           
                            }
                            else {    
                                datatable.row('.selected').remove().draw(false);                                  
                            }
                                                        
                            dialog.dialog('close');
                        }
                    },
                    error: function () { alert('Error Exclude Recording! It could be the timeout issue. Please try to reload your browser.'); }
                });


            },
            close: function () {             
                 
                dialog.dialog('close');
            }
        }

    });
}