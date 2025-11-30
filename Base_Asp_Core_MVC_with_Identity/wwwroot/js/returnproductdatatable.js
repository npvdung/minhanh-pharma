$(document).ready(function () {
  // GIỮ LẠI BIẾN TABLE ĐỂ RELOAD
  var table = $("#customerDatatable").DataTable({
    dom: "Bfrtip",
    buttons: [
      {
        extend: "excelHtml5",
        text: "Xuất báo cáo trả/huỷ hàng",
        title: "Báo cáo trả/huỷ hàng",
        exportOptions: {
          // 0 = ID (ẩn), 1 = STT, 2 = Ngày xuất, 3 = Mã xuất,
          // 4 = Mã lô, 5 = Lí do, 6 = Tổng tiền, 7-8 = nút
          columns: [1, 2, 3, 4, 5, 6],
          format: {
            body: function (data, row, column, node) {
              // Cột 6 trong DataTable là "Tổng tiền"
              if (column === 6) {
                if (data == null) return 0;

                if (typeof data === "string") {
                  // bỏ ký tự tiền tệ, dấu . ngăn nghìn
                  data = data.replace(/[^\d,-]/g, "").replace(/\./g, "");
                }

                var num = parseFloat(data);
                return isNaN(num) ? 0 : num;
              }
              return data;
            },
          },
        },
        customizeData: function (data) {
          var idxTongTien = data.header.indexOf("Tổng tiền");
          var total = 0;

          data.body.forEach(function (row) {
            var cell = row[idxTongTien] || "";
            var digits = cell.toString().replace(/[^\d]/g, "");
            if (digits) {
              var value = parseInt(digits, 10);
              if (!isNaN(value)) {
                total += value;
              }
            }
          });

          var footerRow = new Array(data.header.length).fill("");
          footerRow[0] = "Tổng cộng";
          footerRow[idxTongTien] = total.toLocaleString("vi-VN") + " đ";
          data.body.push(footerRow);
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
      // GỬI KÈM KHOẢNG NGÀY LÊN API
      data: function (d) {
        d.fromDate = $("#fromDate").val();
        d.toDate = $("#toDate").val();
      },
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

      // CỘT MÃ LÔ
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

      // Cột "In phiếu"
      {
        data: null,
        width: "80px",
        orderable: false,
        searchable: false,
        render: function (data, type, row) {
          if (type === "display" && row && row.id) {
            var Id = row.id;
            // Mở view Print ở tab mới
            return `<a href="/ReturnProduct/Print/${Id}" 
                       target="_blank" 
                       class="btn btn-outline-secondary btn-sm m-1">
                        <i class="fa fa-print"></i>
                    </a>`;
          }
          return "";
        },
      },

      // Cột nút Xem
      {
        data: null,
        width: "50px",
        orderable: false,
        searchable: false,
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

  // ĐẨY CỤM CHỌN NGÀY LÊN CẠNH Ô SEARCH
  var filter = $("#customerDatatable_filter");
  filter.css("display", "flex").css("gap", "20px");
  filter.prepend($("#dateFilterWrapper"));

  // Reload bảng khi đổi ngày
  $("#fromDate, #toDate").on("change", function () {
    table.ajax.reload();
  });
});
