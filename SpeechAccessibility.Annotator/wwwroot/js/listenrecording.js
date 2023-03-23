
function listenRecording(audio, dialog) {
    $("#listen-dialog").dialog({
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
                $(this).dialog('close');
            }
        }

    });
}
