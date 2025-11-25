$(document).ready(function () {
  $("#customerDatatable").DataTable({
    language: {
      sProcessing: "Đang xử lý...",
      sLengthMenu: "Hiển thị _MENU_ mục",
      sZeroRecords: "Không tìm thấy dữ liệu",
      sInfo: "Hiển thị từ _START_ đến _END_ của _TOTAL_ mục",
      sInfoEmpty: "Hiển thị từ 0 đến 0 của 0 mục",
      sInfoFiltered: "(đã lọc từ _MAX_ mục)",
      sSearch: "Tìm kiếm:",
      oPaginate: {
        sFirst: "Đầu",
        sPrevious: "Trước",
        sNext: "Tiếp",
        sLast: "Cuối",
      },
    },
    processing: true,
    serverSide: true,
    filter: true,
    ajax: {
      url: "/api/CategoryApi",
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
        orderable: false,
        render: function (data, type, row, meta) {
          return meta.row + meta.settings._iDisplayStart + 1;
        },
      },
      { data: "categoryCode", name: "categoryCode", autoWidth: true },
      { data: "categoryName", name: "categoryName", autoWidth: true },
      {
        data: "description",
        name: "description",
        autoWidth: true,
        orderable: false,
      },
      {
        targets: 1,
        width: "50px",
        orderable: false,
        render: function (data, type, row) {
          var Id = row.id || "";
          return `<a href="/Category/Edit/${Id}" class="btn btn-primary center-block m-1">Sửa</a>`;
        },
      },
      {
        targets: 1,
        width: "70px",
        orderable: false,
        render: function (data, type, row) {
          var Id = row.id || "";
          return `<button type="button" class="btn btn-danger center-block m-1" title="Xóa thông tin này" onclick="if (confirm('Bạn có chắc chắn muốn xóa danh mục này?')) { DeleteCategory('${Id}'); }">Xoá</button>`;
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

// 🔁 Đổi tên function cho khớp với onclick
function DeleteCategory(id) {
  $.ajax({
    url: "/api/CategoryApi/DeleteEmp?id=" + id,
    type: "DELETE",
    success: function (result) {
      // result là message trả về từ API (ví dụ: "Xoá thành công" hoặc bất kỳ chuỗi nào)
      if (result) {
        alert(result);
      } else {
        alert("Xoá thành công.");
      }

      // Reload lại datatable
      $("#customerDatatable").DataTable().ajax.reload();
    },
    error: function (xhr, status, error) {
      // Hiển thị message lỗi trả về từ API (ví dụ: "Không thể xoá vì đang có sản phẩm...")
      var msg = xhr.responseText || "Xoá không thành công.";
      alert(msg);
      console.log(xhr.responseText);
    },
  });

  // Phần SendMes cũ thực ra không cần nữa,
  // vì DeleteEmp đã trả về message luôn rồi.
}
