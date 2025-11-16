$(document).ready(function () {
    $("#customerDatatable").DataTable({
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
            "url": "/api/SupplierApi",
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
            { "data": "supplierCode", "name": "supplierCode", "autoWidth": true },
            { "data": "supplierName", "name": "supplierName", "autoWidth": true },
            { "data": "email", "name": "email", "autoWidth": true, "orderable": false},
            { "data": "description", "name": "description", "autoWidth": true },
            {
                "targets": 1,
                "width": "50px",
                "orderable": false,
                "render": function (data, type, row) {
                    var Id = '';
                    if (type === 'display' && data !== null) {
                        Id = row.id;
                    }
                    return `<a href="/Supplier/Edit/${Id}" class="btn btn-primary center-block m-1">Sửa</a>`;
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
    $.ajax({
        url: '/api/CategoryApi/SendMes',
        type: 'POST',
        data: { // Dữ liệu gửi lên API Controller
            // Dữ liệu của bạn
        },
        success: function (response) {
            // Xử lý phản hồi từ API Controller
            // Hiển thị thông báo thành công
            alert(response); // Hoặc sử dụng một thư viện thông báo khác
        },
        error: function (xhr, status, error) {
            // Xử lý lỗi nếu cần
        }
    });
}
function EditEmp(id) {

}