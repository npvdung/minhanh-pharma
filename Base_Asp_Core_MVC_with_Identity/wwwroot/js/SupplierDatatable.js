$(document).ready(function () {
  $("#customerDatatable").DataTable({
    language: {
      sProcessing: "Đang xử lý...",
      sLengthMenu: "Hiển thị _MENU_ mục",
      sZeroRecords: "Không tìm thấy dữ liệu",
      sInfo: "Hiển thị từ _START_ đến _END_ của _TOTAL_ mục",
      sInfoEmpty: "Hiển thị từ 0 đến 0 của 0 mục",
      sInfoFiltered: "(đã lọc từ _MAX_ mục)",
      sInfoPostFix: "",
      sSearch: "Tìm kiếm ",
      sUrl: "",
      oPaginate: {
        sFirst: "Đầu",
        sPrevious: "Trước",
        sNext: "Tiếp",
        sLast: "Cuối",
      },
      oAria: {
        sSortAscending: ": Kích hoạt để sắp xếp cột tăng dần",
        sSortDescending: ": Kích hoạt để sắp xếp cột giảm dần",
      },
    },
    processing: true,
    serverSide: true,
    filter: true,
    order: [[2, "desc"]],
    ajax: {
      url: "/api/SupplierApi",
      type: "GET",
      datatype: "json",
      dataSrc: "data",
    },
    columnDefs: [
      {
        targets: [0],
        visible: false,
        searchable: false,
      },
    ],
    columns: [
      { data: "id", name: "Id", autoWidth: true },
      {
        data: null,
        name: "STT1",
        width: "50px",
        autoWidth: true,
        orderable: false,
        searchable: false,
        render: function (data, type, row, meta) {
          return meta.row + meta.settings._iDisplayStart + 1;
        },
      },
      { data: "supplierCode", name: "supplierCode", autoWidth: true },
      { data: "supplierName", name: "supplierName", autoWidth: true },
      { data: "email", name: "email", autoWidth: true, orderable: false },
      { data: "description", name: "description", autoWidth: true },
      {
        targets: 1,
        width: "50px",
        orderable: false,
        render: function (data, type, row) {
          var Id = "";
          if (type === "display" && data !== null) {
            Id = row.id;
          }
          return `<a href="/Supplier/Edit/${Id}" class="btn btn-primary center-block m-1">Sửa</a>`;
        },
      },

      {
        targets: 1,
        width: "70px",
        orderable: false,
        render: function (data, type, row) {
          var Id = "";
          if (type === "display" && data !== null) {
            Id = row.id;
          }
          return `<button type="button" class="btn btn-danger center-block m-1" title="Xóa nhà cung cấp này" onclick="if (confirm('Bạn có chắc chắn muốn xóa nhà cung cấp này?')) { DeleteEmp('${Id}'); }">Xoá</button>`;
        },
      },
    ],
    lengthMenu: [
      [5, 10, 20, 50, 100],
      [5, 10, 20, 50, 100],
    ],
    pageLength: 5,
  });
});

function DeleteEmp(id) {
  $.ajax({
    url: "/api/SupplierApi/DeleteEmp?id=" + id,
    type: "DELETE",
    success: function (result) {
      alert(result || "Xoá nhà cung cấp thành công.");
      $("#customerDatatable").DataTable().ajax.reload();
    },
    error: function (xhr) {
      if (xhr.status === 400) {
        alert(xhr.responseText);
      } else {
        console.log(xhr.responseText);
        alert("Có lỗi xảy ra khi xoá nhà cung cấp.");
      }
    },
  });
}
function EditEmp(id) {}
