$(document).ready(function () {
  // GIỮ LẠI BIẾN TABLE ĐỂ SAU CÒN RELOAD
  var table = $("#customerDatatable").DataTable({
    dom: "Bfrtip",
    buttons: [
      {
        extend: "excelHtml5",
        text: "Xuất báo cáo",
        title: "Báo cáo nhập hàng",
        exportOptions: {
          // Xuất từ cột STT -> Tổng tiền
          // (0 = ID ẩn, 1 = STT, 2 = Mã nhập, 3 = Mã lô,
          //  4 = Tên thuốc, 5 = Ngày nhập, 6 = Nhà cung cấp, 7 = Tổng tiền)
          columns: [1, 2, 3, 4, 5, 6, 7],
          format: {
            body: function (data, row, column, node) {
              // Cột 7 (theo index DataTable) là "totalAmount"
              if (column === 7) {
                if (data == null) return 0;

                if (typeof data === "string") {
                  data = data
                    .replace(/[^\d,-]/g, "") // bỏ " ₫" và ký tự khác
                    .replace(/\./g, ""); // bỏ dấu . ngăn cách nghìn
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
      url: "/api/ImportProductApi",
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

      { data: "importCode", name: "importCode", autoWidth: true },
      { data: "batchCode", name: "batchCode", autoWidth: true },
      { data: "productName", name: "productName", autoWidth: true },

      {
        data: "importDate",
        name: "importDate",
        autoWidth: true,
        orderable: false,
        render: function (data) {
          if (!data) return "";
          const date = new Date(data);
          return date.toLocaleDateString("vi-VN");
        },
      },

      { data: "supplierName", name: "supplierName", autoWidth: true },

      {
        data: "totalAmount",
        name: "totalAmount",
        autoWidth: true,
        orderable: false,
        render: function (data) {
          if (!data) return "0";
          return new Intl.NumberFormat("vi-VN", {
            style: "currency",
            currency: "VND",
          }).format(data);
        },
      },

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
          return `<a href="/ImportProduct/Edit/${Id}" class="btn btn-primary center-block m-1">Xem</a>`;
        },
      },

      {
        data: null,
        width: "110px",
        orderable: false,
        searchable: false,
        render: function (data, type, row) {
          const id = row.id;
          const status = row.status;
          const APPROVED =
            typeof row.approvedValue !== "undefined" ? row.approvedValue : 1;

          const isApproved = status === APPROVED;
          const disabledAttr = isApproved ? "disabled" : "";
          const btnText = isApproved ? "Đã duyệt" : "Duyệt";

          return `<button type="button" class="btn btn-success center-block m-1"
                                onclick="approveImport('${id}')"
                                ${disabledAttr}>${btnText}</button>`;
        },
      },
    ],

    lengthMenu: [
      [5, 10, 20, 50, 100],
      [5, 10, 20, 50, 100],
    ],
    pageLength: 5,
  });

  // Đẩy cụm chọn ngày lên cạnh ô Search
  var filter = $("#customerDatatable_filter");
  filter.css("display", "flex").css("gap", "20px");
  filter.prepend($("#dateFilterWrapper"));

  // 🔁 Reload bảng khi thay đổi ngày
  $("#fromDate, #toDate").on("change", function () {
    table.ajax.reload();
  });
});

function approveImport(id) {
  if (
    !confirm(
      "Bạn có chắc chắn muốn phê duyệt phiếu nhập này và cập nhật tồn kho?"
    )
  ) {
    return;
  }

  $.ajax({
    url: "/ImportProduct/Approve",
    type: "POST",
    data: { id: id },
    success: function (response) {
      if (response.success) {
        alert(response.message);
        $("#customerDatatable").DataTable().ajax.reload(null, false);
      } else {
        alert(response.message || "Có lỗi xảy ra khi phê duyệt.");
      }
    },
    error: function (xhr) {
      console.log(xhr);
      alert("Không thể phê duyệt. Vui lòng thử lại.");
    },
  });
}

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
