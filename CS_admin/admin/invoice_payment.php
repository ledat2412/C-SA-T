<?php
$auth = isset($_SESSION['admin_auth']) && is_array($_SESSION['admin_auth']) ? $_SESSION['admin_auth'] : array();
$idTaiKhoan = isset($auth['idTaiKhoan']) ? (int) $auth['idTaiKhoan'] : 0;
$isOwnerInvoiceViewer = isset($auth['loaiTaiKhoan']) && $auth['loaiTaiKhoan'] === 'chu_quan_ly';
$invoiceId = isset($_GET['idHoaDonGianHang']) ? (int) $_GET['idHoaDonGianHang'] : 0;
$paymentError = '';

function invoice_payment_money($value)
{
    return number_format((float) $value, 0, ',', '.') . 'đ';
}

function invoice_payment_datetime($value)
{
    if (empty($value)) {
        return 'Chưa có';
    }

    $timestamp = strtotime((string) $value);
    return $timestamp === false ? 'Chưa có' : date('d/m/Y H:i', $timestamp);
}

function invoice_payment_text($value, $emptyText = 'Chưa có')
{
    $value = trim((string) $value);
    return $value !== '' ? $value : $emptyText;
}

function invoice_payment_settings()
{
    $settings = array(
        'bankBin' => '',
        'bankAccountNo' => '',
        'bankAccountName' => '',
    );

    $appSettingsPath = dirname(__DIR__, 2) . '/VinhKhanh/VinhKhanh/appsettings.json';
    if (!is_file($appSettingsPath)) {
        return $settings;
    }

    $raw = file_get_contents($appSettingsPath);
    $decoded = is_string($raw) ? json_decode($raw, true) : null;
    if (!is_array($decoded) || empty($decoded['Payment']['VietQr'])) {
        return $settings;
    }

    $vietQr = $decoded['Payment']['VietQr'];
    $settings['bankBin'] = isset($vietQr['BankBin']) ? preg_replace('/\D+/', '', (string) $vietQr['BankBin']) : '';
    $settings['bankAccountNo'] = isset($vietQr['BankAccountNo']) ? preg_replace('/\D+/', '', (string) $vietQr['BankAccountNo']) : '';
    $settings['bankAccountName'] = isset($vietQr['BankAccountName']) ? trim((string) $vietQr['BankAccountName']) : '';

    return $settings;
}

function invoice_payment_fetch($invoiceId, $idTaiKhoan, $isOwnerInvoiceViewer, &$error)
{
    $error = '';
    if ($invoiceId <= 0) {
        $error = 'Không xác định được hóa đơn cần thanh toán.';
        return null;
    }

    $conn = admin_db_connection();
    if (!$conn instanceof mysqli) {
        $error = 'Không thể mở kết nối DB để tải hóa đơn.';
        return null;
    }

    $sql = "
        SELECT
            hdgh.idHoaDonGianHang,
            hdgh.idGianHang,
            hdgh.tongTien,
            hdgh.ngayHetHan,
            hdgh.trangThai,
            hdgh.ghiChu,
            hdgh.ngayTao,
            gh.ten AS tenGianHang,
            gh.diaChi,
            gh.idChuQuanLy,
            cql.idTaiKhoan AS idTaiKhoanChuQuanLy,
            cql.hoTen AS hoTenChuQuanLy,
            tk.email AS emailChuQuanLy
        FROM hoadongianhang hdgh
        INNER JOIN gianhang gh ON gh.idGianHang = hdgh.idGianHang
        LEFT JOIN chu_quan_ly cql ON cql.idChuQuanLy = gh.idChuQuanLy
        LEFT JOIN taikhoan tk ON tk.idTaiKhoan = cql.idTaiKhoan
        WHERE hdgh.idHoaDonGianHang = ?";

    if ($isOwnerInvoiceViewer) {
        $sql .= " AND cql.idTaiKhoan = ?";
    }

    $sql .= " LIMIT 1";

    $stmt = $conn->prepare($sql);
    if (!$stmt) {
        $error = 'Không thể đọc hóa đơn: ' . $conn->error;
        $conn->close();
        return null;
    }

    if ($isOwnerInvoiceViewer) {
        $stmt->bind_param('ii', $invoiceId, $idTaiKhoan);
    } else {
        $stmt->bind_param('i', $invoiceId);
    }

    if (!$stmt->execute()) {
        $error = 'Không thể đọc hóa đơn: ' . $stmt->error;
        $stmt->close();
        $conn->close();
        return null;
    }

    $result = $stmt->get_result();
    $invoice = $result ? $result->fetch_assoc() : null;
    if ($result) {
        $result->free();
    }

    $stmt->close();
    $conn->close();

    if (!is_array($invoice)) {
        $error = 'Không tìm thấy hóa đơn hoặc bạn không có quyền xem hóa đơn này.';
        return null;
    }

    return $invoice;
}

function invoice_payment_content($invoiceId)
{
    $content = 'HDGH' . str_pad((string) $invoiceId, 4, '0', STR_PAD_LEFT);
    return substr($content, 0, 25);
}

