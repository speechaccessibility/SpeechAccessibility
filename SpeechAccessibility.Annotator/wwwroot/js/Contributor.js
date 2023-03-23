function buildRecordingsTable(contributorId, getUrl) {
    $.ajax({
        url: getUrl,
        type: 'POST',
        data: {
            'contributorId': contributorId
        },
        success: function (data) {
            var recordingList = $("#Recordings_" + contributorId);
            recordingList.html(data);
        },
        error: function () {
            alert("Error loading recording table");
        }
    });

}