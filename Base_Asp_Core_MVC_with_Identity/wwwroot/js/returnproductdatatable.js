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
      sSearch: "Tìm kiếm :",
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
    ajax: {
      url: "/api/ReturnProductApi",
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
      {
        data: "returnDate",
        name: "returnDate",
        autoWidth: true,
        orderable: false,
        render: function (data) {
          if (!data) return "";
          const date = new Date(data);
          return date.toLocaleDateString("vi-VN");
        },
      },
      { data: "exportName", name: "exportName", autoWidth: true },

      // >>> CỘT MÃ LÔ MỚI
      {
        data: "batchCode",
        name: "batchCode",
        autoWidth: true,
        orderable: false,
        searchable: false,
      },

      { data: "reason", name: "reason", autoWidth: true },
      {
        data: "totalAmount",
        name: "totalAmount",
        autoWidth: true,
        orderable: false,
        render: function (data) {
          if (!data) return "0 VND";
          return new Intl.NumberFormat("vi-VN", {
            style: "currency",
            currency: "VND",
          }).format(data);
        },
      },
      {
        targets: 1,
        width: "50px",
        orderable: false,
        render: function (data, type, row) {
          var Id = "";
          if (type === "display" && data !== null) {
            Id = row.id;
          }
          return `<a href="/ReturnProduct/Edit/${Id}" class="btn btn-primary center-block m-1">Xem</a>`;
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
