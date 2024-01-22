// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function formatDate(dateIn) {
    var date = new Date(dateIn);
    var dateDay = ("0" + date.getDate()).slice(-2);
    var dateMonth = ("0" + (date.getMonth() + 1)).slice(-2);
    var dateVal = date.getFullYear() + "-" + (dateMonth) + "-" + (dateDay);

    return dateVal;

}
