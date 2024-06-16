//var dataTable;

//$(document).ready(function () {
//    loadDataTable();
//});

//function loadDataTable() {
//    dataTable = $('#tblData').DataTable({
//        "ajax": { url: '/admin/coupon/getall' },
//        "columns": [
            
//            { data: 'code', "width": "20%" },
//            { data: 'discountAmount', "width": "10%" },

//            {
//                data: 'Id',
//                "render": function (data) {
//                    return `<div class="w-75 btn-group" role="group">
//                    <a href = "/admin/coupon/update/${data}" class="btn btn-primary mx-2" > <i class="bi bi-pencil-square"></i> Edit</a >
//                    <a onClick=Delete('/admin/coupon/delete/${data}') class="btn btn-danger mx-2"><i class="bi bi-trash-fill"></i> Delete</a>
//                    </div>`

//                },
//                "width": "25%"
//            }

//        ]
//    });
//}
