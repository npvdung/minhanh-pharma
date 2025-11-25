$(document).ready(function () {
  $("#customerDatatable").DataTable({
    dom: "Bfrtip",
    buttons: [
      {
        extend: "excelHtml5",
        text: "Xuất báo cáo tồn kho",
        title: "Danh sách tồn kho",
        exportOptions: {
          columns: [1, 2, 3, 4, 5, 6, 7],
        },
      },
    ],
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
      url: "/api/StockApi",
      type: "GET",
      datatype: "json",
      dataSrc: "data",
    },
    columnDefs: [{ targets: [0], visible: false, searchable: false }],
    columns: [
      { data: "id", name: "Id" },

      {
        data: null,
        name: "STT",
        width: "40px",
        render: function (data, type, row, meta) {
          return meta.row + meta.settings._iDisplayStart + 1;
        },
      },

      { data: "productName", name: "productName" },
      { data: "batchCode", name: "batchCode" },
      { data: "supplierName", name: "supplierName" },

      {
        data: "quantityInStock",
        name: "quantityInStock",
        render: function (data) {
          return data ?? 0;
        },
      },

      {
        data: "expirationData",
        name: "expirationData",
        render: function (data) {
          if (!data) return "";
          const d = new Date(data);
          return d.toLocaleDateString("vi-VN");
        },
      },

      { data: "unitPack", name: "unitPack" },
    ],
    lengthMenu: [
      [5, 10, 20, 50],
      [5, 10, 20, 50],
    ],
    pageLength: 10,
  });
});
