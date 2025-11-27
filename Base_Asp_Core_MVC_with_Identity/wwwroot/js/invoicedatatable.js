$(document).ready(function () {
  var table = $("#customerDatatable").DataTable({
    dom: "Bfrtip",
    buttons: [
      {
        extend: "excelHtml5",
        text: "Xuất báo cáo doanh thu",
        title: "Báo cáo bán hàng",
        exportOptions: {
          columns: [1, 2, 3, 4, 5], // STT -> Tổng tiền
        },
        customizeData: function (data) {
          // ====== TÌM CỘT "Tổng tiền" ======
          var idxTongTien = data.header.indexOf("Tổng tiền");
          var total = 0;

          // ====== DUYỆT TỪNG DÒNG ======
          data.body.forEach(function (row) {
            var cell = row[idxTongTien] || "";

            // cell dạng "2.500.000 đ" → loại bỏ ký tự không phải số
            var digits = cell.toString().replace(/[^\d]/g, "");

            if (digits) {
              var value = parseInt(digits, 10);
              if (!isNaN(value)) total += value;
            }
          });

          // ====== TẠO DÒNG TỔNG CỘNG ======
          var footerRow = new Array(data.header.length).fill("");

          footerRow[0] = "Tổng doanh thu";
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
      sSearch: "Tìm kiếm theo tên khách hàng:",
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
      url: "/api/InvoiceApi",
      type: "GET",
      datatype: "json",
      dataSrc: "data",
      // Gửi thêm 2 tham số fromDate & toDate lên API
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
      { data: "id", name: "Id" },
      {
        data: null,
        render: function (data, type, row, meta) {
          return meta.row + meta.settings._iDisplayStart + 1;
        },
      },
      { data: "invoiceCode" },
      {
        data: "invoiceDate",
        render: function (data) {
          if (!data) return "";
          const date = new Date(data);
          return date.toLocaleDateString("vi-VN");
        },
      },
      { data: "customName" },
      {
        data: "totalAmount",
        render: function (data) {
          if (!data) return "0 đ";
          return new Intl.NumberFormat("vi-VN", {
            style: "currency",
            currency: "VND",
          }).format(data);
        },
      },

      // Nút In hóa đơn
      {
        data: null,
        render: function (data, type, row) {
          return `
              <a href="/Invoice/Print/${row.id}" 
                 class="btn btn-outline-secondary m-1"
                 title="In hóa đơn">
                  <i class="fa fa-print"></i>
              </a>`;
        },
      },

      // Nút Xem chi tiết
      {
        data: null,
        render: function (data, type, row) {
          return `<a href="/Invoice/Edit/${row.id}" class="btn btn-primary m-1">Xem</a>`;
        },
      },
    ],

    lengthMenu: [
      [5, 10, 20, 50, 100],
      [5, 10, 20, 50, 100],
    ],
    pageLength: 5,
  });

  // Đưa khung lọc ngày vào cùng dòng với ô search của DataTables
  var filterContainer = $("#customerDatatable_filter");
  if (filterContainer.length && $("#dateFilterWrapper").length) {
    filterContainer.prepend($("#dateFilterWrapper"));
    // thêm chút khoảng cách với label search
    filterContainer.css("display", "flex");
    filterContainer.css("gap", "12px");
    filterContainer.find("label").css("margin-bottom", "0");
  }

  // Khi đổi khoảng ngày → reload lại bảng
  $("#fromDate, #toDate").on("change", function () {
    table.ajax.reload();
  });
});
