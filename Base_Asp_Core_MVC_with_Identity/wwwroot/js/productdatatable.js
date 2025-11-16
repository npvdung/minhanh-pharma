$(document).ready(function () {
    $("#customerDatatable").DataTable({
        dom: 'Bfrtip', // Thêm buttons vào giao diện
        buttons: [
            {
                extend: 'excelHtml5',
                text: 'Xuất Excel', // Nút xuất Excel
                title: 'Danh_sach_san_pham', // Tên file Excel khi xuất
                //exportOptions: {
                //    columns: ':visible' // Xuất tất cả các cột đang hiển thị
                //},
                exportOptions: {
                    columns: [1, 2, 3, 4, 5 ] // Chỉ xuất các cột từ thứ 2 đến thứ 5
                }
            }
        ],
        "language": {
            "sProcessing": "Đang xử lý...",
            "sLengthMenu": "Hiển thị _MENU_ mục",
            "sZeroRecords": "Không tìm thấy dữ liệu",
            "sInfo": "Hiển thị từ _START_ đến _END_ của _TOTAL_ mục",
            "sInfoEmpty": "Hiển thị từ 0 đến 0 của 0 mục",
            "sInfoFiltered": "(đã lọc từ _MAX_ mục)",
            "sInfoPostFix": "",
            "sSearch": "Tìm kiếm ",
            "sUrl": "",
            "oPaginate": {
                "sFirst": "Đầu",
                "sPrevious": "Trước",
                "sNext": "Tiếp",
                "sLast": "Cuối"
            },
            "oAria": {
                "sSortAscending": ": Kích hoạt để sắp xếp cột tăng dần",
                "sSortDescending": ": Kích hoạt để sắp xếp cột giảm dần"
            }
        },
        "processing": true,
        "serverSide": true,
        "filter": true,
        "ajax": {
            "url": "/api/ProductApi",
            "type": "GET",
            "datatype": "json",
            "dataSrc": "data"
        },
        "columnDefs": [{
            "targets": [0],
            "visible": false,
            "searchable": false
        }],
        "columns": [
            { "data": "id", "name": "Id", "autoWidth": true },
            {
                "data": null,
                "name": "STT1",
                "width": "50px",
                "autoWidth": true,
                "orderable": false,
                "searchable": false,
                "render": function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            { "data": "productCode", "name": "productCode", "autoWidth": true },
            { "data": "productName", "name": "productName", "autoWidth": true },
            { "data": "categoryName", "name": "categoryName", "autoWidth": true },
            { "data": "supplierName", "name": "supplierName", "autoWidth": true },
            {
                "targets": 1,
                "width": "50px",
                "orderable": false,
                "render": function (data, type, row) {
                    var Id = '';
                    if (type === 'display' && data !== null) {
                        Id = row.id;
                    }
                    return `<a href="/Product/Edit/${Id}" class="btn btn-primary center-block m-1">Sửa</a>`;
                }
            },

            {
                "targets": 1,
                "width": "70px",
                "orderable": false,
                "render": function (data, type, row) {
                    var Id = '';
                    if (type === 'display' && data !== null) {
                        Id = row.id;
                    }
                    return `<button type="button" class="btn btn-danger center-block m-1" title="Xóa thông tin này" onclick="if (confirm('Bạn có chắc chắn muốn xóa nhân viên này?')) { DeleteEmp('${Id}'); }">Xoá</button>`;
                }
            },

        ],
        "lengthMenu": [[5, 10, 20, 50, 100], [5, 10, 20, 50, 100]],
        "pageLength": 5
    });
});
function DeleteEmp(id) {
    $.ajax({
        url: '/api/CategoryApi/DeleteEmp?id=' + id,
        type: 'DELETE',
        success: function (result) {
            debugger;
            // Xử lý kết quả trả về từ server (nếu cần)
            location.reload();
        },
        error: function (xhr, status, error) {
            // Xử lý lỗi (nếu có)
            console.log(xhr.responseText);
        }
    });
    
}