$invoice = null;
if (!$isOwnerInvoiceViewer) {
    $paymentError = 'Chỉ chủ gian hàng mới có thể thanh toán hóa đơn gian hàng.';
} else {
    $invoice = invoice_payment_fetch($invoiceId, $idTaiKhoan, $isOwnerInvoiceViewer, $paymentError);
}
$settings = invoice_payment_settings();
$paymentContent = $invoice ? invoice_payment_content((int) $invoice['idHoaDonGianHang']) : '';
$amount = $invoice ? (float) ($invoice['tongTien'] ?? 0) : 0;
$qrImageUrl = '';
$canPayInvoice = $invoice && in_array(($invoice['trangThai'] ?? ''), array('chua_thanh_toan', 'qua_han'), true);

if ($invoice && !$canPayInvoice && $paymentError === '') {
    $paymentError = 'Hóa đơn này không ở trạng thái chờ thanh toán.';
}

if ($canPayInvoice && $settings['bankBin'] !== '' && $settings['bankAccountNo'] !== '' && $amount > 0) {
    $qrImageUrl = 'https://img.vietqr.io/image/'
        . rawurlencode($settings['bankBin'] . '-' . $settings['bankAccountNo'] . '-compact2.png')
        . '?amount=' . rawurlencode((string) (int) $amount)
        . '&addInfo=' . rawurlencode($paymentContent)
        . '&accountName=' . rawurlencode($settings['bankAccountName']);
} elseif ($canPayInvoice && $paymentError === '') {
    $paymentError = 'Chưa cấu hình đủ Payment:VietQr trong appsettings hoặc số tiền hóa đơn không hợp lệ.';
}
?>
<main class="main-content">
  <section class="invoice-payment-page">
    <div class="payment-shell">
      <a class="back-link" href="<?php echo htmlspecialchars(admin_url('index1st.php?usecase=invoice&selected=' . (int) $invoiceId), ENT_QUOTES, 'UTF-8'); ?>">
        <i class="fa-solid fa-arrow-left"></i>
        <span>Quay lại hóa đơn</span>
      </a>

      <?php if ($paymentError !== '') { ?>
      <div class="payment-alert"><?php echo htmlspecialchars($paymentError, ENT_QUOTES, 'UTF-8'); ?></div>
      <?php } ?>

      <?php if ($invoice) { ?>
      <div class="payment-card">
        <div class="payment-info">
          <span class="payment-kicker">Thanh toán hóa đơn gian hàng</span>
          <h2>#HDGH-<?php echo str_pad((string) (int) $invoice['idHoaDonGianHang'], 4, '0', STR_PAD_LEFT); ?></h2>
          <p><?php echo htmlspecialchars(invoice_payment_text($invoice['tenGianHang'] ?? '', 'Gian hàng'), ENT_QUOTES, 'UTF-8'); ?></p>

          <div class="amount-box">
            <span>Số tiền cần thanh toán</span>
            <strong><?php echo htmlspecialchars(invoice_payment_money($invoice['tongTien'] ?? 0), ENT_QUOTES, 'UTF-8'); ?></strong>
          </div>

          <div class="payment-grid">
            <div>
              <span>Ngày tạo</span>
              <strong><?php echo htmlspecialchars(invoice_payment_datetime($invoice['ngayTao'] ?? null), ENT_QUOTES, 'UTF-8'); ?></strong>
            </div>
            <div>
              <span>Hạn thanh toán</span>
              <strong><?php echo htmlspecialchars(invoice_payment_datetime($invoice['ngayHetHan'] ?? null), ENT_QUOTES, 'UTF-8'); ?></strong>
            </div>
            <div>
              <span>Nội dung chuyển khoản</span>
              <strong><?php echo htmlspecialchars($paymentContent, ENT_QUOTES, 'UTF-8'); ?></strong>
            </div>
            <div>
              <span>Trạng thái</span>
              <strong><?php echo htmlspecialchars(invoice_payment_text($invoice['trangThai'] ?? '', 'Chưa có'), ENT_QUOTES, 'UTF-8'); ?></strong>
            </div>
          </div>

          <div class="bank-box">
            <span>Tài khoản nhận</span>
            <strong><?php echo htmlspecialchars(invoice_payment_text($settings['bankAccountName']), ENT_QUOTES, 'UTF-8'); ?></strong>
            <p><?php echo htmlspecialchars(invoice_payment_text($settings['bankAccountNo']), ENT_QUOTES, 'UTF-8'); ?> - BIN <?php echo htmlspecialchars(invoice_payment_text($settings['bankBin']), ENT_QUOTES, 'UTF-8'); ?></p>
          </div>
        </div>

        <div class="qr-panel">
          <div class="qr-frame">
            <?php if ($qrImageUrl !== '') { ?>
            <img src="<?php echo htmlspecialchars($qrImageUrl, ENT_QUOTES, 'UTF-8'); ?>" alt="QR thanh toán hóa đơn gian hàng" />
            <?php } else { ?>
            <div class="qr-empty">Chưa thể tạo QR</div>
            <?php } ?>
          </div>
          <p>Quét mã bằng ứng dụng ngân hàng và giữ đúng số tiền, nội dung chuyển khoản.</p>
        </div>
      </div>
      <?php } ?>
    </div>
  </section>
</main>
