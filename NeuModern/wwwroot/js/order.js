$(document).ready(function () {
    var status = getStatusFromUrl(window.location.search);
    loadDataTable(status);
});

function getStatusFromUrl(url) {
    if (url.includes("Processing")) return "Processing";
    if (url.includes("Pending")) return "Pending";
    if (url.includes("Shipped")) return "Shipped";
    if (url.includes("Approved")) return "Approved";
    if (url.includes("Cancelled")) return "Cancelled";
    return "all";
}


function loadDataTable(status) {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            url: '/Admin/Order/GetAll?status=' + status,
            error: function (xhr, errorText) {
                console.error('Error loading data:', errorText);
            }
        },
        "columns": [
            { data: 'id' },
            { data: 'name' },
            { data: 'phoneNumber' },
            { data: 'applicationUser.email' },
            { data: 'orderStatus', "width": "10%" },
            { data: 'orderTotal', "width": "10%" },
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                    <a href="/admin/order/details?orderId=${data}" class="btn btn-primary mx-2"><i class="bi bi-pencil-square"></i></a>

                   


                    <div>`
                },
                "width":"25%"
            }
        ]
    });
}




