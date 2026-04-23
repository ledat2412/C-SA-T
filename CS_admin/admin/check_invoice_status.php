<?php
require_once dirname(__DIR__) . '/connect.php';

header('Content-Type: application/json');

$invoiceId = isset($_GET['id']) ? (int) $_GET['id'] : 0;
if ($invoiceId <= 0) {
    echo json_encode(['status' => 'error']);
    exit;
}

$conn = admin_db_connection();
if (!$conn instanceof mysqli) {
    echo json_encode(['status' => 'error']);
    exit;
}

$stmt = $conn->prepare("SELECT trangThai FROM hoadongianhang WHERE idHoaDonGianHang = ? LIMIT 1");
$stmt->bind_param('i', $invoiceId);
$stmt->execute();
$result = $stmt->get_result();
$row = $result->fetch_assoc();

if ($row) {
    echo json_encode(['status' => $row['trangThai']]);
} else {
    echo json_encode(['status' => 'error']);
}

$stmt->close();
$conn->close();
