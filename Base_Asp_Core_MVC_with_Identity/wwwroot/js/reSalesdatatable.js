$(document).ready(function () {
  $("#customerDatatable").DataTable({
    dom: "Bfrtip",
    buttons: [
      {
        extend: "excelHtml5",
        text: "Xuất báo cáo trả hàng",
        title: "Báo cáo trả hàng",
        exportOptions: {
          // 0 = ID (ẩn)
          // 1 = STT
          // 2 = Mã trả HĐ
          // 3 = Mã lô
          // 4 = Ngày trả
          // 5 = Tên khách hàng
          // 6 = Lí do trả
          // 7 = Tổng tiền
          columns: [1, 2, 3, 4, 5, 6, 7],
          format: {
            body: function (data, row, column, node) {
              // Cột 7 là "Tổng tiền"
              if (column === 7) {
                if (data == null) return 0;

                if (typeof data === "string") {
                  // bỏ ký tự tiền tệ, dấu chấm ngăn nghìn
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
      url: "/api/ReSalseApi",
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
      { data: "id", name: "Id", autoWidth: true }, // 0: ID (ẩn)

      {
        data: null, // 1: STT
        name: "STT1",
        width: "50px",
        autoWidth: true,
        orderable: false,
        searchable: false,
        render: function (data, type, row, meta) {
          return meta.row + meta.settings._iDisplayStart + 1;
        },
      },

      { data: "sales", name: "sales", autoWidth: true }, // 2: Mã trả HĐ

      {
        data: "batchCode", // 3: Mã lô
        name: "batchCode",
        autoWidth: true,
        orderable: false,
        searchable: false,
      },

      {
        data: "invoiceDate", // 4: Ngày trả
        name: "invoiceDate",
        autoWidth: true,
        orderable: false,
        render: function (data) {
          if (!data) return "";
          const date = new Date(data);
          return date.toLocaleDateString("vi-VN");
        },
      },

      { data: "customName", name: "customName", autoWidth: true }, // 5: Tên KH

      {
        data: "reason", // 6: LÍ DO TRẢ
        name: "reason",
        autoWidth: true,
        orderable: false,
      },

      {
        data: "totalAmount", // 7: TỔNG TIỀN
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
        data: null, // 8: Nút Xem
        width: "50px",
        orderable: false,
        searchable: false,
        render: function (data, type, row) {
          var Id = "";
          if (type === "display" && data !== null) {
            Id = row.id;
          }
          return `<a href="/ReSalse/Edit/${Id}" class="btn btn-primary center-block m-1">Xem</a>`;
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
    url: "/api/CategoryApi/DeleteEmp?id=" + id,
    type: "DELETE",
    success: function (result) {
      location.reload();
    },
    error: function (xhr, status, error) {
      console.log(xhr.responseText);
    },
  });

  $.ajax({
    url: "/api/CategoryApi/SendMes",
    type: "POST",
    data: {},
    success: function (response) {
      alert(response);
    },
    error: function (xhr, status, error) {},
  });
}

function EditEmp(id) {}
