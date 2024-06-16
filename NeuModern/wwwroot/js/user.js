$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/Admin/User/GetAll' },
        "columns": [
            { data: 'name' },
            { data: 'email' },
            { data: 'phoneNumber' },
            {
                data: 'isBlocked',
                render: function (data) {
                    return data ? 'Blocked' : 'Not Blocked';
                }
            },
            {
                data: 'id',
                render: function (data) {
                    // Use template literals to insert the value of data
                    return `<div class="w-75 btn-group" role="group">
                                <a href="/Admin/User/Details/${data}" class="btn btn-primary mx-2">
                                    <i class="bi bi-info-circle"></i> Details
                                </a>
                            </div>`;
                }
            }
        ]
    });
}

